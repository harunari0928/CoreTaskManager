using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CoreTaskManager.Model;
using CoreTaskManager.Models;

namespace CoreTaskManager.Pages.Progresses
{
    public class DeleteModel : PageModel
    {
        private readonly CoreTaskManager.Models.CoreTaskManagerContext _context;

        public DeleteModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Progress Progress { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Progress = await _context.Progress.FirstOrDefaultAsync(m => m.Id == id);

            if (Progress == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Progress = await _context.Progress.FindAsync(id);

            if (Progress != null)
            {
                _context.Progress.Remove(Progress);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
