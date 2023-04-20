using Libreria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mvc6.Controllers
{
    public class MenuUsuarioController : Controller
    {
        public Exception error { get; private set; }

        public ActionResult MenuUsuario()
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity())
            {
                var UserEmail = Session["userEmail"] as string;
                var us = db.usuarios.FirstOrDefault(e => e.email == UserEmail);

                if (us != null)
                {
                    try
                    {
                        ViewBag.Usuario = us.nombre + " " + us.apellido;
                    }
                    catch (Exception e)
                    {
                        this.error = e;
                    }
                }
                return PartialView();
            }
        }
    }
}
