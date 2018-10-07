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
        private const string SessionCurrentPage = "CurrentPage";
        private const string SessionCurrentProgress = "CurrentProgress";
        private const string SessionNumberOfTasks = "NumberOfTasks";
        private int _pageSize { get; set; }

        public IndexModel(CoreTaskManager.Models.CoreTaskManagerContext context)
        {
            _context = context;
            _pageSize = 5;
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
        public IList<AchievedTask> AchivedInThisProgress { get; set; }

        public async Task<IActionResult> OnGetAsync(string progressIdString, string currentPageString)
        {
            // progressIdStringがnullであれば-1が代入
            int progressId = int.Parse(progressIdString ?? "-1");
            int currentPage = int.Parse(currentPageString ?? "1");
            if (progressId == -1)
            {
                return Redirect("./Progresses");
            }
            HttpContext.Session.SetInt32(SessionCurrentPage, currentPage);
            HttpContext.Session.SetInt32(SessionCurrentProgress, progressId);

            var _thisProgress = from p in _context.Progresses
                                select p;
            _thisProgress = _thisProgress.Where(p => p.Id == progressId);
            if (_thisProgress.Count() == 0)
            {
                return Redirect("./Progresses");
            }
            var participants = from p in _context.Participants
                               select p;
            participants = participants.Where(p => p.ProgressId == progressId);
            participants = participants.Take(_pageSize).Skip((currentPage - 1) * _pageSize);

            var tasks = from t in _context.Tasks
                        select t;
            tasks = tasks.Where(t => t.ProgressId == progressId);

            var achievedtasks = from a in _context.AchievedTasks
                               select a;
            achievedtasks = achievedtasks.Where(a => a.ProgressId == progressId);

            
            Participants = await participants.ToListAsync();
            ThisTasks = await tasks.ToListAsync();
            ThisProgress = await _thisProgress.FirstAsync();
            AchivedInThisProgress = await achievedtasks.ToListAsync();
            HttpContext.Session.SetInt32(SessionNumberOfTasks, ThisProgress.NumberOfItems);
            return Page();
        }
        public ActionResult OnPostSetTasks()
        {
            try
            {
                int? _progressId = HttpContext.Session.GetInt32(SessionCurrentProgress);
                int? _numberOfTasks = HttpContext.Session.GetInt32(SessionNumberOfTasks);
                if (_progressId == null || _numberOfTasks == null)
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
                                    ProgressId = (int)_progressId,
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
        public ActionResult OnPostUpdateProgress()
        {
            try
            {
                int? _progressId = HttpContext.Session.GetInt32(SessionCurrentProgress);
                int? _numberOfTasks = HttpContext.Session.GetInt32(SessionNumberOfTasks);
                if (_progressId == null || _numberOfTasks == null)
                {
                    return new JsonResult("serverError");
                }
                string cellId = "";
                {
                    var stream = new MemoryStream();
                    Request.Body.CopyTo(stream);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        string requestBody = reader.ReadToEnd();
                        if (requestBody.Length <= 0)
                        {
                            return new JsonResult("failed");
                        }
                        var receiveData = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);
                        cellId = receiveData["cellId"];
                    }
                }
                var columAlphabet = (ColumAlphaBet)Enum.Parse(typeof(ColumAlphaBet), cellId[0].ToString());
                int rowNumber = int.Parse(cellId.Substring(1, cellId.Length - 1));
                int clickedTaskId = AcquireClickedTask(_context.Tasks, (int)_progressId, rowNumber).Id;
                var clickedParticipant = AcquireClickedParticipant(_context.Participants, (int)_progressId, columAlphabet);
                var clickedParticipantName = clickedParticipant.UserName;
                // 進捗更新申請した人とユーザが一致しなければ処理中断
                if (User.Identity.Name != clickedParticipantName)
                {
                    return new JsonResult("failed");
                }
                int clickedParticipantCurrentProgress = clickedParticipant.CurrentProgress;
                // ひとつ前のタスクが完了していなければ進捗更新できない
                if (rowNumber != clickedParticipantCurrentProgress + 1)
                {
                    return new JsonResult("failed");
                }
                _context.AchievedTasks.Add(new AchievedTask
                {
                    ProgressId = (int)_progressId,
                    TaskId = clickedTaskId,
                    UserName = clickedParticipantName,
                    AchievedDateTime = DateTime.Now,
                    IsAuthorized = false
                });
                _context.SaveChanges();
                var result = new Dictionary<string, string>
                {
                    { "dateTime", DateTime.Now.ToString() }
                };
                return new JsonResult(JsonConvert.SerializeObject(result));
            }
            catch
            {
                return new JsonResult("serverError");
            }
        }
        public async Task<IActionResult> OnPostSetPariticipant()
        {
            var _progressId = HttpContext.Session.GetInt32(SessionCurrentProgress);
            var _currentPage = HttpContext.Session.GetInt32(SessionCurrentPage);
            if (_progressId == null)
            {
                return RedirectToPage("../Progresses");
            }
            var participantInThisProgress = _context.Participants.Where(p => p.UserName == User.Identity.Name)
                .Where(p => p.ProgressId == _progressId);
            if (participantInThisProgress.Count() != 0)
            {
                return RedirectToPage("../Progresses");
            }
            _context.Participants.Add(new Participant
            {
                ProgressId = (int)_progressId,
                UserName = User.Identity.Name,
                CurrentProgress = 0
            });

            await _context.SaveChangesAsync();
            return Redirect($"TaskManager/Index?progressIdString={_progressId}&currentPageString={_currentPage.ToString()}");
        }
        public async Task<IActionResult> OnPostDeleteParticipant()
        {
            var _progressId = HttpContext.Session.GetInt32(SessionCurrentProgress);
            var _currentPage = HttpContext.Session.GetInt32(SessionCurrentPage);
            if (_progressId == null)
            {
                return RedirectToPage("../Progresses");
            }
            var _thisParticipant = _context.Participants.Where(p => p.ProgressId == _progressId)
                .Where(p => p.UserName == User.Identity.Name).FirstOrDefaultAsync();
            if (_thisParticipant.Result == null)
            {
                return Redirect($"TaskManager/Index?progressIdString={_progressId.ToString()}&currentPageString={_currentPage.ToString()}");
            }
            int id = _thisParticipant.Result.Id;
            ThisParticipant = await _context.Participants.FindAsync(id);
            if (ThisParticipant != null)
            {
                _context.Participants.Remove(ThisParticipant);
                await _context.SaveChangesAsync();
            }

            return Redirect($"TaskManager/Index?progressIdString={_progressId.ToString()}&currentPageString={_currentPage.ToString()}");
        }
        private Participant AcquireClickedParticipant(DbSet<Participant> displayedParticipants, int progressId, ColumAlphaBet alphabet)
        {
            var selectedParticipant = displayedParticipants.Where(p => p.ProgressId == progressId).ToList()[(int)alphabet];
            return selectedParticipant;
        }
        private TaskModel AcquireClickedTask(DbSet<TaskModel> displayedTasks, int progressId, int rowNumber)
        {
            var selectedTask = displayedTasks.Where(p => p.ProgressId == progressId)
                .Skip(rowNumber - 1).First();
            return selectedTask;
        }
        enum ColumAlphaBet
        {
            A = 0,
            B,
            C,
            D,
            E
        }
    }
}