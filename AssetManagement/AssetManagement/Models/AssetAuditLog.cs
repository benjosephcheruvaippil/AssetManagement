using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models
{
    public class AssetAuditLog
    {
        public int AssetLogId { get; set; }
        public double? LiquidAssetValue { get; set; }
        public double? NonMovableAssetValue { get; set; }
        public double? NetAssetValue { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
