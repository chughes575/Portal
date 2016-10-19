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

namespace linx_tablets.Reporting
{
    public partial class InternalReporting : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void gvLatestInventory_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                string date = DataBinder.Eval(e.Row.DataItem, "DateCreated").ToString();
                DateTime dt = Convert.ToDateTime(date);
                TimeSpan ts = (DateTime.Now - dt);


                System.Web.UI.WebControls.Image theImage = (System.Web.UI.WebControls.Image)e.Row.FindControl("imgImportStatus");
                if (ts.TotalHours > 24)
                {
                    theImage.ImageUrl = "~/images/x.png";
                }
                else
                {
                    theImage.ImageUrl = "~/images/tick.png";
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
        protected void btnDownloadInvBalance_Command(object sender, CommandEventArgs e)
        {
            string str1 = e.CommandArgument.ToString();

            this.runReport("select * from vw_appleinventorybalancereport where InventoryReportID=" + str1, "Inventory_Balance_ReportID_" + str1 + "_" + Common.timestamp() + ".csv");

        }

        protected void btnAvailTrackDownload_Click(object sender, EventArgs e)
        {
            runReport(@"exec [sp_apple_availibillity_tracker_download]", "Availibility_Report_" + Common.timestamp() + "_.csv");
        }

        protected void btn_fcgroupedproduct_Click(object sender, EventArgs e)
        {
            runReport(@"select * from vw_fcgroupedproductreport", "Fc_Grouped_Product_Report_" + Common.timestamp() + "_.csv");
        }

        protected void btn_DonloadOverStockReport_Click(object sender, EventArgs e)
        {
            string fileName = "OverStock_Report_" + Common.timestamp() + ".csv";
            runReport(@"exec [sp_appleOverStockReport]", fileName);
        }

        protected void btnPoOverDueRed_Click(object sender, EventArgs e)
        {
            string fileName = "Overdue_PO_report_Red_"+Common.timestamp()+".csv";
            runReport("exec sp_apple_overduepo_red", fileName);
        }

        protected void btnPoOverDueAmber_Click(object sender, EventArgs e)
        {
            string fileName = "Overdue_PO_report_Amber_" + Common.timestamp() + ".csv";
            runReport("exec sp_apple_overduepo_amber", fileName);
        }
        
    }
}