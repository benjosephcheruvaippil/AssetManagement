using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.FirestoreModel
{
    [FirestoreData]
    public class IncomeExpense
    {
        [FirestoreProperty]
        public int TransactionId { get; set; }
        [FirestoreProperty]
        public double Amount { get; set; }
        [FirestoreProperty]
        public double TaxAmountCut { get; set; }
        [FirestoreProperty]
        public string TransactionType { get; set; }
        [FirestoreProperty]
        public string Date { get; set; }
        [FirestoreProperty]
        public string CategoryName { get; set; }
        [FirestoreProperty]
        public string OwnerName { get; set; }
        [FirestoreProperty]
        public string Remarks { get; set; }
        [FirestoreProperty]
        public string Mode { get; set; }
    }
}
