using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OpBot.Cards.ProductCatalog
{
    public class EditOrAddProductCatalogItem
    {
        public bool IsEdit { get; set; }

        public ProductCatalogItem? Item { get; set; }
    }
}
