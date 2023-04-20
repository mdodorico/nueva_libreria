using Libreria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mvc6.Controllers
{
    public class MenuAdminController : Controller
    {
        public ActionResult MenuAdmin()
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity())
            {
                var UserEmail = Session["userEmail"] as string;
                var us = db.usuarios.FirstOrDefault(e => e.email == UserEmail);

                if (us.rol == "ADMIN")
                {
                    return PartialView();
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
