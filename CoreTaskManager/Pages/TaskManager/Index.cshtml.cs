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
        [BindProperty]
        public IList<Participant> Participants { get; set; }
        public IList<Participant> SelectedParticipants { get; set; }
        [BindProperty]
        public Participant ThisParticipant { get; set; }
        [BindProperty]
        public Progress ThisProgress { get; set; }

        public async Task<IActionResult> OnGetAsync(string progressId)
        {
            if (String.IsNullOrEmpty(progressId))
            {
                return Redirect("./Progresses");
            }
            HttpContext.Session.SetString(SessionCurrentProgress, progressId);

            var participants = from p in _context.Participants
                               select p;
            participants = participants.Where(p => p.ProgressId == int.Parse(progressId))
                            .Take(5).Skip((_pageNum - 1) * 5);

            var tasks = from t in _context.Tasks
                        select t;
            tasks = tasks.Where(t => t.Id == int.Parse(progressId));
            Participants = await _context.Participants.ToListAsync();
            ThisTasks = await tasks.ToListAsync();
            ThisProgress = await _context.Progresses.FirstOrDefaultAsync(p => p.Id == int.Parse(progressId));
            HttpContext.Session.SetString(SessionNumberOfTasks, ThisProgress.NumberOfItems.ToString());
            return Page();
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
                            ThisProgress = _context.Progresses.FirstOrDefault(p => p.Id == _progressId);
                            ThisProgress.NumberOfItems = receiveData.Count;
                        }
                    }
                }

                Tasks.ForEach(task => _context.Tasks.Add(task));
                _context.SaveChanges();
                return new JsonResult("success");
            }
            catch
            {
                return new JsonResult("serverError");
            }

        }
        public async Task OnPostSetPariticipant()
        {
            string _progressIdString = HttpContext.Session.GetString(SessionCurrentProgress);
            if (String.IsNullOrEmpty(_progressIdString))
            {
                Redirect("./Progresses");
            }
            int _progressId = int.Parse(_progressIdString);
            var participantInThisProgress = _context.Participants.Where(p => p.UserName == User.Identity.Name)
                .Where(p => p.ProgressId == _progressId);
            if (participantInThisProgress.Count() != 0)
            {
                return;
            }
            _context.Participants.Add(new Participant
            {
                ProgressId = _progressId,
                UserName = User.Identity.Name,
                CurrentProgress = 0
            });

            await _context.SaveChangesAsync();
            await OnGetAsync(_progressIdString);
        }
        public async Task OnPostDeleteParticipant()
        {
            string _progressIdString = HttpContext.Session.GetString(SessionCurrentProgress);
            if (String.IsNullOrEmpty(_progressIdString))
            {
                return;
            }
            int _progressId = int.Parse(_progressIdString);
            var _thisParticipant = _context.Participants.Where(p => p.ProgressId == _progressId)
                .Where(p => p.UserName == User.Identity.Name).FirstOrDefaultAsync();
            int id = _thisParticipant.Result.Id;
            ThisParticipant = await _context.Participants.FindAsync(id);
            if (ThisParticipant != null)
            {
                _context.Participants.Remove(ThisParticipant);
                await _context.SaveChangesAsync();
            }
            await OnGetAsync(_progressIdString);

        }
    }
}