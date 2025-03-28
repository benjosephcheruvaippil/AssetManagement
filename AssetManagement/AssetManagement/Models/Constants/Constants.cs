using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.Constants
{
    public class Constants
    {
        public const string IncomeExpenseFirestoreCollection = "IncomeExpenseFirestoreData";
        public const string AssetFirestoreCollection = "AssetFirestoreData";

        public const string AddNewCategoryOption = "Add New Category";
        public const string AddNewOwnerOption = "Add New Owner";

        public const string FromLaunchPage = "LaunchPage";
        public const string FromSettingsPage = "SettingsPage";

        public const string DeviceNumber = "samsung-Phone-SM-S911B";
        //samsung-Phone-SM-S911B

        public static string _currency = "en-IN";
        public static string GetCurrency()
        {
            return _currency;
        }

        public static void SetCurrency(string currency)
        {
            _currency = currency;
        }
        //public const string IncomeExpenseFirestoreCollection = "Maami-IncomeExpenseFirestoreData";
        //public const string AssetFirestoreCollection = "Maami-AssetFirestoreData";

        //IncomeExpenseTest - this is testing collection
        //IncomeExpenseFirestoreData - this is production collection
    }
}
