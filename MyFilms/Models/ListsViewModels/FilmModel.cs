namespace MyFilms.Models.ListsViewModels
{
    public class FilmModel
    {
        public string Caption { get; set; }
        public string Directors { get; set; }
        public string Genres { get; set; }
        public string ImdbRating { get; set; }
        public string Name { get; set; }
        public string PosterLink { get; set; }
        public string Stars { get; set; }
        public string Writers { get; set; }
        public string Year { get; set; }
        public string Id { get; set; }
        public bool InFavourites { get; set; } = false;
        public bool InHistory { get; set; } = false;
        public int UserRating { get; set; } = 0;
    }
}