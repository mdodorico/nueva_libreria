using Libreria.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Libreria.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (DBLibreriaEntity db = new DBLibreriaEntity())
            {
                var productos = from p in db.productos select p;
                return View(productos.ToList());
            }
        }
    }
}