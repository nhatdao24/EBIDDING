using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SMO.Service.Class
{
    public class FILE_STREAM
    {
        public string PKID { get; set; }
        public string REFERENCE_ID { get; set; }
        public string FILE_OLD_NAME { get; set; }
        public string FILE_EXT { get; set; }
        public decimal FILE_SIZE { get; set; }
        public string DIRECTORY_PATH { get; set; }
        public HttpPostedFileBase FILESTREAM { get; set; }
    }
}