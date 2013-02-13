using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;


namespace MiniMe.Models
{
    /// <summary>
    /// Executing the code in this class will cause the database to be dropped and recreated.  
    /// Execute with caution.
    /// To Enable this to run, modify the global.asax file and uncomment out the line: Database.SetInitializer<MiniMeContext>(new MiniMeInitializer()); 
    /// </summary>
    public class MiniMeInitializer : DropCreateDatabaseIfModelChanges<MiniMeContext>
    {
        protected override void Seed(MiniMeContext context)
        {
            var links = new List<Link>
            {
                new Link { AccessCount = 0,
                            DateCreated = DateTime.Now,
                            LastAccessed = DateTime.Now,
                            DestinationUrl = "http://www.redcross.org",
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
            context.SaveChanges();

            //Alter the Short Code column so queries will be case sensitive.
            //TODO: make work with a SQL CE database
            //context.Database.ExecuteSqlCommand("ALTER TABLE Links ALTER COLUMN ShortCode nvarchar(128) COLLATE Latin1_General_CS_AS");

        }
    }
}