using CoreTaskManager.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoreTaskManager.Pages.TaskManager
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly Models.CoreTaskManagerContext _context;
        private const string SessionCurrentProgress = "CurrentProgress";
        private const string SessionNumberOfTasks = "NumberOfTasks";
        private const int _pageSize = 5;

        public IndexModel(Models.CoreTaskManagerContext context)
        {
            _context = context;
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
            int progressId = int.Parse(progressIdString);
            HttpContext.Session.SetInt32(SessionCurrentProgress, progressId);
            
            var thisProgress = from p in _context.Progresses
                               select p;
            thisProgress = thisProgress.Where(p => p.Id == progressId);
            if (thisProgress.Count() == 0)
            {
                return Redirect("../Progresses");
            }
            var participants = from p in _context.Participants
                               select p;
            participants = participants.Where(p => p.ProgressId == progressId);

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
        public JsonResult OnPostUpdateProgress()
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
                    AchievedDateTime = DateTime.Now.ToLocalTime(),
                    IsAuthorized = false,
                    Description = aSingleWord
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
                Redirect($"TaskManager/Index?progressIdString={progressId}");
                return new JsonResult(JsonConvert.SerializeObject(result));
            }
            catch
            {
                return new JsonResult("serverError");
            }
        }
        
        public async Task<IActionResult> OnPostSetPariticipant()
        {
            if (!HttpContext.Session.IsAvailable)
            {
                await HttpContext.Session.LoadAsync();
            }
            var progressId = HttpContext.Session.GetInt32(SessionCurrentProgress);
            var participantInThisProgress = _context.Participants.Where(p => p.UserName == User.Identity.Name)
                .Where(p => p.ProgressId == progressId);
            if (participantInThisProgress.Count() != 0)
            {
                return Redirect($"TaskManager/Index?progressIdString={progressId}");
            }
            _context.Participants.Add(new Participant
            {
                ProgressId = (int)progressId,
                UserName = User.Identity.Name,
                CurrentProgress = 0
            });

            await _context.SaveChangesAsync();
            return Redirect($"TaskManager/Index?progressIdString={progressId}");
        }
        public async Task<IActionResult> OnPostDeleteParticipant()
        {
            if (!HttpContext.Session.IsAvailable)
            {
                await HttpContext.Session.LoadAsync();
            }
            if (HttpContext.Session.GetInt32(SessionCurrentProgress) == null)
            {
                return Redirect("../Progresses");
            }
            var progressId = HttpContext.Session.GetInt32(SessionCurrentProgress);
            var thisParticipant = await _context.Participants.Where(p => p.ProgressId == progressId)
                .Where(p => p.UserName == User.Identity.Name).FirstOrDefaultAsync();
            if (thisParticipant == null)
            {
                return Redirect($"TaskManager/Index?progressIdString={progressId.ToString()}");
            }
            var achievedTasks = _context.AchievedTasks.Where(a => a.ProgressId == progressId).Where(a => a.UserName == thisParticipant.UserName).ToList();
            achievedTasks.ForEach(a => _context.AchievedTasks.Remove(a));
            _context.Participants.Remove(thisParticipant);
            await _context.SaveChangesAsync();

            return Redirect($"TaskManager/Index?progressIdString={progressId.ToString()}");
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
            var sendStatement = new StringBuilder();
            sendStatement.AppendLine("<!channel>");
            sendStatement.AppendLine($"{progress.UserName}さん");
            sendStatement.AppendLine("以下のurl先で承認作業をお願いします");
            var ownerPageUrl = $"{Request.Scheme}://{Request.Host}/OwnerPage?progressIdString={progress.Id}";
            sendStatement.AppendLine(ownerPageUrl);
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
        private enum ColumAlphaBet
        {
            A = 0,
            B,
            C,
            D,
            E,
            F,
            G,
            H,
            I,
            J,
            K,
            L,
            M,
            N,
            O,
            P,
            Q,
            R,
            S,
            T,
            U,
            V,
            W,
            X,
            Y,
            Z
        }
    }
}