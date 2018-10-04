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
        private const string SessionCurrentProgress = "CurrentProgress";
        private const string SessionNumberOfTasks = "NumberOfTasks";
        private int _pageNum { get; set; }

        public IndexModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
            _pageNum = 1;
        }

        [BindProperty]
        public IList<TaskModel> ThisTasks { get; set; }
        public IList<Participant> Participants { get; set; }
        public Progress ThisProgress { get; set; }

        public async Task OnGetAsync(string progressId)
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
                ThisProgress = await _context.Progress.FirstOrDefaultAsync(p => p.Id == int.Parse(progressId));
                HttpContext.Session.SetString(SessionNumberOfTasks, ThisProgress.NumberOfItems.ToString());

            }
            else
            {
                RedirectToPage("./Index");
            }
        }

        public ActionResult OnPostSetTasks()
        {
            try
            {
                int _progressId = int.Parse(HttpContext.Session.GetString(SessionCurrentProgress));
                int _numberOfTasks = int.Parse(HttpContext.Session.GetString(SessionNumberOfTasks));
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
                            for (int i = 1; i <= receiveData.Count; i++)
                            {
                                var receiveString = receiveData[$"task{i.ToString()}"];
                                // すでにタスクが登録されていた場合は登録できない
                                
                                if (_numberOfTasks > 0)
                                {
                                    return new JsonResult("wrongString");
                                }
                                if (receiveString.Length > 15 || receiveString.Length == 0)
                                {
                                    return new JsonResult("wrongString");
                                }

                                Tasks.Add(new TaskModel
                                {
                                    ProgressId = _progressId,
                                    TaskName = receiveString
                                });
                            }
                            ThisProgress = _context.Progress.FirstOrDefault(p => p.Id == _progressId);
                            ThisProgress.NumberOfItems = receiveData.Count;
                        }
                    }
                }

                Tasks.ForEach(task => _context.Task.Add(task));
                _context.SaveChanges();
                return new JsonResult("success");
            }
            catch
            {
                return new JsonResult("serverError");
            }

        }
    }

}