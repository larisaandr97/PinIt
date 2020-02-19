using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pinterest.Models
{
    public class Bookmark
    {
        [Key]
        public int BookmarkId { get; set; }

        [StringLength(30, ErrorMessage = "The title cannot be more than 30 characters long")]
        [Required(ErrorMessage = "The title is a required field")]
        public string Title { get; set; }

        public string FilePath { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }

        public int Likes { get; set; }

        [Required(ErrorMessage = "The category is a required field")]
        public int CategoryId { get; set; }

        public string UserId { get; set; }

        public string Tags { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual Category Category { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}