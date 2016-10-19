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
        protected void bindBPCExports()
        {
            string exportsSQL = @"select pc.CustomerID,pc.CustomerName,pc.CustomerCode as Oracle_Customer_Code, bpc.Customer_Code as BPC_Customer_Code
,bpc.Customer_S, bpc.ForecastExportEnabled,bpc.SalesExportEnabled,bpc.StockExportEnabled,
case 
when pc.CustomerID = 1 then fcr1.lastfiledate
when pc.CustomerID = 2 then fcr2.lastfiledate
when pc.CustomerID = 4 then fcr4.lastfiledate
when pc.CustomerID = 5 then fcr5.lastfiledate
when pc.CustomerID = 6 then fcr6.lastfiledate
else getdate()
end as [Last Forecast Date],
case 
when pc.CustomerID = 1 then fcrepos1.lastfiledate
when pc.CustomerID = 2 then fcrepos2.lastfiledate
when pc.CustomerID = 4 then fcrepos4.lastfiledate
when pc.CustomerID = 5 then fcrepos5.lastfiledate
when pc.CustomerID = 6 then fcrepos6.lastfiledate
else getdate()
end as [LAst EPOS Date],
case 
when pc.CustomerID = 1 then fcrinv1.lastfiledate
when pc.CustomerID = 2 then fcrinv2.lastfiledate
when pc.CustomerID = 4 then fcrinv4.lastfiledate
when pc.CustomerID = 5 then fcrinv5.lastfiledate
when pc.CustomerID = 6 then fcrinv6.lastfiledate
else getdate()
end as [Last Cust Stock Date]
 from MSE_BPCExports bpc 
inner join mse_PortalCustomers pc on pc.CustomerID=bpc.CustomerID
left outer join (select customerid, ReportType,max(ImportDate) as ReportDate from MSE_PortalSalesEposFiles
group by customerid, ReportType) as lastimportsales on lastimportsales.customerid=bpc.customerid and lastimportsales.ReportType='sales'
left outer join (select customerid, ReportType,max(ImportDate) as ReportDate from MSE_PortalSalesEposFiles
group by customerid, ReportType) as lastimportstock on lastimportstock.customerid=bpc.customerid and lastimportstock.ReportType='stock'
left outer join (select customerid, ReportType,max(ImportDate) as ReportDate from MSE_PortalSalesEposFiles
group by customerid, ReportType) as lastimportepos on lastimportepos.customerid=bpc.customerid and lastimportepos.ReportType='epos'
left outer join mse_portalforecastreportmanagement fcr1 on fcr1.reportid=7
left outer join mse_portalforecastreportmanagement fcr2 on fcr2.reportid=32
left outer join mse_portalforecastreportmanagement fcr4 on fcr4.reportid=54
left outer join mse_portalforecastreportmanagement fcr5 on fcr5.reportid=23
left outer join mse_portalforecastreportmanagement fcr6 on fcr6.reportid=34

left outer join mse_portalforecastreportmanagement fcrepos1 on fcrepos1.reportid=55   
left outer join mse_portalforecastreportmanagement fcrepos2 on fcrepos2.reportid=56
left outer join mse_portalforecastreportmanagement fcrepos4 on fcrepos4.reportid=57
left outer join mse_portalforecastreportmanagement fcrepos5 on fcrepos5.reportid=40
left outer join mse_portalforecastreportmanagement fcrepos6 on fcrepos6.reportid=52

left outer join mse_portalforecastreportmanagement fcrinv1 on fcrinv1.reportid=58   
left outer join mse_portalforecastreportmanagement fcrinv2 on fcrinv2.reportid=59
left outer join mse_portalforecastreportmanagement fcrinv4 on fcrinv4.reportid=60
left outer join mse_portalforecastreportmanagement fcrinv5 on fcrinv5.reportid=28
left outer join mse_portalforecastreportmanagement fcrinv6 on fcrinv6.reportid=53";
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
                CheckBox chkForecastExportEdit = (CheckBox)gridViewRow.FindControl("chkForecastExportEdit");
                CheckBox chkSalesExportEdit = (CheckBox)gridViewRow.FindControl("chkSalesExportEdit");
                CheckBox chkStockExportEdit = (CheckBox)gridViewRow.FindControl("chkStockExportEdit");

                string updateSQL= string.Format("update MSE_BPCExports set Customer_Code='{0}',Customer_s='{1}',ForecastExportEnabled={2},SalesExportEnabled={3},StockExportEnabled={4} where Customerid={5} ", 
                    txtBPC_Customer_Code.Text,
                    txtCustomer_S.Text,
                    chkForecastExportEdit.Checked ? 1 : 0,
                    chkSalesExportEdit.Checked ? 1 : 0,
                    chkStockExportEdit.Checked ? 1 : 0,
                    customerID
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
    }
}