using System.Collections.Generic;
using MyFilms.Models.ListsViewModels;

namespace MyFilms.Services
{
    public interface IHelper
    {
        IEnumerable<string> ParsePage(string url);
        FilmModel ParseFilmJson(string json);
        string GetFilmJson(string id);
    }
}