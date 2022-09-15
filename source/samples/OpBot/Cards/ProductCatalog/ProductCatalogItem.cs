namespace OpBot.Cards.ProductCatalog
{
    public class ProductCatalogItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string PictureUri { get; set; } = string.Empty;
    }
}
