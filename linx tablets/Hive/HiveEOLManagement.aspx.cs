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
    public partial class HiveEOLManagement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            gvCustomerViewResults.DataSource = Common.runSQLDataset("sp_portal_Searchcustomerview2");
            gvCustomerViewResults.DataBind();

            ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + gvCustomerViewResults.ClientID + "', 400, 1100 , 60 ,true); </script>", false);
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
        protected void gvCustomerViewResults_PreRender(object sender, EventArgs e)
        {



            if (gvCustomerViewResults.Rows.Count > 0)
            {
                //This replaces <td> with <th> and adds the scope attribute
                gvCustomerViewResults.UseAccessibleHeader = true;

                //This will add the <thead> and <tbody> elements
                gvCustomerViewResults.HeaderRow.TableSection = TableRowSection.TableHeader;

                //This adds the <tfoot> element. 
                //Remove if you don't have a footer row
                gvCustomerViewResults.FooterRow.TableSection = TableRowSection.TableFooter;
            }
        }
        
    }
}