using System.Collections.Generic;
using VBJWeboldal.Models;

namespace VBJWeboldal.ViewModels
{
    public class HomeViewModel
    {
        public List<News> FrissHirek { get; set; }
        public List<Event> KozelgoEsemeny { get; set; }
        public List<Gallery> FrissGaleriaKep { get; set; }
        public List<Document> FrissDokumentumok { get; set; }
    }
}
