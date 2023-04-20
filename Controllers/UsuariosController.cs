using Libreria.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;

namespace Libreria.Controllers
{
    public class UsuariosController : Controller
    {
        static string key { get; set; } = "A!9HHhi%XjjYY4YP2@Nob009X";

        public ActionResult Index()
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                var UserEmail = Session["userEmail"] as string;
                var us = db.usuarios.FirstOrDefault(e => e.email == UserEmail);

                if (us.rol == "ADMIN")
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            } 
        }

        public ActionResult GetData()
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                List<usuarios> usuariosLista = db.usuarios.ToList<usuarios>();
                List<usuarios> usuariosListaFinal = new List<usuarios>();

                foreach (usuarios u in usuariosLista)
                {
                    usuarios user = new usuarios();
                    user.id = u.id;
                    user.nombre = u.nombre;
                    user.apellido = u.apellido;
                    user.email = u.email;
                    user.clave = u.clave;
                    user.rol = u.rol;

                    usuariosListaFinal.Add(user);
                }

                return Json(new { data = usuariosListaFinal }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CrearEditar(int id = 0)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                if (id == 0)
                {
                    return View(new usuarios());
                }
                else
                {
                    usuarios user = db.usuarios.Where(x => x.id == id).FirstOrDefault<usuarios>();
                    user.clave = Decrypt(user.clave);
                    return View(user);
                }
            }  
        }

        [HttpPost]
        public ActionResult CrearEditar(usuarios user)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                if (user.id == 0)
                {
                    var query = (from us in db.usuarios
                                 select us
                           ).AsEnumerable().Where(us => us.email == user.email);

                    if (query.Count() > 0)
                    {
                        return RedirectToAction("Index", new { ac = "emailExiste" });
                    }

                    user.clave = Encrypt(user.clave);

                    db.usuarios.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index", new { ac = "nuevo" });
                }
                else
                {
                    if (user.clave == null)
                    {
                        usuarios usuario = UsuarioEnBaseDeDatos(user.id);
                        user.clave = Decrypt(usuario.clave);
                        user.clave = Encrypt(user.clave);
                    }
                    else
                    {
                        user.clave = Encrypt(user.clave);
                    }

                    db.Entry(user).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", new { ac = "editado" });
                }
            } 
        }

        [HttpGet]
        public ActionResult EditarPerfil()
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                int userId = Convert.ToInt32(Session["userId"].ToString());
                usuarios user = db.usuarios.Where(x => x.id == userId).FirstOrDefault<usuarios>();
                user.clave = Decrypt(user.clave);
                return View(user);
            }  
        }

        [HttpPost]
        public ActionResult EditarPerfil(usuarios user)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                usuarios usuario = UsuarioEnBaseDeDatos(user.id);

                user.email = usuario.email;
                user.rol = usuario.rol;

                if (user.clave == null)
                {
                    user.clave = Decrypt(usuario.clave);
                    user.clave = Encrypt(user.clave);
                }
                else
                {
                    user.clave = Encrypt(user.clave);
                }

                db.Entry(user).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("EditarPerfil", new { ac = "editado" });
            } 
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                usuarios user = db.usuarios.Where(x => x.id == id).FirstOrDefault<usuarios>();
                db.usuarios.Remove(user);
                db.SaveChanges();
                return Json(new { success = true, message = "Registro eliminado del sistema" }, JsonRequestBehavior.AllowGet);
            }    
        }

        [HttpPost]
        public ActionResult EliminarCuenta()
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                int userId = Convert.ToInt32(Session["userId"].ToString());
                usuarios user = db.usuarios.Where(x => x.id == userId).FirstOrDefault<usuarios>();
                EnviarEmail(user, "Confirmación de cuenta eliminada || Mundo Creativo");
                db.usuarios.Remove(user);
                db.SaveChanges();
                return RedirectToAction("Ingresar", "Login", new { ac = "eliminado" });
            }   
        }

        public void EnviarEmail(usuarios user, String asunto)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity())
            {
                MailMessage message = new MailMessage();
                MailAddress fromAddress = new MailAddress("mundocreativolibreria@gmail.com", "Mundo Creativo");
                const string fromPassword = "wcufmtlmccatiiwq";
                string textBody = "Estimado/a " + user.nombre + ",<br/><br/> Su cuenta fue eliminada exitosamente del sistema.<br/>" + 
                    "Lamentamos verlo partir, pero esperamos que vuelva pronto a visitar nuestro sitio." +
                    " Para futuras compras deberá volver a registrarse, utilizando el mismo correo o uno nuevo<br/>" +
                    "¡Muchas gracias!<br/><br/>Mundo Creativo";

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

        public usuarios UsuarioEnBaseDeDatos(int id)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                usuarios user = db.usuarios.Where(x => x.id == id).FirstOrDefault<usuarios>();
                return user;
            }
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