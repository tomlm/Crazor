using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Crazor.Blazor.Tests.Validation
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
        [System.ComponentModel.Description("Your birthday")]
        [Range(typeof(DateTime), minimum: "01-01-1900", maximum: "01-01-2100", ErrorMessage = "Birthday has to be between 1900 and 2022")]
        public DateTime? Birthday { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Range(typeof(DateTime), minimum: "08:00", maximum: "20:00", ErrorMessage = "Arrival time must be between 8AM and 8PM")]
        public DateTime? ArrivalTime { get; set; }

        [Required]
        [System.ComponentModel.Description("Percentage")]
        [Range(minimum:0f, maximum: 100.0f, ErrorMessage = "Percentage must be between 0 and 100.")]
        public Double? Percent  { get; set; }

        [Required]
        [Range(minimum: 0, maximum: 100, ErrorMessage = "Attendees must be between 0 and 100.")]
        public int? Attendees { get; set; }

        [System.ComponentModel.Description("Cool")]
        [Required(ErrorMessage = "Cool is required")]
        public bool? IsCool{ get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Favorite Pet is required")]
        public Pets? FavoritePet { get; set; }
    }
}
