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
    public partial class VendorPO : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!Page.IsPostBack)
            bindForecastWeeksDDL();

        }
        protected void bindForecastWeeksDDL()
        {
            string poWeeks = Common.runSQLScalar("select cast(configvalue as int) from mse_appleconfig where configkey='Po Weeks'").ToString();
            ddlForecastWeeks.DataBind();
            ddlForecastWeeks.SelectedIndex = ddlForecastWeeks.Items.IndexOf(ddlForecastWeeks.Items.FindByText(poWeeks));
        }
        protected void btnRunPoSUggestions_Click(object sender, EventArgs e)
        {
            this.createPoSuggestion(false);
        }
        protected void btnUkReplenFile_Command(object sender, CommandEventArgs e)
        {
            string str1 = e.CommandArgument.ToString().Substring(0, 1);
            string str2 = e.CommandArgument.ToString().Substring(2, 1);
            string str3 = "";
            if (!(str2 == "1"))
            {
                if (!(str2 == "2"))
                {
                    if (!(str2 == "3"))
                    {
                        if (!(str2 == "4"))
                        {
                            if (str2 == "5")
                                str3 = "Lu50";
                        }
                        else
                            str3 = "Lu40";
                    }
                    else
                        str3 = "Lu30";
                }
                else
                    str3 = "Lu20";
            }
            else
                str3 = "Lu10";

            if (str1 == "r" && str2 == "9")
            {
                this.runReport("exec sp_appleposuggestionsconsolidatedALL", "POSuggestions_Report_consolidated_All_" + Common.timestamp() + ".csv");
            }
            else if (str1 == "r" && str2 == "0")
            {
                this.runReport("exec sp_appleposuggestionsconsolidated", "POSuggestions_Report_consolidated_" + Common.timestamp() + ".csv");
            }

            else
            {
                if (!(str1 == "r"))
                    return;
                //this.runReport("select * from vw_ApplePoSuggestionsLocale where plantcode='" + str3 + "'", "POSuggestions_Report_" + str3 + "_" + Common.timestamp() + ".csv");
                this.runReport("exec sp_ApplePoSuggestionsLocale " + str2, "POSuggestions_Report_" + str3 + "_" + Common.timestamp() + ".csv");
            }
        }

        protected void btnUkReplenFile_Command_Bretford(object sender, CommandEventArgs e)
        {
            string str1 = e.CommandArgument.ToString().Substring(0, 1);
            string str2 = e.CommandArgument.ToString().Substring(2, 1);
            string str3 = "";
            if (!(str2 == "1"))
            {
                if (!(str2 == "2"))
                {
                    if (!(str2 == "3"))
                    {
                        if (!(str2 == "4"))
                        {
                            if (str2 == "5")
                                str3 = "Lu50";
                        }
                        else
                            str3 = "Lu40";
                    }
                    else
                        str3 = "Lu30";
                }
                else
                    str3 = "Lu20";
            }
            else
                str3 = "Lu10";

            if (str1 == "r" && str2 == "9")
            {
                this.runReport("exec sp_appleposuggestionsconsolidatedALLBretford", "POSuggestions_Report_consolidated_Bretford_All_" + Common.timestamp() + ".csv");
            }
            else if (str1 == "r" && str2 == "0")
            {
                this.runReport("exec sp_appleposuggestionsconsolidatedBretford", "POSuggestions_Report_consolidated_Bretford_" + Common.timestamp() + ".csv");
            }
            else
            {
                if (!(str1 == "r"))
                    return;
                //this.runReport("select * from vw_ApplePoSuggestionsLocaleBretford where plantcode='" + str3 + "'", "POSuggestions_Bretford_Report_" + str3 + "_" + Common.timestamp() + ".csv");
                this.runReport("exec sp_ApplePoSuggestionsLocaleBretford " + str2, "POSuggestions_Bretford_Report_" + str3 + "_" + Common.timestamp() + ".csv");
            }
        }
        private void createPoSuggestion(bool testing)
        {
            DataRowCollection dataRowCollection = Common.runSQLRows("select * from mse_applelocalemapping");
            List<string> list = new List<string>();
            foreach (DataRow dataRow in (InternalDataCollectionBase)dataRowCollection)
            {
                try
                {
                    string contents = Common.dataTableToTextFile(Common.runSQLDataset(string.Format("select * from vw_ApplePoSuggestionsLocale where plantcode='{0}'", (object)dataRow[1].ToString())).Tables[0], ",", "\r\n", true);
                    string str1 = "\\\\10.16.72.129\\company\\applefiles\\";
                    string str2 = string.Format("Po_Suggestions_{0}_{1}_{2}.csv", (object)dataRow[1].ToString(), (object)dataRow[2].ToString(), (object)Common.timestamp());
                    System.IO.File.AppendAllText(str1 + str2, contents);
                    list.Add(str1 + str2);
                }
                catch
                {
                }
            }
            string contents1 = Common.dataTableToTextFile(Common.runSQLDataset("select  *from vw_ApplePoSuggestionsConsolidated").Tables[0], ",", "\r\n", true);
            string str3 = "\\\\10.16.72.129\\company\\applefiles\\";
            string str4 = string.Format("Po_Suggestions_Consolidated_{0}.csv", (object)Common.timestamp());
            System.IO.File.AppendAllText(str3 + str4, contents1);
            list.Add(str3 + str4);
            if (list.Count <= 0)
                return;
            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
            message.From = new MailAddress("chris.hughes@exertis.co.uk");
            if (!testing)
                message.CC.Add("alina.gavenyte@exertis.co.uk");
            if (testing)
                message.To.Add("chris.hughes@exertis.co.uk");
            else
                message.To.Add("chris.hughes@exertis.co.uk");
            message.Subject = "Apple_Po_Suggestions_" + Common.timestamp();
            message.Body = "";
            foreach (string fileName in list)
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

        protected void btnUpdateWeeks_Click(object sender, EventArgs e)
        {
            int updateVal = int.Parse(ddlForecastWeeks.SelectedValue.ToString());
            string updateSQL = "update mse_appleconfig set configvalue='" + updateVal + "' where configkey='Po Weeks'";
            Common.runSQLNonQuery(updateSQL);
            bindForecastWeeksDDL();
        }
    }
}