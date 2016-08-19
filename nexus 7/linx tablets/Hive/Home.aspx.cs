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
    public partial class UserUploads : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


            gvOrdersSummary.DataSource = Common.runSQLDataset("exec [sp_bghome]");
            gvOrdersSummary.DataBind();
            if (!Page.IsPostBack)
            {
                string stockLevelSetup = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='StockLevelsMethod' and CustomerID=5").ToString();
                rbtnlstStockSetup.SelectedIndex = rbtnlstStockSetup.Items.IndexOf(rbtnlstStockSetup.Items.FindByValue(stockLevelSetup));
            }
            else
            {
                sqlDSKewillProductStockStatusReport.DataBind();
                gvKewillProductStockStatusLastUpdate.DataBind();
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

        protected void btnUpdateStockLevels_Click(object sender, EventArgs e)
        {
            string updateVal = rbtnlstStockSetup.SelectedValue.ToString();

            string updateSQL = string.Format("update portalconfig set configvalue='{0}' where ConfigKey='StockLevelsMethod'", updateVal);
            Common.runSQLNonQuery(updateSQL);
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Stock Level Setup Updated.');", true);
        }

        protected void btnDownloadRollingDocument_Click(object sender, EventArgs e)
        {

            string filename = "BritishGas_Orders_Rolling_Report_" + Common.timestamp() + ".csv";
            runReport("[sp_HiveBritishGasOrders]", filename);
        }
        protected void btnDownloadRollingDocument_outstanding_Click(object sender, EventArgs e)
        {

            string filename = "BritishGas_Orders_Rolling_Report_" + Common.timestamp() + ".csv";
            runReport("[sp_HiveBritishGasOrders] 1", filename);
        }
    }
}