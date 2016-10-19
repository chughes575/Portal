using MSE_Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Configuration;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using System.Data.OleDb;
namespace PortalCommon
{
    public class Excel
    {
        HSSFWorkbook hssfWorkbook;
        ICellStyle dateStyle;

        public Excel()
        {

        }

        #region File Operations
        public static void convertExcelToCSV(string sourceFile, string worksheetName, string targetFile)
        {
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + sourceFile + ";Extended Properties=\"Excel 12.0;\"";
            OleDbConnection conn = null;
            StreamWriter wrtr = null;
            OleDbCommand cmd = null;
            OleDbDataAdapter da = null;
            try
            {
                conn = new OleDbConnection(strConn);
                conn.Open();

                cmd = new OleDbCommand("SELECT * FROM [" + worksheetName + "$]", conn);
                cmd.CommandType = CommandType.Text;
                wrtr = new StreamWriter(targetFile);

                da = new OleDbDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    string rowString = "";
                    for (int y = 0; y < dt.Columns.Count; y++)
                    {
                        rowString += "\"" + dt.Rows[x][y].ToString() + "\",";
                    }
                    wrtr.WriteLine(rowString);
                }
                Console.WriteLine();
                Console.WriteLine("Done! Your " + sourceFile + " has been converted into " + targetFile + ".");
                Console.WriteLine();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                Console.ReadLine();
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Dispose();
                cmd.Dispose();
                da.Dispose();
                wrtr.Close();
                wrtr.Dispose();
            }
        }
        public static string ConvertExcelToCsv(string excelFilePath)
        {

            string filePath = "";
            string csvOutputFile = excelFilePath.Replace(".xls","_"+Common.timestamp()+".csv");
            if (!File.Exists(excelFilePath)) throw new FileNotFoundException(excelFilePath);
            if (File.Exists(csvOutputFile)) throw new ArgumentException("File exists: " + csvOutputFile);

            // connection string
            var cnnStr = String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;IMEX=1;HDR=NO\"", excelFilePath);
            var cnn = new OleDbConnection(cnnStr);

            // get schema, then data
            var dt = new DataTable();
            try
            {
                cnn.Open();
                var schemaTable = cnn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if (schemaTable.Rows.Count < 1) throw new ArgumentException("The worksheet number provided cannot be found in the spreadsheet");
                string worksheet = schemaTable.Rows[0]["table_name"].ToString().Replace("'", "");
                string sql = String.Format("select * from [{0}]", worksheet);
                var da = new OleDbDataAdapter(sql, cnn);
                da.Fill(dt);
            }
            catch (Exception e)
            {
                // ???
                throw e;
            }
            finally
            {
                // free resources
                cnn.Close();
            }

            // write out CSV data
            using (var wtr = new StreamWriter(csvOutputFile))
            {
                foreach (DataRow row in dt.Rows)
                {
                    bool firstLine = true;
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (!firstLine) { wtr.Write(","); } else { firstLine = false; }
                        var data = row[col.ColumnName].ToString().Replace("\"", "\"\"");
                        wtr.Write(String.Format("{0}", data));
                    }
                    wtr.WriteLine();
                }
                filePath = csvOutputFile;
            }
            return filePath;
        }
        public void LoadFile(string filename)
        {
            try
            {
                //read the template via FileStream, it is suggested to use FileAccess.Read to prevent file lock.
                //book1.xls is an Excel-2007-generated file, so some new unknown BIFF records are added. 
                FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);

                hssfWorkbook = new HSSFWorkbook(file);
            }
            catch (Exception ex)
            {
                string err = "ERROR: Failed to load Excel file " + Path.GetFileName(filename) + ". Error was: " + ex.Message;

                throw new Exception(err, ex);
            }
        }
        public void SaveAs()
        {
            //Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            //Microsoft.Office.Interop.Excel.Workbook wbWorkbook = wbWorkbook.


            //Microsoft.Office.Interop.Excel.Sheets wsSheet = wbWorkbook.Worksheets;
            //Microsoft.Office.Interop.Excel.Worksheet CurSheet = (Microsoft.Office.Interop.Excel.Worksheet)wsSheet[1];

            //Microsoft.Office.Interop.Excel.Range thisCell = (Microsoft.Office.Interop.Excel.Range)CurSheet.Cells[1, 1];

            //thisCell.Value2 = "This is a test.";

            //wbWorkbook.SaveAs(@"c:\test\one.xls", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlShared, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            //wbWorkbook.SaveAs(@"c:\test\two.csv", Microsoft.Office.Interop.Excel.XlFileFormat.xlCSVWindows, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlShared, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            //wbWorkbook.Close(false, "", true);
        }
        public void SaveFile(string outputFileName)
        {
            try
            {
                //Write the stream data of workbook to the root directory
                FileStream file = new FileStream(outputFileName, FileMode.Create);
                hssfWorkbook.Write(file);
                file.Close();
            }
            catch (Exception ex)
            {
                string err = "ERROR: Failed to save Excel file " + Path.GetFileName(outputFileName) + ". Error was: " + ex.Message;

                throw new Exception(err, ex);
            }
        }
        #endregion File Operations

        #region Sheet Operations
        /// <summary>
        /// Sets a cell value in a worksheet
        /// </summary>
        /// <param name="sheetName">The name of the worksheet (it must already exist)</param>
        /// <param name="row">The row to update (zero-based int)</param>
        /// <param name="column">The column to update (zero-based int, or string with Excel column name)</param>
        /// <param name="value">The value to set - can be string, bool, DateTime, double or RichTextString</param>
        /// <param name="recalcFormulas">Set to true to recalculate any formulas in the sheet after applying the update</param>
        public void SetCellValue(string sheetName, int row, object column, object value, bool recalcFormulas)
        {
            //no data to store, so leave the cell alone
            if (value.ToString() == "")
                return;

            //convert the column name to a number if needs be
            int columnNumber;
            if (column.GetType() == typeof(string))
                columnNumber = GetColumnNumberForColumnHeader(column.ToString());
            else if (column.GetType() == typeof(int))
                columnNumber = (int)column;
            else
                throw new TypeLoadException("Invalid type passed for column - should be int or string");


            //check there is a workbook open
            if (hssfWorkbook == null)
                throw new Exception("No workbook loaded!");

            //set the sheet
            ISheet sheet = null;
            try
            {
                sheet = hssfWorkbook.GetSheet(sheetName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading specified worksheet. Check worksheet exists. Error was:" + ex.Message);
            }
            if (sheet == null)
                throw new Exception("Specified worksheet does not exist in the file.");

            //check the row exists, and create if not
            IRow tRow = sheet.GetRow(row);
            if (tRow == null)
            {
                sheet.CreateRow(row);
                tRow = sheet.GetRow(row);
            }

            //check the cell exists, and create it if it doesn't
            ICell cell = sheet.GetRow(row).GetCell(columnNumber);
            if (cell == null)
            {
                CreateCell(sheetName, row, columnNumber);
                cell = sheet.GetRow(row).GetCell(columnNumber);
            }


            //update the cell
            if (value.GetType() == typeof(string))
                cell.SetCellValue(value.ToString());
            else if (value.GetType() == typeof(bool))
                cell.SetCellValue((bool)value);
            else if (value.GetType() == typeof(Double))
                cell.SetCellValue((Double)value);
            else if (value.GetType() == typeof(DateTime))
                cell.SetCellValue((DateTime)value);
            else if (value.GetType() == typeof(HSSFRichTextString))
                cell.SetCellValue((HSSFRichTextString)value);
            else if (value.GetType() == typeof(int))
                cell.SetCellValue(System.Convert.ToDouble(value));
            else if (value.GetType() == typeof(decimal))
                cell.SetCellValue(System.Convert.ToDouble(value));
            else if (value.GetType() == typeof(float))
                cell.SetCellValue(System.Convert.ToDouble(value));
            else if (value.GetType() == typeof(byte))
                cell.SetCellValue(System.Convert.ToDouble(value));
            else
                cell.SetCellValue(value.ToString());

            if (recalcFormulas)
                sheet.ForceFormulaRecalculation = true;
            else
                sheet.ForceFormulaRecalculation = false;
        }

        private int GetColumnNumberForColumnHeader(string name)
        {
            // this code shamelessly 'borrowed' from StackOverflow
            //   http://stackoverflow.com/questions/848147/how-to-convert-excel-sheet-column-names-into-numbers
            int number = 0;
            int pow = 1;
            for (int i = name.Length - 1; i >= 0; i--)
            {
                number += (name[i] - 'A' + 1) * pow;
                pow *= 26;
            }

            return number;
        }

        public void CreateWorkBook()
        {
            hssfWorkbook = new HSSFWorkbook();

            dateStyle = hssfWorkbook.CreateCellStyle();
            IDataFormat format = hssfWorkbook.CreateDataFormat();
            dateStyle.DataFormat = format.GetFormat("dd/mm/yyyy hh:mm");
        }

        public void CreateWorksheet(string worksheetName)
        {
            hssfWorkbook.CreateSheet(worksheetName);
        }

        public void CreateRow(string sheetName, int rowNumber)
        {
            ISheet theSheet = hssfWorkbook.GetSheet(sheetName);
            theSheet.CreateRow(rowNumber);
        }

        public void CreateCell(string sheetName, int rowNumber, int columnNumber, Type cellType, string foreColour, string backColour)
        {
            ISheet theSheet = hssfWorkbook.GetSheet(sheetName);
            IRow theRow = theSheet.GetRow(rowNumber);

            ICell theCell;
            if (cellType == typeof(int) || cellType == typeof(double) || cellType == typeof(decimal))
                theCell = theRow.CreateCell(columnNumber, CellType.Numeric);
            else
                theCell = theRow.CreateCell(columnNumber, CellType.String);

            if (cellType == typeof(DateTime))
            {

                theCell.CellStyle = dateStyle;
            }




            //if
            if (backColour != null && foreColour != null)
            {
                ICellStyle style = hssfWorkbook.CreateCellStyle();

                switch (backColour.ToLower())
                {
                    case "black":


                        style.FillForegroundColor = HSSFColor.Black.Index;
                        style.FillForegroundColor = HSSFColor.Black.Index;
                        break;
                    case "red":
                        style.FillBackgroundColor = HSSFColor.Red.Index;
                        style.FillForegroundColor = HSSFColor.Red.Index;
                        break;
                    case "yellow":
                        style.FillBackgroundColor = HSSFColor.Yellow.Index;
                        style.FillForegroundColor = HSSFColor.Yellow.Index;
                        break;
                    case "green":
                        style.FillBackgroundColor = HSSFColor.Green.Index;
                        style.FillForegroundColor = HSSFColor.Green.Index;
                        break;
                    case "blue":
                        style.FillBackgroundColor = HSSFColor.Blue.Index;
                        style.FillForegroundColor = HSSFColor.Blue.Index;

                        HSSFWorkbook hwb = new HSSFWorkbook();
                        HSSFPalette palette = hwb.GetCustomPalette();
                        HSSFColor myColor = palette.FindSimilarColor(86, 112, 1);
                        short palIndex = myColor.Indexed;
                        style.FillForegroundColor = palIndex;
                        style.FillBackgroundColor = palIndex;
                         
 //                       HSSFColor grey =new XSSFColor(new java.awt.Color(192,192,192));
 //cellStyle.setFillForegroundColor(grey);
                        break;
                    case "white":
                    default:
                        style.FillBackgroundColor = HSSFColor.White.Index;
                        style.FillForegroundColor = HSSFColor.White.Index;
                        break;
                }

                IFont font = hssfWorkbook.CreateFont();
                switch (foreColour.ToLower())
                {
                    case "black":
                        font.Color = HSSFColor.Black.Index;
                        break;
                    case "red":
                        font.Color = HSSFColor.Red.Index;
                        break;
                    case "yellow":
                        font.Color = HSSFColor.Yellow.Index;
                        break;
                    case "green":
                        font.Color = HSSFColor.Green.Index;
                        break;
                    case "blue":
                        font.Color = HSSFColor.Blue.Index;
                        break;
                    case "white":
                    default:
                        font.Color = HSSFColor.White.Index;
                        break;
                }
                style.SetFont(font);
                style.FillPattern = FillPattern.SolidForeground;
                theCell.CellStyle = style;
            }
        }

        public void CreateCell(string sheetName, int rowNumber, int columnNumber, string foreColour, string backColour)
        {
            CreateCell(sheetName, rowNumber, columnNumber, typeof(string), foreColour, backColour);
        }

        public void CreateCell(string sheetName, int rowNumber, int columnNumber)
        {
            CreateCell(sheetName, rowNumber, columnNumber, typeof(string), null, null);
        }

        #endregion Sheet Operations

        /// <summary>
        /// Populates a worksheet with data from a DataTable
        /// </summary>
        /// <param name="worksheet">The name of the worksheet to populate</param>
        /// <param name="table">The data table containing the source data</param>
        /// <param name="recalcFormulas">If true, formulas are recalculated once the data is inserted</param>
        /// <param name="replaceNullsWithBlanks">If true, NULL values are replaced by the empty string. If false, the value of the cell is left intact</param>
        /// <param name="rowOffset">The number of rows from the top of the worksheet to start from</param>
        /// <param name="columnOffset">The number of columns from the left of the worksheet to start from</param>
        public void ConvertDataTableToExcelSheet(string worksheet, DataTable table, bool recalcFormulas, bool replaceNullsWithBlanks, int rowOffset, int columnOffset)
        {
            //allow for an offset from cell A1
            int rowCounter = rowOffset;
            int columnCounter = columnOffset;

            //loop through each row in the table
            foreach (DataRow row in table.Rows)
            {
                //loop through each column in the row
                foreach (DataColumn col in table.Columns)
                {
                    SetCellValue(worksheet, rowCounter, columnCounter, (row[col] == null && replaceNullsWithBlanks) ? "" : row[col], recalcFormulas);
                    columnCounter++;
                }

                rowCounter++;
            }
        }

        public static bool GenerateExcelSheet(string templateFile, DataTable dataTable, DataTable schemaTable, string worksheetName, ExcelHeader headObject, string outFileName)
        {
            Excel excel = new Excel();
            if (templateFile != "")
            {
                excel.LoadFile(templateFile);
            }
            else
            {
                excel.CreateWorkBook();
                excel.CreateWorksheet(worksheetName);
            }

            bool wereRecordsReturned = false;
            int recCount = 0;

            if (headObject.hasHeader)
            {
                recCount++;

                if (templateFile == "")
                {
                    excel.CreateRow(worksheetName, 0);
                }

                //write out the fields
                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    if (headObject.fontColor == null || headObject.backColor == null)
                    {
                        excel.CreateCell(worksheetName, recCount - 1, i, "white", "black");
                    }
                    else
                    {
                        excel.CreateCell(worksheetName, recCount - 1, i, headObject.fontColor, headObject.backColor);
                    }
                    excel.SetCellValue(worksheetName, recCount - 1, i, schemaTable.Rows[i]["ColumnName"].ToString(), true);
                }
            }

            DataTableReader reader = dataTable.CreateDataReader();
            while (reader.Read())
            {
                recCount++;

                if (!wereRecordsReturned) wereRecordsReturned = true;

                //create the row in the file if we need to
                if (templateFile == "")
                {
                    excel.CreateRow(worksheetName, recCount - 1);
                }

                //write out the fields
                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    if (templateFile == "")
                    {
                        if (
                            schemaTable.Rows[i]["DataType"].ToString() == "System.Int32" ||
                            schemaTable.Rows[i]["DataType"].ToString() == "System.Double" ||
                            schemaTable.Rows[i]["DataType"].ToString() == "System.Decimal" ||
                            schemaTable.Rows[i]["DataType"].ToString() == "System.Float"
                        )
                            excel.CreateCell(worksheetName, recCount - 1, i, typeof(int), null, null);
                        else if (schemaTable.Rows[i]["DataType"].ToString() == "System.DateTime")
                            excel.CreateCell(worksheetName, recCount - 1, i, typeof(DateTime), null, null);
                        else
                            excel.CreateCell(worksheetName, recCount - 1, i);
                    }

                    excel.SetCellValue(worksheetName, recCount - 1, i, reader[schemaTable.Rows[i]["ColumnName"].ToString()], true);


                }
            }

            excel.SaveFile(outFileName);

            //flush from memory
            excel = null;

            return wereRecordsReturned;
        }
        public static bool GenerateExcelSheetNew(DataSet ds, string worksheetName, string outFileName)
        {
            Excel excel = new Excel();
            DataTableReader readerIn = ds.CreateDataReader();
            DataTable schemaTable = readerIn.GetSchemaTable();
            DataTable dataTable = ds.Tables[0];


            excel.CreateWorkBook();
            excel.CreateWorksheet(worksheetName);

            bool wereRecordsReturned = false;
            int recCount = 0;


            recCount++;

            excel.CreateRow(worksheetName, 0);

            //write out the fields
            for (int i = 0; i < schemaTable.Rows.Count; i++)
            {
                excel.CreateCell(worksheetName, recCount - 1, i, "white", "blue");
                
                excel.SetCellValue(worksheetName, recCount - 1, i, schemaTable.Rows[i]["ColumnName"].ToString(), true);
            }



            DataTableReader reader = dataTable.CreateDataReader();
            while (reader.Read())
            {
                recCount++;

                if (!wereRecordsReturned) wereRecordsReturned = true;

                //create the row in the file if we need to
                excel.CreateRow(worksheetName, recCount - 1);

                //write out the fields
                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {

                    string dtype = schemaTable.Rows[i]["DataType"].ToString();
                    string dval = reader[schemaTable.Rows[i]["ColumnName"].ToString()].ToString();
                    if (dval == "")
                    {
                        dval = " ";
                    }
                    object ob = dval;
                    Common.log(dval);
                    if (
                        schemaTable.Rows[i]["DataType"].ToString() == "System.Int32" ||
                        schemaTable.Rows[i]["DataType"].ToString() == "System.Double" ||
                        schemaTable.Rows[i]["DataType"].ToString() == "System.Decimal" ||
                        schemaTable.Rows[i]["DataType"].ToString() == "System.Float"
                    )
                        excel.CreateCell(worksheetName, recCount - 1, i, typeof(int), null, null);

                    else if (schemaTable.Rows[i]["DataType"].ToString() == "System.DateTime")
                        excel.CreateCell(worksheetName, recCount - 1, i, typeof(DateTime), null, null);
                    else
                        excel.CreateCell(worksheetName, recCount - 1, i);


                    excel.SetCellValue(worksheetName, recCount - 1, i, ob, true);


                }
            }



            excel.SaveFile(outFileName);

            //flush from memory


            return wereRecordsReturned;
        }

    }

    public class ExcelHeader
    {
        public bool hasHeader;
        public string fontColor;
        public string backColor;
    }
}
