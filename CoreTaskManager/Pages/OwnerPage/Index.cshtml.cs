using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CoreTaskManager.Model;
using CoreTaskManager.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CoreTaskManager.Pages.OwnerPage
{
    public class IndexModel : PageModel
    {
        private readonly CoreTaskManager.Models.CoreTaskManagerContext _context;
        private const string SessionCurrentProgress = "CurrentProgress";

        public IndexModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
        }

        public IList<AchievedTask> AchievedTasks { get;set; }

        public async Task<IActionResult> OnGetAsync(string progressIdString)
        {
            if (!HttpContext.Session.IsAvailable)
            {
                await HttpContext.Session.LoadAsync();
            }
            if (String.IsNullOrEmpty(progressIdString))
            {
                return Redirect("../Progresses");
            }
            int currentProgress = int.Parse(progressIdString);
            var thisProgress = _context.Progresses.Where(p => p.Id == currentProgress).First();
            // 進捗を作成したユーザとページを開いたユーザが一致しなければ一覧ページへ戻る
            if (thisProgress.UserName != User.Identity.Name)
            {
                return Redirect("../Progresses");
            }
            HttpContext.Session.SetInt32(SessionCurrentProgress, currentProgress);
            AchievedTasks = await _context.AchievedTasks.ToListAsync();
            return Page();
        }
        public JsonResult OnPostSendUnappliedTable()
        {
            int? currentProgress = HttpContext.Session.GetInt32(SessionCurrentProgress);
            if (currentProgress == null)
            {
                Redirect("../Progresses");
                return new JsonResult("serverError");
            }
            try
            {
                AchievedTasks = _context.AchievedTasks.Where(a => a.ProgressId == currentProgress).ToList();
                var unappliedTasks = new List<UnappliedTaskTableModel>();
                foreach (var achievedTask in AchievedTasks)
                {
                    unappliedTasks.Add(new UnappliedTaskTableModel
                    {
                        AchievedTaskId = achievedTask.Id,
                        UserName = achievedTask.UserName,
                        ProgressName = _context.Progresses.Where(p => p.Id == achievedTask.ProgressId).First().Title,
                        TaskName = _context.Tasks.Where(t => t.Id == achievedTask.TaskId).First().TaskName,
                        AchievedDateTime = achievedTask.AchievedDateTime,
                        Description = achievedTask.Description ?? ""
                    });
                }
                return new JsonResult(JsonConvert.SerializeObject(unappliedTasks));
            }
            catch
            {
                return new JsonResult("serverError");
            }
        }
    }
}
