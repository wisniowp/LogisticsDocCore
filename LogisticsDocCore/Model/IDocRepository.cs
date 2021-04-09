using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogisticsDocCore.Model
{
    public class IDocRepository
    {
        public IEnumerable<Doc> AllDocs { get; }
    }
}
