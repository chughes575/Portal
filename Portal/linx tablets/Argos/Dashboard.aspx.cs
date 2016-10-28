﻿using MSE_Common;
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


namespace linx_tablets.Argos
{
    public partial class ArgosDashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {

                gvArgosDashboard.DataSource = Common.runSQLDataset("exec [sp_portalargos_dashboard]");
                gvArgosDashboard.DataBind();
                if (gvArgosDashboard.Rows.Count > 0)
                {
                    //Attribute to show the Plus Minus Button.
                    gvArgosDashboard.HeaderRow.Cells[0].Attributes["data-class"] = "expand";

                    //Attribute to hide column in Phone.
                    gvArgosDashboard.HeaderRow.Cells[2].Attributes["data-hide"] = "phone";
                    gvArgosDashboard.HeaderRow.Cells[3].Attributes["data-hide"] = "phone";

                    //Adds THEAD and TBODY to GridView.
                    gvArgosDashboard.HeaderRow.TableSection = TableRowSection.TableHeader;
                }
            }


            string leadTimeComponentExertisHive = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsed' and CustomerID=4").ToString();
            lblWeeksUsed.Text = leadTimeComponentExertisHive;
        }
        protected void gvArgosDashboard_PreRender(object sender, EventArgs e)
        {



            if (gvArgosDashboard.Rows.Count > 0)
            {
                //This replaces <td> with <th> and adds the scope attribute
                gvArgosDashboard.UseAccessibleHeader = true;

                //This will add the <thead> and <tbody> elements
                gvArgosDashboard.HeaderRow.TableSection = TableRowSection.TableHeader;

                //This adds the <tfoot> element. 
                //Remove if you don't have a footer row
                gvArgosDashboard.FooterRow.TableSection = TableRowSection.TableFooter;
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



            if (gvArgosDashboard.Rows.Count > 0)
            {
                //This replaces <td> with <th> and adds the scope attribute
                gvArgosDashboard.UseAccessibleHeader = true;

                //This will add the <thead> and <tbody> elements
                gvArgosDashboard.HeaderRow.TableSection = TableRowSection.TableHeader;

                //This adds the <tfoot> element. 
                //Remove if you don't have a footer row
                gvArgosDashboard.FooterRow.TableSection = TableRowSection.TableFooter;
            }
        }
        protected void gvBundleSuggestions_DataBound(Object sender, EventArgs e)
        {
            if (gvArgosDashboard.Rows.Count > 0)
            {
                gvArgosDashboard.Enabled = true;
            }
            else
            {
                gvArgosDashboard.Enabled = false;
            }
            for (int i = 0; i <= gvArgosDashboard.Rows.Count - 1; i++)
            {

                String status = gvArgosDashboard.Rows[i].Cells[4].Text;
                String status3PL = gvArgosDashboard.Rows[i].Cells[5].Text;
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
                        gvArgosDashboard.Rows[i].Cells[4].BackColor = green;
                        break;
                    case "red":
                        gvArgosDashboard.Rows[i].Cells[4].BackColor = red;
                        break;
                    case "amber":
                        gvArgosDashboard.Rows[i].Cells[4].BackColor = amber;
                        break;

                }
                switch (status3PL.ToLower())
                {
                    case "green":
                        gvArgosDashboard.Rows[i].Cells[5].BackColor = green;
                        break;
                    case "red":
                        gvArgosDashboard.Rows[i].Cells[5].BackColor = red;
                        break;
                    case "amber":
                        gvArgosDashboard.Rows[i].Cells[5].BackColor = amber;
                        break;

                }
            }
        }

        protected void excelImgIcon_Click(object sender, ImageClickEventArgs e)
        {
            string filename = "Argos_Availability_" + Common.timestamp() + ".csv";
            runReport("[sp_portalargos_dashboard]", filename);
        }
    }
}