using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models
{
    public class AssetDocuments
    {
        [PrimaryKey, AutoIncrement]
        public int AssetDocumentsId { get; set; }
        public int AssetId { get; set; }
        public string FileId { get; set; }
        public string FilePath { get; set; }
        public string FileFormat { get; set; }
    }

    public class FileList
    {
        public string FileId { get; set; }
        public string FilePath { get; set; }
    }
}
