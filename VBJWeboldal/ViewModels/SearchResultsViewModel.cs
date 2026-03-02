using System.Collections.Generic;
using VBJWeboldal.Models;

namespace VBJWeboldal.ViewModels
{
    public class SearchResultsViewModel
    {
        public string SearchQuery { get; set; }

        public bool SearchNews { get; set; } = true;
        public bool SearchEvents { get; set; } = true;
        public bool SearchDocuments { get; set; } = true;
        public bool SearchGalleries { get; set; } = true;
        public bool SearchUsers { get; set; } = true;

        public List<News> NewsResults { get; set; } = new List<News>();
        public List<Event> EventResults { get; set; } = new List<Event>();
        public List<Document> DocumentResults { get; set; } = new List<Document>();
        public List<Gallery> GalleryResults { get; set; } = new List<Gallery>();
        public List<ApplicationUser> UserResults { get; set; } = new List<ApplicationUser>();
    }
}