using Microsoft.AspNet.Identity;
using Pinterest.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pinterest.Controllers
{
    public class BookmarkController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        Dictionary<int, List<string>> likesMap = new Dictionary<int, List<string>>();
        private int _perPage = 8;

        // GET: Bookmark
        public ActionResult Index(String SearchText, String Option)
        {
            var bookmarks = db.Bookmarks.Include("Category").Include("User").OrderBy(a => a.Date);
            Console.WriteLine(Option);
            if (Option == "1")
            {
                bookmarks = db.Bookmarks.Include("Category").Include("User").Where(x => x.Tags.Contains(SearchText) || x.Description.Contains(SearchText) || x.Title.Contains(SearchText) || SearchText == null).OrderByDescending(a => a.Date);
            }
            else
            {
                if (Option == "2")
                {
                    bookmarks = db.Bookmarks.Include("Category").Include("User").Where(x => x.Tags.Contains(SearchText) || x.Description.Contains(SearchText) || x.Title.Contains(SearchText) || SearchText == null).OrderByDescending(a => a.Likes);
                }
            }

            var totalItems = bookmarks.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this._perPage;
            }
        
            var paginatedBookmarks = bookmarks.Skip(offset).Take(this._perPage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.showButtons = false;
            if (User.IsInRole("Editor") || User.IsInRole("Administrator"))
            {
                ViewBag.showButtons = true;
            }

            ViewBag.currentPage = currentPage;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.Bookmarks = paginatedBookmarks;
            ViewBag.Categories = GetAllCategories();

            return View();
        }

        [Authorize(Roles = "Editor, Administrator")]
        public ActionResult Show(int id)
        {
            Bookmark bookmark = db.Bookmarks.Find(id);
            ViewBag.bookmark = bookmark;

            List<Bookmark> likeThis = new List<Bookmark>();

            String[] tags = bookmark.Tags.Split(',');
            foreach (String tag in tags)
            {
                var more = db.Bookmarks.Include("Category").Include("User").Where(m => m.Tags.Contains(tag) && m.BookmarkId != bookmark.BookmarkId).ToList();

                foreach (Bookmark b in more)
                {
                    likeThis.Add(b);
                }
            }
            ViewBag.moreLikeThis = likeThis.Distinct().ToList();

            bookmark.Categories = GetAllCategories();
            ViewBag.Categories = GetAllCategories();
            ViewBag.showButtons = false;
            ViewBag.path = ".." + bookmark.FilePath;
            ViewBag.UserName = User.Identity.GetUserName();
            ViewBag.Comments = db.Comments.Include("Bookmark").Where(x => x.BookmarkId.Equals(bookmark.BookmarkId)).OrderBy(a => a.Date);

            // if user is admin or editor and wants to edit his own bookmark
            if ((User.IsInRole("Editor") && (User.Identity.GetUserId() == bookmark.UserId)) || User.IsInRole("Administrator"))
            {
                ViewBag.showButtons = true;
            }
            return View(bookmark);
        }

        [Authorize(Roles = "Editor, Administrator")]
        public ActionResult New()
        {
            Bookmark bookmark = new Bookmark();
            bookmark.Categories = GetAllCategories();
            bookmark.UserId = User.Identity.GetUserId();

            return View(bookmark);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();
            var categories = from cat in db.Categories
                             select cat;
            foreach(var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            return selectList;
        }

        [NonAction]
        public int GetMaxId()
        {
            var Bookmarks = db.Bookmarks.OrderByDescending(m => m.BookmarkId);
            if (Bookmarks.ToList().Count() != 0)
            {
                var latestBookmark = Bookmarks.First();
                return latestBookmark.BookmarkId + 1;
            }
            else return 0;
        }

        [Authorize(Roles ="Editor, Administrator")]
        [HttpPost]
        public ActionResult New(Bookmark bookmark, HttpPostedFileBase file)
        {
            try
            {
                bookmark.Categories = GetAllCategories();

                if (ModelState.IsValid)
                {
                    if (file != null)
                    {
                        int id = GetMaxId();
                        string name = id.ToString() + ".png";
                        string path = Path.Combine(Server.MapPath("~/Images"),
                                                    name);
                        file.SaveAs(path);
                        bookmark.FilePath = "../Images/" + name;
                        bookmark.BookmarkId = id;
                    }
                    //bookmark.Tags = "ala";
                    bookmark.Likes = 0;
                    bookmark.Comments = new List<Comment>();
                    db.Bookmarks.Add(bookmark);
                    db.SaveChanges();
                    TempData["message"] = "The bookmark has been added!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(bookmark);
                }
            }
            catch (Exception e)
            {
                bookmark.Categories = GetAllCategories();
                return View(bookmark);
            }
        }

        [Authorize(Roles = "Editor, Administrator")]
        public ActionResult Edit(int id)
        {
            Bookmark bookmark = db.Bookmarks.Find(id);
            //am nevoie de toate categoriile ca sa pot face drop down
            bookmark.Categories = GetAllCategories();
            bookmark.UserId = User.Identity.GetUserId();

            if (bookmark.UserId == User.Identity.GetUserId() || User.IsInRole("Administartor"))
            {
                return View(bookmark);
            }
            else
            {
                TempData["message"] = "You cannot edit a bookmark that doesn't belong to you!";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Editor, Administrator")]
        [HttpPut]
        public ActionResult Edit(int id, Bookmark requestBookmark)
        {

            try
            {
                requestBookmark.Categories = GetAllCategories();

                if (ModelState.IsValid)
                {
                    Bookmark bookmark = db.Bookmarks.Find(id);
                    if (bookmark.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(bookmark))
                        {
                            bookmark.Title = requestBookmark.Title;
                            bookmark.Description = requestBookmark.Description;
                            bookmark.Date = requestBookmark.Date;
                            bookmark.CategoryId = requestBookmark.CategoryId;
                            bookmark.Tags = requestBookmark.Tags;
                            db.SaveChanges();
                            TempData["message"] = "The bookmark has been edited";
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "You cannot edit a bookmark that doesn't belong to you!";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return View(requestBookmark);
                }
            }
            catch (Exception e)
            {
                return View(requestBookmark);
            }
        }

        [Authorize(Roles = "Editor, Administrator")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Bookmark bookmark = db.Bookmarks.Find(id);
            if (bookmark.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.Bookmarks.Remove(bookmark);
                db.SaveChanges();
                TempData["message"] = "The bookmark has been removed!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You cannot delete a bookmark that doesn't belong to you!";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Editor, Administrator")]
        [HttpPost]
        public ActionResult Save(Bookmark bookmark)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int categoryId = bookmark.CategoryId;
                    Debug.WriteLine(bookmark.Title);
                    Bookmark savedBookmark = new Bookmark();

                    savedBookmark.Title = bookmark.Title;
                    savedBookmark.Description = bookmark.Description;
                    savedBookmark.Date = bookmark.Date;
                    savedBookmark.FilePath = bookmark.FilePath;
                    savedBookmark.CategoryId = categoryId;
                    savedBookmark.Tags = bookmark.Tags;
                    savedBookmark.Comments = new List<Comment> ();
                    savedBookmark.Likes = 0;
                    string userId = User.Identity.GetUserId();
                    savedBookmark.UserId = userId;

                    db.Bookmarks.Add(savedBookmark);
                    db.SaveChanges();
                    TempData["message"] = "The bookmark has been saved!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Cannot save bookmark!";
                    return RedirectToAction("Index");

                }
            }
            catch (Exception e)
            {
                TempData["message"] = "Exception!";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Like(int id)
        {
            // string bookmarkId = Request["BookmarkId"].ToString();
            Bookmark bookmark = db.Bookmarks.Find(id);
            int likes = bookmark.Likes;
            Debug.WriteLine(bookmark.Title);
            if (!likesMap.ContainsKey(id))
            {
                likesMap.Add(bookmark.BookmarkId, new List<string>());
            }

            List<string> values = likesMap[id];
            // var items = likesMap.SelectMany(d => d.Value).ToList();
            string currentUser = User.Identity.GetUserId();
            if (!values.Contains(currentUser))
            {
                values.Add(currentUser);
                likesMap[id] = values;
                bookmark.Likes = likes + 1;
            }

            db.SaveChanges();
            return RedirectToAction("Index");

        }

    }
}