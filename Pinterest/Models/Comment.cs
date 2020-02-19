using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pinterest.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required(ErrorMessage = "The content of the comment is a required field!")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public int BookmarkId { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }

        public virtual Bookmark Bookmark { get; set; }

        public virtual ApplicationUser User { get; set; }

    }
}