using Libreria.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Libreria.Controllers
{
    public class LoginController : Controller
    {
        static string key { get; set; } = "A!9HHhi%XjjYY4YP2@Nob009X";

        [AllowAnonymous]
        public ActionResult Ingresar()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]

        public ActionResult Ingresar(UsuarioLogin user)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                if (ModelState.IsValid)
                {
                    var query = (from us in db.usuarios
                                 select us
                           ).AsEnumerable().Where(us => us.email == user.email && Decrypt(us.clave) == user.clave);

                    if (query.Count() > 0)
                    {
                        usuarios us = db.usuarios.ToList().Where(x => x.email == user.email).FirstOrDefault();
                        Session["userId"] = us.id;
                        Session["userEmail"] = us.email;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Ingresar", new { ac = "error" });
                    }
                }
                else
                {
                    return RedirectToAction("Ingresar", new { ac = "error" });
                }
            }   
        }

        public ActionResult Registrarse()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registrarse(usuarios usuario)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                var query = (from us in db.usuarios
                             select us
                                ).AsEnumerable().Where(us => us.email == usuario.email);

                if (query.Count() > 0)
                {
                    return RedirectToAction("Registrarse", new { ac = "emailExiste" });
                }

                usuario.clave = Encrypt(usuario.clave);
                usuario.rol = "USER";

                db.usuarios.Add(usuario);
                db.SaveChanges();
                EnviarEmailCuentaNueva(usuario, "Bienvenido/a a Mundo Creativo");
                return RedirectToAction("Registrarse", new { ac = "nuevo" });
            }   
        }

        public ActionResult RecuperarClave()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RecuperarClave(String email)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                usuarios user = db.usuarios.Where(x => x.email == email).FirstOrDefault<usuarios>();

                if (user == null)
                {
                    return RedirectToAction("RecuperarClave", new { ac = "emailNoExiste" });
                }

                string password = CreateRandomPassword();
                user.clave = Encrypt(password);

                db.Entry(user).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                EnviarEmailRecuperarClave(user, "Cambiar contraseña || Mundo Creativo");
                return RedirectToAction("RecuperarClave", new { ac = "emailEnviado" });
            }  
        }

        public void EnviarEmailCuentaNueva(usuarios user, String asunto)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                MailMessage message = new MailMessage();
                MailAddress fromAddress = new MailAddress("mundocreativolibreria@gmail.com", "Mundo Creativo");
                const string fromPassword = "wcufmtlmccatiiwq";
                string textBody = "¡Bienvenido " + user.nombre + " a Mundo Creativo!<br/> Puede iniciar sesión cuando desee para realizar compras en el sitio.";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                message.From = fromAddress;
                message.To.Add(new MailAddress(user.email));
                message.Subject = asunto;
                message.IsBodyHtml = true;
                message.Body = textBody;
                smtp.Send(message);
            }   
        }

        public void EnviarEmailRecuperarClave(usuarios user, String asunto)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                MailMessage message = new MailMessage();
                MailAddress fromAddress = new MailAddress("mundocreativolibreria@gmail.com", "Mundo Creativo");
                const string fromPassword = "wcufmtlmccatiiwq";
                string textBody = "Estimado/a " + user.nombre + ",<br/> Esta es su nueva clave para acceder al sitio.<br/>" + Decrypt(user.clave.ToString());

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                message.From = fromAddress;
                message.To.Add(new MailAddress(user.email));
                message.Subject = asunto;
                message.IsBodyHtml = true;
                message.Body = textBody;
                smtp.Send(message);
            }  
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            Session.Remove("userId");
            Session.Remove("userEmail");
            return RedirectToAction("Ingresar", "Login");
        }

        private static string CreateRandomPassword(int length = 8)
        {
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }

        public static string Encrypt(string text)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateEncryptor())
                    {
                        byte[] textBytes = UTF8Encoding.UTF8.GetBytes(text);
                        byte[] bytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);
                        return Convert.ToBase64String(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        public static string Decrypt(string cipher)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateDecryptor())
                    {
                        byte[] cipherBytes = Convert.FromBase64String(cipher);
                        byte[] bytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return UTF8Encoding.UTF8.GetString(bytes);
                    }
                }
            }
        }
    }
}