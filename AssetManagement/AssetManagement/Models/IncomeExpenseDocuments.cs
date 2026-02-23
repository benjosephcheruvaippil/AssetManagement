using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetManagement.Models
{
    public class IncomeExpenseDocuments
    {
        [PrimaryKey, AutoIncrement]
        public int IncomeExpenseDocumentId { get; set; }
        public int TransactionId { get; set; }
        public string FileName { get; set; } // files are stored inside media folder.
        public string FileFormat { get; set; }
    }
}
