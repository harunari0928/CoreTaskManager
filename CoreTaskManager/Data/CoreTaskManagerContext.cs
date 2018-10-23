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
        public CoreTaskManagerContext(DbContextOptions<CoreTaskManagerContext> options)
            : base(options)
        {
        }

        public DbSet<CoreTaskManager.Model.Progress> Progresses { get; set; }
        public DbSet<CoreTaskManager.Model.Participant> Participants { get; set; }
        public DbSet<CoreTaskManager.Model.TaskModel> Tasks { get; set; }
        public DbSet<CoreTaskManager.Model.AchievedTask> AchievedTasks { get; set; }

        #region SeedData
        public static List<Progress> GetSeedingProgresses()
        {
            return new List<Progress> {
                new Progress
                {
                    Id = 0,
                    Title = "MTMR",
                    RegisteredDateTime = DateTime.Now,
                    Description = "aaaa",
                    NumberOfItems = 0,
                },

                new Progress
                {
                    Id = 1,
                    Title = "KOSEi",
                    RegisteredDateTime = DateTime.Now,
                    Description = "afefabb",
                    NumberOfItems = 0,
                },

                new Progress
                {
                    Id = 2,
                    Title = "dafeaf",
                    RegisteredDateTime = DateTime.Now,
                    Description = "cccc",
                    NumberOfItems = 0,
                },

                new Progress
                {
                    Id = 3,
                    Title = "fdefsfes",
                    RegisteredDateTime = DateTime.Now,
                    Description = "ddddd",
                    NumberOfItems = 0,
                },

                new Progress
                {
                    Id = 4,
                    Title = "MTMR",
                    RegisteredDateTime = DateTime.Now,
                    Description = "aaaa",
                    NumberOfItems = 0,
                },
                new Progress
                {
                    Id = 5,
                    Title = "MTMR2",
                    RegisteredDateTime = DateTime.Now,
                    Description = "aaaa",
                    NumberOfItems = 0,
                },
                new Progress
                {
                    Id = 6,
                    Title = "MTMR3",
                    RegisteredDateTime = DateTime.Now,
                    Description = "aaaa",
                    NumberOfItems = 0,
                }
                };
        }
        #endregion
        public async virtual Task<List<Progress>> GetProgressAsync()
        {
            return await Progresses.ToListAsync();
        }
    }
}
