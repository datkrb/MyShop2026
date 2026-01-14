# Product Import Guide

## Supported File Formats
- Excel: `.xlsx`, `.xls`
- Access Database: `.accdb`, `.mdb`

## Excel/Access Column Structure

Your import file must have the following columns (case-insensitive):

**⚠️ Important: Column order doesn't matter!** The system reads columns by name, not position. You can arrange them in any order.

| Column Name | Type | Required | Description | Example |
|------------|------|----------|-------------|---------|
| `Name` | Text | ✅ Yes | Product name | "Laptop Dell XPS 15" |
| `SKU` | Text | ✅ Yes | Unique product code | "DELL-XPS-001" |
| `ImportPrice` | Number | ✅ Yes | Cost price | 15000000 |
| `SalePrice` | Number | ✅ Yes | Selling price | 20000000 |
| `Stock` | Number | ✅ Yes | Quantity in stock | 10 |
| `CategoryId` | Number | ✅ Yes | Category ID (must exist in database) | 5 |
| `Description` | Text | ❌ No | Product description | "High-performance laptop" |

## How to Import with Images

To import products **with images**, add image paths to the `Description` field using this format:

```
Your product description here IMAGE:C:\path\to\image1.jpg;C:\path\to\image2.jpg;C:\path\to\image3.jpg
```

### Example:
```
Description: "High-performance laptop with 16GB RAM IMAGE:C:\Images\laptop1.jpg;C:\Images\laptop2.jpg"
```

**Important Notes:**
- Use `IMAGE:` as a separator between description and image paths
- Separate multiple image paths with semicolons (`;`)
- Use **absolute paths** to image files
- Images will be automatically uploaded after product creation
- The description will be cleaned (IMAGE: part removed) after import

## Excel Example

| Name | SKU | ImportPrice | SalePrice | Stock | CategoryId | Description |
|------|-----|-------------|-----------|-------|------------|-------------|
| Laptop Dell XPS 15 | DELL-001 | 15000000 | 20000000 | 10 | 5 | High-performance laptop IMAGE:C:\Images\dell1.jpg;C:\Images\dell2.jpg |
| iPhone 15 Pro | IPH-001 | 25000000 | 30000000 | 5 | 5 | Latest iPhone model IMAGE:C:\Images\iphone.jpg |
| Áo Polo Nam | POLO-001 | 180000 | 289000 | 50 | 6 | Cotton polo shirt |

## Access Database

For Access databases, create a table named `Products` with the same column structure.

## Steps to Import

1. Click the **"Import Products"** button in the Products page
2. Select your Excel or Access file
3. Wait for the import process to complete
4. Check the notification for success/failure count
5. The product list will automatically refresh

## Import Results

After import completes, you'll see a detailed notification showing:

### Example 1: All New Products (Success)
```
Import completed: 5 product(s) processed

✅ New products created: 5
   SKUs: DELL-001, IPH-001, POLO-001, SHIRT-002, LAPTOP-003
```

### Example 2: Mixed Results
```
Import completed: 10 product(s) processed

✅ New products created: 6
   SKUs: DELL-001, IPH-001, POLO-001, SHIRT-002, LAPTOP-003, PHONE-004

⚠️ Existing products (skipped): 3
   SKUs: SA-115, EXISTING-001, DUP-SKU-002

❌ Failed to import: 1
   SKUs: INVALID-PRODUCT
```

### Example 3: All Existing (Duplicates)
```
Import completed: 3 product(s) processed

⚠️ Existing products (skipped): 3
   SKUs: SA-115, DELL-XPS-001, IPH-15-PRO
```

**Note:** Products with duplicate SKUs will be skipped automatically to prevent data conflicts.

## Troubleshooting

### Common Issues:

**"No products found in the file"**
- Check that your Excel sheet has the correct column headers
- Ensure the first row contains column names
- For Access, verify the table is named "Products"

**"Failed to import products"**
- Verify `CategoryId` exists in the database
- Check that all required fields are filled
- Ensure numeric fields contain valid numbers
- Verify image paths are correct and files exist

**Images not uploading**
- Check that image paths are absolute (full path)
- Verify image files exist at the specified locations
- Ensure image formats are supported (.jpg, .jpeg, .png, .gif, .bmp, .webp)

## Getting Category IDs

To find valid Category IDs:
1. Go to the Products page
2. Check existing products to see their category IDs
3. Or create categories first using "New Category" button
4. Category IDs are shown in the database (typically 5, 6, 7, etc.)
