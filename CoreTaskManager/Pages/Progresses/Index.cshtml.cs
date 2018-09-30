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

        public IndexModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
        }

        public IList<Progress> Progress { get;set; }
        public SelectList Genres { get; set; }
        public string ProgressGenre { get; set; }

        public async Task OnGetAsync(string searchString)
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
            Genres = new SelectList(await genreQuery.Distinct().ToListAsync());
            Progress = await progresses.ToListAsync();
        }
    }


}
