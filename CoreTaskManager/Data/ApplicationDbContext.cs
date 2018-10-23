using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CoreTaskManager.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreTaskManager.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MyIdentityUser> MyIdentityUsers { get; set; }

        #region SeedData
        public static List<MyIdentityUser> GetSeedingUserIdentities()
        {
            return new List<MyIdentityUser>
            {
                new MyIdentityUser
                {
                    Id = "a",
                    UserName = "ryo",
                },
                new MyIdentityUser
                {
                    Id = "b",
                    UserName = "ryo",
                },
                new MyIdentityUser
                {
                    Id = "c",
                    UserName = "ryo",
                },
                new MyIdentityUser
                {
                    Id = "d",
                    UserName = "ryo",
                },
                new MyIdentityUser
                {
                    Id = "e",
                    UserName = "ryo",
                },
                new MyIdentityUser
                {
                    Id = "F",
                    UserName = "ryo",
                }
            };
        }
        #endregion
        public async virtual Task<List<MyIdentityUser>> GetIdentityAsync()
        {
            return await MyIdentityUsers.ToListAsync();
        }
    }
}
