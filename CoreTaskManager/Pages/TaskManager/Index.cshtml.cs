﻿using System;
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
using System.Net;
using System.Text;

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
            if (String.IsNullOrEmpty(progressIdString))
            {
                return Redirect("./Progresses");
            }
            int progressId = int.Parse(progressIdString);
            int currentPage = int.Parse(currentPageString ?? "1");
            HttpContext.Session.SetInt32(SessionCurrentPage, currentPage);
            HttpContext.Session.SetInt32(SessionCurrentProgress, progressId);

            var thisProgress = from p in _context.Progresses
                                select p;
            thisProgress = thisProgress.Where(p => p.Id == progressId);
            if (thisProgress.Count() == 0)
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
            ThisProgress = await thisProgress.FirstAsync();
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
                int? progressId = HttpContext.Session.GetInt32(SessionCurrentProgress);
                int? numberOfTasks = HttpContext.Session.GetInt32(SessionNumberOfTasks);

                if (progressId == null || numberOfTasks == null)
                {
                    return new JsonResult("serverError");
                }

                var aSingleWord = "";
                var cellId = "";
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
                        aSingleWord = receiveData["aSingleWord"];
                    }
                }

                var columAlphabet = (ColumAlphaBet)Enum.Parse(typeof(ColumAlphaBet), cellId[0].ToString());
                int rowNumber = int.Parse(cellId.Substring(1, cellId.Length - 1));
                var clickedTask = AcquireClickedTask(_context.Tasks, (int)progressId, rowNumber);
                int clickedTaskId = clickedTask.Id;
                var clickedParticipant = AcquireClickedParticipant(_context.Participants, (int)progressId, columAlphabet);
                var clickedParticipantName = clickedParticipant.UserName;
                int clickedParticipantCurrentProgress = clickedParticipant.CurrentProgress;

                // 進捗更新申請した人とユーザが一致しなければ処理中断
                if (User.Identity.Name != clickedParticipantName)
                {
                    return new JsonResult("failed");
                }
                // ひとつ前のタスクが完了していなければ進捗更新できない
                else if (rowNumber != clickedParticipantCurrentProgress + 1)
                {
                    return new JsonResult("failed");
                }
                // すでに登録されている場合も処理中断
                else if (_context.AchievedTasks.Where(a => a.TaskId == clickedTaskId)
                    .Where(a => a.UserName == clickedParticipantName).Count() > 0)
                {
                    return new JsonResult("failed");
                }

                var achievdTask = new AchievedTask
                {
                    ProgressId = (int)progressId,
                    TaskId = clickedTaskId,
                    UserName = clickedParticipantName,
                    AchievedDateTime = DateTime.Now,
                    IsAuthorized = false
                };

                var thisProgress = _context.Progresses.Where(p => p.Id == progressId).First();
                if (!String.IsNullOrEmpty(thisProgress.SlackAppUrl))
                {
                    NortificationToOutside(thisProgress, clickedTask, achievdTask, aSingleWord);
                }

                _context.AchievedTasks.Add(achievdTask);
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
                return Redirect("./Progresses");
            }
            var participantInThisProgress = _context.Participants.Where(p => p.UserName == User.Identity.Name)
                .Where(p => p.ProgressId == _progressId);
            if (participantInThisProgress.Count() != 0)
            {
                return Redirect("./Progresses");
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
            var progressId = HttpContext.Session.GetInt32(SessionCurrentProgress);
            var currentPage = HttpContext.Session.GetInt32(SessionCurrentPage);
            if (progressId == null)
            {
                return Redirect("../Progresses");
            }
            var thisParticipant = _context.Participants.Where(p => p.ProgressId == progressId)
                .Where(p => p.UserName == User.Identity.Name).FirstOrDefaultAsync();
            if (thisParticipant.Result == null)
            {
                return Redirect($"TaskManager/Index?progressIdString={progressId.ToString()}&currentPageString={currentPage.ToString()}");
            }
            int id = thisParticipant.Result.Id;
            ThisParticipant = await _context.Participants.FindAsync(id);
            if (ThisParticipant != null)
            {
                _context.Participants.Remove(ThisParticipant);
                await _context.SaveChangesAsync();
            }
            return Redirect($"TaskManager/Index?progressIdString={progressId.ToString()}&currentPageString={currentPage.ToString()}");
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
        private void NortificationToOutside(Progress progress, TaskModel thisTask, AchievedTask achievedTask, string aSingleWord)
        {
            var wc = new WebClient();
            // TODO 承認作業用url
            var sendStatement = new StringBuilder();
            sendStatement.AppendLine("以下のurl先で承認作業をお願いします");
            sendStatement.AppendLine("");
            sendStatement.AppendLine("<申請者>");
            sendStatement.AppendLine(achievedTask.UserName);
            sendStatement.AppendLine("");
            sendStatement.AppendLine("<タスク名>");
            sendStatement.AppendLine("■" + progress.Title);
            sendStatement.AppendLine("  -" + thisTask.TaskName);
            if (!String.IsNullOrEmpty(aSingleWord))
            {
                sendStatement.AppendLine("");
                sendStatement.AppendLine("<一言>");
                sendStatement.AppendLine(aSingleWord);
            }
            var sendData = JsonConvert.SerializeObject(new
            {
                text = sendStatement.ToString(),
                icon_emoji = ":cyclone:",
                username = "承認依頼bot"
            });

            wc.Headers.Add(HttpRequestHeader.ContentType, "application/json;charset=UTF-8");
            wc.Encoding = Encoding.UTF8;

            wc.UploadString(progress.SlackAppUrl, sendData);
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