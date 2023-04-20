using Libreria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Net;
using System.Net.Mail;

namespace Libreria.Controllers
{
    public class CarritoController : Controller
    {
        int userId;
        double valorTotal;

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetData()
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                valorTotal = 0;
                List<carrito> car = db.carrito.ToList();
                List<carrito> shop = new List<carrito>();
                userId = Convert.ToInt32(Session["userId"].ToString());

                foreach (carrito c in car)
                {
                    if (c.id_usuario == userId)
                    {
                        carrito carrito = new carrito();

                        carrito.id = c.id;
                        carrito.id_producto = c.id_producto;
                        carrito.imagen = c.imagen;
                        carrito.nombre = c.nombre;
                        carrito.precio = (float)c.precio;
                        carrito.descripcion = c.descripcion;
                        carrito.cantidad = c.cantidad;

                        shop.Add(carrito);
                    }
                }

                foreach (carrito carro in shop)
                {
                    if (carro.cantidad == 1)
                    {
                        valorTotal = valorTotal + carro.precio;
                    }
                    else
                    {
                        valorTotal = valorTotal + (carro.precio * carro.cantidad);
                    }
                }

                return Json(new { data = shop, total = valorTotal }, JsonRequestBehavior.AllowGet);
            }   
        }

        [HttpPost]
        public ActionResult Crear(int id)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                userId = Convert.ToInt32(Session["userId"].ToString());
                productos producto = db.productos.ToList().Where(x => x.id == id).FirstOrDefault();
                carrito car = new carrito();

                List<carrito> carro = db.carrito.ToList();
                List<carrito> shop = new List<carrito>();

                foreach (carrito c in carro)
                {
                    if (c.id_usuario == userId)
                    {
                        carrito carrito = new carrito();
                        carrito.precio = (float)c.precio;
                        valorTotal = valorTotal + carrito.precio;
                    }
                }

                if (Existe(producto.id))
                {
                    carrito c = db.carrito.Where(x => x.id_producto == id).FirstOrDefault();
                    c.cantidad = c.cantidad + 1;
                    productos pr = db.productos.Where(x => x.id == id).FirstOrDefault();
                    pr.stock = pr.stock - 1;

                    valorTotal = valorTotal + c.precio;

                    db.SaveChanges();
                    return Json(new { total = valorTotal, redirectToUrl = Url.Action("Index", "Carrito") });
                }
                else
                {
                    car.imagen = producto.imagen;
                    car.nombre = producto.nombre;
                    car.precio = producto.precio;
                    car.descripcion = producto.descripcion;
                    car.cantidad = 1;
                    car.id_usuario = userId;
                    car.id_producto = producto.id;

                    db.carrito.Add(car);
                    db.SaveChanges();

                    productos pr = db.productos.Where(x => x.id == producto.id).FirstOrDefault();
                    pr.stock = pr.stock - 1;

                    valorTotal = valorTotal + car.precio;

                    db.SaveChanges();
                    return Json(new { total = valorTotal, redirectToUrl = Url.Action("Index", "Carrito") });
                }
            }  
        }

        public ActionResult Finalizar()
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                int userId = Convert.ToInt32(Session["userId"].ToString());
                usuarios user = db.usuarios.Where(x => x.id == userId).FirstOrDefault<usuarios>();
                EnviarEmail(user, "Compra finalizada || Mundo Creativo");

                List<carrito> car = db.carrito.ToList();

                foreach (carrito c in car)
                {
                    if (c.id_usuario == user.id)
                    {
                        db.carrito.Remove(c);
                    }
                }

                db.SaveChanges();
                return View("Finalizar");
            }
        }

        public void EnviarEmail(usuarios user, String asunto)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                const string fromPassword = "wcufmtlmccatiiwq";
                List<carrito> car = db.carrito.ToList();
                List<carrito> shop = new List<carrito>();

                foreach (carrito c in car)
                {
                    if (c.id_usuario == user.id)
                    {
                        carrito carrito = new carrito();

                        carrito.id = c.id;
                        carrito.id_producto = c.id_producto;
                        carrito.imagen = c.imagen;
                        carrito.nombre = c.nombre;
                        carrito.precio = (float)c.precio;
                        carrito.descripcion = c.descripcion;
                        carrito.cantidad = c.cantidad;

                        shop.Add(carrito);
                    }
                }

                string textBody = "Estimado/a " + user.nombre + ",<br/> Su compra finalizó con éxito, a continuación puede visualizar los detalles de la misma.<br/>"
                             + "Próximamente le estaremos enviando un email para realizar el pago.<br/><br/>¡Muchas gracias!<br/><br/> Mundo Creativo";

                textBody += "<div style='margin-top: 30px; text-align: center'><table border=" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 400 + "><tr bgcolor='#4da6ff'><td><b>Producto</b></td> <td> <b> Precio </b> </td><td> <b> Cantidad </b> </td></tr>";
                foreach (carrito c in shop)
                {
                    textBody += "<tr><td>" + c.nombre + "</td><td> " + String.Format("{0:c}", c.precio) + "</td> <td>" + c.cantidad + "</td></tr>";
                }
                textBody += "</tbody></table></div>";

                MailMessage message = new MailMessage();
                MailAddress fromAddress = new MailAddress("mundocreativolibreria@gmail.com", "Mundo Creativo");

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
        public ActionResult BorrarUno(int id)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                carrito car = db.carrito.Where(x => x.id == id).FirstOrDefault<carrito>();
                productos pr = db.productos.Where(x => x.id == car.id_producto).FirstOrDefault();

                if (car.cantidad > 1)
                {
                    car.cantidad = car.cantidad - 1;
                    pr.stock = pr.stock + 1;
                }
                else if (car.cantidad == 1)
                {
                    db.carrito.Remove(car);
                    pr.stock = pr.stock + 1;
                }
                db.SaveChanges();
                valorTotal = valorTotal - car.precio;
                return Json(new { total = valorTotal, success = true }, JsonRequestBehavior.AllowGet);
            } 
        }

        [HttpPost]
        public ActionResult BorrarTodo(int id)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                carrito car = db.carrito.Where(x => x.id == id).FirstOrDefault<carrito>();
                productos pr = db.productos.Where(x => x.id == car.id_producto).FirstOrDefault();

                pr.stock = pr.stock + car.cantidad;
                db.carrito.Remove(car);
                db.SaveChanges();
                valorTotal = valorTotal - car.precio;
                return Json(new { total = valorTotal, success = true }, JsonRequestBehavior.AllowGet);
            }   
        }

        public Boolean Existe(int idProducto)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                var query = (from car in db.carrito
                             select car
                              ).AsEnumerable().Where(car => car.id_producto == idProducto);

                if (query.Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}