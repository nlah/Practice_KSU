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
        public delegate List<Ad_model> fuck(Ad message,string mess, int id);
        [HttpGet]
        public ActionResult Index(int id, string search)
        {
            Ad inf = Ad.GetInstance();
            if (id < 0) id = 0;
            var Search_data = ((fuck)Session["fuck"])(inf,search,id* count_Search);
            if (Search_data.Count == 0)
            {
                id--;
                if (id < 0) id = 0;
                Search_data = ((fuck)Session["fuck"])(inf, search,id* count_Search);
            }
            ViewData["id"] = id ;
            ViewData["search"] = search;
            return View(Search_data);
        }
        [HttpPost]
        public ActionResult Index(string search)
        {
            fuck fuck_search = (Ad a, string mess,int id) => { ViewData["tags_search"] = test2.Models.tag.Search_all(search, count_Search); return a.Search_all(mess, count_Search,id); };
            return View("Index", standart_fuck(fuck_search, search));
        }

        [HttpGet]
        public ActionResult type(string type)
        {
            fuck fuck_search = (Ad a, string mess, int skip) => a.ads_type(mess, count_Search, skip);
            return View("Index", standart_fuck(fuck_search, type));
        }

        [HttpGet]
        public ActionResult tag(string tag)
        {
            fuck fuck_search = (Ad a, string mess, int skip) => a.ads_tag(mess, count_Search, skip);
            return View("Index", standart_fuck(fuck_search, tag));

        }
        List<Ad_model> standart_fuck(fuck fuck_search,string search)
        {
            Ad inf = Ad.GetInstance();
            List<Ad_model> data = fuck_search(inf, search, 0);
            ViewData["id"] = 1;
            ViewData["search"] = search;
            Session["fuck"] = fuck_search;
            return data;
        }

    }
}