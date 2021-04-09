using System;
using System.Collections.Generic;

#nullable disable

namespace LogisticsDocCore.Model
{
    public partial class Organizer
    {
        public Organizer()
        {
            Docs = new HashSet<Doc>();
            Users = new HashSet<User>();
        }

        public string Organizer1 { get; set; }
        public string OrganizerCountry { get; set; }
        public string OrganizerCity { get; set; }
        public string OrganizerPostCode { get; set; }
        public string OrganizerAddress { get; set; }
        public string OrganizerName { get; set; }

        public virtual ICollection<Doc> Docs { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public void CopyFrom(Organizer org)
        {

            this.OrganizerAddress = org.OrganizerAddress;
            this.OrganizerCity = org.OrganizerCity;
            this.OrganizerCountry = org.OrganizerCountry;
            this.OrganizerPostCode = org.OrganizerPostCode;
        }
    }
}
