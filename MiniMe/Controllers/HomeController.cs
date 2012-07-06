﻿using System.Web.Mvc;
using MiniMe.Models;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;
using System.Data.Entity;
using System.Data;
using System.Web;

namespace MiniMe.Controllers
{
    public class HomeController : Controller
    {
        private LinkDBContext db = new LinkDBContext();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Link link)
        {
            if (ModelState.IsValid)
            {
                //Check to see if the URL already exists in the database
                Link ExistLink = db.Links.SingleOrDefault(l => l.DestinationUrl.ToLower() == link.DestinationUrl.ToLower());

                if (ExistLink != null)
                {
                    return View(ExistLink);
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
                    return View(link);
                }
            }

            return View(link);
        }

      
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

            link.AccessCount += 1;
            link.LastAccessed = DateTime.Now;
            db.Entry(link).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectPermanent(link.DestinationUrl);
        }


        //TODO: Limit to IP range, error checking
        public JsonResult Shorten()
        {
            string ReferrerURL = Request.UrlReferrer.ToString().ToLower();

            Link ExistLink = db.Links.SingleOrDefault(l => l.DestinationUrl.ToLower() == ReferrerURL);

            string ShortenedURL = "http://localhost:58342/";

            if (ExistLink != null)
            {
                ShortenedURL += ExistLink.ShortCode;
            }
            else
            {
                Link link = new Link();
                link.LinkID = Guid.NewGuid();
                link.DestinationUrl = Request.UrlReferrer.ToString();
                link.AccessCount = 0;
                link.DateCreated = DateTime.Now;
                link.LastAccessed = DateTime.Now;
                link.ShortCode = CreateUnusedShortCode();
                db.Links.Add(link);
                db.SaveChanges();
                ShortenedURL += link.ShortCode;
            } 

            return Json(new { ShortenedURL = ShortenedURL }, JsonRequestBehavior.AllowGet);
        }

        private string CreateUnusedShortCode()
        {           
            for (int x = 0; x < 10; x++)
            {
                string newKey = CreateRandomAlphaNumericSequence(5);
                if (db.Links.Where(i => i.ShortCode == newKey).Count() == 0)
                    return newKey;
            }
            throw new Exception();
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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        
    }
}