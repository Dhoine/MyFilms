using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyFilms.Models.ListsViewModels;
using MyFilms.Services;

namespace MyFilms.Controllers
{
    public class ListsController : Controller
    {
        private const string TopFulmsUrl = "http://www.imdb.com/chart/top?ref_=nv_mv_250_6";
        private readonly IHelper _helper;

        public ListsController(IHelper helper)
        {
            _helper = helper;
        }


        [Route("Lists/TopFilms/{page}")]
        public IActionResult TopFilms(int page)
        {
            var ids = _helper.ParsePage(TopFulmsUrl).ToList();
            if (page < 1 || page > Math.Ceiling(Convert.ToDecimal(ids.Count) / 5))
                return RedirectToAction(nameof(HomeController.Index), "Home");
            ViewBag.PagesCount = Math.Ceiling(Convert.ToDecimal(ids.Count) / 5);
            ViewBag.CurrentPage = page;
            return View(ids.GetRange((page - 1) * 5, 5).Select(film => _helper.ParseFilmJson(_helper.GetFilmJson(film))).ToArray());
        }
    }
}