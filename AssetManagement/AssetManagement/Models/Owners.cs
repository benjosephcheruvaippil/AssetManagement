using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models
{  
    public class Owners
    {
        [PrimaryKey, AutoIncrement]
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
    }
}
