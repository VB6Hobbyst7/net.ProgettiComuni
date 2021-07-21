using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ItextPDF {


    public class PDLTecEurolab {

        // external data
        public static string pathFilePDF;
        public static string pathFilePDFLocale;
        public static string fileNameLogo;
        public static string fileNameFirmaCliente;
        public static string fileNameIntestazione;
        public static string errorMessage = "";
        public static string drc;


        // ------------------------------
        // Class fields
        // ------------------------------
        
        private static readonly BaseFont font;// = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        public static Font font_FOOTER;// = new Font(font, 12, Font.NORMAL, BaseColor.BLACK);

        static PDLTecEurolab()
        { 
            font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            font_FOOTER = new Font(font, 6, Font.NORMAL, BaseColor.BLACK);
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

        //public static System.Collections.Generic.Dictionary<string, string> caption_data;
        public static string[] caption_data;

        public static int topHeaderHeight = 10;
        int leftBorder = 20;


        public class PDFFooter : PdfPageEventHelper
        {
            // write on top of document
            public override void OnOpenDocument(PdfWriter writer, Document document)
            {
                //base.OnOpenDocument(writer, document);
                //PdfPTable tabFot = new PdfPTable(new float[] { 1F });
                //tabFot.SpacingAfter = 10F;
                //PdfPCell cell;
                //tabFot.TotalWidth = 300F;
                //cell = new PdfPCell(new Phrase("Header"));
                //tabFot.AddCell(cell);
                //tabFot.WriteSelectedRows(0, -1, 150, document.Top, writer.DirectContent);
            }

            // write on start of each page
            public override void OnStartPage(PdfWriter writer, Document document)
            {
                base.OnStartPage(writer, document);
            }

            // write on end of each page
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                base.OnEndPage(writer, document);

                // immagine logo
                //includeLogo(writer, document);

                // include firma
                //includeFirma(writer, document);


                //PdfPTable tabFot = new PdfPTable(new float[] { 1F });
                //PdfPCell cell;
                //tabFot.TotalWidth = 300F;
                //cell = new PdfPCell(new Phrase("Footer"));
                //tabFot.AddCell(cell);
                PdfPTable tabFot  = new PdfPTable(1);
                tabFot.TotalWidth = 10;
                //tabFot.TotalHeight = document.PageSize.Height - 250;


                foreach (var pair in caption_data)
                {

                    //if (pair.Value.ToString() != "")
                    //{
                        PdfPCell xCell = new PdfPCell(new Phrase(new Chunk(pair, font_FOOTER)));
                        xCell.Rotation = 270;
                        //tabFot.DefaultCell.Border = 0;// Rectangle.NO_BORDER;
                        xCell.Border = Rectangle.NO_BORDER;
                        tabFot.AddCell(xCell);

                    //PdfPCell xCell2 = new PdfPCell(new Phrase(new Chunk(pair.Value, font_FOOTER)));
                        //tabFot.AddCell(xCell2);
                    //}
                }

                //PdfPCell xCell2 = new PdfPCell(new Phrase(new Chunk("IMMAGINE FIRMA", font_FOOTER)));
                //xCell2.Rowspan = tabFot.Rows.Count;
                //tabFot.AddCell(xCell2);

                tabFot.WriteSelectedRows(0, -1, document.PageSize.Width - 20, int.Parse((document.PageSize.Height / 2).ToString()), writer.DirectContent);
            }

            //write on close of document
            public override void OnCloseDocument(PdfWriter writer, Document document)
            {
                base.OnCloseDocument(writer, document);
            }
        } 
        // ------------------------------
        // Instance methods
        // ------------------------------

        public static void CombineMultiplePDFs(string[] fileNames, string outFile)
        {

            // step 1: creation of a document-object
            iTextSharp.text.Document document = new iTextSharp.text.Document();

            // step 2: we create a writer that listens to the document
            PdfCopy writer = new PdfCopy(document, new FileStream(outFile, FileMode.Create));
            if (writer == null)
            {
                return;
            }

            // step 3: we open the document
            document.Open();

            foreach (string fileName in fileNames)
            {
                if (File.Exists(fileName))
                {

                    // we create a reader for a certain document
                    PdfReader reader = new PdfReader(fileName);
                    reader.ConsolidateNamedDestinations();

                    // step 4: we add content
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        PdfImportedPage page = writer.GetImportedPage(reader, i);
                        writer.AddPage(page);
                    }

                    PRAcroForm form = reader.AcroForm;
                    if (form != null)
                    {
                        writer.CopyAcroForm(reader);
                    }

                    reader.Close();
                }
            }

            // step 5: we close the document and writer
            writer.Close();
            document.Close();

        }


        public static void Create(FileStream output) {

            
            pathFilePDFLocale = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + "tmpFile.pdf");
            //pathFilePDFLocale = Path.Combine(Path.GetTempPath(), "tmpFile.pdf");
             File.Delete(pathFilePDFLocale);
            File.Copy(pathFilePDF, pathFilePDFLocale, true);
            
            //Document document = new Document(getSizePagePDF(pathFilePDFLocale, 10, 150), 10, 10, 0, 0);
            Document document = new Document(PageSize.A4);

            try {
                PdfWriter writer = PdfWriter.GetInstance(document, output);
                writer.PageEvent = new PDFFooter();
                document.Open();

                // Load the background image and add it to the document structure
                //readerBicycle = new PdfReader(Resources.GetBicycle());
                //PdfTemplate background = writer.GetImportedPage(readerBicycle, 1);

                // Create a page in the document and add it to the bottom layer
                document.NewPage();
                _pcb = writer.DirectContent;
                //_pcb.AddTemplate(background, 0, 0);

                //_pcb.SetFontAndSize(font, 9);

                //includeIntestazione(writer, document);


                // Get the top layer and write some text
                //_pcb = writer.DirectContent;
                
                //_pcb.BeginText();


                //int writePoint = System.Convert.ToInt32(document.PageSize.Height) - topHeaderHeight + 20;
                //int step = 16;

                //_pcb.SetFontAndSize(font, 6);

                //Phrase pr = new Phrase();
                //Chunk cRet = new Chunk("\n", font_BLACK);

                //_pcb.EndText();

                includePDFScaled(writer, document, 1f);

                //includeSensoLettura(writer, document);

                //includeFirmaCliente(writer, document);

                writer.Flush();
                
            }
            finally {

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

            reader.Close();

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

        private static void includePDF(PdfWriter writer, Document doc)
        {

            //Bind a reader to the file that we created above
            PdfReader reader = new PdfReader(pathFilePDFLocale);
                        
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

                writer.DirectContentUnder.AddTemplate(imp, tm);

                //imp.ClosePath();

                //_pcb = writer.DirectContentUnder;
                //// Get the top layer and write some text
                //_pcb = writer.DirectContent;
                //_pcb.BeginText();


                //SetFont7();
                //int myInt = System.Convert.ToInt32(((doc.PageSize.Height / 2) - ((imp.Height * scale) / 2) - 40));
                //PrintTextCentered("Immagine scalata di un fattore " + scale.ToString(), 280, 100);


                //_pcb.EndText();

            }

            //reader.Close();


        }

        private static void includePDFScaled(PdfWriter writer, Document doc, float scale = 1)
        {

            //Bind a reader to the file that we created above
            PdfReader reader = new PdfReader(pathFilePDFLocale);

            //Get the number of pages in the original file
            int pageCount = reader.NumberOfPages;

            try
            {

                //Loop through each page
                for (int i = 0; i < pageCount; i++)
                {
                    //We're putting four original pages on one new page so add a new page every four pages

                        doc.NewPage();

                    //Get a page from the reader (remember that PdfReader pages are one-based)
                    var imp = writer.GetImportedPage(reader, (i + 1));

                    bool isLandscape = (imp.Width > imp.Height);
                    if (isLandscape)
                    {

                        if (imp.Width > doc.PageSize.Width)
                        {
                            //scale = (doc.PageSize.Width - 20) / imp.Width;
                            scale = (doc.PageSize.Width - (60 + doc.Bottom)) / imp.Height;

                        }
                    }
                    else
                    {
                        if (imp.Height > (doc.PageSize.Height - (60 + doc.Bottom)))
                        {
                            scale = (doc.PageSize.Height - (60 + doc.Bottom)) / imp.Height;

                        }


                    }

                    //scale = 1;
                    //A transform matrix is an easier way of dealing with changing dimension and coordinates on an rectangle
                    var tm = new System.Drawing.Drawing2D.Matrix();

                    ////Scale the image by half
                    tm.Scale(scale, scale);

                    //float translateX = 20;
                    //float translateY = doc.Bottom + 20 ;

                    //if (isLandscape)
                    //{
                    //    translateX = (doc.PageSize.Width - (imp.Height * scale)) / 2;
                    //    translateY = doc.PageSize.Height + 180;
                    //    tm.Translate(translateX, translateY);
                    //    tm.Rotate(-90f);
                        
                    //}
                    //else 
                    //{
                    //    translateX = (doc.PageSize.Width - (imp.Width * scale)) / 2;
                    //    translateY = ((doc.PageSize.Height - (imp.Height * scale)) / 2) + doc.Bottom;
                    //    tm.Translate(translateX, translateY);
                    //}
                    //Add our imported page using the matrix that we set above
                    writer.DirectContentUnder.AddTemplate(imp, tm);
                    
                    //includeDettagliCertificato(writer, doc);


                    //imp.ClosePath();

                    //_pcb = writer.DirectContentUnder;
                    //// Get the top layer and write some text
                    //_pcb = writer.DirectContent;
                    //_pcb.BeginText();


                    //SetFont7();
                    //int myInt = System.Convert.ToInt32(((doc.PageSize.Height / 2) - ((imp.Height * scale) / 2) - 40));
                    //PrintTextCentered("Immagine scalata di un fattore " + scale.ToString(), 280, 100);


                    //_pcb.EndText();

                }

                //reader.Close();

            }
            catch (System.Exception ex)
            {
                errorMessage = ex.Message;
            
            }
            //catch (PdfException pdfEX)
            //{



            //}
        }

        public static void includeLogo(PdfWriter writer, Document doc)
        {
            //Test files that we'll be creating
            if (File.Exists(fileNameLogo))
            {
                var fileImg = fileNameLogo;
                if (File.Exists(fileImg))
                {
                    iTextSharp.text.Image imp = iTextSharp.text.Image.GetInstance(fileImg);

                    float scale = 1;
                    bool isLandscape = (imp.Width > imp.Height);
                    
                    if (imp.Width > 150)
                    {
                        scale = 150 / imp.Width;
                    }

                    if ((imp.Height * scale) > 60)
                    {
                        scale = 60 / imp.Height;
                    }

                    imp.ScalePercent(scale * 100);
                    imp.SetAbsolutePosition(50, 10);
                    doc.Add(imp);

                }
            }


        }

        public static void includeFirma(PdfWriter writer, Document doc)
        {
            //Test files that we'll be creating
            if (File.Exists(fileNameFirmaCliente))
            {

                PdfPTable tabFot = new PdfPTable(1);
                tabFot.TotalWidth = 80;
                

                var fileImg = fileNameFirmaCliente;
                if (File.Exists(fileImg))
                {
                    iTextSharp.text.Image imp = iTextSharp.text.Image.GetInstance(fileImg);

                    float scale = 1;
                    bool isLandscape = (imp.Width > imp.Height);

                    if (imp.Width > 60)
                    {
                        scale = 60 / imp.Width;
                    }

                    if ((imp.Height * scale) > 40)
                    {
                        scale = 40 / imp.Height;
                    }

                    imp.ScalePercent(scale * 100);
                    //imp.SetAbsolutePosition(doc.PageSize.Width-120, 10);
                    //doc.Add(imp);

                    //PdfPCell xCell = new PdfPCell(new Phrase(new Chunk(string.Format("Torino, {0}/{1}/{2}",System.DateTime.Now.Day,System.DateTime.Now.Month,System.DateTime.Now.Year), font_FOOTER)));
                    PdfPCell xCell = new PdfPCell(new Phrase(new Chunk(drc, font_FOOTER)));
                    tabFot.AddCell(xCell);
                    imp.SetAbsolutePosition(10, 10);
                    PdfPCell xCellImg = new PdfPCell(imp);

                    xCellImg.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    xCellImg.VerticalAlignment = PdfPCell.ALIGN_CENTER;
                    xCellImg.Padding = 2;

                    tabFot.AddCell(xCellImg);

                    tabFot.WriteSelectedRows(0, -1, 460, doc.Bottom, writer.DirectContent);
                    

                }
            }


        }
    
    }


}