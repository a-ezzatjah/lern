SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF (SELECT COUNT(*) FROM dbo.SiteBanners WHERE Id IN (1, 2, 3)) <> 3
    THROW 50001, N'Expected the three existing storefront banners.', 1;

UPDATE dbo.SiteBanners
SET LinkUrl = N'/shop?category=316'
WHERE Id = 1 AND ImageUrl = N'/assets/images/slider/snap-press-desktop-v2.png';

UPDATE dbo.SiteBanners
SET ImageUrl = N'/assets/images/slider/thread-desktop.png',
    MobileImageUrl = N'/assets/images/slider/thread-mobile.png',
    AltText = N'نخ خیاطی در خرازی کوهستانی',
    LinkUrl = N'/shop?category=2'
WHERE Id = 2 AND ImageUrl IN (N'/assets/images/slider/slider-2-2.jpg', N'/assets/images/slider/thread-desktop.png');

UPDATE dbo.SiteBanners
SET ImageUrl = N'/assets/images/slider/zip-desktop.png',
    MobileImageUrl = N'/assets/images/slider/zip-mobile.png',
    AltText = N'زیپ و متعلقات در خرازی کوهستانی',
    LinkUrl = N'/shop?category=72'
WHERE Id = 3 AND ImageUrl IN (N'/assets/images/slider/slider-2-3.jpg', N'/assets/images/slider/zip-desktop.png');

COMMIT TRANSACTION;
