namespace OpBot.Cards.ProductCatalog
{
    public class CreateEditProductCatalogModel
    {
        public bool IsEdit { get; set; }
        public ProductCatalogItem Item { get; set; } = new ProductCatalogItem();
    }
}