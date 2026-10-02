using System.Collections.Generic;

namespace SMO
{
    public static class Global
    {
        //public static SAPDestinationConfig DestinationConfig = new SAPDestinationConfig();
        public const string ApplicationName = "SMO";
        public const string LanguageDefault = "vi";
        public const string DateSAPFormat = "yyyy-MM-dd";
        public const string NumberFormat = "#,#0.##";
        public const string DateToStringFormat = "dd/MM/yyyy";
        public const string DateTimeToStringFormat = "dd/MM/yyyy HH:mm";
        public const string KeyMaHoa = "D2s@1234!@#";
        public static string IdSpecPrice = "";
        public static string IsViettinBank = "";

        public static Dictionary<string, int> ListIPLogin = new Dictionary<string, int>();
    }
}