using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.FirestoreModel
{
    [FirestoreData]
    public class AssetDetails
    {
        [FirestoreProperty]
        public int AssetId { get; set; }
        [FirestoreProperty]
        public string InvestmentEntity { get; set; }
        [FirestoreProperty]
        public string Type { get; set; }
        [FirestoreProperty]
        public double Amount { get; set; }
        [FirestoreProperty]
        public double InterestRate { get; set; }
        [FirestoreProperty]
        public string InterestFrequency { get; set; }
        [FirestoreProperty]
        public string Holder { get; set; }
        [FirestoreProperty]
        public string Remarks { get; set; }
        [FirestoreProperty]
        public string StartDate { get; set; }
        [FirestoreProperty]
        public string MaturityDate { get; set; }
        [FirestoreProperty]
        public string AsOfDate { get; set; }
    }
}
