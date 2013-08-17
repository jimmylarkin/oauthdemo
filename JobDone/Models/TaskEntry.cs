using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace JobDone.Models
{
    [Table("Tasks")]
    public class TaskEntry
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("UserId")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public UserProfile User { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("Estimation")]
        public string Estimation { get; set; }

        [Column("Completed")]
        public DateTime? Completed { get; set; }
    }
}
