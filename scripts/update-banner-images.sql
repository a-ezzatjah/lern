-- Repoint home-slider banners from the old PNG art to the optimized JPEGs.
-- Safe to run more than once.
UPDATE dbo.SiteBanners
SET ImageUrl = REPLACE(ImageUrl, '.png', '.jpg')
WHERE ImageUrl LIKE '/assets/images/slider/%.png';

UPDATE dbo.SiteBanners
SET MobileImageUrl = REPLACE(MobileImageUrl, '.png', '.jpg')
WHERE MobileImageUrl LIKE '/assets/images/slider/%.png';
