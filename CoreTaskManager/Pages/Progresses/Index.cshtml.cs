using CoreTaskManager.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly Data.ApplicationDbContext _userContext;
        private readonly int _pageSize;

        public const string SessionCurrentPage = "CurrentPage";
        public const string SessionProgressGenre = "ProgressGenre";
        public const string SessionSearchString = "SearchString";
        public const string SessionNumOfProgresses = "NumOfProgresses";
        public const string SessionLastPage = "LastPage";


        public IndexModel(Models.CoreTaskManagerContext context, Data.ApplicationDbContext userContext)
        {
            _context = context;
            _userContext = userContext;
            // 一ページに表示する項目の量
            _pageSize = 6;
        }

        public IList<Progress> Progresses { get; set; }
        public SelectList Genres { get; set; }
        public IList<Participant> Participants { get; set; }
        public string ProgressGenre { get; set; }
        public IList<MyIdentityUser> ServiceUsers { get; set; }

        public async Task OnGetAsync(string progressGenre, string searchString, string currentPageString)
        {
            if (!HttpContext.Session.IsAvailable)
            {
                await HttpContext.Session.LoadAsync();
            }

            try
            {
                // 検索クエリをセッションに保存
                int currentPage = int.Parse(currentPageString);
                HttpContext.Session.SetInt32(SessionCurrentPage, currentPage);
                HttpContext.Session.SetString(SessionProgressGenre, progressGenre ?? "");
                HttpContext.Session.SetString(SessionSearchString, searchString ?? "");
                ViewData["CurrentPage"] = currentPageString;

                var progresses = _context.FilterUsingSearchStrings(progressGenre, searchString);
                var progressesInAPage = progresses.Paging(currentPage, _pageSize);
                Progresses = await progressesInAPage.ToListAsync();

                // ページ遷移制限のための値をセッションに保存
                int numOfProgresses = progresses.Count();
                int lastPage = numOfProgresses / _pageSize + 1;
                HttpContext.Session.SetInt32(SessionNumOfProgresses, numOfProgresses);
                HttpContext.Session.SetInt32(SessionLastPage, lastPage);

                Genres = new SelectList(await GenerateGenreList().ToListAsync());

                ServiceUsers = await _userContext.MyIdentityUsers.ToListAsync();
                Participants = new List<Participant>();
                await progresses.Select(p => p.Id).ForEachAsync(id =>
                {
                    // 各進捗の参加者を4人ランダムに抽出
                    var concerned4People = _context.Participants.Where(p => p.ProgressId == id).OrderBy(i => Guid.NewGuid()).Take(4).ToList();
                    if (concerned4People.Count > 0)
                    {
                        concerned4People.ForEach(person => Participants.Add(person));
                    }
                });
            }
            catch (Exception e) when (e is ArgumentNullException || e is ArgumentException)
            {
                ViewData["CurrentPage"] = "1";
                await OnGetAsync(progressGenre ?? "", searchString ?? "", "1");
            }
        }
        public async Task<IActionResult> OnPostCurrentPageAsync()
        {
            if (!HttpContext.Session.IsAvailable)
            {
                await HttpContext.Session.LoadAsync();
            }
            var genre = HttpContext.Session.GetString(SessionProgressGenre);
            var searchString = HttpContext.Session.GetString(SessionSearchString);
            var currentPage = HttpContext.Session.GetInt32(SessionCurrentPage);
            return Redirect($"Progresses?progressesGenre={genre}&searchString={searchString}&currentPageString={currentPage}");
        }
        public async Task<IActionResult> OnPostNextPageAsync()
        {
            if (!HttpContext.Session.IsAvailable)
            {
                await HttpContext.Session.LoadAsync();
            }
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
            var progressGenre = HttpContext.Session.GetString(SessionProgressGenre);
            var searchString = HttpContext.Session.GetString(SessionSearchString);
            return Redirect($"Progresses?progressesGenre={progressGenre}&searchString={searchString}&currentPageString={currentPage}");
        }
        public async Task<IActionResult> OnPostPrevPageAsync()
        {
            if (!HttpContext.Session.IsAvailable)
            {
                await HttpContext.Session.LoadAsync();
            }
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
            var progressGenre = HttpContext.Session.GetString(SessionProgressGenre);
            var searchString = HttpContext.Session.GetString(SessionSearchString);
            return Redirect($"Progresses?progressesGenre={progressGenre}&searchString={searchString}&currentPageString={currentPage}");
        }


        IQueryable<string> GenerateGenreList()
        {
            var genreQuery = from m in _context.Progresses
                             orderby m.Genre
                             select m.Genre;
            return genreQuery.Distinct();
        }
    }
}
