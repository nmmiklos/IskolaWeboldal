namespace VBJWeboldal.ViewModels
{
    public class LessonViewModel
    {
        public string Day { get; set; }
        public int Period { get; set; } // Hanyadik óra
        public string ClassName { get; set; }
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public string Room { get; set; }
        public string Group { get; set; }
    }

    public class TimetableResultViewModel
    {
        public string SelectedValue { get; set; }
        public string HomeroomTeacher { get; set; } // Csak ha osztályt választunk
        public List<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();
    }
}