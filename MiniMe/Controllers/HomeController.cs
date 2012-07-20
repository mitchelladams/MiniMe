using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using MiniMe.Models;

namespace MiniMe.Controllers
{
    public class HomeController : Controller
    {
        private LinkDBContext db = new LinkDBContext();
        private string BaseURL = ConfigurationManager.AppSettings["BaseURL"].ToString();

        public ActionResult Index()
        {            
            return View();
        }

        public ActionResult Create()
        {
            if (RouteData.Values["id"] != null)
            {
                Link l = new Link();
                l.ShortCode = RouteData.Values["id"].ToString();
                return View(l);
            }           
            return View();
        }       
  
      
        /// <summary>
        /// Checks the ID of the route against the database and goes from there.
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDestination()
        {                               
            string shortCode = RouteData.Values["id"].ToString();

            if (string.IsNullOrEmpty(shortCode)) return View("Index");

            Link link = db.Links.SingleOrDefault(l => l.ShortCode == shortCode);
            
            if (link == null)
            {
                ViewBag.ShortCode = shortCode;                
                return View("NotFound");
            }

            link.AccessCount++;
            link.LastAccessed = DateTime.Now;
            db.Entry(link).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectPermanent(link.DestinationUrl);
        }

        [HttpPost]
        public JsonResult Shorten(Link link)
        {         
            if (!string.IsNullOrEmpty(link.DestinationUrl))
            {
                if (IsUrlValid(link.DestinationUrl))
                {
                    //URL is valid
                    //Check to see if short code in use
                    if (!String.IsNullOrEmpty(link.ShortCode))
                    {
                        if (db.Links.Where(l => l.ShortCode == link.ShortCode).Count() > 0) return GetLinkAsJSON(null, "The short code " + link.ShortCode + " already exists.");
                    }
                    
    
                    Link ExistLink = db.Links.SingleOrDefault(l => l.DestinationUrl.Trim().ToLower() == link.DestinationUrl.Trim().ToLower());
                    if (ExistLink != null)
                    {
                        return GetLinkAsJSON(ExistLink, "");
                    }
                    else
                    {
                        Link newLink = new Link();
                        newLink.LinkID = Guid.NewGuid();
                        newLink.DestinationUrl = link.DestinationUrl;
                        newLink.AccessCount = 0;
                        newLink.DateCreated = DateTime.Now;
                        newLink.LastAccessed = DateTime.Now;

                        if (String.IsNullOrEmpty(link.ShortCode))
                        {
                            newLink.ShortCode = CreateUnusedShortCode();
                        }
                        else
                        {                          
                            newLink.ShortCode = link.ShortCode;                           
                        }
                        db.Links.Add(newLink);
                        db.SaveChanges();
                        return GetLinkAsJSON(newLink, "");
                    }
                }                
            }
          
            return GetLinkAsJSON(null, "Please provide a valid URL such as http://www.google.com.");             
            
        }    
        
    
        private JsonResult GetLinkAsJSON(Link link, string message)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            if (link != null)
            {
                result.Data = new
                {
                    Success = true,                   
                    ShortURL = this.BaseURL + link.ShortCode,
                    AccessCount = link.AccessCount,
                    OriginalURL = link.DestinationUrl                    
                };
            }
            else
            {
                result.Data = new
                {
                    Success = false,
                    Message = message                    
                };
            }
            return result;           

        }
                

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        #region Utilities

        private string CreateUnusedShortCode()
        {
            for (int x = 0; x < 20; x++) //try 20 times to generate a code
            {
                string newKey = CreateRandomAlphaNumericSequence(5);
                if (db.Links.Where(i => i.ShortCode == newKey).Count() == 0)
                    return newKey;
            }
            throw new Exception("A short code could not be generated.");
        }

 


        private string CreateRandomAlphaNumericSequence(int length)
        {
            String _allowedChars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            Byte[] randomBytes = new Byte[length];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);
            char[] chars = new char[length];
            int allowedCharCount = _allowedChars.Length;

            for (int i = 0; i < length; i++)
            {
                chars[i] = _allowedChars[(int)randomBytes[i] % allowedCharCount];
            }
            return new string(chars);
        }


        private bool IsUrlValid(string url)
        {
            return Regex.IsMatch(url, @"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
        }

        #endregion

    }
}
