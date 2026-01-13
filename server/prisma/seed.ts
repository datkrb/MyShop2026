import { PrismaClient, UserRole, OrderStatus } from '@prisma/client';
import * as bcrypt from 'bcryptjs';

const prisma = new PrismaClient();

// Helper function to generate random number in range
function randomInt(min: number, max: number): number {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

// Helper function to generate placeholder image URLs
function getPlaceholderImages(index: number): string[] {
  return [
    `https://placehold.co/300x300/08ffff/01?text=Image+2`,
    `https://placehold.co/300x300/08ffff/01?text=Image+3`
  ];
}

async function main() {
  console.log('🌱 Start seeding database...');

  // =========================
  // 1. CLEAN DATABASE
  // =========================
  await prisma.orderItem.deleteMany();
  await prisma.productImage.deleteMany();
  await prisma.order.deleteMany();
  await prisma.product.deleteMany();
  await prisma.category.deleteMany();
  await prisma.customer.deleteMany();
  await prisma.user.deleteMany();

  // =========================
  // 2. CREATE USERS
  // =========================
  const hashedPassword = await bcrypt.hash('123456', 10);

  const admin = await prisma.user.create({
    data: {
      username: 'admin',
      password: hashedPassword,
      role: UserRole.ADMIN
    }
  });

  const sale1 = await prisma.user.create({
    data: {
      username: 'sale1',
      password: hashedPassword,
      role: UserRole.SALE
    }
  });

  const sale2 = await prisma.user.create({
    data: {
      username: 'sale2',
      password: hashedPassword,
      role: UserRole.SALE
    }
  });

  console.log('✅ Users created');

  // =========================
  // 3. CREATE CUSTOMERS (10 customers)
  // =========================
  const customersData = [
    { name: 'Nguyễn Văn An', phone: '0901234567', email: 'nguyenvanan@gmail.com', address: '123 Nguyễn Huệ, Quận 1, TP.HCM' },
    { name: 'Trần Thị Bích', phone: '0907654321', email: 'tranthibich@gmail.com', address: '456 Lê Lợi, Quận 3, TP.HCM' },
    { name: 'Lê Hoàng Cường', phone: '0912345678', email: 'lehoangcuong@gmail.com', address: '789 Hai Bà Trưng, Quận 1, TP.HCM' },
    { name: 'Phạm Thị Diệu', phone: '0923456789', email: 'phamthidieu@gmail.com', address: '321 Võ Văn Tần, Quận 3, TP.HCM' },
    { name: 'Hoàng Minh Đức', phone: '0934567890', email: 'hoangminhduc@gmail.com', address: '654 Cách Mạng Tháng 8, Quận 10, TP.HCM' },
    { name: 'Võ Ngọc Hà', phone: '0945678901', email: 'vongocha@gmail.com', address: '12 Trần Phú, Quận 5, TP.HCM' },
    { name: 'Đặng Quốc Hùng', phone: '0956789012', email: 'dangquochung@gmail.com', address: '88 Lý Thường Kiệt, Quận Tân Bình, TP.HCM' },
    { name: 'Bùi Minh Khang', phone: '0967890123', email: 'buiminhkhang@gmail.com', address: '55 Nguyễn Văn Cừ, Quận 5, TP.HCM' },
    { name: 'Ngô Thị Lan', phone: '0978901234', email: 'ngothilan@gmail.com', address: '200 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM' },
    { name: 'Dương Văn Minh', phone: '0989012345', email: 'duongvanminh@gmail.com', address: '99 Phan Xích Long, Quận Phú Nhuận, TP.HCM' }
  ];

  await prisma.customer.createMany({ data: customersData });
  const customerList = await prisma.customer.findMany();
  console.log('✅ Customers created');

  // =========================
  // 4. CREATE CATEGORIES
  // =========================
  const categoriesData = [
    { name: 'Điện Tử', description: 'Các sản phẩm điện tử, công nghệ như điện thoại, laptop, máy tính bảng, phụ kiện điện tử và thiết bị thông minh.' },
    { name: 'Thời Trang', description: 'Quần áo, giày dép, phụ kiện thời trang nam nữ với nhiều phong cách từ công sở đến thể thao.' },
    { name: 'Nhà Cửa & Trang Trí', description: 'Đồ nội thất, đèn trang trí, tranh ảnh và các vật dụng làm đẹp không gian sống.' }
  ];

  await prisma.category.createMany({ data: categoriesData });
  const categoryList = await prisma.category.findMany();
  console.log('✅ Categories created');

  // =========================
  // 5. CREATE PRODUCTS WITH IMAGES
  // =========================

  // Danh mục: Điện Tử
  const dienTuProducts = [
    { name: 'Điện Thoại Samsung Galaxy S24 Ultra', sku: 'DT-SAM-001', importPrice: 25000000, salePrice: 32990000, description: 'Điện thoại Samsung Galaxy S24 Ultra với chip Snapdragon 8 Gen 3, màn hình Dynamic AMOLED 6.8 inch, camera 200MP và S Pen tích hợp.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327260/lkq9wk1bhvyd4nrcqach.jpg' },
    { name: 'Điện Thoại iPhone 17 Pro Max', sku: 'DT-IPH-001', importPrice: 30000000, salePrice: 39990000, description: 'iPhone 17 Pro Max với chip A19 Pro, hệ thống camera 48MP ProMotion, màn hình Super Retina XDR 6.9 inch và thời lượng pin cả ngày.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327262/bsocoxevuht1j0tlnc6w.jpg' },
    { name: 'Laptop ASUS ZenBook S13', sku: 'LT-ASU-001', importPrice: 22000000, salePrice: 28990000, description: 'Laptop ASUS ZenBook S13 siêu mỏng nhẹ với Intel Core Ultra 7, RAM 16GB, SSD 512GB, màn hình OLED 13.3 inch 2.8K.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327266/ngw1pmtjynwkx3zvllv6.png' },
    { name: 'Bộ Máy Tính Để Bàn Văn Phòng', sku: 'PC-VAN-001', importPrice: 8000000, salePrice: 10990000, description: 'Bộ máy tính để bàn văn phòng với Intel Core i5, RAM 8GB, SSD 256GB, phù hợp cho công việc văn phòng và học tập.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327268/n0cojmzveh7dychv4pic.jpg' },
    { name: 'Máy Tính Bảng iPad 10.9 inch', sku: 'TB-IPD-001', importPrice: 11000000, salePrice: 14990000, description: 'iPad 10.9 inch thế hệ mới với chip A14 Bionic, hỗ trợ Apple Pencil, màn hình Liquid Retina sắc nét.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327270/djaygszqycdq5f4o5xq9.jpg' },
    { name: 'Tai Nghe Có Dây Type-C Xiaomi', sku: 'TN-XIA-001', importPrice: 100000, salePrice: 199000, description: 'Tai nghe có dây cổng Type-C Xiaomi với chất lượng âm thanh Hi-Res, micro tích hợp, dây dài 1.2m.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327273/hjr01jtihmmrih51qj0d.jpg' },
    { name: 'Tai Nghe Bluetooth SOUNDPEATS Sport', sku: 'TN-SOU-001', importPrice: 500000, salePrice: 799000, description: 'Tai nghe Bluetooth thể thao SOUNDPEATS với âm bass mạnh mẽ, chống nước IPX7, thời lượng pin 10 giờ.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327275/psca4yvrdvpxo1vmiod1.jpg' },
    { name: 'Loa Bluetooth JBL Charge 5', sku: 'LO-JBL-001', importPrice: 2800000, salePrice: 3790000, description: 'Loa Bluetooth JBL Charge 5 công suất 40W, chống nước IP67, thời lượng pin 20 giờ, có thể sạc cho thiết bị khác.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327277/vdwfpdlrapsmvcuo7hng.jpg' },
    { name: 'Đồng Hồ Thông Minh Mibro T1', sku: 'DH-MIB-001', importPrice: 800000, salePrice: 1290000, description: 'Đồng hồ thông minh Mibro T1 với màn hình AMOLED 1.6 inch, đo nhịp tim, SpO2, theo dõi giấc ngủ và hơn 100 chế độ thể thao.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327280/xkyvrkjykcvacykmi2ji.jpg' },
    { name: 'Tivi Thông Minh Xiaomi ES Pro 86 inch', sku: 'TV-XIA-001', importPrice: 35000000, salePrice: 42990000, description: 'Tivi thông minh Xiaomi ES Pro 86 inch 4K với hệ điều hành Google TV, loa Dolby Audio, hỗ trợ HDMI 2.1.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327282/oosg2z8ijfuhsfktem47.jpg' },
    { name: 'Chuột Gaming KENOO ESPORT G102', sku: 'CM-KEN-001', importPrice: 150000, salePrice: 249000, description: 'Chuột gaming KENOO ESPORT G102 với cảm biến quang học 6000 DPI, đèn LED RGB 16.8 triệu màu, thiết kế công thái học.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327286/p7jzzyfnbwreclfvfbmq.jpg' },
    { name: 'Bàn Phím Cơ Cidoo ABM098 Tri-mode', sku: 'BP-CID-001', importPrice: 1200000, salePrice: 1690000, description: 'Bàn phím cơ Cidoo ABM098 kết nối 3 chế độ Bluetooth/2.4GHz/USB-C, switch Gateron, keycap PBT, đèn RGB.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327288/fsxnjyzmn7qt5c2fff0g.jpg' },
    { name: 'Webcam EMEET C960 Full HD 1080P', sku: 'WC-EME-001', importPrice: 600000, salePrice: 890000, description: 'Webcam EMEET C960 Full HD 1080P với micro kép tích hợp, tự động lấy nét, plug and play, phù hợp họp online.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327291/yerwytcmjen3nezc8dcd.jpg' },
    { name: 'Micro Thu Âm Audio-Technica AT2020', sku: 'MI-AUT-001', importPrice: 2200000, salePrice: 2990000, description: 'Micro thu âm Audio-Technica AT2020 condenser chuyên nghiệp, tần số đáp ứng 20Hz-20kHz, phù hợp studio và podcast.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327293/m1yzul1zo6c2n6klhh1i.jpg' },
    { name: 'Router Wi-Fi 4G LTE TP-Link TL-MR6400', sku: 'RO-TPL-001', importPrice: 900000, salePrice: 1290000, description: 'Router Wi-Fi 4G LTE TP-Link TL-MR6400 tốc độ 300Mbps, hỗ trợ cắm SIM 4G trực tiếp, phù hợp vùng không có mạng cáp.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327295/xneflxazvjuzghstzfz8.jpg' },
    { name: 'Ổ Cứng HDD Western Digital Blue 1TB', sku: 'OC-WDS-001', importPrice: 850000, salePrice: 1190000, description: 'Ổ cứng HDD Western Digital Blue 1TB 7200RPM, cache 64MB, giao tiếp SATA III, bảo hành 2 năm.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327299/ernyifrpddn8f06tljnl.jpg' },
    { name: 'Ổ Cứng SSD Samsung 980 Pro 1TB NVMe', sku: 'SSD-SAM-001', importPrice: 2500000, salePrice: 3290000, description: 'Ổ cứng SSD Samsung 980 Pro 1TB PCIe Gen 4.0 x4 NVMe, tốc độ đọc 7000MB/s, ghi 5000MB/s, M.2 2280.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327302/hxomy2ns52bljvqg2xjc.jpg' },
    { name: 'USB Flash Drive 1TB Thiết Kế Xoay', sku: 'USB-1TB-001', importPrice: 1800000, salePrice: 2390000, description: 'USB Flash Drive 1TB với thiết kế xoay tiện lợi, tốc độ USB 3.0, vỏ kim loại chống sốc.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327304/hu1lbghd5muzygleny88.jpg' },
    { name: 'Pin Sạc Dự Phòng 20000mAh', sku: 'PIN-20K-001', importPrice: 350000, salePrice: 549000, description: 'Pin sạc dự phòng 20000mAh hỗ trợ sạc nhanh 22.5W, 2 cổng USB-A, 1 cổng USB-C, màn hình LED hiển thị dung lượng.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327307/cqxwpnjl35ki243y2rzn.jpg' },
    { name: 'Cáp Sạc Nhanh USB-C Ugreen 3.0', sku: 'CAP-UGR-001', importPrice: 80000, salePrice: 149000, description: 'Cáp sạc nhanh USB 3.0 sang USB Type-C Ugreen, hỗ trợ sạc nhanh 60W, dây dài 1m, vỏ bọc nylon bền bỉ.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327310/qznoyuzkuiqtsfaxt7tr.jpg' },
    { name: 'Camera An Ninh EZVIZ H90 Dual 2 Mắt', sku: 'CAM-EZV-001', importPrice: 1800000, salePrice: 2490000, description: 'Camera an ninh EZVIZ H90 Dual với 2 camera, độ phân giải 2K+2K, xoay 360°, đàm thoại 2 chiều, lưu trữ đám mây.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327312/y1roz0fhgguldlewwa3c.jpg' },
    { name: 'Máy In Laser Màu Canon LBP621CW', sku: 'MIN-CAN-001', importPrice: 4500000, salePrice: 5890000, description: 'Máy in laser màu Canon LBP621CW với tốc độ in 18 trang/phút, kết nối Wi-Fi, in qua mobile, độ phân giải 1200x1200 dpi.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327314/ut0vuvrdydd3mobw94qu.jpg' }
  ];

  // Danh mục: Thời Trang
  const thoiTrangProducts = [
    { name: 'Áo Polo Nam Không Túi A2MN', sku: 'TT-POL-001', importPrice: 180000, salePrice: 289000, description: 'Áo polo nam không túi A2MN438R2 chất liệu cotton co giãn, thoáng mát, phù hợp đi làm và dạo phố.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327317/abeszvwjxkxjo4y6kfvl.jpg' },
    { name: 'Áo Sơ Mi Nam Trắng Art Vải Nhăn', sku: 'TT-SMN-001', importPrice: 220000, salePrice: 349000, description: 'Áo sơ mi nam trắng Art với chất liệu vải nhăn thời trang, form regular fit, phù hợp công sở lẫn casual.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327319/p0asw0x5xuqrxuz8psng.jpg' },
    { name: 'Áo Khoác Dù Nam 2 Mặt LADOS Cao Cấp', sku: 'TT-AKD-001', importPrice: 280000, salePrice: 459000, description: 'Áo khoác dù nam 2 mặt LADOS phiên bản cao cấp, chống nước nhẹ, mặc được 2 mặt với 2 màu sắc khác nhau.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327321/ffoqdzffold9vopoa7yu.jpg' },
    { name: 'Quần Jean Nam SMART JEANS Co Giãn', sku: 'TT-QJN-001', importPrice: 250000, salePrice: 399000, description: 'Quần Jean nam SMART JEANS siêu co giãn, form slim fit, màu xanh đậm, phù hợp mọi hoạt động.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327325/kxuxtqwoaxc4divpqiia.jpg' },
    { name: 'Quần Tây Nam Slimfit Xám Chì', sku: 'TT-QTN-001', importPrice: 200000, salePrice: 329000, description: 'Quần tây nam slimfit màu xám chì, chất liệu cao cấp không nhăn, phù hợp đi làm và các sự kiện.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327327/utocbsuxppzdjvmnp9x9.jpg' },
    { name: 'Giày Thể Thao Nam Chạy Bộ', sku: 'TT-GTT-001', importPrice: 350000, salePrice: 549000, description: 'Giày thể thao nam thời trang phù hợp chạy bộ và tập gym, đế cao su chống trượt, mũi giày thoáng khí.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327329/t57ucflzwsjqc8gyrgof.jpg' },
    { name: 'Dép Sandal MLB Chunky New York Yankees', sku: 'TT-DEP-001', importPrice: 1200000, salePrice: 1690000, description: 'Dép sandal MLB Chunky New York Yankees phong cách Hàn Quốc, đế chunky dày, logo thêu nổi bật.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327332/o2izymdx7kfn5gwquwgc.jpg' },
    { name: 'Thắt Lưng Nam Da Thật Gento', sku: 'TT-TLN-001', importPrice: 280000, salePrice: 429000, description: 'Thắt lưng nam da thật Gento với khóa kim loại cao cấp, bề mặt da mềm mại, phù hợp quần tây và jean.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327335/s1shx64skug4op8fpyhg.jpg' },
    { name: 'Ví Da Nam Cao Cấp', sku: 'TT-VDN-001', importPrice: 180000, salePrice: 299000, description: 'Ví da nam cao cấp thiết kế gấp đôi, nhiều ngăn đựng thẻ, chất liệu da PU bền đẹp.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327337/jeflza9hfgogeqkejwgm.jpg' },
    { name: 'Mũ Lưỡi Trai Đen Trơn Classic', sku: 'TT-MUN-001', importPrice: 80000, salePrice: 149000, description: 'Mũ lưỡi trai đen trơn phong cách Classic, chất liệu vải kaki, có khóa điều chỉnh kích cỡ.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327339/pwdohxidi1r9istkiyat.jpg' },
    { name: 'Áo Thun Nữ Hình 2 Chú Thỏ', sku: 'TT-ATN-001', importPrice: 120000, salePrice: 199000, description: 'Áo thun nữ in hình 2 chú thỏ dễ thương, chất liệu cotton mềm mại, form oversize trẻ trung.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327341/ffkkz4x3tquhqu4ujqvm.jpg' },
    { name: 'Áo Sơ Mi Nữ Công Sở Quý Cô', sku: 'TT-SMI-001', importPrice: 180000, salePrice: 289000, description: 'Áo sơ mi nữ không pence dòng cổ điển quý cô công sở, chất liệu lụa cao cấp, form regular fit.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327343/fslwtnzjumynidwcbdfu.jpg' },
    { name: 'Váy Liền Thân Nữ Thanh Lịch', sku: 'TT-VAY-001', importPrice: 250000, salePrice: 389000, description: 'Váy liền thân nữ thiết kế thanh lịch, chất liệu vải cao cấp, phù hợp đi làm và dự tiệc.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327345/sulkscxuditng3hculj8.jpg' },
    { name: 'Chân Váy Ngắn Nữ Công Sở', sku: 'TT-CVN-001', importPrice: 150000, salePrice: 249000, description: 'Chân váy ngắn nữ phong cách công sở, chất liệu vải không nhăn, nhiều màu sắc lựa chọn.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327347/vjaxuruuujko4yq2on6t.jpg' },
    { name: 'Quần Jeans Nữ Ống Rộng New Jean', sku: 'TT-QJN-002', importPrice: 220000, salePrice: 349000, description: 'Quần Jeans nữ wash ống rộng New Jean, chất liệu denim cao cấp, form baggy trẻ trung.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327350/tqzxskrqz5dyxgjtooos.jpg' },
    { name: 'Áo Khoác Nữ Hình Chú Sóc Ngộ Nghĩnh', sku: 'TT-AKN-001', importPrice: 280000, salePrice: 449000, description: 'Áo khoác nữ in hình chú sóc ngộ nghĩnh, chất liệu nỉ dày dặn, giữ ấm tốt cho mùa đông.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327352/pxn0yq8sqp7qteh6xr03.jpg' },
    { name: 'Túi Xách Nữ Cao Cấp GN911', sku: 'TT-TXN-001', importPrice: 450000, salePrice: 699000, description: 'Túi xách nữ cao cấp GN911 thiết kế sang trọng, chất liệu da PU cao cấp, nhiều ngăn tiện lợi.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327354/lmh0mgu6glku8k8gca3n.jpg' },
    { name: 'Giày Cao Gót Đông Hải Mũi Nhọn', sku: 'TT-GCG-001', importPrice: 320000, salePrice: 499000, description: 'Giày cao gót Đông Hải trơn mũi nhọn, gót cao 7cm, chất liệu da tổng hợp bền đẹp.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327356/h1zerjmw2sxh9pwcps2b.jpg' },
    { name: 'Giày Bệt Nữ Da Mềm Quai Ngang', sku: 'TT-GBN-001', importPrice: 200000, salePrice: 319000, description: 'Giày bệt nữ da mềm quai ngang xinh xắn, đế bằng êm ái, phù hợp đi làm và dạo phố.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327358/v1s13q6rbkpkfewzap70.jpg' },
    { name: 'Đồng Hồ Thời Trang Nữ Dây Kim Loại', sku: 'TT-DHN-001', importPrice: 250000, salePrice: 399000, description: 'Đồng hồ thời trang nữ dây kim loại mạ vàng, mặt số thanh lịch, chống nước 3ATM.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327360/gkjgtjeas7uwfbngruog.jpg' },
    { name: 'Kính Mát Thời Trang Nam Polarized', sku: 'TT-KMN-001', importPrice: 180000, salePrice: 289000, description: 'Kính mát thời trang nam tròng Polarized chống chói, gọng kim loại cao cấp, bảo vệ mắt khỏi tia UV.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327362/s4nlcdwxdfvfminmerg4.jpg' },
    { name: 'Khăn Choàng Cổ Nữ Lụa Hoa Văn', sku: 'TT-KCN-001', importPrice: 150000, salePrice: 249000, description: 'Khăn choàng cổ nữ chất liệu lụa mềm mại, họa tiết hoa văn tinh tế, kích thước 180x90cm.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327365/vwmms7a6gtcyloioyhdb.jpg' }
  ];

  // Danh mục: Nhà Cửa & Trang Trí
  const nhaCuaProducts = [
    { name: 'Tranh Canvas Trang Trí Căn Hộ', sku: 'NC-TRA-001', importPrice: 150000, salePrice: 249000, description: 'Tranh treo tường in canvas trang trí căn hộ, hình ảnh phong cảnh thiên nhiên, khung gỗ chắc chắn.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327368/pjbkarqv9hp1q3xfu8vh.jpg' },
    { name: 'Đồng Hồ Treo Tường Kích Thước Lớn', sku: 'NC-DHT-001', importPrice: 280000, salePrice: 429000, description: 'Đồng hồ treo tường kích thước lớn 60cm, thiết kế hiện đại, máy kim trôi êm ái không gây tiếng ồn.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327374/xpxruxleulkqwrdvoaq4.jpg' },
    { name: 'Rèm Cửa Vải Nhung Sang Trọng', sku: 'NC-REM-001', importPrice: 350000, salePrice: 549000, description: 'Rèm cửa bằng vải nhung mềm mại sang trọng, cản sáng tốt, kích thước 150x250cm, có móc treo.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327376/tbzyer2u5vrszwj44xvl.jpg' },
    { name: 'Thảm Nỉ Trải Sàn Màu Xám Trắng', sku: 'NC-THA-001', importPrice: 280000, salePrice: 449000, description: 'Thảm nỉ trải sàn sự kiện màu xám trắng, chống trượt, dễ vệ sinh, kích thước 160x230cm.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327378/rvhn7fehldae6wct3l5o.jpg' },
    { name: 'Vỏ Gối Trang Trí Sofa', sku: 'NC-GOI-001', importPrice: 50000, salePrice: 99000, description: 'Vỏ gối trang trí sofa họa tiết hình học, chất liệu vải canvas, kích thước 45x45cm, có khóa kéo.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327380/cepjjtthiz6uzbxfcfmu.jpg' },
    { name: 'Dây Đèn Trang Trí 5m 20 Bóng Tròn', sku: 'NC-DEN-001', importPrice: 80000, salePrice: 139000, description: 'Dây đèn trang trí 5m với 20 bóng tròn LED, ánh sáng vàng ấm, chống nước IP44, phù hợp trang trí nội ngoại thất.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327382/qwmndchgckljmyflbo8i.jpg' },
    { name: 'Đèn Ngủ Để Bàn Euroto B-025L', sku: 'NC-DNG-001', importPrice: 120000, salePrice: 199000, description: 'Đèn ngủ để bàn Euroto B-025L thiết kế đẹp, ánh sáng dịu nhẹ, tiết kiệm điện, có công tắc bật/tắt.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327385/ngrryjxobne66zx7zcj7.jpg' },
    { name: 'Cây Cảnh Để Bàn Mini', sku: 'NC-CAY-001', importPrice: 60000, salePrice: 119000, description: 'Cây cảnh để bàn mini trang trí văn phòng, cây giả chất liệu nhựa cao cấp, không cần tưới nước.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327389/uiulsfyni3ney1spwqrd.jpg' },
    { name: 'Bình Hoa Gốm Sứ Trang Trí', sku: 'NC-BHO-001', importPrice: 100000, salePrice: 179000, description: 'Bình hoa gốm sứ trang trí phong cách Bắc Âu, màu trắng tinh khiết, cao 25cm, phù hợp cắm hoa khô.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327393/vuq1uhhwnylpebfw6x97.jpg' },
    { name: 'Kệ Gỗ Trang Trí KG011', sku: 'NC-KEG-001', importPrice: 180000, salePrice: 289000, description: 'Kệ gỗ trang trí KG011 đa năng, có thể treo tường hoặc đặt bàn, chất liệu gỗ MDF phủ melamine.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327397/jajbbjbtlfhh1fbvcsnb.jpg' },
    { name: 'Kệ Sách Để Bàn Gỗ MDF SAI SHAN GAPI', sku: 'NC-KES-001', importPrice: 250000, salePrice: 399000, description: 'Kệ sách để bàn SAI SHAN GAPI gỗ MDF cao cấp, thiết kế 3 tầng, phù hợp đựng sách và đồ dùng văn phòng.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327399/hryzpjxyp6vvw29damoi.jpg' },
    { name: 'Bàn Làm Việc PTR120C2', sku: 'NC-BAN-001', importPrice: 1200000, salePrice: 1690000, description: 'Bàn làm việc PTR120C2 kích thước 120x60cm, mặt bàn gỗ công nghiệp, chân sắt sơn tĩnh điện chắc chắn.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327401/dqkn7np101mqtfti520m.jpg' },
    { name: 'Ghế Làm Việc Tại Nhà GTN13', sku: 'NC-GHE-001', importPrice: 800000, salePrice: 1190000, description: 'Ghế làm việc tại nhà GTN13 có tựa lưng cao, đệm mút dày êm ái, bánh xe di chuyển linh hoạt.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327403/rjgvjelizxwsrwm0sub0.jpg' },
    { name: 'Ghế Sofa Băng Dài Giá Rẻ', sku: 'NC-SOF-001', importPrice: 2500000, salePrice: 3490000, description: 'Ghế sofa băng dài 1m8, bọc vải cao cấp, đệm mút D40 êm ái, khung gỗ tự nhiên chắc chắn.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327408/ei76bgoemjf1n7obzz50.png' },
    { name: 'Tủ Quần Áo Đơn Nhỏ Gọn', sku: 'NC-TUA-001', importPrice: 1500000, salePrice: 2190000, description: 'Tủ quần áo đơn nhỏ gọn kích thước 80x50x180cm, gỗ công nghiệp MDF, có gương và nhiều ngăn.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327411/ch5ptnworpvoceje6crx.jpg' },
    { name: 'Giường Ngủ Hiện Đại Có Ngăn Kéo', sku: 'NC-GIU-001', importPrice: 3500000, salePrice: 4690000, description: 'Giường ngủ hiện đại kích thước 1m6x2m với 2 ngăn kéo tiện lợi, gỗ công nghiệp cao cấp, chịu lực tốt.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327414/toqkzzxx5x04qnlelun4.jpg' },
    { name: 'Gương Trang Trí Phòng Khách Cách Điệu', sku: 'NC-GUO-001', importPrice: 350000, salePrice: 549000, description: 'Gương trang trí phòng khách thiết kế cách điệu hiện đại, khung kim loại mạ vàng, đường kính 60cm.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327417/hofqqknxuwqm36xz2j9o.jpg' },
    { name: 'Nến Thơm Mưa Relax', sku: 'NC-NEN-001', importPrice: 80000, salePrice: 149000, description: 'Nến thơm hương mưa thư giãn, thời gian cháy lên đến 45 giờ, không khói, an toàn cho sức khỏe.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327422/txt0lcqrgwxd1fq23awu.jpg' },
    { name: 'Khung Ảnh FISKBO IKEA 30x40cm', sku: 'NC-KHA-001', importPrice: 50000, salePrice: 99000, description: 'Khung ảnh FISKBO IKEA màu trắng kích thước 30x40cm, có chân đứng và móc treo tường.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327424/ysnx75odlnmsxh1my5pn.jpg' },
    { name: 'Tượng Decor Nam Thần Hy Lạp', sku: 'NC-TUO-001', importPrice: 180000, salePrice: 289000, description: 'Tượng decor nam thần Hy Lạp bằng nhựa composite, cao 30cm, phù hợp trang trí kệ sách và phòng khách.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327427/di0wncg93zdvgsbxwuvr.jpg' },
    { name: 'Giỏ Nhựa Đựng Đồ Đa Năng', sku: 'NC-GIO-001', importPrice: 35000, salePrice: 69000, description: 'Giỏ nhựa đựng đồ đa năng, chất liệu PP cao cấp, màu sắc pastel xinh xắn, kích thước 30x20x15cm.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327429/bet7jhwudipkwnbxqmdp.jpg' },
    { name: 'Hộp Đựng Quà Kraft Kèm Trang Trí', sku: 'NC-HOP-001', importPrice: 25000, salePrice: 49000, description: 'Hộp đựng quà tặng Kraft kèm trang trí nơ, kích thước 20x15x10cm, phù hợp làm quà sinh nhật và lễ tết.', imageUrl: 'https://res.cloudinary.com/dd6hyrrdf/image/upload/v1768327431/h57gl4elimaxiiwyiqyt.jpg' }
  ];

  // Create products with images
  const dienTuCategory = categoryList.find(c => c.name === 'Điện Tử')!;
  const thoiTrangCategory = categoryList.find(c => c.name === 'Thời Trang')!;
  const nhaCuaCategory = categoryList.find(c => c.name === 'Nhà Cửa & Trang Trí')!;

  const allProductsData = [
    ...dienTuProducts.map(p => ({ ...p, categoryId: dienTuCategory.id })),
    ...thoiTrangProducts.map(p => ({ ...p, categoryId: thoiTrangCategory.id })),
    ...nhaCuaProducts.map(p => ({ ...p, categoryId: nhaCuaCategory.id }))
  ];

  // Create each product with 3 images
  for (const productData of allProductsData) {
    const stock = randomInt(2, 20);
    const product = await prisma.product.create({
      data: {
        sku: productData.sku,
        name: productData.name,
        importPrice: productData.importPrice,
        salePrice: productData.salePrice,
        stock: stock,
        description: productData.description,
        categoryId: productData.categoryId,
        images: {
          create: [
            { url: productData.imageUrl },
            { url: 'https://placehold.co/300x300/08ffff/01?text=Image+2' },
            { url: 'https://placehold.co/300x300/08ffff/01?text=Image+3' }
          ]
        }
      }
    });
  }

  const products = await prisma.product.findMany({ include: { images: true } });
  console.log('✅ Products and ProductImages created');

  // =========================
  // 6. CREATE ORDERS FOR CUSTOMERS
  // Each customer gets 3 orders, each with ~3 random products
  // =========================
  const statuses = [OrderStatus.PAID, OrderStatus.PENDING, OrderStatus.DRAFT];
  const users = [admin, sale1, sale2];

  for (const customer of customerList) {
    for (let orderIndex = 0; orderIndex < 3; orderIndex++) {
      const orderItems = [];
      let finalPrice = 0;

      // Random 3 products for each order
      const selectedProducts = products
        .sort(() => 0.5 - Math.random())
        .slice(0, 3);

      const randomUser = users[Math.floor(Math.random() * users.length)];
      const randomStatus = statuses[Math.floor(Math.random() * statuses.length)];

      // Generate random date within the last 30 days
      const daysAgo = Math.floor(Math.random() * 30);
      const orderDate = new Date();
      orderDate.setDate(orderDate.getDate() - daysAgo);

      for (const product of selectedProducts) {
        const quantity = randomInt(1, 4);
        const unitSalePrice = product.salePrice;
        const totalPrice = quantity * unitSalePrice;

        finalPrice += totalPrice;

        orderItems.push({
          productId: product.id,
          quantity,
          unitSalePrice,
          totalPrice
        });
      }

      await prisma.order.create({
        data: {
          finalPrice,
          status: randomStatus,
          customerId: customer.id,
          createdById: randomUser.id,
          createdTime: orderDate,
          orderItems: {
            create: orderItems
          }
        }
      });
    }
  }

  console.log('✅ Orders created (3 orders per customer)');
  console.log('✅ Database seeding completed.');
  console.log('');
  console.log('📊 Summary:');
  console.log(`   - Users: 3 (1 Admin, 2 Sales)`);
  console.log(`   - Customers: ${customerList.length}`);
  console.log(`   - Categories: ${categoryList.length}`);
  console.log(`   - Products: ${products.length}`);
  console.log(`   - Orders: ${customerList.length * 3}`);
}

main()
  .catch((e) => {
    console.error('❌ Seeding error:', e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
