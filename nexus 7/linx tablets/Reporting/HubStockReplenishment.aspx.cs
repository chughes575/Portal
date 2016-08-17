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
    public partial class HubStockReplenishment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            bindSiButtons();
            if (!Page.IsPostBack)
            {
                this.bindSafetyData();
                this.bindForecastData();
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
        protected void btntockOverride_Click(object sender, EventArgs e)
        {
            if (fuStockOverride.HasFile)
            {
                Common.runSQLNonQuery("delete from productdataloader_oracleapplestockoverride_tempload");
                string filename = Path.GetFileNameWithoutExtension(fuStockOverride.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuStockOverride.FileName);
                string filePath = @"\\10.16.72.129\company\";
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuStockOverride.SaveAs(filePathLocale + filename);

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
                        string bulkInsert = string.Format(@"BULK INSERT productdataloader_oracleapplestockoverride_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                        Common.runSQLNonQuery(bulkInsert);
                        if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_oracleapplestockoverride_tempload").ToString()) == 0)
                            throw new Exception();

                        string updateSQL = string.Format("exec sp_productdataloader_importstockoverride '{0}','{1}'", amendedFileName, HttpContext.Current.User.Identity.Name.ToString());
                        Common.runSQLNonQuery(updateSQL);
                    }
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the stock file has been imported');", true);

                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file');", true);
                }
            }
        }

        private void bindForecastData()
        {
            this.gvForecastOverride.DataSource = (object)Common.runSQLDataset("select * from  mse_appleforecastoverride");
            this.gvForecastOverride.DataBind();
        }
        private void bindSafetyData()
        {
            this.gvSafety.DataSource = (object)Common.runSQLDataset("select * from MSE_AppleLocaleSafetyLevels sf inner join mse_applelocalemapping lm on lm.localeid=sf.localeid");
            this.gvSafety.DataBind();
        }
        protected void btnDownloadShipments_ClickNew(object sender, EventArgs e)
        {
            string filename = string.Format("Apple_InTransit_Scheduled_Report_{0}.csv", (object)Common.timestamp());
            this.runReport("select * from vw_AppleInTransitReport order by [Hub Date Scheduled /  In Transit]", filename);
        }
        protected void btnRunStockReplen_Click(object sender, EventArgs e)
        {
            this.createStockUpOrders(true, false);
        }
        protected void btnUkReplenReport_Command(object sender, CommandEventArgs e)
        {
            string str1 = e.CommandArgument.ToString().Substring(0, 1);
            string str2 = e.CommandArgument.ToString().Substring(2, 1);
            string localeID = "";
            string str3 = "";
            string locale = "";
            if (!(str2 == "1"))
            {
                if (!(str2 == "2"))
                {
                    if (!(str2 == "3"))
                    {
                        if (!(str2 == "4"))
                        {
                            if (str2 == "5")
                            {
                                str3 = "Lu50";
                                locale = "UAE";
                                localeID = "5";
                            }
                        }
                        else
                        {
                            str3 = "Lu40";
                            locale = "IT";
                            localeID = "4";
                        }
                    }
                    else
                    {
                        str3 = "Lu30";
                        locale = "CZ";
                        localeID = "3";
                    }
                }
                else
                {
                    str3 = "Lu20";
                    locale = "NL";
                    localeID = "2";
                }
            }
            else
            {
                str3 = "Lu10";
                locale = "UK";
                localeID = "1";
            }
            if (str1 == "r")
            {
                this.runReport("exec [sp_StockUpLocale] " + str2, "StockUp_Report_" + str3 + "_" + Common.timestamp() + ".csv");
            }
            else if (str1 == "c")
            {

                Response.Redirect("DownloadFile.ashx?localeid=" + str2);
                //this.runReport("exec [sp_StockUpLocaleCarton] " + str2, "StockUp_Report_Carton_" + str3 + "_" + Common.timestamp() + ".csv");
            }
            else
            {
                //string query = string.Format("select lm.PlantDescription,lm.ExertisOutAccount,\r\nlm.PlantCode, vw.AppleCode, vw.Exertis_Part_Number, toorder,lm.edilocationcode\r\n from vw_applestockuplocales vw inner join mse_applelocalemapping lm on lm.plantcode = vw.plantcode\r\n where vw.plantcode = '{0}' and toorder> 0", (object)str3);
                string str4 = Common.runSQLScalar(string.Format("[sp_StockUpLocaleReplen] {0}", localeID)).ToString();
                this.sqlDSReplenEntries.DataBind();

                this.gvReplenEntries.DataBind();
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Replen created, ReplenID: " + str4 + "');", true);
            }
        }
        public string createReplenOrderFile(string query, string locale, bool oracleSubmission)
        {
            string str1 = "Apple_" + locale + "_" + Common.runSQLScalar("select CAST(DATEPART(d,COALESCE(getdate(),'01/01/1901')) AS Varchar)+ '-'+\r\n                                 UPPER(CONVERT(varchar(3), DATENAME(MONTH, COALESCE(getdate(),'01/01/1901'))))+ '-'+\r\n                                     CAST(DATEPART(yyyy,COALESCE(getdate(),'01/01/1901')) AS Varchar)").ToString();
            List<string> list = new List<string>();
            string str2 = string.Empty;
            foreach (DataRow dataRow in (InternalDataCollectionBase)Common.runSQLRows(query))
            {
                string str3 = dataRow[5].ToString();
                string str4 = dataRow[6].ToString();
                string str5 = string.Empty;
                string str6 = string.Empty;
                string str7 = string.Empty;
                string str8 = string.Empty;
                string str9 = string.Empty;
                string str10 = string.Empty;
                string str11 = dataRow[1].ToString() + "," + str1 + "," + dataRow[4].ToString() + "," + dataRow[4].ToString() + "," + str3 + "," + string.Empty + "," + string.Empty + "," + str2 + "," + str4 + "," + str5 + "," + str6 + "," + str7 + "," + str8 + "," + str9 + ",0.00," + str10 + "," ?? "";
                list.Add(str11);
            }
            string str12 = "";
            try
            {
                string str3 = "MSE_ORDERS_" + Common.timestamp() + ".csv";
                str12 = "C:\\Linx-tablets\\replen files\\" + str3;
                if (System.IO.File.Exists(str12))
                    System.IO.File.Move(str12, str12 + ".OLD");
                if (!oracleSubmission)
                {
                    System.IO.File.WriteAllLines(str12, list.ToArray());
                    this.Response.ContentType = "text/comma-separated-values";
                    this.Response.AddHeader("content-disposition", "attachment; filename=" + str3);
                    this.Response.Buffer = true;
                    this.Response.Write(System.IO.File.ReadAllText(str12));
                }
            }
            catch (Exception ex)
            {
            }
            return str12;
        }
        private void createStockUpOrders(bool runToday, bool testing)
        {
            DataRowCollection dataRowCollection1 = Common.runSQLRows("select * from mse_applelocalemapping");
            int num1 = int.Parse(Common.runSQLScalar("select coalesce((select count(*) from MSE_AppleReplenDates where cast(ReplenReportDate as date) = cast(getdate() as date) and processed=0) ,0)").ToString());
            int num2 = int.Parse(Common.runSQLScalar("select count(*) from MSE_AppleInventoryreports where DATEDIFF(HOUR,datecreated,GETDATE())<24").ToString());
            int num3 = 1;
            if (!(num1 == num3 | runToday))
                return;
            if (num2 == 5)
            {
                Common.runSQLNonQuery("insert into MSE_AppleReplenDates \r\nselect getdate(),0 from MSE_AppleReplenDates where cast(getdate() as date) not in (select ReplenReportDate from MSE_AppleReplenDates)");
                List<string> list = new List<string>();
                foreach (DataRow dataRow in (InternalDataCollectionBase)dataRowCollection1)
                {
                    try
                    {
                        string contents = Common.dataTableToTextFile(Common.runSQLDataset(string.Format("select * from vw_applestockuplocales where plantcode='{0}'", (object)dataRow[1].ToString())).Tables[0], ",", "\r\n", true);
                        string str1 = "\\\\10.16.72.129\\company\\applefiles\\";
                        string str2 = string.Format("StockUp_Suggestions_{0}_{1}_{2}.csv", (object)dataRow[1].ToString(), (object)dataRow[2].ToString(), (object)Common.timestamp());
                        System.IO.File.AppendAllText(str1 + str2, contents);
                        list.Add(str1 + str2);
                    }
                    catch
                    {
                    }
                }
                DataRow dataRow1 = Common.runSQLRow("SELECT        TOP (1) * FROM dbo.MSE_AppleForecastCommitReports ORDER BY ReportDate DESC");
                int index1 = 2;
                string str3 = dataRow1[index1].ToString();
                int index2 = 3;
                string str4 = dataRow1[index2].ToString();
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
                message.Subject = "Apple_StockUp_Suggestions_" + Common.timestamp();
                message.Body = string.Format("Apple Forecast Commit report filename: {0} \\n Apple Forecast Commit report imported at: {1} \\n \\n ", (object)str3, (object)str4);
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
                    Common.runSQLNonQuery(string.Format("update MSE_AppleReplenDates set Processed=1 where cast(getdate() as date) = ReplenReportDate", (object)1));
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                if (DateTime.Now.Hour < 9)
                    return;
                MailMessage message = new MailMessage();
                SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
                message.From = new MailAddress("chris.hughes@exertis.co.uk");
                message.To.Add("chris.hughes@exertis.co.uk");
                message.Subject = (string)(object)int.Parse(Common.runSQLScalar("select 5-count(*) from MSE_AppleInventoryreports where cast(datecreated as date)=cast(getdate() as date)").ToString()) + (object)" Outstanding inventory reports - Replen orders not run";
                DataRowCollection dataRowCollection2 = Common.runSQLRows("select * from MSE_AppleLocaleMapping lm \r\nleft outer join (select localeid,reportid from MSE_AppleInventoryreports where cast(datecreated as date)=cast(getdate() as date)) as reports on reports.LocaleID=lm.LocaleID\r\nwhere reports.reportid is null");
                string str = "";
                int num4 = 0;
                foreach (DataRow dataRow in (InternalDataCollectionBase)dataRowCollection2)
                {
                    str = str + dataRow[1].ToString() + " - " + dataRow[2].ToString() + ", ";
                    ++num4;
                }
                if (num4 > 0)
                    str = str.Substring(0, str.Length - 2);
                message.Body = "Locales missing: " + str;
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
        protected void gvSafety_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow gridViewRow = this.gvSafety.Rows[e.RowIndex];
                string str = this.gvSafety.DataKeys[e.RowIndex].Value.ToString();
                string id = "txtSafety";
                string idEOL = "txtSafetyEOL";
                TextBox textBox = (TextBox)gridViewRow.FindControl(id);
                TextBox textBoxEOL = (TextBox)gridViewRow.FindControl(idEOL);
                Common.runSQLNonQuery(string.Format("update MSE_AppleLocaleSafetyLevels set safetystockpercentage={1},eolsafetystockpercentage={2} where localeid='{0}' ", (object)str, (object)textBox.Text, (object)textBoxEOL.Text));
                this.gvSafety.EditIndex = -1;
                this.bindSafetyData();
            }
            catch
            {
            }
        }
        protected void gvForecastOverride_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow gridViewRow = this.gvForecastOverride.Rows[e.RowIndex];
                string str = this.gvSafety.DataKeys[e.RowIndex].Value.ToString();
                TextBox ddlWeek1b = (TextBox)gridViewRow.FindControl("txtWeek1");
                TextBox ddlWeek2b = (TextBox)gridViewRow.FindControl("txtWeek2");
                TextBox ddlWeek3b = (TextBox)gridViewRow.FindControl("txtWeek3");

                Common.runSQLNonQuery(string.Format("update mse_appleforecastoverride set week1={0},week2={1},week3={2} ", ddlWeek1b.Text, ddlWeek2b.Text, ddlWeek3b.Text));
                this.gvForecastOverride.EditIndex = -1;
                this.bindForecastData();
            }
            catch
            {
            }
        }
        protected void gvForecastOverride_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.gvForecastOverride.EditIndex = -1;
            this.bindForecastData();
        }

        protected void gvForecastOverride_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.gvForecastOverride.EditIndex = e.NewEditIndex;
            this.bindForecastData();
        }

        protected void gvSafety_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.gvSafety.EditIndex = -1;
            this.bindSafetyData();
        }

        protected void gvSafety_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.gvSafety.EditIndex = e.NewEditIndex;
            this.bindSafetyData();
        }

        protected void gvReplenEntries_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string commandName = e.CommandName.ToString();
            if (commandName == "downloadReplenLines")
            {
                this.runReport(string.Format(" select rl.ReplenID,lm.Localeid as ExertisLocaleID,lm.PlantCode,rl.AppleCode,rl.Exertis_part_number,Qty from mse_applereplenlines rl\r\n inner join mse_applereplens rlh on rlh.ReplenID=rl.ReplenID\r\n inner join mse_applelocalemapping lm on lm.LocaleID = rlh.LocaleID\r\n Where rlh.replenid={0}", (object)e.CommandArgument.ToString()), string.Format("AppleReplenReport_ReplenID_{0}_{1}.csv", (object)e.CommandArgument.ToString(), (object)Common.timestamp()));
            }
            else if (commandName == "removeReplen")
            {
                string confirmValue = Request.Form["confirm_value"];
                if (confirmValue == "Yes")
                {
                    try
                    {
                        string deleteReplen = string.Format(@"delete from mse_applereplenlines where replenid in ({0})
delete from mse_applereplens where replenid in ({0})", e.CommandArgument.ToString());
                        Common.runSQLNonQuery(deleteReplen);
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('ReplenID: " + e.CommandArgument.ToString() + " Removed');", true);
                        this.sqlDSReplenEntries.DataBind();
                        this.gvReplenEntries.DataBind();
                    }
                    catch (Exception ex)
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Replen removal failed, please try again');", true);
                    }
                }
            }
            else if ((commandName == "submitReplenToOracle") || (commandName == "resubmitReplenToOracle"))
            {
                try
                {
                    string str2 = this.replenFileFromEntry(int.Parse(e.CommandArgument.ToString()));
                    string ftpFolder = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='Oracle Ftp Order Directory'").ToString();
                    if (System.IO.File.Exists(str2))
                    {
                        new FTP("ftp.micro-p.com", ftpFolder, "exertismse", "m1cr0p").uploadFile(str2);
                        Common.runSQLNonQuery(string.Format("update mse_applereplens set senttooracle=1, SentToOracleDate=getdate(),SentToOracleFilename='{1}' where replenid={0}", (object)e.CommandArgument.ToString(), (object)Path.GetFileName(str2)));
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Replen exported to oracle, ReplenID: " + e.CommandArgument.ToString() + " Filename: " + Path.GetFileName(str2) + "');", true);
                    }
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Replen export failed, please try again');", true);
                }
                this.sqlDSReplenEntries.DataBind();
                this.gvReplenEntries.DataBind();
            }
        }

        protected void gvReplenEntries_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            Button button1 = (Button)e.Row.FindControl("btnSubmitToOracle");
            Button button2 = (Button)e.Row.FindControl("btnResubmitToOracle");
            if (DataBinder.Eval(e.Row.DataItem, "SentToOracle").ToString().Equals("Yes"))
                button1.Enabled = false;
            else
                button2.Enabled = false;
        }
        public string replenFileFromEntry(int replenID)
        {
            string sql = string.Format(@" select lm.ExertisOutAccount,rl.AppleCode,Qty,lm.edilocationcodereplen, apm.unitprice
from mse_applereplenlines rl 
inner join MSE_AppleLocaleMapping lm on lm.LocaleID=rl.LocaleID
inner join mse_applereplens ap on ap.replenid=rl.replenid
left outer join MSE_AppleProductMapping apm on apm.applecode=rl.applecode
where rl.replenid={0}", (object)replenID);


            //string orderNumber = "Apple_" + Common.runSQLRow(string.Format("select lm.*\r\nfrom mse_applereplens ar inner join mse_applelocalemapping lm on lm.localeid=ar.LocaleID\r\nwhere ar.ReplenID={0}", (object)replenID))[6].ToString() + "_" + Common.runSQLScalar("select CAST(DATEPART(d,COALESCE(getdate(),'01/01/1901')) AS Varchar)+ '-'+\r\n                                 UPPER(CONVERT(varchar(3), DATENAME(MONTH, COALESCE(getdate(),'01/01/1901'))))+ '-'+\r\n                                     CAST(DATEPART(yyyy,COALESCE(getdate(),'01/01/1901')) AS Varchar)").ToString();
            string orderNumber = Common.runSQLScalar(string.Format("select cast(lm.appleponumber as varchar(100))+'_'+cast(datepart(day,dategenerated) as varchar(2))+cast(datepart(MONTH,dategenerated) as varchar(2))+'_' + cast(coalesce(ar.increment,0)+1 as varchar(30)) from mse_applereplens ar inner join mse_applelocalemapping lm on lm.localeid=ar.LocaleID where ar.ReplenID={0}", replenID)).ToString();
            List<string> list = new List<string>();
            string str2 = string.Empty;
            foreach (DataRow dataRow in (InternalDataCollectionBase)Common.runSQLRows(sql))
            {

                string accountNumber = dataRow[0].ToString();
                string product_ID = "CUS_" + dataRow[1].ToString();
                string qty = dataRow[2].ToString();
                string ediLocationCode = dataRow[3].ToString();
                string unitPrice = dataRow[4].ToString();
                string orderType = "MP-SAL-Consignmt SHIP";
                string delivery_Method = "000001_XX_F_INTXFER";
                string lineVal = accountNumber + "," + orderNumber + "," + product_ID + "," + string.Empty + "," + qty + "," + string.Empty + "," + string.Empty + "," + string.Empty + "," + ediLocationCode + "," + string.Empty + "," + string.Empty + "," + string.Empty + "," + string.Empty + "," + string.Empty + "," + "0" + "," + string.Empty + "," + delivery_Method + "," + string.Empty + "," + orderType;
                list.Add(lineVal);
            }
            string str12 = "";
            try
            {
                str12 = "C:\\Linx-tablets\\replen files\\" + ("MSE_ORDERS_" + Common.timestamp() + ".csv");
                if (System.IO.File.Exists(str12))
                    System.IO.File.Move(str12, str12 + ".OLD");
                System.IO.File.WriteAllLines(str12, list.ToArray());
                Common.runSQLNonQuery(string.Format("update mse_applereplens set increment = coalesce(increment,0)+1 where replenid={0}", replenID));
            }
            catch (Exception ex)
            {
            }

            return str12;
        }
        private void bindSiButtons()
        {
            if (Common.runSQLScalar("select active from productdataloader_textfileimports where importid=153").ToString().ToLower() == "true")
            {
                btnEnableStockSi.Enabled = false;
                btnDisableStockSi.Enabled = true;
                lblStockSiImportStatus.Text = "Enabled";
            }
            else
            {
                btnEnableStockSi.Enabled = true;
                btnDisableStockSi.Enabled = false;
                lblStockSiImportStatus.Text = "Disabled";
            }
        }
        protected void btnEnableStockSi_Click(object sender, EventArgs e)
        {
            Common.runSQLNonQuery("update productdataloader_textfileimports set active=1 where importid=153");
            bindSiButtons();
        }
        protected void btnDisableStockSi_Click(object sender, EventArgs e)
        {
            Common.runSQLNonQuery("update productdataloader_textfileimports set active=0 where importid=153");
            bindSiButtons();
        }
        protected void btnDownloadStockOverrideTemplate_Click(object sender, EventArgs e)
        {
            runReport("select * from productdataloader_oracleapplestockoverride_tempload where ExertisCode='999'", "StockOverrideTemplate.csv");
        }
        protected void btnDownloadStock_Click(object sender, EventArgs e)
        {
            runReport(@"select Exertis_Part_Number,moas.FreeStockQty
  from MSE_Oracle_Apple_Stock moas inner join MSE_AppleProductMapping apm on apm.Exertis_Part_Number=moas.Item_Code", "StockValues_" + Common.timestamp() + ".csv");
        }

        protected void btnManualOrderUploadSubmit_Click(object sender, EventArgs e)
        {

            if (fuManualOrderUpload.HasFile)
            {
                Common.runSQLNonQuery("delete from productdataloader_applereplenorder_tempload");
                string filename = Path.GetFileNameWithoutExtension(fuManualOrderUpload.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuManualOrderUpload.FileName);
                string filePath = @"\\10.16.72.129\company\";
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuManualOrderUpload.SaveAs(filePathLocale + filename);

                    }
                    catch
                    {
                        throw new Exception();
                    }

                    //
                    string reportData = File.ReadAllText(filePathLocale + filename, Encoding.Default);

                    string amendedFileName = "Amended" + filename;

                    File.AppendAllText(filePathLocale + amendedFileName, reportData, Encoding.Default);
                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/apple files/", "extranet", "Extranet1");
                    try
                    {
                        ftpClient.uploadFile(filePathLocale + amendedFileName);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception();
                    }
                    string newFilename = "\\\\10.16.72.129\\company\\ftp\\root\\msesrvdom\\extranet\\apple files\\" + amendedFileName;
                    string bulkInsert = string.Format(@"BULK INSERT productdataloader_applereplenorder_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_applereplenorder_tempload").ToString()) == 0)
                        throw new Exception();

                    string updateSQL = string.Format("exec sp_appleimportmanualreplenorders '{0}','{1}'", amendedFileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    gvReplenEntries.DataBind();
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the order file has been created and the replen orders have been generated');", true);

                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, " + ex.Message + "');", true);
                }
            }
        }

        protected void btnManualOrderTemplate_Click(object sender, EventArgs e)
        {
            string filename = "Manual_Replen_order_Template.csv";
            runReport("select * from vw_replenmanuatemplate", filename);
        }


    }
}