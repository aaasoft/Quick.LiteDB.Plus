using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quick.LiteDB.Plus
{
    public interface IHasDependcyRelation
    {
        ModelDependcyInfo[] GetDependcyRelation();
    }
}
