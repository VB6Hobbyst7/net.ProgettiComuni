using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Data;
using System.Windows.Forms;


namespace ExcelImportLib{
//{
//    public class ExcelExport
//    {
//        public static Microsoft.Office.Interop.Excel.Application excelApp;
//        public static Microsoft.Office.Interop.Excel.Workbook excelWorkbook;

//        public static void ExportToExcel(DataSet dataSet, string outputPath)
//        {
//            // Create the Excel Application object
//            excelApp = new Microsoft.Office.Interop.Excel.Application();

//            // Create a new Excel Workbook
//            excelWorkbook = excelApp.Workbooks.Add(Type.Missing);

//            int sheetIndex = 0;

//            // Copy each DataTable
//            foreach (System.Data.DataTable dt in dataSet.Tables)
//            {

//                // Copy the DataTable to an object array
//                string[,] rawData = new string[dt.Rows.Count + 1, dt.Columns.Count];

//                // Copy the column names to the first row of the object array
//                for (int col = 0; col < dt.Columns.Count; col++)
//                {
//                    rawData[0, col] = dt.Columns[col].ColumnName;
//                }

//                // Copy the values to the object array
//                for (int col = 0; col < dt.Columns.Count; col++)
//                {
//                    for (int row = 0; row < dt.Rows.Count; row++)
//                    {
//                        rawData[row + 1, col] = dt.Rows[row].ItemArray[col].ToString();
//                    }
//                }

//                // Calculate the final column letter
//                string finalColLetter = string.Empty;
//                string colCharset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
//                int colCharsetLen = colCharset.Length;

//                if (dt.Columns.Count > colCharsetLen)
//                {
//                    finalColLetter = colCharset.Substring(
//                        (dt.Columns.Count - 1) / colCharsetLen - 1, 1);
//                }

//                finalColLetter += colCharset.Substring(
//                        (dt.Columns.Count - 1) % colCharsetLen, 1);

//                // Create a new Sheet
//                Worksheet excelSheet = (Worksheet)excelWorkbook.Sheets.Add(
//                    excelWorkbook.Sheets.get_Item(++sheetIndex),
//                    Type.Missing, 1, XlSheetType.xlWorksheet);

//                excelSheet.Name = dt.TableName;

//                // Fast data export to Excel
//                string excelRange = string.Format("A1:{0}{1}",
//                    finalColLetter, dt.Rows.Count + 1);

//                excelSheet.get_Range(excelRange, Type.Missing).Value2 = rawData;

//                // Mark the first row as BOLD
//                ((Range)excelSheet.Rows[1, Type.Missing]).Font.Bold = true;
//            }

//            // Save and Close the Workbook
//            excelWorkbook.SaveAs(outputPath, XlFileFormat.xlWorkbookNormal, Type.Missing,
//                Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlExclusive,
//                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
//            excelWorkbook.Close(true, Type.Missing, Type.Missing);
//            excelWorkbook = null;

//            // Release the Application object
//            excelApp.Quit();
//            excelApp = null;

//            // Collect the unreferenced objects
//            GC.Collect();
//            GC.WaitForPendingFinalizers();

//        }


//        public static void ExportDGVToExcel(DataGridView dataGridView1, string outputPath)
//        {
//            int sheetIndex = 0;

//            // Create the Excel Application object
//            excelApp = new Microsoft.Office.Interop.Excel.Application();

//            // Create a new Excel Workbook
//            excelWorkbook = excelApp.Workbooks.Add(Type.Missing);

//            object misValue = System.Reflection.Missing.Value;



//            Worksheet excelSheet = (Worksheet)excelWorkbook.Sheets.Add(
//                excelWorkbook.Sheets.get_Item(++sheetIndex),
//                Type.Missing, 1, XlSheetType.xlWorksheet);


//            int i = 0;

//            int j = 0;



//            for (i = 0; i <= dataGridView1.RowCount - 1; i++)
//            {

//                for (j = 0; j <= dataGridView1.ColumnCount - 1; j++)
//                {

//                    DataGridViewCell cell = dataGridView1[j, i];

//                    excelSheet.Cells[i + 1, j + 1].NumberFormat = "@";
//                    excelSheet.Cells[i + 1, j + 1] = string.Format(cell.Value.ToString());

//                }

//            }




//            // Save and Close the Workbook
//            excelWorkbook.SaveAs(outputPath, XlFileFormat.xlWorkbookNormal, Type.Missing,
//                Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlExclusive,
//                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
//            excelWorkbook.Close(true, Type.Missing, Type.Missing);
//            excelWorkbook = null;

//            // Release the Application object
//            excelApp.Quit();
//            excelApp = null;

//            // Collect the unreferenced objects
//            GC.Collect();
//            GC.WaitForPendingFinalizers();



//            MessageBox.Show("Excel file created , you can find the file c:\\csharp.net-informations.xls");

//        }



//        private void releaseObject(object obj)
//        {

//            try
//            {

//                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);

//                obj = null;

//            }

//            catch (Exception ex)
//            {

//                obj = null;

//                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());

//            }

//            finally
//            {

//                GC.Collect();

//            }

//        }
//    }
}
