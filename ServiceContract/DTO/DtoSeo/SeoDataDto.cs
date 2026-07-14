namespace ServiceContract.DTO.DtoSeo
{
    public class SeoDataDto
    {
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }

        public string? CanonicalUrl { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImageUrl { get; set; }

        public bool IndexPage { get; set; } = true;
        public bool FollowPage { get; set; } = true;
    }
}
