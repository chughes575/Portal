

using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;

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
    public partial class OrderSummary : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void btnUkReplenFile_Command(object sender, CommandEventArgs e)
        {
            string str1 = e.CommandArgument.ToString();
            string type = e.CommandArgument.ToString().Substring(0, 1);
            
                if (str1 == "sugall")
                {
                    this.runReport("exec [sp_portalhive_pocomponentsuggestions] 0", "POSuggestions_Hive_Components_All_" + Common.timestamp() + ".csv");
                }
                if (str1 == "sugs")
                {
                    this.runReport("exec [sp_portalhive_pocomponentsuggestions] 1,0,1", "POSuggestions_Hive_Components_Spares_" + Common.timestamp() + ".csv");
                }
                if (str1 == "eol1")
                {
                    runReport("exec [ sp_portalhive_pobundlesuggestions] @all=0,@download=1,@delisted=1", "Hive_EOL_Bundle_Report_" + Common.timestamp() + ".csv");
                }
                if (str1 == "eol2")
                {
                    runReport("exec [sp_portalhive_pocomponentsuggestions] 0,1", "Hive_EOL_Components_Report_" + Common.timestamp() + ".csv");
                }
                if (str1 == "eol3")
                {
                    runReport("exec [sp_portalhive_pobundlesuggestions_exertis] 0,1", "Exertis_Hive_EOL_Bundle_Report_" + Common.timestamp() + ".csv");
                }
                if (str1 == "hivesoh")
                {
                    runReport("exec [sp_hivesohreportdownload]", "Hive_SOH_Report_" + Common.timestamp() + ".csv");
                }
                if (str1 == "gr1")
                {
                    runReport("exec [SP_portal_oraclegoodsreceiptsdownload] 1", "Goods_Receipts_Report_" + Common.timestamp() + ".csv");
                }
                if (str1 == "gr2")
                {
                    runReport("exec [SP_portal_oraclegoodsreceiptsdownload] 0", "Goods_Receipts_Report_" + Common.timestamp() + ".csv");
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
    }
}