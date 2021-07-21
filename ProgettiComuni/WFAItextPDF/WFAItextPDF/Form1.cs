using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

using ItextPDF;

namespace WFAItextPDF
{
    public partial class Form1 : Form
    {
        //public static string[] files;
        //public static string fOut = "";
        //public static string[] data;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
             PDFMergeWithFooter();
            
        }

        public void  PDFMergeWithFooter()
        {

            string f = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".pdf");

            if (CommonParams.files.Length == 1)
            {
                File.Copy(CommonParams.files[0], f, true);
            }

            if (CommonParams.files.Length > 1)
            {
                //ItextPDF.ResizePDFGiraudi.CombineMultiplePDFs(CommonParams.files, f);
                ItextPDF.PDLTecEurolab.CombineMultiplePDFs(CommonParams.files, f);
            }


            //string fOut = @"D:\_VM_Shared\ProgettiComuni\WFAItextPDF\testX.pdf";

            Cursor.Current = Cursors.WaitCursor;

            System.Collections.Generic.Dictionary<string, string> attrVal = new System.Collections.Generic.Dictionary<string, string>();

            //foreach (ListViewItem li in listViewAttr.Items)
            //{
            //    attrVal.Add(li.SubItems[0].Text + "|" + li.Index.ToString(), li.SubItems[1].Text);
            //}


            MemoryStream ms = new MemoryStream();

            string strTMPPDF = CommonParams.fOut;// Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".pdf");
            FileStream fs = new FileStream(strTMPPDF, FileMode.Create);

            try
            {

                //foreach (DataGridViewRow r in dgFeedback.Rows)
                //{
                //    if (r.Cells["FEEDBACKCAUSA"].Value!= null)
                //    {
                //        if (r.Cells["FEEDBACKCAUSA"].Value.ToString() == "1")
                //        {
                //attrVal.Add(string.Format("R-M-O-{0}", r.Index.ToString()), string.Format("{0} - {1}", r.Cells["CODMACCHINA"].Value, r.Cells["DESCRIZIONEOPERATORE"].Value));

                //attrVal.Add(string.Format("COPIA CERTIFICATO valido {0} di ", "qta"), "");
                //attrVal.Add(string.Format("Materiale {0}", "mat"), "");
                //attrVal.Add(string.Format("fornito con D.D.T.n. {0} del {1}", "nddt", "dataddt"), "");
                //attrVal.Add(string.Format("alla Ditta {0}", "cliente"), "");
                //attrVal.Add(string.Format("Torino {0}", "Fratelli Gidaurdi spa Via Orbetello 124-TO"), "");

                //        }
                //    }

                //}
                ItextPDF.ResizePDFGiraudi.pathFilePDF = f;
                ItextPDF.ResizePDFGiraudi.caption_data = CommonParams.data;
                ItextPDF.ResizePDFGiraudi.fileNameLogo = CommonParams.logo;
                ItextPDF.ResizePDFGiraudi.fileNameFirmaCliente = CommonParams.firma;

                ItextPDF.ResizePDFGiraudi.drc = CommonParams.drc;

                ItextPDF.ResizePDFGiraudi.Create(fs);


                ////ItextPDF.PDLTecEurolab.pathFilePDF = f;
                ////ItextPDF.PDLTecEurolab.caption_data = CommonParams.data;
                ////ItextPDF.PDLTecEurolab.fileNameLogo = CommonParams.logo;
                ////ItextPDF.PDLTecEurolab.fileNameFirmaCliente = CommonParams.firma;

                ////ItextPDF.PDLTecEurolab.drc = CommonParams.drc;

                ////ItextPDF.PDLTecEurolab.Create(fs);
                //Article.Pdf.SchedaTecnica.Create(fs, attrVal);
                string strFilePDFPDL = f;

                //MessageBox.Show("Elaborazione terminata");


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
                fs.Close();
                Cursor.Current = Cursors.Default;

            }

            Cursor.Current = Cursors.Default;        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string[] files = { @"D:\_VM_Shared\ProgettiComuni\WFAItextPDF\1.pdf", @"D:\_VM_Shared\ProgettiComuni\WFAItextPDF\2.pdf", @"D:\_VM_Shared\ProgettiComuni\WFAItextPDF\3.pdf" };
            ItextPDF.ResizePDFGiraudi.CombineMultiplePDFs(CommonParams.files, CommonParams.fOut);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            this.Text =string.Format("ItaPDF {0}", Application.ProductVersion);

            try
            {

                if ((CommonParams.files.Length > 0) && (CommonParams.fOut != ""))
                {
                    
                    textBox1.Text += "\r\n" + CommonParams.fOut;

                    // elimino file output
                    File.Delete(CommonParams.fOut);

                    if (CommonParams.data != null)
                    {
                        PDFMergeWithFooter();
                    }
                    else
                    {
                        //ItextPDF.ResizePDFGiraudi.CombineMultiplePDFs(CommonParams.files, CommonParams.fOut);
                        ItextPDF.PDLTecEurolab.CombineMultiplePDFs(CommonParams.files, CommonParams.fOut);
                    }


                    if (ItextPDF.ResizePDFGiraudi.errorMessage != "")
                    {
                        textBox1.Text += "\r\n ERRORE \r\n" + ItextPDF.ResizePDFGiraudi.errorMessage;
                    }

                    textBox1.Text += "\r\n" + "Elaborazione terminata";


                    webBrowser1.Navigate(CommonParams.fOut);

                }
                else
                {

                    textBox1.Text += string.Format("CommonParams.files.Length {0}\r\n" + "nessun parametro passato al programma", CommonParams.files.Length);

                }

                //this.Close();

            }
            catch (Exception ex)
            {

                textBox1.Text += "\r\n" + ex.Message;

                if(ItextPDF.ResizePDFGiraudi.errorMessage != "")
                {
                textBox1.Text += "\r\n" + ItextPDF.ResizePDFGiraudi.errorMessage;
                }
                    
                textBox1.Text += "\r\n" + CommonParams.fOut;

                for (int i = 0; i < CommonParams.files.Length; i++)
                {
                    textBox1.Text += "\r\n" + CommonParams.files[i];
                }

                textBox1.Text += "\r\n" + CommonParams.logo;

                textBox1.Text += "\r\n" + CommonParams.firma;

                for (int i = 0; i < CommonParams.data.Length; i++)
                {
                    textBox1.Text += "\r\n" + CommonParams.data[i];
                }

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            webBrowser1.Navigate("about:blank");
            webBrowser1.Dispose();
        }
    }
}
