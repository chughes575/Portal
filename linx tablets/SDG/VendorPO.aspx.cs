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
    public partial class VendorPO : System.Web.UI.Page
    {
        public int customerID = 1;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                bindSuppliers();
                bindLeadTimes();
              string leadtime =Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsed' and CustomerID=1").ToString();
              ddlForecastAmountUsed.SelectedIndex = ddlForecastAmountUsed.Items.IndexOf(ddlForecastAmountUsed.Items.FindByValue(leadtime));

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
                    runReport("exec [sp_sdgPoSuggestions] @customerid=" + customerID + ", @vendorname=" + spParam + ", @all=0", "POSuggestions_All_" + Common.timestamp() + ".csv");
                    break;
                case "2":
                    if (ddlSuppliers.SelectedIndex != 0)
                        spParam = "'" + ddlSuppliers.SelectedValue + "'";
                    runReport("exec sp_sdgPoSuggestions " + customerID + ", @vendorname=" + spParam + ", @all=1", "POSuggestions_" + Common.timestamp() + ".csv");
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
            ddlAllSuppliers.DataSource = Common.runSQLDataset(@"SELECT distinct pv.VendorName from MSE_PortalVendors pv order by  pv.VendorName");
            ddlSuppliers.DataSource = Common.runSQLDataset(@"SELECT distinct pv.VendorName from MSE_PortalVendors pv order by  pv.VendorName");

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
            gvPOSupplierLeadTimes.DataSource = Common.runSQLDataset(@"select * from mse_PortaVendorLeadTimes  where customerid=1 order by SupplierDesc");
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
            runReport("sp_portalproductleadtimedownload " + 1, filename);
        }
        protected void btnProductLeadTimeUpload_Click(object sender, EventArgs e)
        {
            if (fuProductLeadTime.HasFile)
            {
                Common.runSQLNonQuery("delete from portalproductleadtimes_sdg_tempload");
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
                    string bulkInsert = string.Format(@"BULK INSERT portalproductleadtimes_sdg_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from portalproductleadtimes_sdg_tempload").ToString()) == 0)
                        throw new Exception();


                    if (int.Parse(Common.runSQLScalar("select count(*) from portalproductleadtimes_sdg_tempload where leadtime is null or coalesce(CustSku,'')=''").ToString()) > 0)
                        throw new Exception();

                    string updateSQL = string.Format("exec sp_portaluploadproductleadtimes {0},'{1}','{2}'", 1, amendedFileName, HttpContext.Current.User.Identity.Name.ToString());
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
                Common.runSQLNonQuery("delete from mse_PortaProductLeadTimes where customerid=1");
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Lead times removed');", true);
            }
        }
        protected void btnUpdateForecastWeeksUsed_Click(object sender, EventArgs e)
        {
           int leadTime = int.Parse(ddlForecastAmountUsed.SelectedValue.ToString());
           Common.runSQLNonQuery("update PortalConfig set configvalue='" + leadTime + "' where ConfigKey='ForecastWeeksUsed' and CustomerID=1");
           ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Lead Time Update Successful');", true);
        }


        
    }
}