using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoreTaskManager.Model;

namespace CoreTaskManager.Models
{
    public class CoreTaskManagerContext : DbContext
    {
        public CoreTaskManagerContext (DbContextOptions<CoreTaskManagerContext> options)
            : base(options)
        {
        }

        public DbSet<CoreTaskManager.Model.Progress> Progresses { get; set; }
        public DbSet<CoreTaskManager.Model.Participant> Participants { get; set; }
        public DbSet<CoreTaskManager.Model.TaskModel> Tasks { get; set; }
    }
}
