using AssetManagement.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Common
{
    public class CommonFunctions
    {
        private SQLiteAsyncConnection _dbConnection;
        public void CommonDataForDefaults()
        {
            AddIncomeExpenseCategoryDefaults();
            AddCurrencyListDefaults();
            AddUserCurrencyDefault();
        }
        public void AddIncomeExpenseCategoryDefaults()
        {

        }

        public async void AddCurrencyListDefaults()
        {
            List<Currency> currencyList = new List<Currency>
            {
               new Currency { Country="India",CurrencyName = "INR",CurrencyCode="" },
               new Currency { Country="United States",CurrencyName = "USD",CurrencyCode="" },
               new Currency { Country="England",CurrencyName = "GBP",CurrencyCode="" },
               new Currency { Country="Germany",CurrencyName = "UERO",CurrencyCode="" },
               new Currency { Country="Singapore",CurrencyName = "SGD",CurrencyCode="" }
            };

            await _dbConnection.InsertAllAsync(currencyList);
        }

        public void AddUserCurrencyDefault()
        {

        }
    }
}
