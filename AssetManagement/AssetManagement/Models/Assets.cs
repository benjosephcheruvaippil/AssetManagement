using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models
{
    public class Assets
    {
        [PrimaryKey, AutoIncrement]
        public int AssetId { get; set; }
        public string InvestmentEntity { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public string InterestFrequency { get; set; }
        public string Holder { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public string Remarks { get; set; }
        public DateTime AsOfDate { get; set; }
    }
}
