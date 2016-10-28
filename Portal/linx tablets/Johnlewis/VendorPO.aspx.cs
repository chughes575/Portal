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
namespace linx_tablets.Johnlewis
{
    public partial class VendorPO : System.Web.UI.Page
    {
        public int customerID = 6;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                bindSuppliers();
                bindLeadTimes();
                string leadtime = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsed' and CustomerID=" + customerID).ToString();
                lblWeeksUsed.Text = leadtime;
            }
        }
        protected void btnDLSuggestions_Command(object sender, CommandEventArgs e)
        {
            string commandParam = e.CommandArgument.ToString();
            string spParam = "null";

            switch (commandParam)
            {
                case "1":
                    if (ddlAllSuppliers.SelectedIndex != 0)
                        spParam = "'" + ddlAllSuppliers.SelectedValue + "'";
                    runReport("exec [sp_genericPoSuggestions] @customerid=" + customerID + ", @vendorname=" + spParam+", @all=0", "POSuggestions_All_" + Common.timestamp() + ".csv");
                    break;
                case "2":
                    if (ddlSuppliers.SelectedIndex != 0)
                        spParam = "'" + ddlSuppliers.SelectedValue + "'";
                    runReport("exec sp_genericPoSuggestions " + customerID + ", @vendorname=" + spParam + ", @all=1", "POSuggestions_" + Common.timestamp() + ".csv");
                    break;
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
        protected void bindSuppliers()
        {
            ddlAllSuppliers.DataTextField = "VendorName";
            ddlAllSuppliers.DataValueField = "VendorName";
            ddlSuppliers.DataTextField = "VendorName";
            ddlSuppliers.DataValueField = "VendorName";
            ddlAllSuppliers.DataSource = Common.runSQLDataset(@"SELECT distinct pv.VendorName from MSE_PortalVendors pv where customerid="+customerID+" order by  pv.VendorName");
            ddlSuppliers.DataSource = Common.runSQLDataset(@"SELECT distinct pv.VendorName from MSE_PortalVendors pv where customerid=" + customerID + " order by  pv.VendorName");

            ddlSuppliers.DataBind();
            ddlAllSuppliers.DataBind();
            ddlAllSuppliers.Items.Insert(0, new ListItem("-- All Vendors --"));
            ddlSuppliers.Items.Insert(0, new ListItem("-- All Vendors --"));

        }
        protected void ddlAllSuppliers_DataBound(object sender, EventArgs e)
        {
            ddlAllSuppliers.Items.Insert(0, new ListItem("-- All Vendors --"));
            ddlAllSuppliers.DataBind();
        }
        private void bindLeadTimes()
        {
            gvPOSupplierLeadTimes.DataSource = Common.runSQLDataset(@"select * from mse_PortaVendorLeadTimes where customerid="+customerID+" order by SupplierDesc");
            gvPOSupplierLeadTimes.DataBind();
        }
        protected void gvPOSupplierLeadTimes_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.gvPOSupplierLeadTimes.EditIndex = -1;
            this.bindLeadTimes();
        }

        protected void gvPOSupplierLeadTimes_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.gvPOSupplierLeadTimes.EditIndex = e.NewEditIndex;
            this.bindLeadTimes();
        }

        protected void ggvPOSupplierLeadTimes_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow gridViewRow = this.gvPOSupplierLeadTimes.Rows[e.RowIndex];
                string ID = this.gvPOSupplierLeadTimes.DataKeys[e.RowIndex].Value.ToString();
                TextBox txtLeadTime = (TextBox)gridViewRow.FindControl("txtLeadTime");

                Common.runSQLNonQuery(string.Format("update mse_PortaVendorLeadTimes set leadtime={1} where id={0} ", ID, txtLeadTime.Text));
                this.gvPOSupplierLeadTimes.EditIndex = -1;
                bindLeadTimes();
            }
            catch
            {
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
        protected void btnProductLeadTimeDownload_Click(object sender, EventArgs e)
        {
            string filename = "Product_LeadTime_" + Common.timestamp() + ".csv";
            runReport("sp_portalproductleadtimedownload " + customerID, filename);

        }
        protected void btnProductLeadTimeUpload_Click(object sender, EventArgs e)
        {
            if (fuProductLeadTime.HasFile)
            {
                string tempTableName = "portalproductleadtimes_dixons_tempload";
                Common.runSQLNonQuery("delete from portalproductleadtimes_dixons_tempload");
                string filename = Path.GetFileNameWithoutExtension(fuProductLeadTime.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuProductLeadTime.FileName);
                string filePath = @"\\10.16.72.129\company\";
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuProductLeadTime.SaveAs(filePathLocale + filename);

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
                    string bulkInsert = string.Format(@"BULK INSERT "+tempTableName+@" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName).ToString()) == 0)
                        throw new Exception();


                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempTableName + @" where leadtime is null or coalesce(CustSku,'')=''").ToString()) > 0)
                        throw new Exception();

                    string updateSQL = string.Format("exec sp_portaluploadproductleadtimes {0},'{1}','{2}'", customerID, amendedFileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the stock file has been imported');", true);

                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file');", true);
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

        protected void btnRemoveAllLeadTimes_Click(object sender, EventArgs e)
        {
            string confirmValue = Request.Form["confirm_value"];
            if (confirmValue == "Yes")
            {
                Common.runSQLNonQuery("delete from mse_PortaProductLeadTimes where customerid="+customerID);
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Lead times removed');", true);
            }
        }

        protected void btnDownloadVendorData_Click(object sender, EventArgs e)
        {
            string filename = "VendorDate_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_sdgVendorDataExisting " + customerID, filename);
        }
        protected void btnDownloadVndorTemplate_Click(object sender, EventArgs e)
        {
            string filename = "Vendor_Data_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_sp_sdgVendorDataTemplate", filename);
        }
        protected void btnUploadVendorData_Click(object sender, EventArgs e)
        {
            string tempLoadTable = "productdataloader_PortalVendors_jlp_tempload";
            if (fuVendorData.HasFile)
            {
                Common.runSQLNonQuery("delete from " + tempLoadTable);
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
                    string bulkInsert = string.Format(@"BULK INSERT " + tempLoadTable + @" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempLoadTable).ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempLoadTable + @" where (vendorname is null)").ToString()) > 0)
                        throw new Exception("Ensure VendorCode and VendorName (columns a and b) are populated for every line");

                    if (int.Parse(Common.runSQLScalar("select  count(*) from " + tempLoadTable + " where replace(vendorname,'\"','')+'-'+replace(Manufacturer,'\"','') not in (select vendorname+'-'+Manufacturer from MSE_PortalVendors)").ToString()) > 0)
                        throw new Exception("Unknown vendor found in file");

                    if (int.Parse(Common.runSQLScalar("select coalesce((select count(*) from " + tempLoadTable + " group by VendorName,manufacturer having count(ContactEmailAddress)>1),0)").ToString()) > 0)
                        throw new Exception("Ensure VendorName and manufacturer only appears once in the uploaded file");



                    string updateSQL = string.Format("exec sp_vendordataupload {0},'{1}','{2}'", customerID, fuVendorData.FileName, HttpContext.Current.User.Identity.Name.ToString());
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