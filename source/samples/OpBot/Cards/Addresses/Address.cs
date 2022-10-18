using Crazor;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace OpBot.Cards.Addresses
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
        public States State { get; set; }

        [Required]
        [MaxLength(20)]
        [DisplayName("Zip")]
        [RegularExpression(@"\d{5}([ \-]\d{4})?", ErrorMessage ="The Zip must match pattern #####-#### or #####.")]
        public string? PostalCode { get; set; }

        [Required]
        public Countries Country { get; set; }
    }
}