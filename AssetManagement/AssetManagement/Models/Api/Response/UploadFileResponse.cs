using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Models.Api.Response
{
    public class UploadFileResponse
    {
        public string? FileId { get; set; }
        public string? FileFormat { get; set; }
        public string? FileViewPath { get; set; }
        public bool? IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}
