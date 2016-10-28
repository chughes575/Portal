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
    public partial class VMI : Page
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
        protected void gvVMI_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                string date = DataBinder.Eval(e.Row.DataItem, "DateCreated").ToString();
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
        protected void btnUploadVMI_Click(object sender, EventArgs e)
        {
            if (fupProductVMI.HasFile)
            {
                Common.runSQLNonQuery("delete from productdataoader_apple_vmi_report_temp_load_upload");
                string filename = Path.GetFileNameWithoutExtension(fupProductVMI.FileName) + "_" + Common.timestamp() + Path.GetExtension(fupProductVMI.FileName);
                string filePath = @"\\10.16.72.129\company\";
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fupProductVMI.SaveAs(filePathLocale + filename);

                    }
                    catch
                    {
                        throw new Exception();
                    }

                    //
                    string reportData = File.ReadAllText(filePathLocale + filename, Encoding.Default);
                    bool bypass = false;
                    //    if (!bypass)
                    //    {
                    //        reportData = Regex.Replace(reportData,
                    //@",(?=[^""]*""(?:[^""]*""[^""]*"")*[^""]*$)",
                    //String.Empty);
                    //    }

                    var parts = reportData.Split('"');

                    for (var i = 1; i < parts.Length; i += 2)
                    {
                        parts[i] = parts[i].Replace(",", "");
                    }

                    reportData = string.Join("\"", parts);
                    string amendedFileName = "Amended" + filename;

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
                        string bulkInsert = string.Format(@"BULK INSERT productdataoader_apple_vmi_report_temp_load_upload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                        Common.runSQLNonQuery(bulkInsert);
                        if (int.Parse(Common.runSQLScalar("select count(*) from productdataoader_apple_vmi_report_temp_load_upload").ToString()) == 0)
                            throw new Exception();

                        string updateSQL = string.Format("exec sp_applevmireport_upload '{0}','{1}'", amendedFileName, HttpContext.Current.User.Identity.Name.ToString());
                        Common.runSQLNonQuery(updateSQL);
                    }
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the VMI report has been imported');", true);
                    sqlDsVMIReportSource.DataBind();
                    ddlVMIReports.DataBind();
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file');", true);
                }
            }
        }
        protected void btnDownloadVMI_ClickNew(object sender, EventArgs e)
        {
            int num = 15;
            try
            {
                num = int.Parse(this.ddlVMIReports.SelectedValue);
            }
            catch
            {
            }
            if (num == 0)
                return;
            List<string> list = new List<string>();
            string filename = string.Format("VMI_Report_{0}.csv", (object)Common.timestamp());
            this.runReport(string.Format("exec [sp_vmireport]  {0} ", (object)num), filename);
        }
        protected void btnRunVMI_Click(object sender, EventArgs e)
        {
            this.createVmiReport(true);
        }
        private void createVmiReport(bool testing)
        {
            List<string> list1 = new List<string>();
            foreach (DataRow dataRow in (InternalDataCollectionBase)Common.runSQLRows(string.Format("select vmireportid,filename,replace(convert(varchar,cast(datecreated as date),103),'/','') from mse_applevmidatareports where case when {0} is then 1 else 0coalesce(responsesent,0)=0 order by datecreated asc", (object)Convert.ToInt32(testing))))
            {
                List<string> list2 = new List<string>();
                string str1 = string.Format("VMI_Report_{1}_{0}.csv", (object)Common.timestamp(), (object)dataRow[2].ToString());
                try
                {
                    string contents = Common.dataTableToTextFile(Common.runSQLDataset(string.Format("select * from vw_applevmireportresponse where vmireportid={1}", (object)Common.timestamp(), (object)dataRow[0].ToString())).Tables[0], ",", "\r\n", true);
                    string str2 = "\\\\10.16.72.129\\company\\applefiles\\";
                    System.IO.File.AppendAllText(str2 + str1, contents);
                    list2.Add(str2 + str1);
                }
                catch
                {
                }
                if (list2.Count > 0)
                {
                    MailMessage message = new MailMessage();
                    SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
                    message.From = new MailAddress("chris.hughes@exertis.co.uk");
                    if (!testing)
                        message.CC.Add("alina.gavenyte@exertis.co.uk");
                    message.To.Add("chris.hughes@exertis.co.uk");
                    message.Subject = string.Format("VMI_Report_{1}__{0}_", (object)Common.timestamp(), (object)dataRow[2].ToString());
                    message.Body = string.Format("Apple VMI report filename: {0}  VMIReportID: {1}  VMI Report Date: {2}", (object)str1, (object)dataRow[0].ToString(), (object)dataRow[2].ToString());
                    foreach (string fileName in list2)
                    {
                        try
                        {
                            Attachment attachment = new Attachment(fileName);
                            message.Attachments.Add(attachment);
                        }
                        catch
                        {
                        }
                    }
                    smtpClient.Port = 587;
                    smtpClient.Credentials = (ICredentialsByHost)new NetworkCredential("chris.hughes@exertis.co.uk", "Shetland992");
                    smtpClient.EnableSsl = true;
                    try
                    {
                        smtpClient.Send(message);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
    }
}