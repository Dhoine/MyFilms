using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyFilms.Models
{
    public class FilmDbModel
    {
        public string Id { get; set; }
        public bool InFavourites { get; set; }
        public bool InHistory { get; set; }
        public int UserRating { get; set; }
        public ApplicationUser User { get; set; }
    }
}
