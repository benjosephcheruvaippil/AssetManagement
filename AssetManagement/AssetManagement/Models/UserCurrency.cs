using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models
{
    public class UserCurrency
    {
        [PrimaryKey, AutoIncrement]
        public int UserCurrencyId { get; set; }
        public string Country { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
    }
}
