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

        public int HirekSzama { get; set; }
        public int EsemenyekSzama { get; set; }
        public int GaleriaKepekSzama { get; set; }
    }
}
