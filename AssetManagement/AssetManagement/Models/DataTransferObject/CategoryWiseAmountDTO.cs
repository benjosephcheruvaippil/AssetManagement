using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.DataTransferObject
{
    public class CategoryWiseAmountDTO
    {
        public string CategoryName { get; set; }
        public string TransactionType { get; set; }
        public bool? OneTimeExpense { get; set; }
        public double Amount { get; set; }
    }
}
