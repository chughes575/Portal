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
using System.Data.SqlClient;
using PortalCommon;

namespace linx_tablets.BPC
{
    public partial class ConsignmetnUploads : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {


        }
        public List<SqlParameter> consignmentRetailerInsertParameters = new List<SqlParameter>();
        protected void gvConsignmentretailers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            //if (e.CommandName == "NoDataInsert")
            //{
            //    TextBox txtNoDataNotes = (TextBox)gvOrderNotes.Controls[0].Controls[0].FindControl("txtNoDataNotes");
            //    SqlParameter notes = new SqlParameter("@notes", SqlDbType.VarChar, 8000);
            //    notes.Direction = ParameterDirection.Input;
            //    notes.Value = txtNoDataNotes.Text;
            //    orderNotesInsertParameters.Add(notes);

            //    orderNotesInsertParameters.Add(new SqlParameter("@orderID", Request["orderID"]));
            //    orderNotesInsertParameters.Add(new SqlParameter("@staffID", Session["LoginStaffID"]));

            //    sqlDsOrderNotes.Insert();
            //}
            //else

            if (e.CommandName == "Insert")
            {
                TextBox txtRetailDescription = (TextBox)gvConsignmentretailers.FooterRow.FindControl("txtRetailDescription");
                TextBox txtOracleCode = (TextBox)gvConsignmentretailers.FooterRow.FindControl("txtOracleCode");
                //BoundField bndRetailerID = (BoundField)gvConsignmentretailers.FooterRow.FindControl("txtOracleCode");

                SqlParameter retailerdescriptionParam = new SqlParameter("@retailerdescription", SqlDbType.VarChar, 8000);
                SqlParameter oraclecodeParam = new SqlParameter("@oraclecode", SqlDbType.VarChar, 8000);

                
                retailerdescriptionParam.Direction = ParameterDirection.Input;
                retailerdescriptionParam.Value = txtRetailDescription.Text;
                oraclecodeParam.Direction = ParameterDirection.Input;
                oraclecodeParam.Value = txtOracleCode.Text;

                consignmentRetailerInsertParameters.Add(retailerdescriptionParam);
                consignmentRetailerInsertParameters.Add(oraclecodeParam);
                //consignmentRetailerInsertParameters.Add(retailerIDParam);
                sqlDsConsignmentRetailers.Insert();
            }
            else if (e.CommandName == "Download")
            {
                string filePath = @"C:\linx-tablets\replen files\";
                string filename = "Consignment_Stock_Existing__Retailer_" + e.CommandArgument.ToString() + "_" + Common.timestamp() + ".xls";
                DataSet dsConsignmentStock = Common.runSQLDataset("exec sp_portalretailerconsignmentstock_existing " + e.CommandArgument.ToString());

                PortalCommon.Excel.GenerateExcelSheetNew(dsConsignmentStock, "Inventory", filePath + filename);
                //PortalCommon.Excel.convertExcelToCSV(filePath + filename, "Inventory", filePath + Path.GetFileNameWithoutExtension(filename) + ".xls");
                string filepathCSV = PortalCommon.Excel.ConvertExcelToCsv(filePath + filename);
                FileInfo file = new FileInfo(filePath + filename);
                DownloadFile(file);
            }
            else if (e.CommandName == "DownloadOs")
            {
                GridViewRow row = (GridViewRow)(((Button)e.CommandSource).NamingContainer);
                // row contains current Clicked Gridview Row



                DropDownList ddl = row.FindControl("ddlFileNames") as DropDownList;
                string filename = ddl.SelectedValue.ToString();
                string locale = Common.runSQLScalar(@"select dbo.fn_PortalGetFilSubPath('" + filename + "')").ToString();
                string filePath = @"C:\Linx-tablets\saleseposfiles\" + locale + @"\" + filename;
                FileInfo file = new FileInfo(filePath);
                DownloadFile(file);
            }
        }

        protected void gvConsignmentretailers_DataBound(Object sender, EventArgs e)
        {

            for (int i = 0; i <= gvConsignmentretailers.Rows.Count - 1; i++)
            {

                DropDownList ddl = gvConsignmentretailers.Rows[i].FindControl("ddlFileNames") as DropDownList;
                Button btnDownloadLinesC = gvConsignmentretailers.Rows[i].FindControl("btnDownloadLinesC") as Button;
                Button btnDeleteButton = gvConsignmentretailers.Rows[i].FindControl("Button4") as Button;
                Button btnEditButton = gvConsignmentretailers.Rows[i].FindControl("EditNotesButton") as Button;
                Label lblRetailerID = gvConsignmentretailers.Rows[i].FindControl("lblRetailerID") as Label;
                Label lblReportDate = gvConsignmentretailers.Rows[i].FindControl("lblReportDate") as Label;


                
                String status3PL = gvConsignmentretailers.Rows[i].Cells[0].Text;
                //String status = gvCustomerViewResults.Rows[i].Cells[0].Text;
                const string greenHex = "#00cc66";
                const string redHex = "#ff0000";
                const string amberHex = "#ffcc00";
                Color green = System.Drawing.ColorTranslator.FromHtml(greenHex);
                Color red = System.Drawing.ColorTranslator.FromHtml(redHex);
                Color amber = System.Drawing.ColorTranslator.FromHtml(amberHex);






                if (lblRetailerID.Text == "4" || lblRetailerID.Text == "5" || lblRetailerID.Text == "6" || lblRetailerID.Text == "7" || lblRetailerID.Text == "8" || lblRetailerID.Text == "9")
                {
                    btnEditButton.Visible = false;
                    btnDeleteButton.Visible = false;
                    
                    gvConsignmentretailers.Rows[i].Cells[1].BackColor = amber;
                }
                else
                {
                    lblReportDate.Visible = false;
                    ddl.Visible = false;
                }
                    if (ddl.Items.Count == 0)
                {
                    ddl.Enabled = false;
                    ddl.Items.Add("N/A");
                    btnDownloadLinesC.Enabled = false;
                }
            }
        }
        protected void sqlDsConsignmentRetailers_Inserting(object sender, SqlDataSourceCommandEventArgs e)
        {
            e.Command.Parameters.Clear();
            foreach (SqlParameter p in consignmentRetailerInsertParameters)
                e.Command.Parameters.Add(p);
        }
        protected void btnDownloadExistingConsignmentStock_Click(object sender, EventArgs e)
        {
            string filePath = @"C:\linx-tablets\replen files\";
            string filename = "Stock_Existing_" + Common.timestamp() + ".xls";
            PortalCommon.Excel.GenerateExcelSheetNew(Common.runSQLDataset("exec sp_portalretailerconsignmentstock_existing"), "Inventory", filePath + filename);
            FileInfo file = new FileInfo(filePath + filename);
            DownloadFile(file);



        }
        protected void btnDownloadTemplateConsignmentStock_Click(object sender, EventArgs e)
        {
            string filePath = @"C:\linx-tablets\replen files\";
            string filename = "Stock_Template_" + Common.timestamp() + ".xls";
            PortalCommon.Excel.GenerateExcelSheetNew(Common.runSQLDataset("exec sp_portalretailerconsignmentstock_template"), "Inventory", filePath + filename);
            FileInfo file = new FileInfo(filePath + filename);
            DownloadFile(file);
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

        protected void btnUploadConsignmentStock_Click(object sender, EventArgs e)
        {
            string ext = Path.GetExtension(fuConsignmentStock.FileName);
            if (fuConsignmentStock.HasFile && Path.GetExtension(fuConsignmentStock.FileName).ToLower() == ".xls")
            {
                Common.runSQLNonQuery("delete from product_dataloader_portal_retailerconsignmentstockfigures");
                string filename = Path.GetFileNameWithoutExtension(fuConsignmentStock.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuConsignmentStock.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuConsignmentStock.SaveAs(filePathLocale + filename);
                        filename = Path.GetFileName(PortalCommon.Excel.ConvertExcelToCsv(filePathLocale + filename));

                    }
                    catch (Exception ex)
                    {
                        Common.log(ex.Message);
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('File save failure " + ex.Message.ToString() + "');", true);
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
                    string bulkInsert = string.Format(@"BULK INSERT product_dataloader_portal_retailerconsignmentstockfigures FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from product_dataloader_portal_retailerconsignmentstockfigures").ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from product_dataloader_portal_retailerconsignmentstockfigures where (customersku is null or RetailerID is null or StockQty is null)").ToString()) > 0)
                        throw new Exception("Ensure CustomerSKU/RetailerID/Qty is populated for every line");
                    if (int.Parse(Common.runSQLScalar("select count(*) from product_dataloader_portal_retailerconsignmentstockfigures where retailerid not in (select retailerid from MSE_PortalConsignmentRetailers)").ToString()) > 0)
                        throw new Exception("Unkown RetailerID found in file");

                    if (int.Parse(Common.runSQLScalar("select count(*) from product_dataloader_portal_retailerconsignmentstockfigures where retailerid in (4, 5, 6, 7, 8, 9)").ToString()) > 0)
                        throw new Exception("Email suppliers found in files. These are populated via email pickup onl please remove");
                    
                    string updateSQL = string.Format("exec sp_portalretailerconsignmentstock '{0}','{1}'", fuConsignmentStock.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, retailer consignment stock levels have been updated');", true);
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
    }
}