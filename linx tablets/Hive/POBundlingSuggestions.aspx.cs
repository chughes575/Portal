

using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;

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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;



namespace linx_tablets.Hive
{
    public partial class WorkorderManagement : System.Web.UI.Page
    {
        public class Excel
        {
            HSSFWorkbook hssfWorkbook;
            ICellStyle dateStyle;

            public Excel()
            {

            }

            #region File Operations
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
            
            public static bool GenerateExcelSheetMultipleSheet(string templateFile, DataTable dataTable, DataTable dataTable1, DataTable schemaTable, DataTable schemaTable1, string worksheetName, string worksheetName1, ExcelHeader headObject, string outFileName)
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
                    for (int i = 0; i < schemaTable1.Rows.Count; i++)
                    {
                        if (headObject.fontColor == null || headObject.backColor == null)
                        {
                            excel.CreateCell(worksheetName1, recCount - 1, i, "white", "black");
                        }
                        else
                        {
                            excel.CreateCell(worksheetName1, recCount - 1, i, headObject.fontColor, headObject.backColor);
                        }
                        excel.SetCellValue(worksheetName1, recCount - 1, i, schemaTable1.Rows[i]["ColumnName"].ToString(), true);
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
                recCount = 1;
                DataTableReader reader1 = dataTable1.CreateDataReader();
                while (reader1.Read())
                {
                    recCount++;

                    if (!wereRecordsReturned) wereRecordsReturned = true;

                    //create the row in the file if we need to
                    if (templateFile == "")
                    {
                        excel.CreateRow(worksheetName1, recCount - 1);
                    }

                    //write out the fields
                    for (int i = 0; i < schemaTable1.Rows.Count; i++)
                    {
                        if (templateFile == "")
                        {
                            if (
                                schemaTable1.Rows[i]["DataType"].ToString() == "System.Int32" ||
                                schemaTable1.Rows[i]["DataType"].ToString() == "System.Double" ||
                                schemaTable1.Rows[i]["DataType"].ToString() == "System.Decimal" ||
                                schemaTable1.Rows[i]["DataType"].ToString() == "System.Float"
                            )
                                excel.CreateCell(worksheetName1, recCount - 1, i, typeof(int), null, null);
                            else if (schemaTable.Rows[i]["DataType"].ToString() == "System.DateTime")
                                excel.CreateCell(worksheetName1, recCount - 1, i, typeof(DateTime), null, null);
                            else
                                excel.CreateCell(worksheetName1, recCount - 1, i);
                        }

                        excel.SetCellValue(worksheetName1, recCount - 1, i, reader1[schemaTable1.Rows[i]["ColumnName"].ToString()], true);


                    }
                }

                excel.SaveFile(outFileName);

                //flush from memory
                excel = null;

                return wereRecordsReturned;
            }
        }
        public class ExcelHeader
        {
            public bool hasHeader;
            public string fontColor;
            public string backColor;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            
           
            int weekNo = int.Parse(Common.runSQLScalar("select datepart(week,getdate())").ToString());

            for (int i = 0; i < 15; i++)
            {
                
            }
            
            
            if (!Page.IsPostBack)
            {
                string leadTimeComponentExertisHive = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsedBundles3pl' and CustomerID=5").ToString();
            lblWeeksUsed.Text = leadTimeComponentExertisHive;


               

                string exertisHiveleadTimeComponentExertisHive = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsedBundlesExertisHive' and CustomerID=6").ToString();
            lblExertisHiveWeeksUsed.Text = exertisHiveleadTimeComponentExertisHive;



            string comp = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsed3pl' and CustomerID=5").ToString();
            Label1.Text = comp;
            }
        }
        protected void DownloadFile(FileInfo file)
        {
            Response.Clear();

            Response.ClearHeaders();

            Response.ClearContent();

            Response.AddHeader("Content-Disposition", "attachment; filename=" + file.Name);

            Response.AddHeader("Content-Length", file.Length.ToString());

            //Response.ContentType = file.

            Response.Flush();

            Response.TransmitFile(file.FullName);

            Response.End();
        }
        private void runReport(string query, string filename)
        {
            //this.Session["ReportQuery"] = (object)query;
            //this.Session["ReportQueryIsSp"] = (object)false;
            //this.Session["ReportDelimiter"] = (object)",";
            //this.Session["ReportHasHeader"] = (object)true;
            //this.Session["ReportFileName"] = (object)filename;
            //this.Session["ReportTextQualifier"] = (object)"\"";
            //this.Response.Redirect("~/reporting/report-export-csv.aspx");
            string filePathD = @"C:\linx-tablets\replen files\";

            filename = filename.Replace(".csv", ".xls");
            DataSet dsConsignmentStock = Common.runSQLDataset(query);

            PortalCommon.Excel.GenerateExcelSheetNew(dsConsignmentStock, "Download", filePathD + filename);
            
            FileInfo file = new FileInfo(filePathD + filename);
            DownloadFile(file);
        }

        protected void btnUkReplenFile_Command(object sender, CommandEventArgs e)
        {
            string str1 = e.CommandArgument.ToString().Substring(2, e.CommandArgument.ToString().Length - 2);
            string type = e.CommandArgument.ToString().Substring(0, 1);
            if (type == "c")
            {
                if (str1 == "sug")
                {
                    this.runReport("exec [sp_portalhive_pocomponentsuggestions] 1", "POSuggestions_Hive_Components_" + Common.timestamp() + ".csv");
                }
                else if (str1 == "sugall")
                {
                    this.runReport("exec [sp_portalhive_pocomponentsuggestions] 0", "POSuggestions_Hive_Components_All_" + Common.timestamp() + ".csv");
                }
                if (str1 == "sugs")
                {
                    this.runReport("exec [sp_portalhive_pocomponentsuggestions] 1,0,1", "POSuggestions_Hive_Components_Spares_" + Common.timestamp() + ".csv");
                }
                else if (str1 == "sugsall")
                {
                    this.runReport("exec [sp_portalhive_pocomponentsuggestions] 0,0,1", "POSuggestions_Hive_Components_Spares_All_" + Common.timestamp() + ".csv");
                }
            }
        }
        public void LoadFile(string filename)
        {
            try
            {
                //read the template via FileStream, it is suggested to use FileAccess.Read to prevent file lock.
                //book1.xls is an Excel-2007-generated file, so some new unknown BIFF records are added. 
                FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);

                HSSFWorkbook hssfWorkbook = new HSSFWorkbook(file);
            }
            catch (Exception ex)
            {
                string err = "ERROR: Failed to load Excel file " + Path.GetFileName(filename) + ". Error was: " + ex.Message;

                throw new Exception(err, ex);
            }
        }
        protected void btnDownloadBundleSuggestions_Click(object sender, EventArgs e)
        {
            string filename = "Hive_Bundle_WorkOrders_" + Common.timestamp() + ".xls";
            //runReport("exec [sp_portalhive_pobundleworkorders]", filename);
            DataSet dsWorkOrderFileBundles = Common.runSQLDataset("EXEC [sp_portalhive_pobundleworkorders_header]");
            DataSet dsWorkOrderFileComponents = Common.runSQLDataset("EXEC [sp_portalhive_pobundleworkorders]");

            DataTableReader readerBundles = dsWorkOrderFileBundles.CreateDataReader();
            DataTable schemaTableBundles = readerBundles.GetSchemaTable();
            DataTableReader readerComponents = dsWorkOrderFileComponents.CreateDataReader();
            DataTable schemaTableComponents = readerComponents.GetSchemaTable();

            ExcelHeader header = new ExcelHeader();
            header.hasHeader = true;
            Excel.GenerateExcelSheetMultipleSheet(
                @"C:\apple live\ExcelTemplates\WorkOrderTemplate.xls",
                dsWorkOrderFileBundles.Tables[0],
                dsWorkOrderFileComponents.Tables[0],
                schemaTableBundles,
                schemaTableComponents,
                "Required Bundles",
                "Required Components",
                header,
                @"C:\linx-tablets\replen files\" + filename
            );
            string filePath = @"C:\linx-tablets\replen files\" + filename;
            FileInfo file = new FileInfo(filePath);
            DownloadFile(file);
            

            
        }
        
    }
}