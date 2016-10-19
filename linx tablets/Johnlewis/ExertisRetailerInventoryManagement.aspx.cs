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
using System.Data.SqlClient;
using PortalCommon;
namespace linx_tablets.Johnlewis
{
    public partial class ConsignmetnUploads : System.Web.UI.Page
    {
        public int customerID = 6;
        protected void Page_Load(object sender, EventArgs e)
        {


        }
        
        protected void btnDownloadExistingConsignmentStock_Click(object sender, EventArgs e)
        {
            string filePath = @"C:\linx-tablets\replen files\";
            string filename = "Stock_Existing_" + Common.timestamp() + ".xls";
            PortalCommon.Excel.GenerateExcelSheetNew(Common.runSQLDataset("exec sp_portal_generic_retailerconsignmentstock_existing 6"), "Inventory", filePath + filename);
            FileInfo file = new FileInfo(filePath + filename);
            DownloadFile(file);



        }
        protected void btnDownloadTemplateConsignmentStock_Click(object sender, EventArgs e)
        {
            string filePath = @"C:\linx-tablets\replen files\";
            string filename = "Stock_Template_" + Common.timestamp() + ".xls";
            PortalCommon.Excel.GenerateExcelSheetNew(Common.runSQLDataset("exec sp_portalretailerconsignmentstock_template"), "Inventory", filePath + filename);
            FileInfo file = new FileInfo(filePath + filename);
            DownloadFile(file);
        }
        private void runReport(string query, string filename)
        {
            this.Session["ReportQuery"] = (object)query;
            this.Session["ReportQueryIsSp"] = (object)false;
            this.Session["ReportDelimiter"] = (object)",";
            this.Session["ReportHasHeader"] = (object)true;
            this.Session["ReportFileName"] = (object)filename;
            this.Session["ReportTextQualifier"] = (object)"\"";
            this.Response.Redirect("~/reporting/report-export-csv.aspx");
        }

        protected void btnUploadConsignmentStock_Click(object sender, EventArgs e)
        {
            string temploadTable = "product_dataloader_portal_jlp_retailerconsignmentstockfigures";
            string ext = Path.GetExtension(fuConsignmentStock.FileName);
            if (fuConsignmentStock.HasFile && Path.GetExtension(fuConsignmentStock.FileName).ToLower() == ".xls")
            {
                Common.runSQLNonQuery("delete from " + temploadTable);
                string filename = Path.GetFileNameWithoutExtension(fuConsignmentStock.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuConsignmentStock.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuConsignmentStock.SaveAs(filePathLocale + filename);
                        filename = Path.GetFileName(PortalCommon.Excel.ConvertExcelToCsv(filePathLocale + filename));

                    }
                    catch (Exception ex)
                    {
                        Common.log(ex.Message);
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('File save failure " + ex.Message.ToString() + "');", true);
                    }
                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalUploadedFiles/", "exertissdg", "Exertissdg1");
                    try
                    {
                        ftpClient.uploadFile(filePathLocale + filename);
                    }
                    catch (Exception ex)
                    {
                    }
                    string newFilename = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\exertissdg\portalUploadedFiles\" + filename;
                    string bulkInsert = string.Format(@"BULK INSERT " + temploadTable + @" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + temploadTable).ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from " + temploadTable +" where (customersku is null or RetailerID is null or StockQty is null)").ToString()) > 0)
                        throw new Exception("Ensure CustomerSKU/RetailerID/Qty is populated for every line");
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + temploadTable + " where retailerid not in (select retailerid from MSE_PortalConsignmentRetailers where customerid="+customerID+")").ToString()) > 0)
                        throw new Exception("Unkown RetailerID found in file");

                    
                    
                    string updateSQL = string.Format("exec sp_portalretailerconsignmentstock '{0}','{1}',{2}", fuConsignmentStock.FileName, HttpContext.Current.User.Identity.Name.ToString(),customerID);
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, retailer consignment stock levels have been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report.');", true);
                }
            }
        }
        protected void gvLastImportedForecastPortal_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                System.Web.UI.WebControls.Image theImage = (System.Web.UI.WebControls.Image)e.Row.FindControl("imgImportStatus");
                if (int.Parse(DataBinder.Eval(e.Row.DataItem, "dateDiffImport").ToString()) > int.Parse(DataBinder.Eval(e.Row.DataItem, "warningdiff").ToString()))
                {
                    theImage.ImageUrl = "~/images/x.png";
                }
                else
                {
                    theImage.ImageUrl = "~/images/tick.png";
                }
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
    }
}