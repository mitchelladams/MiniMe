using MiniMe.Models;
using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace MiniMe.Controllers
{
    public class HomeController : Controller
    {
        private MiniMeContext db = new MiniMeContext();
        private string BaseURL = ConfigurationManager.AppSettings["BaseURL"].ToString();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
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

            try
            {
                //Record a click
                Click c = new Click();
                c.ClickID = Guid.NewGuid();
                c.ClientDevice = Request.UserAgent;
                c.ClientIP = Request.UserHostAddress;
                c.DateCreated = DateTime.Now;
                c.DestinationUrl = link.DestinationUrl.ToLower();
                c.ShortCodeUsed = shortCode;
                if (User.Identity.IsAuthenticated) c.UserID = User.Identity.Name;
                db.Entry(c).State = EntityState.Added;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                //TODO: Handle the exception better
            }

            return RedirectPermanent(link.DestinationUrl);
        }

        /// <summary>
        /// Main controller to create a shortened URL.  
        /// </summary>
        /// <param name="link"></param>
        /// <returns>JsonpResult</returns>
        public JsonpResult Shorten(Link link)
        {
            if (!string.IsNullOrEmpty(link.DestinationUrl))
            {
                if (IsUrlValid(link.DestinationUrl)) //Check to see if the URL is valid.
                {
                    //This will test to see if the user wants to use a custom code by passing in an ID value in the URL
                    //Example Shorten\mycode\callback?
                    if (String.IsNullOrEmpty(link.ShortCode) && RouteData.Values["id"] != null)
                    {
                        link.ShortCode = RouteData.Values["id"].ToString();
                    }

                    //Check to see if short code in use
                    if (!String.IsNullOrEmpty(link.ShortCode))
                    {
                        //If short code is found in database, notify the user.
                        if (db.Links.Where(i => i.ShortCode == link.ShortCode).Count() > 0) return GetLinkAsJSONP(null, "The short code " + link.ShortCode + " already exists.");
                    }

                    Link ExistLink = db.Links.SingleOrDefault(l => l.DestinationUrl.Trim().ToLower() == link.DestinationUrl.Trim().ToLower());
                    if (ExistLink != null)
                    {
                        return GetLinkAsJSONP(ExistLink, "");
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
                        return GetLinkAsJSONP(newLink, "");
                    }
                }
            }

            return GetLinkAsJSONP(null, "Please provide a valid URL such as http://www.google.com.");

        }

        /// <summary>
        /// Returns a link as JSONP
        /// </summary>
        /// <param name="link"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private JsonpResult GetLinkAsJSONP(Link link, string message)
        {
            JsonpResult result = new JsonpResult();
            if (link != null)
            {
                var data = new
                {
                    Success = true,
                    ShortURL = this.BaseURL + link.ShortCode,
                    AccessCount = link.AccessCount,
                    OriginalURL = link.DestinationUrl
                };
                return new JsonpResult(data);
            }
            else
            {
                var data = new
                {
                    Success = false,
                    Message = message
                };
                return new JsonpResult(data);
            }
        }

        /// <summary>
        /// Closes all our database connections
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        #region Utilities

        /// <summary>
        /// Creates and returns an unused short code.
        /// </summary>
        /// <returns></returns>
        private string CreateUnusedShortCode()
        {
            for (int x = 0; x < 20; x++) //try 20 times to generate a code
            {
                string newKey = CreateRandomAlphaNumericSequence(5);
                if (db.Links.Where(i => i.ShortCode == newKey).Count() == 0)
                    return newKey;
            }
            throw new Exception("No code could be created.");
        }

        /// <summary>
        /// Generates a random sequence of characters
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private string CreateRandomAlphaNumericSequence(int length)
        {
            String _allowedChars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
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

        /// <summary>
        /// Basic method to validate a string for a URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool IsUrlValid(string url)
        {
            return Regex.IsMatch(url, @"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
        }

        #endregion

    }
}
