using System.Collections.Generic;
using MyFilms.Data;
using MyFilms.Models.ListsViewModels;

namespace MyFilms.Services
{
    public interface IHelper
    {
        IEnumerable<string> ParsePage(string url);
        FilmModel ParseFilmJson(string json);
        string GetFilmJson(string id);
        string GetSearchJson(string search);
        IEnumerable<FilmModel> ParseSearchJson(string json);
        IEnumerable<string> GetListFromDb(int type, ApplicationDbContext context,string userId);
    }
}