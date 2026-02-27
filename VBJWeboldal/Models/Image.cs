namespace VBJWeboldal.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int GalleryId { get; set; }
        public Gallery Gallery { get; set; }
    }
}