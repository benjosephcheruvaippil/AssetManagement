using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models
{
    public class DataSyncAudit
    {
        //public int AuditId { get; set; }
        public DateTime Date { get; set; }
        public string Action { get; set; }
    }
}
