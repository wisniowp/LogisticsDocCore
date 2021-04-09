using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace LogisticsDocCore.Model
{
   

    public static class StringExtensions
    {
        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }
        public static DateTime ToTimeZoneTime(this DateTime time, string timeZoneId = "Central Europe Standard Time")
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return time.ToTimeZoneTime(tzi);
        }
        public static DateTime ToTimeZoneTime(this DateTime time, TimeZoneInfo tzi)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(time, tzi);
        }
    }

    public enum Months {Styczeń,Luty,Marzec,Kwiecień,Maj,Czerwiec,Lipiec,Sierpień,Wrzesień,Październik,Listopad,Grudzień}
    public enum DocTypes { Komplet_Dokumentow, CMR_WZ, Aneks_7, Potwierdzenie_Odbioru, Kwit_Wagowy, Inne}
    public enum DocTypesNullable { Komplet_Dokumentow, CMR_WZ, Aneks_7, Potwierdzenie_Odbioru, Kwit_Wagowy, Inne, Wszystkie_Typy }
    public enum DocStatuses { Nowy, Sprawdzony, Niepoprawny, Przekazany}
    public enum DocStatusesNullable { Nowy, Sprawdzony, Niepoprawny, Przekazany, Wszystkie_Statusy }
    public class AllUsers
    {
        public List<User> AllUs { get; set; }
        public AllUsers(ApplicationDbContext db)
        {
            using (db)
            {
                var usersAll = db.AppUsers;
                this.AllUs = usersAll.ToList();
            }
        }
    }
    public class ViewDocsList
    {
        public List<Doc> All { get; set; }
       
        public string mode { get; set; }        
        public string sortOrder { get; set; }
        public string searchString { get; set; }
        public string searchOrganizer { get; set; }
        public DocTypesNullable searchType { get; set; }
        public DocStatusesNullable searchStatus { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
        public bool CanAddEvent { get; set; }
        public SelectList Organizers { get; set; }
        public int PageNo { get; set; }
        public int MaxPageNo { get; set; }



        public static string GetUrl(bool isFull, IServiceProvider _services)
        {
            HttpContext httpContext = _services.GetRequiredService<IHttpContextAccessor>()?
                .HttpContext;
            if (isFull) return httpContext.Request.Host + httpContext.Request.Path;
            else
            {
                return httpContext.Request.Host + httpContext.Request.Path;

                //if (HttpContext.Current.Request.Url.ToString().IndexOf(HttpContext.Current.Request.RawUrl.ToString()) != -1)
                //    return HttpContext.Current.Request.Url.ToString().Substring(0, HttpContext.Current.Request.Url.ToString().IndexOf(HttpContext.Current.Request.RawUrl.ToString()));
                //else return HttpContext.Current.Request.Url.ToString();
            }

        }      

        public static System.Net.Mail.MailMessage SendEmail(string MailTo, string MailFrom, Doc ve, string LoginTo, string LoginFrom, string url, string url2, string subject, string comments, IConfiguration config)
        {


            System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
                            new System.Net.Mail.MailAddress(Convert.ToString(config.GetValue<string>("AppData:MailingLogin")), "Dokumentacja Transpotowa"),
                            new System.Net.Mail.MailAddress(MailTo));
           // m.Subject = "Udział w wydarzeniu wymagający Twojej akceptacji";
            m.Subject = subject;
            //m.Body = Event.Models.mailTemp.body1;
            switch (subject) {
                  case "Przekazanie dokumentu": m.Body = m.Body + string.Format("Hej, </span></p><p class='csC8F6D76'>{1} przekazuje dokumentację transportową {4} od firmy {5} z komentarzem: {6} </p> <p> By zobaczycz szczegóły zaloguj się i przejdź: <a href=\"{2}\" title=\"Szczegóły dokumentu w portalu\">{2}</a></p>"
                    , LoginTo, LoginFrom, url, url2, ve.DocsName, ve.OrganizerName, comments);
                    m.Subject = m.Subject + " od firmy transp.: " + ve.OrganizerName + ", dot dokumentu: " + ve.ExternalLinkBig.Replace("/Images/Items/","");
                    break;
                case "Braki/Błędy w dokumencie":
                    m.Body = m.Body + string.Format("Szanowny {0}, </span></p><p class='csC8F6D76'> Prosimy o poprawienie dokumentów {4}. Użytkownik {1} zwrócił je do poprawienia z komentarzem: {6} </p> <p> By zobaczycz szczegóły zaloguj się i przejdż: <a href=\"{2}\" title=\"Szczegóły dokumentu w portalu\">{2}</a></p>"
                    , LoginTo, LoginFrom, url, url2, ve.DocsName, ve.OrganizerName, comments);
                    break;
                case "Poprawiony dokument":
                    m.Body = m.Body + string.Format("Szanowny {0}, </span></p><p class='csC8F6D76'> Użytkownik {1} zwrócił poprawione dokumenty {4} dot firmy {5}. Od siebie dał komentarz: {6} </p> <p> By zobaczycz szczegóły zaloguj się i przejdź: <a href=\"{2}\" title=\"Szczegóły dokumentu w portalu\">{2}</a></p>"
                    , LoginTo, LoginFrom, url, url2, ve.DocsName, ve.OrganizerName, comments);
                    break;
                case "Udział w wydarzeniu wymagający Twojej akceptacji": m.Body = m.Body + string.Format("Drogi {0}</span></p><p class='csC8F6D76'>{1} zapisał się na Twoje wydarzenie {4} wymagające akceptacji. Od siebie dodał to: {5}. Proszę zaloguj się aby dać mu: </p> <p> Potwierdzenie: <a href=\"{2}\" title=\"Potwierdzenie udziału w wydarzeniu\">{2}</a></p> <p>Anulowanie: <a href=\"{3}\" title=\"Potwierdzenie udziału w wydarzeniu\">{3}</a></p>"
                    , LoginTo, LoginFrom, url, url2, ve.DocsName, comments);
                    break;
                case "Zaproszenie na wydarzenie": m.Body = m.Body + string.Format("Drogi {0}</span></p><p class='cs2654AE3A'>{1} chce zaprosić Ciebie na wydarzenie {4}. <BR/>Od siebie dodał to: {5}. <BR/>Proszę zaloguj się aby dać mu: </p> <p> Potwierdzenie: <a href=\"{2}\" title=\"Potwierdzenie zaproszenia\">{2}<span class=\"glyphicon glyphicon-check\"></span></a></p> <p>Anulowanie: <a href=\"{3}\" title=\"Potwierdzenie udziału w wydarzeniu\">{3}<span class=\"glyphicon glyphicon-ban-circle\"></span></a></p>"
                    , LoginTo, LoginFrom, url, url2, ve.DocsName, comments);
                    break;
                case "Cofnięcie zaproszenia": m.Body = m.Body + string.Format("Drogi {0}</span></p><p class='cs2654AE3A'>{1} cofnął zaproszenie na wydarzenie {3}. <BR/>Od siebie napisał: {4}. <BR/>Zobacz swoje zaproszenia <a href=\"{2}\" title=\"Twoje zaproszenia na wydarzenia\">{2}</a>", LoginTo, LoginFrom, url, ve.DocsName, comments);
                    break;
                case "Przyjęcie zaproszenia na wydarzenie": m.Body = m.Body + string.Format("Drogi {0}</span></p><p class='cs2654AE3A'>{1} Przyjął Twoje zaproszenie na wydarzenie {3}. <BR/>Od siebie napisał: {4}. <BR/>Zarządzaj swoimi uczestnictwami <a href=\"{2}\" title=\"Twoje zaproszenia na wydarzenia\">{2}</a>", LoginTo, LoginFrom, url, ve.DocsName, comments);
                    break;
                case "Rezygnacja z udziału w wydarzeniu": m.Body = m.Body + string.Format("Drogi {0}</span></p><p class='cs2654AE3A'>{1} Zrezygnował z Twojego zaproszenia na wydarzenie {3}. <BR/>Od siebie napisał: {4}. <BR/>Zarządzaj swoimi uczestnictwami tutaj <a href=\"{2}\" title=\"Twoje uczestnictwa w wydarzeniach\">{2}</a>", LoginTo, LoginFrom, url, ve.DocsName, comments);
                    break;
                case "Anulowanie wydarzenia": m.Body = m.Body + string.Format("Drogi {0}</span></p><p class='cs2654AE3A'>{1} anulował wydarzenie {3}. <BR/>Od siebie napisał: {4}. <BR/>Zobacz swoje zaproszenia <a href=\"{2}\" title=\"Twoje zaproszenia na wydarzenia\">{2}</a>", LoginTo, LoginFrom, url, ve.DocsName, comments);
                    break;                
        }
            //if (subject != "Przekazanie dokumentu") m.Body = m.Body + Event.Models.mailTemp.body3; else m.Body = m.Body + Event.Models.mailTemp.body4;
            m.Body = m.Body + mailTemp.body4;

            m.IsBodyHtml = true;
            return m;

        }


       

        public ViewDocsList(IConfiguration config)
        {
            MaxPageNo = Convert.ToInt32(config.GetValue<string>("AppData:MaxPageNo"));
        }
        
   
    }

        public class ViewUsers
        {
            [StringLength(50)]
            [Display(Name = "Login")]
            public string Login { get; set; }
            [StringLength(100)]
            [Required]
            [Display(Name = "Email")]
            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }
            [Required]
            [Display(Name = "Hasło")]
            [StringLength(100, ErrorMessage = "Pole {0} musi mieć przynajmniej {2} znaków.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            [DataType(DataType.Password)]
            [Display(Name = "Potwierdź hasło")]
            [System.ComponentModel.DataAnnotations.CompareAttribute("Password", ErrorMessage = "Hasła nie są zgodne!")]
            public string ConfirmPassword { get; set; }
            [StringLength(30)]
            [Required]
            [Display(Name = "Imię")]
            public string Name { get; set; }
            [StringLength(50)]
            [Required]
            [Display(Name = "Nazwisko")]
            public string Surname { get; set; }
            
            [Display(Name = "Może tworzyć dokumenty")]
            public bool CanCreateEvent { get; set; }
            [Display(Name = "Może zarządzać użytkownikami")]
            public bool CanManageUsers { get; set; }
            [Display(Name = "Jest Księgowością")]
            public bool IsAccounting { get; set; }
        public string ErrorMsg { get; set; }
            [Display(Name = "Czy potwierdzone?")]
            public bool IsConfirmed { get; set; }
            public string mode { get; set; }
            public int ev_id { get; set; }
           
            [Display(Name = "Powiązanie z Organizatorem")]
            public string LinkedToOrganizer { get; set; }
            [Display(Name = "Organizatorzy")]
            public SelectList Organizers { get; set; }
            public ViewUsers(User us, ApplicationDbContext db)
            {
                Login = us.Login;
               
                Email = us.Email;
                Name = us.Name;
                Password = us.Password;
                IsAccounting = us.IsAccounting;
                Surname = us.Surname;
                CanCreateEvent = us.CanCreateEvent;
                CanManageUsers = us.CanManageUsers;
                LinkedToOrganizer = us.LinkedToOrganizer;
                using (db)
                {
                    List<Object> list = new List<object>();
                    foreach (var org in db.Organizers)
                    {
                        list.Add(new { Code = org.Organizer1, Description = org.Organizer1 });
                    }
                    this.Organizers = new SelectList(list, "Code", "Description");
                }
            }

            public ViewUsers(ApplicationDbContext db)
            {
                using (db)
                {
                    List<Object> list = new List<object>();
                    foreach (var org in db.Organizers)
                    {
                        list.Add(new { Code = org.Organizer1, Description = org.Organizer1 });
                    }
                    this.Organizers = new SelectList(list, "Code", "Description");
                }
            }
            public ViewUsers(string log, ApplicationDbContext db)
            {
                using (db)
                {
                    User us = db.AppUsers.Find(log);
                    if (us != null)
                    {
                        Login = us.Login;
                        IsAccounting = us.IsAccounting;
                        Email = us.Email;
                        Name = us.Name;
                        Password = us.Password;
                        CanCreateEvent = us.CanCreateEvent;
                        CanManageUsers = us.CanManageUsers;    
                        Surname = us.Surname;
                    }
                    
                        List<Object> list = new List<object>();
                        foreach (var org in db.Organizers)
                        {
                            list.Add(new { Code = org.Organizer1, Description = org.Organizer1 });
                        }
                        this.Organizers = new SelectList(list, "Code", "Description");
                    
                }
            }
            public void InitiateFromDB(string login, string searchString, string sortOrder, ApplicationDbContext db)
            {
                using (db)
                {
                    List<Object> list = new List<object>();
                    foreach (var org in db.Organizers)
                    {
                        list.Add(new { Code = org.Organizer1, Description = org.Organizer1 });
                    }
                    this.Organizers = new SelectList(list, "Code", "Description");                  
                }
            }
        }
        public class ImgObj
        {
            #region Properties

            /// <summary>  
            /// Gets or sets Image ID.  
            /// </summary>  
            public int FileId { get; set; }

            /// <summary>  
            /// Gets or sets Image name.  
            /// </summary>  
            public string FileName { get; set; }

            /// <summary>  
            /// Gets or sets Image extension.  
            /// </summary>  
            public string FileContentType { get; set; }

            #endregion
        }  
        public class ViewDocs
        {

            [Display(Name = "Nr Dokumentu")]
            public int DocsId { get; set; }
            [Required]
            [Display(Name = "Typ dokumentu")]
            public DocTypes DocType { get; set; }
            [Required]
            [Range(2020, 2050)]
            [Display(Name = "Rok dokumentu")]
            public Int16 DocYear { get; set; }
            [Required]
            [Range(1, 12)]
            [Display(Name = "Miesiąc dokumentu")]
            public Int16 DocMonth { get; set; }
            [Display(Name = "Status")]
            public DocStatuses Status { get; set; }
            [StringLength(50)]
            [Required]
            [Display(Name = "Nazwa Dokumentu")]
            public string DocsName { get; set; }
            [Required]
            [Range(1, 53)]
            [Display(Name = "Tydzień dokumentu")]
            public Int16 DocWeek { get; set; }
            [Required]
            [StringLength(50)]
            [Display(Name = "Nr faktury")]
            public string InvoiceNo { get; set; }
            [Display(Name = "Czy zagraniczny?")]
            public Boolean IsForeign { get; set; }

        [Display(Name = "Data zmiany statusu")]
            [Required]
            [DataType(DataType.Date)]
            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
            public DateTime StausChangeDate  { get; set; }
            [Display(Name = "Czas zmiany statusu")]
            [DataType(DataType.Time)]
            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
            public DateTime StausChangeTime { get; set; }
            
            [Display(Name = "Login Zgłaszającego Wydarzenie")]
            public string CreatedByLogin { get; set; }
            
            public string mode { get; set; }
            
            [Display(Name = "Plik dokumentu pdf")]
            public string ExternalLinkBig { get; set; }
            [Display(Name = "Opis")]
            [DataType(DataType.MultilineText)]
            public string Description { get; set; }          
           
            [Display(Name = "Plik dokumentu pdf")]
            public IFormFile FileAttachBig { get; set; }
            //public List<ImgObj> ImgLst { get; set; }  
           
            [Display(Name = "Organizator")]
            public string OrganizerEvent { get; set; }            
            
          
            [Display(Name = "Organizatorzy")]
            public SelectList Organizers { get; set; }
            [Display(Name = "Nazwa Organizatora")]
            public string OrganizerName { get; set; }

        public ViewDocs(ApplicationDbContext db)
            {
                using (db)
                {
                    List<Object> list = new List<object>();
                    foreach (var org in db.Organizers)
                    {
                        list.Add(new { Code = org.Organizer1, Description = org.Organizer1});
                    }
                    this.Organizers = new SelectList(list, "Code", "Description");
                    this.StausChangeDate  = DateTime.Now.Date;
                    this.StausChangeTime = DateTime.Now;

                this.Status = DocStatuses.Nowy;

                }
            }
            public ViewDocs(Doc ev, ApplicationDbContext db)
            {
                this.CreatedByLogin = ev.CreatedByLogin;
                this.DocsId = ev.DocsId;
                this.DocsName = ev.DocsName;
                this.DocYear = ev.DocYear;
                this.DocMonth = ev.DocMonth;
                this.DocType = (DocTypes)ev.DocType;
                this.StausChangeDate = ev.StausChangeDateTime.Date ;
                this.StausChangeTime = ev.StausChangeDateTime;
                this.Status = (DocStatuses)ev.Status;
                this.DocWeek = ev.DocWeek;
                this.InvoiceNo = ev.InvoiceNo;
                this.IsForeign = ev.IsForeign;
                this.ExternalLinkBig = ev.ExternalLinkBig;
                this.Description = ev.Description;

                this.OrganizerEvent = ev.OrganizerEvent;

                this.OrganizerName = ev.OrganizerName;
                using (db)
                {
                    
                    var orgb = db.Organizers.Find(ev.OrganizerEvent);
                 
                   // this.OrganizerName = orgb.Organizer;
                    
                    List<Object> list = new List<object>();
                    foreach (var org in db.Organizers)
                    {
                        list.Add(new { Code = org.Organizer1, Description = org.Organizer1 });
                    }
                    this.Organizers = new SelectList(list, "Code", "Description");                    
                }
            }
                public ViewDocs(int ev_id, ApplicationDbContext db)
            {

                using (db)
                {
                    var ev = db.Docs.Find(ev_id);
                                   this.CreatedByLogin = ev.CreatedByLogin;

                this.DocType = (DocTypes)ev.DocType;
                this.DocsId = ev.DocsId;
                this.DocsName = ev.DocsName;
                this.DocYear = ev.DocYear;
                this.DocMonth = ev.DocMonth;
                this.StausChangeDate = ev.StausChangeDateTime.Date;
                this.StausChangeTime = ev.StausChangeDateTime;
                this.DocWeek = ev.DocWeek;
                this.InvoiceNo = ev.InvoiceNo;
                this.IsForeign = ev.IsForeign;
                this.Status = (DocStatuses)ev.Status;

                this.ExternalLinkBig = ev.ExternalLinkBig;
                this.Description = ev.Description;

                this.OrganizerEvent = ev.OrganizerEvent;

                var orga = db.Organizers.Find(ev.OrganizerEvent);

                //this.OrganizerName = orga.Organizer;                
                this.OrganizerName = ev.OrganizerName;
                
                    List<Object> list = new List<object>();
                    foreach (var org in db.Organizers)
                    {
                        list.Add(new { Code = org.Organizer1, Description = org.Organizer1 });
                    }
                    this.Organizers = new SelectList(list, "Code", "Description");

                }
            }
        }

    /*
        public static class ConfigHelper
        {
            public static string MailingLogin()
            {
                return ConfigurationManager.AppSettings["MailingLogin"];
            }
            public static string AccountingLogin()
            {
                return ConfigurationManager.AppSettings["AccountingLogin"];
            }

        public static string MailingPassword()
            {
                return ConfigurationManager.AppSettings["MailingPassword"];
            }

            public static string SmtpAddress()
            {
                return ConfigurationManager.AppSettings["SmtpAddress"];
            }

            internal static string ImageFolder()
            {
                return ConfigurationManager.AppSettings["ImageFolder"];
            }
            internal static string YearBackForDeletion()
            {
                return ConfigurationManager.AppSettings["YearBackForDeletion"];
            }
            internal static string MaxPageNo()
            {
                return ConfigurationManager.AppSettings["MaxPageNo"];
            }
    }
    */
        public class Email
        {
            [Display(Name = "Mail gdzie chcesz wysłać")]
            public string MailTo { get; set; }
            public string MailFrom { get; set; }
            [Display(Name = "Nazwa osoby, do której chcesz wysłać")]
            public string LoginTo { get; set; }
            public string LoginFrom { get; set; }
            [Display(Name = "Tutuł maila")]
            public string Subject { get; set; }
            [Display(Name = "Dodaj komentarz od siebie")]
            public string Comments { get; set; }
            [Display(Name = "Treść maila")]
            public string Body { get; set; }
            public int EvID { get; set; }
            public string Action { get; set; }
            public string Action2 { get; set; }
            public string WhereNext { get; set; } 
            public Email() { }
            public Email(string mt, string mf, string lt, string lf, int ei, string su, string co, string bo, string ac, string ac2, string wn)
            {                                    
                    MailTo = mt;
                    MailFrom = mf;
                    LoginTo = lt;
                    LoginFrom = lf;
                    Subject = su;
                    Comments = co;
                    EvID = ei;
                    Body = bo;
                    WhereNext = wn;
                    Action = ac;
                    Action2 = ac2;
            }
        }
        public class RouteMapViewModel
        {
            public string StartLocation { get; set; }
            public string EndLocation { get; set; }
            public DateTime StartDate { get; set; }
        }

        public class ImageResult
        {
            public bool Success { get; set; }
            public string ImageName { get; set; }
            public string ErrorMessage { get; set; }

        }
        public class ImageUpload
        {
            // set default size here
            public int Width { get; set; }

            public int Height { get; set; }

            // folder for the upload, you can put this in the web.config
            private readonly string UploadPath = "~/Images/Items/";

            public ImageResult RenameUploadFile(IFormFile file, IHostEnvironment _env,Int32 counter = 0)
            {
                var fileName = Path.GetFileName(file.FileName);
            string prepend = "item_";
                string finalFileName = prepend + ((counter).ToString()) + "_" + fileName;
                if (System.IO.File.Exists
                    (Path.Combine(_env.ContentRootPath, UploadPath + finalFileName)))
                {
                    //file exists => add country try again
                    return RenameUploadFile(file, _env,++counter);
                }
                //file doesn't exist, upload item but validate first
                return UploadFile(file, finalFileName, _env);
            }

            private ImageResult UploadFile(IFormFile file, string fileName, IHostEnvironment _env)
            {
                ImageResult imageResult = new ImageResult { Success = true, ErrorMessage = null };

                var path =
              Path.Combine(_env.ContentRootPath, UploadPath + fileName);
                string extension = Path.GetExtension(file.FileName);

                //make sure the file is valid
                if (!ValidateExtension(extension))
                {
                    imageResult.Success = false;
                    imageResult.ErrorMessage = "Invalid Extension";
                    return imageResult;
                }

                try
                {
                    using (Stream fileStream = new FileStream(path, FileMode.Create))   
                        file.CopyTo(fileStream);                    
                /*
                    Image imgOriginal = Image.FromFile(path);

                    //pass in whatever value you want 
                    Image imgActual = Scale(imgOriginal);
                    imgOriginal.Dispose();
                    imgActual.Save(path);
                    imgActual.Dispose();
                */
                    imageResult.ImageName = "/Images/Items/" + fileName;
                
                    return imageResult;
                }
                catch (Exception ex)
                {
                    // you might NOT want to show the exception error for the user
                    // this is generaly logging or testing

                    imageResult.Success = false;
                    imageResult.ErrorMessage = ex.Message;
                    return imageResult;
                }
            }

            private bool ValidateExtension(string extension)
            {
                extension = extension.ToLower();
                switch (extension)
                {
                    case ".pdf":
                        return true;                    
                    default:
                        return false;
                }
            }

            private Image Scale(Image imgPhoto)
            {
                float sourceWidth = imgPhoto.Width;
                float sourceHeight = imgPhoto.Height;
                float destHeight = 0;
                float destWidth = 0;
                int sourceX = 0;
                int sourceY = 0;
                int destX = 0;
                int destY = 0;

                // force resize, might distort image
                if (Width != 0 && Height != 0)
                {
                    destWidth = Width;
                    destHeight = Height;
                }
                // change size proportially depending on width or height
                else if (Height != 0)
                {
                    destWidth = (float)(Height * sourceWidth) / sourceHeight;
                    destHeight = Height;
                }
                else
                {
                    destWidth = Width;
                    destHeight = (float)(sourceHeight * Width / sourceWidth);
                }

                Bitmap bmPhoto = new Bitmap((int)destWidth, (int)destHeight,
                                            PixelFormat.Format32bppPArgb);
                bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

                Graphics grPhoto = Graphics.FromImage(bmPhoto);
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

                grPhoto.DrawImage(imgPhoto,
                    new Rectangle(destX, destY, (int)destWidth, (int)destHeight),
                    new Rectangle(sourceX, sourceY, (int)sourceWidth, (int)sourceHeight),
                    GraphicsUnit.Pixel);

                grPhoto.Dispose();

                return bmPhoto;
            }
        }
        public static class mailTemp
        {
            static string bd1 =
            @"<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
<meta http-equiv='Content-Type' content='text/html; '>
<title></title>
<style type='text/css'>
			.cs162A16FE{}
			.cs75B5223{width:594.75pt;padding:0pt 0pt 0pt 0pt;border-top:none;border-right:none;border-bottom:none;border-left:none}
			.cs2654AE3A{text-align:left;text-indent:0pt;margin:0pt 0pt 0pt 0pt}
			.csC8F6D76{color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:normal;font-style:normal;}
			.cs7382B3A{text-align:left;text-indent:0pt;margin:0pt 1pt 0pt 1pt}
			.cs23FB0664{color:#000000;background-color:transparent;font-family:'Times New Roman';font-size:12pt;font-weight:normal;font-style:normal;}
			.cs1752EA45{width:595.5pt;padding:0pt 0pt 0pt 0pt;border-top:none;border-right:none;border-bottom:none;border-left:none}
			.cs2C18EB4{color:#000000;background-color:transparent;font-family:'Times New Roman';font-size:0.5pt;font-weight:normal;font-style:normal;}
			.cs4306042E{color:#000000;background-color:transparent;font-family:Calibri;font-size:11pt;font-weight:bold;font-style:normal;}
			.csB5692613{text-align:left;text-indent:0pt;margin:5pt 0pt 0pt 0pt}
			.csDAAE5F7{color:#000000;background-color:transparent;font-family:Calibri;font-size:12pt;font-weight:normal;font-style:normal;}
		</style>
</head>
<body bgcolor='#FFFFFF'>
<table class='cs162A16FE' border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse;'>
<tbody>
<tr style='height:30.75pt;'>
<td class='cs75B5223' valign='top' width='793'>
<p class='cs2654AE3A'><span class='csC8F6D76'>";

    static string bd2 =
@"</td>
</tr>
</tbody>
</table>
<p class='cs7382B3A'><span class='cs23FB0664'>&nbsp;</span></p>
<table class='cs162A16FE' border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse;'>
<tbody>
<tr style='height:3.75pt;'>
<td class='cs1752EA45' valign='top' width='794'>
<p class='cs2654AE3A'><span class='cs2C18EB4'>&nbsp;</span></p>
</td>
</tr>
<tr style='height:67.5pt;'>
<td class='cs1752EA45' valign='top' width='794'>
<p class='cs2654AE3A'><span class='csC8F6D76'>&nbsp;</span></p>
<p class='cs2654AE3A'><span class='csC8F6D76'>Z pozdrowieniami </span></p>
<p class='cs2654AE3A'><span class='csC8F6D76'>Zespół DokumentyTransportowe.pl</span></p>
</td>
</tr>
</tbody>
</table>
<p class='csB5692613'><span class='csDAAE5F7'><img src='~/Content/Stena_logo_heaven60x60_jpg.jpg' alt='' style='border-width:0px;'></span></p>
</body>
</html>";
    static string bd3 =
        @"<p class='cs2654AE3A'><span class='csC8F6D76'>&nbsp;</span></p>
<p class='cs2654AE3A'><span class='csC8F6D76'>Z pozdrowieniami </span></p>
<p class='cs2654AE3A'><span class='csC8F6D76'>Zespół DokumentyTransportowe.pl</span></p>
</td>
</tr>
</tbody>
</table>
<p class='csB5692613'><span class='csDAAE5F7'><img src='~/Content/Stena_logo_heaven60x60_jpg.jpg' alt='' style='border-width:0px;'></span></p>";

        static string bd4 =
        @"<p class='cs2654AE3A'><span class='csC8F6D76'>&nbsp;</span></p>
<p class='cs2654AE3A'><span class='csC8F6D76'>Z pozdrowieniami </span></p>
<p class='cs2654AE3A'><span class='csC8F6D76'>Zespół Logistyki Stena Recycling</span></p>
</td>
</tr>
</tbody>
</table>
<p class='csB5692613'><span class='csDAAE5F7'><img src='~/Content/Stena_logo_heaven60x60_jpg.jpg' alt='' style='border-width:0px;'></span></p>";
        public static string body1 { get { return bd1; } set { body1 = value; } }
            public static string body2 { get { return bd2; } set { body2 = value; } }
            public static string body3 { get { return bd3; } set { body3 = value; } }
            public static string body4 { get { return bd4; } set { body4 = value; } }
    }

        
        public class DeleteOld
        {
            [Required]
            [UIHint("dateFrom")]
            [DataType(DataType.Date)]
            [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd}")]   
            [Display(Name = "Data od której masowo usunąć")]            
            public DateTime dateFrom { get; set; }            
            [Required]
            [UIHint("dateTo")]
            [DataType(DataType.Date)]
            [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd}")]
            [Display(Name = "Data do której masowo usunąć")]
            public DateTime dateTo { get; set; }            

        }

}