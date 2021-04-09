using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogisticsDocCore.Data;
using LogisticsDocCore.Model;

namespace LogisticsDocCore.Model
{
    public class UserRepository: IUserRepository
    {
        private readonly ApplicationDbContext _appDbContxt;
        public UserRepository(ApplicationDbContext appDbContext)
        {
            _appDbContxt = appDbContext;
        }

        public IEnumerable<User> AllUsers => (IEnumerable<User>)_appDbContxt.Users;
    }
}
