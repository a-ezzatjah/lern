# نتیجه بررسی امنیت — ۲۰۲۶-۱۰-۱۱

برای انتشار عمومی آماده نیست. این بررسی کد و آزمون اجزای اصلاح‌شده است؛ تأیید امنیت کامل یا آزمون سرور واقعی نیست.

کیت برای فروشگاه دارای حساب، سفارش و دادهٔ شخصی مناسب است. دو فایل راهنمای بزرگ محتوای یکسان دارند. مثال‌های Node/npm، OAuth، BaaS و LLM به قابلیت فعلی این پروژه مربوط نیستند. پنج فایل اصلی برای مراجعات بعدی در همین پوشه نگه داشته شده‌اند؛ دستورهای commit، branch و تغییر محیط واقعی اجرا نشده‌اند.

| شناسه | شدت | شاهد | مسئله و اصلاح | وضعیت |
|---|---|---|---|---|
| F01 / H9 | High | Entities/Entities.csproj؛ خروجی NuGet audit | وابستگی System.Security.Cryptography.Xml نسخه 8.0.3 دارای پنج advisory شدید بود؛ نسخه اصلاح‌شده 8.0.4 در همان major صریحاً تعیین شد | اصلاح شد؛ ممیزی نهایی NuGet چهار پروژه بدون بسته آسیب‌پذیر گزارش‌شده |
| F02 / A10,A12 | High | SqlAuthenticationTicketStore؛ RevokeSessionsAsync؛ AuthSecuritySmoke | مخزن نشست استاندارد در SQL، حذف نشست خروج و ابطال همه نشست‌ها پس از تغییر رمز/نقش/disable/delete اضافه شد | اصلاح و آزمون محلی موفق؛ migration روی DB واقعی اجرا نشده |
| F03 / A3 | High | UserAuthService و ستون‌های Users؛ AuthSecuritySmoke | پنج خطا در ۱۵ دقیقه، قفل ۱۵ دقیقه و backoff تا ۲۴ ساعت؛ قفل row در SQL Server | اصلاح و آزمون محلی ۱۵/۳۰ دقیقه موفق؛ همزمانی SQL Server هنوز runtime بررسی نشده |
| F04 / S11,S12 | Medium | StorePasswords؛ فرم‌های ثبت/تغییر/مدیریت؛ AuthSecuritySmoke | framework PBKDF2-SHA512 600000 و قالب نسخه‌دار؛ legacy کامل بررسی/ارتقا می‌یابد؛ رمز جدید ۱۵+؛ فهرست کوچک رمز رایج | ارتقای سازگار انجام شد؛ Argon2id/FIPS و blocklist جامع کیت محقق نیست |
| F05 / A16,H6 | High | Program.cs؛ StoreRequestSecurity.cs؛ tools/SecuritySmoke | APIهای cookie فاقد محافظت سراسری درخواست جعلی بودند. فیلتر همه POST/PUT/DELETE را با Origin دقیق یا antiforgery بررسی می‌کند؛ مسیر دریافت توکن برای ابزارهای API اضافه شد | اصلاح و آزمون اجزا موفق؛ تست کامل صفحات فروشگاه انجام نشد |
| F06 / S14,A6,H8,H11 | Medium | Program.cs؛ StoreRequestSecurity.cs | HTTPS/HSTS اولیه و کوکی Secure در production؛ Swagger فقط development؛ هدر جلوگیری از قاب، nosniff، referrer و permissions؛ خطای عمومی و no-store افزوده شد | اصلاح کد؛ HTTPS و میزبان واقعی تأیید نشده |
| F07 / D6 | Medium | lern/appsettings.json، ConnectionStrings:DefaultConnectionstring | تنظیم توسعه اعتبار گواهی SQL را بررسی نمی‌کند؛ production باید اتصال با Encrypt=True و TrustServerCertificate=False و گواهی معتبر داشته باشد | باقی؛ اتصال توسعه تغییر نکرد |
| F08 / H10 | Medium | AdminController.IsValidImage / IsValidAvatar / Create؛ AccountApiController.HasValidImageSignature؛ AdminArticlesController.IsImage | برخی آپلودها تنها پسوند/اندازه یا magic bytes دارند؛ decode واقعی، محدودیت ابعاد/حجم بازشده و re-encode ندارند؛ پروفایل در wwwroot عمومی ذخیره می‌شود | باقی؛ انتخاب کتابخانه تصویر و حریم خصوصی فایل لازم |
| F09 / H5 | Medium | RichText.cs؛ StoreRequestSecurity.cs | HTML عمدی با HtmlSanitizer پالایش می‌شود؛ CSP فعلی قاب، plugin و base را محدود می‌کند و هنوز سیاست سخت script-src ندارد؛ قالب‌ها inline script دارند | بخشی اصلاح شد؛ CSP کامل نیازمند تغییر قالب‌ها |
| F10 / D2,D7,D10,D11,D12,A15 | Medium | CustomerUser.cs؛ AccountApiController.cs؛ data-inventory.md | کد ملی/تولد بدون کاربرد روشن؛ رمزنگاری فیلدی، حذف حساب، نگهداری داده، بکاپ آزموده و MFA مدیر تأیید/پیاده‌سازی نشده | باقی؛ تصمیم مالک و تنظیم محیط لازم |

قابلیت پرداخت فعلاً UnavailablePaymentGateway دارد. کد OrderPayments تأیید را به gateway سمت سرور واگذار می‌کند؛ تطابق merchant/currency/signature ارائه‌دهنده واقعی تا انتخاب درگاه تأییدشدنی نیست. اطلاعات PAN/CVV/PIN در مدل‌های بررسی‌شده دیده نشد.

محدودیت: gitleaks تاریخچه نصب/اجرا نشده؛ JS قالب، تمام مسیرها با دو کاربر، TLS/backup/monitoring میزبان و همزمانی SQL Server تأیید نشده. تست‌های ورود/نشست روی دیتابیس ساختگی حافظه‌ای و HTTP loopback انجام شد؛ reset/MFA/email وجود ندارد. AdminAccessSmoke فقط build شد؛ اجرا نشد چون روی کاربران موجود کار می‌کند.

مالک پیاده‌سازی و آزمون محلی نشست/قفل/رمز را تأیید کرد؛ انجام شد. اجرای migration روی دیتابیس فعلی/آنلاین، MFA/recovery و نگهداری/حذف داده هنوز نیازمند تأیید و اطلاعات میزبان است. جزئیات در auth-implementation.md و SQL قابل بازبینی authentication-migration.sql.
