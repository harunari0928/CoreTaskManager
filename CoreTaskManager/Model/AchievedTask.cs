using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTaskManager.Model
{
    public class AchievedTask
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "progressId")]
        public int ProgressId { get; set; }
        [JsonProperty(PropertyName = "taskId")]
        public int TaskId { get; set; }
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }
        [JsonProperty(PropertyName = "achievedDateTime")]
        public DateTime AchievedDateTime { get; set; }
        [JsonProperty(PropertyName = "isAuthorized")]
        public bool IsAuthorized { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    public class UnappliedTaskTableModel
    {
        [JsonProperty(PropertyName = "achivedTaskId")]
        public int AchievedTaskId { get; set; }
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }
        [JsonProperty(PropertyName = "progressName")]
        public string ProgressName { get; set; }
        [JsonProperty(PropertyName = "taskName")]
        public string TaskName { get; set; }
        [JsonProperty(PropertyName = "achievedDateTime")]
        public DateTime AchievedDateTime { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
