using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace JobDone.Models
{
    [Table("ExternalLogins")]
    public class ExternalLoginProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("UserId")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public UserProfile User { get; set; }

        [Column("Provider")]
        public string Provider { get; set; }

        [Column("ProviderUserId")]
        public string ProviderUserId { get; set; }
    }
}
