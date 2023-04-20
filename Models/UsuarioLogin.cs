using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Libreria.Models
{
    public class UsuarioLogin
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9]+(\\.[a-z]{2,4})$", ErrorMessage = "Ingrese un email válido")]
        [Display(Name = "Email")]
        public string email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string clave { get; set; }

        [Display(Name = "¿Recordar cuenta?")]
        public bool RememberMe { get; set; }
    }

    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DBLibreriaEntities")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}