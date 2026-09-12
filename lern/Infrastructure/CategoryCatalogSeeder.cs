using Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace lern.Infrastructure;

public static class CategoryCatalogSeeder
{
    private sealed record CatalogNode(string Name, params CatalogNode[] Children);
    private static readonly Encoding Windows1252 = Encoding.GetEncoding(1252);
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

    public static async Task SeedAsync(ShopDbContext db)
    {
        var roots = new[]
        {
            new CatalogNode("نخ",
                new CatalogNode("دوک نخ خیاطی", new CatalogNode("دوک نخ خیاطی سفید و مشکی"), new CatalogNode("نخ پلی استر رنگی"), new CatalogNode("نخ لی رنگی")),
                new CatalogNode("ماسوره نخ خیاطی", new CatalogNode("ماسوره نخ ۴۰۰ یارد"), new CatalogNode("ماسوره نخ ۴۰۰ یاردی رنگی"), new CatalogNode("ماسوره نخ طلقی"), new CatalogNode("ماسوره نخ لی"), new CatalogNode("ماسوره نخ دریما"), new CatalogNode("قرقره نخ ۱۰"), new CatalogNode("ماسوره نخ نایلون")),
                new CatalogNode("نخ استرج", new CatalogNode("دوک نخ استرج مشکی و سفید"), new CatalogNode("نخ استرج رنگی"), new CatalogNode("ماسوره نخ استرج")),
                new CatalogNode("نخ کوک", new CatalogNode("نخ کوک")), new CatalogNode("نخ ملحفه و نخ لحاف", new CatalogNode("نخ ملحفه دوزی - نخ لحاف")), new CatalogNode("نخ آرایشی", new CatalogNode("نخ آرایشی")), new CatalogNode("نخ عمامه", new CatalogNode("نخ عمامه")), new CatalogNode("نخ کوبلن - نخ گلدوزی", new CatalogNode("نخ کوبلن و نخ گلدوزی")), new CatalogNode("نخ نایلون قلاب بافی", new CatalogNode("نخ قلاب بافی سفید"), new CatalogNode("نخ قلاب بافی رنگی")), new CatalogNode("نخ لیف", new CatalogNode("نخ لیف")), new CatalogNode("نخ سرکیسه دوزی", new CatalogNode("نخ سر کیسه دوزی")), new CatalogNode("نخ نامرئی", new CatalogNode("نخ نامرئی")), new CatalogNode("نخ تسبیح", new CatalogNode("نخ تسبیح")), new CatalogNode("نخ گلابتون", new CatalogNode("نخ گلابتون")), new CatalogNode("نخ مرسریزه - نخ گیپور بافی", new CatalogNode("نخ مرسریزه - گیپور بافی")), new CatalogNode("نخ کنفی و گونی", new CatalogNode("نخ و طناب کنفی و گونی")), new CatalogNode("نخ رافیا", new CatalogNode("نخ کاغذی رافیا"))),
            new CatalogNode("سوزن",
                new CatalogNode("سوزن چرخ خیاطی", new CatalogNode("سوزن اچ آ - معمولی"), new CatalogNode("سوزن دی بی - راستادوز"), new CatalogNode("سوزن دی سی - سردوز یا زیگزاگ"), new CatalogNode("سوزن دی وی - میاندوز"), new CatalogNode("سوزن دی پی - جادکمه"), new CatalogNode("سوزن ژرسه"), new CatalogNode("سوزن دو قلو")),
                new CatalogNode("سوزن دستی خیاطی", new CatalogNode("سوزن رز"), new CatalogNode("سوزن دختری"), new CatalogNode("سوزن رویال"), new CatalogNode("سوزن سری و ته طلایی"), new CatalogNode("سوزن ملحفه دوزی"), new CatalogNode("سوزن لحاف دوزی"), new CatalogNode("سوزن تشک دوزی"), new CatalogNode("سوزن جوالدوز")),
                new CatalogNode("سوزن هنرهای دستی", new CatalogNode("سوزن روبان دوزی"), new CatalogNode("سوزن پته دوزی"), new CatalogNode("سوزن گلدوزی"), new CatalogNode("سوزن کوبلن"), new CatalogNode("سوزن منجق دوزی"), new CatalogNode("سوزن گونی بافی")), new CatalogNode("سوزن تحریر - ته مروارید", new CatalogNode("سوزن تحریر"), new CatalogNode("سوزن ته مروارید")), new CatalogNode("سنجاق قفلی", new CatalogNode("سنجاق قفلی")), new CatalogNode("سوزن نخ کن", new CatalogNode("سوزن نخ کن")), new CatalogNode("پونز", new CatalogNode("پونز"))),
            new CatalogNode("زیپ و متعلقات",
                new CatalogNode("زیپ ۲۰ سانت", new CatalogNode("زیپ دنده ۴ مشکی - سفید"), new CatalogNode("زیپ دنده ۳ مشکی و سفید"), new CatalogNode("زیپ دنده ۳ رنگی"), new CatalogNode("زیپ دنده ۴ رنگی"), new CatalogNode("زیپ دنده ۵ و دنده ۷")), new CatalogNode("زیپ نامرئی - مخفی", new CatalogNode("زیپ مخفی سفید و مشکی"), new CatalogNode("زیپ مخفی ۲۰ سانت رنگی"), new CatalogNode("زیپ مخفی ۵۰ و ۶۰ سانت دنده ۳"), new CatalogNode("زیپ مخفی ۲۰ سانت کتان"), new CatalogNode("زیپ مخفی ۵۰ سانت کتان")), new CatalogNode("زیپ لباسی", new CatalogNode("زیپ لباسی سفید و مشکی"), new CatalogNode("زیپ لباسی ۵۰ سانت رنگی")), new CatalogNode("زیپ دندانه فلزی", new CatalogNode("زیپ دندانه فلز کاپشنی"), new CatalogNode("زیپ دندانه فلز شلواری"), new CatalogNode("زیپ طرح دندانه فلزی")), new CatalogNode("زیپ دندانه لاکی", new CatalogNode("زیپ کاپشنی مشکی و سفید"), new CatalogNode("زیپ کاپشنی رنگی"), new CatalogNode("زیپ کاپشنی سیلور")), new CatalogNode("زیپ دندانه استخوانی", new CatalogNode("زیپ استخوانی مشکی و سفید"), new CatalogNode("زیپ کاپشنی استخوانی رنگی")), new CatalogNode("زیپ طاقه ای - متری - یکسره", new CatalogNode("زیپ طاقه ای متری مشکی - سفید"), new CatalogNode("زیپ طاقه ای متری رنگی")), new CatalogNode("ماشین زیپ - سری زیپ", new CatalogNode("سری زیپ پلاستیکی"), new CatalogNode("سری زیپ فلزی و کاپشنی"), new CatalogNode("سری زیپ استخوانی")), new CatalogNode("زیپ چسب", new CatalogNode("زیپ چسب باریک"), new CatalogNode("زیپ چسب پهن"))),
            new CatalogNode("قیچی و لوازم برش", new CatalogNode("قیچی آرایشی", new CatalogNode("قیچی آرایشی")), new CatalogNode("قیچی خیاطی", new CatalogNode("قیچی خیاطی لاکی"), new CatalogNode("قیچی ساووئی"), new CatalogNode("قیچی سینگر")), new CatalogNode("قیچی فاف", new CatalogNode("سایر قیچی های خیاطی لاکی"), new CatalogNode("سایر قیچی های خیاطی فلزی"), new CatalogNode("دسته قیچی")), new CatalogNode("قیچی دالبر", new CatalogNode("قیچی دالبر")), new CatalogNode("قیچی سرکج - سر صاف", new CatalogNode("قیچی سرکج و سر صاف و سر گرد")), new CatalogNode("قیچی سرنخ زن", new CatalogNode("قیچی سرنخ زن")), new CatalogNode("کاتر - تیز بر - موکت بر", new CatalogNode("کاتر - تیز بر")), new CatalogNode("قیچی کاغذ", new CatalogNode("قیچی کاغذ")), new CatalogNode("قیچی آشپزخانه و باغبانی", new CatalogNode("قیچی آشپزخانه و باغبانی")), new CatalogNode("قیچی تاشو")),
            new CatalogNode("لوازم خیاطی", new CatalogNode("محافظ کفی اتو - ابزار اتوکاری", new CatalogNode("اتو"), new CatalogNode("محافظ کفی اتو - آهن اتو"), new CatalogNode("میز اتو - پلاستیک و ژلاتین اتوکاری")), new CatalogNode("گونیا - خط کش", new CatalogNode("خط کش و شابلون خیاطی"), new CatalogNode("گونیا و پیستوله")), new CatalogNode("جعبه خیاطی", new CatalogNode("جعبه های خیاطی")), new CatalogNode("فن", new CatalogNode("فن دامنی"), new CatalogNode("فن شلواری - چهار تکه")), new CatalogNode("صابون", new CatalogNode("صابون و خودکار خیاطی")), new CatalogNode("سوزن خیاطی دستی", new CatalogNode("سوزن خیاطی دستی")), new CatalogNode("رولت خیاطی", new CatalogNode("رولت خیاطی")), new CatalogNode("کارگاه گلدوزی", new CatalogNode("کارگاه گلدوزی لاک و پلاستیکی"), new CatalogNode("کارگاه گلدوزی چوبی")), new CatalogNode("چوب کار", new CatalogNode("چوب کار - چوب لباسی")), new CatalogNode("دستگاه های اتیکت زن", new CatalogNode("تفنگ کارت زن"), new CatalogNode("تیبر و سوزن تفنگ کارت زن"), new CatalogNode("نخ پلیپ")), new CatalogNode("فنر و ژئون لباس عروس", new CatalogNode("فنر لباس و ژئون لباس عروس")), new CatalogNode("دستگاه های خیاطی", new CatalogNode("دستگاه های خیاطی")), new CatalogNode("ملزومات مغازه خیاطی", new CatalogNode("ترازو دیجیتال"), new CatalogNode("ملزومات مغازه خیاطی")), new CatalogNode("کلف لگو - کلکین - کلف رنگی", new CatalogNode("کلف برش"), new CatalogNode("کلف و مغفرا رنگی")), new CatalogNode("ابر - پشم شیشه", new CatalogNode("ابر"), new CatalogNode("پشم و شیشه - الیاف"))),
            new CatalogNode("لوازم چرخ خیاطی", new CatalogNode("پایه چرخ خیاطی", new CatalogNode("پایه چرخ معمولی"), new CatalogNode("پایه چرخ راستادوز"), new CatalogNode("پایه چین - پایه پلیسه"), new CatalogNode("پایه زیپ"), new CatalogNode("پایه لول"), new CatalogNode("پایه جادکمه"), new CatalogNode("پایه فودر"), new CatalogNode("پایه دوک")), new CatalogNode("ماکو چرخ خیاطی", new CatalogNode("ماکو چرخ معمولی"), new CatalogNode("ماکو چرخ راستادوز")), new CatalogNode("ماسوره چرخ خیاطی", new CatalogNode("ماسوره چرخ معمولی"), new CatalogNode("ماسوره چرخ راستادوز")), new CatalogNode("تسمه چرخ خیاطی", new CatalogNode("تسمه چرخ خیاطی")), new CatalogNode("روغن چرخ خیاطی", new CatalogNode("روغن چرخ خیاطی")), new CatalogNode("موتور چرخ خیاطی", new CatalogNode("موتور چرخ خیاطی")), new CatalogNode("لامپ چرخ خیاطی", new CatalogNode("لامپ چرخ خیاطی")), new CatalogNode("بیخ گوشتی چرخ خیاطی", new CatalogNode("بیخ گوشتی چرخ خیاطی")), new CatalogNode("پنس", new CatalogNode("پنس"))),
            new CatalogNode("مروارید - سرمه - منجق - تکه دوزی", new CatalogNode("مروارید", new CatalogNode("مروارید شماره ۳"), new CatalogNode("مروارید شماره ۴"), new CatalogNode("مروارید شماره ۵"), new CatalogNode("مروارید شماره ۶"), new CatalogNode("مروارید شماره ۸"), new CatalogNode("مروارید شماره ۱۰"), new CatalogNode("مروارید اشک"), new CatalogNode("مروارید گندمی"), new CatalogNode("مروارید پانچی"), new CatalogNode("مروارید رنگی و سنگی")), new CatalogNode("منجوق - مبله - پولک", new CatalogNode("منجوق گرد"), new CatalogNode("منجوق گرد درشت"), new CatalogNode("مبله ساده"), new CatalogNode("مبله بیج"), new CatalogNode("مبله شکسته"), new CatalogNode("پولک گرد"), new CatalogNode("پولک ریبانی و سکه ای"), new CatalogNode("پولک ستاره"), new CatalogNode("پولک برگی"), new CatalogNode("پولک طرح دار تزئینی")), new CatalogNode("گل های دوختنی و تزئینی", new CatalogNode("گل پفکی"), new CatalogNode("گل شکوفه"), new CatalogNode("گل و پاپیون تزئینی"), new CatalogNode("شکوفه نگین دار")), new CatalogNode("تکه دوزی و سرزانو و آرم", new CatalogNode("سرزانو - سرآنجی"), new CatalogNode("یقه آماده"), new CatalogNode("تکه دوزی عروسی"), new CatalogNode("تکه دوزی تزئینی"), new CatalogNode("تکه دوزی آرم و مارک"), new CatalogNode("آستین چادر ملی"), new CatalogNode("ساز لباس"), new CatalogNode("برچسب حرارتی و چاپی لباس"))),
            new CatalogNode("لوازم لباس زیر", new CatalogNode("کاپ لباس زیر", new CatalogNode("کاپ لباس و لباس زیر مشکی"), new CatalogNode("کاپ لباس و لباس زیر سفید"), new CatalogNode("کاپ لباس و لباس زیر کرم")), new CatalogNode("کش لباس زیر", new CatalogNode("کش رکاب لباس زیر"), new CatalogNode("کش لبه لباس زیر"), new CatalogNode("کش پهن لباس زیر")), new CatalogNode("بند نامرئی", new CatalogNode("بند نامرئی")), new CatalogNode("فنر لباس زیر", new CatalogNode("فنر لباس زیر")), new CatalogNode("قزن لباس زیر", new CatalogNode("قزن متری لباس زیر"), new CatalogNode("قزن - رابط سوتین مشکی"), new CatalogNode("قزن - رابط سوتین رنگی"), new CatalogNode("قزن - رابط سوتین سفید")), new CatalogNode("بالابر لباس زیر", new CatalogNode("بالابر لباس زیر"))),
            new CatalogNode("لوازم پرده", new CatalogNode("نوار پرده", new CatalogNode("نوار پرده")), new CatalogNode("گیره و سرب پرده", new CatalogNode("گیره و بست پرده"), new CatalogNode("سرب پرده"))),
            new CatalogNode("کاموا و لوازم بافتنی", new CatalogNode("کاموا", new CatalogNode("کاموا خزدار یوموش"), new CatalogNode("کاموا ظریف"), new CatalogNode("کاموا ضخیم"), new CatalogNode("کاموا سوزنی و زری دار"), new CatalogNode("کاموا مخمل")), new CatalogNode("میل بافتنی", new CatalogNode("میل گرد"), new CatalogNode("میل تفلون بلند"), new CatalogNode("میل بلند فلزی")), new CatalogNode("قلاب بافتنی", new CatalogNode("قلاب ژاپن"), new CatalogNode("قلاب طرح"), new CatalogNode("قلاب فلزی"), new CatalogNode("قلاب دسته پلاستیک")), new CatalogNode("دستگاه های بافت", new CatalogNode("گلساز - موتیف - منگوله ساز"), new CatalogNode("سوزن پلاستیکی"), new CatalogNode("دانه گیر"))),
            new CatalogNode("یراق - نوار - روبان - مغزی", new CatalogNode("روبان", new CatalogNode("روبان زری"), new CatalogNode("روبان ۲ میل ساتن"), new CatalogNode("روبان ۷ ساتن"), new CatalogNode("روبان ۱۳ ساتن"), new CatalogNode("روبان ۲۵ ساتن"), new CatalogNode("روبان ۴۰ ساتن"), new CatalogNode("روبان ۷ حریر"), new CatalogNode("روبان ۲۵ حریر"), new CatalogNode("روبان ۴۰ حریر"), new CatalogNode("روبان ۱۳ خالدار"), new CatalogNode("روبان ۲۵ خالدار"), new CatalogNode("روبان گروگرین"), new CatalogNode("روبان طرح دار")), new CatalogNode("ریشه", new CatalogNode("ریشه و نوار مبلی"), new CatalogNode("ریشه پرچم")), new CatalogNode("یراق", new CatalogNode("نوار قلاب بافی"), new CatalogNode("یراق تاجی"), new CatalogNode("یراق ریشه دار"), new CatalogNode("یراق تزئینی")), new CatalogNode("نوار مغزی", new CatalogNode("نوار مغزی ساتن و کتان"), new CatalogNode("نوار مغزی تزئینی")), new CatalogNode("نوار اریب - گان", new CatalogNode("نوار گان"), new CatalogNode("نوار اریب بشور ۱ سانت"), new CatalogNode("نوار اریب بشور ۲ سانت"), new CatalogNode("نوار اریب نخی و کتان"), new CatalogNode("نوار اریب ساتن"), new CatalogNode("نوار اریب کشی")), new CatalogNode("نوار تزئینی - جشن تکلیف", new CatalogNode("نوار پلیسه"), new CatalogNode("نوار لوزی"), new CatalogNode("نوار گل شکوفه"), new CatalogNode("نوار گل تاجی - نوار گنبدی")), new CatalogNode("نوار زیگزاک", new CatalogNode("نوار زیگزاک"))),
            new CatalogNode("قیطان", new CatalogNode("قیطان تزئینی", new CatalogNode("قیطان شیاردار")), new CatalogNode("نخ مکرومه", new CatalogNode("نخ مکرومه سوزنی سفید و مشکی"), new CatalogNode("نخ مکرومه سوزنی رنگی"), new CatalogNode("نخ مکرومه کتان")), new CatalogNode("قیطان ماکارانی", new CatalogNode("قیطان ماکارانی")), new CatalogNode("قیطان پفکی - قیطان مات", new CatalogNode("قیطان مات"))),
            new CatalogNode("دکمه پرسی و متعلقات", new CatalogNode("دستگاه پرس دکمه", new CatalogNode("دستگاه پرس دکمه"), new CatalogNode("دستگاه سوراخ کن - پانچ")), new CatalogNode("پولک دکمه پرسی", new CatalogNode("پولک دکمه پرسی فلزی"), new CatalogNode("پولک دکمه پرسی پلاستیکی"), new CatalogNode("حلقه پولک دور دکمه پرسی")), new CatalogNode("قالب دکمه پرسی", new CatalogNode("قالب برنجی دکمه پرسی"), new CatalogNode("قالب چهارپارچه"), new CatalogNode("قالب دکمه حلقه"), new CatalogNode("قالب دکمه لی"), new CatalogNode("قالب دکمه نما"), new CatalogNode("قالب منگنه")), new CatalogNode("دکمه چهارپارچه", new CatalogNode("چهارپارچه ۳"), new CatalogNode("چهارپارچه میانه"), new CatalogNode("چهارپارچه آلفا")), new CatalogNode("دکمه حلقه ای", new CatalogNode("دکمه حلقه ای")), new CatalogNode("دکمه لی و متعلقات", new CatalogNode("دکمه لی و متعلقات")), new CatalogNode("حلقه منگنه پرسی و متعلقات", new CatalogNode("حلقه منگنه"), new CatalogNode("دکمه نما"))),
            new CatalogNode("دکمه", new CatalogNode("دکمه ریلی", new CatalogNode("دکمه ریلی")), new CatalogNode("دکمه فشاری", new CatalogNode("دکمه فشاری فلز مشکی"), new CatalogNode("دکمه فشاری فلز نقره ای"), new CatalogNode("دکمه فشاری پلاستیک سفید")), new CatalogNode("دکمه پیراهنی", new CatalogNode("دکمه پیراهنی صدف"), new CatalogNode("دکمه پیراهنی طرح فلز"), new CatalogNode("دکمه پیراهنی موادی"), new CatalogNode("دکمه پیراهنی طرح دار")), new CatalogNode("دکمه روپوشی", new CatalogNode("دکمه روپوشی موادی"), new CatalogNode("دکمه روپوشی صدف")), new CatalogNode("دکمه مبلی", new CatalogNode("دکمه مبلی لبخندی و ساده")), new CatalogNode("دکمه کت و شلواری", new CatalogNode("دکمه شلواری موادی"), new CatalogNode("دکمه شلواری صدف"), new CatalogNode("دکمه کت"), new CatalogNode("دکمه شلواری طرح فلز")), new CatalogNode("دکمه نوزادی - دکمه عروسکی", new CatalogNode("دکمه نوزادی - عروسکی"))),
            new CatalogNode("دکمه مانتویی - اسپرت - پالتویی", new CatalogNode("دکمه شومیزی", new CatalogNode("دکمه شومیزی")), new CatalogNode("۲ یا ۴ سوراخ", new CatalogNode("۲ یا ۴ سوراخ تک سایز"), new CatalogNode("۲ یا ۴ سوراخ دو سایز"), new CatalogNode("۲ یا ۴ سوراخ سه سایز"), new CatalogNode("۲ یا ۴ سوراخ چهار سایز"), new CatalogNode("۲ یا ۴ سوراخ چوبی")), new CatalogNode("پایه دار - پشت تونلی", new CatalogNode("پایه دار - پشت تونلی تک سایز"), new CatalogNode("پایه دار - پشت تونلی دو سایز"), new CatalogNode("پایه دار - پشت تونلی سه سایز")), new CatalogNode("پالتویی", new CatalogNode("دکمه پالتویی")), new CatalogNode("دکمه دست دوز", new CatalogNode("دکمه گلدوزی دست دوز")))
        };

        // Normalize legacy UTF-8/Windows-1252 mojibake in the catalog before
        // inserting, so the database and mega menu receive Persian text.
        roots = roots.Select(NormalizeCatalogNode).ToArray();

        // Some categories were previously inserted after a UTF-8/Windows-1252
        // decoding mistake. Normalize those names before matching the catalog
        // so the mega menu and admin use the same Persian values everywhere.
        var allCategories = await db.Categories.ToListAsync();
        var repaired = false;
        foreach (var category in allCategories)
        {
            if (!LooksLikeMojibake(category.Name))
                continue;

            var decoded = NormalizeText(category.Name);
            if (!decoded.Contains('\uFFFD'))
            {
                category.Name = decoded;
                repaired = true;
            }
        }

        if (repaired)
            await db.SaveChangesAsync();

        // A category name can legitimately occur in separate branches. Match
        // catalog entries by their parent and name, not by name globally.
        var existing = allCategories
            .GroupBy(x => (x.ParentId, x.Name))
            .ToDictionary(group => group.Key, group => group.OrderBy(x => x.Id).First());
        var position = 0;
        foreach (var root in roots)
            await AddNodeAsync(root, null);

        async Task AddNodeAsync(CatalogNode node, int? parentId)
        {
            if (!existing.TryGetValue((parentId, node.Name), out var category))
            {
                // Older databases may have a global unique index on Name.
                // Reuse an existing same-name row when that index is present,
                // then update its parent/sort instead of inserting a duplicate.
                category = allCategories.FirstOrDefault(x => x.Name == node.Name);
                if (category is null)
                {
                    category = new Category { Name = node.Name, Slug = $"catalog-{++position:D4}", ParentId = parentId, SortOrder = position };
                    db.Categories.Add(category);
                    await db.SaveChangesAsync();
                    allCategories.Add(category);
                }
                else if (category.ParentId != parentId)
                {
                    category.ParentId = parentId;
                    category.SortOrder = position;
                    await db.SaveChangesAsync();
                }
                existing[(parentId, node.Name)] = category;
            }
            foreach (var child in node.Children)
                await AddNodeAsync(child, category.Id);
        }

        static bool LooksLikeMojibake(string value) =>
            value.IndexOfAny(['\u00D8', '\u00D9', '\u00DA', '\u00DB']) >= 0;

        static string NormalizeText(string value)
        {
            for (var attempt = 0; attempt < 3 && LooksLikeMojibake(value); attempt++)
            {
                try
                {
                    var decoded = StrictUtf8.GetString(Windows1252.GetBytes(value));
                    if (decoded == value)
                        break;

                    value = decoded;
                }
                catch (DecoderFallbackException)
                {
                    break;
                }
            }

            return value;
        }

        static CatalogNode NormalizeCatalogNode(CatalogNode node) =>
            new(NormalizeText(node.Name), node.Children.Select(NormalizeCatalogNode).ToArray());
    }
}
