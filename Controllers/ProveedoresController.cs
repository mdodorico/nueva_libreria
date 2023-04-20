using Libreria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Libreria.Controllers
{
    public class ProveedoresController : Controller
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
                List<proveedores> provList = db.proveedores.ToList<proveedores>();
                List<proveedores> provListFinal = new List<proveedores>();

                foreach (proveedores p in provList)
                {
                    proveedores pFinal = new proveedores();
                    pFinal.id = p.id;
                    pFinal.nombre = p.nombre;
                    pFinal.email = p.email;
                    pFinal.telefono = p.telefono;
                    pFinal.direccion = p.direccion;

                    provListFinal.Add(pFinal);
                }

                return Json(new { data = provListFinal }, JsonRequestBehavior.AllowGet);
            } 
        }

        [HttpGet]
        public ActionResult CrearEditar(int id = 0)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                if (id == 0)
                {
                    return View(new proveedores());
                }
                else
                {
                    return View(db.proveedores.Where(x => x.id == id).FirstOrDefault<proveedores>());
                }
            }   
        }

        [HttpPost]
        public ActionResult CrearEditar(proveedores prov)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                if (prov.id == 0)
                {
                    db.proveedores.Add(prov);
                    db.SaveChanges();
                    return RedirectToAction("Index", new { ac = "nuevo" });
                }
                else
                {
                    db.Entry(prov).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", new { ac = "editado" });
                }
            }  
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity()) 
            {
                proveedores prov = db.proveedores.Where(x => x.id == id).FirstOrDefault<proveedores>();
                db.proveedores.Remove(prov);
                db.SaveChanges();
                return Json(new { success = true, message = "Registro eliminado del sistema" }, JsonRequestBehavior.AllowGet);
            }  
        }
    }
}