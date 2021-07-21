using System.IO;
using System.Reflection;

namespace Article.Pdf {


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