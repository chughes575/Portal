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


namespace linx_tablets.Johnlewis
{
    public partial class JohnLewisDashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                
                gvJohnLewisDashboard.DataSource = Common.runSQLDataset("exec [sp_portaljlp_dashboard]");
                gvJohnLewisDashboard.DataBind();
                if (gvJohnLewisDashboard.Rows.Count > 0)
                {

                    ////Attribute to show the Plus Minus Button.
                    // gvJohnLewisDashboard.HeaderRow.Cells[0].Attributes["data-class"] = "expand";

                    ////Attribute to hide column in Phone.
                    //gvJohnLewisDashboard.HeaderRow.Cells[2].Attributes["data-hide"] = "phone";
                    //gvJohnLewisDashboard.HeaderRow.Cells[3].Attributes["data-hide"] = "phone";

                    ////Adds THEAD and TBODY to GridView.
                    //gvJohnLewisDashboard.HeaderRow.TableSection = TableRowSection.TableHeader;
                }
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + gvJohnLewisDashboard.ClientID + "', 600, 1286 , " + Common.getConfigValue("SDGNUM") + " ,false); </script>", false);
            }


            string leadTimeComponentExertisHive = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsed' and CustomerID=6").ToString();
            lblWeeksUsed.Text = leadTimeComponentExertisHive;
        }
        protected void gvArgosDashboard_PreRender(object sender, EventArgs e)
        {



            if (gvJohnLewisDashboard.Rows.Count > 0)
            {
                //This replaces <td> with <th> and adds the scope attribute
                gvJohnLewisDashboard.UseAccessibleHeader = true;

                //This will add the <thead> and <tbody> elements
                gvJohnLewisDashboard.HeaderRow.TableSection = TableRowSection.TableHeader;

                //This adds the <tfoot> element. 
                //Remove if you don't have a footer row
                gvJohnLewisDashboard.FooterRow.TableSection = TableRowSection.TableFooter;
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



            if (gvJohnLewisDashboard.Rows.Count > 0)
            {
                //This replaces <td> with <th> and adds the scope attribute
                gvJohnLewisDashboard.UseAccessibleHeader = true;

                //This will add the <thead> and <tbody> elements
                gvJohnLewisDashboard.HeaderRow.TableSection = TableRowSection.TableHeader;

                //This adds the <tfoot> element. 
                //Remove if you don't have a footer row
                gvJohnLewisDashboard.FooterRow.TableSection = TableRowSection.TableFooter;
            }
        }
        protected void gvBundleSuggestions_DataBound(Object sender, EventArgs e)
        {
            int columnIndex = int.Parse(Common.runSQLScalar("select configvalue from config where configkey='jlpsafetyindex'").ToString());
            if (gvJohnLewisDashboard.Rows.Count > 0)
            {
                gvJohnLewisDashboard.Enabled = true;
            }
            else
            {
                gvJohnLewisDashboard.Enabled = false;
              
            }
            for (int i = 0; i <= gvJohnLewisDashboard.Rows.Count - 1; i++)
            {

                
                String status = gvJohnLewisDashboard.Rows[i].Cells[columnIndex].Text;
                //String status = gvCustomerViewResults.Rows[i].Cells[0].Text;
                const string greenHex = "#00cc66";
                const string redHex = "#ff0000";
                const string amberHex = "#ffcc00";
                const string greyHex = "#b7aeae";
                Color green = System.Drawing.ColorTranslator.FromHtml(greenHex);
                Color red = System.Drawing.ColorTranslator.FromHtml(redHex);
                Color amber = System.Drawing.ColorTranslator.FromHtml(amberHex);
                Color grey = System.Drawing.ColorTranslator.FromHtml(greyHex);
                
                switch (status.ToLower())
                {
                    case "green":
                        gvJohnLewisDashboard.Rows[i].Cells[columnIndex].BackColor = green;
                        break;
                    case "red":
                        gvJohnLewisDashboard.Rows[i].Cells[columnIndex].BackColor = red;
                        break;
                    case "amber":
                        gvJohnLewisDashboard.Rows[i].Cells[columnIndex].BackColor = amber;
                        break;
                    case "grey":
                        gvJohnLewisDashboard.Rows[i].Cells[columnIndex].BackColor = grey;
                        break;

                }
            }
        }

        protected void excelImgIcon_Click(object sender, ImageClickEventArgs e)
        {
            string filename = "Jlp_Availability_" + Common.timestamp() + ".csv";
            runReport("[sp_portaljlp_dashboard]", filename);
        }
    }
}