using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using MyFilms.Models.ListsViewModels;
using Newtonsoft.Json.Linq;

namespace MyFilms.Controllers
{
    public class ListsController : Controller
    {
        private const string GetByIdUrl = "https://theimdbapi.org/api/movie?movie_id=";
        [Route("Lists/TopFilms/{page}")]
        public IActionResult TopFilms(int page)
        {
            var models = new List<FilmModel>();
            var url = "http://www.imdb.com/chart/top?ref_=nv_mv_250_6";
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var ids = Regex.Matches(doc.ParsedText, "(?<=data-titleid=\")tt(\\d+?)(?=\">)").ToList();
            foreach (var film in ids.GetRange((page-1)*5,5))
            {
                var id = film.ToString();
                var requestUrl = GetByIdUrl + id;
                using (var wc = new WebClient())
                {
                    var model = new FilmModel();
                    var json = wc.DownloadString(requestUrl);
                    var o = JObject.Parse(json);
                    model.Caption = (string) o.SelectToken("description");
                    model.Directors = (string) o.SelectToken("director");
                    var genreArr = o.SelectToken("genre");
                    foreach (var genre in genreArr)
                    {
                        model.Genres += (string) genre + "\t";
                    }
                    model.ImdbRating = (string) o.SelectToken("rating");
                    model.Name = (string) o.SelectToken("title");
                    var starsArr = o.SelectToken("stars");
                    foreach (var star in starsArr)
                    {
                        model.Stars += (string) star;
                    }
                    var writersArr = o.SelectToken("writers");
                    foreach (var writer in writersArr)
                    {
                        model.Writers += (string)writer;
                    }
                    model.PosterLink = (string) o.SelectToken("poster.thumb");
                    model.Year = (string) o.SelectToken("year");
                    models.Add(model);
                    Console.WriteLine(json);
                }
            }
            return View(models.ToArray());
        }
    }
}