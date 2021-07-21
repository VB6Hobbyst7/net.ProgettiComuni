using System.IO;
using System;
using System.Collections;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ItextPDF {


    public class SchedaReclamo {

        // external data
        public static string pathFilePDF;
        public static string pathFilePDFLocale;
        public static string fileNameAvvolgimento;
        public static string fileNameFirmaCliente;

        public static System.Collections.Specialized.StringCollection arrayAttributi;

        

        // ------------------------------
        // Class fields
        // ------------------------------
        
        
        private static readonly BaseFont font;// = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);

        public static Font font_BLACK;// = new Font(font, 12, Font.NORMAL, BaseColor.BLACK);
        public static Font fontB_BLACK;// = new Font(font, 12, Font.BOLD, BaseColor.BLACK);
        public static Font fontO_BLACK;// = new Font(font, 12, Font.ITALIC, BaseColor.BLACK);
        public static Font fontBO_BLACK;// = new Font(font, 12, Font.BOLDITALIC, BaseColor.BLACK);
        public static Font font_RED;// = new Font(font, 12, Font.NORMAL, BaseColor.RED);
        public static Font fontB_RED;// = new Font(font, 12, Font.BOLD, BaseColor.RED);
        public static Font fontO_RED;// = new Font(font, 12, Font.ITALIC, BaseColor.RED);
        public static Font fontBO_RED;// = new Font(font, 12, Font.BOLDITALIC, BaseColor.RED);

        static SchedaReclamo()
        { 
            font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);

            font_BLACK = new Font(font, 12, Font.NORMAL, BaseColor.BLACK);
            fontB_BLACK = new Font(font, 12, Font.BOLD, BaseColor.BLACK);
            fontO_BLACK = new Font(font, 12, Font.ITALIC, BaseColor.BLACK);
            fontBO_BLACK = new Font(font, 12, Font.BOLDITALIC, BaseColor.BLACK);
            font_RED = new Font(font, 12, Font.NORMAL, BaseColor.RED);
            fontB_RED = new Font(font, 12, Font.BOLD, BaseColor.RED);
            fontO_RED = new Font(font, 12, Font.ITALIC, BaseColor.RED);
            fontBO_RED = new Font(font, 12, Font.BOLDITALIC, BaseColor.RED);
        
        }


        // ------------------------------
        // Instance fields
        // ------------------------------

        private readonly string _name;
        private readonly string _distance;
        private readonly string _date;
        private readonly string _raceName;
        public static readonly bool _showRulers;

        public static PdfContentByte _pcb;

        // ------------------------------
        // Constructors
        // ------------------------------

        //public SchedaTecnica(string name, string distance, string date, string raceName, bool showRulers) {
        //    _name = name;
        //    _distance = distance;
        //    _date = date;
        //    _raceName = raceName;
        //    _showRulers = showRulers;
        //}

        public static int topHeaderHeight = 180;
        int leftBorder = 30;

        

        // ------------------------------
        // Instance methods
        // ------------------------------
        
        public static void Create(FileStream output, System.Collections.Generic.Dictionary<string, string> caption_data) 
        {
            string[] printAttributi = new string[arrayAttributi.Count];
            arrayAttributi.CopyTo(printAttributi, 0);

            Document document = new Document(PageSize.A4, 2, 2, 10, 10);

            try
            {
                fileNameFirmaCliente = "FIRMACLIENTE.JPG";

                pathFilePDFLocale = Path.Combine(Path.GetTempPath(), "tmpFile.pdf");
                //File.Delete(pathFilePDFLocale);
                //File.Copy(pathFilePDF, pathFilePDFLocale, true);



                PdfWriter writer = PdfWriter.GetInstance(document, output);
                document.Open();

                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 80;

                string intestazione = "ANALISI RILEVAZIONE NON CONFORMITA' - {0}";



                //Paragraph p = new Paragraph();
                //Phrase pr = new Phrase();
                //Chunk cRet = new Chunk("\n", font_BLACK);

                foreach (var pair in caption_data)
                {
                    
                    //if (pair.Key.Contains("Pubblicazioni collegate-Oggetto2"))
                    //{
                    for (int ia = 0; ia < arrayAttributi.Count; ia++)
                    {
                        string[] attributo = arrayAttributi[ia].Split(Char.Parse("|"));

                        if (attributo[0] != "")
                        {
                            if (pair.Key.ToUpper().Contains(attributo[0].ToUpper()))
                            //if (pair.Key.ToUpper() == attributo[0].ToUpper())
                            {

                                if (pair.Key.ToUpper().Contains("TIPO RILAVORAZIONE"))
                                {
                                    PdfPCell cell;
                                    if (pair.Value.ToUpper() == "RIC")
                                    {
                                        // intestazione

                                        cell = new PdfPCell(new Phrase(string.Format(intestazione, "DIFETTOSITA' CLIENTE")));
                                        cell.BackgroundColor = CMYKColor.RED;
                                        
                                    }
                                    else
                                    {
                                        cell = new PdfPCell(new Phrase(string.Format(intestazione, "DIFETTOSITA' INTERNE")));
                                        cell.BackgroundColor = CMYKColor.YELLOW;

                                    }

                                    cell.Colspan = 4;
                                    cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                                    table.AddCell(cell);
                                }


                                if (pair.Value.ToString() != "")
                                {
                                    PdfPCell xCell = new PdfPCell(new Phrase(attributo[1]));

                                    table.AddCell(xCell);

                                    PdfPCell xCell2 = new PdfPCell(new Phrase(pair.Value));
                                    xCell2.Colspan = 3;
                                    table.AddCell(xCell2);
                                }

                                break;
                            }
                        }

                    }
                    arrayAttributi.Contains(pair.Key.Substring(0, pair.Key.Length - 2));
                    

                        
                    //}

                    //if (pair.Key.ToUpper().Contains("CLIENTE"))
                    //{
                    //    //cx = new Chunk(string.Format("{0} {1}", pair.Key, pair.Value.ToString()), fontB_RED);
                    //    table.AddCell(pair.Key.ToString());
                    //    table.AddCell(caption_data[pair.Key].ToString());
                    //    //t.AddCell(caption_data[pair.Value].ToString());
                    //}
                    //else
                    //{
                    //    //cx = new Chunk(string.Format("{0} {1}", pair.Key, pair.Value), font_BLACK);
                    //    table.AddCell(pair.Key.ToString());
                    //    table.AddCell(caption_data[pair.Key].ToString());
                    //    //t.AddCell(caption_data[pair.Value].ToString());
                    //}

                    //pr.Add(cx);

                    //Chunk linebreak = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(.3f, 100f, GrayColor.LIGHT_GRAY, Element.ALIGN_CENTER, -1));
                    //pr.Add(linebreak);

                    //pr.Add(cRet);

                }

                //p.Add(t);

                document.Add(table);

                //_pcb.EndText();

                //includePDF(writer, document);

                //includeSensoLettura(writer, document);

                //includeFirmaCliente(writer, document);

                writer.Flush();

            }
            catch (Exception e)
            { 
                
            }
            finally {
                //if (readerBicycle!= null) {
                //    readerBicycle.Close();
                //}
                document.Close();
            }
        }


        public static Rectangle getSizePagePDF(string source, float addWidth, float addHeight)
        {
            
            PdfReader reader = new PdfReader(source);
            Rectangle pagesize = reader.GetPageSizeWithRotation(1);
            Rectangle newPageSize = null;

            if (pagesize.Rotation == 0)
            {
                if (pagesize.Width > (PageSize.A4.Width - addWidth))
                {
                    newPageSize = new Rectangle(pagesize.Width + addWidth, pagesize.Height + addHeight);
                }
            }
            else
            {
                if (pagesize.Height > (PageSize.A4.Height - addHeight))
                {
                    newPageSize = new Rectangle(pagesize.Width + addWidth, pagesize.Height + addHeight);
                }       
            }

            if (newPageSize == null)
                newPageSize = PageSize.A4;

            return newPageSize;


        }

        public static void SetFont7()
        {
            _pcb.SetFontAndSize(font, 7);
        }

        public static void SetFont18()
        {
            _pcb.SetFontAndSize(font, 18);
        }

        public static void SetFont36()
        {
            _pcb.SetFontAndSize(font, 36);
        }

        public static void PrintText(string text, int x, int y)
        {
            _pcb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, x, y, 0);
        }

        public static void PrintTextCentered(string text, int x, int y)
        {
            _pcb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, text, x, y, 0);
        }

        public static void PrintXAxis(int y)
        {
            SetFont7();
            int x = 600;
            while(x>=0) {
                if (x%20==0) {
                    PrintTextCentered(""+x, x, y+8);
                    PrintTextCentered("|", x, y);
                }
                else {
                    PrintTextCentered(".",x,y);
                }
                x -= 5;
            }
        }

        public static void PrintYAxis(int x)
        {
            SetFont7();
            int y = 800;
            while(y>=0) {
                if (y%20==0) {
                    PrintText("__ "+y, x, y);
                }
                else {
                    PrintText("_", x, y);
                }
                y = y - 5;
            }
        }

        public static void includePDF(PdfWriter writer, Document doc)
        {
            //Test files that we'll be creating
            //var file2 = Path.Combine(@"C:\_VM_Shared\Notarianni\immaginiBOZZE\Work", "x_2.pdf");
            //File.Copy(pathFilePDF, Path.Combine(@"C:\_VM_Shared\Notarianni\immaginiBOZZE\Work", "tmpFile.pdf"), true);
            //var file1 = Path.Combine(@"D:\SchedeTecniche", "x.pdf");
            

            //Bind a reader to the file that we created above
//            PdfReader reader = new PdfReader(Path.Combine(@"C:\_VM_Shared\Notarianni\immaginiBOZZE\Work", "x.pdf"));
            PdfReader reader = new PdfReader(pathFilePDFLocale);
            {
                        

                //Get the number of pages in the original file
                int pageCount = reader.NumberOfPages;

                //Loop through each page
                for (int i = 0; i < pageCount; i++) {
                    //We're putting four original pages on one new page so add a new page every four pages
                    if (i % 4 > 0) {
                        doc.NewPage();
                    }

                    //Get a page from the reader (remember that PdfReader pages are one-based)
                    var imp = writer.GetImportedPage(reader, (i + 1));
                    float scale = 1;
                    bool isLandscape = (imp.Width > imp.Height);
                    if (isLandscape)
                    {
                        if (imp.Width > doc.PageSize.Width)
                        {
                            scale = (doc.PageSize.Width - 10) / imp.Width;

                        }
                    }
                    else
                    {
                        if (imp.Height > 600)
                        {
                            scale = 600 / imp.Height;

                        }
                    
                    
                    }
                    //A transform matrix is an easier way of dealing with changing dimension and coordinates on an rectangle
                    var tm = new System.Drawing.Drawing2D.Matrix();

                    //Scale the image by half
                    tm.Scale(scale, scale);

                    float translateX = 0;
                    float translateY = 0;

                    if (isLandscape)
                    {
                        translateX = 5;
                        translateY = (doc.PageSize.Height / 2) - ((imp.Height * scale) / 2);
                    }
                    else
                    {
                        translateX = (doc.PageSize.Width / 2) - ((imp.Width * scale) / 2);
                        translateY = (doc.PageSize.Height / 2) - ((imp.Height * scale) / 2)-100;                   
                    }

                    tm.Translate(translateX, translateY);
                    //Add our imported page using the matrix that we set above
                    writer.DirectContent.AddTemplate(imp,tm);


                    _pcb = writer.DirectContentUnder;
                    // Get the top layer and write some text
                    _pcb = writer.DirectContent;
                    _pcb.BeginText();


                    SetFont7();
                    int myInt = System.Convert.ToInt32(((doc.PageSize.Height / 2) - ((imp.Height * scale) / 2) - 40));
                    PrintTextCentered("Immagine scalata di un fattore " + scale.ToString(), 280, 100);


                    _pcb.EndText();

                }

                reader.Close();


            }



        }

        //public static void getPageSizePDF(PdfWriter writer, Document doc)
        //{
        //    //Test files that we'll be creating
        //    //var file2 = Path.Combine(@"C:\_VM_Shared\Notarianni\immaginiBOZZE\Work", "x_2.pdf");
        //    File.Copy(pathFilePDF, Path.Combine(@"C:\_VM_Shared\Notarianni\immaginiBOZZE\Work", "tmpFile.pdf"), true);
        //    //var file1 = Path.Combine(@"D:\SchedeTecniche", "x.pdf");


        //    //Bind a reader to the file that we created above
        //    //            PdfReader reader = new PdfReader(Path.Combine(@"C:\_VM_Shared\Notarianni\immaginiBOZZE\Work", "x.pdf"));
        //    PdfReader reader = new PdfReader(pathFilePDF);
        //    {


        //        //Get the number of pages in the original file
        //        int pageCount = reader.NumberOfPages;

        //        //Loop through each page
        //        for (int i = 0; i < pageCount; i++)
        //        {
        //            //We're putting four original pages on one new page so add a new page every four pages
        //            if (i % 4 > 0)
        //            {
        //                doc.NewPage();
        //                doc.SetPageSize(new Rectangle(doc.PageSize.Width + 50, doc.PageSize.Height + 500));
        //            }

        //            //Get a page from the reader (remember that PdfReader pages are one-based)
        //            var imp = writer.GetImportedPage(reader, (i + 1));
        //            float scale = 1;
        //            bool isLandscape = (imp.Width > imp.Height);
        //            if (isLandscape)
        //            {
        //                if (imp.Width > doc.PageSize.Width)
        //                {
        //                    scale = (doc.PageSize.Width - 10) / imp.Width;

        //                }
        //            }
        //            else
        //            {
        //                if (imp.Height > 600)
        //                {
        //                    scale = 600 / imp.Height;

        //                }


        //            }
        //            //A transform matrix is an easier way of dealing with changing dimension and coordinates on an rectangle
        //            var tm = new System.Drawing.Drawing2D.Matrix();

        //            //Scale the image by half
        //            tm.Scale(scale, scale);

        //            float translateX = 0;
        //            float translateY = 0;

        //            if (isLandscape)
        //            {
        //                translateX = 5;
        //                translateY = (doc.PageSize.Height / 2) - ((imp.Height * scale) / 2);
        //            }
        //            else
        //            {
        //                translateX = (doc.PageSize.Width / 2) - ((imp.Width * scale) / 2);
        //                translateY = (doc.PageSize.Height / 2) - ((imp.Height * scale) / 2) - 100;
        //            }

        //            tm.Translate(translateX, translateY);
        //            //Add our imported page using the matrix that we set above
        //            writer.DirectContent.AddTemplate(imp, tm);


        //            _pcb = writer.DirectContentUnder;
        //            // Get the top layer and write some text
        //            _pcb = writer.DirectContent;
        //            _pcb.BeginText();


        //            SetFont7();
        //            int myInt = System.Convert.ToInt32(((doc.PageSize.Height / 2) - ((imp.Height * scale) / 2) - 40));
        //            PrintTextCentered("Immagine scalata di un fattore " + scale.ToString(), 280, 100);


        //            _pcb.EndText();

        //        }

        //        reader.Close();


        //    }
        //}

        public static void includeIntestazione(PdfWriter writer, Document doc)
        {
            //Test files that we'll be creating
            var fileImg = Path.Combine(@"C:\_VM_Shared\Notarianni\immaginiBOZZE\Work", "intestazione.jpg");

            iTextSharp.text.Image imp = iTextSharp.text.Image.GetInstance(fileImg);

            float scale = 1;
            bool isLandscape = (imp.Width > imp.Height);
            if (imp.Width > doc.PageSize.Width)
            {
                scale = (doc.PageSize.Width - 50) / imp.Width;

            }

            imp.ScalePercent((float)(scale*100));

            doc.Add(imp);

            ////A transform matrix is an easier way of dealing with changing dimension and coordinates on an rectangle
            //var tm = new System.Drawing.Drawing2D.Matrix();

            ////Scale the image by half
            //tm.Scale(scale, scale);

            //float translateX = 0;
            //float translateY = doc.PageSize.Height;

            //tm.Translate(translateX, translateY);
            ////Add our imported page using the matrix that we set above
            //writer.DirectContent.AddImage(imp, tm);

        }



        public static void includeSensoLettura(PdfWriter writer, Document doc)
        {
            //Test files that we'll be creating
            var fileImg = Path.Combine(@"C:\_VM_Shared\Notarianni\immaginiBOZZE\Work", fileNameAvvolgimento);
            if (File.Exists(fileImg))
            {

                iTextSharp.text.Image imp = iTextSharp.text.Image.GetInstance(fileImg);

                float scale = 1;
                bool isLandscape = (imp.Width > imp.Height);
                if (imp.Width > 200)
                {
                    scale = 200 / imp.Width;

                }

                imp.ScalePercent(scale * 100);
                imp.SetAbsolutePosition(20, 20);
                doc.Add(imp);

            }

        }

        public static void includeFirmaCliente(PdfWriter writer, Document doc)
        {
            //Test files that we'll be creating
            var fileImg = Path.Combine(@"C:\_VM_Shared\Notarianni\immaginiBOZZE\Work", fileNameFirmaCliente);
            if (File.Exists(fileImg))
            { 
            
                iTextSharp.text.Image imp = iTextSharp.text.Image.GetInstance(fileImg);

                float scale = 1;
                bool isLandscape = (imp.Width > imp.Height);
                if (imp.Width > 200)
                {
                    scale = 200 / imp.Width;

                }

                imp.ScalePercent(scale*100);
                imp.SetAbsolutePosition(250, 20);
                doc.Add(imp);   
         
            }    


        }
    
    
    }


}