# مسیرهای قابل دسترسی — ۲۰۲۶-۱۰-۱۱

فهرست ایستای attributeها؛ تأیید اجرایی مجوز/مالکیت نیست. مسیر نسبی با Route کلاس ترکیب می‌شود؛ / ابتدای action مسیر مطلق است. [controller] نام کلاس بدون Controller؛ route متعارف Program.cs فعال است. ستون مجوز attribute کلاس/متد است؛ برخی actionهای مهمان مجوز نشست را با CustomerSession بررسی می‌کنند.

| فایل:خط | Route کلاس | روش/مسیر action | مجوز اعلامی |
|---|---|---|---|
| lern/Controller/AccountApiController.cs:28 | api/account | [HttpPost("register")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AccountApiController.cs:37 | api/account | [HttpPost("login")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AccountApiController.cs:47 | api/account | [HttpPost("logout")] | Authorize |
| lern/Controller/AccountApiController.cs:55 | api/account | [HttpGet("me")] | Authorize |
| lern/Controller/AccountApiController.cs:64 | api/account | [HttpPost("profile-image")] | Authorize |
| lern/Controller/AccountApiController.cs:119 | api/account | [HttpPut("me")] | Authorize |
| lern/Controller/AccountController.cs:26 | متعارف/مطلق | [HttpGet("/login")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AccountController.cs:34 | متعارف/مطلق | [HttpGet("/account/access-denied")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AccountController.cs:35 | متعارف/مطلق | [HttpGet("/Account/AccessDenied")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AccountController.cs:45 | متعارف/مطلق | [HttpGet("/register")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AccountController.cs:49 | متعارف/مطلق | [HttpGet("/account")] | Authorize |
| lern/Controller/AccountController.cs:100 | متعارف/مطلق | [HttpGet("/account/discounts")] | Authorize |
| lern/Controller/AccountController.cs:127 | متعارف/مطلق | [HttpGet("/account/change-password")] | Authorize |
| lern/Controller/AccountController.cs:132 | متعارف/مطلق | [HttpPost("/account/change-password")] | Authorize |
| lern/Controller/AccountController.cs:150 | متعارف/مطلق | [HttpGet("/account/profile")] | Authorize |
| lern/Controller/AccountController.cs:154 | متعارف/مطلق | [HttpPost("/account/logout")] | Authorize |
| lern/Controller/AccountController.cs:163 | متعارف/مطلق | [HttpGet("/account/orders")] | Authorize |
| lern/Controller/AccountController.cs:215 | متعارف/مطلق | [HttpGet("/account/orders/{id:int}")] | Authorize |
| lern/Controller/AccountController.cs:226 | متعارف/مطلق | [HttpGet("/account/addresses")] | Authorize |
| lern/Controller/AccountController.cs:237 | متعارف/مطلق | [HttpPost("/account/addresses/{id:int}/delete")] | Authorize |
| lern/Controller/AccountController.cs:250 | متعارف/مطلق | [HttpPost("/account/addresses/{id:int}/default")] | Authorize |
| lern/Controller/AccountController.cs:265 | متعارف/مطلق | [HttpGet("/account/comments")] | Authorize |
| lern/Controller/AccountController.cs:311 | متعارف/مطلق | [HttpGet("/account/comments/edit")] | Authorize |
| lern/Controller/AccountController.cs:324 | متعارف/مطلق | [HttpPost("/account/comments/{id:int}/delete")] | Authorize |
| lern/Controller/AccountController.cs:337 | متعارف/مطلق | [HttpGet("/account/comments/{id:int}/edit")] | Authorize |
| lern/Controller/AccountController.cs:355 | متعارف/مطلق | [HttpPost("/account/comments/{id:int}/edit")] | Authorize |
| lern/Controller/AccountDashboardController.cs:18 | api/account/dashboard | [HttpGet] | Authorize |
| lern/Controller/AccountInboxController.cs:15 | api/account/inbox | [HttpGet] | Authorize |
| lern/Controller/AddressApiController.cs:17 | api/address | [HttpGet] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AddressApiController.cs:27 | api/address | [HttpPut] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AddressApiController.cs:60 | api/address | [HttpPost] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AddressApiController.cs:76 | api/address | [HttpGet("list")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AddressApiController.cs:86 | api/address | [HttpPost("{id:int}/default")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AddressApiController.cs:96 | api/address | [HttpDelete("{id:int}")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/AdminArticlesController.cs:14 | Admin/Articles | [HttpGet("")] | Admin |
| lern/Controller/AdminArticlesController.cs:34 | Admin/Articles | [HttpGet("create")] | Admin |
| lern/Controller/AdminArticlesController.cs:37 | Admin/Articles | [HttpGet("{id:int}/edit")] | Admin |
| lern/Controller/AdminArticlesController.cs:50 | Admin/Articles | [HttpPost("create")] | Admin |
| lern/Controller/AdminArticlesController.cs:51 | Admin/Articles | [HttpPost("{id:int}/edit")] | Admin |
| lern/Controller/AdminCommentsController.cs:14 | Admin/Comments | [HttpGet("")] | Admin |
| lern/Controller/AdminCommentsController.cs:42 | Admin/Comments | [HttpPost("{id:int}/moderate")] | Admin |
| lern/Controller/AdminComplaintsController.cs:14 | Admin/Complaints, Admin/Settings/complaints | [HttpGet("")] | Admin |
| lern/Controller/AdminComplaintsController.cs:28 | Admin/Complaints, Admin/Settings/complaints | [HttpPost("{id:int}/review")] | Admin |
| lern/Controller/AdminController.cs:33 | Admin | [HttpGet("")] | Admin |
| lern/Controller/AdminController.cs:75 | Admin | [HttpGet("Products/Create")] | Admin |
| lern/Controller/AdminController.cs:88 | Admin | [HttpGet("Products")] | Admin |
| lern/Controller/AdminController.cs:108 | Admin | [HttpGet("Categories")] | Admin |
| lern/Controller/AdminController.cs:135 | Admin | [HttpGet("Users")] | Admin |
| lern/Controller/AdminController.cs:158 | Admin | [HttpGet("Users/Create")] | Admin |
| lern/Controller/AdminController.cs:164 | Admin | [HttpPost("Users/Create")] | Admin |
| lern/Controller/AdminController.cs:209 | Admin | [HttpGet("Users/{id:int}/Edit")] | Admin |
| lern/Controller/AdminController.cs:222 | Admin | [HttpPost("Users/{id:int}/Edit")] | Admin |
| lern/Controller/AdminController.cs:257 | Admin | [HttpPost("Users/{id:int}/Delete")] | Admin |
| lern/Controller/AdminController.cs:270 | Admin | [HttpPost("Categories")] | Admin |
| lern/Controller/AdminController.cs:289 | Admin | [HttpPost("Categories/{id:int}/Edit")] | Admin |
| lern/Controller/AdminController.cs:312 | Admin | [HttpPost("Categories/{id:int}/Delete")] | Admin |
| lern/Controller/AdminController.cs:334 | Admin | [HttpGet("Products/Edit")] | Admin |
| lern/Controller/AdminController.cs:341 | Admin | [HttpPost("Products/Delete")] | Admin |
| lern/Controller/AdminController.cs:369 | Admin | [HttpGet("Products/{id:int}/Edit")] | Admin |
| lern/Controller/AdminController.cs:420 | Admin | [HttpPost("Products/{id:int}/Edit")] | Admin |
| lern/Controller/AdminController.cs:518 | Admin | [HttpPost("Products")] | Admin |
| lern/Controller/AdminCouponsController.cs:15 | Admin/Coupons | [HttpGet("")] | Admin |
| lern/Controller/AdminCouponsController.cs:19 | Admin/Coupons | [HttpGet("create")] | Admin |
| lern/Controller/AdminCouponsController.cs:22 | Admin/Coupons | [HttpGet("customers")] | Admin |
| lern/Controller/AdminCouponsController.cs:40 | Admin/Coupons | [HttpPost("create")] | Admin |
| lern/Controller/AdminCouponsController.cs:86 | Admin/Coupons | [HttpPost("{id:int}/toggle")] | Admin |
| lern/Controller/AdminCouponsController.cs:97 | Admin/Coupons | [HttpPost("{id:int}/delete")] | Admin |
| lern/Controller/AdminOrdersController.cs:15 | Admin/Orders | [HttpGet("")] | Admin |
| lern/Controller/AdminOrdersController.cs:67 | Admin/Orders | [HttpPost("{id:int}/shipping")] | Admin |
| lern/Controller/AdminOrdersController.cs:83 | Admin/Orders | [HttpGet("{id:int}")] | Admin |
| lern/Controller/AdminReportsController.cs:18 | Admin/Reports | [HttpGet("")] | Admin |
| lern/Controller/AdminSeoController.cs:14 | Admin/Seo | [HttpGet("")] | Admin |
| lern/Controller/AdminSeoController.cs:25 | Admin/Seo | [HttpGet("children")] | Admin |
| lern/Controller/AdminSeoController.cs:35 | Admin/Seo | [HttpPost("")] | Admin |
| lern/Controller/AdminSeoController.cs:73 | Admin/Seo | [HttpGet("/Admin/Settings/category-seo")] | Admin |
| lern/Controller/AdminSeoController.cs:84 | Admin/Seo | [HttpGet("/Admin/Settings/category-seo/children")] | Admin |
| lern/Controller/AdminSeoController.cs:94 | Admin/Seo | [HttpGet("/Admin/Settings/category-seo/{id:int}/editor")] | Admin |
| lern/Controller/AdminSeoController.cs:104 | Admin/Seo | [HttpPost("/Admin/Settings/category-seo/{id:int}")] | Admin |
| lern/Controller/AdminSettingsController.cs:15 | Admin/Settings | [HttpGet("")] | Admin |
| lern/Controller/AdminSettingsController.cs:18 | Admin/Settings | [HttpGet("banners")] | Admin |
| lern/Controller/AdminSettingsController.cs:24 | Admin/Settings | [HttpGet("inbox")] | Admin |
| lern/Controller/AdminSettingsController.cs:31 | Admin/Settings | [HttpGet("category-images")] | Admin |
| lern/Controller/AdminSettingsController.cs:38 | Admin/Settings | [HttpPost("category-images/{id:int}")] | Admin |
| lern/Controller/AdminSettingsController.cs:62 | Admin/Settings | [HttpPost("banner/{id:int}")] | Admin |
| lern/Controller/AdminSettingsController.cs:129 | Admin/Settings | [HttpPost("banner/{id:int}/delete")] | Admin |
| lern/Controller/AdminSettingsController.cs:143 | Admin/Settings | [HttpPost("inbox")] | Admin |
| lern/Controller/AdminSettingsController.cs:163 | Admin/Settings | [HttpPost("banner/create")] | Admin |
| lern/Controller/AdminTransactionsController.cs:14 | Admin/Transactions | [HttpGet("")] | Admin |
| lern/Controller/ArticlesController.cs:11 | articles | [HttpGet("")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/ArticlesController.cs:12 | articles | [HttpGet("/blog")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/ArticlesController.cs:29 | articles | [HttpGet("{slug}")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/CartController.cs:14 | api/cart | [HttpGet] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/CartController.cs:61 | api/cart | [HttpPost] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/CartController.cs:98 | api/cart | [HttpPut("{id}")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/CartController.cs:122 | api/cart | [HttpDelete("{id}")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/CartPageController.cs:8 | cart | [HttpGet] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/FavoritesController.cs:16 | متعارف/مطلق | [HttpGet("/account/favorites")] | Authorize |
| lern/Controller/FavoritesController.cs:41 | متعارف/مطلق | [HttpGet("/api/favorites")] | Authorize |
| lern/Controller/FavoritesController.cs:45 | متعارف/مطلق | [HttpPut("/api/favorites/{productId:int}")] | Authorize |
| lern/Controller/FavoritesController.cs:59 | متعارف/مطلق | [HttpDelete("/api/favorites/{productId:int}")] | Authorize |
| lern/Controller/OrderController.cs:22 | api/orders | [HttpGet] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/OrderController.cs:28 | api/orders | [HttpPost("coupon/preview")] | Authorize |
| lern/Controller/OrderController.cs:45 | api/orders | [HttpPost("checkout")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/OrderController.cs:119 | api/orders | [HttpPost("{id}/pay")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/OrderPaymentsController.cs:8 | متعارف/مطلق | [HttpGet("/api/payments/status")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/OrderPaymentsController.cs:11 | متعارف/مطلق | [HttpPost("/api/orders/{id:int}/payment")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/OrderPaymentsController.cs:25 | متعارف/مطلق | [HttpGet("/payments/callback")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/OrderReceiptController.cs:11 | متعارف/مطلق | [HttpGet("/orders/{id:int}")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/ProductController.cs:19 | api/[controller] | [HttpGet("cards")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/ProductController.cs:25 | api/[controller] | [HttpGet("{id}")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/ProductController.cs:34 | api/[controller] | [HttpPost] | Admin |
| lern/Controller/ProductController.cs:43 | api/[controller] | [HttpGet("{id}/edit")] | Admin |
| lern/Controller/ProductController.cs:51 | api/[controller] | [HttpPut("{id}")] | Admin |
| lern/Controller/ProductController.cs:65 | api/[controller] | [HttpDelete("{id}")] | Admin |
| lern/Controller/ProductRatingController.cs:19 | api/products/{productId:int}/rating | [HttpPut] | Authorize |
| lern/Controller/ProductRatingController.cs:45 | api/products/{productId:int}/rating | [HttpPost("/api/products/{productId:int}/comments")] | Authorize |
| lern/Controller/ProductRatingController.cs:100 | api/products/{productId:int}/rating | [HttpGet("/api/products/{productId:int}/comments")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/ProductRatingController.cs:142 | api/products/{productId:int}/rating | [HttpPut("/api/product-comments/{id:int}")] | Authorize |
| lern/Controller/ProductRatingController.cs:185 | api/products/{productId:int}/rating | [HttpDelete("/api/product-comments/{id:int}")] | Authorize |
| lern/Controller/ProductRatingController.cs:202 | api/products/{productId:int}/rating | [HttpPut("/api/product-comments/{id:int}/approval")] | Admin |
| lern/Controller/SecurityController.cs:13 | متعارف/مطلق | [HttpGet("/api/security/csrf")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/ShopController.cs:12 | shop | [HttpGet("")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/ShopController.cs:69 | shop | [HttpGet("suggest")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/SitemapController.cs:12 | متعارف/مطلق | [HttpGet("/sitemap.xml")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/SitemapController.cs:22 | متعارف/مطلق | [HttpGet("/sitemap-{page:int}.xml")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/SitemapController.cs:30 | متعارف/مطلق | [HttpGet("/robots.txt")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/StoreCategoryController.cs:10 | category | [HttpGet("{id:int}")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/StoreMenuController.cs:15 | menu | [HttpGet("categories")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/StoreProductController.cs:21 | product | [HttpGet("{id:int}")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/StoreSupportController.cs:11 | متعارف/مطلق | [HttpGet("/about")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/StoreSupportController.cs:14 | متعارف/مطلق | [HttpGet("/contact")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/StoreSupportController.cs:17 | متعارف/مطلق | [HttpGet("/faq")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/StoreSupportController.cs:20 | متعارف/مطلق | [HttpGet("/complaints")] | عمومی؛ بررسی داخلی لازم |
| lern/Controller/StoreSupportController.cs:24 | متعارف/مطلق | [HttpPost("/complaints")] | عمومی؛ بررسی داخلی لازم |

دیگر سطح‌ها: wwwroot عمومی شامل uploads؛ Swagger فقط development؛ migration/seed هنگام startup؛ OrderReservationWorker پس‌زمینه؛ callback پرداخت GET؛ SQL Server محلی با دسترسی شبکه تأییدنشده. OAuth/WebSocket/LLM/BaaS در سرویس‌های بررسی‌شده نیست. AllowedHosts production باید دامنه واقعی باشد. پشت proxy فقط proxy شناخته‌شده برای ForwardedHeaders تعریف شود؛ بدون توپولوژی میزبان این تنظیم اعمال نشد.

## تغییرات احراز هویت

| مسیر/جزء | روش | دسترسی | ورودی |
|---|---|---|---|
| /api/account/logout-all | POST | Authorize + StoreWriteProtection | توکن CSRF یا Origin دقیق |
| /account/logout-all | POST | Authorize + ValidateAntiForgeryToken | فرم CSRF |
| AuthenticationSessionWorker | پس‌زمینه، بدون route | داخلی | حذف نشست‌های منقضی/نامعتبر |

StoreCookieEvents نقش قدیمی را ارتقا نمی‌دهد؛ کوکی قدیمی نیازمند ورود مجدد است. migration/seed startup فقط با تنظیم صریح Database:ApplyMigrationsOnStartup=true انجام می‌شود؛ پیش‌فرض خاموش است.
