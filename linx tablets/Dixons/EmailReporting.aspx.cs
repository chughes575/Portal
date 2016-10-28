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
    public partial class EmailReporting : System.Web.UI.Page
    {
        public int customerID = 2;
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
             string reportID=   Common.runSQLScalar(insertSql).ToString();


                string insertVendorsSql = string.Format(@"exec sp_dsgVendorInsertVendors {0}", reportID);
                string deleteVendorsSql = string.Format(@"exec sp_dsgVendorDeleteVendors {0}", reportID);
                Common.runSQLNonQuery(insertVendorsSql);
                Common.runSQLNonQuery(deleteVendorsSql);


            }
        }
        protected void gvEmailReports_OnRowDataBound(object source, GridViewRowEventArgs e)
        {
            string CurrentreportID = Common.runSQLScalar("select id from MSE_PortalVendorEmails where customerid = "+customerID+" and weekno = datepart(week, getdate()) and year = datepart(year, getdate())").ToString();
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
                DataRow dr = Common.runSQLRow("select * from mse_portalvendors where vendorid="+ arr[1].ToString());
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

                string spCall = string.Format("exec [sp_DSGVendorPOEmail] {0}", e.CommandArgument.ToString());
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

            bool success = false;
            try
            {
                DataRow dr = Common.runSQLRow("select * from mse_portalvendors where vendorid=" + vendorID);

                List<string> fileList = new List<string>();
                string filePath = @"C:\Linx-tablets\saleseposfiles\emails\";
                string fileName = "Vendor_Sales" + "_" + dr[2].ToString().Replace(":", "") + "_" + dr[3].ToString().Replace(":", "") + "_" + Common.timestamp() + ".csv";
                DataSet ds = Common.runSQLDataset(string.Format("exec [sp_DSGVendorPOEmail] {0}", dr[0].ToString()));
                if (ds.Tables[0].Rows.Count == 0 || ds.Tables[0].Rows.Count == 1)
                {
                    bool ah = true;
                }
                string vendorPoLinesContent = Common.dataTableToTextFile(ds.Tables[0], ",", "\r\n", true);

                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 1 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(0*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 2 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(1*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 3 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(2*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 4 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(3*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 5 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(4*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 6 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(5*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 7 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(6*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 8 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(7*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 9 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(8*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 10 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(9*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 11 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(10*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 12 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(11*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 13 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(12*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 14 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(13*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 15 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(14*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 16 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(15*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 17 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(16*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 18 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(17*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 19 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(18*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 20 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(19*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 21 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(20*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 22 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(21*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 23 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(22*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 24 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(23*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 25 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(24*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 26 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(25*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 27 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(26*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 28 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(27*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 29 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(28*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 30 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(29*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 31 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(30*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 32 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(31*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 33 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(32*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 34 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(33*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 35 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(34*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 36 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(35*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 37 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(36*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 38 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(37*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 39 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(38*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 40 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(39*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 41 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(40*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 42 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(41*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 43 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(42*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 44 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(43*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 45 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(44*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 46 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(45*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 47 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(46*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 48 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(47*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 49 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(48*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 50 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(49*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 51 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(50*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                vendorPoLinesContent = vendorPoLinesContent.Replace("Week 52 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(51*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());

                if (File.Exists(filePath + fileName))
                    File.Delete(filePath + fileName);

                File.AppendAllText(filePath + fileName, vendorPoLinesContent);
                fileList.Add(filePath + fileName);



                string manufacturerName = manufacturer;
                string recipients = dr[5].ToString();
                string exertisContactEmails = dr[8].ToString();

                string formattedExertisContactEmails = "";
                //email stuff
                string[] emailAddresses = recipients.Split(';');
                string[] emailContactAddresses = exertisContactEmails.Split(';');
                bool emailValidation = true;
                bool emailExertisValidation = true;
                if (exertisContactEmails == "")
                    emailExertisValidation = false;

                if (recipients == "")
                    emailValidation = false;

                Regex rgsEmail = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

                for (int i = 0; i < emailContactAddresses.Length; i++)
                {

                    if (rgsEmail.IsMatch(emailContactAddresses[i].ToLower()))
                    {
                        formattedExertisContactEmails += emailContactAddresses[i] + "/";
                    }
                    else
                    {
                        emailExertisValidation = false;
                    }
                }
                if (formattedExertisContactEmails.Length > 0)
                {
                    formattedExertisContactEmails = formattedExertisContactEmails.Substring(0, formattedExertisContactEmails.Length - 1);
                }
                for (int i = 0; i < emailAddresses.Length; i++)
                {

                    if (!rgsEmail.IsMatch(emailAddresses[i].ToLower()))
                    {
                        emailValidation = false;
                    }
                }

                MailMessage mail = new MailMessage();
                mail.Subject = string.Format("{0}/{1} : DSG - Stock & Sales", vendorName, manufacturerName);
                SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                SmtpServer = new SmtpClient("exertissap-co-uk01i.mail.protection.outlook.com");
                mail.From = new System.Net.Mail.MailAddress(Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailUsername' and customerid=6").ToString());
                mail.From = new System.Net.Mail.MailAddress("noreply@exertis-sap.co.uk");
                mail.CC.Add("chris.hughes@exertis.co.uk");
                mail.CC.Add("alina.gavenyte@exertis.co.uk");

                
                bool testing = false;

                if (Common.runSQLScalar("select configvalue from portalconfig where configkey='dsgemailtesting'").ToString() == "1")
                {
                    testing = true;
                    mail.To.Add("chris.hughes@exertis.co.uk");
                }
                else
                {
                    mail.To.Add("sam.williams@exertis.co.uk");
                    mail.To.Add("steve.baldwin@exertis.co.uk");
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
                string defaultEmailAddress = "sam.williams@exertis.co.uk";
                if (!emailExertisValidation)
                {
                    formattedExertisContactEmails = defaultEmailAddress;
                }
                mail.Body = Common.runSQLScalar("select configvalue from portalconfig  where configkey='DSGVendorEmailBody'").ToString().Replace("[VENDORNAME]", vendorName).Replace("[MANUFACTURER]", manufacturerName);
                mail.Body = mail.Body.Replace("[CONTACTEMAIL]", formattedExertisContactEmails);
                mail.IsBodyHtml = true;
                AlternateView av = AlternateView.CreateAlternateViewFromString(Common.runSQLScalar("select configvalue from portalconfig where configkey='DSGVendorEmailBodyhtml'").ToString().Replace("[VENDORNAME]", vendorName).Replace("MANUFACTURER", manufacturerName).Replace("[CONTACTEMAIL]", formattedExertisContactEmails), null, "text/html");
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
                SmtpServer.Port = 587;
                SmtpServer.Port = 25;
                SmtpServer.Credentials = new System.Net.NetworkCredential(Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailUsername' and customerid=6").ToString(), Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailPassword' and customerid=6").ToString());
                SmtpServer.Credentials = null;
                SmtpServer.EnableSsl = true;
                SmtpServer.EnableSsl = false;
                string updateSQL = string.Format(@"update MSE_PortalVendorProcessed set Processed=1 where vendorid={0} and reportid={1}", vendorID, reportID);
                try
                {

                    if (mail.Attachments.Count == 0)
                        throw new Exception("No attachments aborting!!!");

                    SmtpServer.Send(mail);
                    success = true;
                    Common.runSQLNonQuery(updateSQL);

                }
                catch (Exception ex)
                {
                    success = false;
                }
            }
            catch (Exception ex1)
            {
                success = false;
            }
            return success;
        }
    }
}