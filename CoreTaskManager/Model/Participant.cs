using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTaskManager.Model
{
    public class Participant
    {
        public int Id { get; set; }
        public int ProgressId { get; set; }
        public string UserName { get; set; }
        public int CurrentProgress { get; set; }
    }
}
