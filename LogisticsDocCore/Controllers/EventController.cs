using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using MVCIdentityConfirm.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Configuration;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LogisticsDocCore.Model;
using LogisticsDocCore.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;

namespace LogisticsDocCore.Controllers
{
    public class EventController : Controller
    {
        private readonly IDocRepository _docRepository;
        private readonly IUserRepository _usersRepository;
        private readonly Model.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;

        public EventController(Model.ApplicationDbContext context, IDocRepository docRepository, IUserRepository usersRepository, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IServiceProvider services, IConfiguration configuration, IHostEnvironment env)
        {
            _docRepository = docRepository;
            _context = context;
            _usersRepository = usersRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _services = services;
            _configuration = configuration;
            _env = env;
        }
        [Authorize]
        public ActionResult Index(string sortOrder, string searchString, string searchOrganizer, DocTypesNullable searchType = DocTypesNullable.Wszystkie_Typy, DocStatusesNullable searchStatus = DocStatusesNullable.Wszystkie_Statusy, short searchYear = 0, short searchMonth = 0, short searchWeek = 0, short PageNo = 0)
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
                .HttpContext.Session;
            ViewDocsList usersEvents = new ViewDocsList(_configuration);
            searchOrganizer = ((searchOrganizer != "") ? searchOrganizer : null);
            searchString = ((searchString != "") ? searchString : null);
            //searchType = ((searchType != "") ? searchString : null);
            string prevSearchString, prevSearchOrganizer, prevSortOrder;
            short prevSearchYear, prevSearchMonth, prevSearchWeek, prevPageNo;            
            DocTypesNullable prevSearchType = (DocTypesNullable)(session.GetInt32("SearchType") != null ? (DocTypesNullable)session.GetInt32("SearchType") : DocTypesNullable.Wszystkie_Typy);
            DocStatusesNullable prevSearchStatus = (DocStatusesNullable)(session.GetInt32("SearchStatus") != null ? (DocStatusesNullable)session.GetInt32("SearchStatus") : DocStatusesNullable.Wszystkie_Statusy);
            prevSearchString = session.GetString("SearchString") != null ? session.GetString("SearchString") : null;
            prevSearchOrganizer = session.GetString("SearchOrganizer") != null ? session.GetString("SearchOrganizer"): null;
            prevSearchYear = (short) (session.GetInt32("SearchYear") != null ? (short) session.GetInt32("SearchYear") : 0);
            prevSearchMonth = (short)(session.GetInt32("SearchMonth") != null ? (short) session.GetInt32("SearchMonth") : 0);
            prevSearchWeek = (short)(session.GetInt32("SearchWeek") != null ? (short)session.GetInt32("SearchWeek") : 0);
            prevPageNo = (short)(session.GetInt32("PageNo") != null ? (short)session.GetInt32("PageNo") : 0);
            prevSortOrder = session.GetString("sortOrder") != null ? session.GetString("sortOrder") : null;
            if (prevSearchString != searchString || prevSearchOrganizer != searchOrganizer || (prevSortOrder != sortOrder && sortOrder!= null) || prevSearchType != searchType || 
                    prevSearchStatus != searchStatus || prevSearchYear != searchYear || prevSearchMonth != searchMonth || prevSearchWeek != searchWeek) PageNo = 1;
            if (PageNo != prevPageNo && searchMonth == 0 && searchOrganizer == null && searchStatus == DocStatusesNullable.Wszystkie_Statusy && searchString == null 
                && searchType == DocTypesNullable.Wszystkie_Typy && searchWeek == 0 && searchYear == 0)
            {
                searchMonth = prevSearchMonth;
                searchOrganizer = prevSearchOrganizer;
                searchStatus = prevSearchStatus;
                searchString = prevSearchString;
                searchType = prevSearchType;
                searchWeek = prevSearchWeek;
                searchYear = prevSearchYear;
                sortOrder = prevSortOrder;
            }
            if (PageNo == 0) PageNo = prevPageNo;
            if (PageNo == 0) PageNo = 1;
          
            session.SetString("SearchString",searchString);
            session.SetInt32("SearchYear", searchYear);
            session.SetInt32("SearchMonth",searchMonth);
            session.SetInt32("SearchWeek",searchWeek);
            session.SetString("SearchOrganizer",searchOrganizer);
            session.SetInt32("SearchType", (int)searchType);
            session.SetInt32("SearchStatus", (int)searchStatus);
            session.SetInt32("PageNo",PageNo);
            session.SetString("sortOrder",sortOrder);
            ViewBag.NameSortParm = sortOrder == "name_desc" ? "name" : "name_desc";
            ViewBag.DateSortParm = sortOrder == "date_desc" ? "date" : "date_desc";
            ViewBag.CountrySortParm = sortOrder == "country_desc" ? "country" : "country_desc";
            ViewBag.ImageFolder = Convert.ToString(_configuration.GetValue<string>("AppData:ImageFolder"));
            ViewBag.UrlFull = ViewDocsList.GetUrl(true, _services);
            ViewBag.Url = ViewDocsList.GetUrl(false, _services);
            using (_context)
            {
                var us = _context.AppUsers.Find(User.Identity.Name);
                Boolean CMU = us.CanManageUsers;
                Boolean CAE = us.CanCreateEvent;
                List<Object> list = new List<object>();
                var org = _context.Organizers.FirstOrDefault();
                list.Add(new { Code = org.Organizer1, Description = org.OrganizerName });                
                usersEvents.Organizers = new SelectList(list, "Code", "Description");

                var nearEv = _context.Docs.Where(x => x.CreatedByLogin == User.Identity.Name || CMU || us.IsAccounting);
                if (!String.IsNullOrEmpty(searchString))
                {
                    PageNo = 1;
                    nearEv = nearEv.Where(s => s.DocsName.Contains(searchString)
                                           || s.Description.Contains(searchString) || s.OrganizerEvent.Contains(searchString));
                }
                if (searchMonth != 0)
                {
                    PageNo = 1;
                    nearEv = nearEv.Where(s => s.DocMonth  == searchMonth);
                }
                if (searchWeek != 0)
                {
                    PageNo = 1;
                    nearEv = nearEv.Where(s => s.DocWeek == searchWeek);
                }
                if (searchYear != 0)
                {
                    PageNo = 1;
                    nearEv = nearEv.Where(s => s.DocYear  == searchYear);
                }
                if (!String.IsNullOrEmpty(searchOrganizer))
                {

                    nearEv = nearEv.Where(s => s.OrganizerEvent.Contains(searchOrganizer));
                }
                if (searchType != DocTypesNullable.Wszystkie_Typy)
                {

                    nearEv = nearEv.Where(s => s.DocType == (int) searchType);
                }
                if (searchStatus != DocStatusesNullable.Wszystkie_Statusy)
                {

                    nearEv = nearEv.Where(s => s.Status == (int)searchStatus);
                }
                switch (sortOrder)
                {
                    case "name_desc":
                        nearEv = nearEv.OrderByDescending(s => s.DocsName);
                        break;
                    case "name":
                        nearEv = nearEv.OrderBy(s => s.DocsName);
                        break;
                    case "date_desc":
                        nearEv = nearEv.OrderByDescending(s => s.StausChangeDateTime );
                        break;
                    case "country_desc":
                        nearEv = nearEv.OrderByDescending(s => s.IsForeign);
                        break;
                    case "country":
                        nearEv = nearEv.OrderBy(s => s.IsForeign);
                        break;
                    default:
                        nearEv = nearEv.OrderByDescending(s => s.StausChangeDateTime);
                        //nearEv = nearEv.OrderBy(s => s.StausChangeDateTime );
                        //nearEv = nearEv.OrderByDescending(s => s.DocYear).ThenByDescending(s => s.DocMonth).ThenByDescending(s => s.DocWeek);
                        break;
                }
                
                int q, MaxPageNo;
                MaxPageNo = Convert.ToInt32(_configuration.GetValue<string>("AppData:MaxPageNo"));
                ViewBag.MaxPage = Math.DivRem(nearEv.Count(), MaxPageNo, out q) + (q > 0 ? 1 : 0);
                ViewBag.PageNo = PageNo;                
                usersEvents.sortOrder = sortOrder==null? "date_desc": sortOrder;
                usersEvents.searchString = searchString;
                usersEvents.searchOrganizer = searchOrganizer;
                usersEvents.searchType = searchType;
                usersEvents.searchStatus = searchStatus;
                ViewBag.CAE = CAE;
                ViewBag.CMU = CMU;
                ViewBag.Result = session.GetString("result");
                session.SetString("result", null);
                usersEvents.All = nearEv.Skip(MaxPageNo * (PageNo - 1)).Take(MaxPageNo).ToList();
                return View(usersEvents);
            }
        }

        [HttpGet]
        [Authorize]
        public JsonResult Organizers(string q)
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
               .HttpContext.Session;
            List<Object> list = new List<Object>();
            using (_context)
            {
                List<Organizer> acc = new List<Organizer>();
                acc = _context.Organizers.Where(v => (v.OrganizerName.Contains(q)) || v.Organizer1.Contains(q)).ToList();
                foreach (var v in acc)
                {
                    list.Add(new { id = v.Organizer1, text = v.OrganizerName });
                }
                var data = list;
                if (q != null && q != "" && q != "  ")
                {
                    session.SetString("Organizers", JsonConvert.SerializeObject(acc));
                    session.SetString("SearchCriteria", q);
                } else
                {
                    session.SetString("Organizers",null);
                    session.SetString("SearchCriteria",null);
                }
                return Json(data);
            }

        }
        // GET: Event

        //[ValidateAntiForgeryToken]

        
        [Authorize]
        public ActionResult Modify(int ev)
        {
            
            using (_context)
            {
                string embed = "<object data=\"{0}\" type=\"application/pdf\" width=\"800px\" height=\"500px\">";
                embed += "If you are unable to view file, you can download from <a href = \"{0}\">here</a>";
                embed += " or download <a target = \"_blank\" href = \"http://get.adobe.com/reader/\">Adobe PDF Reader</a> to view the file.";
                embed += "</object>";
                Doc Dbev = _context.Docs.Find(ev);
                ViewDocs e = new ViewDocs(Dbev,_context);
                var s = _env.ContentRootPath;
                TempData["Embed"] = string.Format(embed, _env.ContentRootPath + (Convert.ToString(_configuration.GetValue<string>("AppData:ImageFolder")) + Dbev.ExternalLinkBig));
                    
                User us = _context.AppUsers.Find(User.Identity.Name);
                bool super = us.CanManageUsers;
                    
                ViewBag.DocsId = ev;
                ViewBag.ImageFolder = (Convert.ToString(_configuration.GetValue<string>("AppData:ImageFolder")));
                ViewBag.super = super;
                if (Dbev.CreatedByLogin == User.Identity.Name || us.CanManageUsers)
                    return View(e);
                else return View("NotAuthorize");
            }

        }
        [Authorize]
        public ActionResult ShowDetails(int ev)
        {           
            using (Model.ApplicationDbContext db = _context)
            {
                
                string embed = "<object data=\"{0}\" type=\"application/pdf\" width=\"800px\" height=\"500px\">";
                embed += "If you are unable to view file, you can download from <a href = \"{0}\">here</a>";
                embed += " or download <a target = \"_blank\" href = \"http://get.adobe.com/reader/\">Adobe PDF Reader</a> to view the file.";
                embed += "</object>";
                Doc Dbev = db.Docs.Find(ev);
                ViewDocs e = new ViewDocs(Dbev,db);
                TempData["Embed"] = string.Format(embed, _env.ContentRootPath + (Convert.ToString(_configuration.GetValue<string>("AppData:ImageFolder")) + Dbev.ExternalLinkBig));
                ViewBag.DocsId = ev;
                //var OrgWWW = db.Organizers.Find(Dbev.OrganizerEvent).LinkToOrganizerWWW;
                //ViewBag.OrgWWW = OrgWWW;
                ViewBag.ImageFolder = Convert.ToString(_configuration.GetValue<string>("AppData:ImageFolder"));
                return View(e);

            }

        }

        [ValidateAntiForgeryToken]        
        [Authorize]
        [HttpPost]

        public ActionResult Modify(ViewDocs Modifiedevent)
        {
            if (ModelState.IsValid)
            {
                using (Model.ApplicationDbContext db = _context)
                {
                    int j;
                    Boolean IsError = false;
                    DateTime dzis = DateTime.UtcNow.ToTimeZoneTime("Central Europe Standard Time");
                    Modifiedevent.StausChangeDate = dzis.Date;
                    Modifiedevent.StausChangeTime = dzis;
                    Doc Dbev = db.Docs.Find(Modifiedevent.DocsId);
                    User us = db.AppUsers.Find(User.Identity.Name);
                    if (Dbev.CreatedByLogin != User.Identity.Name && !us.CanManageUsers)
                        return View("NotAuthorize");
                    if (Modifiedevent.DocWeek / 4.33 > Modifiedevent.DocMonth + 1 || Modifiedevent.DocWeek / 4.33 < Modifiedevent.DocMonth - 1)
                    {
                        IsError = true;
                        ViewBag.Error = "Nie spójny nr tygodnia z nr miesiąca!";
                        ModelState.AddModelError("DocWeek", "Nie spójny nr tygodnia z nr miesiąca!");
                    }
                    Dbev.CopyFrom(Modifiedevent, _context);

                    
                    if (Modifiedevent.FileAttachBig != null)
                        if (Modifiedevent.FileAttachBig.Length > 0)
                        {
                            ImageUpload imageUpload = new ImageUpload { Width = 600 };
                            ImageResult imageResult = imageUpload.RenameUploadFile(Modifiedevent.FileAttachBig, _env);
                            if (!imageResult.Success)
                            {
                                ViewBag.Error = imageResult.ErrorMessage;
                                ModelState.AddModelError("FileAttachBig", "Nie prawidłowe rozszerzenie pliku. Importować mozna tylko pliki pdf");
                                IsError = true;
                                                                
                            }
                            if (Dbev.ExternalLinkBig != "")
                            {
                                string fullPath = _env.ContentRootPath + "~" + Dbev.ExternalLinkBig;
                                //string fullPath = Request.MapPath("~" + Dbev.ExternalLinkBig);
                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }
                            Dbev.ExternalLinkBig = imageResult.ImageName;
                        }
                        else
                        {
                            ModelState.AddModelError("FileAttachBig", "Plik jest pusty");
                            IsError = true;
                        }

                    if(!IsError)
                    {
                        j = db.SaveChanges();
                        return RedirectToAction("Index", "Event");
                    }
                    {
                        bool super = db.AppUsers.Find(User.Identity.Name).CanManageUsers;
                        var LinkOrg = db.AppUsers.Find(User.Identity.Name).LinkedToOrganizer;
                        if (LinkOrg != null && LinkOrg != "")
                        {
                            var LinkOrgDB = db.Organizers.Find(LinkOrg);
                            if (LinkOrgDB != null)
                            {
                                Modifiedevent.OrganizerEvent = LinkOrgDB.Organizer1;
                                Modifiedevent.OrganizerName = LinkOrgDB.OrganizerName;
                            }
                        }
                        ViewBag.LinkOrg = LinkOrg;
                        ViewBag.super = super;
                        return View(Modifiedevent);
                    }
 
                    
                }
            }
            else return View(Modifiedevent);
        }
        [Authorize]
        public ActionResult Create()
        {           
            ViewDocs ev = new ViewDocs(_context);
            DateTime dzis = DateTime.UtcNow.ToTimeZoneTime("Central Europe Standard Time");
            ev.StausChangeDate = dzis.Date;
            ev.StausChangeTime = dzis;
            ev.DocYear = (Int16) dzis.Year;
            ev.DocMonth = (Int16) dzis.Month;
            ev.DocWeek = (Int16)(dzis.Month * 4.33);
            using (Model.ApplicationDbContext db = _context)
            {
                bool super = db.AppUsers.Find(User.Identity.Name).CanManageUsers;
                var LinkOrg = db.AppUsers.Find(User.Identity.Name).LinkedToOrganizer;
                if (LinkOrg != null && LinkOrg != "")
                {
                    var LinkOrgDB = db.Organizers.Find(LinkOrg);
                    if (LinkOrgDB != null)
                    {
                        ev.OrganizerEvent = LinkOrgDB.Organizer1;
                        ev.OrganizerName = LinkOrgDB.OrganizerName;
                    }
                }
                ViewBag.LinkOrg = LinkOrg;
                ViewBag.super = super;
            }
            return View("Create", ev);
        }

        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public ActionResult Create(ViewDocs ev)
        {
            if (ModelState.IsValid)
            {
                Boolean IsError = false;
                DateTime dzis = DateTime.UtcNow.ToTimeZoneTime("Central Europe Standard Time");
                ev.StausChangeDate = dzis.Date;
                ev.StausChangeTime = dzis;
                ev.CreatedByLogin = User.Identity.Name;
                ev.Status = DocStatuses.Nowy;
                Doc dbEv = new Doc(ev);
                using (Model.ApplicationDbContext db = _context)
                {
                    int j;
                    if (ev.DocWeek / 4.33 > ev.DocMonth + 1 || ev.DocWeek / 4.33 < ev.DocMonth - 1)
                    {
                        IsError = true;
                        ViewBag.Error = "Nie spójny nr tygodnia z nr miesiąca!";
                        ModelState.AddModelError("DocWeek", "Nie spójny nr tygodnia z nr miesiąca!");
                    }
                    if (ev.FileAttachBig != null)
                        if (ev.FileAttachBig.Length > 0)
                        {
                            ImageUpload imageUpload = new ImageUpload { Width = 600 };
                            ImageResult imageResult = imageUpload.RenameUploadFile(ev.FileAttachBig, _env);
                            if (!imageResult.Success)
                            {
                                ViewBag.Error = imageResult.ErrorMessage;
                                ModelState.AddModelError("FileAttachBig", "Nie prawidłowe rozszerzenie pliku. Importować mozna tylko pliki pdf");
                                IsError = true;
                            }
                            if (dbEv.ExternalLinkBig != "" && !IsError)
                            {
                                string fullPath = _env.ContentRootPath + "~" + dbEv.ExternalLinkBig;
                                //string fullPath = Request.MapPath("~" + dbEv.ExternalLinkBig);
                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                                dbEv.ExternalLinkBig = imageResult.ImageName;
                            }
                            
                        } else
                        {
                            ModelState.AddModelError("FileAttachBig", "Plik jest pusty");
                            IsError = true;
                        }
                    else
                    {
                        ModelState.AddModelError("FileAttachBig", "Nie wskazałeś pliku pdf");
                        IsError = true;
                    }
                    if (!IsError)
                    {
                        db.Docs.Add(dbEv);
                        j = db.SaveChanges();
                        return RedirectToAction("Index", "Event");
                    } else
                    {
                        bool super = db.AppUsers.Find(User.Identity.Name).CanManageUsers;
                        var LinkOrg = db.AppUsers.Find(User.Identity.Name).LinkedToOrganizer;
                        if (LinkOrg != null && LinkOrg != "")
                        {
                            var LinkOrgDB = db.Organizers.Find(LinkOrg);
                            if (LinkOrgDB != null)
                            {
                                ev.OrganizerEvent = LinkOrgDB.Organizer1;
                                ev.OrganizerName = LinkOrgDB.OrganizerName;
                            }
                        }
                        ViewBag.LinkOrg = LinkOrg;
                        ViewBag.super = super;
                    }
                }
            }
            return View(ev);
        }
        [Authorize]
        public ActionResult Cancel(int ev)
        {           
            using (Model.ApplicationDbContext db = _context)
            {
                int j;
                Doc Dbev = db.Docs.Find(ev);
                User us = db.AppUsers.Find(User.Identity.Name);
                if (Dbev.CreatedByLogin != User.Identity.Name && !us.CanManageUsers)
                    return View("NotAuthorize");
                ViewDocs e = new ViewDocs(Dbev,db);
                ViewBag.DocsId = ev;
                return View(e);
            }            
        }
        [Authorize]
        [HttpPost]
        public ActionResult Cancel(ViewDocs Deletedevent)
        {          
            if (Deletedevent.DocsId != 0)
            {
                using (Model.ApplicationDbContext db = _context)
                {
                    int j;
                    Doc Dbev = db.Docs.Find(Deletedevent.DocsId);
                    User us = db.AppUsers.Find(User.Identity.Name);
                    if (Dbev.CreatedByLogin != User.Identity.Name && !us.CanManageUsers)
                        return View("NotAuthorize");
                    
                    var OtEv = db.Docs.Where(d => d.DocsId != Deletedevent.DocsId && (d.ExternalLinkBig == Deletedevent.ExternalLinkBig)).FirstOrDefault();
                    string fullPath2 = _env.ContentRootPath + "~" + Dbev.ExternalLinkBig;
                    //string fullPath2 = Request.MapPath("~" + Dbev.ExternalLinkBig);
                        if (System.IO.File.Exists(fullPath2))
                        {
                            System.IO.File.Delete(fullPath2);
                        }
                       
                    db.Docs.Remove(Dbev);
                    j = db.SaveChanges();
                   
                     return RedirectToAction("Index", "Event");
                }
            }
            else return View(Deletedevent);
        }
        [Authorize]
        public ActionResult Check(int ev)
        {          
            if (ev != 0)
            {
                using (Model.ApplicationDbContext db = _context)
                {
                    int j;
                    DateTime dzis = DateTime.UtcNow.ToTimeZoneTime("Central Europe Standard Time");
                    Doc Dbev = db.Docs.Find(ev);
                    User us = db.AppUsers.Find(User.Identity.Name);
                    if (Dbev.CreatedByLogin != User.Identity.Name && !us.CanManageUsers)
                        return View("NotAuthorize");
                    if (Dbev.IsForeign) return RedirectToAction("Send", "Event",new { ev = ev});
                    Dbev.Status = (int)DocStatuses.Sprawdzony;
                    Dbev.StausChangeDateTime = dzis;
                    j = db.SaveChanges();
                    
                }
            }
            return RedirectToAction("Index", "Event");
        }
        [Authorize]
        public ActionResult Uncheck(int ev)
        {
            if (ev != 0)
            {
                using (Model.ApplicationDbContext db = _context)
                {
                    int j;
                    DateTime dzis = DateTime.UtcNow.ToTimeZoneTime("Central Europe Standard Time");
                    Doc Dbev = db.Docs.Find(ev);
                    User us = db.AppUsers.Find(User.Identity.Name);
                    if (Dbev.CreatedByLogin != User.Identity.Name && !us.CanManageUsers)
                        return View("NotAuthorize");
                    Dbev.Status = (int)DocStatuses.Niepoprawny;
                    Dbev.StausChangeDateTime = dzis;
                    j = db.SaveChanges();
                    return RedirectToAction("SendEmail", "Event", new { ev_id = ev, login = Dbev.CreatedByLogin, Act = "Uncheck", Act2 = "", Wn = "Index" });

                }
            }
            return RedirectToAction("Index", "Event");
        }
        [Authorize]
        public ActionResult Correct(int ev)
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
                .HttpContext.Session;
            if (ev != 0)
            {
                using (Model.ApplicationDbContext db = _context)
                {
                    int j;
                    DateTime dzis = DateTime.UtcNow.ToTimeZoneTime("Central Europe Standard Time");
                    Doc Dbev = db.Docs.Find(ev);
                    User us = db.AppUsers.Find(User.Identity.Name);
                    if (Dbev.CreatedByLogin != User.Identity.Name && !us.CanManageUsers)
                        return View("NotAuthorize");
                    Dbev.Status = (int)DocStatuses.Nowy;
                    Dbev.StausChangeDateTime = dzis;
                    j = db.SaveChanges();
                    us = db.AppUsers.Where(x => x.CanManageUsers).FirstOrDefault();
                    string to_login;
                    if (us != null)
                    {
                        to_login = us.Login;
                        return RedirectToAction("SendEmail", "Event", new { ev_id = ev, login = to_login, Act = "Correct", Act2 = "", Wn = "Index" });
                    }
                    else
                    {
                        session.SetString("result", "Brak ustawionego użytkownika zarządzającego");
                        return RedirectToAction("Index", "Event");
                    }

                }
            }
            return RedirectToAction("Index", "Event");
        }
        [Authorize]
        public ActionResult Send(int ev)
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
               .HttpContext.Session;
            if (ev != 0)
            {
                using (Model.ApplicationDbContext db = _context)
                {
                    int j;
                    DateTime dzis = DateTime.UtcNow.ToTimeZoneTime("Central Europe Standard Time");
                    Doc Dbev = db.Docs.Find(ev);
                    User us = db.AppUsers.Find(User.Identity.Name);
                    if (Dbev.CreatedByLogin != User.Identity.Name && !us.CanManageUsers && !us.IsAccounting)
                        return View("NotAuthorize");
                    Dbev.Status = (int)DocStatuses.Przekazany;
                    Dbev.StausChangeDateTime = dzis; 
                    j = db.SaveChanges();
                    us = db.AppUsers.Where(x => x.IsAccounting).FirstOrDefault();
                    string to_login;
                    if (us != null) { to_login = us.Login;
                        return RedirectToAction("SendEmail", "Event", new { ev_id = ev, login = to_login, Act = "Send", Act2 = "", Wn = "Index" });
                    }
                    else
                    {
                        session.SetString("result","Brak ustawionego użytkownika Jest Księgowością");
                        return RedirectToAction("Index", "Event");
                    }
                    
                }
            }
            else return RedirectToAction("Index", "Event");
        }
        [Authorize]
        public ActionResult UserData(string login, bool super = false)
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
              .HttpContext.Session;
            using (Model.ApplicationDbContext db = _context)
            {
                User DbUser = db.AppUsers.Find(User.Identity.Name);
                    
                if ((login != User.Identity.Name && login != null) && DbUser.CanManageUsers)
                    DbUser = db.AppUsers.Find(login);
                if (DbUser.CanManageUsers) super = true;
                if (DbUser == null) return View("NotAuthorize");
                ViewUsers _user = new ViewUsers(DbUser,db);
               
                ViewBag.Message = session.GetString("Message");
                ViewBag.super = super;
                session.SetString("Message",null);
                return View(_user);
            }            
        }
        [Authorize]
        [HttpPost]
        //[ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult UserData(ViewUsers vUser)
        {
            using (Model.ApplicationDbContext db = _context)
            {
                User DbUser = db.AppUsers.Find(vUser.Login);
                User CurUser = db.AppUsers.Find(User.Identity.Name);
                if (DbUser == null) return View("NotAuthorize");
                DbUser.CopyFrom(vUser);
                db.SaveChanges();
                if(CurUser.CanManageUsers && vUser.Login != User.Identity.Name)
                    return RedirectToAction("UserList", "Event");
                else return RedirectToAction("Index", "Event");
            }
            
        }

        [Authorize]
        public ActionResult SendEmail(int ev_id, string login, string Act, string Act2, string Wn)
        {
             using (Model.ApplicationDbContext db = _context)
            {
                Doc eve = db.Docs.Find(ev_id);
                var CurUs = db.AppUsers.Find(User.Identity.Name);
                if ((eve != null ) && (eve.CreatedByLogin == User.Identity.Name || CurUs.CanManageUsers || CurUs.IsAccounting))
                {
                    string id = login;
                    //if (login != User.Identity.Name) id = login; else id = eve.CreatedByLogin;
                    var owner = db.AppUsers.Find(id);
                    var you = db.AppUsers.Find(User.Identity.Name);
                    Email emi;                    
                    switch (Act)
                    {                        
                        case "Send":                            
                            emi = new Email(owner.Email, you.Email, owner.Login, you.Login, eve.DocsId, "Przekazanie dokumentu"
                            , "", Url.Action(Act, "Event", new { unev_id = ev_id, us_login = owner.Login }, Request.Scheme), Act, Act2, Wn);
                            break;
                        case "Uncheck":                            
                            emi = new Email(owner.Email, you.Email, owner.Login, you.Login, eve.DocsId, "Braki/Błędy w dokumencie"
                            , "", Url.Action(Act, "Event", new { unev_id = ev_id, us_login = owner.Login }, Request.Scheme), Act, Act2, Wn);
                            break;
                        case "Correct":
                            emi = new Email(owner.Email, you.Email, owner.Login, you.Login, eve.DocsId, "Poprawiony dokument"
                            , "", Url.Action(Act, "Event", new { unev_id = ev_id, us_login = owner.Login }, Request.Scheme), Act, Act2, Wn);
                            break;
                        case "Modify":
                            emi = new Email(owner.Email, you.Email, owner.Login, you.Login, eve.DocsId, "Modyfikacja wydarzenia"
                            , "", Url.Action(Act, "Event", new { unev_id = ev_id, us_login = you.Login }, Request.Scheme), Act, Act2, Wn);
                            break;
                        default: emi = new Email(); break;
                    }                    
                    return View(emi);
                }
                else return View("NotAuthorize");

            }
        }
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> SendEmail(Email em)
        {
            using (Model.ApplicationDbContext db = _context)
            {
                Doc ev = db.Docs.Find(em.EvID);
                if (ev != null)
                {
                    System.Net.Mail.MailMessage mail;
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(Convert.ToString(_configuration.GetValue<string>("AppData:SmtpAddress")));
                    smtp.Credentials = new System.Net.NetworkCredential(Convert.ToString(_configuration.GetValue<string>("AppData:MailingLogin")),
                        Convert.ToString(_configuration.GetValue<string>("AppData:MailingPassword")));
                    //smtp.EnableSsl = true;
                    //smtp.Port = 587;
                    //smtp.UseDefaultCredentials = false;
                    //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    string fullPath2 = _env.ContentRootPath +"~" + ev.ExternalLinkBig;
                    //string fullPath2 = Request.MapPath("~" + ev.ExternalLinkBig);
                    switch (em.Action)
                    {
                        case "Send":
                            mail = ViewDocsList.SendEmail(em.MailTo, em.MailFrom, ev, em.LoginTo, em.LoginFrom,
                                  Url.Action("ShowDetails", "Event", new { ev = em.EvID}, Request.Scheme)
                                  //Url.Action("UnAttend", "Event", new { unev_id = em.EvID, us_login = em.LoginTo }, Request.Url.Scheme)
                                  ,"",em.Subject, em.Comments, _configuration);
                            
                            if (System.IO.File.Exists(fullPath2))
                            {
                                System.Net.Mail.Attachment at = new System.Net.Mail.Attachment(fullPath2);
                                mail.Attachments.Add(at);
                            }
                            await smtp.SendMailAsync(mail);
                            break;
                        case "Uncheck":
                            mail = ViewDocsList.SendEmail(em.MailTo, em.MailFrom, ev, em.LoginTo, em.LoginFrom,
                                  Url.Action("Modify", "Event", new { ev = em.EvID }, Request.Scheme)
                                  //Url.Action("UnAttend", "Event", new { unev_id = em.EvID, us_login = em.LoginTo }, Request.Url.Scheme)
                                  , "", em.Subject, em.Comments, _configuration);
       
                            if (System.IO.File.Exists(fullPath2))
                            {
                                System.Net.Mail.Attachment at = new System.Net.Mail.Attachment(fullPath2);
                                mail.Attachments.Add(at);
                            }
                            await smtp.SendMailAsync(mail);
                            break;
                        case "Correct":
                            mail = ViewDocsList.SendEmail(em.MailTo, em.MailFrom, ev, em.LoginTo, em.LoginFrom,
                                  Url.Action("ShowDetails", "Event", new { ev = em.EvID }, Request.Scheme)
                                  //Url.Action("UnAttend", "Event", new { unev_id = em.EvID, us_login = em.LoginTo }, Request.Url.Scheme)
                                  , "", em.Subject, em.Comments, _configuration);

                            if (System.IO.File.Exists(fullPath2))
                            {
                                System.Net.Mail.Attachment at = new System.Net.Mail.Attachment(fullPath2);
                                mail.Attachments.Add(at);
                            }
                            await smtp.SendMailAsync(mail);
                            break;
                        case "Confirm":
                            mail = ViewDocsList.SendEmail(em.MailTo, em.MailFrom, ev, em.LoginTo, em.LoginFrom,
                                  Url.Action(em.Action, "Event", new { unev_id = em.EvID, us_login = em.LoginTo }, Request.Scheme),
                                  Url.Action("UnAttend", "Event", new { unev_id = em.EvID, us_login = em.LoginTo }, Request.Scheme),
                                  em.Subject, em.Comments, _configuration);
                            await smtp.SendMailAsync(mail);
                            break;                       
                        case "Delete":
                            
                            db.Docs.Remove(ev);
                            db.SaveChanges();
                            break;
                        case "Modify":
                            
                            break;
                    }
                }

                switch (em.WhereNext)
                {
                    case "MyEvent-attended": return RedirectToAction("MyEvents", "Event", new { mode = "attended" });
                    case "MyEvent-confirmed": return RedirectToAction("MyEvents", "Event", new { mode = "confirmed" });
                    case "MyEvent-sentToYou": return RedirectToAction("MyEvents", "Event", new { mode = "sentToYou" });
                    case "MyEventsInvitations": return RedirectToAction("MyEventsInvitations", "Event", new { event_id = em.EvID });
                    case "MyEvent-created": return RedirectToAction("MyEvents", "Event", new { mode = "created" });
                    case "ShowDetails": return RedirectToAction("ShowDetails", "Event", new { ev = em.EvID });
                    case "Index": return RedirectToAction("Index", "Event");
                    default: return RedirectToAction("MyEvents", "Event");

                }
            }
        }
       

      
        [Authorize]
        public ActionResult UserList()
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
              .HttpContext.Session;
            using (Model.ApplicationDbContext db = _context)
            {

                var curuser = db.AppUsers.Find(User.Identity.Name);
                if (curuser.CanManageUsers)
                {
                    AllUsers usList = new AllUsers(db);
                    if (session.GetString("Error") != null)
                    {
                        ViewBag.Error = session.GetString("Error");
                        session.SetString("Error",null);
                    }
                    return View(usList);
                }  else return View("NotAuthorize");
            }
        }
        [Authorize]
        public ActionResult ChangeCanCreateEvent(string UserLogin)
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
              .HttpContext.Session;
            using (Model.ApplicationDbContext db = _context)
            {
                var curuser = db.AppUsers.Find(User.Identity.Name);
                if (curuser.CanManageUsers)
                {
                    var us = db.AppUsers.Find(UserLogin);
                    if (us != null)
                    {
                            if (us.LinkedToOrganizer != null)
                            {
                                us.CanCreateEvent = !us.CanCreateEvent;
                                db.SaveChanges();
                            }
                            else
                            {
                                session.SetString("Error","Najpierw przypisz użytkownikowi firmę. Inaczej nie może tworzyć dokumentów!");
                            }
                    }
                    return RedirectToAction("UserList", "Event");
                } else return View("NotAuthorize");
            }
        }
        [Authorize]
        public ActionResult DeleteUser(string UserLogin)
        {
           using (Model.ApplicationDbContext db = _context)
            {
                var curuser = db.AppUsers.Find(User.Identity.Name);
                if (curuser.CanManageUsers)
                {
                    User us = db.AppUsers.Find(UserLogin);
                   // var netus = db.AspNetUsers.Where(x => x.UserName == UserLogin).FirstOrDefault();
                    if (us != null 
                        //&& netus != null
                        )
                    {
                        var UsrDocs = db.Docs.Where(x => x.CreatedByLogin == UserLogin).ToList();
                        if( UsrDocs != null)
                        {
                            //foreach(Docs doc in UsrDocs)
                            //{
                            //    db.Docs.Remove(doc);
                            //}
                            db.Docs.RemoveRange(UsrDocs);
                        }
                    //    db.AspNetUsers.Remove(netus);
                        db.AppUsers.Remove(us);
                        db.SaveChanges();
                    }
                    return RedirectToAction("UserList", "Event");
                }
                else return View("NotAuthorize");
            }
        }
        [Authorize]
        public ActionResult ChangeIsAccounting(string UserLogin)
        {
           using (Model.ApplicationDbContext db = _context)
            {
                var curuser = db.AppUsers.Find(User.Identity.Name);
                if (curuser.CanManageUsers)
                {
                    var us = db.AppUsers.Find(UserLogin);
                    if (us != null)
                    {
                        us.IsAccounting = !us.IsAccounting;
                        db.SaveChanges();
                    }
                    return RedirectToAction("UserList", "Event");
                }
                else return View("NotAuthorize");
            }
        }


       [Authorize]
        public ActionResult ShowOrganizers( string SearchCriteria, bool clearSesion = false)
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
              .HttpContext.Session;
            using (Model.ApplicationDbContext db = _context)
            {
                var curuser = db.AppUsers.Find(User.Identity.Name);
                if (curuser.CanManageUsers)
                {
                    var organizers = new List<Organizer>();
                    if (clearSesion)
                    {
                        session.SetString("Organizers",null);
                        session.SetString("SearchCriteria",null);
                    }
                    if(SearchCriteria != null) organizers = db.Organizers.Where(x => x.Organizer1.Contains(SearchCriteria) || x.OrganizerName.Contains(SearchCriteria)).ToList();

                    //if (Session["Organizers"] != null)
                    //{
                    //    organizers = (List<Organizers>)Session["Organizers"];
                    //    ViewBag.SearchCriteria = Session["SearchCriteria"];
                    //}
                    else organizers = db.Organizers.ToList();
                    if (session.GetString("Error") != null) {
                        ViewBag.Error = session.GetString("Error");
                        session.SetString("Error",null);
                            }                        
                    return View(organizers);
                }
                else return View("NotAuthorize");
            }            
        }

        [Authorize]
        public ActionResult CreateOrganizer()
        {           
            using (Model.ApplicationDbContext db = _context)
            {
                var curuser = db.AppUsers.Find(User.Identity.Name);
                if (curuser.CanManageUsers)
                {

                    return View();
                }
                else return View("NotAuthorize");
            }            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateOrganizer(Organizer or, Model.ApplicationDbContext db)
        {
            using ( db)
            {
                var curuser = db.AppUsers.Find(User.Identity.Name);
                if (curuser.CanManageUsers)
                {                    
                    if (ModelState.IsValid)
                    {
                        db.Organizers.Add(or);
                        db.SaveChanges();
                        return RedirectToAction("ShowOrganizers", "Event");
                    }
                    return View();
                }
                else return View("NotAuthorize");
            }            
        }
        [Authorize]
        public ActionResult EditOrganizer(string id, Model.ApplicationDbContext db)
        {
            using (db)
            {
                var curuser = db.AppUsers.Find(User.Identity.Name);
                if (curuser.CanManageUsers)
                {
                    Organizer or = db.Organizers.Find(id);
                    return View(or);
                }
                else return View("NotAuthorize");
            }
            
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOrganizer(Organizer or)
        {
            using (Model.ApplicationDbContext db = _context)
            {
                var curuser = db.AppUsers.Find(User.Identity.Name);
                if (curuser.CanManageUsers)
                {
                    if (ModelState.IsValid)
                    {
                        Organizer orgDB = db.Organizers.Find(or.Organizer1);
                        orgDB.CopyFrom(or);
                        db.SaveChanges();
                        return RedirectToAction("ShowOrganizers", "Event");
                    }
                    return View();
                }
                else return View("NotAuthorize");
            }            
        }
        [Authorize]
        public ActionResult DeleteOrganizer(string id)
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
              .HttpContext.Session;
            using (Model.ApplicationDbContext db = _context)
            {
                var curuser = db.AppUsers.Find(User.Identity.Name);
                if (curuser.CanManageUsers)
                {
                    var Usrorg = db.AppUsers.Where(x => x.LinkedToOrganizer == id);
                    if(Usrorg != null && Usrorg.Count() >= 1)
                    {
                        session.SetString("Error", "Są inni użytkownicy podpięci pod tą Firmę. Nie można jej usunąć");
                        return RedirectToAction("ShowOrganizers", "Event");
                    }
                    Organizer or = db.Organizers.Find(id);
                    if (or != null)
                    {
                        var docs = db.Docs.Where(x => x.OrganizerEvent == id).ToList() ;
                        if (docs != null) db.Docs.RemoveRange(docs);
                        db.Organizers.Remove(or);
                        db.SaveChanges();
                    }
                    return RedirectToAction("ShowOrganizers", "Event");
                }
                else return View("NotAuthorize");
            }            
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetOrgData(string iso3)
        {
            if (!string.IsNullOrWhiteSpace(iso3))
            {
                using (Model.ApplicationDbContext db = _context)
                {
                    Organizer OrgData = db.Organizers.Find(iso3);
                    return Json(OrgData);
                }
            }
            return null;
        }
        [HttpGet]
        
        public ActionResult About()
        {
            ViewBag.Message = "O nas";
            
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Dane kontaktowe";
            
            return View();
        }


        [Authorize]
        public ActionResult DeleteZoombeFiles()
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
              .HttpContext.Session;
            int remCount = 0;
            using (Model.ApplicationDbContext db = _context)
            {
                string fullPath = _env.ContentRootPath + "~/Images/Items/";
                //string fullPath = Request.MapPath("~/Images/Items/");
                    
                if (System.IO.Directory.Exists(fullPath))
                {
                    IEnumerable<string> fileList = Directory.EnumerateFiles(fullPath);
                    foreach (string file in fileList)
                    {
                        FileInfo f = new FileInfo(file);
                        string FileName = Path.GetFileName(file);
                        var ev = db.Docs.Where(x => x.ExternalLinkBig.Contains(FileName)).FirstOrDefault();
                            
                        if (ev == null)
                        {
                            System.IO.File.Delete(fullPath + FileName);
                            remCount++;
                        }
                    }
                }
            }            
            session.SetString("Message", "Usunięto: " + Convert.ToString(remCount));
            return RedirectToAction("UserData");
        }
        [Authorize]
        public ActionResult DeleteOldEvents()
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
              .HttpContext.Session;
            DeleteOld doinst = new DeleteOld();
            doinst.dateFrom = new DateTime(1900, 1, 1);
            DateTime dzis = DateTime.UtcNow.ToTimeZoneTime("Central Europe Standard Time");
            doinst.dateTo = dzis.Subtract(new TimeSpan(int.Parse(Convert.ToString(_configuration.GetValue<string>("AppData:ImageFolder")))*24*365, 0, 0));
            if(doinst.dateFrom > doinst.dateTo)
            {
                session.SetString("Message", "Nie ma dokumentów do usunięcia");
                return RedirectToAction("UserData");
            }
            return View(doinst);        
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]        
        public ActionResult DeleteOldEvents(DeleteOld delold)
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?
              .HttpContext.Session;
            if (ModelState.IsValid)
            {
                using (Model.ApplicationDbContext db = _context)
                {
                    if (delold.dateFrom > delold.dateTo)
                    {
                        ModelState.AddModelError(string.Empty, "Data od nie może być starsza od Daty do");
                        return View();
                    }
                    if (delold.dateTo>=DateTime.Now)
                    {
                        ModelState.AddModelError(string.Empty, "Data do nie może być starsza od dziś");
                        return View();
                    }
                    User us = db.AppUsers.Find(User.Identity.Name);
                    int j, remCount = 0;
                    var DbevL = db.Docs.ToList();
                    if (!us.CanManageUsers)
                        DbevL = DbevL.Where(x => x.StausChangeDateTime  >= delold.dateFrom && x.CreatedByLogin == User.Identity.Name && x.StausChangeDateTime <= delold.dateTo).ToList();
                    else
                        DbevL = DbevL.Where(x => x.StausChangeDateTime  >= delold.dateFrom && x.StausChangeDateTime <= delold.dateTo).ToList();
                    foreach (Doc Deletedevent in DbevL)
                    {

                        // var OtEv = db.Docs.Where(d => d.DocsId != Deletedevent.DocsId && (d.ExternalLinkBig == Deletedevent.ExternalLinkBig)).FirstOrDefault();
                        string fullPath2 = _env.ContentRootPath +"~" + Deletedevent.ExternalLinkBig;
                        //string fullPath2 = Request.MapPath("~" + Deletedevent.ExternalLinkBig);
                            if (System.IO.File.Exists(fullPath2))
                            {
                                System.IO.File.Delete(fullPath2);
                            }
           
                          
                            
                        remCount++;
                        db.Docs.Remove(Deletedevent);
                    }
                    j = db.SaveChanges();
                    session.SetString("Message","Usunięto: " + Convert.ToString(remCount));
                    return RedirectToAction("UserData", "Event");
                }
            }
            else return View();
                     
        }
    }
}