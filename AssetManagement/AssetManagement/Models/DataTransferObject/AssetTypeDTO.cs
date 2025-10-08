using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.DataTransferObject
{
    public class AssetTypeDTO
    {
        public int AssetTypeId { get; set; }
        public string AssetTypeName { get; set; }
        public string Description { get; set; }
        public bool? EnableAsOfDate { get; set; }
        public bool? EnableMaturityDate { get; set; }
        public bool? IncludeInNetWorth { get; set; }
        public string CategoryTag { get; set; }
        //public string DateDescription
        //{
        //    get
        //    {
        //        if (EnableAsOfDate == true)
        //            return "* As Of Date *";
        //        else if (EnableMaturityDate == true)
        //            return "* Maturity Date *";
        //        else
        //            return "N/A";
        //    }
        //}
    }
}
