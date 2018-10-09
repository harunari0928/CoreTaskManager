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
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace CoreTaskManager.Pages.OwnerPage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CoreTaskManager.Models.CoreTaskManagerContext _context;
        private const string SessionCurrentProgress = "CurrentProgress";
        private const string SessionNumberOfTasks = "NumberOfTasks";

        public IndexModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
        }

        public IList<AchievedTask> AchievedTasks { get;set; }
        public Progress ThisProgress { get; set; }

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
            var thisProgress = _context.Progresses.Where(p => p.Id == currentProgress);
            // 進捗を作成したユーザとページを開いたユーザが一致しなければ一覧ページへ戻る
            if (thisProgress.First().UserName != User.Identity.Name)
            {
                return Redirect("../Progresses");
            }
            HttpContext.Session.SetInt32(SessionCurrentProgress, currentProgress);
            AchievedTasks = await _context.AchievedTasks.ToListAsync();
            ThisProgress = await thisProgress.FirstAsync();
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
                        Description = achievedTask.Description ?? "（なし）"
                    });
                }
                return new JsonResult(JsonConvert.SerializeObject(unappliedTasks));
            }
            catch
            {
                return new JsonResult("serverError");
            }
        }
        public ActionResult OnPostSetTasks()
        {
            if (!HttpContext.Session.IsAvailable)
            {
                HttpContext.Session.LoadAsync();
            }
            try
            {
                int? progressId = HttpContext.Session.GetInt32(SessionCurrentProgress);
                int? numberOfTasks = HttpContext.Session.GetInt32(SessionNumberOfTasks);
                if (progressId == null || numberOfTasks == null)
                {
                    return new JsonResult("serverError");
                }

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
                                if (numberOfTasks > 0)
                                {
                                    return new JsonResult("wrongString");
                                }
                                if (receiveString.Length > 15 || receiveString.Length == 0)
                                {
                                    return new JsonResult("wrongString");
                                }

                                Tasks.Add(new TaskModel
                                {
                                    ProgressId = (int)progressId,
                                    TaskName = receiveString
                                });
                            }
                            ThisProgress = _context.Progresses.FirstOrDefault(p => p.Id == progressId);
                            ThisProgress.NumberOfItems = receiveData.Count;
                        }
                    }
                }

                Tasks.ForEach(task => _context.Tasks.Add(task));
                _context.SaveChanges();
                Redirect($"TaskManager/Index?progressIdString={progressId}");
                return new JsonResult("success");
            }
            catch
            {
                return new JsonResult("serverError");
            }

        }
    }
}
