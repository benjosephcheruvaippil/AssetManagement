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
        //Ben's Samsung: samsung-Phone-SM-S911B
        //Ben's Redmi: 

        public const string AssetTypeProperty = "Property";
        public const string SyncfusionLicenseKey = "Ngo9BigBOggjHTQxAR8/V1JGaF5cX2FCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWX5fdXVXR2FeVExzWkZWYEs=";

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
