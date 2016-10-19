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
    public partial class PreAdvise : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

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
        protected void gvPreAdvise_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string[] paramArray = e.CommandArgument.ToString().Split('|');
            string localeID = paramArray[0];
            string localeDate = paramArray[1];
            string reportSQL = string.Format("[sp_applepreadvisereport] {0},'{1}'", localeID, localeDate);
            string reportFilename = "Pre_AdviseReport_" + localeDate.Replace("-", "") + ".csv";
            runReport(reportSQL, reportFilename);

        }
    }
}