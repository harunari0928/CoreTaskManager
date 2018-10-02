using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CoreTaskManager.Model;
using CoreTaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoreTaskManager.Pages.Progresses
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CoreTaskManager.Models.CoreTaskManagerContext _context;
        private readonly int _pageSize;

        public IndexModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
            _pageSize = 12;
        }

        public IList<Progress> Progress { get;set; }
        public SelectList Genres { get; set; }
        public string ProgressGenre { get; set; }

        public async Task OnGetAsync(string progressGenre, string searchString, string currentPage)
        {
            var genreQuery = from m in _context.Progress
                             orderby m.Genre
                             select m.Genre;

            var progresses = from p in _context.Progress
                             select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                progresses = progresses.Where(p => p.Title.Contains(searchString));
            }
            if (!String.IsNullOrEmpty(progressGenre))
            {
                progresses = progresses.Where(x => x.Genre == progressGenre);
            }
            progresses = progresses.Paging(currentPage, _pageSize);


            Genres = new SelectList(await genreQuery.Distinct().ToListAsync());
            Progress = await progresses.ToListAsync();
        }

     
    }

    static class MyExtenisons
    {
        // TODO: テスト
        public static IQueryable<Progress> Paging(this IQueryable<Progress> progresses,string strCurrentPage, int pageSize)
        {
            // もし変数currenPageが不正な値であればページは１とする
            if (!int.TryParse(strCurrentPage, out int currentPage))
            {
                currentPage = 1;
            }
            return progresses.Skip((currentPage - 1) * pageSize).Take(pageSize);

        }

    }


}
