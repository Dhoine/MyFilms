using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MyFilms.Models.ListsViewModels;
using Newtonsoft.Json.Linq;

namespace MyFilms.Services
{
    public class Helper : IHelper
    {
        private const string GetByIdUrl = "https://theimdbapi.org/api/movie?movie_id=";
        private const string GetByTitleUrl = "http://www.theimdbapi.org/api/find/movie?title={title}&year={year}";

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
            var model = new FilmModel();
            var o = JObject.Parse(json);
            model.Caption = (string) o.SelectToken("description");
            model.Directors = (string) o.SelectToken("director");
            var genreArr = o.SelectToken("genre");
            foreach (var genre in genreArr)
                model.Genres += " | " + (string) genre;
            model.Genres += " |";
            model.ImdbRating = (string) o.SelectToken("rating");
            model.Name = (string) o.SelectToken("title");
            var starsArr = o.SelectToken("stars");
            foreach (var star in starsArr)
                model.Stars += " | " + (string) star;
            model.Stars += " |";
            var writersArr = o.SelectToken("writers");
            foreach (var writer in writersArr)
                model.Writers += " | " + (string) writer;
            model.Writers += " |";
            model.PosterLink = (string) o.SelectToken("poster.thumb");
            model.Year = (string) o.SelectToken("year");
            model.Id = (string) o.SelectToken("imdb_id");
            return model;
        }

        public string GetFilmJson(string id)
        {
            var requestUrl = GetByIdUrl + id;
            return DownloadJson(requestUrl);
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