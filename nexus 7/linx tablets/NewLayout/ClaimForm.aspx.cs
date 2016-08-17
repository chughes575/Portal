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
using System.Data.SqlClient;

namespace linx_tablets.NewLayout
{
    public partial class ClaimForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                bindAccountManagerDDL();
                bindBusinessUnitDDL();
                bindManufacturerDDL();
                bindProductTypeDDL();
            }
        }

        protected void gvCustomerViewResults_DataBound(Object sender, EventArgs e)
        {
            
            if (gvCustomerViewResults.Rows.Count > 0)
            {
                btnDownloadFilteredResults.Enabled = true;
            }
            else
            {
                btnDownloadFilteredResults.Enabled = false;
            }
            for (int i = 0; i <= gvCustomerViewResults.Rows.Count - 1; i++)
            {

                String status = gvCustomerViewResults.Rows[i].Cells[2].Text;
                const string greenHex = "#00cc66";
                const string redHex = "#ff0000";
                const string amberHex = "#ffcc00";
                Color green = System.Drawing.ColorTranslator.FromHtml(greenHex);
                Color red = System.Drawing.ColorTranslator.FromHtml(redHex);
                Color amber = System.Drawing.ColorTranslator.FromHtml(amberHex);
                switch (status)
                {
                    case "green":
                        gvCustomerViewResults.Rows[i].Cells[2].BackColor = green;
                        break;
                    case "red":
                        gvCustomerViewResults.Rows[i].Cells[2].BackColor = red;
                        break;
                    case "amber":
                        gvCustomerViewResults.Rows[i].Cells[2].BackColor = amber;
                        break;

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
        protected void btnDownloadFilteredResults_Click(object sender, EventArgs e)
        {
            string fileName = "Filtered_Results_Report_" + Common.timestamp() + ".csv";
            runReport(returnQuery(), fileName);

        }
        protected string returnQuery()
        {
            string baseQuery = "exec [sp_portal_Searchcustomerview] ";

            if (ddlStockStatusGV_FilterAccountManager.SelectedIndex != 0)
            {
                baseQuery += string.Format(" @accountmanager='{0}',", ddlStockStatusGV_FilterAccountManager.SelectedValue);
            }

            if (ddlStockStatusGV_FilterManufacturer.SelectedIndex != 0)
            {
                baseQuery += string.Format(" @manufacturer='{0}',", ddlStockStatusGV_FilterManufacturer.SelectedValue);
            }



            if (ddlStockStatusGV_FilterProductType.SelectedIndex != 0)
            {
                baseQuery += string.Format(" @producttype='{0}',", ddlStockStatusGV_FilterProductType.SelectedValue);
            }

            if (ddlStockStatusGV_FilterBusinessArea.SelectedIndex != 0)
            {
                baseQuery += string.Format(" @businessarea='{0}',", ddlStockStatusGV_FilterBusinessArea.SelectedValue);
            }

            if (rbtnExertisStock.SelectedIndex != 0)
            {
                switch (rbtnExertisStock.SelectedValue)
                {

                    case "Zero":
                        baseQuery += " @exertisstock=1,";
                        break;
                    case "Not Zero":
                        baseQuery += " @exertisstock=0,";
                        break;
                }
            }

            if (rbtnExertisPO.SelectedIndex != 0)
            {
                switch (rbtnExertisPO.SelectedValue)
                {

                    case "Has Pos":
                        baseQuery += " @exertispo=1,";
                        break;
                    case "No Pos":
                        baseQuery += " @exertispo=0,";
                        break;
                }
            }

            if (rbtnBackOrders.SelectedIndex != 0)
            {
                switch (rbtnBackOrders.SelectedValue)
                {

                    case "Backordered":
                        baseQuery += " @backorders=1,";
                        break;
                    case "No Backorders":
                        baseQuery += " @backorders=0,";
                        break;
                }
            }

            if (rbtnSafetyRating.SelectedIndex != 0)
            {
                baseQuery += " @stockbyweeks=" + rbtnSafetyRating.SelectedValue + ",";
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
            gvCustomerViewResults.DataSource = ds.Tables[0];
            gvCustomerViewResults.DataBind();
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + gvCustomerViewResults.ClientID + "', 400, 1050 , 40 ,true); </script>", false);
        }
        protected void gvProductStockStatus_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            bindSearchResults();
            gvCustomerViewResults.PageIndex = e.NewPageIndex;
            gvCustomerViewResults.DataBind();
        }
        protected void bindAccountManagerDDL()
        {
            string buyerSQL = @"select distinct COALESCE (pm.ExertisAccountManager, 'N/A') as AccountManager  from   MSE_KewillProductStockStatus ss
		inner join MSE_OracleCustomerSkuMapping sm on sm.customercode='MC005030' and sm.customer_item_number=ss.cat_no
		inner join mse_oracleproducts op on op.itemid=sm.inventory_item_id
		 LEFT OUTER JOIN
                         dbo.MSE_PortalAccountManagers AS pm ON pm.Business_area = op.Business_area AND pm.CustomerID = 1
		order by COALESCE (pm.ExertisAccountManager, 'N/A')";

            ddlStockStatusGV_FilterAccountManager.DataTextField = "AccountManager";
            ddlStockStatusGV_FilterAccountManager.DataValueField = "AccountManager";
            ddlStockStatusGV_FilterAccountManager.DataSource = Common.runSQLDataset(buyerSQL);
            ddlStockStatusGV_FilterAccountManager.DataBind();
            ddlStockStatusGV_FilterAccountManager.Items.Insert(0, new ListItem("-- All Account Managers --"));

        }
        protected void bindManufacturerDDL()
        {
            string buyerSQL = @"select distinct op.Manufacturer from   MSE_KewillProductStockStatus ss
		inner join MSE_OracleCustomerSkuMapping sm on sm.customercode='MC005030' and sm.customer_item_number=ss.cat_no
		inner join mse_oracleproducts op on op.itemid=sm.inventory_item_id
		order by op.Manufacturer";

            ddlStockStatusGV_FilterManufacturer.DataTextField = "Manufacturer";
            ddlStockStatusGV_FilterManufacturer.DataValueField = "Manufacturer";
            ddlStockStatusGV_FilterManufacturer.DataSource = Common.runSQLDataset(buyerSQL);
            ddlStockStatusGV_FilterManufacturer.DataBind();
            ddlStockStatusGV_FilterManufacturer.Items.Insert(0, new ListItem("-- All Manufacturers --"));

        }
        protected void bindProductTypeDDL()
        {
            string buyerSQL = @"select distinct a.top_cat 
from(
select op.top_cat
from   MSE_KewillProductStockStatus ss
		inner join MSE_OracleCustomerSkuMapping sm on sm.customercode='MC005030' and sm.customer_item_number=ss.cat_no
		inner join mse_oracleproducts op on op.itemid=sm.inventory_item_id
		
		union
		select op.top_cat from 
		dbo.MSE_SDGProductRange spm
left outer join dbo.MSE_OracleCustomerSkuMapping AS sm ON sm.Customercode = 'MC005030' AND sm.CUSTOMER_ITEM_NUMBER = spm.CatNo 

 LEFT OUTER JOIN
						 mse_oracleproducts op ON op.ItemID = sm.INVENTORY_ITEM_ID AND op.InvOrgID = 88
						 where op.top_cat is not null
						 ) a
		order by a.top_cat";

            ddlStockStatusGV_FilterProductType.DataTextField = "top_cat";
            ddlStockStatusGV_FilterProductType.DataValueField = "top_cat";
            ddlStockStatusGV_FilterProductType.DataSource = Common.runSQLDataset(buyerSQL);
            ddlStockStatusGV_FilterProductType.DataBind();
            ddlStockStatusGV_FilterProductType.Items.Insert(0, new ListItem("-- All Product Types --"));
        }
        protected void bindBusinessUnitDDL()
        {
            string businessUnitSQL = @"select distinct op.business_unit_code from   MSE_KewillProductStockStatus ss
		inner join MSE_OracleCustomerSkuMapping sm on sm.customercode='MC005030' and sm.customer_item_number=ss.cat_no
		inner join mse_oracleproducts op on op.itemid=sm.inventory_item_id
		order by op.business_unit_code";

            ddlStockStatusGV_FilterBusinessArea.DataTextField = "business_unit_code";
            ddlStockStatusGV_FilterBusinessArea.DataValueField = "business_unit_code";
            ddlStockStatusGV_FilterBusinessArea.DataSource = Common.runSQLDataset(businessUnitSQL);
            ddlStockStatusGV_FilterBusinessArea.DataBind();
            ddlStockStatusGV_FilterBusinessArea.Items.Insert(0, new ListItem("-- All Business Areas --"));



        }

        protected void gvCustomerViewResults_Load(object sender, EventArgs e)
        {
            gvCustomerViewResults.RowStyle.CssClass = "row";
            gvCustomerViewResults.CssClass = "gridview";
			gvCustomerViewResults.AlternatingRowStyle.CssClass = "row-alternate";
            gvCustomerViewResults.HeaderStyle.CssClass = "gv-header";
            bindSearchResults();

        }




    }
}