using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Data.SqlClient;
using System.Collections;
using MSE_Common;
using System.Data;
using System.Net.Mail;
using ActiveUp.Net.Mail;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
namespace BpcExportModule
{
    class Program
    {
        static void Main(string[] args)
        {

            switch (int.Parse(args[0].ToString()))
            {
                case 1:
                    runBpcForecastExport();
                    break;
            }
        }
        public static void runBpcForecastExport()
        {
            Common.log("Running BPC Weekly Export");
            int outstandingCount = int.Parse(Common.runSQLScalar(@"select count(*) from MSE_BPCWeeklyExports where datepart(week,getdate())=WeekNo").ToString());

            string fileName = "BPC_Exertis_Export_" + Common.timestamp() + ".csv";
            if (outstandingCount == 0)
            {

                int outstandingReportCount = int.Parse(Common.runSQLScalar(@"select count(*) from (
select 'RETAIL_PLAN' AS CATEGORY,bpc.Customer_Code AS CUSTOMER_CODE,bpc.Customer_S AS CUSTOMER_S,'CC_GB01' AS [C_CODE],'PR_' + catno as PROUDUCT,'RP_INTAKE_U' as RP_ACCOUNT,	'RP_CUS' as RP_AUDITTRAIL,	'BRANDVAL' as BRAND,	'2016.P01.W'+case when datepart(week,getdate())<10 then '0' else '' end + cast(datepart(week,getdate()) as varchar(2)) as TIME_WKS,ForecastWeek1 as [AMOUNT]
 from MSE_PortalForecastingDataCurrent
 fdc inner join mse_portalcustomers pc on pc.customerid=fdc.CustomerID
 left outer join MSE_BPCExports bpc on bpc.CustomerID=pc.CustomerID
where bpc.ForecastExportEnabled=1 and ForecastWeek1 is not null

union
select 'RETAIL_PLAN' AS CATEGORY,bpc.Customer_Code AS CUSTOMER_CODE,bpc.Customer_S AS CUSTOMER_S,'CC_GB01' AS [C_CODE],'PR_' + stk.CustomerSku as PROUDUCT,'RP_STOCK_U' as RP_ACCOUNT,	'RP_CUS' as RP_AUDITTRAIL,	'BRANDVAL' as BRAND,	'2016.P01.W'+case when datepart(week,getdate())<10 then '0' else '' end ++cast(datepart(week,getdate()) as varchar(2)) as TIME_WKS,sum(coalesce(stk.stockqty,0)) as [AMOUNT]
 from MSE_Retailerconsignmentstockfigures stk
  inner join mse_portalcustomers pc on pc.customerid=stk.CustomerID
 left outer join MSE_BPCExports bpc on bpc.CustomerID=pc.CustomerID
where bpc.StockExportEnabled=1 
group by bpc.Customer_Code,bpc.Customer_S,stk.CustomerSku
union

select 'RETAIL_PLAN' AS CATEGORY,bpc.Customer_Code AS CUSTOMER_CODE,bpc.Customer_S AS CUSTOMER_S,'CC_GB01' AS [C_CODE],'PR_' + stk.CustomerSku as PROUDUCT,'RP_STOCK_U' as RP_ACCOUNT,	'RP_CUS' as RP_AUDITTRAIL,	'BRANDVAL' as BRAND,	'2016.P01.W'+case when datepart(week,getdate())<10 then '0' else '' end ++cast(datepart(week,getdate()) as varchar(2)) as TIME_WKS,sum(coalesce(stk.stockqty,0)) as [AMOUNT]
 from MSE_PortalEposFigures stk
  inner join mse_portalcustomers pc on pc.customerid=stk.CustomerID
 left outer join MSE_BPCExports bpc on bpc.CustomerID=pc.CustomerID
where coalesce(bpc.SalesExportEnabled,0) in (1,0) 
and cast(stk.eposdate as date)=cast(dateadd(day,((datepart(weekday,getdate())+6)*-1),getdate()) as date)
group by bpc.Customer_Code,bpc.Customer_S,stk.CustomerSku

) as a ").ToString());

                if (outstandingReportCount > 0)
                {



                    List<string> fileList = new List<string>(); string filePath = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\bpc\Out\WeeklyExports\";
                    DataSet ds = Common.runSQLDataset(string.Format("exec [sp_bpcfilecontents]", 0));

                    string bpcLinesContent = Common.dataTableToTextFile(ds.Tables[0], ",", "\r\n", true);
                    if (File.Exists(filePath + fileName))
                        File.Delete(filePath + fileName);

                    File.AppendAllText(filePath + fileName, bpcLinesContent);
                    fileList.Add(filePath + fileName);







                    string insertSQL = "insert into MSE_BPCWeeklyExports values (datepart(week,getdate()),'" + fileName + "',getdate())";
                    try
                    {

                        if (true)
                        {
                            Common.runSQLNonQuery(insertSQL);
                        }

                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
        }
        private static string downloadAttachment(ActiveUp.Net.Mail.MimePart att, string downloadPath, string vendorPath, string uid)
        {
            string filename = "";
            int count = 0;
            try
            {
                count = int.Parse(Common.runSQLScalar(string.Format("select count(*) from MSE_appleemailfiles where uid='{0}'", uid)).ToString());
                if (count == 0)
                {

                    string newFileName = @"";
                    filename = att.Filename;
                    string filenameNoExt = Path.GetFileNameWithoutExtension(filename);
                    string ext = Path.GetExtension(filename);
                    string insertSql = string.Format(@"insert into MSE_appleemailfiles
output inserted.ID 
values ('{0}','{1}','{2}')", uid, filename, att.ParentMessage.Date);

                    string id = Common.runSQLScalar(insertSql).ToString();
                    newFileName = filenameNoExt + "_uid_" + id + ext;
                    Common.runSQLNonQuery(string.Format(@"update MSE_appleemailfiles set filename='{0}' where id={1}", newFileName, id));
                    att.StoreToFile(downloadPath + vendorPath + @"\" + newFileName);
                    Console.WriteLine("File: " + newFileName + " downloaded");
                    filename = newFileName;
                }
                else
                {
                    filename = "";
                }


            }
            catch (Exception ex)
            {
                filename = "";
            }

            return filename;
        }
    }
}
