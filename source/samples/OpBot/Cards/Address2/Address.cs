using Crazor;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace OpBot.Cards.Address2
{
    public class Address
    {
        public string? Id { get; set; } = Utils.GetNewId();

        [Required]
        [StringLength(100)]
        public string? Street { get; set; }

        [Required]
        [StringLength(100)]
        public string? City { get; set; }

        [Required]
        [StringLength(30)]
        public string? State { get; set; }

        [Required]
        [MaxLength(20)]
        [DisplayName("Zip")]
        public string? PostalCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Country { get; set; }
    }
}
