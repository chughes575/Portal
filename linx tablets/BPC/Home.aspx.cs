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

namespace linx_tablets.BPC
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                bindBPCExports();
            }

        }
        protected void gvBPCExports_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CheckBox chkSalesExportEdit = (System.Web.UI.WebControls.CheckBox)e.Row.FindControl("chkSalesExportEdit");
                CheckBox chkStockExportEdit = (System.Web.UI.WebControls.CheckBox)e.Row.FindControl("chkStockExportEdit");
                int customerid = int.Parse(DataBinder.Eval(e.Row.DataItem, "CustomerID").ToString());
                if (customerid!= 2 && customerid!= 4 && customerid != 6 && (chkSalesExportEdit!= null) )
                {
                    chkSalesExportEdit.Enabled=false;
                    chkStockExportEdit.Enabled=false;
                }
            }

        }
        protected void bindBPCExports()
        {
            string exportsSQL = @"select * from vw_bpccustomers";
            gvBPCExports.DataSource = Common.runSQLDataset(exportsSQL);
            gvBPCExports.DataBind();
        }
        protected void gvBPCExports_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvBPCExports.EditIndex = -1;
            bindBPCExports();
        }

        protected void gvBPCExports_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvBPCExports.EditIndex = e.NewEditIndex;
            bindBPCExports();
        }

        protected void gvBPCExports_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow gridViewRow = this.gvBPCExports.Rows[e.RowIndex];
                string customerID = this.gvBPCExports.DataKeys[e.RowIndex].Value.ToString();
                TextBox txtBPC_Customer_Code = (TextBox)gridViewRow.FindControl("txtBPC_Customer_Code");
                TextBox txtCustomer_S = (TextBox)gridViewRow.FindControl("txtCustomer_S");
                TextBox txt_brand = (TextBox)gridViewRow.FindControl("txt_brand");
                TextBox txt_BusinessUnit = (TextBox)gridViewRow.FindControl("txt_BusinessUnit");
                CheckBox chkForecastExportEdit = (CheckBox)gridViewRow.FindControl("chkForecastExportEdit");
                CheckBox chkSalesExportEdit = (CheckBox)gridViewRow.FindControl("chkSalesExportEdit");
                CheckBox chkStockExportEdit = (CheckBox)gridViewRow.FindControl("chkStockExportEdit");

                string updateSQL = string.Format("update MSE_BPCExports set Customer_Code='{0}',Customer_s='{1}',ForecastExportEnabled={2},SalesExportEnabled={3},StockExportEnabled={4},brand='{6}',[business unit]='{7}'  where Customerid={5} ", 
                    txtBPC_Customer_Code.Text,
                    txtCustomer_S.Text,
                    chkForecastExportEdit.Checked ? 1 : 0,
                    chkSalesExportEdit.Checked ? 1 : 0,
                    chkStockExportEdit.Checked ? 1 : 0,
                    customerID, 
                    txt_brand.Text,
                    txt_BusinessUnit.Text
                    );


                Common.runSQLNonQuery(updateSQL);
                gvBPCExports.EditIndex = -1;
                bindBPCExports();
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

        protected void btnDownloadCurrentFile_Click(object sender, EventArgs e)
        {
            runReport("exec sp_bpcfilecontents", "BPC_Export_" + Common.timestamp() + ".csv");
        }
        protected void btnResubmitCurrentFile_Click(object sender, EventArgs e)
        {
            runReport("exec sp_bpcfilecontents", "BPC_Export_" + Common.timestamp() + ".csv");
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
    }
}