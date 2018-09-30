using CoreTaskManager.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CoreTaskManager.Pages.Progresses
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly CoreTaskManager.Models.CoreTaskManagerContext _context;
        
        public CreateModel(CoreTaskManager.Models.CoreTaskManagerContext context, IHostingEnvironment e)
        {
            _context = context;
            _hostingEnvironment = e;
        }

        public IActionResult OnGet(IFormFile pic)
        {
            return Page();
        }

        [BindProperty]
        public Progress Progress { get; set; }
        
        [Authorize]
        public async Task<IActionResult> OnPostAsync(IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var imageName = "";
            if (file != null)
            {
                imageName = Path.GetFileName(file.FileName);
                var fileName = Path.Combine(_hostingEnvironment.WebRootPath, imageName);
                file.CopyTo(new FileStream(fileName, FileMode.Create));

            }

            // 自動入力項目
            Progress.UserName = User.Identity.Name;
            Progress.RegisteredDateTime = DateTime.Now;
            Progress.NumberOfItems = 0;
            Progress.Image = "/" + imageName;


            _context.Progress.Add(Progress);

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}