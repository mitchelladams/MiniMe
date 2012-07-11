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

        /// <summary>
        /// Default view of the main page
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.BaseURL = BaseURL.Replace("http://", "").Replace("/", "");
            return View();
        }

        /// <summary>
        /// Takes a form submission and returns JSON response
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Index(Link link)
        {                                 
            if (ModelState.IsValid)
            {             
                //Check to see if the URL already exists in the database
                Link ExistLink = db.Links.SingleOrDefault(l => l.DestinationUrl.Trim().ToLower() == link.DestinationUrl.Trim().ToLower());

                if (ExistLink != null)
                {                   
                    return GetLinkAsJSON(ExistLink, true);
                }
                else
                {
                    link.LinkID = Guid.NewGuid();
                    link.DateCreated = DateTime.Now;
                    link.AccessCount = 0;
                    link.ShortCode = CreateUnusedShortCode();
                    link.LastAccessed = DateTime.Now;
                    db.Links.Add(link);
                    db.SaveChanges();

                    return GetLinkAsJSON(link, true);       
                }
            }

            return GetLinkAsJSON(null, false);
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


        public JsonResult Shorten(string url = "")
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (IsUrlValid(url))
                {
                    // the url is valid
                    Link ExistLink = db.Links.SingleOrDefault(l => l.DestinationUrl.Trim().ToLower() == url.Trim().ToLower());
                    if (ExistLink != null)
                    {
                        return GetLinkAsJSON(ExistLink, true);
                    }
                    else
                    {
                        Link link = new Link();
                        link.LinkID = Guid.NewGuid();
                        link.DestinationUrl = url;
                        link.AccessCount = 0;
                        link.DateCreated = DateTime.Now;
                        link.LastAccessed = DateTime.Now;
                        link.ShortCode = CreateUnusedShortCode();
                        db.Links.Add(link);
                        db.SaveChanges();
                        return GetLinkAsJSON(link, true);
                    }
                }                
            }
          
            return GetLinkAsJSON(null, false);             
            
        }    
        
    
        private JsonResult GetLinkAsJSON(Link link, bool isGood)
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            if (link != null && isGood)
            {
                result.Data = new
                {
                    Success = true,
                    Message = "Valid",
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
                    Message = "Invalid request. Please supply a complete url such as http://www.google.com"                    
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
