using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.Reports
{
    public class AssetAllocationReport
    {
        public string AssetType { get; set; }
        public string Amount { get; set; }
        public string PortfolioPercentage { get; set; }
    }
}
