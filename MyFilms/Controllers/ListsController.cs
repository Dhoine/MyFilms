using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyFilms.Data;
using MyFilms.Models;
using MyFilms.Services;

namespace MyFilms.Controllers
{
    public class ListsController : Controller
    {
        private const string TopFulmsUrl = "http://www.imdb.com/chart/top?ref_=nv_mv_250_6";
        private const string PlayingNowUrl = "http://www.imdb.com/chart/boxoffice";
        private const string ComingSoonUrl = "http://www.imdb.com/movies-coming-soon";
        private readonly ApplicationDbContext _dbContext;
        private readonly IHelper _helper;
        private readonly UserManager<ApplicationUser> _userManager;

        public ListsController(IHelper helper, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _helper = helper;
            _userManager = userManager;
            _dbContext = context;
        }


        [Route("Lists/TopFilms/{page}")]
        public IActionResult TopFilms(int page)
        {
            var ids = _helper.ParsePage(TopFulmsUrl).ToList();
            return ShowList(ids, page);
        }

        [Route("Lists/NowPlaying/{page}")]
        public IActionResult NowPlaying(int page)
        {
            var ids = _helper.ParsePage(PlayingNowUrl).ToList();
            return ShowList(ids, page);
        }

        [Route("Lists/ComingSoon/{page}")]
        public IActionResult ComingSoon(int page)
        {
            var ids = _helper.ParsePage(ComingSoonUrl).ToList();
            return ShowList(ids, page);
        }

        public IActionResult Search(string search)
        {
            return View(_helper.ParseSearchJson(_helper.GetSearchJson(search)).ToArray());
        }

        private IActionResult ShowList(IEnumerable<string> films, int page)
        {
            var ids = films as List<string>;
            if (ids != null && (page < 1 || page > Math.Ceiling(Convert.ToDecimal(ids.Count) / 5)) || ids == null)
                return RedirectToAction(nameof(HomeController.Index), "Home");
            ViewBag.PagesCount = Math.Ceiling(Convert.ToDecimal(ids.Count) / 5);
            ViewBag.CurrentPage = page;
            if (page * 5 > ids.Count)
                return View(ids.GetRange((page - 1) * 5, ids.Count - (page - 1) * 5)
                    .Select(film => _helper.ParseFilmJson(_helper.GetFilmJson(film))).ToArray());
            if (User.Identity.Name == null)
                return View(ids.GetRange((page - 1) * 5, 5)
                .Select(film => _helper.ParseFilmJson(_helper.GetFilmJson(film))).ToArray());
            return View(_helper.CheckDbStateOfFilms(ids.GetRange((page - 1) * 5, 5)
                .Select(film => _helper.ParseFilmJson(_helper.GetFilmJson(film))).ToArray(),_dbContext, _userManager.GetUserAsync(User).Result.Id));
        }

        [Route("Lists/Watchlist/{page}")]
        public IActionResult Watchlist(int page)
        {
            if (User.Identity.Name == null) return RedirectToAction(nameof(HomeController.Index), "Home");
            var ids = _helper.GetListFromDb(0, _dbContext, _userManager.GetUserAsync(User).Result.Id);
            return ShowList(ids, page);
        }

        [Route("Lists/MyFilms/{page}")]
        public IActionResult MyFilms(int page)
        {
            if (User.Identity.Name == null) return RedirectToAction(nameof(HomeController.Index), "Home");
            var ids = _helper.GetListFromDb(1, _dbContext, _userManager.GetUserAsync(User).Result.Id);
            return ShowList(ids, page);
        }
    }
}