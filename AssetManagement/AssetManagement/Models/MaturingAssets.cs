using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models
{
    public class MaturingAssets
    {
        public string InvestmentEntity { get; set; }
        public string HolderName { get; set; }
        public DateTime MaturityDate { get; set; }
        public string Amount { get; set; }
    }
}
