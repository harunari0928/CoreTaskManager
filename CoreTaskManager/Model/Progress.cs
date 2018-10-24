using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTaskManager.Model
{
    public class Progress
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Title { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Genre { get; set; }

        [Display(Name = "Registered DateTime"), DataType(DataType.Date)]
        public DateTime RegisteredDateTime{ get; set; }
        public int NumberOfItems { get; set; }
        public string SlackAppUrl { get; set; }
        public string Image { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

    }

    public static class OperateProgresses
    {
        public static IQueryable<Progress> FilterUsingSearchStrings(this Models.CoreTaskManagerContext context,
            string progressGenre, string searchString)
        {
            var progresses = from p in context.Progresses
                             select p;
            progresses = progresses.OrderByDescending(p => p.RegisteredDateTime);
            if (!String.IsNullOrEmpty(searchString))
            {
                progresses = progresses.Where(p => p.Title.Contains(searchString));
            }
            if (!String.IsNullOrEmpty(progressGenre))
            {
                progresses = progresses.Where(x => x.Genre == progressGenre);
            }

            return progresses;
        }
        public static IQueryable<Progress> Paging(this IQueryable<Progress> progresses, int currentPage, int pageSize)
        {
            // もし変数currenPageが不正な値であればページは１とする
            if (currentPage < 1 || pageSize < 1)
            {
                throw new ArgumentException();
            }
            return progresses.Skip((currentPage - 1) * pageSize).Take(pageSize);
        }

        public static IQueryable<string> GenerateGenreList(this Models.CoreTaskManagerContext context)
        {
            var genreQuery = from m in context.Progresses
                             orderby m.Genre
                             select m.Genre;
            return genreQuery.Distinct();
        }

    }
}
