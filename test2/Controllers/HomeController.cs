using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4j.AspNet.Identity;
using System.Web.Mvc;
using Neo4j.Driver.V1;
using Neo4jClient;
using Microsoft.AspNet.Identity.Owin;
using System.ComponentModel.DataAnnotations;
using test2.Models;
using Microsoft.AspNet.Identity;
namespace test2.Controllers
{
    public class MyBaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.data = type_standart.Type_node_to_list(); //Add whatever
            ViewData["id"] = 0;
            base.OnActionExecuting(filterContext);
        }
    }
    public class HomeController : MyBaseController
    {
        public ActionResult Index(int id = 0)
        {
            if (id < 0) id = 0;
            Ad inf = Ad.GetInstance();
            var data = inf.ads(5, 5 * id);
            data = inf.ads(5, 5 * id);
            ViewData["id"] = id;
            return View(data);
        }
        [HttpPost]
        public ActionResult Index(string Search)
        {
            Ad inf = Ad.GetInstance();
            return View(inf.Search_all(Search));
        }
        [Authorize]
        public ActionResult Add_ad()
        {
            var inf = new Models.Ad_model();
            inf.tags_list =type_standart.get_name_list().Select(a=>a.name).ToList();
            return View(inf);

        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Add_ad(Ad_model bane)
        {
            bane.tags_list = type_standart.get_name_list().Select(a => a.name).ToList();
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var a in errors)
                ViewBag.Message += a.ErrorMessage;
            if (ModelState.IsValid)
            {
                User.Identity.GetUserId();
                var id = Models.user.users_find(User.Identity.GetUserId());
                Models.Ad ts = Models.Ad.GetInstance();
                List<string> Teg = bane.tags.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if(!ts.create(User.Identity.GetUserId(), bane.type, Teg, bane.header, bane.data)) return View(bane);
                return View("Index", Ad.GetInstance().ads(10, 0));
            }

            return View(bane);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        [HttpPost]
        public string Contact(Ad_model names)
        {
            string fin = names.data+ names.header;
           
            return fin;
        }

        [Authorize]
        public ActionResult Ad_user()
        {
            Ad inf = Ad.GetInstance();
            return View(inf.User_ad(User.Identity.GetUserId()));
        }
        [Authorize]
        public ActionResult Del_user(string id)
        {

            Ad inf = Ad.GetInstance();
            if (User.IsInRole("Admin"))
            {
                user.Del_node(inf.Ad_user_id(long.Parse(id)), id.ToString());
                return View("Ad_user", inf.User_ad(User.Identity.GetUserId()));
            }
            else if(User.IsInRole("User"))
                user.Del_node(User.Identity.GetUserId(), id.ToString());
            return View("Ad_user", inf.User_ad(User.Identity.GetUserId()));
            
        }
        [Authorize]
        public ActionResult Edit(long id)
        {
            Ad inf = Ad.GetInstance();
            var pre = inf.ad_id(id.ToString());
            foreach (var i in pre.tags_list)
            {
                pre.tags += i + " ";
            }
            return View("Edit", pre);
        }
        [HttpPost]
        [Authorize]
        public ActionResult Edit(Ad_model edit)
        {
            Ad inf = Ad.GetInstance();
            if (User.IsInRole("Admin"))
            {
                inf.Ad_edit(inf.Ad_user_id(edit.id), edit);
            }
            else
            {
                if (User.IsInRole("User"))
                    inf.Ad_edit(User.Identity.GetUserId(), edit);
            }
            return View("Ad_user", inf.User_ad(User.Identity.GetUserId()));
        }

    }
}
