
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Pinterest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace Pinterest.Controllers
{
    /*[Authorize(Roles = "Administrator")]*/
    public class UsersController : Controller
    {

        private ApplicationDbContext db = ApplicationDbContext.Create();
        // GET: Users
        public ActionResult Index()
        {
            var users = from user in db.Users
                        orderby user.UserName
                        select user;
            ViewBag.UsersList = users;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            return View();
        }


        public ActionResult New()
        {
            ApplicationUser user = new ApplicationUser();
            return View(user);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();
            var roles = from role in db.Roles select role;
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
        public ActionResult ShowBookmarks(string id)
        {
            ApplicationUser user = db.Users.Find(id);

            var bookmarks = db.Bookmarks.Include("Category").Include("User");

            var categories = GetAllCategories();
            ICollection<Category> Categories = new List<Category>();

            foreach (Category cat in categories)
            {
                if (GetBokmarksForCateg(cat.CategoryId, id) > 0)
                {
                    Categories.Add(cat);
                }
            }
            ViewBag.Categories = Categories;

            //  System.Diagnostics.Debug.WriteLine(bookmarks.ToArray().Length);

            ViewBag.allBookmarks = GetAllBookmarks(user.Id);

            return View(user);
        }

        [NonAction]
        public int GetBokmarksForCateg(int id, string userid)
        {
            int nr = 0;
            IQueryable<Bookmark> bookmarks = from book in db.Bookmarks
                                             where book.CategoryId == id && book.UserId == userid
                                             select book;
            ICollection<Bookmark> bookmarks1 = bookmarks.ToList();
            nr = bookmarks1.Count();
            return nr;

        }

        [NonAction]
        public ICollection<Category> GetAllCategories()
        {
            IQueryable<Category> categories = from cat in db.Categories
                                              select cat;
            return categories.ToList();
        }

        public ICollection<Bookmark> GetAllBookmarks(string id)
        {
            IQueryable<Bookmark> bookmarks = from book in db.Bookmarks
                                             where book.UserId == id
                                             select book;
            return bookmarks.ToList();
        }


        public ActionResult Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            ViewBag.utilizatorCurent = User.Identity.GetUserId();

            // var userRole = user.Roles.FirstOrDefault();

            var roles = db.Roles.ToList();
            var roleName = roles.Where(j => j.Id == user.Roles.FirstOrDefault().RoleId).Select(a => a.Name).FirstOrDefault();
            ViewBag.roleName = roleName;
            return View(user);
        }


        public ActionResult Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            return View(user);
        }




        [HttpPut]
        public ActionResult Edit(string id, ApplicationUser newData)
        {

            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                if (TryUpdateModel(user))
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.PhoneNumber = newData.PhoneNumber;
                    var roles = from role in db.Roles select role;
                    foreach (var role in roles)
                    {
                        UserManager.RemoveFromRole(id, role.Name);
                    }
                    var selectedRole =
                    db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                    UserManager.AddToRole(id, selectedRole.Name);
                    TempData["message"] = "Utilizatorul a fost editat cu succes";
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                return View(user);
            }

        }
        [HttpDelete]

        public ActionResult Delete(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            TempData["message"] = "The user name " + user.UserName + " has been successfully removed";
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


    }
}