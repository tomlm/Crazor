using Crazor;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Newtonsoft.Json;

namespace OpBot.Cards.Address2
{
    public class Address 
    {
        public string? Id { get; set; } = Utils.GetNewId();
        //Supported Data Annotations 
        //  [PasswordPropertyText]
        //  [Range]
        //  [Phone]
        //  [Url]
        //  [Email]
        //  [StringLength]
        //  [PasswordPropertyText]
        //  [MaxLength]
        //  [RegularExpression("Ames", ErrorMessage = "The value must be Ames")]
        //  [DataType(DataType.Text)]
        //  [DataType(DataType.Email)]
        //  [DataType(DataType.Password)]
        //  [DataType(DataType.Tel)]
        //  [DataType(DataType.Url)]

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

        public Address Clone()
        {
            return (Address)this.DeepClone();
        }
    }
}
