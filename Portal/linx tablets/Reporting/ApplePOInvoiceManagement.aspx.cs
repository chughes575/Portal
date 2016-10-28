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
    public partial class VendorPOInvoiceManagement : System.Web.UI.Page
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
        
        
            protected void gvInvoiceFiles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string str1 = e.CommandName.ToString();
            if (str1 == "downloadInvoiceLines")
            {
                string filename = string.Format("Apple_Po_Invocie_Report_{0}.csv", (object)Common.timestamp());
                this.runReport(string.Format("exec [sp_poinvoicereportdownloadID]  {0} ", e.CommandArgument.ToString()), filename);
            }
            else if (str1 == "resubmitInvoiceToOracle")
            {
                try
                {
                    Common.runSQLNonQuery(string.Format("update mse_applepos set processed=0,increment=coalesce(increment,0)+1 where poid={0}", (object)e.CommandArgument.ToString()));
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Invoice exported to oracle, POID: " + e.CommandArgument.ToString() + "');", true);

                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Replen export failed, please try again');", true);
                }
                sqlDSPOInvoiceFiles.DataBind();
                gvInvoiceFiles.DataBind();
            }
            else if (str1 == "downloadInvoiceLinesMissing")
            {
                string filenameMissing = string.Format("Apple_Po_Invocie_Report_Missing_Price_{0}.csv", (object)Common.timestamp());
                this.runReport(string.Format("exec [sp_poinvoicereportdownloadIDMissing]  {0} ", e.CommandArgument.ToString()), filenameMissing);
            }
        }
        protected void gvInvoiceFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            Button button1 = (Button)e.Row.FindControl("btnResubmitToOracle");
            if (DataBinder.Eval(e.Row.DataItem, "Processed").ToString().ToLower().Equals("true"))
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }
        protected void gvPOInvoice_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                string date = DataBinder.Eval(e.Row.DataItem, "lastfiledate").ToString();
                DateTime dt = Convert.ToDateTime(date);
                TimeSpan ts = (DateTime.Now - dt);


                System.Web.UI.WebControls.Image theImage = (System.Web.UI.WebControls.Image)e.Row.FindControl("imgImportStatus");
                if (ts.TotalHours > 72)
                {
                    theImage.ImageUrl = "~/images/x.png";
                }
                else
                {
                    theImage.ImageUrl = "~/images/tick.png";
                }
            }
        }

        protected void btnDownloadPOInvoiceTemplate_Click(object sender, EventArgs e)
        {
            string filename = "PoInvoiceTemplate.csv";
            runReport("select * from prouctdataloader_applepoinbound_tempload_upload where material_doc='Template'", filename);
        }
        protected void btnUploadPOInvoice_Click(object sender, EventArgs e)
        {
            if (POInvoice.HasFile)
            {
                Common.runSQLNonQuery("delete from prouctdataloader_applepoinbound_tempload_upload");
                string filename = Path.GetFileNameWithoutExtension(POInvoice.FileName) + "_" + Common.timestamp() + Path.GetExtension(POInvoice.FileName);
                string filePath = @"\\10.16.72.129\company\";
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        POInvoice.SaveAs(filePathLocale + filename);

                    }
                    catch
                    {
                        throw new Exception();
                    }

                    //
                    string reportData = File.ReadAllText(filePathLocale + filename, Encoding.Default);
                    bool bypass = false;
                    var parts = reportData.Split('"');

                    for (var i = 1; i < parts.Length; i += 2)
                    {
                        parts[i] = parts[i].Replace(",", "");
                    }

                    reportData = string.Join("\"", parts);
                    string amendedFileName = "Uploaded_" + filename;

                    File.AppendAllText(filePathLocale + amendedFileName, reportData, Encoding.Default);
                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/apple files/", "extranet", "Extranet1");
                    if (bypass)
                    {
                        ftpClient = new FTP("ftp.msent.co.uk", "/in/Apple VMI Report/", "apple", "Apple1");
                    }
                    try
                    {
                        ftpClient.uploadFile(filePathLocale + amendedFileName);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception();
                    }
                    if (!bypass)
                    {
                        string newFilename = "\\\\10.16.72.129\\company\\ftp\\root\\msesrvdom\\extranet\\apple files\\" + amendedFileName;
                        string bulkInsert = string.Format(@"BULK INSERT prouctdataloader_applepoinbound_tempload_upload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                        Common.runSQLNonQuery(bulkInsert);
                        if (int.Parse(Common.runSQLScalar("select count(*) from prouctdataloader_applepoinbound_tempload_upload").ToString()) == 0)
                            throw new Exception();

                        if (int.Parse(Common.runSQLScalar("SELECT COUNT(*) FROM mse_applepos where Purchasing_Doc in (select Purchasing_Doc from prouctdataloader_applepoinbound_tempload_upload)").ToString()) > 0)
                            throw new Exception("Duplicate purchase order numbers found in file");

                        string updateSQL = string.Format("exec sp_applepoimport_upload '{0}','{1}'", amendedFileName, HttpContext.Current.User.Identity.Name.ToString());
                        Common.runSQLNonQuery(updateSQL);
                    }
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the PO invoice report has been imported');", true);
                    sqlDSPOInvoiceFilenames.DataBind();
                    sqlDSorscleLastRunApplePOInvoice.DataBind();
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", string.Format("alert('Upload FAILED, please check the format of the file error: {0}');", errorMessage), true);
                }
            }
        }
        protected void btnDownloadPOInvoice_ClickNew(object sender, EventArgs e)
        {
            int num = 0;
            num = ddlApplePOInvoice.SelectedIndex;
            if (num == 0)
                return;

            List<string> list = new List<string>();
            string filename = string.Format("Apple_Po_Invocie_Report_{0}.csv", (object)Common.timestamp());
            this.runReport(string.Format("exec [sp_poinvoicereportdownload]  '{0}' ", ddlApplePOInvoice.SelectedValue), filename);
        }


    }
}