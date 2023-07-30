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
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public DateTime Date { get; set; }
        public string CategoryName { get; set; }
    }
}
