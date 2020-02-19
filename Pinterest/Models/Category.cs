using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pinterest.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is a required field!")]
        public string CategoryName { get; set; }

        public virtual ICollection<Bookmark> Bookmarks { get; set; }
    
    }
}