using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;


namespace MiniMe.Models
{
    public class LinkInitializer : DropCreateDatabaseIfModelChanges<LinkDBContext>
    {
        protected override void Seed(LinkDBContext context)
        {
            var links = new List<Link>
            {
                new Link { AccessCount = 0,
                            DateCreated = DateTime.Now,
                            LastAccessed = DateTime.Now,
                            DestinationUrl = "http://my.pba.edu",
                            LinkID = Guid.NewGuid(),
                            ShortCode = "abcde"
                },

                new Link { AccessCount = 0,
                            DateCreated = DateTime.Now,
                            LastAccessed = DateTime.Now,
                            DestinationUrl = "http://www.google.com",
                            LinkID = Guid.NewGuid(),
                            ShortCode = "fghij"
                },

                new Link { AccessCount = 0,
                            DateCreated = DateTime.Now,
                            LastAccessed = DateTime.Now,
                            DestinationUrl = "http://www.yahoo.com",
                            LinkID = Guid.NewGuid(),
                            ShortCode = "klmno"
                }

            };


            links.ForEach(d => context.Links.Add(d));

        }
    }
}