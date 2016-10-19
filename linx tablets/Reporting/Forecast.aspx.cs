using MSE_Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace linx_tablets.Reporting
{
    public partial class Forecast : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void btnDownloadFC_Click(object sender, EventArgs e)
        {
            int reportID = int.Parse(this.ddlForecastReports.SelectedValue);
            if (reportID == 0)
                return;
            this.createForecastCommitResponse(reportID, false);
        }
        protected void btnUploadFCProcessing_Click(object sender, EventArgs e)
        {
            List<int> erroRowList = new List<int>();
            List<int> successList = new List<int>();
            List<int> skipRows = new List<int>();
            SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
            conn.Open();
            DataTable table = SqlBulkLoader.getSchemaForTable("productdataloader_apple_forecastcommits_tempload_processing", conn);
            SqlBulkLoader bcp = new SqlBulkLoader(table, "productdataloader_apple_forecastcommits_tempload_processing", conn, true);

            Common.runSQLNonQuery("delete from productdataloader_apple_forecastcommits_tempload_processing");
            if (fupFCUpload.HasFile && Path.GetExtension(fupFCUpload.FileName) == ".csv")
            {
                try
                {
                    string fileNameAndLocation = fupFCUpload.PostedFile.FileName;
                    DataTable spreadsheet = new DataTable();

                    int incorrectColumns = 0;
                    int headerCount = 0;
                    int headerColumnCount = 0;

                    int lineCount = 0;
                    string values = "";
                    string insertvalues = "";
                    int[] headerarray = new int[90];
                    for (int ih = 0; ih < headerarray.Length; ih++)
                    {
                        headerarray[ih] = 1000000;
                    }
                    Stream fileStream = fupFCUpload.PostedFile.InputStream;
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        char[] separator = new char[] { ',' };
                        string item = null;

                        while ((item = streamReader.ReadLine()) != null)
                        {
                            //split
                            string[] itemData = item.Split(separator, StringSplitOptions.None);

                            if (itemData[0].ToString() != "")
                            {
                                string header = "select";

                                string[] itemArray = new string[]
                            {
                                "null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null",
"null"
};
                                for (int i = 0; i < itemData.Length; i++)
                                {
                                    Dictionary<string, string> headerDict = new Dictionary<string, string>();

                                    string valComp = itemData[i].ToLower();
                                    string val = itemData[i];
                                    if (headerCount == 0)
                                    {

                                        if (valComp != "exertisreportid" && valComp != "region" && valComp != "plant" && valComp != "plannercode" && valComp != "article" && valComp != "vendor_mat_num" && valComp != "description" && valComp != "vendornumber" && valComp != "vendorname" && valComp != "prv_wk_bookings_4" && valComp != "prv_wk_bookings_3" && valComp != "prv_wk_bookings_2" && valComp != "prv_wk_bookings_1" && valComp != "total_bookings" && valComp != "average_bookings" && valComp != "prv_wk_sell_thru_retail_4" && valComp != "prv_wk_sell_thru_retail_3" && valComp != "prv_wk_sell_thru_retail_2" && valComp != "prv_wk_sell_thru_retail_1" && valComp != "total_sell_thru_retail" && valComp != "average_sell_thru_retail" && valComp != "prv_wk_sell_thru_online_4" && valComp != "prv_wk_sell_thru_online_3" && valComp != "prv_wk_sell_thru_online_2" && valComp != "prv_wk_sell_thru_online_1" && valComp != "total_sell_thru_online" && valComp != "average_sell_thru_online" && valComp != "prv_wk_sell_thru_rslr_4" && valComp != "prv_wk_sell_thru_rslr_3" && valComp != "prv_wk_sell_thru_rslr_2" && valComp != "prv_wk_sell_thru_rslr_1" && valComp != "total_sell_thru_rslr" && valComp != "average_sell_thru_rslr" && valComp != "prv_wk_sell_thru_all_4" && valComp != "prv_wk_sell_thru_all_3" && valComp != "prv_wk_sell_thru_all_2" && valComp != "prv_wk_sell_thru_all_1" && valComp != "total_sell_thru_all" && valComp != "average_sell_thru_all" && valComp != "gross_fcst_cw" && valComp != "gross_fcst_wk1" && valComp != "gross_fcst_wk2" && valComp != "gross_fcst_wk3" && valComp != "gross_fcst_wk4" && valComp != "gross_fcst_wk5" && valComp != "gross_fcst_wk6" && valComp != "gross_fcst_wk7" && valComp != "gross_fcst_wk8" && valComp != "gross_fcst_wk9" && valComp != "gross_fcst_wk10" && valComp != "gross_fcst_wk11" && valComp != "gross_fcst_wk12" && valComp != "total_gross_fc" && valComp != "open_po" && valComp != "onhand" && valComp != "safety_stock" && valComp != "bklg_retail" && valComp != "bklg_online" && valComp != "bklg_reseller" && valComp != "total_backlog" && valComp != "net_fcst_cw" && valComp != "net_fcst_wk1" && valComp != "net_fcst_wk2" && valComp != "net_fcst_wk3" && valComp != "net_fcst_wk4" && valComp != "net_fcst_wk5" && valComp != "net_fcst_wk6" && valComp != "net_fcst_wk7" && valComp != "net_fcst_wk8" && valComp != "net_fcst_wk9" && valComp != "net_fcst_wk10" && valComp != "net_fcst_wk11" && valComp != "net_fcst_wk12" && valComp != "total_net_fc" && valComp != "net_commits_cw" && valComp != "net_commits_wk1" && valComp != "net_commits_wk2" && valComp != "net_commits_wk3" && valComp != "net_commits_wk4" && valComp != "net_commits_wk5" && valComp != "net_commits_wk6" && valComp != "net_commits_wk7" && valComp != "net_commits_wk8" && valComp != "net_commits_wk9" && valComp != "net_commits_wk10" && valComp != "net_commits_wk11" && valComp != "net_commits_wk12" && valComp != "vendor_comments" && valComp != "report_week" && valComp != "report_date")
                                        {
                                            incorrectColumns++;
                                            skipRows.Add(i);
                                        }
                                        else
                                        {

                                            switch (valComp)
                                            {


                                                case "exertisreportid":
                                                    headerarray[0] = i;
                                                    break;
                                                case "region":
                                                    headerarray[1] = i;
                                                    break;
                                                case "plant":
                                                    headerarray[2] = i;
                                                    break;
                                                case "plannercode":
                                                    headerarray[3] = i;
                                                    break;
                                                case "article":
                                                    headerarray[4] = i;
                                                    break;
                                                case "vendor_mat_num":
                                                    headerarray[5] = i;
                                                    break;
                                                case "description":
                                                    headerarray[6] = i;
                                                    break;
                                                case "vendornumber":
                                                    headerarray[7] = i;
                                                    break;
                                                case "vendorname":
                                                    headerarray[8] = i;
                                                    break;
                                                case "prv_wk_bookings_4":
                                                    headerarray[9] = i;
                                                    break;
                                                case "prv_wk_bookings_3":
                                                    headerarray[10] = i;
                                                    break;
                                                case "prv_wk_bookings_2":
                                                    headerarray[11] = i;
                                                    break;
                                                case "prv_wk_bookings_1":
                                                    headerarray[12] = i;
                                                    break;
                                                case "total_bookings":
                                                    headerarray[13] = i;
                                                    break;
                                                case "average_bookings":
                                                    headerarray[14] = i;
                                                    break;
                                                case "prv_wk_sell_thru_retail_4":
                                                    headerarray[15] = i;
                                                    break;
                                                case "prv_wk_sell_thru_retail_3":
                                                    headerarray[16] = i;
                                                    break;
                                                case "prv_wk_sell_thru_retail_2":
                                                    headerarray[17] = i;
                                                    break;
                                                case "prv_wk_sell_thru_retail_1":
                                                    headerarray[18] = i;
                                                    break;
                                                case "total_sell_thru_retail":
                                                    headerarray[19] = i;
                                                    break;
                                                case "average_sell_thru_retail":
                                                    headerarray[20] = i;
                                                    break;
                                                case "prv_wk_sell_thru_online_4":
                                                    headerarray[21] = i;
                                                    break;
                                                case "prv_wk_sell_thru_online_3":
                                                    headerarray[22] = i;
                                                    break;
                                                case "prv_wk_sell_thru_online_2":
                                                    headerarray[23] = i;
                                                    break;
                                                case "prv_wk_sell_thru_online_1":
                                                    headerarray[24] = i;
                                                    break;
                                                case "total_sell_thru_online":
                                                    headerarray[25] = i;
                                                    break;
                                                case "average_sell_thru_online":
                                                    headerarray[26] = i;
                                                    break;
                                                case "prv_wk_sell_thru_rslr_4":
                                                    headerarray[27] = i;
                                                    break;
                                                case "prv_wk_sell_thru_rslr_3":
                                                    headerarray[28] = i;
                                                    break;
                                                case "prv_wk_sell_thru_rslr_2":
                                                    headerarray[29] = i;
                                                    break;
                                                case "prv_wk_sell_thru_rslr_1":
                                                    headerarray[30] = i;
                                                    break;
                                                case "total_sell_thru_rslr":
                                                    headerarray[31] = i;
                                                    break;
                                                case "average_sell_thru_rslr":
                                                    headerarray[32] = i;
                                                    break;
                                                case "prv_wk_sell_thru_all_4":
                                                    headerarray[33] = i;
                                                    break;
                                                case "prv_wk_sell_thru_all_3":
                                                    headerarray[34] = i;
                                                    break;
                                                case "prv_wk_sell_thru_all_2":
                                                    headerarray[35] = i;
                                                    break;
                                                case "prv_wk_sell_thru_all_1":
                                                    headerarray[36] = i;
                                                    break;
                                                case "total_sell_thru_all":
                                                    headerarray[37] = i;
                                                    break;
                                                case "average_sell_thru_all":
                                                    headerarray[38] = i;
                                                    break;
                                                case "gross_fcst_cw":
                                                    headerarray[39] = i;
                                                    break;
                                                case "gross_fcst_wk1":
                                                    headerarray[40] = i;
                                                    break;
                                                case "gross_fcst_wk2":
                                                    headerarray[41] = i;
                                                    break;
                                                case "gross_fcst_wk3":
                                                    headerarray[42] = i;
                                                    break;
                                                case "gross_fcst_wk4":
                                                    headerarray[43] = i;
                                                    break;
                                                case "gross_fcst_wk5":
                                                    headerarray[44] = i;
                                                    break;
                                                case "gross_fcst_wk6":
                                                    headerarray[45] = i;
                                                    break;
                                                case "gross_fcst_wk7":
                                                    headerarray[46] = i;
                                                    break;
                                                case "gross_fcst_wk8":
                                                    headerarray[47] = i;
                                                    break;
                                                case "gross_fcst_wk9":
                                                    headerarray[48] = i;
                                                    break;
                                                case "gross_fcst_wk10":
                                                    headerarray[49] = i;
                                                    break;
                                                case "gross_fcst_wk11":
                                                    headerarray[50] = i;
                                                    break;
                                                case "gross_fcst_wk12":
                                                    headerarray[51] = i;
                                                    break;
                                                case "total_gross_fc":
                                                    headerarray[52] = i;
                                                    break;
                                                case "open_po":
                                                    headerarray[53] = i;
                                                    break;
                                                case "onhand":
                                                    headerarray[54] = i;
                                                    break;
                                                case "safety_stock":
                                                    headerarray[55] = i;
                                                    break;
                                                case "bklg_retail":
                                                    headerarray[56] = i;
                                                    break;
                                                case "bklg_online":
                                                    headerarray[57] = i;
                                                    break;
                                                case "bklg_reseller":
                                                    headerarray[58] = i;
                                                    break;
                                                case "total_backlog":
                                                    headerarray[59] = i;
                                                    break;
                                                case "net_fcst_cw":
                                                    headerarray[60] = i;
                                                    break;
                                                case "net_fcst_wk1":
                                                    headerarray[61] = i;
                                                    break;
                                                case "net_fcst_wk2":
                                                    headerarray[62] = i;
                                                    break;
                                                case "net_fcst_wk3":
                                                    headerarray[63] = i;
                                                    break;
                                                case "net_fcst_wk4":
                                                    headerarray[64] = i;
                                                    break;
                                                case "net_fcst_wk5":
                                                    headerarray[65] = i;
                                                    break;
                                                case "net_fcst_wk6":
                                                    headerarray[66] = i;
                                                    break;
                                                case "net_fcst_wk7":
                                                    headerarray[67] = i;
                                                    break;
                                                case "net_fcst_wk8":
                                                    headerarray[68] = i;
                                                    break;
                                                case "net_fcst_wk9":
                                                    headerarray[69] = i;
                                                    break;
                                                case "net_fcst_wk10":
                                                    headerarray[70] = i;
                                                    break;
                                                case "net_fcst_wk11":
                                                    headerarray[71] = i;
                                                    break;
                                                case "net_fcst_wk12":
                                                    headerarray[72] = i;
                                                    break;
                                                case "total_net_fc":
                                                    headerarray[73] = i;
                                                    break;
                                                case "net_commits_cw":
                                                    headerarray[74] = i;
                                                    break;
                                                case "net_commits_wk1":
                                                    headerarray[75] = i;
                                                    break;
                                                case "net_commits_wk2":
                                                    headerarray[76] = i;
                                                    break;
                                                case "net_commits_wk3":
                                                    headerarray[77] = i;
                                                    break;
                                                case "net_commits_wk4":
                                                    headerarray[78] = i;
                                                    break;
                                                case "net_commits_wk5":
                                                    headerarray[79] = i;
                                                    break;
                                                case "net_commits_wk6":
                                                    headerarray[80] = i;
                                                    break;
                                                case "net_commits_wk7":
                                                    headerarray[81] = i;
                                                    break;
                                                case "net_commits_wk8":
                                                    headerarray[82] = i;
                                                    break;
                                                case "net_commits_wk9":
                                                    headerarray[83] = i;
                                                    break;
                                                case "net_commits_wk10":
                                                    headerarray[84] = i;
                                                    break;
                                                case "net_commits_wk11":
                                                    headerarray[85] = i;
                                                    break;
                                                case "net_commits_wk12":
                                                    headerarray[86] = i;
                                                    break;
                                                case "vendor_comments":
                                                    headerarray[87] = i;
                                                    break;
                                                case "report_week":
                                                    headerarray[88] = i;
                                                    break;
                                                case "report_date":
                                                    headerarray[89] = i;
                                                    break;

                                            }
                                            values = values + val + ",";
                                            headerColumnCount++;
                                        }

                                    }
                                    else
                                    {
                                        if (headerCount != 0)
                                        {
                                            if (!skipRows.Contains(i))
                                            {


                                                if (val != "")
                                                {
                                                    val = val.Replace("'", "").Replace("\"", "");
                                                }
                                                try
                                                {
                                                    for (int i1 = 0; i1 < headerarray.Length; i1++)
                                                    {
                                                        if (headerarray[i1] == i)
                                                        {
                                                            itemArray[i1] = val;
                                                        }
                                                    }

                                                }
                                                catch (Exception ex)
                                                {
                                                    bool alert = true;
                                                }
                                            }
                                        }


                                    }
                                }
                                string loadString = string.Format(@"{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44},{45},{46},{47},{48},{49},{50},{51},{52},{53},{54},{55},{56},{57},{58},{59},{60},{61},{62},{63},{64},{65},{66},{67},{68},{69},{70},{71},{72},{73},{74},{75},{76},{77},{78},{79},{80},{81},{82},{83},{84},{85},{86},{87},{88},{89}",
                                                       itemArray[0], itemArray[1], itemArray[2], itemArray[3], itemArray[4], itemArray[5], itemArray[6], itemArray[7], itemArray[8], itemArray[9], itemArray[10], itemArray[11], itemArray[12], itemArray[13], itemArray[14], itemArray[15], itemArray[16], itemArray[17], itemArray[18], itemArray[19], itemArray[20], itemArray[21], itemArray[22], itemArray[23], itemArray[24], itemArray[25], itemArray[26], itemArray[27], itemArray[28], itemArray[29], itemArray[30], itemArray[31], itemArray[32], itemArray[33], itemArray[34], itemArray[35], itemArray[36], itemArray[37], itemArray[38], itemArray[39], itemArray[40], itemArray[41], itemArray[42], itemArray[43], itemArray[44], itemArray[45], itemArray[46], itemArray[47], itemArray[48], itemArray[49], itemArray[50], itemArray[51], itemArray[52], itemArray[53], itemArray[54], itemArray[55], itemArray[56], itemArray[57], itemArray[58], itemArray[59], itemArray[60], itemArray[61], itemArray[62], itemArray[63], itemArray[64], itemArray[65], itemArray[66], itemArray[67], itemArray[68], itemArray[69], itemArray[70], itemArray[71], itemArray[72], itemArray[73], itemArray[74], itemArray[75], itemArray[76], itemArray[77], itemArray[78], itemArray[79], itemArray[80], itemArray[81], itemArray[82], itemArray[83], itemArray[84], itemArray[85], itemArray[86], itemArray[87], itemArray[88], itemArray[89]);
                                DataRow row = SqlBulkLoader.parseValuesIntoDataRow(table.NewRow(), loadString, ',');
                                for (int ir = 0; ir < 89; ir++)
                                {
                                    if (row[ir].ToString() == "null")
                                    {
                                        row[ir] = DBNull.Value;
                                    }
                                }
                                if (headerCount != 0)
                                {
                                    bcp.LoadItem(row);
                                }
                                headerCount++;
                            }
                        }
                        bcp.Flush();
                        conn.Close();
                        if (int.Parse(Common.runSQLScalar(@"select count(*) from productdataloader_apple_forecastcommits_tempload_processing 
where coalesce(ExertisReportID,'')='' or coalesce(Plant,'')='' or coalesce(article,'')=''").ToString()) > 0)
                        {
                            throw new Exception("Error 1 -ExertisreportID,Plant or article missing please ensure these are populated for every row");
                        }
                        if (int.Parse(Common.runSQLScalar(@"select count(*) from (
select Article from productdataloader_apple_forecastcommits_tempload_processing 
group by article,plant,ExertisReportID
having count(Report_Date)>1) a").ToString()) > 0)
                        {
                            throw new Exception("Error 2 -Duplicate ExertisReportID, Plant and article rows found");
                        }
                        if (int.Parse(Common.runSQLScalar(@"select count(*) from (
select distinct exertisreportid from productdataloader_apple_forecastcommits_tempload_processing) a").ToString()) > 1)
                        {
                            throw new Exception("Error 3 -Multiple ExertisReportID's found please ensure only one reportid is used in the entire file");
                        }
                        if (int.Parse(Common.runSQLScalar(@"select count(*) from productdataloader_apple_forecastcommits_tempload_processing where exertisreportid not in (select reportid from mse_appleforecastcommitreports)").ToString()) > 1)
                        {
                            throw new Exception("Error 4 -Unknown ExertisReportID in file");
                        }
                        if (int.Parse(Common.runSQLScalar(@"select count(*) from productdataloader_apple_forecastcommits_tempload_processing 
pro left outer join MSE_AppleForecastCommitReportLines fcl on pro.ExertisReportid=fcl.Reportid
and pro.Plant=fcl.Plant
and pro.Article=fcl.AppleCode
where fcl.reportid is null").ToString()) > 1)
                        {
                            throw new Exception("Error 5 - Could not match ExertisReportID, Plant and Article to a report line");
                        }

                    }
                    Common.runSQLNonQuery("delete from fc_processing");
                    string filename = Common.runSQLScalar(@"select substring(FileName,0,39) from MSE_AppleForecastCommitReports where reportid in(select exertisreportid from productdataloader_apple_forecastcommits_tempload_processing)").ToString();
                    filename+=".csv";
                    Common.runSQLNonQuery(@"declare @reportid varchar(10)
select @reportid =  exertisreportid from productdataloader_apple_forecastcommits_tempload_processing
delete from fc_processing
insert into fc_processing
select  fc.* from [vw_forecastcommitresponse] fc where exertisreportid=@reportid");


                    string f = Common.dataTableToTextFile(Common.runSQLDataset(@"select fc.[Region]
      ,fc.[Plant]
      ,fc.[PlannerCode]
      ,fc.[Article]
      ,fc.[Vendor_Mat_Num]
      ,fc.[Description]
      ,fc.[Vendornumber]
      ,fc.[Vendorname]
      ,fc.[Prv_Wk_Bookings_4]
      ,fc.[Prv_Wk_Bookings_3]
      ,fc.[Prv_Wk_Bookings_2]
      ,fc.[Prv_Wk_Bookings_1]
      ,fc.[Total_Bookings]
      ,fc.[Average_Bookings]
      ,fc.[Prv_Wk_Sell_Thru_Retail_4]
      ,fc.[Prv_Wk_Sell_Thru_Retail_3]
      ,fc.[Prv_Wk_Sell_Thru_Retail_2]
      ,fc.[Prv_Wk_Sell_Thru_Retail_1]
      ,fc.[Total_Sell_Thru_Retail]
      ,fc.[Average_Sell_Thru_Retail]
      ,fc.[Prv_Wk_Sell_Thru_Online_4]
      ,fc.[Prv_Wk_Sell_Thru_Online_3]
      ,fc.[Prv_Wk_Sell_Thru_Online_2]
      ,fc.[Prv_Wk_Sell_Thru_Online_1]
      ,fc.[Total_Sell_Thru_Online]
      ,fc.[Average_Sell_Thru_Online]
      ,fc.[Prv_Wk_Sell_Thru_Rslr_4]
      ,fc.[Prv_Wk_Sell_Thru_Rslr_3]
      ,fc.[Prv_Wk_Sell_Thru_Rslr_2]
      ,fc.[Prv_Wk_Sell_Thru_Rslr_1]
      ,fc.[Total_Sell_Thru_Rslr]
      ,fc.[Average_Sell_Thru_Rslr]
      ,fc.[Prv_Wk_Sell_Thru_All_4]
      ,fc.[Prv_Wk_Sell_Thru_All_3]
      ,fc.[Prv_Wk_Sell_Thru_All_2]
      ,fc.[Prv_Wk_Sell_Thru_All_1]
      ,fc.[Total_Sell_Thru_All]
      ,fc.[Average_Sell_Thru_All]
      ,fc.[Gross_Fcst_cw]
      ,fc.[Gross_Fcst_wk1]
      ,fc.[Gross_Fcst_wk2]
      ,fc.[Gross_Fcst_wk3]
      ,fc.[Gross_Fcst_wk4]
      ,fc.[Gross_Fcst_wk5]
      ,fc.[Gross_Fcst_wk6]
      ,fc.[Gross_Fcst_wk7]
      ,fc.[Gross_Fcst_wk8]
      ,fc.[Gross_Fcst_wk9]
      ,fc.[Gross_Fcst_wk10]
      ,fc.[Gross_Fcst_wk11]
      ,fc.[Gross_Fcst_wk12]
      ,fc.[Total_Gross_FC]
      ,fc.[Open_PO]
      ,fc.[OnHand]
      ,fc.[Safety_Stock]
      ,fc.[Bklg_Retail]
      ,fc.[Bklg_Online]
      ,fc.[Bklg_Reseller]
      ,fc.[Total_Backlog]
      ,fc.[Net_Fcst_cw]
      ,fc.[Net_Fcst_wk1]
      ,fc.[Net_Fcst_wk2]
      ,fc.[Net_Fcst_wk3]
      ,fc.[Net_Fcst_wk4]
      ,fc.[Net_Fcst_wk5]
      ,fc.[Net_Fcst_wk6]
      ,fc.[Net_Fcst_wk7]
      ,fc.[Net_Fcst_wk8]
      ,fc.[Net_Fcst_wk9]
      ,fc.[Net_Fcst_wk10]
      ,fc.[Net_Fcst_wk11]
      ,fc.[Net_Fcst_wk12]
      ,fc.[Total_Net_FC],
coalesce(pro.[Net_Commits_cw],fc.[Net_Commits_cw]) as [Net_Commits_cw],coalesce(pro.[Net_Commits_wk1],fc.[Net_Commits_wk1]) as [Net_Commits_wk1],coalesce(pro.[Net_Commits_wk2],fc.[Net_Commits_wk2]) as [Net_Commits_wk2],coalesce(pro.[Net_Commits_wk3],fc.[Net_Commits_wk3]) as [Net_Commits_wk3],coalesce(pro.[Net_Commits_wk4],fc.[Net_Commits_wk4]) as [Net_Commits_wk4],coalesce(pro.[Net_Commits_wk5],fc.[Net_Commits_wk5]) as [Net_Commits_wk5],coalesce(pro.[Net_Commits_wk6],fc.[Net_Commits_wk6]) as [Net_Commits_wk6],coalesce(pro.[Net_Commits_wk7],fc.[Net_Commits_wk7]) as [Net_Commits_wk7],coalesce(pro.[Net_Commits_wk8],fc.[Net_Commits_wk8]) as [Net_Commits_wk8],coalesce(pro.[Net_Commits_wk9],fc.[Net_Commits_wk9]) as [Net_Commits_wk9],coalesce(pro.[Net_Commits_wk10],fc.[Net_Commits_wk10]) as [Net_Commits_wk10],coalesce(pro.[Net_Commits_wk11],fc.[Net_Commits_wk11]) as [Net_Commits_wk11],coalesce(pro.[Net_Commits_wk12],fc.[Net_Commits_wk12]) as [Net_Commits_wk12],coalesce(pro.[Vendor_Comments],fc.[Vendor_Comments]) as [Vendor_Comments]

,fc.[Report_Week] as [Report_Week]
,fc.[Report_Date] as [Report_Date] 


from fc_processing fc 
left outer join productdataloader_apple_forecastcommits_tempload_processing pro on pro.ExertisReportid=fc.ExertisReportid
and pro.Plant=fc.Plant
and pro.Article=fc.Article").Tables[0], ",", "\r\n", true);
                    Common.runSQLNonQuery(string.Format("update mse_appleoracleimportmanagement set lastfilename='{0}',lastfiledate=getdate(),username='{1}' where reportid=10", Path.GetFileName(fupFCUpload.FileName), HttpContext.Current.User.Identity.Name.ToString()));
                    string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                    if (File.Exists(filePathLocale + filename))
                    {
                        File.Delete(filePathLocale + filename);
                    }
                    File.AppendAllText(filePathLocale + filename, f, Encoding.Default);
                    File.AppendAllText(filePathLocale + filename.Replace(".csv","")+"_"+Common.timestamp()+".csv", f, Encoding.Default);
                    string ftpHost = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftphost'").ToString();
                    string ftpPort = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftpport'").ToString();
                    string ftpUsername = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftpusername'").ToString();
                    string ftpPassword = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftppassword'").ToString();
                    string ftpDirectory = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='FC directory'").ToString();
                    try
                    {
                        Renci.SshNet.SftpClient client = new Renci.SshNet.SftpClient(ftpHost, int.Parse(ftpPort), ftpUsername, ftpPassword);
                        client.Connect();
                        client.ChangeDirectory(ftpDirectory);
                        Stream str = fupFCUpload.FileContent;
                        using (FileStream fStream = new FileStream(filePathLocale + filename, FileMode.Open))
                        {
                            client.BufferSize = 4 * 1024; // bypass Payload error large files
                            client.UploadFile(fStream, filename);
                        }
                        sqlDSFCLatest.DataBind();
                        gvFcUploaded.DataBind();
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, The Forecast Commit file has been uploaded to the Apple SFTP server. The following fields were pulled and used from the uploaded file " + values.Substring(0,values.Length-1)  + "');", true);
                    }
                    catch (Exception ex)
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, Please try again');", true);
                    }

                }

                   


                catch (Exception ex)
                {
                    if (ex.Message.Substring(0, 7) != "Error 1" && ex.Message.Substring(0, 7) != "Error 2" && ex.Message.Substring(0, 7) != "Error 3" && ex.Message.Substring(0, 7) != "Error 4" && ex.Message.Substring(0, 7) != "Error 5")
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, Please try again');", true);
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('" +ex.Message  + "');", true);
                    }
                }
            }

        }
        protected void gvLastImportedOracle_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                System.Web.UI.WebControls.Image theImage = (System.Web.UI.WebControls.Image)e.Row.FindControl("imgImportStatus");
                if (int.Parse(DataBinder.Eval(e.Row.DataItem, "dateDiffImport").ToString()) > 24)
                {
                    theImage.ImageUrl = "~/images/x.png";
                }
                else
                {
                    theImage.ImageUrl = "~/images/tick.png";
                }
            }

        }
        protected void btnUploadFC_Click(object sender, EventArgs e)
        {
            if (fupFCUpload.HasFile)
            {
                Common.log("Apple processing- now beginning forecast report pickup from Apple SFTP");
                string localDir = @"c:\appleprocessing\tempdir\";
                string ftpHost = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftphost'").ToString();
                string ftpPort = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftpport'").ToString();
                string ftpUsername = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftpusername'").ToString();
                string ftpPassword = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftppassword'").ToString();

                try
                {
                    Renci.SshNet.SftpClient client = new Renci.SshNet.SftpClient(ftpHost, int.Parse(ftpPort), ftpUsername, ftpPassword);
                    client.Connect();
                    client.ChangeDirectory("/to_apple/");
                    Stream str = fupFCUpload.FileContent;
                    using (var fileStream = str)
                    {
                        client.BufferSize = 4 * 1024; // bypass Payload error large files
                        client.UploadFile(fileStream, Path.GetFileName(fupFCUpload.FileName));
                    }
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, The Forecast Commit file has been uploaded to the Apple SFTP server');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, Please try again');", true);
                }
            }


        }


        private void createForecastCommitResponse(int reportID, bool testing)
        {
            string filename = Common.runSQLScalar(string.Format("select substring(FileName,0,39) from MSE_AppleForecastCommitReports where reportid={0}", (object)reportID)).ToString() + ".csv";
            this.runReport(string.Format("sp_appleforecastcommitresponse {0}\r\n", (object)reportID), filename);
        }
        protected void gvFC_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                string date = DataBinder.Eval(e.Row.DataItem, "ReportDate").ToString();
                DateTime dt = Convert.ToDateTime(date);
                TimeSpan ts = (DateTime.Now - dt);


                System.Web.UI.WebControls.Image theImage = (System.Web.UI.WebControls.Image)e.Row.FindControl("imgImportStatus");
                if (ts.TotalHours > 100)
                {
                    theImage.ImageUrl = "~/images/x.png";
                }
                else
                {
                    theImage.ImageUrl = "~/images/tick.png";
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
    }
}