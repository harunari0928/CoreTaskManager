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

namespace CoreTaskManager.Pages.TaskManager
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CoreTaskManager.Models.CoreTaskManagerContext _context;

        public IndexModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public IList<TaskModel> ThisTask { get;set; }

        public async Task OnGetAsync(string progressId)
        {
            var tasks = from t in _context.Task
                        select t;
            if (!String.IsNullOrEmpty(progressId))
            {
                tasks = tasks.Where(t => t.Id == int.Parse(progressId));
            }
            else
            {
                Response.Redirect("~/Progress");
            }
            ThisTask = await tasks.ToListAsync();
            
        }
    }
}
