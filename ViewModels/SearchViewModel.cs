namespace EXE.ViewModels
{
    public class SearchViewModel
    {
        public string FullName { get; set; }
        public decimal? MinHourlyRate { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        public int? MinExperience { get; set; }
        public int? MaxExperience { get; set; }
        public string Location { get; set; }
        public string Language {  get; set; }
        public string Type { get; set; }
    }
}
