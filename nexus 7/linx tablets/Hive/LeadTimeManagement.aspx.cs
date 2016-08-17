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

namespace linx_tablets.Hive
{
    public partial class LeadTimeManagement : System.Web.UI.Page
    {
        public string filePathD = @"C:\linx-tablets\replen files\";
        protected void Page_Load(object sender, EventArgs e)
        {
            int counter = 0;
            int weeksRemain = 0;
            int weekNo = int.Parse(Common.runSQLScalar("select datepart(week,getdate())+8").ToString());
            string yearNo = Common.runSQLScalar("select datepart(year,getdate())").ToString();
            string yearNon = Common.runSQLScalar("select datepart(year,getdate())+1").ToString();
            string valuesSQL = "";
            string valuesSQLN = "";
            string nulls = "select catno," + yearNo.ToString()+",";
            string nullsNext = "select catno," + yearNon.ToString() + ",";
            for(int i1=1; i1 <weekNo;i1++)
            {
                nulls += "null,";
            }
            for (int i = 1; i <= 15; i++)
            {
                if (weekNo + i <= 53)
                {
                    valuesSQL += "Forecastweek" + (i).ToString()+",";
                }
                else
                {
                    weeksRemain = 16 - i;
                    break;
                }
            }
            if (weekNo + 15 < 52)
            {
                for (int loop = 0; loop <= 52 - (weekNo + 15); loop++)
                {
                    valuesSQL += "null,";
                }
            }

            if (weekNo + 15 > 52)
            {
                for (int iN = 16 - weeksRemain; iN <=15; iN++)
                {
                    valuesSQLN += "Forecastweek" + (iN).ToString() + ",";
                }

                for (int loop = 0; loop < (52- weeksRemain); loop++)
                {
                    valuesSQLN += "null,";
                }
            }
            valuesSQLN = valuesSQLN.Substring(0, valuesSQLN.Length - 1);
            valuesSQL = valuesSQL.Substring(0, valuesSQL.Length - 1);
            nulls += valuesSQL;
            nullsNext += valuesSQLN ;


            nulls += " from MSE_datetester";
            nullsNext += " from MSE_datetester";
            string sqlInsert = "insert into productdataloader_PortalForecastingData_tempload ";
            sqlInsert += nulls + " union ";
            sqlInsert += nullsNext;
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


            filename = filename.Replace(".csv", ".xls");
            DataSet dsConsignmentStock = Common.runSQLDataset(query);

            PortalCommon.Excel.GenerateExcelSheetNew(dsConsignmentStock, "Download", filePathD + filename);
            
            FileInfo file = new FileInfo(filePathD + filename);
            DownloadFile(file);
        }
        protected void btnUploadComponentLeadTimes_Click(object sender, EventArgs e)
        {
            if (fuLeadTimes.HasFile && Path.GetExtension(fuLeadTimes.FileName).ToLower() == ".xls")
            {
                Common.runSQLNonQuery("delete from productdataloader_portal_hivecomponentleadtime_tempload");
                string filename = Path.GetFileNameWithoutExtension(fuLeadTimes.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuLeadTimes.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuLeadTimes.SaveAs(filePathLocale + filename);
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
                    string bulkInsert = string.Format(@"BULK INSERT productdataloader_portal_hivecomponentleadtime_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_portal_hivecomponentleadtime_tempload").ToString()) == 0)
                        throw new Exception("Table empty");




                    string updateSQL = string.Format("exec sp_hivecomponentleadtimeupload '{0}','{1}'", fuLeadTimes.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, Component lead times have been updated');", true);
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

        protected void btnDownloadLeadTimes_Click(object sender, EventArgs e)
        {
            string filename = "Component_Lead_Times_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_portalhivecomponentleadtimesexisting", filename);
        }
        protected void btnDownloadLeadTimesTemplate_Click(object sender, EventArgs e)
        {
            string filename = "Component_Lead_Times_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_portalhivecomponentleadtimestemplate", filename);
        }
    }
}