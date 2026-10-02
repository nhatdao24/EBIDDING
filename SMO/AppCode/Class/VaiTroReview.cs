using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMO
{
    public static class VaiTroReview
    {
        public const string BanDieuHanh = "01";
        public const string CungUng = "02";

        public static string GetText(string type)
        {
            if (type == BanDieuHanh)
            {
                return "Ban điều hành";
            }

            else if (type == CungUng)
            {
                return "Cung ứng";
            }
            return "Cả hai";
        }
    }
}