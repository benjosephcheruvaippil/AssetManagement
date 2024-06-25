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
        public CommonFunctions()
        {
                
        }
        private async Task SetUpDb()
        {
            try
            {
                if (_dbConnection == null)
                {
                    string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Assets.db3");
                    _dbConnection = new SQLiteAsyncConnection(dbPath);
                    await _dbConnection.CreateTableAsync<Assets>();
                    await _dbConnection.CreateTableAsync<IncomeExpenseModel>();
                    await _dbConnection.CreateTableAsync<IncomeExpenseCategories>();
                    await _dbConnection.CreateTableAsync<DataSyncAudit>();
                    await _dbConnection.CreateTableAsync<AssetAuditLog>();
                    await _dbConnection.CreateTableAsync<Owners>();
                    await _dbConnection.CreateTableAsync<UserCurrency>();
                    await _dbConnection.CreateTableAsync<Currency>();
                }
            }
            catch (Exception)
            {
              
            }
        }

        public async Task CommonDataForDefaults()
        {
            await AddIncomeExpenseCategoryDefaults();
            await AddCurrencyListDefaults();
            await AddUserCurrencyDefault();
        }
        public async Task AddIncomeExpenseCategoryDefaults()
        {

        }

        public async Task AddCurrencyListDefaults()
        {
            await SetUpDb();
            List<Currency> currencyList = new List<Currency>
            {
               new Currency { Country="United States",CurrencyName = "USD",CurrencyCode="en-US" },
               new Currency { Country="Germany",CurrencyName = "Euro",CurrencyCode="en-IE" },
               new Currency { Country="Japan",CurrencyName = "JPY",CurrencyCode="en-JP" },
               new Currency { Country="United Kingdom",CurrencyName = "GBP",CurrencyCode="en-GB" },
               new Currency { Country="Australia",CurrencyName = "AUD",CurrencyCode="en-AU" },
               new Currency { Country="India",CurrencyName = "INR",CurrencyCode="en-IN" }
            };

            await _dbConnection.InsertAllAsync(currencyList);
        }

        public async Task AddUserCurrencyDefault()
        {

        }
    }
}
