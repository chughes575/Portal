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

namespace linx_tablets.SDG
{
    public partial class UserUploads : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                bindAccountManagers();


                string forecastWeek = Common.runSQLScalar("select datepart(week,getdate())").ToString();
                lblCurrentForecastWeek.Text = forecastWeek;
            }

            string historicForecastYearsExertisHiveSQL = @"select distinct a.forecastyear from (
select distinct forecastyear from MSE_PortalForecastingData where forecastyear<=datepart(year,getdate()) and customerid in (1)
union
select 2016 as forecastyear
) a ";
            ddlHistoricSDGForecastYears.DataSource = Common.runSQLDataset(historicForecastYearsExertisHiveSQL);
            ddlHistoricSDGForecastYears.DataBind();
        }
        protected void btnDownloadSDGForecastHistoric_Click(object sender, EventArgs e)
        {
            string filename = "SDG_Forecast_Historic_Year_" + ddlHistoricSDGForecastYears.SelectedValue + "_" + Common.timestamp() + ".csv";
            runReport(string.Format("exec [sp_forecastportalforecasthistoricdownload] 1,{0}", ddlHistoricSDGForecastYears.SelectedValue), filename);
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

        protected void btnDownloadProductRange_Click(object sender, EventArgs e)
        {
            string filename = "Product_Range_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_SDG_Product_RangeExisting", filename);
        }
        protected void btnDownloadProductRangeTemplate_Click(object sender, EventArgs e)
        {
            string filename = "Product_Range_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_SDG_Product_RangeTemplate", filename);
        }

        protected void btnUploadProductRange_Click(object sender, EventArgs e)
        {
            if (fuProductRange.HasFile)
            {
                Common.runSQLNonQuery("delete from productdataloader_SDGProductRange_tempload");
                string filename = Path.GetFileNameWithoutExtension(fuProductRange.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuProductRange.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuProductRange.SaveAs(filePathLocale + filename);

                    }
                    catch
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('File save failure');", true);
                    }
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

                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalUploadedFiles/", "exertissdg", "Exertissdg1");
                    try
                    {
                        ftpClient.uploadFile(filePathLocale + amendedFileName);
                    }
                    catch (Exception ex)
                    {
                    }
                    string newFilename = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\exertissdg\portalUploadedFiles\" + amendedFileName;
                    string bulkInsert = string.Format(@"BULK INSERT productdataloader_SDGProductRange_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_SDGProductRange_tempload").ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_SDGProductRange_tempload where (catno is null)").ToString()) > 0)
                        throw new Exception("Ensure Catno (columns a) is populated for every line");

                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_SDGProductRange_tempload where catno='tbc' and exertiscode is null").ToString()) > 0)
                        throw new Exception("Ensure Entries where tbc is used for the catno have the exertis part code populated");

                    if (int.Parse(Common.runSQLScalar(@"select coalesce((select count(*) from productdataloader_SDGProductRange_tempload 
where catno='tbc'
group by catno,ExertisCode having count(ProductLaunchDate)>1),0)

+ coalesce((select count(*) from productdataloader_SDGProductRange_tempload 
where catno<>'tbc'
group by catno having count(ProductLaunchDate)>1),0)").ToString()) > 0)
                        throw new Exception("Ensure Catno only appears one in the uploaded file unless tbc is used and a exertis part code is supplied");


                    
                    string updateSQL = string.Format("exec sp_sdgproductrangeupload {0},'{1}','{2}'", 1, fuProductRange.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, product range has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report');", true);
                }
            }
        }
        protected void btnDownloadForecast_Click(object sender, EventArgs e)
        {
            string filename = "Forecast_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_forecastportalforecastdownload 1", filename);
        }
        

        protected void btnUploadForecast_Click(object sender, EventArgs e)
        {
            string loadTableName = "portalforecastupload_2years_sdg_tempload";
            if (fupForecast.HasFile)
            {
                Common.runSQLNonQuery("delete from " + loadTableName);
                Common.runSQLNonQuery("delete from productdataloader_PortalForecastingData_SDG_tempload");
                Common.runSQLNonQuery("delete from portalforecastupload_sdg_tempload");
                string filename = Path.GetFileNameWithoutExtension(fupForecast.FileName) + "_" + Common.timestamp() + Path.GetExtension(fupForecast.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fupForecast.SaveAs(filePathLocale + filename);

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
                    string bulkInsert = string.Format(@"BULK INSERT "+loadTableName+@" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + loadTableName ).ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from " + loadTableName + @" where (catno is null)").ToString()) > 0)
                        throw new Exception("Ensure Catno (column a) is populated fort every line");

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
                    string sqlInsert = "insert into productdataloader_PortalForecastingData_SDG_tempload ";
                    sqlInsert += nulls;
                    sqlInsert += nullsNext;
                    sqlInsert += nullsNext1;


                    Common.runSQLNonQuery(sqlInsert);



                    string updateSQL = string.Format("exec sp_portalsdgforecastupload '{0}','{1}'", fupForecast.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    //string updateSQL = string.Format("exec sp_sdgforecastupload '{0}','{1}'", fupForecast.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the forecast has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report.');", true);
                }
            }
        }

        protected void btnDownloadVendorData_Click(object sender, EventArgs e)
        {
            string filename = "VendorDate_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_sdgVendorDataExisting", filename);
        }
        protected void btnDownloadVndorTemplate_Click(object sender, EventArgs e)
        {
            string filename = "Vendor_Data_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_sp_sdgVendorDataTemplate", filename);
        }
        protected void btnUploadVendorData_Click(object sender, EventArgs e)
        {
            if (fuVendorData.HasFile)
            {
                Common.runSQLNonQuery("delete from productdataloader_PortalVendors_tempload");
                string filename = Path.GetFileNameWithoutExtension(fuVendorData.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuVendorData.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuVendorData.SaveAs(filePathLocale + filename);

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
                    string bulkInsert = string.Format(@"BULK INSERT productdataloader_PortalVendors_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_PortalVendors_tempload").ToString()) == 0)
                        throw new Exception("Table empty");
                    
                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_PortalVendors_tempload where (vendorname is null)").ToString()) > 0)
                        throw new Exception("Ensure VendorCode and VendorName (columns a and b) are populated for every line");

                    if (int.Parse(Common.runSQLScalar("select  count(*) from productdataloader_PortalVendors_tempload where replace(vendorname,'\"','')+'-'+replace(Manufacturer,'\"','') not in (select vendorname+'-'+Manufacturer from MSE_PortalVendors)").ToString()) > 0)
                        throw new Exception("Unknown vendor found in file");

                    if (int.Parse(Common.runSQLScalar("select coalesce((select count(*) from productdataloader_PortalVendors_tempload group by VendorName,manufacturer having count(ContactEmailAddress)>1),0)").ToString()) > 0)
                        throw new Exception("Ensure VendorName and manufacturer only appears once in the uploaded file");

                    
                    
                    string updateSQL = string.Format("exec sp_vendordataupload {0},'{1}','{2}'", 1, fuVendorData.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, vendor data has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report');", true);
                }
            }
        }

















        protected void btnDownloadAccountManagerExisting_Click(object sender, EventArgs e)
        {
            string filename = "AccountManager_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_sdgAccountManagerDataExisting", filename);
        }
        protected void btnDownloadAccountManagerTemplate_Click(object sender, EventArgs e)
        {
            string filename = "AccountManager_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_sdgAccountManagerDataTemplate", filename);
        }



        protected void btrnUploadAccountManagerData_Click(object sender, EventArgs e)
        {
            if (fuAccountManager.HasFile)
            {
                Common.runSQLNonQuery("delete from productdataloader_AccountManagers_tempload");
                string filename = Path.GetFileNameWithoutExtension(fuAccountManager.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuAccountManager.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuAccountManager.SaveAs(filePathLocale + filename);

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
                    string bulkInsert = string.Format(@"BULK INSERT productdataloader_AccountManagers_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_PortalVendors_tempload").ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_AccountManagers_tempload where Business_AreaID not in (select Business_AreaID from MSE_PortalAccountManagers)").ToString()) > 0)
                        throw new Exception("Ensure business_areaid exists in the existing download");

                    
                    if (int.Parse(Common.runSQLScalar(@"select coalesce((select count(*) from 
productdataloader_AccountManagers_tempload 
group by Business_areaid having count(ExertisContactEmailAddress)>1),0)").ToString()) > 0)
                        throw new Exception("Ensure VendorName only appears once in the uploaded file");

                    string updateSQL = string.Format("exec sp_accountmanagerdataupload {0},'{1}','{2}'", 1, fuVendorData.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, Account Manager data has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report');", true);
                }
            }
        }
        private void bindAccountManagers()
        {
            string accountManagersSql = "select * from MSE_PortalAccountManagers order by business_area";
            gvPortalAccountManagers.DataSource = Common.runSQLDataset(accountManagersSql);
            gvPortalAccountManagers.DataBind();
        }
        protected void gvPortalAccountManagers_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.gvPortalAccountManagers.EditIndex = -1;
            this.bindAccountManagers();
        }

        protected void gvPortalAccountManagers_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.gvPortalAccountManagers.EditIndex = e.NewEditIndex;
            this.bindAccountManagers();
        }

        protected void gvPortalAccountManagers_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow gridViewRow = this.gvPortalAccountManagers.Rows[e.RowIndex];
                string businessAreaID = this.gvPortalAccountManagers.DataKeys[e.RowIndex].Value.ToString();
                TextBox txtAccountManager = (TextBox)gridViewRow.FindControl("txtAccountManager");
                TextBox txtContactEmailAddress = (TextBox)gridViewRow.FindControl("txtContactEmailAddress");
                TextBox txtContactTelephone = (TextBox)gridViewRow.FindControl("txtContactTelephone");

                Common.runSQLNonQuery(string.Format("update MSE_PortalAccountManagers set AccountManager='{1}',ContactEmailAddress='{2}',ContactTelephone='{3}' where business_AreaID='{0}' ", businessAreaID, txtAccountManager.Text, txtContactEmailAddress.Text, txtContactTelephone.Text));
                gvPortalAccountManagers.EditIndex = -1;
                this.bindAccountManagers();
            }
            catch
            {
            }
        }
    }
}