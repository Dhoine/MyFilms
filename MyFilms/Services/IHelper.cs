using System.Collections.Generic;
using MyFilms.Data;
using MyFilms.Models;
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
        IEnumerable<FilmModel> CheckDbStateOfFilms(IEnumerable<FilmModel> films, ApplicationDbContext context, string userId);
        void SaveToDb(string id, ApplicationDbContext context, int type, bool arg, ApplicationUser user);
        void SaveToDb(string id, ApplicationDbContext context, int rate, ApplicationUser user);
    }
}