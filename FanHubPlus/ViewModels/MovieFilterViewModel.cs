using System.Collections.Generic;
using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    public class MovieFilterViewModel
    {
        public string? Genre { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }          // rating | title | year
        public List<VideoItem> Movies { get; set; } = new();
        public List<string> Genres { get; set; } = new();
    }
}
