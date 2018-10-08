using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTaskManager.Model
{
    public class AchievedTask
    {
        public int Id { get; set; }
        public int ProgressId { get; set; }
        public int TaskId { get; set; }
        public string UserName { get; set; }
        public DateTime AchievedDateTime { get; set; }
        public bool IsAuthorized { get; set; }
        public string Description { get; set; }
    }
}
