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
    public partial class BundleManagement : System.Web.UI.Page
    {
        public string filePathD = @"C:\linx-tablets\replen files\";
        protected void Page_Load(object sender, EventArgs e)
        {


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


            filename = filename.Replace(".csv", ".xls");
            DataSet dsConsignmentStock = Common.runSQLDataset(query);

            PortalCommon.Excel.GenerateExcelSheetNew(dsConsignmentStock, "Download", filePathD + filename);
            
            FileInfo file = new FileInfo(filePathD + filename);
            DownloadFile(file);
        }
        protected void btnUploadBundleProducts_Click(object sender, EventArgs e)
        {
            if (fuBundleProducts.HasFile && Path.GetExtension(fuBundleProducts.FileName).ToLower() == ".xls")
            {
                Common.runSQLNonQuery("delete from productdataloader_portal_hiveproductbundle_tempload");
                string filename = Path.GetFileNameWithoutExtension(fuBundleProducts.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuBundleProducts.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuBundleProducts.SaveAs(filePathLocale + filename);
                        filename = Path.GetFileName(PortalCommon.Excel.ConvertExcelToCsv(filePathLocale + filename));

                    }
                    catch
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('File save failure');", true);
                    }
                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalUploadedFiles/", "exertissdg", "Exertissdg1");
                    try
                    {
                        ftpClient.uploadFile(filePathLocale + filename);
                    }
                    catch (Exception ex)
                    {
                    }
                    string newFilename = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\exertissdg\portalUploadedFiles\" + filename;
                    string bulkInsert = string.Format(@"BULK INSERT productdataloader_portal_hiveproductbundle_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_portal_hiveproductbundle_tempload").ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar(@"select count(*) from productdataloader_portal_hiveproductbundle_tempload
 where (exertis3plproductcode is null or componentpartcode is null or componentqty is null or hivesku is  null)").ToString()) > 0)
                        throw new Exception("Ensure all columns are populated fort every line");

                    string updateSQL = string.Format("exec sp_hivebundleupload '{0}','{1}'", fuBundleProducts.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the Bundle range has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report.');", true);
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

        protected void btnDownloadBundleRange_Click(object sender, EventArgs e)
        {
            string filename = "Bundle_Range_Existing_" + Common.timestamp() + ".csv";
            runReport("exec sp_portalhivebundleproductsexisting", filename);
        }
        protected void btnDownloadBundleRangeTemplate_Click(object sender, EventArgs e)
        {
            string filename = "Bundle_Range_Template_" + Common.timestamp() + ".csv";
            runReport("exec sp_portalhivebundleproductstemplate", filename);
        }



        protected void gvBundleProducts_RowDataBound(object source, GridViewRowEventArgs e)
        {

            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                GridViewRow row = e.Row;
                GridView gv = new GridView();
                gv = (GridView)row.FindControl("gvBundleProductsComponents");
                string bundleID = ((DataRowView)e.Row.DataItem)["bundleid"].ToString();
                gv.DataSource = Common.runSQLDataset(string.Format(@" select bcp.ProductCode,Product_Description,qty as ComponentQty from MSE_PortalHiveBundleComponentProduct bcp left outer join mse_oracleproducts op on op.product_code=bcp.ProductCode
 where BundleID=50 order by Product_Description", bundleID));
                gv.DataBind();
            }
        }
    }
}