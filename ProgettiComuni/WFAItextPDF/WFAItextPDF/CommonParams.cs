using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WFAItextPDF
{
    class CommonParams
    {
        public static string[] files;
        public static string fOut = "";
        public static string[] data;
        public static string logo;
        public static string firma;
        public static string drc = string.Format("Torino, {0}/{1}/{2}", System.DateTime.Now.Day, System.DateTime.Now.Month, System.DateTime.Now.Year);
    }
}
