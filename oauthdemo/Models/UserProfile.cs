using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace oauthdemo.Models
{
    [Table("users")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int UserId { get; set; }

        [Column("username")]
        public string UserName { get; set; }

        [Column("password")]
        public string Password { get; set; }
    }
}
