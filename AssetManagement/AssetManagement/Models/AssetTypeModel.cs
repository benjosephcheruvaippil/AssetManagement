using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models
{
    public class AssetTypeModel
    {
        [PrimaryKey, AutoIncrement]
        public int AssetTypeId { get; set; }
        public string AssetTypeName { get; set; }
        public string Description { get; set; }
        public bool? EnableAsOfDate { get; set; }
        public bool? EnableMaturityDate { get; set; }
        public bool? IncludeInNetWorth { get; set; }
        public string CategoryTag { get; set; }
    }
}
