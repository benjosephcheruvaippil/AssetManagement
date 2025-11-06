using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.DataTransferObject
{
    public class IncomeExpenseDTO
    {
        public int TransactionId { get; set; }
        public double Amount { get; set; }
        public string TransactionType { get; set; }
        public DateTime Date { get; set; }
        public double TaxAmountCut { get; set; }
        public string OwnerName { get; set; }
        public string CategoryName { get; set; }
        public string Remarks { get; set; }
        public string Mode { get; set; }
        public Color ModeColor => !string.IsNullOrEmpty(Mode) ? (Mode.ToLower() == "file_upload" ? Colors.Red : null) : null;
    }
}
