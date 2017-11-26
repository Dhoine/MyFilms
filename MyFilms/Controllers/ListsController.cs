using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyFilms.Services;

namespace MyFilms.Controllers
{
    public class ListsController : Controller
    {
        private const string TopFulmsUrl = "http://www.imdb.com/chart/top?ref_=nv_mv_250_6";
        private const string PlayingNowUrl = "http://www.imdb.com/chart/boxoffice";
        private const string ComingSoonUrl = "http://www.imdb.com/movies-coming-soon";
        private readonly IHelper _helper;

        public ListsController(IHelper helper)
        {
            _helper = helper;
        }


        [Route("Lists/TopFilms/{page}")]
        public IActionResult TopFilms(int page)
        {
            var ids = _helper.ParsePage(TopFulmsUrl).ToList();
            return ShowCommonList(ids, page);
        }

        [Route("Lists/NowPlaying/{page}")]
        public IActionResult NowPlaying(int page)
        {
            var ids = _helper.ParsePage(PlayingNowUrl).ToList();
            return ShowCommonList(ids, page);
        }

        [Route("Lists/ComingSoon/{page}")]
        public IActionResult ComingSoon(int page)
        {
            var ids = _helper.ParsePage(ComingSoonUrl).ToList();
            return ShowCommonList(ids, page);
        }

        private IActionResult ShowCommonList(IEnumerable<string> films, int page)
        {
            var ids = films as List<string>;
            if (ids != null && (page < 1 || page > Math.Ceiling(Convert.ToDecimal(ids.Count) / 5)) || ids == null)
                return RedirectToAction(nameof(HomeController.Index), "Home");
            ViewBag.PagesCount = Math.Ceiling(Convert.ToDecimal(ids.Count) / 5);
            ViewBag.CurrentPage = page;
            if (page * 5 > ids.Count)
                return View(ids.GetRange((page - 1) * 5, ids.Count - (page - 1) * 5)
                    .Select(film => _helper.ParseFilmJson(_helper.GetFilmJson(film))).ToArray());
            return View(ids.GetRange((page - 1) * 5, 5)
                .Select(film => _helper.ParseFilmJson(_helper.GetFilmJson(film))).ToArray());
        }
    }
}