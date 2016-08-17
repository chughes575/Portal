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
namespace linx_tablets.SDG
{
    public partial class ProductStockStatusSearch : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                bindBusinessUnitDDL();
                bindBuyerDDL();
                bindManufacturerDDL();
                bindAccountManagerDDL();
                bindProductTypeDDL();
                bindSearchResults();
                

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
        protected void bindBuyerDDL()
        {
            string buyerSQL = @"select distinct op.buyer from   MSE_KewillProductStockStatus ss
		inner join MSE_OracleCustomerSkuMapping sm on sm.customercode='MC005030' and sm.customer_item_number=ss.cat_no
		inner join mse_oracleproducts op on op.itemid=sm.inventory_item_id
		order by op.buyer";

            ddlStockStatusGV_FilterBuyer.DataTextField = "buyer";
            ddlStockStatusGV_FilterBuyer.DataValueField = "buyer";
            ddlStockStatusGV_FilterBuyer.DataSource = Common.runSQLDataset(buyerSQL);
            ddlStockStatusGV_FilterBuyer.DataBind();
            ddlStockStatusGV_FilterBuyer.Items.Insert(0, new ListItem("-- All Buyers --"));



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


        protected void bindSearchResults()
        {

            string sql = returnQuery();
            DataSet ds = Common.runSQLDataset(sql);
            gvProductStockStatus.DataSource = ds.Tables[0];
            gvProductStockStatus.DataBind();
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + gvProductStockStatus.ClientID + "', 400, 1050 , 40 ,true); </script>", false);
        }
        protected void btnFilterReport_Click(object sender, EventArgs e)
        {
            bindSearchResults();
        }
        protected void btnDownloadFilteredResults_Click(object sender, EventArgs e)
        {
            string fileName = "Filtered_Results_Report_" + Common.timestamp() + ".csv";
            runReport(returnQuery(), fileName);

        }
        protected string returnQuery()
        {
            string orderBy = "[cat no]";
            string sql = Common.getConfigValue("SDG Product Status SQL");
            sql = "select * from sdgresults";
            int whereCounter = 0;
            sql += " where ";

            int ind = rbtnExertisStock.SelectedIndex;
            if (rbtnExertisStock.SelectedIndex != 0)
            {
                
                switch (rbtnExertisStock.SelectedValue)
                {
                    case "Zero":
                        sql += "cast([exertis stock] as int)=0 ";
                        break;
                    case "Not Zero":
                        sql += "cast([exertis stock] as int)<>0 ";
                        break;
                }
                whereCounter++;
            }
            if (rbtnExertisPO.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                switch (rbtnExertisPO.SelectedValue)
                {
                    case "Zero":
                        sql += "cast([Total Qty on PO] as int)=0 ";
                        break;
                    case "Not Zero":
                        sql += "cast([Total Qty on PO] as int)<>0 ";
                        break;
                }
                whereCounter++;
            }

            if (rbtnBackOrders.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                switch (rbtnBackOrders.SelectedValue)
                {
                    case "Zero":
                        sql += "cast([Backorders] as int)=0 ";
                        break;
                    case "Not Zero":
                        sql += "cast([Backorders] as int)<>0 ";
                        break;
                }
                whereCounter++;
            }


            if (rbtnVendorLT.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                switch (rbtnVendorLT.SelectedValue)
                {
                    case "Zero":
                        sql += "cast([Vendor LT] as int)=0 ";
                        break;
                    case "Not Zero":
                        sql += "cast([Vendor LT] as int)<>0 ";
                        break;
                }
                whereCounter++;
            }

            if (rbtnRunrate.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                switch (rbtnRunrate.SelectedValue)
                {
                    case "Zero":
                        sql += "cast([7 Day Run rate] as int)=0 ";
                        break;
                    case "Not Zero":
                        sql += "cast([7 Day Run rate] as int)<>0 ";
                        break;
                }
                whereCounter++;
            }

            if (rbtnStockByWeeks.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                switch (rbtnStockByWeeks.SelectedValue)
                {
                    case "Zero":
                        sql += "cast([Stock by Wks] as int)=0 ";
                        break;
                    case "Not Zero":
                        sql += "cast([Stock by Wks] as int)<>0 ";
                        break;
                }
                whereCounter++;
            }

            if (rbtnAgedStock.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                switch (rbtnAgedStock.SelectedValue)
                {
                    case "Zero":
                        sql += "cast([Aged Stock Qty] as int)=0 ";
                        break;
                    case "Not Zero":
                        sql += "cast([Aged Stock Qty] as int)<>0 ";
                        break;
                }
                whereCounter++;
            }



            if (ddlStockStatusGV_FilterBusinessArea.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                sql += "[business unit]='" + ddlStockStatusGV_FilterBusinessArea.SelectedValue + "' ";
                whereCounter++;
            }
            if (ddlStockStatusGV_FilterBuyer.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                sql += "[Buyer]='" + ddlStockStatusGV_FilterBuyer.SelectedValue + "'";
                whereCounter++;
            }
            if (ddlStockStatusGV_FilterManufacturer.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                sql += "brand='" + ddlStockStatusGV_FilterManufacturer.SelectedValue + "'";
                whereCounter++;
            }
            if (ddlStockStatusGV_FilterAccountManager.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                sql += "[Account Manager]='" + ddlStockStatusGV_FilterAccountManager.SelectedValue + "'";
                whereCounter++;
            }

            if (ddlStockStatusGV_FilterProductType.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                sql += "[Top Cat]='" + ddlStockStatusGV_FilterProductType.SelectedValue + "'";
                whereCounter++;
            }


            
            if (ddlStockStatusGV_FilterExertisLive.SelectedIndex != 0)
            {
                if (whereCounter > 0)
                {
                    sql += "and ";
                }
                switch (ddlStockStatusGV_FilterExertisLive.SelectedValue)
                {
                    case "Live":
                        sql += "[Exertis Live]='Yes'";
                        break;
                    case "Not Live":
                        sql += "[Exertis Live]='No'";
                        break;
                }
                whereCounter++;

            }
            if (whereCounter == 0)
            {
                sql = sql.Replace("sdgresults where", "sdgresults");
            }
            sql += Common.getConfigValue("SDG Product Status SQL group by");
            
            
            sql += @"  order by " + orderBy;

            bool test = false;
            
            return sql;
        }

        protected void gvProductStockStatus_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            bindSearchResults();
            gvProductStockStatus.PageIndex = e.NewPageIndex;
            gvProductStockStatus.DataBind();
        }

        protected void gvProductStockStatus_DataBound(object sender, EventArgs e)
        {
            if (gvProductStockStatus.Rows.Count > 0)
            {
                btnDownloadFilteredResults.Enabled = true;
            }
            else
            {
                btnDownloadFilteredResults.Enabled = false;
            }
        }

        protected void gvProductStockStatus_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            const string greenHex = "#E77474";
            const string amberHex = "#74AEE7";
            const string redHex = "#D82E2E";


            Color Cancelled = System.Drawing.ColorTranslator.FromHtml(greenHex);
            Color Closed = System.Drawing.ColorTranslator.FromHtml(amberHex);
            Color Shipped = System.Drawing.ColorTranslator.FromHtml(redHex);
            

        }
    }
}