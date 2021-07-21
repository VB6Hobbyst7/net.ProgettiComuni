using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceDirectory = Properties.Settings.Default.KnoS_Files; // @"C:\current";
            string archiveDirectory = @"C:\archive";

            string srcpattern = string.Format("{0}*.pdf", "404");
            Console.WriteLine(sourceDirectory);

            try
            {
                var txtFiles = Directory.EnumerateFiles(sourceDirectory, srcpattern, SearchOption.AllDirectories);

                Console.WriteLine(txtFiles.Count());

                foreach (string currentFile in txtFiles)
                {
                    string fileName = currentFile.Substring(sourceDirectory.Length + 1);
                    //Directory.Move(currentFile, Path.Combine(archiveDirectory, fileName));
                    Console.WriteLine(fileName);
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }
    }
}
