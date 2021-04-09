using LogisticsDocCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogisticsDocCore.Model
{
    public class DocRepository:IDocRepository
    {
        private readonly ApplicationDbContext _appDbContxt;
        public DocRepository(ApplicationDbContext appDbContext)
        {
            _appDbContxt = appDbContext;
        }

        public IEnumerable<Doc> AllDocs => (IEnumerable<Doc>)_appDbContxt.Docs;
    }
}
