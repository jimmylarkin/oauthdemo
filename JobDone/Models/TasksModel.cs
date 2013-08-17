using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace JobDone.Models
{
    public class HomeModel
    {
        [Required()]
        public string TaskDescription { get; set; }

        public string TaskEstimation { get; set; }

        public List<TaskEntry> Tasks { get; set; }

        public int CompletedCount { get; set; }

        public int ToDoCount { get; set; }
    }
}
