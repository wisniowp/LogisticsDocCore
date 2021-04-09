using LogisticsDocCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogisticsDocCore.Model
{
    public interface IUserRepository
    {
        IEnumerable<User> AllUsers { get; }
    }
}
