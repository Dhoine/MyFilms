using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyFilms.Services;

namespace MyFilms.Controllers
{
    public class InfoController : Controller
    {
        private readonly IHelper _helper;

        public InfoController(IHelper helper)
        {
            _helper = helper;
        }

        [Route("Info/{id}")]
        public IActionResult Info(string id)
        {
            var json = _helper.GetFilmJson(id);
            var model = _helper.ParseFilmJson(json);
            return View(model);
        }
    }
}