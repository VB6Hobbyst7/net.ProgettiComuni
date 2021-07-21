using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace WFAItextPDF
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

        
        [STAThread]
        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                //MessageBox.Show(args.Length.ToString());

                if (args[0].StartsWith("/f"))
                {
                    args[0] = args[0].Substring(2);
                    string[] filesInput = args[0].Split(char.Parse("|"));

                    for (int i = 0; i < filesInput.Length; i++)
                    {
                        //MessageBox.Show(filesInput[i]);
                    }

                    CommonParams.files = filesInput;
                
                }

                if (args[1].StartsWith("/out"))
                {
                    args[1] = args[1].Substring(4);
                    CommonParams.fOut = args[1];
                }

                if (args.Length >= 3)
                {
                    if (args[2].StartsWith("/data"))
                    {
                        args[2] = args[2].Substring(5);
                        string[] dataString = args[2].Split(char.Parse("|"));

                        for (int i = 0; i < dataString.Length; i++)
                        {
                            //MessageBox.Show(dataString[i]);
                        }

                        CommonParams.data = dataString;

                    }
                }

                if (args.Length >= 4)
                {
                    if (args[3].StartsWith("/logo"))
                    {
                        args[3] = args[3].Substring(5);
                        CommonParams.logo = args[3];
                    }
                }

                if (args.Length >= 5)
                {
                    if (args[4].StartsWith("/firma"))
                    {
                        args[4] = args[4].Substring(6);
                        CommonParams.firma = args[4];
                    }
                }

                if (args.Length >= 6)
                {
                    if (args[5].StartsWith("/drc"))
                    {
                        args[5] = args[5].Substring(4);
                        CommonParams.drc = string.Format("Torino, {0}", args[5]);
                    }
                }

            
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Form1 frm = new Form1();
                Application.Run(frm);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }
    }
}
