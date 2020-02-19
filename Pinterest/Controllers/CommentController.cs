using Microsoft.AspNet.Identity;
using Pinterest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Pinterest.Controllers
{
    public class CommentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult New()
        {
            Comment comment = new Comment();
            return View(comment);
        }

        [Authorize(Roles = "Editor,Administrator")]
        [HttpPost]
        public ActionResult New(Comment comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    comment.UserId = User.Identity.GetUserId();
                    comment.UserName = db.Users.Find(comment.UserId).UserName;
                   // comment.UserName = Membership.GetUser(comment.UserId).UserName;
                    db.Comments.Add(comment);
                    db.SaveChanges();
                    TempData["message"] = "The new comment has been added";
                    return RedirectToAction("Show", "Bookmark", new { id = comment.BookmarkId });
                }
                else
                {
                  
                    return RedirectToAction("Show", "Bookmark", new { id = comment.BookmarkId });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Show", "Bookmark", new { id = comment.BookmarkId });
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Comment comment = db.Comments.Find(id);
            if (User.IsInRole("Administrator"))
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
                TempData["message"] = "The comment has been removed!";
                return RedirectToAction("Show", "Bookmark", new { id = comment.BookmarkId });
            }
            else
            {

                return RedirectToAction("Show", "Bookmark", new { id = comment.BookmarkId });
            }
        }
    }
}