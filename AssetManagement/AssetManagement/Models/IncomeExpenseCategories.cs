using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models
{
    public class IncomeExpenseCategories
    {
        [PrimaryKey, AutoIncrement]
        public int IncomeExpenseCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryType { get; set; }
        public bool? IsOneTimeExpense { get; set; }
        public bool? IsVisible { get; set; }

        //public IncomeExpenseCategories()
        //{
        //    IsVisible = true;
        //}
    }
}
