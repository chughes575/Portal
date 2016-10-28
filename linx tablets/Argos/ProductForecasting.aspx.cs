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
namespace linx_tablets.Argos
{
    public partial class ProductForecasting : System.Web.UI.Page
    {
        public List<SqlParameter> forecastExceptionInsertParameters = new List<SqlParameter>();
        protected void Page_Load(object sender, EventArgs e)
        {
            string forecastWeek = Common.runSQLScalar("select datepart(week,getdate())").ToString();
            lblCurrentForecastWeek.Text = forecastWeek;
            if (!Page.IsPostBack)
            {
                string ForecastSellThruWeeksUsed = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsed' and CustomerID=4").ToString();
                ddlForecastAmountUsed.SelectedIndex = ddlForecastAmountUsed.Items.IndexOf(ddlForecastAmountUsed.Items.FindByValue(ForecastSellThruWeeksUsed));

                string selectSQL = "select configvalue from portalconfig where configkey='SellThruOverwrite' and customerid=" + 4;
                txtSellThroughPercentage.Text = Common.runSQLScalar(selectSQL).ToString();
            }

            string historicForecastYears = @"select distinct a.forecastyear from (
select distinct forecastyear from MSE_PortalForecastingData where forecastyear<=datepart(year,getdate()) and customerid in (4)
union
select 2016 as forecastyear
) a ";
            ddlHistoricForecastYears.DataSource = Common.runSQLDataset(historicForecastYears);
            ddlHistoricForecastYears.DataBind();



        }
        protected void btnUpdateForecastUsed_Click(object sender, EventArgs e)
        {
            int leadTime = int.Parse(ddlForecastAmountUsed.SelectedValue.ToString());
            Common.runSQLNonQuery("update PortalConfig set configvalue='" + leadTime + "' where ConfigKey='ForecastWeeksUsed' and CustomerID=4");
            Common.runSQLNonQuery(string.Format("update mse_portalforecastreportmanagement set lastfiledate=getdate(),Username='{0}' where reportid=44", HttpContext.Current.User.Identity.Name.ToString()));
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Forecast Weeks Used Update Successful');", true);
        }
        protected void btnUpdateSellThroughOverwrite_Click(object sender, EventArgs e)
        {

            string updateSQL = string.Format("update portalconfig set configvalue={0} where configkey='SellThruOverwrite' and customerid=" + 4, txtSellThroughPercentage.Text);
            Common.runSQLNonQuery(updateSQL);
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Sell Through Overwrite Update Successful');", true);
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
        protected void btnUploadArgosForecast_Click(object sender, EventArgs e)
        {
            string temploadTable = "productdataloader_PortalForecastingData_argos_tempload";
            string loadTableName = "portalforecastupload_argosmain_104_tempload";
            if (fupArgosForecast.HasFile)
            {
                Common.runSQLNonQuery("DELETE FROM " + temploadTable);
                Common.runSQLNonQuery("DELETE FROM " + loadTableName);

                string filename = Path.GetFileNameWithoutExtension(fupArgosForecast.FileName) + "_" + Common.timestamp() + Path.GetExtension(fupArgosForecast.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fupArgosForecast.SaveAs(filePathLocale + filename);
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
                    string bulkInsert = string.Format(@"BULK INSERT " + loadTableName + @" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + loadTableName).ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from " + loadTableName + @"  where (catno is null)").ToString()) > 0)
                        throw new Exception("Ensure CustomerSKU (column a) is populated for every line");


                    int weeksRemain = 0;
                    int weekNo = int.Parse(Common.runSQLScalar("select datepart(week,getdate())").ToString());
                    int curWeekNoCount = 0;
                    string yearNo = Common.runSQLScalar("select datepart(year,getdate())").ToString();
                    string yearNon = Common.runSQLScalar("select datepart(year,getdate())+1").ToString();
                    string yearNon1 = Common.runSQLScalar("select datepart(year,getdate())+2").ToString();
                    string valuesSQL = "";
                    string valuesSQLN = "";
                    string valuesSQLN1 = "";
                    string nulls = "select catno," + yearNo.ToString() + ",";
                    string nullsNext = "select catno," + yearNon.ToString() + ",";
                    string nullsNext1 = "select catno," + yearNon1.ToString() + ",";
                    for (int i1 = 1; i1 < weekNo; i1++)
                    {
                        nulls += "null,";
                    }
                    for (int i = 1; i <= 104; i++)
                    {
                        if (weekNo + i <= 53)
                        {
                            valuesSQL += "Forecastweek" + (i).ToString() + ",";
                            curWeekNoCount++;
                        }
                        else
                        {
                            weeksRemain = 105 - i;
                            break;
                        }
                    }
                    for (int i1 = 0; i1 < 52; i1++)
                    {
                        valuesSQLN += "Forecastweek" + ((105 + i1) - weeksRemain).ToString() + ",";
                        curWeekNoCount++;
                    }
                    int weeksRemainC = 104 - ((53 - weekNo) + 52);
                    for (int i5 = 1; i5 <= weeksRemainC; i5++)
                    {
                        valuesSQLN1 += "Forecastweek" + (curWeekNoCount + i5).ToString() + ",";

                    }

                    for (int nullsC = 1; nullsC <= (52 - weeksRemainC); nullsC++)
                    {
                        valuesSQLN1 += "null,";
                    }




                    valuesSQL = valuesSQL.Substring(0, valuesSQL.Length - 1);
                    valuesSQLN = valuesSQLN.Substring(0, valuesSQLN.Length - 1);
                    valuesSQLN1 = valuesSQLN1.Substring(0, valuesSQLN1.Length - 1);

                    nulls += valuesSQL;
                    nullsNext += valuesSQLN;
                    nullsNext1 += valuesSQLN1;

                    nulls += " from " + loadTableName + " union ";
                    nullsNext += " from " + loadTableName + " union ";
                    nullsNext1 += " from " + loadTableName;
                    string sqlInsert = "insert into productdataloader_PortalForecastingData_argos_tempload ";
                    sqlInsert += nulls;
                    sqlInsert += nullsNext;
                    sqlInsert += nullsNext1;


                    Common.runSQLNonQuery(sqlInsert);



                    string updateSQL = string.Format("exec sp_portalargosforecastupload '{0}','{1}'", fupArgosForecast.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the argos forecast has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report.');", true);
                }
            }
        }
        protected void btnUploadArgosIntake_Click(object sender, EventArgs e)
        {
            string temploadTable = "productdataloader_portal_argos_intake_tempload";
            string lineTable ="test_load_portal_argos_intakeweeks_lines";
            string headerTable ="testportal_argos_intake_lines";
            if (fupArgosIntake.HasFile)
            {
                Common.runSQLNonQuery("DELETE FROM " + temploadTable);
                Common.runSQLNonQuery("DELETE FROM " + lineTable);
                Common.runSQLNonQuery("DELETE FROM " + headerTable);

                string filename = Path.GetFileNameWithoutExtension(fupArgosIntake.FileName) + "_staging_" + Common.timestamp() + Path.GetExtension(fupArgosIntake.FileName);
                string amendedFilename = Path.GetFileNameWithoutExtension(fupArgosIntake.FileName) + "_" + Common.timestamp() + Path.GetExtension(fupArgosIntake.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fupArgosIntake.SaveAs(filePathLocale + filename);
                    }
                    catch
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('File save failure');", true);
                    }
                    string testString =File.ReadAllText(filePathLocale + filename);
                    testString = "0," + testString.Substring(testString.IndexOf("\n", 0) + 1, testString.Length - (testString.IndexOf("\n", 0) + 1));
                    int indexCheck = 0;
                    int startIndex = 0;
                    int indexCounter = 1;

                    while (indexCheck > -2)
                    {
                        int index = testString.IndexOf("\n", startIndex);
                        if (index == testString.LastIndexOf("\n"))
                        {
                            break;
                        }
                        startIndex = index + 3;
                        string beforeSub = testString.Substring(0, index + 1);
                        string afterSub = testString.Substring(index + 1, (testString.Length - index) - 1);

                        testString = beforeSub + indexCounter.ToString() + "," + afterSub;
                        indexCounter++;
                        indexCheck = testString.IndexOf("\n", startIndex);
                    }
                    File.AppendAllText(filePathLocale + amendedFilename, testString);


                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalUploadedFiles/", "exertissdg", "Exertissdg1");
                    try
                    {
                        ftpClient.uploadFile(filePathLocale + amendedFilename);
                    }
                    catch (Exception ex)
                    {
                    }
                    string newFilename = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\exertissdg\portalUploadedFiles\" + amendedFilename;
                    string bulkInsert = string.Format(@"BULK INSERT " + temploadTable + @" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    string testSP = "exec [sp_argosemailimport_intake_test]";
                    Common.runSQLNonQuery(testSP);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + temploadTable).ToString()) == 0)
                        throw new Exception("Table empty");


                    string updateSQL = string.Format("exec [sp_argosemailimport_intake_weeks] '{0}','{1}',null", fupArgosIntake.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    sqlDSArgosIntakeUploads.DataBind();
                    gvArgosUploads.DataBind();
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the argos forecast has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report.');", true);
                }
            }
        }
        protected void gvArgosUploads_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string commandName = e.CommandName.ToString();
            if (commandName == "downloadfile")
            {
                this.runReport(string.Format(" select il.[ID],iwl.[Product],[Supplier Ref],[Description],[Lead Time],[Stock Controller],[Catalogue Opening Stock],[Deliveries to date],[Catalogue to date sales],[Current Stock],[Catalogue to date demand],[Overdue deliveries],[Initial Catalogue Estimate],[Latest Catalogue Estimate],[Next Catalogue Estimate],[Next + 1 Catalogue Estimate],[Week],[Week Commencing],[Firm Orders],[Planned Orders],[Product On promotion],[Forecast Demand],[Stock Target],[Forecasted Stock ] from  mse_portal_argos_intakeweeks_lines  iwl inner join mse_portal_argos_intake_lines il on il.ID=iwl.ID and il.Product=iwl.Product where iwl.id={0} order by il.product", (object)e.CommandArgument.ToString()), string.Format("Argos_Intake_ID_{0}_{1}.csv", (object)e.CommandArgument.ToString(), (object)Common.timestamp()));
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

        protected void btnDownload3PLForecast_Click(object sender, EventArgs e)
        {
            string filename = "Argos_Forecast_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_forecastportalforecastdownload 4", filename);
        }
        protected void btnDownload3plForecastTemplate_Click(object sender, EventArgs e)
        {
            string filename = "Argos_Forecast_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_forecastportalforecastdownloadtemplate 4", filename);
        }

        protected void btnDownloadForecastHistoric_Click(object sender, EventArgs e)
        {
            string filename = "Argos_Forecast_Historic_Year_" + ddlHistoricForecastYears.SelectedValue + "_" + Common.timestamp() + ".csv";
            runReport(string.Format("exec [sp_forecastportalforecasthistoricdownload] 4,{0}", ddlHistoricForecastYears.SelectedValue), filename);
        }

    }
}