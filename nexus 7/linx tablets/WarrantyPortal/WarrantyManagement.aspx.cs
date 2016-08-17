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

namespace linx_tablets.WarrantyPortal
{
    public partial class WarrantyManagement : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!HttpContext.Current.User.IsInRole("warrantygroupsales"))
                imgSales.ImageUrl = "~/Images/x.png";

            if (!HttpContext.Current.User.IsInRole("warrantygroupreturns"))
                imgReturns.ImageUrl = "~/Images/x.png";

            if (!HttpContext.Current.User.IsInRole("warrantygroupcustomers"))
                imgCustomers.ImageUrl = "~/Images/x.png";
        }
        private void runReport(string query, string filename)
        {
            runReport(query, filename, false);
        }
        private void runReport(string query, string filename, bool sp)
        {
            this.Session["ReportQuery"] = (object)query;
            this.Session["ReportQueryIsSp"] = sp;
            this.Session["ReportDelimiter"] = (object)",";
            this.Session["ReportHasHeader"] = (object)true;
            this.Session["ReportFileName"] = (object)filename;
            this.Session["ReportTextQualifier"] = (object)"\"";
            this.Response.Redirect("~/warrantyportal/report-export-csv.aspx");
        }
        protected void btnWarrantyDownload_Click(object sender, EventArgs e)
        {
            string filename = "Customer_Vendor_Terms_Existing_" + Common.timestamp() + ".csv";
            runReport(@"SELECT [Customer]
      ,[Vendor]
      ,[Business_Unit]
      ,[Faulty]
      ,[Faulty_Terms]
      ,[Change_of_Mind_Remorse]
      ,[COM_Remorse_Terms]
      ,[Stock_Rotations]
      ,[Rotations_Terms]
      ,[Exertis_Deal_With_NFF]
      ,[Exertis_Deal_With_CID]
      ,[CID_Terms]
      ,[Who_Deals_With_Warranty_Repaired_Devices]
      ,[Vendor_Term___DOA]
      ,[Vendor_Term___Warranty]
      ,[Last_Update]
      ,[Agreed_By]
      ,[Review_Date]
      ,[Exposure]
      ,[Bridge]
  FROM [MSE].[dbo].[mse_warrantyportalterms]", filename);
        }
        protected void btnWarrantyUpload_Click(object sender, EventArgs e)
        {
            if (fuWarrantyUpload.HasFile)
            {
                Common.runSQLNonQuery("delete from productdataloader_warrantyupload_tempload");
                string filename = Path.GetFileNameWithoutExtension(fuWarrantyUpload.FileName).Replace("'","") + "_" + Common.timestamp() + Path.GetExtension(fuWarrantyUpload.FileName);
                string filePath = @"\\10.16.72.129\company\";
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuWarrantyUpload.SaveAs(filePathLocale + filename);

                    }
                    catch
                    {
                        throw new Exception();
                    }

                    //
                    string reportData = File.ReadAllText(filePathLocale + filename, Encoding.Default);
                    bool bypass = false;
                   var parts = reportData.Split('"');

                    for (var i = 1; i < parts.Length; i += 2)
                    {
                        parts[i] = parts[i].Replace(",", "");
                    }

                    reportData = string.Join("\"", parts);
                    string amendedFileName = "Amended" + filename;

                    File.AppendAllText(filePathLocale + amendedFileName, reportData, Encoding.Default);
                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/apple files/", "extranet", "Extranet1");
                    if (bypass)
                    {
                        ftpClient = new FTP("ftp.msent.co.uk", "/in/Apple VMI Report/", "apple", "Apple1");
                    }
                    try
                    {
                        ftpClient.uploadFile(filePathLocale + amendedFileName);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception();
                    }
                    if (!bypass)
                    {
                        string newFilename = "\\\\10.16.72.129\\company\\ftp\\root\\msesrvdom\\extranet\\apple files\\" + amendedFileName;
                        string bulkInsert = string.Format(@"BULK INSERT productdataloader_warrantyupload_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                        Common.runSQLNonQuery(bulkInsert);
                        if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_warrantyupload_tempload").ToString()) == 0)
                            throw new Exception();

                        string updateSQL = string.Format("exec sp_productdataloader_warrantyuploaddebug '{0}','{1}'", amendedFileName, HttpContext.Current.User.Identity.Name.ToString());
                        bool csGroup = HttpContext.Current.User.IsInRole("cswarrantygroup");
                        Common.runSQLNonQuery(updateSQL);
                    }
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the warranty file has been imported. This change is subject to approval from the sales, returns and customer services teams. These changes will not become live until approved by all three groups');", true);
                    sqlDSApprovals.DataBind();
                    gvApprovals.DataBind();

                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("duplicate"))
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, multiple rows found with the same customer,vendor and business_unit values.');", true);
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file');", true);
                    }
                    
                }
            }
        }

        protected void ddlCustomers_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (ddlCustomers.SelectedIndex == 0)
            {
                string ddlSQLAllVendor = "select distinct Vendor from mse_warrantyportalterms order by Vendor";
            }
            else
            {
                string ddlSQLVendors = string.Format(@"select distinct Vendor from mse_warrantyportalterms 
where customer='{0}'
order by Vendor", ddlCustomers.SelectedValue);
            }

        }

        protected void ddlVendors_SelectedIndexChanged(object sender, EventArgs e)
        {
//            if (ddlCustomers.SelectedIndex == 0)
//            {
//                string ddlSQLAllVendor = "select distinct Vendor from mse_warrantyportalterms order by Vendor";
//            }
//            else
//            {
//                string ddlSQLVendors = string.Format(@"select distinct Vendor from mse_warrantyportalterms 
//where customer='{0}'
//order by Vendor", ddlCustomers.SelectedValue);
//            }

        }

        protected void btnViewTerms_Click(object sender, EventArgs e)
        {
            string customerParam = "null";
            string vendorParam = "null";
            string productCodeParam = "null";
            string customerProductCodeParam = "null";

            if (ddlCustomers.SelectedIndex != 0)
                customerParam = "'" + ddlCustomers.SelectedValue.Replace("'", "").Replace(",", "") + "'";

            if (ddlVendors.SelectedIndex != 0)
                vendorParam = "'" + ddlVendors.SelectedValue.Replace("'", "").Replace(",", "") + "'";

            if (txtProductCode.Text != "")
                productCodeParam = "'" + txtProductCode.Text.Replace("'", "").Replace(",", "") + "'";

            if (txtCustomerProductCode.Text != "")
                productCodeParam = "'"+txtCustomerProductCode.Text.Replace("'", "").Replace(",", "")+"'";

            string procedureSearchCall = string.Format("exec sp_productdataloader_warrantysearch {0},{1},{2},{3}", vendorParam, customerParam, productCodeParam, customerProductCodeParam);
            DataSet DSsearchResults = Common.runSQLDataset(procedureSearchCall);

            gvWarrantySearchTerms.DataSource = DSsearchResults;
            gvWarrantySearchTerms.DataBind();

        }

        protected void gvWarrantySearchTerms_RowDataBound(object sender, GridViewRowEventArgs e)
        {

            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                GridViewRow row = e.Row;
                FormView fv = new FormView();
                fv = (FormView)row.FindControl("fvTermDataFull");
                string TermID = ((DataRowView)e.Row.DataItem)["TermID"].ToString();
                fv.DataSource = Common.runSQLDataset(@"select * from mse_warrantyportalterms where termid =" + TermID).Tables[0];
                fv.DataBind();
            }
        }

        protected void fvTermDataFull_ItemCommand(object sender, FormViewCommandEventArgs e)
        {
            string termID = e.CommandArgument.ToString();
            string filename = "Customer_Vendor_Terms_TermID_" + termID + "_" + Common.timestamp() + ".csv";
            runReport(@"SELECT [Customer]
      ,[Vendor]
      ,[Business_Unit]
      ,[Faulty]
      ,[Faulty_Terms]
      ,[Change_of_Mind_Remorse]
      ,[COM_Remorse_Terms]
      ,[Stock_Rotations]
      ,[Rotations_Terms]
      ,[Exertis_Deal_With_NFF]
      ,[Exertis_Deal_With_CID]
      ,[CID_Terms]
      ,[Who_Deals_With_Warranty_Repaired_Devices]
      ,[Vendor_Term___DOA]
      ,[Vendor_Term___Warranty]
      ,[Last_Update]
      ,[Agreed_By]
      ,[Review_Date]
      ,[Exposure]
      ,[Bridge]
  FROM [MSE].[dbo].[mse_warrantyportalterms] where termid =" + termID, filename);
        }

        protected void btnDownloadSearchResults_Click(object sender, EventArgs e)
        {
            string customerParam = "null";
            string vendorParam = "null";
            string productCodeParam = "null";
            string customerProductCodeParam = "null";

            if (ddlCustomers.SelectedIndex != 0)
                customerParam = "'" + ddlCustomers.SelectedValue.Replace("'", "").Replace(",", "") + "'";

            if (ddlVendors.SelectedIndex != 0)
                vendorParam = "'" + ddlVendors.SelectedValue.Replace("'", "").Replace(",", "") + "'";

            if (txtProductCode.Text != "")
                productCodeParam = "'" + txtProductCode.Text.Replace("'", "").Replace(",", "") + "'";

            if (txtCustomerProductCode.Text != "")
                productCodeParam = "'" + txtCustomerProductCode.Text.Replace("'", "").Replace(",", "") + "'";

            string procedureSearchCall = string.Format("exec sp_productdataloader_warrantysearch {0},{1},{2},{3}", vendorParam, customerParam, productCodeParam, customerProductCodeParam);
            string filename = "Customer_Vendor_Terms_SearchResults_" + Common.timestamp() + ".csv";
            runReport(procedureSearchCall, filename);
        }

        protected void gvApprovals_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string approvalID = e.CommandArgument.ToString();
            string username = HttpContext.Current.User.Identity.Name;
            string approveCountSql = string.Format("select count(*) from mse_warrantyportalapproval where SalesApproval=1 and ReturnsApproval=1 and CustomersApproval=1 and approvalid={0}", approvalID);
            string approvalSPSQL = string.Format("exec sp_warrantyapprovalmakelive {0}", approvalID);
            switch (e.CommandName)
            {
                case "downloadModifiedTerms":
                    string filename = "Modified_Terms_" + Common.timestamp() +".csv";
                    string downloadChangedTermsSQL = "select * from mse_warrantyportalapprovalitems where approvalid=" + approvalID;
                    runReport(downloadChangedTermsSQL, filename);
                    break;
                case "approveTermsSales":
                    string approveSalesSQL = string.Format("update mse_warrantyportalapproval set SalesApproval=1,SalesApprovalUser='{1}' where approvalid={0}", approvalID, username);
                    Common.runSQLNonQuery(approveSalesSQL);
                    if (int.Parse(Common.runSQLScalar(approveCountSql).ToString()) == 1)
                    {
                        //run stored which procedure which makes changes live
                        Common.runSQLNonQuery(approvalSPSQL);
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Terms approved, this approval has now been signed off by sales, returns and customer services group so these changes are now live');", true);
                        sqlDSApprovals.DataBind();
                        gvApprovals.DataBind();
                        sqlDSApprovalsComplete.DataBind();
                        gvApprovalsComplete.DataBind();
                    }
                    else
                    {
                        
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Terms approved');", true);
                        sqlDSApprovals.DataBind();
                        gvApprovals.DataBind();
                    }
                    
                    break;
                    case "approveTermsReturns":
                    string approveReturnsSQL = string.Format("update mse_warrantyportalapproval set ReturnsApproval=1,ReturnsApprovaluser='{1}' where approvalid={0}", approvalID, username);
                    Common.runSQLNonQuery(approveReturnsSQL);
                    if (int.Parse(Common.runSQLScalar(approveCountSql).ToString()) == 1)
                    {
                        //run stored which procedure which makes changes live
                        Common.runSQLNonQuery(approvalSPSQL);
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Terms approved, this approval has now been signed off by sales, returns and customer services group so these changes are now live');", true);
                        sqlDSApprovals.DataBind();
                        gvApprovals.DataBind();
                        sqlDSApprovalsComplete.DataBind();
                        gvApprovalsComplete.DataBind();
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Terms approved');", true);
                        sqlDSApprovals.DataBind();
                        gvApprovals.DataBind();
                    }
                    break;
                    case "approveTermsCustomers":

                    string approveCustomersSQL = string.Format("update mse_warrantyportalapproval set customersapproval=1,customersapprovalUser='{1}' where approvalid={0}", approvalID, username);
                    Common.runSQLNonQuery(approveCustomersSQL);
                    if (int.Parse(Common.runSQLScalar(approveCountSql).ToString()) == 1)
                    {
                        //run stored which procedure which makes changes live
                        Common.runSQLNonQuery(approvalSPSQL);
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Terms approved, this approval has now been signed off by sales, returns and customer services group so these changes are now live');", true);
                        sqlDSApprovals.DataBind();
                        gvApprovals.DataBind();
                        sqlDSApprovalsComplete.DataBind();
                        gvApprovalsComplete.DataBind();
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Terms approved');", true);
                        sqlDSApprovals.DataBind();
                        gvApprovals.DataBind();
                    }
                    break;
            }
        }

        protected void gvApprovals_RowDataBound(object sender, GridViewRowEventArgs e)
        {
             if (e.Row.RowType != DataControlRowType.DataRow)
                return;

             Button btnSalesApprove = (Button)e.Row.FindControl("btnApproveSales");
             Button btnReturns = (Button)e.Row.FindControl("btnApproveReturns");
             Button btnCS = (Button)e.Row.FindControl("btnApproveCustomers");
             if (DataBinder.Eval(e.Row.DataItem, "SalesApproval").ToString().Equals("True") && DataBinder.Eval(e.Row.DataItem, "ReturnsApproval").ToString().Equals("True") && DataBinder.Eval(e.Row.DataItem, "CustomersApproval").ToString().Equals("True"))
             {
                 e.Row.BackColor = Color.Green;
                 btnSalesApprove.Enabled = false;
                 btnReturns.Enabled = false;
                 btnCS.Enabled = false;
                 return;
             }


             

             
            if (DataBinder.Eval(e.Row.DataItem, "SalesApproval").ToString().Equals("False") && HttpContext.Current.User.IsInRole("warrantygroupsales"))
            {
                btnSalesApprove.BackColor = Color.Green;
            }
            else
            {
                btnSalesApprove.Enabled = false;
                btnSalesApprove.BackColor = Color.Red;
            }

            if (DataBinder.Eval(e.Row.DataItem, "ReturnsApproval").ToString().Equals("False") && HttpContext.Current.User.IsInRole("warrantygroupreturns"))
            {
                btnReturns.BackColor = Color.Green;
            }
            else
            {
                btnReturns.Enabled = false;
                btnReturns.BackColor = Color.Red;
            }

            if (DataBinder.Eval(e.Row.DataItem, "CustomersApproval").ToString().Equals("False") && HttpContext.Current.User.IsInRole("warrantygroupcustomers"))
            {
                btnCS.BackColor = Color.Green;
            }
            else
            {
                btnCS.Enabled = false;
                btnCS.BackColor = Color.Red;
            } 
            
        }

        
    }
}