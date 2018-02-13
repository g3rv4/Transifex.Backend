namespace Transifex.Backend.ViewModels.Home
{
    public class QueryRequestViewModel
    {
        public string StringRegex { get; set; }
        public string TranslationRegex { get; set; }
        public bool? IsReviewed { get; set; }
        public bool? WithNonReviewedSuggestions { get; set; }
        public string[] OnlyTranslationsFromUsers { get; set; }
        public string[] OnlySuggestionsFromUsers { get; set; }
    }
}