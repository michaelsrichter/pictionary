using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dx.pictionary.app.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Game(int id)
        {
            return
                View(new GameViewModel
                {
                    GameId = "Game " + id,
                    ProductFeature = new ProductFeature {Category = "PAAS", Name = "DocumentDb"}
                });
        }
    }

    public class ProductFeature
    {
        public string Name { get; set; }
        public string Category { get; set; }
    }

    public class GameViewModel
    {
        public ProductFeature ProductFeature { get; set; }
        public string GameId { get; set; }
    }
}