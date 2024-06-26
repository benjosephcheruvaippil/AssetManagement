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
            try
            {
                //add expense default
                await SetUpDb();
                int exsistingDataExpenseCategory = await _dbConnection.Table<IncomeExpenseCategories>().Where(c => c.CategoryType == "Expense").CountAsync();
                if (exsistingDataExpenseCategory == 0)
                {
                    List<IncomeExpenseCategories> objExpenseCategory = new List<IncomeExpenseCategories>
                {
                    new IncomeExpenseCategories{CategoryName="Household Items",CategoryType="Expense"},
                    new IncomeExpenseCategories{CategoryName="Autombile",CategoryType="Expense"},
                    new IncomeExpenseCategories{CategoryName="Leisure",CategoryType="Expense"}
                };
                    await _dbConnection.InsertAllAsync(objExpenseCategory);
                }
                //add expense default

                //add income default
                int exsistingDataIncomeCategory = await _dbConnection.Table<IncomeExpenseCategories>().Where(c => c.CategoryType == "Income").CountAsync();
                if (exsistingDataIncomeCategory == 0)
                {
                    List<IncomeExpenseCategories> objExpenseCategory = new List<IncomeExpenseCategories>
                {
                    new IncomeExpenseCategories{CategoryName="Salary",CategoryType="Income"},
                    new IncomeExpenseCategories{CategoryName="Business Income",CategoryType="Income"},
                    new IncomeExpenseCategories{CategoryName="Passive Income",CategoryType="Income"}
                };
                    await _dbConnection.InsertAllAsync(objExpenseCategory);
                }
                //add income default
            }
            catch(Exception ex)
            {
                
            }
        }

        public async Task AddCurrencyListDefaults()
        {
            await SetUpDb();
            int exsistingData = await _dbConnection.Table<Currency>().CountAsync();
            if (exsistingData == 0)
            {
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
        }

        public async Task AddUserCurrencyDefault()
        {
            await SetUpDb();
            int exsistingDataUserCurrency = await _dbConnection.Table<UserCurrency>().CountAsync();
            if (exsistingDataUserCurrency == 0)
            {
                UserCurrency objUserCurrency = new UserCurrency
                {
                    Country = "India",
                    CurrencyName = "INR",
                    CurrencyCode = "en-US"
                };
                await _dbConnection.InsertAsync(objUserCurrency);
            }
        }
    }
}
