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
    public partial class ExertisStockPO : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


            gvBundleSuggestions.DataSource = Common.runSQLDataset("exec [sp_portalhive_pobundlesuggestions_exertis] 0");
            gvBundleSuggestions.DataBind();

            string leadTimeComponentExertisHive = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsedBundlesExertisHive' and CustomerID=6").ToString();
            lblWeeksUsed.Text = leadTimeComponentExertisHive;
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
        

        protected void gvBundleSuggestions_PreRender(object sender, EventArgs e)
        {



            if (gvBundleSuggestions.Rows.Count > 0)
            {
                //This replaces <td> with <th> and adds the scope attribute
                gvBundleSuggestions.UseAccessibleHeader = true;

                //This will add the <thead> and <tbody> elements
                gvBundleSuggestions.HeaderRow.TableSection = TableRowSection.TableHeader;

                //This adds the <tfoot> element. 
                //Remove if you don't have a footer row
                gvBundleSuggestions.FooterRow.TableSection = TableRowSection.TableFooter;
            }
        }
        protected void gvBundleSuggestions_DataBound(Object sender, EventArgs e)
        {
            if (gvBundleSuggestions.Rows.Count > 0)
            {
                gvBundleSuggestions.Enabled = true;
            }
            else
            {
                gvBundleSuggestions.Enabled = false;
            }
            for (int i = 0; i <= gvBundleSuggestions.Rows.Count - 1; i++)
            {

                String status = gvBundleSuggestions.Rows[i].Cells[4].Text;
                String status3PL = gvBundleSuggestions.Rows[i].Cells[5].Text;
                //String status = gvCustomerViewResults.Rows[i].Cells[0].Text;
                const string greenHex = "#00cc66";
                const string redHex = "#ff0000";
                const string amberHex = "#ffcc00";
                Color green = System.Drawing.ColorTranslator.FromHtml(greenHex);
                Color red = System.Drawing.ColorTranslator.FromHtml(redHex);
                Color amber = System.Drawing.ColorTranslator.FromHtml(amberHex);
                switch (status.ToLower())
                {
                    case "green":
                        gvBundleSuggestions.Rows[i].Cells[4].BackColor = green;
                        break;
                    case "red":
                        gvBundleSuggestions.Rows[i].Cells[4].BackColor = red;
                        break;
                    case "amber":
                        gvBundleSuggestions.Rows[i].Cells[4].BackColor = amber;
                        break;

                }
                switch (status3PL.ToLower())
                {
                    case "green":
                        gvBundleSuggestions.Rows[i].Cells[5].BackColor = green;
                        break;
                    case "red":
                        gvBundleSuggestions.Rows[i].Cells[5].BackColor = red;
                        break;
                    case "amber":
                        gvBundleSuggestions.Rows[i].Cells[5].BackColor = amber;
                        break;

                }
            }
        }

        protected void excelImgIcon_Click(object sender, ImageClickEventArgs e)
        {
            string filename = "Exertis_Stock_Bundle_Availability_" + Common.timestamp() + ".csv";
            runReport("[sp_portalhive_pobundlesuggestions_exertis_downloaddetail]", filename);
        }
        protected void excelConsolidatedImgIcon_Click(object sender, ImageClickEventArgs e)
        {
            string filename = "Exertis_Stock_Bundle_Consolidated_Availability_" + Common.timestamp() + ".csv";
            runReport("[sp_portalhive_pobundlesuggestions_exertis_MFRPARTNO] 0", filename);
        }
    }
}