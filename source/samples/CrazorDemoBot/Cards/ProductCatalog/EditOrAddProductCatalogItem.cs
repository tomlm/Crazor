using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CrazorDemoBot.Cards.ProductCatalog
{
    public class EditOrAddProductCatalogItem
    {
        public bool IsEdit { get; set; }

        public ProductCatalogItem? Item { get; set; }
    }
}
