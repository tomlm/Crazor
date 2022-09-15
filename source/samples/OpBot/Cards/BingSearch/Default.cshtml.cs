using Microsoft.Bing.WebSearch.Models;
using Microsoft.Bing.WebSearch;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OpBot.Cards.BingSearch
{
    public class DefaultModel
    {
        public int Offset { get; set; }

        public IList<WebPage> Results { get; set; } = new List<WebPage>();

        public bool HasNext => Results.Any(); 

        public bool HasPrev => Results.Any() && Offset > 0;
    }
}
