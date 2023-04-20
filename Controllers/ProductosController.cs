using Libreria.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Libreria.Controllers
{
    public class ProductosController : Controller
    {
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
                List<productos> prodList = db.productos.ToList<productos>();
                List<productos> prodListFinal = new List<productos>();

                foreach (productos p in prodList)
                {
                    productos pFinal = new productos();
                    pFinal.id = p.id;
                    pFinal.imagen = p.imagen;
                    pFinal.nombre = p.nombre;
                    pFinal.precio = p.precio;
                    pFinal.descripcion = p.descripcion;
                    pFinal.stock = p.stock;

                    prodListFinal.Add(pFinal);
                }

                return Json(new { data = prodListFinal }, JsonRequestBehavior.AllowGet);
            }     
        }

        [HttpGet]
        public ActionResult CrearEditar(int id = 0)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                if (id == 0)
                {
                    return View(new productos());
                }
                else
                {
                    return View(db.productos.Where(x => x.id == id).FirstOrDefault<productos>());
                }
            }    
        }

        [HttpPost]
        public ActionResult CrearEditar(productos producto, HttpPostedFileBase file)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity())
            {
                if (producto.id == 0)
                {
                    if (file != null)
                    {
                        string ruta = Server.MapPath("~/Imagenes/");
                        ruta += file.FileName;
                        file.SaveAs(ruta);
                        producto.imagen = Path.GetFileName(file.FileName);
                    }
                    else
                    {
                        producto.imagen = "no-image.png";
                    }

                    if (ValidarPrecio(producto.precio))
                    {
                        db.productos.Add(producto);
                        db.SaveChanges();
                        return RedirectToAction("Index", new { ac = "nuevo" });
                    }
                    else
                    {
                        productos prod = ProductoEnBaseDeDatos(producto.id);
                        producto.precio = prod.precio;
                        return RedirectToAction("Index", new { ac = "error" });
                    }
                }
                else
                {
                    if (file != null)
                    {
                        string ruta = Server.MapPath("~/Imagenes/");
                        ruta += file.FileName;
                        file.SaveAs(ruta);
                        producto.imagen = Path.GetFileName(file.FileName);
                    }
                    else
                    {
                        productos prod = ProductoEnBaseDeDatos(producto.id);
                        producto.imagen = prod.imagen;
                    }

                    if (ValidarPrecio(producto.precio))
                    {
                        db.Entry(producto).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index", new { ac = "editado" });
                    }
                    else
                    {
                        productos prod = ProductoEnBaseDeDatos(producto.id);
                        producto.precio = prod.precio;
                        return RedirectToAction("Index", new { ac = "error" });
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                productos prod = db.productos.Where(x => x.id == id).FirstOrDefault<productos>();
                db.productos.Remove(prod);
                db.SaveChanges();
                return Json(new { success = true, message = "Registro eliminado del sistema" }, JsonRequestBehavior.AllowGet);
            } 
        }

        public productos ProductoEnBaseDeDatos(int id)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                productos prod = db.productos.Where(x => x.id == id).FirstOrDefault<productos>();
                return prod;
            }
        }

        public Boolean ValidarPrecio(double precio) {

            Regex regex = new Regex(@"^[1-9]\d*(,\d+)?$");

            if (regex.IsMatch(precio.ToString()))
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