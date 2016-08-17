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

namespace linx_tablets.John_Lewis
{
    public partial class UserUploads1 : System.Web.UI.Page
    {
        public int customerid = 6;
        public string customerFilename = "John_Lewis";
        protected void Page_Load(object sender, EventArgs e)
        {
            string forecastWeek = Common.runSQLScalar("select datepart(week,getdate())").ToString();
            lblCurrentForecastWeek.Text = forecastWeek;
            if (!Page.IsPostBack)
            {
                string forecastWeeksUsed = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsed' and CustomerID=" + customerid).ToString();
                ddlForecastAmountUsed.SelectedIndex = ddlForecastAmountUsed.Items.IndexOf(ddlForecastAmountUsed.Items.FindByValue(forecastWeeksUsed));
            }

            sqlDSPortal_JohnLewis_LastUpdates.DataBind();
            gvPortal_JohnLewis_LastUpdate.DataBind();
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
        protected void btnUploadProductRange_Click(object sender, EventArgs e)
        {
            string tempTableName = "productdataloader_portal_productrange_johnlewis_tempload";
            if (fuProductRange.HasFile)
            {
                Common.runSQLNonQuery("delete from " + tempTableName);
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
                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalUploadedFiles/", "exertissdg", "Exertissdg1");
                    try
                    {
                        ftpClient.uploadFile(filePathLocale + filename);
                    }
                    catch (Exception ex)
                    {
                    }
                    string newFilename = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\exertissdg\portalUploadedFiles\" + filename;
                    string bulkInsert = string.Format(@"BULK INSERT " + tempTableName + @" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName).ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar(@"select count(*) from " + tempTableName + @" 
 where (ExertisProductCode is null)").ToString()) > 0)
                        throw new Exception("Ensure ExertisProductCode is populated for every line");

                    string updateSQL = string.Format("exec sp_portal_johnlewis_productrange_upload '{0}','{1}',{2}", fuProductRange.FileName, HttpContext.Current.User.Identity.Name.ToString(), customerid);
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the Product range has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report.');", true);
                }
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

        protected void btnDownloadProdctRange_Click(object sender, EventArgs e)
        {
            string filename = customerFilename + "_Product_Range_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_portal_productrange_download_existing " + customerid, filename);
        }
        protected void btnDownloadProductRangeTemplate_Click(object sender, EventArgs e)
        {
            string filename = customerFilename + "_Product_Range_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_portal_productrange_download_template " + customerid, filename);
        }

        protected void btnDownloadForecast_Click(object sender, EventArgs e)
        {
            string filename = customerFilename + "_Forecast_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_forecastportalforecastdownload " + customerid, filename);
        }
        protected void btnDownloadForecastTemplate_Click(object sender, EventArgs e)
        {
            string filename = customerFilename + "_Forecast_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_forecastportalforecastdownloadtemplate " + customerid, filename);
        }

        protected void gvBundleProducts_RowDataBound(object source, GridViewRowEventArgs e)
        {

            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                GridViewRow row = e.Row;
                GridView gv = new GridView();
                gv = (GridView)row.FindControl("gvBundleProductsComponents");
                string bundleID = ((DataRowView)e.Row.DataItem)["bundleid"].ToString();
                gv.DataSource = Common.runSQLDataset(string.Format(@" select bcp.ProductCode,Product_Description,qty as ComponentQty from MSE_PortalHiveBundleComponentProduct bcp left outer join mse_oracleproducts op on op.product_code=bcp.ProductCode
 where BundleID=50 order by Product_Description", bundleID));
                gv.DataBind();
            }
        }


        protected void btnUploadForecast_Click(object sender, EventArgs e)
        {
            string tempTableName = "portalforecastupload_portal_johnlewis_tempload";
            if (fupForecast.HasFile)
            {
                Common.runSQLNonQuery("delete from " + tempTableName);
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
                    string bulkInsert = string.Format(@"BULK INSERT " + tempTableName + @" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName).ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName + " where (customersku is null)").ToString()) > 0)
                        throw new Exception("Ensure CustomerSKU (column a) is populated for every line");

                    string updateSQL = string.Format("exec sp_portalforecasttemploaforecastupload '{0}','{1}',{2}", fupForecast.FileName, HttpContext.Current.User.Identity.Name.ToString(), customerid);
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the forecast has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report.');", true);
                }
            }
        }
        protected void btnUpdateForecastWeeksUsed_Click(object sender, EventArgs e)
        {
            int leadTime = int.Parse(ddlForecastAmountUsed.SelectedValue.ToString());
            Common.runSQLNonQuery("update PortalConfig set configvalue='" + leadTime + "' where ConfigKey='ForecastWeeksUsed' and CustomerID=" + customerid);
            Common.runSQLNonQuery(string.Format("update mse_portalforecastreportmanagement set lastfiledate=getdate(),Username='{0}' where reportid=32", HttpContext.Current.User.Identity.Name.ToString()));
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Forecast Weeks Used Update Successful');", true);
        }



        protected void btnProductLeadTimeUpload_Click(object sender, EventArgs e)
        {
            string tempTableName = "productdataloader_portalposuppliers_johnlewis_tempload";
            if (fuVendorLeadTime.HasFile)
            {
                Common.runSQLNonQuery("delete from " + tempTableName);
                string filename = Path.GetFileNameWithoutExtension(fuVendorLeadTime.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuVendorLeadTime.FileName);
                string filePath = @"\\10.16.72.129\company\";
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuVendorLeadTime.SaveAs(filePathLocale + filename);

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
                    string bulkInsert = string.Format(@"BULK INSERT " + tempTableName + @" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName).ToString()) == 0)
                        throw new Exception();


                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName + " where leadtime is null or supplierid is null").ToString()) > 0)
                        throw new Exception();
                    if (int.Parse(Common.runSQLScalar(@"select count(*) from (
select count(*) as cnt from productdataloader_portalposuppliers_tempload group by supplierid having count(leadtime)>1) a").ToString()) > 0)
                        throw new Exception("Multiple instances of the same supplierid in file");
                    if (int.Parse(Common.runSQLScalar("select count(*) from "+tempTableName+" where supplierid not in (select supplierid from mse_portalposuppliers where customerid="+customerid+")").ToString()) > 0)
                        throw new Exception("Unknown SupplierID in file");


                    string updateSQL = string.Format("exec sp_portalvendorleadtime_johnlewis_update '{0}','{1}',{2}", amendedFileName, HttpContext.Current.User.Identity.Name.ToString(), customerid);
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the vendor lead time file has been imported');", true);

                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file');", true);
                }
            }
        }
        protected void btnVendorLeadTimeDownload_Click(object sender, EventArgs e)
        {
            string filename = customerFilename + "_Product_LeadTime_" + Common.timestamp() + ".csv";
            runReport("sp_portalvendorleadtime_download " + customerid, filename);

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
        protected void btnDownloadVendorData_Click(object sender, EventArgs e)
        {
            string filename = customerFilename + "_VendorData_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_PortalVendorDataExisting " + customerid, filename);
        }
        protected void btnDownloadVndorTemplate_Click(object sender, EventArgs e)
        {
            string filename = "Vendor_Data_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_sp_sdgVendorDataTemplate", filename);
        }
        protected void btnUploadVendorData_Click(object sender, EventArgs e)
        {
            string tempTableName = "productdataloader_PortalVendors_johnlewis_tempload";
            if (fuVendorData.HasFile)
            {
                Common.runSQLNonQuery("delete from " + tempTableName);
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
                    string bulkInsert = string.Format(@"BULK INSERT " + tempTableName + @" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName).ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName + " where (vendorname is null)").ToString()) > 0)
                        throw new Exception("Ensure VendorCode and VendorName (columns a and b) are populated for every line");

                    if (int.Parse(Common.runSQLScalar("select  count(*) from " + tempTableName + " where replace(vendorname,'\"','')+'-'+replace(Manufacturer,'\"','') not in (select vendorname+'-'+Manufacturer from MSE_PortalVendors)").ToString()) > 0)
                        throw new Exception("Unknown vendor found in file");

                    if (int.Parse(Common.runSQLScalar("select coalesce((select count(*) from " + tempTableName + " group by VendorName having count(ContactEmailAddress)>1),0)").ToString()) > 0)
                        throw new Exception("Ensure VendorName only appears once in the uploaded file");



                    string updateSQL = string.Format("exec sp_vendordataPortal_johnlewis_upload {0},'{1}','{2}'", customerid, fuVendorData.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, vendor data has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report');", true);
                }
            }
        }
    }
}