using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTaskManager.Model
{
    public class TaskModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ProgressId { get; set; }

        [Required]
        [StringLength(15)]
        public string TaskName { get; set; }
        public bool IsCreated { get; set; }

    }
}
