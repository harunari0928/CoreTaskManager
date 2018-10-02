using CoreTaskManager.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTaskManager.Pages.Progresses
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CoreTaskManager.Models.CoreTaskManagerContext _context;
        private readonly int _pageSize;
        private static int _numOfProgresses;
        private static int _lastPage;
        private static string _progressGenre;
        private static string _searchString;

        public IndexModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
            _pageSize = 12;
        }

        public IList<Progress> Progress { get; set; }
        public SelectList Genres { get; set; }
        public string ProgressGenre { get; set; }
        public static int CurrentPage { get; set; }

        public async Task OnGetAsync(string progressGenre, string searchString)
        {
            CurrentPage = 1;
            _progressGenre = progressGenre;
            _searchString = searchString;
            var progresses = FilterProgresses(progressGenre, searchString);
            Progress = await progresses.ToListAsync();
            Genres = new SelectList(await GenerateGenreList().Distinct().ToListAsync());

        }
        public async Task OnPostCurrentPage()
        {
            var progresses = FilterProgresses(_progressGenre, _searchString);
            Progress = await progresses.ToListAsync();
            Genres = new SelectList(await GenerateGenreList().Distinct().ToListAsync());
        }
        public async Task OnPostNextPage()
        {
            CurrentPage++;
            if (CurrentPage > _lastPage)
            {
                CurrentPage = _lastPage;
            }

            var progresses = FilterProgresses(_progressGenre, _searchString);
            Progress = await progresses.ToListAsync();
            Genres = new SelectList(await GenerateGenreList().Distinct().ToListAsync());


        }
        public async Task OnPostPrevPage()
        {
            CurrentPage--;
            if (CurrentPage < 1)
            {
                CurrentPage = 1;
            }
            var progresses = FilterProgresses(_progressGenre, _searchString);
            Progress = await progresses.ToListAsync();
            Genres = new SelectList(await GenerateGenreList().Distinct().ToListAsync());

        }

        private IQueryable<Progress> FilterProgresses(string progressGenre, string searchString)
        {
            ViewData["CurrentPage"] = CurrentPage;
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
            _numOfProgresses = progresses.Count();
            _lastPage = _numOfProgresses / _pageSize + 1;
            return progresses = Paging(progresses, CurrentPage, _pageSize);
        }
        private IQueryable<string> GenerateGenreList()
        {
            var genreQuery = from m in _context.Progress
                             orderby m.Genre
                             select m.Genre;
            return genreQuery;
        }
        private IQueryable<Progress> Paging(IQueryable<Progress> progresses, int currentPage, int pageSize)
        {
            // もし変数currenPageが不正な値であればページは１とする
            if (currentPage < 1)
            {
                currentPage = 1;
            }
            return progresses.Skip((currentPage - 1) * pageSize).Take(pageSize);

        }
    }
}
