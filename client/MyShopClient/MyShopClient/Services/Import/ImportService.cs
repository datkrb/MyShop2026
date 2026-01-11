using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Threading.Tasks;
using MiniExcelLibs;
using MyShopClient.Models;
using System.Linq;

namespace MyShopClient.Services.Import;

public class ImportService
{
    // Excel Import
    public async Task<List<ApiProduct>> ImportFromExcelAsync(string filePath)
    {
        var products = new List<ApiProduct>();

        try
        {
            // Query with useHeaderRow: true to treat first row as keys
            var rows = await MiniExcel.QueryAsync(filePath, useHeaderRow: true);

            foreach (var row in rows)
            {
                var p = new ApiProduct();
                // Create a case-insensitive dictionary for easier lookup
                var r = ((IDictionary<string, object>)row)
                        .ToDictionary(k => k.Key.Trim().ToLower(), v => v.Value);

                if (r.TryGetValue("name", out var name)) p.Name = name?.ToString() ?? "New Product";
                if (r.TryGetValue("sku", out var sku)) p.Sku = sku?.ToString() ?? Guid.NewGuid().ToString().Substring(0, 8);
                
                if (r.TryGetValue("importprice", out var ip)) p.ImportPrice = TryConvertDecimal(ip);
                if (r.TryGetValue("saleprice", out var sp)) p.SalePrice = TryConvertDecimal(sp);
                if (r.TryGetValue("stock", out var st)) p.Stock = TryConvertInt(st);
                if (r.TryGetValue("description", out var desc)) p.Description = desc?.ToString();
                
                if (r.TryGetValue("categoryid", out var cid))
                {
                    p.CategoryId = TryConvertInt(cid);
                }

                if (!string.IsNullOrEmpty(p.Name))
                {
                    products.Add(p);
                }
            }
        }
        catch (Exception ex) 
        {
            System.Diagnostics.Debug.WriteLine($"Excel Import Loop Error: {ex.Message}");
            throw;
        }

        return products;
    }

    private decimal TryConvertDecimal(object? value)
    {
        if (value == null) return 0;
        if (decimal.TryParse(value.ToString(), out decimal res)) return res;
        return 0;
    }

    private int TryConvertInt(object? value)
    {
        if (value == null) return 0;
        if (int.TryParse(value.ToString(), out int res)) return res;
        return 0;
    }

    // Access Import
    public async Task<List<ApiProduct>> ImportFromAccessAsync(string filePath)
    {
        var products = new List<ApiProduct>();

        // Ensure this runs on a thread that can handle OLEDB (Windows specific)
        await Task.Run(() => 
        {
            // Try 12.0 provider first (newer), then 4.0 (older)
            string connString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";
            
            try 
            {
                using (OleDbConnection conn = new OleDbConnection(connString))
                {
                    conn.Open();
                    // Assume table name is "Products"
                    string query = "SELECT * FROM Products"; 
                    
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var p = new ApiProduct();
                            // Basic mapping - names must match
                            p.Name = reader["Name"]?.ToString() ?? "Imported";
                            p.Sku = reader["Sku"]?.ToString() ?? Guid.NewGuid().ToString().Substring(0, 8);
                            
                            if (reader["ImportPrice"] != DBNull.Value) p.ImportPrice = Convert.ToDecimal(reader["ImportPrice"]);
                            if (reader["SalePrice"] != DBNull.Value) p.SalePrice = Convert.ToDecimal(reader["SalePrice"]);
                            if (reader["Stock"] != DBNull.Value) p.Stock = Convert.ToInt32(reader["Stock"]);
                            if (reader["Description"] != DBNull.Value) p.Description = reader["Description"]?.ToString();
                             if (reader["CategoryId"] != DBNull.Value) p.CategoryId = Convert.ToInt32(reader["CategoryId"]);

                            products.Add(p);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fallback or rethrow
                throw new Exception("Could not read Access file. Ensure 'Microsoft Access Database Engine 2010/2016' is installed and table 'Products' exists.");
            }
        });

        return products;
    }
}
