using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using test2.Models;
namespace test2.Controllers
{

    public class SearchController : MyBaseController
    {
        static int count_Search=5;
        public delegate List<Ad_model> fuck(Ad message,string mess);
        [HttpGet]
        public ActionResult Index(int id, string search)
        {
            Ad inf = Ad.GetInstance();
            if (id < 1) id = 1;
            var Search_data = ((fuck)Session["fuck"])(inf,search);
            int index = 0;
            List<Ad_model> data = new List<Ad_model>();
            foreach (var i in Search_data)
            {
                index++;            
                if (index == count_Search * id) break;
                if (count_Search * (id-1) <= index) data.Add(i);
            }
            ViewData["id"] = id ;
            ViewData["search"] = search;
            return View(data);
        }
        [HttpPost]
        public ActionResult Index(string search)
        {
            Ad inf = Ad.GetInstance();
            var Search_data = inf.Search_all(search);
            int index = 0;
            List<Ad_model> data = new List<Ad_model>();
            foreach (var i in Search_data)
            {
                index++;
                if (index == count_Search) break;
                data.Add(i);
            }
            ViewData["id"] = 1;
            ViewData["search"] = search;
            fuck fuck_search = (Ad a, string mess) => { ViewData["tags_search"] = test2.Models.tag.Search_all(search); return a.Search_all(mess); };
            Session["fuck"] = fuck_search;
            return View(data);
        }

        [HttpGet]
        public ActionResult type(string type)
        {
            Ad inf = Ad.GetInstance();
            int index = 0;
            List<Ad_model> data = new List<Ad_model>(); 
            foreach (var i in inf.ads_type(type))
            {
                index++;
                if (index == count_Search ) break;
                data.Add(i);
            }
            ViewData["id"] = 1;
            ViewData["search"] = type;
            fuck fuck_search = (Ad a, string mess) => a.ads_type(mess);
            Session["fuck"] = fuck_search;
            return View("Index", data);
        }

        [HttpGet]
        public ActionResult tag(string tag)
        {
            Ad inf = Ad.GetInstance();
            int index = 0;
            List<Ad_model> data = new List<Ad_model>();
            foreach (var i in inf.ads_tag(tag))
            {
                index++;
                if (index == count_Search) break;
                data.Add(i);
            }
            ViewData["id"] = 1;
            ViewData["search"] = tag;
            fuck fuck_search = (Ad a, string mess) => a.ads_tag(mess);
            Session["fuck"] = fuck_search;
            return View("Index", data);

        }

    }
}