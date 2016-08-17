using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace linx_tablets.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack && User.Identity.IsAuthenticated) 
            {
                if (User.IsInRole("applegroup"))
                {
                    Response.Redirect("~/Reporting/AppleReporting.aspx");
                }
                else if (User.IsInRole("sdggroup"))
                {
                    Response.Redirect("~/SDG/UserUploads.aspx");
                }
                else if (User.IsInRole("sdgpublicgroup"))
                {
                    Response.Redirect("~/SDG/Public/ProductStatusDashboard.aspx");
                }
                else if (User.IsInRole("warrantygroupcustomers") || User.IsInRole("warrantygroupreturns") || User.IsInRole("warrantygroupsales"))
                {
                    Response.Redirect("~/WarrantyPortal/WarrantyManagement.aspx");
                }
                else if (User.IsInRole("Hivegroup"))
                {
                    Response.Redirect("~/Hive/home.aspx");
                }

                else
                {
                    Response.Redirect("~/Reporting/AppleReporting.aspx");
                }


                
            }
        }
        
    }
}