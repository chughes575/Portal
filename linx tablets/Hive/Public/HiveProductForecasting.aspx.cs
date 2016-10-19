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
namespace linx_tablets.Hive.Public
{
    public partial class CustomerForecasting : System.Web.UI.Page
    {
        public List<SqlParameter> forecastExceptionInsertParameters = new List<SqlParameter>();
        protected void Page_Load(object sender, EventArgs e)
        {
            string forecastWeek = Common.runSQLScalar("select datepart(week,getdate())").ToString();
            lblCurrentForecastWeek.Text = forecastWeek;
            if (!Page.IsPostBack)
            {

            }

            string historicForecastYears3PLSQL =@"select distinct a.forecastyear from (
select distinct forecastyear from MSE_PortalForecastingData where forecastyear<=datepart(year,getdate()) and customerid in (5)
union
select 2016 as forecastyear
) a ";
            string historicForecastYearsExertisHiveSQL =@"select distinct a.forecastyear from (
select distinct forecastyear from MSE_PortalForecastingData where forecastyear<=datepart(year,getdate()) and customerid in (6)
union
select 2016 as forecastyear
) a ";
            ddlHistoricExertisHiveForecastYears.DataSource = Common.runSQLDataset(historicForecastYearsExertisHiveSQL);
            ddlHistoricExertisHiveForecastYears.DataBind();

            ddlHistoric3PLForecastYears.DataSource = Common.runSQLDataset(historicForecastYears3PLSQL);
            ddlHistoric3PLForecastYears.DataBind();

        }
        protected void sqlDsForecastException_Inserting(object sender, SqlDataSourceCommandEventArgs e)
        {
            e.Command.Parameters.Clear();
            foreach (SqlParameter p in forecastExceptionInsertParameters)
                e.Command.Parameters.Add(p);
        }
        protected void gvForecastPercentages_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "NoDataInsert")
            {
                TextBox txtNoDataProductCode = (TextBox)gvForecastPercentages.Controls[0].Controls[0].FindControl("txtNoDataProductCode");
                TextBox txtNoDataForecast = (TextBox)gvForecastPercentages.Controls[0].Controls[0].FindControl("txtNoDataForecast");
                SqlParameter prodCode = new SqlParameter("@productcode", SqlDbType.VarChar, 8000);
                SqlParameter forecastAmount = new SqlParameter("@ForecastAmount", SqlDbType.Float, 8000);
                prodCode.Direction = ParameterDirection.Input;
                forecastAmount.Direction = ParameterDirection.Input;
                prodCode.Value = txtNoDataProductCode.Text;
                forecastAmount.Value = txtNoDataForecast.Text;
                forecastExceptionInsertParameters.Add(prodCode);
                forecastExceptionInsertParameters.Add(forecastAmount);

                try
                {
                    sqlDsForecastException.Insert();
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Please ensure duplicate entries aren't entered');", true);
                }
            }
            else if (e.CommandName == "Insert")
            {
                TextBox txtProductCode = (TextBox)gvForecastPercentages.FooterRow.FindControl("txtProductCode");
                TextBox txtForecastPercentage = (TextBox)gvForecastPercentages.FooterRow.FindControl("txtRetailDescription");

                SqlParameter ProductCodeParam = new SqlParameter("@productcode", SqlDbType.VarChar, 8000);
                SqlParameter forecastAmountParam = new SqlParameter("@forecastamount", SqlDbType.Float, 8000);


                ProductCodeParam.Direction = ParameterDirection.Input;
                ProductCodeParam.Value = txtProductCode.Text;
                forecastAmountParam.Direction = ParameterDirection.Input;
                forecastAmountParam.Value = txtForecastPercentage.Text;

                forecastExceptionInsertParameters.Add(ProductCodeParam);
                forecastExceptionInsertParameters.Add(forecastAmountParam);
                //consignmentRetailerInsertParameters.Add(retailerIDParam);
                try
                {
                    sqlDsForecastException.Insert();
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Please ensure duplicate entries aren't entered');", true);
                }
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
        private void runReport(string query, string filename)
        {
            //this.Session["ReportQuery"] = (object)query;
            //this.Session["ReportQueryIsSp"] = (object)false;
            //this.Session["ReportDelimiter"] = (object)",";
            //this.Session["ReportHasHeader"] = (object)true;
            //this.Session["ReportFileName"] = (object)filename;
            //this.Session["ReportTextQualifier"] = (object)"\"";
            //this.Response.Redirect("~/reporting/report-export-csv.aspx");
            string filePathD = @"C:\linx-tablets\replen files\";

            filename = filename.Replace(".csv", ".xls");
            DataSet dsConsignmentStock = Common.runSQLDataset(query);

            PortalCommon.Excel.GenerateExcelSheetNew(dsConsignmentStock, "Download", filePathD + filename);
            
            FileInfo file = new FileInfo(filePathD + filename);
            DownloadFile(file);
        }
        protected void btnUpload3PLForecast_Click(object sender, EventArgs e)
        {
            if (fup3PLForecast.HasFile)
            {
                Common.runSQLNonQuery("delete from portalforecastupload_15_hive_3pl_tempload");
                Common.runSQLNonQuery("delete from productdataloader_PortalForecastingData_3pl_tempload");

                string filename = Path.GetFileNameWithoutExtension(fup3PLForecast.FileName) + "_" + Common.timestamp() + Path.GetExtension(fup3PLForecast.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fup3PLForecast.SaveAs(filePathLocale + filename);
                        filename = Path.GetFileName(PortalCommon.Excel.ConvertExcelToCsv(filePathLocale + filename));
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
                    string bulkInsert = string.Format(@"BULK INSERT portalforecastupload_15_hive_3pl_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from portalforecastupload_15_hive_3pl_tempload").ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from portalforecastupload_15_hive_3pl_tempload where (catno is null)").ToString()) > 0)
                        throw new Exception("Ensure CustomerSKU (column a) is populated for every line");

                    string loadTableName = "portalforecastupload_15_hive_3pl_tempload";
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
                    string sqlInsert = "insert into productdataloader_PortalForecastingData_3pl_tempload ";
                    sqlInsert += nulls;
                    sqlInsert += nullsNext;
                    sqlInsert += nullsNext1;


                    Common.runSQLNonQuery(sqlInsert);



                    string updateSQL = string.Format("exec sp_portal3plforecastupload '{0}','{1}'", fup3PLForecast.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the 3PL forecast has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report.');", true);
                }
            }
        }

        protected void btnUploadExertisHiveForecast_Click(object sender, EventArgs e)
        {

            string loadTableName = "portalforecastupload_15_exertishive_3pl_tempload";
            if (fupExertisHiveForecast.HasFile)
            {
                Common.runSQLNonQuery("delete from " + loadTableName);
                Common.runSQLNonQuery("delete from productdataloader_PortalForecastingData_exertishive_tempload");

                string filename = Path.GetFileNameWithoutExtension(fupExertisHiveForecast.FileName) + "_" + Common.timestamp() + Path.GetExtension(fupExertisHiveForecast.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fupExertisHiveForecast.SaveAs(filePathLocale + filename);
                        filename = Path.GetFileName(PortalCommon.Excel.ConvertExcelToCsv(filePathLocale + filename));
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

                    if (int.Parse(Common.runSQLScalar("select count(*) from " + loadTableName + " where (catno is null)").ToString()) > 0)
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
                    string sqlInsert = "insert into productdataloader_PortalForecastingData_exertishive_tempload ";
                    sqlInsert += nulls;
                    sqlInsert += nullsNext;
                    sqlInsert += nullsNext1;


                    Common.runSQLNonQuery(sqlInsert);



                    string updateSQL = string.Format("exec sp_portalexertishiveforecastupload '{0}','{1}'", fupExertisHiveForecast.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the Exertis Hive forecast has been updated');", true);
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

        protected void btnDownload3PLForecast_Click(object sender, EventArgs e)
        {
            string filename = "Hive_3PLForecast_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_forecastportalforecastdownload 5", filename);
        }
        protected void btnDownload3plForecastTemplate_Click(object sender, EventArgs e)
        {
            string filename = "Hive_3PLForecast_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_forecastportalforecastdownloadtemplate 5", filename);
        }

        protected void btnDownloadexertishiveForecast_Click(object sender, EventArgs e)
        {
            string filename = "ExertisHive_Forecast_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_forecastportalforecastdownload 6", filename);
        }
        protected void btnDownloadexertishiveForecastTemplate_Click(object sender, EventArgs e)
        {
            string filename = "Exertishive_Forecast_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_forecastportalforecastdownloadtemplate 6", filename);
        }

        protected void btnDownload3PLForecastHistoric_Click(object sender, EventArgs e)
        {
            string filename = "Hive_3PLForecast_Historic_Year_" + ddlHistoric3PLForecastYears.SelectedValue + "_" + Common.timestamp() + ".csv";
            runReport(string.Format("exec [sp_forecastportalforecasthistoricdownload] 5,{0}", ddlHistoric3PLForecastYears.SelectedValue), filename);
        }
        protected void btnDownloadExertishiveForecastHistoric_Click(object sender, EventArgs e)
        {
            string filename = "Exertishive_Forecast_Historic_Year_" + ddlHistoricExertisHiveForecastYears.SelectedValue + "_" + Common.timestamp() + ".csv";
            runReport(string.Format("exec [sp_forecastportalforecasthistoricdownload] 6,{0}", ddlHistoricExertisHiveForecastYears.SelectedValue), filename);
        }
       
    }
}