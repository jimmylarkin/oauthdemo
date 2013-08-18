using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JobDone.Models
{
  public class ProfileModel
  {
    [Required(ErrorMessage = "The username is required")]
    [Display(Name = "User name")]
    public string UserName { get; set; }
  }
}
