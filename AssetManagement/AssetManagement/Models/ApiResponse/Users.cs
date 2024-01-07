using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.ApiResponse
{
    public class Users
    {
        public int total { get; set; }
        public List<Data> data { get; set; }
    }

    public class Data
    {
        public int id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
    }
}
