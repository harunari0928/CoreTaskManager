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
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace CoreTaskManager.Pages.TaskManager
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CoreTaskManager.Models.CoreTaskManagerContext _context;
        public const string SessionCurrentProgress = "CurrentProgress";
        private int _pageNum { get; set; }

        public IndexModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
            _pageNum = 1;
        }

        [BindProperty]
        public IList<TaskModel> ThisTasks { get; set; }
        public IList<Participant> Participants { get; set; }

        public async Task<IActionResult> OnGetAsync(string progressId)
        {
            HttpContext.Session.SetString(SessionCurrentProgress, progressId);
            var participants = from p in _context.Participants
                               select p;
            participants = participants.Where(p => p.ProgressId == int.Parse(progressId))
                            .Take(5).Skip((_pageNum - 1) * 5);

            var tasks = from t in _context.Task
                        select t;
            if (!String.IsNullOrEmpty(progressId))
            {
                tasks = tasks.Where(t => t.Id == int.Parse(progressId));
                Participants = await participants.ToListAsync();
                ThisTasks = await tasks.ToListAsync();
                return Page();

            }
            return RedirectToPage("./Index");

        }

        // TODO:　例外処理
        public ActionResult OnPostSend()
        {
            var Tasks = new List<TaskModel>();
            {
                var stream = new MemoryStream();
                Request.Body.CopyTo(stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    string requestBody = reader.ReadToEnd();
                    if (requestBody.Length > 0)
                    {
                        var receiveData = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);
                        for (int i = 0; i < receiveData.Count; i++)
                        {
                            Tasks.Add(new TaskModel
                            {
                                ProgressId = int.Parse(HttpContext.Session.GetString(SessionCurrentProgress)),
                                TaskName = receiveData["task" + i.ToString()]
                            });
                        }
                    }
                }
            }
            Tasks.ForEach(task => _context.Task.Add(task));

            return new JsonResult("");
        }
    }

}