using DataAnnotationsExtensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace MiniMe.Models
{
    public class Link
    {

        public Guid LinkID { get; set; }

        public DateTime DateCreated { get; set; }

        [MaxLength(128)]
        public string ShortCode { get; set; }

        [Required(ErrorMessage = "Destination URL is required")]
        [Url]
        public string DestinationUrl { get; set; }

        public int AccessCount { get; set; }

        public DateTime LastAccessed { get; set; }

    }

    public class MiniMeContext : DbContext
    {
        public DbSet<Link> Links { get; set; }
        public DbSet<Click> Clicks { get; set; }
    }
}