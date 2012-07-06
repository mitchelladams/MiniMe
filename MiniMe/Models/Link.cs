using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace MiniMe.Models
{
    public class Link
    {

        public Guid LinkID { get; set; }

        public DateTime DateCreated { get; set; }
        
        public string ShortCode { get; set; }
        
        [Required(ErrorMessage="Destination URL is required")]    
        [Url]
        public string DestinationUrl { get; set; }

        public int AccessCount { get; set; }

        public DateTime LastAccessed { get; set; }

    }

    public class LinkDBContext : DbContext
    {
        public DbSet<Link> Links { get; set; }
    }
}