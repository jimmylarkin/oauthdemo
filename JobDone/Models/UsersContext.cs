using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace JobDone.Models
{
  public class DataContext : DbContext
  {
    public DataContext() : base("DefaultConnection")
    {
    }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<ExternalLoginProfile> ExternalLoginProfiles { get; set; }
    public DbSet<TaskEntry> Tasks { get; set; }
  }
}