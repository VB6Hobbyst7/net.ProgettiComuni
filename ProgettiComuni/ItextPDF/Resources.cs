using System.IO;
using System.Reflection;

namespace ItextPDF {


    public static class Resources {

        // ------------------------------
        // Class fields
        // ------------------------------

        private const string KeyBicycle = "bicycle.pdf";

        // ------------------------------
        // Class methods
        // ------------------------------

        public static Stream GetBicycle() {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof (Resources), KeyBicycle);
        }


    }


}