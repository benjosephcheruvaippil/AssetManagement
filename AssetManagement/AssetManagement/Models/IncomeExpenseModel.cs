using Google.Cloud.Firestore;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models
{
    public class IncomeExpenseModel
    {
        [PrimaryKey, AutoIncrement]
        public int TransactionId { get; set; }
        public double Amount { get; set; }
        public string TransactionType { get; set; }
        public DateTime Date { get; set; }
        public double TaxAmountCut { get; set; }
        public string OwnerName { get; set; }
        public string CategoryName { get; set; }
        public string Remarks { get; set; }
    }
}
