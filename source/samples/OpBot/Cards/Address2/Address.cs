using Crazor;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace OpBot.Cards.Address2
{
    public class Address
    {
        public string? Id { get; set; } = Utils.GetNewId();

        [BindProperty]
        [Required]
        [StringLength(100)]
        public string? Street { get; set; }

        [BindProperty]
        [Required]
        [StringLength(100)]
        public string? City { get; set; }

        [BindProperty]
        [Required]
        [StringLength(30)]
        public string? State { get; set; }

        [BindProperty]
        [Required]
        [MaxLength(20)]
        [DisplayName("Zip")]
        public string? PostalCode { get; set; }

        [BindProperty]
        [Required]
        [MaxLength(50)]
        public string? Country { get; set; }
    }
}
