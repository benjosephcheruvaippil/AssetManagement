using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.Reports
{
    public class IncomeExpenseReport
    {
        public string Month { get; set; }
        public decimal ExpenseAmount { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal BalanceAmount { get; set; }
    }
}
