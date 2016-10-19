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
    public partial class HiveProductStockHoldingRules : System.Web.UI.Page
    {
        public int customerID = 5;
        protected void Page_Load(object sender, EventArgs e)
        {

            string sql = Common.runSQLScalar("select configvalue from portalconfig where configkey='Show ExertisHive WeeksUsed Control'").ToString();
            if(sql == "1")
            pnlExertisHiveWeeksUsed.Visible = true;
            
            if (!Page.IsPostBack)
            {
                string leadTimeComponentExertis3PL = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsed3pl' and CustomerID=5").ToString();
                ddlForecastAmountUsedExertis3PL.SelectedIndex = ddlForecastAmountUsedExertis3PL.Items.IndexOf(ddlForecastAmountUsedExertis3PL.Items.FindByValue(leadTimeComponentExertis3PL));

                string leadTimeComponentExertisHive = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsedExertisHive' and CustomerID=6").ToString();
                ddlForecastAmountUsedExertisHive.SelectedIndex = ddlForecastAmountUsedExertisHive.Items.IndexOf(ddlForecastAmountUsedExertisHive.Items.FindByValue(leadTimeComponentExertisHive));

                string leadTimeBundlesExertisHive = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsedBundlesExertisHive' and CustomerID=6").ToString();
                ddlForecastAmountUsedBundlesExertisHive.SelectedIndex = ddlForecastAmountUsedBundlesExertisHive.Items.IndexOf(ddlForecastAmountUsedBundlesExertisHive.Items.FindByValue(leadTimeBundlesExertisHive));
                
                string leadTimeBundlesExertis3PL = Common.runSQLScalar("select configvalue from PortalConfig where ConfigKey='ForecastWeeksUsedBundles3pl' and CustomerID=5").ToString();
                ddlForecastAmountUsedBundlesExertis3PL.SelectedIndex = ddlForecastAmountUsedBundlesExertis3PL.Items.IndexOf(ddlForecastAmountUsedBundlesExertis3PL.Items.FindByValue(leadTimeBundlesExertis3PL));

                string selectSQL = "select configvalue from portalconfig where configkey='SellThruOverwrite' and customerid="+customerID;
                txtSellThroughPercentage.Text = Common.runSQLScalar(selectSQL).ToString();
            }
        }
        protected void btnUpdate3PLForecastWeeksUsed_Click(object sender, EventArgs e)
        {
            int leadTime = int.Parse(ddlForecastAmountUsedExertis3PL.SelectedValue.ToString());
            Common.runSQLNonQuery("update PortalConfig set configvalue='" + leadTime + "' where ConfigKey='ForecastWeeksUsed3pl' and CustomerID=5");
            Common.runSQLNonQuery(string.Format("update mse_portalforecastreportmanagement set lastfiledate=getdate(),Username='{0}' where reportid=27", HttpContext.Current.User.Identity.Name.ToString()));
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Forecast Weeks Used Update Successful');", true);
        }
        protected void btnUpdateExertisHiveForecastWeeksUsed_Click(object sender, EventArgs e)
        {
            int leadTime = int.Parse(ddlForecastAmountUsedExertisHive.SelectedValue.ToString());
            Common.runSQLNonQuery("update PortalConfig set configvalue='" + leadTime + "' where ConfigKey='ForecastWeeksUsedExertisHive' and CustomerID=6");
            Common.runSQLNonQuery(string.Format("update mse_portalforecastreportmanagement set lastfiledate=getdate(),Username='{0}' where reportid=26", HttpContext.Current.User.Identity.Name.ToString()));
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Forecast Weeks Used Update Successful');", true);
        }

        protected void btnUpdateForecastUsedBundlesExertisHive_Click(object sender, EventArgs e)
        {
            int leadTime = int.Parse(ddlForecastAmountUsedBundlesExertisHive.SelectedValue.ToString());
            Common.runSQLNonQuery("update PortalConfig set configvalue='" + leadTime + "' where ConfigKey='ForecastWeeksUsedBundlesExertisHive' and CustomerID=6");
            Common.runSQLNonQuery(string.Format("update mse_portalforecastreportmanagement set lastfiledate=getdate(),Username='{0}' where reportid=29", HttpContext.Current.User.Identity.Name.ToString()));
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Lead Time Update Successful');", true);
        }

        protected void btnUpdate3PLForecastWeeksUsedBundles_Click(object sender, EventArgs e)
        {
            int leadTime = int.Parse(ddlForecastAmountUsedBundlesExertis3PL.SelectedValue.ToString());
            Common.runSQLNonQuery("update PortalConfig set configvalue='" + leadTime + "' where ConfigKey='ForecastWeeksUsedBundles3pl' and CustomerID=5");
            Common.runSQLNonQuery(string.Format("update mse_portalforecastreportmanagement set lastfiledate=getdate(),Username='{0}' where reportid=30", HttpContext.Current.User.Identity.Name.ToString()));
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Lead Time Update Successful');", true);
        }

        protected void btnUpdateSellThroughOverwrite_Click(object sender, EventArgs e)
        {
            
            string updateSQL = string.Format("update portalconfig set configvalue={0} where configkey='SellThruOverwrite' and customerid=5", txtSellThroughPercentage.Text);
            Common.runSQLNonQuery(updateSQL);
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Sell Through Overwrite Update Successful');", true);
        }
    }
}