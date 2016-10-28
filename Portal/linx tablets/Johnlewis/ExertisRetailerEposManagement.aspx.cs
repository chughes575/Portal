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
namespace linx_tablets.Johnlewis
{
    public partial class EposSales : System.Web.UI.Page
    {
        public int customerID = 6;
        protected void Page_Load(object sender, EventArgs e)
        {

        }
       
        protected void gvConsignmentretailers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Download")
            {
                string filename = "Epos_Data_Existing_Retailer_" + e.CommandArgument.ToString() + "_" + Common.timestamp() + ".csv";
                runReport("exec sp_portaleposdata_existing @customerid=" + customerID + ",@retailerid=" + e.CommandArgument.ToString(), filename);
            }
            else if (e.CommandName == "DownloadOs")
            {
                GridViewRow row = (GridViewRow)(((Button)e.CommandSource).NamingContainer);
                // row contains current Clicked Gridview Row



                DropDownList ddl = row.FindControl("ddlFileNames") as DropDownList;
                string filename = ddl.SelectedValue.ToString();
                string locale = Common.runSQLScalar(@"select dbo.fn_PortalGetFilSubPath('" + filename + "')").ToString();
                string filePath = @"C:\Linx-tablets\saleseposfiles\" + locale + @"\" + filename;
                FileInfo file = new FileInfo(filePath);
                DownloadFile(file);
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
        
        protected void btnDownloadExistingEposdata_Click(object sender, EventArgs e)
        {
            string filename = "Epos_Data_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_portaleposdata_generic_existing @customerid=" + customerID, filename);
        }
        protected void btnDownloadTemplateEposData_Click(object sender, EventArgs e)
        {
            string filename = "Epos_Data_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_portaleposdata_generic_template @customerid=" + customerID, filename);
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
            string tempTableName = "product_dataloader_portal_jlp_eposfigures";
            if (fuConsignmentStock.HasFile)
            {
                Common.runSQLNonQuery("delete from " + tempTableName);
                string filename = Path.GetFileNameWithoutExtension(fuConsignmentStock.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuConsignmentStock.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuConsignmentStock.SaveAs(filePathLocale + filename);

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
                    string bulkInsert = string.Format(@"BULK INSERT "+tempTableName + @" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName).ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from "+tempTableName+" where retailerid not in (select retailerid from MSE_PortalConsignmentRetailers where customerid="+customerID+")").ToString()) > 0)
                        throw new Exception("Unkown RetailerID found in file");


                    if (int.Parse(Common.runSQLScalar("select coalesce((select count(*) from "+tempTableName+"  group by customersku,cast(eposdate as date) having count(stockqty)>1),0)").ToString()) > 0)
                        throw new Exception("Unkown RetailerID found in file");
              
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName + " where retailerid in (4, 5, 6, 7, 8, 9)").ToString()) > 0)
                        throw new Exception("Email suppliers found in files. These are populated via email pickup onl please remove");


                    string updateSQL = string.Format("exec sp_portalretailer_generic_eposupload {0},'{1}','{2}'", customerID, fuConsignmentStock.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, epos stock/sales have been updated');", true);
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
    }
}