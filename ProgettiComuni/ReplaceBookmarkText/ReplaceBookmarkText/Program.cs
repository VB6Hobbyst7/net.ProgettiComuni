using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;

namespace ReplaceBookmarkText
{
    class Program
    {
        static void Main(string[] args)
        {
            File.Delete("../../Test01out.docx");
            File.Copy("../../Test01.docx", "../../Test01out.docx");
            using (WordprocessingDocument doc =
                WordprocessingDocument.Open("../../Test01out.docx", true))
            {
                Console.WriteLine("Test01.docx Bookmark A: >{0}<",
                    BookmarkReplacer.GetBookmarkText(doc, "A"));
                BookmarkReplacer.ReplaceBookmarkText(doc, "A", "abc");
            }
            File.Delete("../../Test02out.docx");
            File.Copy("../../Test02.docx", "../../Test02out.docx");
            using (WordprocessingDocument doc =
                WordprocessingDocument.Open("../../Test02out.docx", true))
            {
                Console.WriteLine("Test02.docx Bookmark B: >{0}<",
                    BookmarkReplacer.GetBookmarkText(doc, "B"));
                BookmarkReplacer.ReplaceBookmarkText(doc, "B", "");
            }
            File.Delete("../../Test03out.docx");
            File.Copy("../../Test03.docx", "../../Test03out.docx");
            using (WordprocessingDocument doc =
                WordprocessingDocument.Open("../../Test03out.docx", true))
            {
                Console.WriteLine("Test03.docx Bookmark C: >{0}<",
                    BookmarkReplacer.GetBookmarkText(doc, "C"));
                BookmarkReplacer.ReplaceBookmarkText(doc, "C", "xyz");
            }
            File.Delete("../../Test04out.docx");
            File.Copy("../../Test04.docx", "../../Test04out.docx");
            using (WordprocessingDocument doc =
                WordprocessingDocument.Open("../../Test04out.docx", true))
            {
                Console.WriteLine("Test04.docx Bookmark ABC: >{0}<",
                    BookmarkReplacer.GetBookmarkText(doc, "ABC"));
                BookmarkReplacer.ReplaceBookmarkText(doc, "ABC", "was a");
            }

            try
            {
                File.Delete("../../Test05out.docx");
                File.Copy("../../Test05.docx", "../../Test05out.docx");
                using (WordprocessingDocument doc =
                    WordprocessingDocument.Open("../../Test05out.docx", true))
                {
                    BookmarkReplacer.ReplaceBookmarkText(doc, "ABC", "was a");
                }
            }
            catch (BookmarkReplacerException e)
            {
                Console.WriteLine(e.Message);
            }

            try
            {
                File.Delete("../../Test06out.docx");
                File.Copy("../../Test06.docx", "../../Test06out.docx");
                using (WordprocessingDocument doc =
                    WordprocessingDocument.Open("../../Test06out.docx", true))
                {
                    BookmarkReplacer.ReplaceBookmarkText(doc, "abc", "123");
                }
            }
            catch (BookmarkReplacerException e)
            {
                Console.WriteLine(e.Message);
            }
            try
            {
                File.Delete("../../Test07out.docx");
                File.Copy("../../Test07.docx", "../../Test07out.docx");
                using (WordprocessingDocument doc =
                    WordprocessingDocument.Open("../../Test07out.docx", true))
                {
                    Console.WriteLine("Test07.docx Bookmark abc: >{0}<",
                        BookmarkReplacer.GetBookmarkText(doc, "abc"));
                    BookmarkReplacer.ReplaceBookmarkText(doc, "abc", "123");
                }
            }
            catch (BookmarkReplacerException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
