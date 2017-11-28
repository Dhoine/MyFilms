using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
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
            if (User.Identity.Name == null)
                return View(_helper.ParseSearchJson(_helper.GetSearchJson(search)).ToArray());
            return View(_helper.CheckDbStateOfFilms(_helper.ParseSearchJson(_helper.GetSearchJson(search)), _dbContext,
                _userManager.GetUserAsync(User).Result.Id).ToArray());
        }

        private IActionResult ShowList(IEnumerable<string> films, int page)
        {
            var ids = films as List<string>;
            if (ids != null && (page < 1 || page > Math.Ceiling(Convert.ToDecimal(ids.Count) / 5)) || ids == null)
                return RedirectToAction(nameof(HomeController.Index), "Home");
            ViewBag.PagesCount = Math.Ceiling(Convert.ToDecimal(ids.Count) / 5);
            ViewBag.CurrentPage = page;
            var count = 5;
            if (page * 5 > ids.Count)
                count = ids.Count - (page - 1) * 5;
            if (User.Identity.Name == null)
                return View(ids.GetRange((page - 1) * 5, count)
                    .Select(film => _helper.ParseFilmJson(_helper.GetFilmJson(film))).ToArray());
            return View(_helper.CheckDbStateOfFilms(ids.GetRange((page - 1) * 5, count)
                    .Select(film => _helper.ParseFilmJson(_helper.GetFilmJson(film))).ToArray(), _dbContext,
                _userManager.GetUserAsync(User).Result.Id));
        }

        [Authorize]
        [Route("Lists/Watchlist/{page}")]
        public IActionResult Watchlist(int page)
        {
            var ids = _helper.GetListFromDb(0, _dbContext, _userManager.GetUserAsync(User).Result.Id);
            return ShowList(ids, page);
        }

        [Authorize]
        [Route("Lists/MyFilms/{page}")]
        public IActionResult MyFilms(int page)
        {
            var ids = _helper.GetListFromDb(1, _dbContext, _userManager.GetUserAsync(User).Result.Id);
            return ShowList(ids, page);
        }

        [HttpPost]
        [Authorize]
        [Route("Lists/AddToFavourites/{id}")]
        public IActionResult AddToFavourites(string id)
        {
            _helper.SaveToDb(id, _dbContext, 0, true, _userManager.GetUserAsync(User).Result);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("Lists/RemoveFromFavourites/{id}")]
        public IActionResult RemoveFromFavourites(string id)
        {
            _helper.SaveToDb(id, _dbContext, 2, true, _userManager.GetUserAsync(User).Result);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("Lists/RemoveFromHistory/{id}")]
        public IActionResult RemoveFromHistory(string id)
        {
            _helper.SaveToDb(id, _dbContext, 3, true, _userManager.GetUserAsync(User).Result);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("Lists/AddToHistory/{id}")]
        public IActionResult AddToHistory(string id)
        {
            _helper.SaveToDb(id, _dbContext, 1, true, _userManager.GetUserAsync(User).Result);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("Lists/Rate/{id}/{rate}")]
        public IActionResult Rate(string id, int rate)
        {
            _helper.SaveToDb(id, _dbContext, rate, _userManager.GetUserAsync(User).Result);
            return Ok();
        }
    }
}