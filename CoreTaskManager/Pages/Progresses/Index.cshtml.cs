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
                HttpContext.Session.SetInt32(SessionCurrentPage, 1);
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
            // sessionページ数がnullであれば1を代入
            int currentPage = 1;
            if (HttpContext.Session.GetInt32(SessionCurrentPage) != null)
            {
                currentPage = (int)HttpContext.Session.GetInt32(SessionCurrentPage);
            }
            int lastPage;
            if (HttpContext.Session.GetInt32(SessionLastPage) != null)
            {
                lastPage = (int)HttpContext.Session.GetInt32(SessionLastPage);
            }
            else
            {
                lastPage = _context.Progresses.Count() / _pageSize + 1;
            }
            currentPage++;
            if (currentPage > lastPage)
            {
                currentPage = lastPage;
            }
            else
            {
                HttpContext.Session.SetInt32(SessionCurrentPage, currentPage);
            }
            var _progresssGenre = HttpContext.Session.GetString(SessionProgressGenre);
            var _searchString = HttpContext.Session.GetString(SessionSearchString);
            await OnGetAsync(_progresssGenre, _searchString, currentPage.ToString());
            return Redirect($"Progresses?progressGenre={ProgressGenre}&searchString={_searchString}&currentPageString={currentPage.ToString()}");
        }
        public async Task<IActionResult> OnPostPrevPage()
        {
            int currentPage = 1;
            if (HttpContext.Session.GetInt32(SessionCurrentPage) != null)
            {
                currentPage = (int)HttpContext.Session.GetInt32(SessionCurrentPage);
            }
            currentPage--;
            if (currentPage < 1)
            {
                currentPage = 1;
                HttpContext.Session.SetInt32(SessionCurrentPage, 1);
            }
            else
            {
                HttpContext.Session.SetInt32(SessionCurrentPage, currentPage);
            }
            var progresssGenre = HttpContext.Session.GetString(SessionProgressGenre);
            var searchString = HttpContext.Session.GetString(SessionSearchString);
            var progresses = FilterProgresses(progresssGenre, searchString, currentPage.ToString());
            await OnGetAsync(progresssGenre, searchString, currentPage.ToString());
            return Redirect($"Progresses?progressGenre={ProgressGenre}&searchString={searchString}&currentPageString={currentPage}");
        }

        private IQueryable<Progress> FilterProgresses(string progressGenre, string searchString, string currentPageString)
        {
            int currentPage = 1;
            ViewData["CurrentPage"] = "1";
            if (!String.IsNullOrEmpty(currentPageString))
            {
                currentPage = int.Parse(currentPageString);
                ViewData["CurrentPage"] = currentPageString;
            }

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

            HttpContext.Session.SetInt32(SessionNumOfProgresses, progresses.Count());
            int lastPage = progresses.Count() / _pageSize + 1;
            HttpContext.Session.SetInt32(SessionLastPage, lastPage);
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
