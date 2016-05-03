using System;
using System.Text.RegularExpressions;
using System.Web;

namespace HostsHandler
{
    public class Helper 
    {
        public static string footerFormat = "#end of {0}";

        public static string test() { return ""; }

        public static Match getIPMatchFromString(string inputString)
        {
            return Regex.Match(inputString, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");
        }

    }
}
