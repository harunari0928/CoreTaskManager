using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTaskManager.Model
{
    public class Progress
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Title { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Genre { get; set; }

        [Display(Name = "Registered DateTime"), DataType(DataType.Date)]
        public DateTime RegisteredDateTime{ get; set; }
        public int NumberOfItems { get; set; }
        public string SlackAppUrl { get; set; }
        public string Image { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

    }
}
