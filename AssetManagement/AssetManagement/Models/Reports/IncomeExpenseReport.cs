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
        public string ExpenseAmount { get; set; }
        public string IncomeAmount { get; set; }
        public string BalanceAmount { get; set; }
    }
}
