using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.DataTransferObject
{
    public class DownloadExcelDTO
    {
        public byte[] ExcelByteArray { get; set; }
        public string FileName { get; set; }
    }
}
