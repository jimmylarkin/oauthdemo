using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace oauthdemo.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "The username is required")]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "The password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}