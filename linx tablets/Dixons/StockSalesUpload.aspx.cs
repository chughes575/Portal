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

namespace linx_tablets.Dixons
{
    public partial class StockSalesUpload : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void gvStockSalesUploads_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string commandName = e.CommandName.ToString();
            if (commandName == "downloadfile")
            {
                runReport(string.Format(" select * from mse_portal_dsg_stocksales where id={0}", (object)e.CommandArgument.ToString()), string.Format("DSG_SalesStock_ID_{0}_{1}.csv", (object)e.CommandArgument.ToString(), (object)Common.timestamp()));
            }
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
        protected void btnUploadStockSales_Click(object sender, EventArgs e)
        {
            string temploadTable = "productdataloader_portal_dsg_stocksales";
            if (fupStockSales.HasFile)
            {
                Common.runSQLNonQuery("DELETE FROM " + temploadTable);

                string filename = Path.GetFileNameWithoutExtension(fupStockSales.FileName) + "_staging_" + Common.timestamp() + Path.GetExtension(fupStockSales.FileName);
                string amendedFilename = Path.GetFileNameWithoutExtension(fupStockSales.FileName) + "_" + Common.timestamp() + Path.GetExtension(fupStockSales.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fupStockSales.SaveAs(filePathLocale + filename);
                    }
                    catch
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('File save failure');", true);
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


                    string updateSQL = string.Format("exec [sp_portalstocksalesimport_postload_dsg] '{0}','{1}'", fupStockSales.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    sqldsuploads.DataBind();
                    gvStockSalesUploads.DataBind();
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the DSG stock/sales file has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report.');", true);
                }
            }
        }
    }
}