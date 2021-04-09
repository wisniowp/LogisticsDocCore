using System;
using System.Collections.Generic;

#nullable disable

namespace LogisticsDocCore.Model
{
    public partial class User
    {
        public User()
        {
            Docs = new HashSet<Doc>();
        }

        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool CanCreateEvent { get; set; }
        public bool CanManageUsers { get; set; }
        public string LinkedToOrganizer { get; set; }
        public bool IsAccounting { get; set; }

        public virtual Organizer LinkedToOrganizerNavigation { get; set; }
        public virtual ICollection<Doc> Docs { get; set; }
        public User(ViewUsers us)
        {

            Email = us.Email;
            Name = us.Name;
            Password = us.Password;
            Surname = us.Surname;
            CanCreateEvent = us.CanCreateEvent;
            IsAccounting = us.IsAccounting;
        }
        public void CopyFrom(ViewUsers us)
        {

            Email = us.Email;
            Name = us.Name;
            //Password = us.Password;
            Surname = us.Surname;
            CanCreateEvent = us.CanCreateEvent;
            LinkedToOrganizer = us.LinkedToOrganizer;
        }
    }
}
