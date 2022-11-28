using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CrazorDemoBot.Cards.ProductCatalog
{
    public class ProductCatalogItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        [Url]
        [DisplayName("Picturl Url")]
        public string? PictureUri { get; set; }
    }
}
