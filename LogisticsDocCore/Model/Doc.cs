using System;
using System.Collections.Generic;

#nullable disable

namespace LogisticsDocCore.Model
{
    public partial class Doc
    {
        public int DocsId { get; set; }
        public string DocsName { get; set; }
        public string CreatedByLogin { get; set; }
        public int Status { get; set; }
        public string ExternalLinkBig { get; set; }
        public string Description { get; set; }
        public string OrganizerEvent { get; set; }
        public string OrganizerName { get; set; }
        public short DocYear { get; set; }
        public short DocMonth { get; set; }
        public int DocType { get; set; }
        public DateTime StausChangeDateTime { get; set; }
        public short DocWeek { get; set; }
        public string InvoiceNo { get; set; }
        public bool IsForeign { get; set; }

        public virtual User CreatedByLoginNavigation { get; set; }
        public virtual Organizer OrganizerEventNavigation { get; set; }

        public Doc()
        {

        }
        public Doc(ViewDocs ev)
        {
            this.CreatedByLogin = ev.CreatedByLogin;
            //var EvTime = new System.TimeSpan(ev.DocsInsertTime.Hour, ev.DocsInsertTime.Minute, 0);
            this.DocsName = ev.DocsName;
            this.DocYear = ev.DocYear;
            this.DocMonth = ev.DocMonth;
            this.StausChangeDateTime = ev.StausChangeTime;
            this.DocWeek = ev.DocWeek;
            this.InvoiceNo = ev.InvoiceNo;
            this.IsForeign = ev.IsForeign;
            this.DocType = (int)ev.DocType;
            //this.StausChangeDateTime  = this.StausChangeDateTime .Add(EvTime);

            this.Status = (int)ev.Status;
            this.Description = ev.Description;

            this.OrganizerEvent = ev.OrganizerEvent;

            this.OrganizerName = ev.OrganizerName;
            //using (ApplicationDBContext db = new ApplicationDBContext())
            //{
            //    var org = db.Organizers.Find(ev.OrganizerEvent);

            //    //  this.OrganizerName = org.OrganizerName;
            //}
        }

        public void CopyFrom(ViewDocs ev, Model.ApplicationDbContext db)
        {
            this.CreatedByLogin = ev.CreatedByLogin;

            //var EvTime = new System.TimeSpan(ev.DocsInsertTime.Hour, ev.DocsInsertTime.Minute, 0);
            this.DocsName = ev.DocsName;
            this.DocYear = ev.DocYear;
            this.DocMonth = ev.DocMonth;
            this.StausChangeDateTime = ev.StausChangeTime;
            this.DocType = (int)ev.DocType;
            this.StausChangeDateTime = ev.StausChangeTime;
            this.DocWeek = ev.DocWeek;
            this.InvoiceNo = ev.InvoiceNo;
            this.IsForeign = ev.IsForeign;
            this.Status = (int)ev.Status;

            this.ExternalLinkBig = ev.ExternalLinkBig;
            this.Description = ev.Description;

            if (ev.OrganizerEvent != null && ev.OrganizerEvent != "") this.OrganizerEvent = ev.OrganizerEvent;

            this.OrganizerName = ev.OrganizerName;
            using (db)
            {
                var org = db.Organizers.Find(ev.OrganizerEvent);

                //this.OrganizerName = org.OrganizerName;
            }
        }

        public int DaysBetween(DateTime d1, DateTime d2)
        {
            TimeSpan span = d2.Subtract(d1);
            return (int)span.TotalDays;
        }
    }
}
