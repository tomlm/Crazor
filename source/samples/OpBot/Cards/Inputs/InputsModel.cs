using Crazor.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpBot.Cards.Inputs
{
    public enum Pets
    {
        Dogs,
        Cats,
        Gerbils,
        Hamsters,
        Horses,
        Birds,
        Snakes,
        Other
    }

    public class InputsModel
    {
        [Required]
        [DataType(DataType.Date)]
        [Description("Your birthday")]
        [Range(typeof(DateTime), minimum: "01-01-1900", maximum: "01-01-2100", ErrorMessage = "Birthday has to be between 1900 and 2022")]
        public DateTime? Birthday { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Description("Arrival time")]
        [Range(typeof(DateTime), minimum: "08:00", maximum: "20:00", ErrorMessage = "Arrival time must be between 8AM and 8PM")]
        public DateTime? Arrival { get; set; }

        [Required]
        [Description("Percentage")]
        [Range(minimum:0, maximum: 100, ErrorMessage = "percentage must be between 0 and 100.")]
        public Double? Percent  { get; set; }

        [Required]
        [Description("Attendees")]
        [Range(minimum: 0, maximum: 100, ErrorMessage = "Attendees must be between 0 and 100.")]
        public int? Attendees { get; set; }

        [Description("Cool")]
        public bool IsCool{ get; set; }

        [Required]
        [Phone]
        [Description("Phone Number")]
        public string Phone{ get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Description("Password")]
        public string Password { get; set; }

        [Required]
        [Description("Favorite pet")]
        public Pets? FavoritePet { get; set; }
    }
}
