# منتشر نکن — گزارش ۲۰۲۶-۱۰-۱۱

اصلاح ورود/نشست پس از تأیید مالک پیاده‌سازی و محلی آزموده شد؛ F02/F03 در کد بسته شدند. تا اعمال migration و تأیید سایر blockerها برای انتشار عمومی آماده نیست. محیط آنلاین/دیتابیس فروشگاه تغییر نکرد. پنج فایل اصلی کیت برای مراجعات بعدی حفظ شده‌اند.

اصلاحات: محافظت write با Origin دقیق یا توکن CSRF؛ سقف IP برای ورود/ثبت/تغییر رمز؛ Secure کوکی production؛ HTTPS/HSTS اولیه؛ Swagger فقط development؛ خطای عمومی؛ هدرهای امنیتی/no-store؛ ignore اسرار؛ XML 8.0.3 به 8.0.4. محدودیت IP حافظه‌ای و بدون per-account/قفل مشترک است. CSP هنوز script-src سخت ندارد. نشست SQL و ابطال، قفل حساب/backoff، رمز نسخه‌دار سازگار و خروج همه دستگاه‌ها نیز اضافه شد؛ مهاجرت خودکار startup به‌طور پیش‌فرض خاموش است.

Build محلول و build AdminAccessSmoke موفق؛ SecuritySmoke: ۳۷ بررسی؛ AuthSecuritySmoke: ۷۰ بررسی محلی. SQLite تست به bundle 2.1.13 ارتقا یافت؛ ممیزی NuGet محلول و پروژه تست بسته آسیب‌پذیر شناخته‌شده گزارش نکرد. EF pending-model-changes: تغییر معوق ندارد. SQL migration فقط تولید شد و اجرا نشد. gitleaks/JS audit/میزبان تأیید نشده.

| شناسه | مورد کیت | وضعیت | شاهد | کار لازم |
|---|---|---|---|---|
| S1 | No secrets in code, config, tests, or logs **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S2 | `.env*` in `.gitignore`; `.env.example` has placeholders only **[BLOCKER]** | ✅ | .gitignore و .env.example؛ الگوهای ignore و placeholder | ASP.NET فایل env را خودکار نمی‌خواند؛ از environment میزبان استفاده کنید |
| S3 | No secret in frontend bundle or `NEXT_PUBLIC_`/`VITE_`/`REACT_APP_` vars; public keys are protected by backend rules **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S4 | Separate keys for dev / staging / production | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S5 | Keys have least privilege and spending limits | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S6 | Privileged/confidential-credential calls (AI, payment, SMS...) run on the backend; browser SDKs use only restricted public keys | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S7 | Secret scanning (`gitleaks`) ran on code and full history: no findings **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S8 | No leaked key is still active (rotated first; git history rewritten only if it was committed, with my approval) | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S9 | No real secret was pasted into AI chats, issues, or screenshots | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S10 | Passwords of hosting/DB/GitHub/email are in a password manager with 2FA on | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S11 | Passwords hashed with Argon2id (or scrypt / PBKDF2 for FIPS; bcrypt legacy only), real parameters verified, no truncation, no plaintext/MD5/SHA1 **[BLOCKER]** | ❌ | PasswordHasher framework: PBKDF2-SHA512 600000؛ legacy upgrade با حفظ رمز | بهبود رمز انجام شد؛ شرط Argon2id یا PBKDF2 صرفاً FIPS کیت عیناً محقق نیست |
| S12 | Password policy follows NIST rev. 4 (15+ chars if password-only, 8+ only with enforced MFA, 64+ allowed, no truncation, breached check, no forced composition/rotation) | ❌ | ۱۵ code point و ۶۴+ و Unicode و local blocklist؛ آزمون‌های موفق | blocklist کوچک است؛ فهرست جامع رمز افشاشده تأیید نشده |
| S13 | Tokens/IDs come from a secure random generator | ✅ | UserAuthService از RandomNumberGenerator؛ کوکی با framework DataProtection؛ CustomerSession شناسه Guid | آزمون توکن reset نامربوط؛ قابلیت ندارد |
| S14 | HTTPS only, HTTP redirects, HSTS header present (staged rollout; `includeSubDomains`/preload only after a subdomain inventory) **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S15 | All DB queries parameterized **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S16 | Server-side schema validation and writable-field allowlists/DTOs on every write endpoint (no mass assignment) | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| S17 | Debug off, no default credentials, DB user has least privilege **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| A1 | Generic error for wrong email/password; no account enumeration (login, sign-up, reset) | ❌ | خطای login در HTTP body/status یکسان؛ dummy hash؛ تست محلی | sign-up هنوز موفق/ناموفق قابل تفکیک؛ زمان پاسخ دقیق benchmark نشده |
| A2 | Rate limiting on login, sign-up, reset, OTP | ❌ | CreateAuthenticationLimiter سقف IP روی login/register/change-password | سقف per-account و storage مشترک چند replica لازم است |
| A3 | Lockout/backoff: 5 failures in 15 min → 15 min lock, stored server-side **[BLOCKER]** | ✅ | UserAuthService و AuthSecuritySmoke: قفل ۱۵ دقیقه، backoff ۳۰ و ورود پس از انقضا | شاهد محلی؛ همزمانی SQL Server و recovery/email پیش از انتشار لازم است |
| A4 | Breached-password check on sign-up/change/reset | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| A5 | Failed logins and password changes are logged (without secrets) and the user is notified | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| A6 | Cookies: `HttpOnly`, `Secure`, `SameSite` (`__Host-` prefix: `Path=/`, no `Domain`) **[BLOCKER]** | ✅ | پاسخ login تست دارای Secure/HttpOnly/SameSite=Lax | TLS termination در تست شبیه‌سازی شده؛ TLS واقعی میزبان تأیید نشده |
| A7 | Random server-validated session IDs; nothing sensitive in `localStorage` | ✅ | ITicketStore، کلید ۲۵۶ بیت، lookup hash و بررسی نسخه/نقش هر درخواست | مخزن SQL و DataProtection پایدار پس از migration لازم است |
| A8 | Session ID rotated on login and privilege change | ✅ | دو login مستقل کوکی متفاوت؛ تغییر نقش نشست قبلی را باطل می‌کند | اعتبارسنجی محلی |
| A9 | Idle timeout and absolute timeout enforced; sensitive actions need fresh re-authentication | ✅ | آزمون clock کنترل‌شده: بیکاری ۳۰ دقیقه، سقف ۱۲ ساعت با فعالیت مداوم؛ رمز فعلی برای تغییر رمز | سایر عملیات حساس باید جدا بررسی شوند |
| A10 | Logout deletes the session on the server, revokes refresh + remember-me tokens, kills access tokens, clears the cookie **[BLOCKER]** | ✅ | خروج HTTP و استفاده کوکی قبلی → 401؛ رکورد حذف؛ renewal آن را زنده نمی‌کند | شاهد local fixture؛ migration واقعی هنوز اجرا نشده |
| A11 | Logout is `POST` with CSRF protection; old cookie/tokens after logout return `401`; no cached private pages | ✅ | POST خروج بدون دفاع CSRF → 400؛ خروج معتبر → موفق؛ API no-store؛ کوکی قبلی 401 | back-button مرورگر واقعی هنوز آزمون نشده |
| A12 | "Log out all devices"; all sessions/tokens end after password change, reset, account deletion and disablement | ✅ | تست خروج همه دستگاه‌ها، تغییر رمز، تغییر نقش، disable/re-enable و delete | reset وجود ندارد؛ هنگام افزودن باید همین ابطال اعمال شود |
| A13 | Reset tokens: single-use, hashed, expiring (15–60 min) **[BLOCKER]** | ➖ | IUserAuthService و Account controllers مسیر reset ندارند | هنگام افزودن recovery اجرای این قانون ضروری است |
| A14 | Email verification; re-auth for sensitive actions | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| A15 | 2FA available; required for admin accounts | ❌ | Program.cs AddCookie بدون MFA | MFA مدیر و recovery امن |
| A16 | Server-side authorization on every request; CSRF protection on state-changing routes **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| A17 | If social login is used: PKCE S256 + state, exact redirect URIs; OIDC: nonce + ID token validated, user keyed on (iss, sub); no email-only account merging | ➖ | Program.cs فقط AddCookie؛ provider OAuth/OIDC ثبت نشده | در صورت افزودن ورود اجتماعی بررسی شود |
| A-T | The 8 tests listed in `auth.md` were run and passed | ❌ | آزمون‌های ورود/نشست اجرا شد؛ reset نامربوط است | benchmark timing و مرورگر/TLS واقعی و SQL Server runtime باقی است |
| D1 | `docs/security/data-inventory.md` exists and matches the current database/forms | ✅ | data-inventory.md؛ استخراج properties از ۱۳ مدل و بررسی فرم‌ها | بازبینی پس از هر قابلیت جدید |
| D2 | No field is collected without a reason | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| D4 | No full card number / CVV / PIN stored or logged; payments via provider's hosted page/SDK **[BLOCKER]** | ✅ | CustomerUser/Order/PaymentTransaction؛ PAN/CVV/PIN مدل نمی‌شود؛ درگاه غیرفعال | پرداخت واقعی فقط صفحه درگاه؛ داده موجود دیتابیس خوانده نشده |
| D5 | Payments verified on the server: signature + status + order/amount/currency/merchant match; fulfilment deduplicated | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| D6 | Authenticated TLS for all traffic, including DB/cache connections on private networks (certificate + hostname validated, no insecure fallback) | ❌ | appsettings connection TLS با bypass گواهی توسعه | اتصال production با گواهی معتبر، بدون bypass |
| D7 | Encryption at rest; sensitive fields encrypted; keys in a secret manager | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| D8 | APIs return only needed fields; users read only their own data; admin panel masks values | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| D9 | Logs, errors, and error trackers mask personal data; no PII in URLs **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| D10 | Automatic deletion jobs exist for every retention line | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| D11 | "Delete my account" works end to end (DB, files, third parties) and invalidates all sessions/tokens | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| D12 | Backups are encrypted, automatic, retention set, restore tested | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| D13 | No unnecessary personal data sent to AI/third parties | ❌ | فراخوانی LLM در کد دیده نشد؛ سرویس‌های عملیاتی میزبان تأیید نشده | فهرست ارائه‌دهندگان واقعی را مشخص کنید |
| D14 | No production data in dev/staging | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| D15 | Privacy policy matches reality | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| D16 | Incident plan exists (`docs/security/incident.md`) | ✅ | incident.md | تماس مسئول و تمرین پاسخ لازم است |
| H2 | `docs/security/attack-surface.md` is up to date for new routes/forms/uploads | ✅ | attack-surface.md؛ attributeهای مسیر و دیگر سطح‌ها | آزمون اجرایی تمام مسیرها باقی است |
| H3 | Every object route checks ownership (no IDOR); admin routes check role; no mass assignment **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| H4 | No injection (SQL/NoSQL/command) **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| H5 | No unsafe HTML rendering; CSP set | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| H6 | CSRF protection verified | ❌ | StoreWriteProtection؛ ۳۷ بررسی SecuritySmoke موفق | آزمون کامل فرم‌ها و API در فروشگاه/staging انجام نشده |
| H7 | URL-fetching features block internal addresses (SSRF) | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| H8 | Security headers set (CSP incl. `frame-ancestors`); CORS allowlist; BaaS rules (RLS etc.) tested as anonymous user **[BLOCKER]** | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| H9 | `npm audit` (or equivalent): 0 open Critical/High; closed only by a fix or by evidence of false positive / not reachable (documentation alone is not enough) **[BLOCKER]** | ❌ | NuGet audit نهایی: هر ۴ پروژه بدون بسته آسیب‌پذیر گزارش‌شده؛ XML 8.0.4 | JS قالب و وابستگی‌های بدون manifest هنوز audit کامل نشده‌اند |
| H10 | File uploads: type/size limits, random names, private storage | ❌ | AdminController، AccountApiController، AdminArticlesController | decode/re-encode، محدودیت ابعاد و ذخیره خصوصی پروفایل |
| H11 | Production errors show no stack traces or internals | ❌ | Program.cs production UseExceptionHandler و خطای عمومی | رفتار محیط production و همه actionها هنوز runtime بررسی نشده |
| H12 | Rate limits on expensive or abusable endpoints (AI, SMS, email) | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| H13 | Security events logged and alerts configured | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| H14 | Webhook signatures and payment details verified; lockfile committed; business-logic abuse checked | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| H15 | If the app uses an LLM: prompt injection considered, tools/data limited per user, output validated | ➖ | Program.cs و services: اتصال LLM وجود ندارد | در صورت افزودن LLM بررسی شود |
| H16 | Crypto and auth failures cross-checked against S11–S14 and A1–A17 | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| H17 | `docs/security/findings.md` has no open Critical/High **[BLOCKER]** | ❌ | F01/F02/F03/F05 در کد بسته شدند؛ بررسی عمومی و میزبان کامل نیست | باقی‌مانده‌ها در findings.md و auth-implementation.md |
| H20 | Unsafe deserialization and XXE checked (safe parsers, XML external entities disabled) | ❌ | تأیید کامل نشده؛ شواهد جزئی در findings.md و کد فعلی | بررسی کامل محلی یا تأیید/تنظیم میزبان لازم است |
| R1 | Code committed and tagged (rollback point exists) **[BLOCKER]** | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R2 | All production env vars are set on the host; none are missing or taken from `.env` in the repo | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R3 | Valid HTTPS certificate; HTTP redirects to HTTPS | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R4 | Database is not reachable from the public internet; only the app can connect | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R5 | Admin panel is protected (auth + 2FA, ideally IP-restricted); no default admin account | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R6 | Test accounts, seed data, and sample pages are removed from production | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R7 | Latest backup exists and a restore was tested recently | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R8 | Monitoring and alerts are on (errors, failed logins, cost spikes on paid APIs) | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R9 | Security headers verified on the LIVE URL (`curl -sI https://yourdomain`) | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R10 | Dependencies installed from the lockfile (`npm ci`), no dev-only packages in production | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R11 | `hack.md` audit re-run on everything changed since the last release | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |
| R12 | Rollback steps written down and tried | ❌ | انتشار/commit/tag و دسترسی میزبان انجام نشده | در مرحله انتشار توسط مالک بررسی شود |

## هشت آزمون auth کیت

| شماره | وضعیت | دلیل |
|---|---|---|
| 1 | ✅ | پنج خطا و رد رمز صحیح هنگام قفل؛ رفع قفل پس از زمان کنترل‌شده و backoff بعدی |
| 2 | ❌ | HTTP status/body یکسان تأیید شد؛ benchmark آماری timing انجام نشد |
| 3 | ✅ | خروج و استفاده دوباره کوکی قبلی روی API → 401 |
| 4 | ✅ | loginهای مستقل نشست تصادفی متفاوت دارند |
| 5 | ➖ | reset وجود ندارد |
| 6 | ✅ | flags در پاسخ login بررسی شد؛ TLS termination شبیه‌سازی شده است |
| 7 | ✅ | تغییر رمز نشست دستگاه دوم را باطل می‌کند |
| 8 | ✅ | خروج، تغییر رمز، خروج همه، disable/re-enable و delete نشست قبلی را رد می‌کنند؛ reset/refresh مستقل وجود ندارند |

## سازگاری و باقی‌مانده

ابزار API ابتدا GET /api/security/csrf بزند و کوکی را حفظ کند؛ برای write توکن برگشتی را در X-CSRF-TOKEN بفرستد؛ پس از ورود توکن تازه بگیرد. fetch مرورگر هم‌مبدأ Origin معتبر دارد؛ فرم‌های MVC توکن موجود دارند. ابزار قبلی بدون Origin/token اکنون ۴۰۰ می‌گیرد. آزمون کامل صفحات فروشگاه انجام نشده.

پشت proxy، known proxy و forwarded scheme/host باید صحیح تنظیم شوند؛ در غیر این صورت redirect HTTPS ممکن است loop شود. AllowedHosts production باید دامنه واقعی باشد. HSTS با ۳۰۰ ثانیه بدون includeSubDomains/preload آغاز می‌شود.

تأیید پیاده‌سازی محلی دریافت و انجام شد. مرحله باقی‌مانده: اجرای migration روی دیتابیس مقصد پس از backup و review؛ این مرحله صریحاً از تأیید پیاده‌سازی محلی جدا بود. همچنین MFA/recovery و حذف/مدت نگهداری داده هنوز تعیین نشده‌اند. جزئیات و SQL در auth-implementation.md و authentication-migration.sql.

AdminAccessSmoke اجرا نشد چون role کاربران موجود را در تراکنش تغییر می‌دهد. آزمون تمام مسیرها با دو کاربر، CSP کامل، parse تصویر، JS audit، backup restore، TLS میزبان، logging و monitoring باقی است. ❌ شامل شکست یا تأییدنشده است؛ ✅ فقط بر اساس شواهد مشخص آمده.
