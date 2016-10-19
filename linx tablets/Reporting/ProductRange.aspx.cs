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

namespace linx_tablets.Reporting
{
    public partial class ProductRange : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

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
        protected void gvLastImportedOracle_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                System.Web.UI.WebControls.Image theImage = (System.Web.UI.WebControls.Image)e.Row.FindControl("imgImportStatus");
                if (int.Parse(DataBinder.Eval(e.Row.DataItem, "dateDiffImport").ToString()) > 24)
                {
                    theImage.ImageUrl = "~/images/x.png";
                }
                else
                {
                    theImage.ImageUrl = "~/images/tick.png";
                }
            }

        }
        protected void btnDownloadRange_Click(object sender, EventArgs e)
        {
            string filename = "ProductRange_Existing_" + Common.timestamp() + ".csv";
            runReport("select * from vw_appleproductrangeexisting", filename);
        }
        protected void btnDownloadRangeTemplate_Click(object sender, EventArgs e)
        {
            string filename = "ProductRange_Template_" + Common.timestamp() + ".csv";
            runReport("select * from vw_appleproductrangetemplate", filename);
        }
        protected void btnUploadProductRange_Click(object sender, EventArgs e)
        {
            if (fupProductRange.HasFile)
            {
                Common.runSQLNonQuery("delete from productdataloader_apple_productrange_tempload_upload");
                string filename = Path.GetFileNameWithoutExtension(fupProductRange.FileName) + "_" + Common.timestamp() + Path.GetExtension(fupProductRange.FileName);
                string filePath = @"\\10.16.72.129\company\";
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fupProductRange.SaveAs(filePathLocale + filename);

                    }
                    catch
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('File save failure');", true);
                    }

                    


                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/apple files/", "extranet", "Extranet1");
                    try
                    {
                        ftpClient.uploadFile(filePathLocale + filename);
                    }
                    catch (Exception ex)
                    {
                    }
                    string newFilename = "\\\\10.16.72.129\\company\\ftp\\root\\msesrvdom\\extranet\\apple files\\" + filename;
                    string bulkInsert = string.Format(@"BULK INSERT productdataloader_apple_productrange_tempload_upload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_apple_productrange_tempload_upload").ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_apple_productrange_tempload_upload where (innerqty is not null and outerqty is null) or (outerqty is not null and innerqty is null)").ToString()) > 0)
                        throw new Exception("Ensure both outer and inner qtys are populated or leave both blank");

                    string updateSQL = string.Format("exec sp_oracleproductrangeimport_upload '{0}','{1}'", fupProductRange.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the range has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report Exception: "+ex.Message+"');", true);
                }
            }
        }
    }
}