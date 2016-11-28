using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace test2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : MyBaseController
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View(Models.type_standart.Type_node_to_list());
        }
        [HttpPost]
        public ActionResult Add_tag_rel(string Type, string Tag)
        {
            test2.Models.type_standart.type_tag_rel(Type, Tag);
            return RedirectToActionPermanent("Index");
        }
        [HttpPost]
        public ActionResult Add_tag(string name)
        {
            Models.tag.tag_Add(name);
            Models.type_standart.update();
            return RedirectToActionPermanent("Index");
        }
        [HttpPost]
        public ActionResult Add_type(string name)
        {
            Models.type_standart.type_Add(name);
            Models.type_standart.update();
            return RedirectToActionPermanent("Index");
        }
        [HttpPost]
        public ActionResult det_type(string name)
        {
            Models.type_standart.type_del(name);
            Models.type_standart.update();

            return RedirectToActionPermanent("Index");
        }
        [HttpPost]
        public ActionResult del_tag(string name)
        {
            Models.tag.tag_del(name);
            Models.type_standart.update();
            return RedirectToActionPermanent("Index");
        }
        [HttpPost]
        public ActionResult del_tag_rel(string Type, string Tag)
        {
            test2.Models.type_standart.type_tag_rel_del(Type, Tag);
            Models.type_standart.update();
            return RedirectToActionPermanent("Index");
        }
    }
}