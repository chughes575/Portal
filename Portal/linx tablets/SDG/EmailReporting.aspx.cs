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

namespace linx_tablets.SDG
{
    public partial class EmailReporting : System.Web.UI.Page
    {
        public int customerID = 1;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string insertSql = string.Format(@" declare @IDNL table (ID int)
					



if ((select count(*) from MSE_PortalVendorEmails where customerid = {0} and weekno = datepart(week, getdate()) and year = datepart(year, getdate()))= 0)
begin

                	insert into MSE_PortalVendorEmails
						output inserted.ID into @IDNL values ({0},datepart(week,getdate()),datepart(year,getdate()))


            end
			else begin
			insert into @IDNL 
			 values ((select id from MSE_PortalVendorEmails where customerid = {0} and weekno = datepart(week, getdate()) and year = datepart(year, getdate())))
			end

			select id from @IDNL", customerID);
                string reportID = Common.runSQLScalar(insertSql).ToString();


                string insertVendorsSql = string.Format(@"exec sp_sdgVendorInsertVendors {0}", reportID);
                string deleteVendorsSql = string.Format(@"exec sp_sdgVendorDeleteVendors {0}", reportID);
                Common.runSQLNonQuery(insertVendorsSql);
                Common.runSQLNonQuery(deleteVendorsSql);


            }
        }
        protected void gvEmailReports_OnRowDataBound(object source, GridViewRowEventArgs e)
        {
            string CurrentreportID = Common.runSQLScalar("select id from MSE_PortalVendorEmails where customerid = " + customerID + " and weekno = datepart(week, getdate()) and year = datepart(year, getdate())").ToString();
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                GridViewRow row = e.Row;
                GridView gvReportVendors = new GridView();
                gvReportVendors = (GridView)row.FindControl("gvReportVendors");
                Button btnSend = new Button();
                btnSend = (Button)row.FindControl("btnSendReportVendors");

                string reportID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
                if (reportID != CurrentreportID)
                {
                    btnSend.Visible = false;
                }
                DataSet dsReportVendors = Common.runSQLDataset(string.Format(@"exec [sp_portaldownloademailreportvendors_display] {0},{1}", reportID, customerID));

                gvReportVendors.DataSource = dsReportVendors;
                gvReportVendors.DataBind();
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

        protected void gvReportVendors_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string commandName = e.CommandName.ToString();
            if (commandName == "reprocess")
            {
                string[] arr = e.CommandArgument.ToString().Split('-');
                string spCall = string.Format("sp_vendoremailreprocess {0},{1}", arr[0].ToString(), arr[1].ToString());
                Common.runSQLNonQuery(spCall);
                DataRow dr = Common.runSQLRow("select * from mse_portalvendors where vendorid=" + arr[1].ToString());
                bool success = sendVendorEmail(int.Parse(dr[0].ToString()), dr[2].ToString(), dr[3].ToString(), int.Parse(arr[0].ToString()));
                string progress = "";
                if (success)
                {
                    progress += string.Format("{0} {1} Manuf: {2} - Success {3}", dr[0].ToString(), dr[2].ToString(), dr[3].ToString(), "<br>");
                }
                else
                {
                    progress += string.Format("{0} {1} Manuf: {2} - Failed {3}", dr[0].ToString(), dr[2].ToString(), dr[3].ToString(), "<br>");
                }

                lblNumberImported.Text = progress;
                pnlOK.Visible = true;

                sqlDSorscleLastLeadTime.DataBind();
                gvEmailReports.DataBind();
            }
            if (commandName == "preview")
            {

                string spCall = string.Format("exec [sp_sdgVendorPOEmail] {0}", e.CommandArgument.ToString());
                string fileName = "Vendor_Sales_" + Common.timestamp() + ".csv";
                runReport(spCall, fileName);
            }


        }
        protected void gvReportVendors_RowDataBound(object source, GridViewRowEventArgs e)
        {
            string CurrentreportID = Common.runSQLScalar("select id from MSE_PortalVendorEmails where customerid = " + customerID + " and weekno = datepart(week, getdate()) and year = datepart(year, getdate())").ToString();
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridViewRow row = e.Row;
                Button btnSend = new Button();
                btnSend = (Button)row.FindControl("btnReprocessReportVendors");

                string reportID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
                if (reportID != CurrentreportID)
                {
                    btnSend.Visible = false;
                }
            }

        }
        protected void gvEmailReports_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string commandName = e.CommandName.ToString();
            if (commandName == "downloadreportvendors")
            {
                string filename = string.Format("Email_Vendors_{1}_{0}.csv", (object)Common.timestamp(), e.CommandArgument.ToString());
                this.runReport(string.Format("exec sp_portaldownloademailreportvendors {0},{1} ", e.CommandArgument.ToString(), customerID), filename);
            }
            if (commandName == "downloadconsolidated")
            {
                string filename = string.Format("Email_Vendors_All.csv", (object)Common.timestamp(), e.CommandArgument.ToString());
                this.runReport(string.Format("exec [sp_sdgVendorPOEmailAll] ", e.CommandArgument.ToString(), customerID), filename);
            }

            
            if (commandName == "sendemails")
            {
                DataRowCollection drC = Common.runSQLRows(string.Format(@"select distinct pv.* from MSE_PortalVendorProcessed jlp inner join MSE_PortalVendors pv on pv.vendorid=jlp.vendorid
where jlp.reportid={0} and processed=0", e.CommandArgument.ToString()));
                string progress = "";
                foreach (DataRow dr in drC)
                {

                    bool success = sendVendorEmail(int.Parse(dr[0].ToString()), dr[2].ToString(), dr[3].ToString(), int.Parse(e.CommandArgument.ToString()));
                    if (success)
                    {
                        progress += string.Format("{0} {1} Manuf: {2} - Success {3}", dr[0].ToString(), dr[2].ToString(), dr[3].ToString(), "<br>");
                    }
                    else
                    {
                        progress += string.Format("{0} {1} Manuf: {2} - Failed \r\n", dr[0].ToString(), dr[2].ToString(), dr[3].ToString());
                    }
                }

                lblNumberImported.Text = progress;
                pnlOK.Visible = true;
                sqlDSorscleLastLeadTime.DataBind();
                gvEmailReports.DataBind();
            }
        }
        protected bool sendVendorEmail(int vendorID, string vendorName, string manufacturer, int reportID)
        {
            bool testing = false;
            bool success = false;
            try
            {
                DataRow dr = Common.runSQLRow("select * from mse_portalvendors where vendorid=" + vendorID);

                List<string> fileList = new List<string>();
                string filePath = @"C:\Linx-tablets\saleseposfiles\emails\";
                string fileName = "Vendor_Sales" + "_" + dr[2].ToString().Replace(":", "") + "_" + dr[3].ToString().Replace(":", "") + "_" + Common.timestamp() + ".csv";
                DataSet ds = Common.runSQLDataset(string.Format("[sp_sdgVendorPOEmail] {0}", dr[0].ToString()));
                if (ds.Tables[0].Rows.Count == 0)
                {
                    bool alertme = true;
                }

                string vendorPoLinesContent = Common.dataTableToTextFile(ds.Tables[0], ",", "\r\n", true);
                if (File.Exists(filePath + fileName))
                    File.Delete(filePath + fileName);

                File.AppendAllText(filePath + fileName, vendorPoLinesContent);
                fileList.Add(filePath + fileName);



                string manufacturerName = dr[3].ToString();
                string recipients = dr[5].ToString();
                string exertisRecipients = dr[8].ToString();

                //email stuff
                string[] emailAddresses = recipients.Split(';');
                string[] eertisEmailAddresses = exertisRecipients.Split(';');

                bool emailValidation = true;
                bool exertisEmailValidation = true;

                if (recipients == "")
                    emailValidation = false;
                if (exertisRecipients == "")
                    exertisEmailValidation = false;

                Regex rgsEmail = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

                //Vendor Recipients
                for (int i = 0; i < emailAddresses.Length; i++)
                {

                    if (!rgsEmail.IsMatch(emailAddresses[i].ToLower()))
                    {
                        emailValidation = false;
                    }
                }

                //Exertis Staff Recipients
                for (int i = 0; i < eertisEmailAddresses.Length; i++)
                {

                    if (!rgsEmail.IsMatch(eertisEmailAddresses[i].ToLower()))
                    {
                        exertisEmailValidation = false;
                    }
                }

                bool useEx = true;
                MailMessage mail = new MailMessage();
                mail.Subject = string.Format("{0}/{1} : Shop Direct - Stock & Sales", vendorName, manufacturerName);
                SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                if (useEx)
                {
                    SmtpServer = new SmtpClient("exertissap-co-uk01i.mail.protection.outlook.com");
                }
                mail.From = new System.Net.Mail.MailAddress(Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailUsername' and customerid=1").ToString());
                SmtpServer.Port = 587;

                SmtpServer.Credentials = new System.Net.NetworkCredential(Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailUsername' and customerid=1").ToString(), Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailPassword' and customerid=1").ToString());
                SmtpServer.EnableSsl = true;

                if (useEx)
                {
                    mail.From = new System.Net.Mail.MailAddress("noreply@exertis-sap.co.uk");
                    SmtpServer.Port = 25;
                    SmtpServer.Credentials = null;
                    SmtpServer.EnableSsl = false;
                }
                //mail.To.Add("chris.hughes@exertis.co.uk");



                if (testing)
                {
                    mail.To.Add("chris.hughes@exertis.co.uk");
                }
                else
                {
                    mail.CC.Add("chris.hughes@exertis.co.uk");
                    mail.CC.Add("alina.gavenyte@exertis.co.uk");
                    mail.To.Add("sdgsupplychain@exertis.co.uk");
                }
                if (emailValidation)
                {
                    if (!testing)
                    {
                        for (int i1 = 0; i1 < emailAddresses.Length; i1++)
                        {
                            mail.To.Add(emailAddresses[i1]);
                        }
                    }
                }
                else
                {
                    mail.Subject += string.Format(" Missing Vendor/Manuf Email To Forward", vendorName, manufacturerName);
                }


                if (exertisEmailValidation)
                {

                    for (int i1 = 0; i1 < eertisEmailAddresses.Length; i1++)
                    {
                        mail.To.Add(eertisEmailAddresses[i1]);
                    }
                }
                mail.Body = Common.runSQLScalar("select configvalue from portalconfig  where configkey='SDGVendorEmailBody'").ToString().Replace("[VENDORNAME]", vendorName).Replace("MANUFACTURER", manufacturerName);

                mail.IsBodyHtml = true;
                AlternateView av = AlternateView.CreateAlternateViewFromString(Common.runSQLScalar("select configvalue from portalconfig where configkey='SDGVendorEmailBodyHTML'").ToString().Replace("[VENDORNAME]", vendorName).Replace("MANUFACTURER", manufacturerName), null, "text/html");
                mail.AlternateViews.Add(av);
                foreach (string file in fileList)
                {
                    try
                    {
                        System.Net.Mail.Attachment attachmentStockUpFile;
                        attachmentStockUpFile = new System.Net.Mail.Attachment(file);
                        mail.Attachments.Add(attachmentStockUpFile);

                    }
                    catch
                    {
                    }
                }
                
                string updateSQL = string.Format(@"update MSE_PortalVendorProcessed set Processed=1 where vendorid={0} and reportid={1}", vendorID, reportID);
                try
                {

                    if (mail.Attachments.Count == 0)
                        throw new Exception("No attachments aborting!!!");

                    SmtpServer.Send(mail);
                    Common.runSQLNonQuery(updateSQL);
                    success = true;

                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {
                success = false;
            }
            return success;
        }
    }
}