using CoreTaskManager.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly Models.CoreTaskManagerContext _context;
        private readonly int _pageSize;

        public const string SessionCurrentPage = "CurrentPage";
        public const string SessionProgressGenre = "ProgressGenre";
        public const string SessionSearchString = "SearchString";
        public const string SessionNumOfProgresses = "NumOfProgresses";
        public const string SessionLastPage = "LastPage";


        public IndexModel(Models.CoreTaskManagerContext context)
        {
            _context = context;
            _pageSize = 12;
        }

        public IList<Progress> Progress { get; set; }
        public SelectList Genres { get; set; }
        public string ProgressGenre { get; set; }
        
        public async Task OnGetAsync(string progressGenre, string searchString)
        {
            HttpContext.Session.SetString(SessionCurrentPage, "1");
            HttpContext.Session.SetString(SessionProgressGenre, progressGenre ?? "");
            HttpContext.Session.SetString(SessionSearchString, searchString ?? "");
            
            var progresses = FilterProgresses(progressGenre, searchString);
            Progress = await progresses.ToListAsync();
            Genres = new SelectList(await GenerateGenreList().ToListAsync());

        }
        public async Task OnPostCurrentPage()
        {
            var progresses = FilterProgresses(HttpContext.Session.GetString(SessionProgressGenre), HttpContext.Session.GetString(SessionSearchString));
            Progress = await progresses.ToListAsync();
            Genres = new SelectList(await GenerateGenreList().ToListAsync());
        }
        public async Task OnPostNextPage()
        {
            int _currentPage = int.Parse(HttpContext.Session.GetString(SessionCurrentPage));
            int _lastPage = int.Parse(HttpContext.Session.GetString(SessionLastPage));
            _currentPage++;
            if (_currentPage > _lastPage)
            {
                _currentPage = _lastPage;
            }
            else
            {
                HttpContext.Session.SetString(SessionCurrentPage, _currentPage.ToString());
            }

            var progresses = FilterProgresses(HttpContext.Session.GetString(SessionProgressGenre), HttpContext.Session.GetString(SessionSearchString));
            Progress = await progresses.ToListAsync();
            Genres = new SelectList(await GenerateGenreList().ToListAsync());


        }
        public async Task OnPostPrevPage()
        {
            int _currentPage = int.Parse(HttpContext.Session.GetString(SessionCurrentPage));
            _currentPage--;
            if (_currentPage < 1)
            {
                _currentPage = 1;
            }
            else
            {
                HttpContext.Session.SetString(SessionCurrentPage,_currentPage.ToString());
            }
            var progresses = FilterProgresses(HttpContext.Session.GetString(SessionProgressGenre), HttpContext.Session.GetString(SessionSearchString));
            Progress = await progresses.ToListAsync();
            Genres = new SelectList(await GenerateGenreList().ToListAsync());

        }

        private IQueryable<Progress> FilterProgresses(string progressGenre, string searchString)
        {
            ViewData["CurrentPage"] = HttpContext.Session.GetString(SessionCurrentPage);
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
            HttpContext.Session.SetString(SessionNumOfProgresses, progresses.Count().ToString());
            int _lastPage = progresses.Count() / _pageSize + 1;
            int _currentPage = int.Parse(HttpContext.Session.GetString(SessionCurrentPage));
            HttpContext.Session.SetString(SessionLastPage, _lastPage.ToString());            
            return progresses = Paging(progresses, _currentPage, _pageSize);
        }
        private IQueryable<string> GenerateGenreList()
        {
            var genreQuery = from m in _context.Progress
                             orderby m.Genre
                             select m.Genre;
            return genreQuery.Distinct();
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
