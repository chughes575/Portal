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
    public partial class EmailReporting : System.Web.UI.Page
    {
        public int customerID = 1;
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void gvEmailReports_OnRowDataBound(object source, GridViewRowEventArgs e)
        {

            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                GridViewRow row = e.Row;
                GridView gvReportVendors = new GridView();
                gvReportVendors = (GridView)row.FindControl("gvReportVendors");

                string reportID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
                DataSet dsReportVendors = Common.runSQLDataset(string.Format(@"exec [sp_portaldownloademailreportvendors_display] {0},{1}", reportID, customerID));

                gvReportVendors.DataSource = dsReportVendors;
                gvReportVendors.DataBind();
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
        protected void gvEmailReports_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string commandName = e.CommandName.ToString();
            if (commandName == "downloadreportvendors")
            {
                string filename = string.Format("Email_Vendors_{1}_{0}.csv", (object)Common.timestamp(), e.CommandArgument.ToString());
                this.runReport(string.Format("exec sp_portaldownloademailreportvendors {0},{1} ", e.CommandArgument.ToString(),customerID), filename);
            }
        }
    }
}