using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OpBot.Cards.BingSearch
{
    public class SearchModel
    {
        [BindProperty]
        [Required]
        [StringLength(100)]
        public string? Query { get; set; }

        [BindProperty]
        public int Offset { get; set; }
    }
}
