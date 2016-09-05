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
    public partial class VendorPO : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string leadTimeComponentExertisHive = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsed3pl' and CustomerID=5").ToString();
            lblWeeksUsed.Text = leadTimeComponentExertisHive;

            //gvBundleSuggestions.DataSource = Common.runSQLDataset("exec [ sp_portalhive_pobundlesuggestions] 0");
            //gvBundleSuggestions.DataBind();
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + gvBundleSuggestions.ClientID + "', 600, 1286 , " + Common.getConfigValue("SDGNUM") + " ,false); </script>", false);
            if (!Page.IsPostBack)
            {
                
                bindSearchResults();
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
        protected void btnUkReplenFile_Command(object sender, CommandEventArgs e)
        {
            string str1 = e.CommandArgument.ToString().Substring(2, e.CommandArgument.ToString().Length-2);
            string type = e.CommandArgument.ToString().Substring(0, 1);
            if (type == "c")
            {
                if (str1 == "sug")
                {
                    this.runReport("exec [sp_portalhive_pocomponentsuggestions] 1", "POSuggestions_Hive_Components_" + Common.timestamp() + ".csv");
                }
                else if (str1 == "sugall")
                {
                    this.runReport("exec [sp_portalhive_pocomponentsuggestions] 0", "POSuggestions_Hive_Components_All_" + Common.timestamp() + ".csv");
                }
            }
            else if (type == "b")
            {
                if (str1 == "sug")
                {
                    this.runReport("exec [sp_portalhive_pobundlesuggestions] 1", "POSuggestions_Hive_Bundles_" + Common.timestamp() + ".csv");
                }
                else if (str1 == "sugall")
                {
                    this.runReport("exec [sp_portalhive_pobundlesuggestions] 0", "POSuggestions_Hive_Bundles_All_" + Common.timestamp() + ".csv");
                }
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
            }
        }
        protected void excelImgIcon_Click(object sender, ImageClickEventArgs e)
        {
            string filename = "Hive_Stock_Bundle_Availability_" + Common.timestamp() + ".csv";
            runReport(returnQuery()+",@download=1", filename);
        }
        protected string returnQuery()
        {
            string baseQuery = "EXEC [ sp_portalhive_pobundlesuggestions] @all=0,";

           

            if (ddlStockStatusGV_FilterSafetyRating.SelectedIndex != 0)
            {
                baseQuery += string.Format(" @safetyrating='{0}',", ddlStockStatusGV_FilterSafetyRating.SelectedValue);
            }

            if (ddlStockStatusGV_FilterExertisStock.SelectedIndex != 0)
            {
                switch (ddlStockStatusGV_FilterExertisStock.SelectedValue)
                {

                    case "Zero":
                        baseQuery += " @exertisstock=1,";
                        break;
                    case "Not Zero":
                        baseQuery += " @exertisstock=0,";
                        break;
                }
            }

            if (ddlStockStatusGV_FilterExertisPO.SelectedIndex != 0)
            {
                switch (ddlStockStatusGV_FilterExertisPO.SelectedValue)
                {

                    case "Has Pos":
                        baseQuery += " @exertispo=1,";
                        break;
                    case "No Pos":
                        baseQuery += " @exertispo=0,";
                        break;
                }
            }

            if (ddlStockStatusGV_FilterCustomerOrders.SelectedIndex != 0)
            {
                switch (ddlStockStatusGV_FilterCustomerOrders.SelectedValue)
                {

                    case "Backordered":
                        baseQuery += " @backorders=1,";
                        break;
                    case "No Backorders":
                        baseQuery += " @backorders=0,";
                        break;
                }
            }
            if (ddlStockStatusGV_FilterCustomerAllocatedOrders.SelectedIndex != 0)
            {
                switch (ddlStockStatusGV_FilterCustomerAllocatedOrders.SelectedValue)
                {

                    case "Allocated":
                        baseQuery += " @allocatedorders=1,";
                        break;
                    case "No Allocated":
                        baseQuery += " @allocatedorders=0,";
                        break;
                }
            }


            

            baseQuery = baseQuery.Substring(0, baseQuery.Length - 1);
            return baseQuery;
        }
        protected void btnFilterReport_Click(object sender, EventArgs e)
        {
            bindSearchResults();

        }
        protected void bindSearchResults()
        {
            string sql = returnQuery();
            DataSet ds = Common.runSQLDataset(sql);
            gvBundleSuggestions.DataSource = ds.Tables[0];
            gvBundleSuggestions.DataBind();
            
        }
    }
}