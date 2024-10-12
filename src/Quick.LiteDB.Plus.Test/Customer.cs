using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.LiteDB.Plus.Test
{
    [Table("ccc")]
    public class Customer : BaseModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string[] Phones { get; set; }
        public bool IsActive { get; set; }
    }
}
