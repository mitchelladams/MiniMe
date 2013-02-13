using System;
using System.ComponentModel.DataAnnotations;

namespace MiniMe.Models
{
    public class Click
    {
        public Guid ClickID { get; set; }

        [MaxLength(128)]
        public string ShortCodeUsed { get; set; }

        public string DestinationUrl { get; set; }

        [MaxLength(64)]
        public string UserID { get; set; }

        public DateTime DateCreated { get; set; }

        public string ClientIP { get; set; }

        public string ClientDevice { get; set; }
    }
}