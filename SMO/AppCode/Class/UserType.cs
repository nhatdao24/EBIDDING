using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMO
{
    public static class UserType
    {
        /// <summary>
        /// User khách hàng
        /// </summary>
        public const string NB = "NB";
        /// <summary>
        /// User cửa hàng
        /// </summary>
        public const string NhaThau = "NT";
        public static string GetText(string type, string lang = "vi")
        {
            if (lang == "vi")
            {
                if (type == NB)
                {
                    return "CTD";
                }

                else if (type == NhaThau)
                {
                    return "Nhà thầu";
                }
                else
                {
                    return "Chưa thiết lập";
                }
            }
            else
            {
                if (type == NB)
                {
                    return "CTD";
                }

                else if (type == NhaThau)
                {
                    return "Contractor";
                }
                else
                {
                    return "Not config";
                }
            }
        }
    }
}