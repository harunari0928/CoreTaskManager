using CoreTaskManager.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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

        public IList<Progress> Progresses { get; set; }
        public SelectList Genres { get; set; }
        public string ProgressGenre { get; set; }
        
        public async Task OnGetAsync(string progressGenre, string searchString, string currentPageString)
        {
            if (String.IsNullOrEmpty(currentPageString))
            {
                HttpContext.Session.SetString(SessionCurrentPage, "1");
            }
            HttpContext.Session.SetString(SessionProgressGenre, progressGenre ?? "");
            HttpContext.Session.SetString(SessionSearchString, searchString ?? "");
            
            var progresses = FilterProgresses(progressGenre, searchString, currentPageString);
            Progresses = await progresses.ToListAsync();
            Genres = new SelectList(await GenerateGenreList().ToListAsync());

        }
        public async Task<IActionResult> OnPostCurrentPage()
        {
            var _genre = HttpContext.Session.GetString(SessionProgressGenre);
            var _searchString = HttpContext.Session.GetString(SessionSearchString);
            var _currentPage = HttpContext.Session.GetString(SessionCurrentPage);
            await OnGetAsync(_genre, _searchString, _currentPage);
            return Redirect($"Progresses?progressGenre={ProgressGenre}&searchString={_searchString}&currentPageString={_currentPage}");
        }
        public async Task<IActionResult> OnPostNextPage()
        {
            int _currentPage = int.Parse(HttpContext.Session.GetString(SessionCurrentPage) ?? "1");
            int _lastPage;
            if (String.IsNullOrEmpty(HttpContext.Session.GetString(SessionCurrentPage)))
            {
                _lastPage = _context.Progresses.Count() / _pageSize + 1;
            }
            else
            {
                _lastPage = int.Parse(HttpContext.Session.GetString(SessionLastPage));
                _currentPage++;
            }
            if (_currentPage > _lastPage)
            {
                _currentPage = _lastPage;
            }
            else
            {
                HttpContext.Session.SetString(SessionCurrentPage, _currentPage.ToString());
            }
            var _progresssGenre = HttpContext.Session.GetString(SessionProgressGenre);
            var _searchString = HttpContext.Session.GetString(SessionSearchString);
            await OnGetAsync(_progresssGenre, _searchString, _currentPage.ToString());
            return Redirect($"Progresses?progressGenre={ProgressGenre}&searchString={_searchString}&currentPageString={_currentPage.ToString()}");
        }
        public async Task<IActionResult> OnPostPrevPage()
        {
            int _currentPage = int.Parse(HttpContext.Session.GetString(SessionCurrentPage) ?? "1");
            _currentPage--;
            if (_currentPage < 1)
            {
                _currentPage = 1;
                HttpContext.Session.SetString(SessionCurrentPage, "1");
            }
            else
            {
                HttpContext.Session.SetString(SessionCurrentPage,_currentPage.ToString());
            }
            var _progresssGenre = HttpContext.Session.GetString(SessionProgressGenre);
            var _searchString = HttpContext.Session.GetString(SessionSearchString);
            var progresses = FilterProgresses(_progresssGenre, _searchString, _currentPage.ToString());
            await OnGetAsync(_progresssGenre, _searchString, _currentPage.ToString());
            return Redirect($"Progresses?progressGenre={ProgressGenre}&searchString={_searchString}&currentPageString={_currentPage}");
        }

        private IQueryable<Progress> FilterProgresses(string progressGenre, string searchString, string currentPageString)
        {
            ViewData["CurrentPage"] = HttpContext.Session.GetString(SessionCurrentPage) ?? "";
            var progresses = from p in _context.Progresses
                             select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                progresses = progresses.Where(p => p.Title.Contains(searchString));
            }
            if (!String.IsNullOrEmpty(progressGenre))
            {
                progresses = progresses.Where(x => x.Genre == progressGenre);
            }
            int _numOfProgresses = progresses.Count();
            HttpContext.Session.SetString(SessionNumOfProgresses, _numOfProgresses.ToString());
            int _lastPage = _numOfProgresses / _pageSize + 1;
            int currentPage = int.Parse(currentPageString ?? "1");
            HttpContext.Session.SetString(SessionLastPage, _lastPage.ToString());            
            return progresses = Paging(progresses, currentPage, _pageSize);
        }
        private IQueryable<string> GenerateGenreList()
        {
            var genreQuery = from m in _context.Progresses
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
