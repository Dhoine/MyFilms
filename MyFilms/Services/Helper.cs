using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MyFilms.Data;
using MyFilms.Models;
using MyFilms.Models.ListsViewModels;
using Newtonsoft.Json.Linq;

namespace MyFilms.Services
{
    public class Helper : IHelper
    {
        private const string GetByIdUrl = "https://theimdbapi.org/api/movie?movie_id=";
        private const string SearchUrl = "http://www.theimdbapi.org/api/find/movie?title={title}";

        public IEnumerable<string> GetListFromDb(int type, ApplicationDbContext context, string userId)
        {
            var list = type == 0
                ? context.UserFilms.Where(e => e.User.Id == userId && e.InFavourites).ToList()
                : context.UserFilms.Where(e => e.User.Id == userId && e.InHistory).ToList();
            return list.Select(film => film.Id).ToList();
        }

        public void SaveToDb(string id, ApplicationDbContext context, int type, bool arg, ApplicationUser user)
        {
            FilmDbModel dbFilm;
            var found = false;
            try
            {
                dbFilm = context.UserFilms.Single(e => e.Id == id && e.User.Id == user.Id);
                found = true;
            }
            catch (Exception)
            {
                dbFilm = new FilmDbModel {User = user, Id = id};
            }

            switch (type)
            {
                case 0:
                    dbFilm.InFavourites = true;
                    break;
                case 1:
                    dbFilm.InHistory = true;
                    break;
                case 2:
                    dbFilm.InFavourites = false;
                    break;
                case 3:
                    dbFilm.InHistory = false;
                    break;
                default:
                    return;
            }
            if (!found) context.UserFilms.Add(dbFilm);
            context.SaveChanges();
        }

        public void SaveToDb(string id, ApplicationDbContext context, int rate, ApplicationUser user)
        {
            FilmDbModel dbFilm;
            var found = false;
            try
            {
                dbFilm = context.UserFilms.Single(e => e.Id == id && e.User.Id == user.Id);
                found = true;
            }
            catch (Exception)
            {
                dbFilm = new FilmDbModel {User = user, Id = id};
            }
            dbFilm.UserRating = rate;
            if (!found) context.UserFilms.Add(dbFilm);
            context.SaveChanges();
        }

        public IEnumerable<string> ParsePage(string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var ids = Regex.Matches(doc.ParsedText, "(?<=data-titleid=\")tt(\\d+?)(?=\">)").Select(m => m.Value);
            var list = ids as IList<string> ?? ids.ToList();
            if (!list.Any())
                list = Regex.Matches(doc.ParsedText, "(?<=data-tconst=\")tt(\\d+?)(?=\">)").Select(m => m.Value)
                    .ToList();
            if (!list.Any())
                list = Regex.Matches(doc.ParsedText, "(?<=data-tconst=\")tt(\\d+?)(?=\" )").Select(m => m.Value)
                    .ToList();
            return list;
        }

        public FilmModel ParseFilmJson(string json)
        {
            var o = JObject.Parse(json);
            return CreateFilmModelFromJObject(o);
        }

        public string GetFilmJson(string id)
        {
            var requestUrl = GetByIdUrl + id;
            return DownloadJson(requestUrl);
        }

        public string GetSearchJson(string search)
        {
            var requestUrl = SearchUrl.Replace("{title}", search);
            return DownloadJson(requestUrl);
        }

        public IEnumerable<FilmModel> ParseSearchJson(string json)
        {
            var jarr = JArray.Parse(json);
            return jarr.Select(CreateFilmModelFromJObject).ToList();
        }

        public IEnumerable<FilmModel> CheckDbStateOfFilms(IEnumerable<FilmModel> films, ApplicationDbContext context,
            string userId)
        {
            var res = new List<FilmModel>();
            foreach (var film in films)
            {
                try
                {
                    var dbFilm = context.UserFilms.Single(e => e.Id == film.Id && e.User.Id == userId);
                    film.InFavourites = dbFilm.InFavourites;
                    film.InHistory = dbFilm.InHistory;
                    film.UserRating = dbFilm.UserRating;
                }
                catch (Exception)
                {
                    //skip then
                }
                res.Add(film);
            }
            return res.ToArray();
        }

        public void SaveToDb(string id, ApplicationDbContext context, int rate, string userId)
        {
            FilmDbModel dbFilm;
            var found = false;
            try
            {
                dbFilm = context.UserFilms.Single(e => e.Id == id && e.User.Id == userId);
                found = true;
            }
            catch (Exception)
            {
                dbFilm = new FilmDbModel();
            }
            dbFilm.UserRating = rate;
            if (!found) context.UserFilms.Add(dbFilm);
            context.SaveChanges();
        }

        private static FilmModel CreateFilmModelFromJObject(JToken o)
        {
            var model = new FilmModel
            {
                Caption = (string) o.SelectToken("description"),
                Directors = (string) o.SelectToken("director"),
                ImdbRating = (string) o.SelectToken("rating"),
                Name = (string) o.SelectToken("title"),
                PosterLink = (string) o.SelectToken("poster.thumb"),
                Year = (string) o.SelectToken("year"),
                Id = (string) o.SelectToken("imdb_id")
            };
            var genreArr = o.SelectToken("genre");
            foreach (var genre in genreArr)
                model.Genres += " | " + (string) genre;
            model.Genres += " |";

            var starsArr = o.SelectToken("stars");
            foreach (var star in starsArr)
                model.Stars += " | " + (string) star;
            model.Stars += " |";
            var writersArr = o.SelectToken("writers");
            foreach (var writer in writersArr)
                model.Writers += " | " + (string) writer;
            model.Writers += " |";
            return model;
        }

        private static string DownloadJson(string url)
        {
            string json;
            using (var wc = new WebClient())
            {
                json = wc.DownloadString(url);
            }
            return json;
        }
    }
}