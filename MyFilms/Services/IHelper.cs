using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyFilms.Models.ListsViewModels;

namespace MyFilms.Services
{
    public interface IHelper
    {
        IEnumerable<string> ParsePage(string url);
        FilmModel ParseFilmJson(string json);
        string GetFilmJson(string title, string year);
        string GetFilmJson(string id);
    }
}
