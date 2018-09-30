using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CoreTaskManager.Model;
using CoreTaskManager.Models;

namespace CoreTaskManager.Pages.TaskManager
{
    public class DetailsModel : PageModel
    {
        private readonly CoreTaskManager.Models.CoreTaskManagerContext _context;

        public DetailsModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
        }

        public TaskModel TaskModel { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TaskModel = await _context.Task.FirstOrDefaultAsync(m => m.Id == id);

            if (TaskModel == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
