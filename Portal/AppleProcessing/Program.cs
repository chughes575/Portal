using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Net.Mail;

using Renci;
using MSE_Common;
using AppleProcessing.ServiceReference2;
namespace AppleProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "1":
                    getForecastCommitFilesSFTP();
                    break;
                case "2":
                    generatePOSuggestionsEmail();
                    break;
                case "3":
                    processVendorCommitEmails();
                    break;

            }
        }
        public static void processVendorCommitEmails()
        {



            Common.log("Processing Apple Vendor Commit Emails");
            int outstandingCount = int.Parse(Common.runSQLScalar(@"select count(fcr.ReportID)
  from MSE_AppleForecastCommitReports fcr 
inner join mse_applevendorprocessed vp on vp.ReportID=fcr.ReportID
 where coalesce(vp.Processed,0)=0 and datediff(day,ReportDate,getdate())<=7 and fcr.reportid = (select top 1 reportid from MSE_AppleForecastCommitReports order by ReportDate desc)").ToString());

            string reportID = "";
            string reportname = "";
            if (outstandingCount == 0)
            {
                Common.log("No outstanding vendor emails to be processed");
            }
            else
            {
                try
                {


                    DataRow dr = Common.runSQLRow(@" select distinct fcr.ReportID,substring(fcr.FileName,0,39) as [Filename],fcr.ReportDate,fcr.Processed,fcr.DiscrepancyReport,coalesce(fcr.VendorReportProcessed,0) as VendorReportProcessed from MSE_AppleForecastCommitReports fcr 
inner join mse_applevendorprocessed vp on vp.ReportID=fcr.ReportID
 where coalesce(vp.Processed,0)=0 and datediff(day,ReportDate,getdate())<=7 and fcr.reportid = (select top 1 reportid from MSE_AppleForecastCommitReports order by ReportDate desc)");


                    dr = Common.runSQLRow(@" select distinct fcr.ReportID,substring(fcr.FileName,0,39) as [Filename],fcr.ReportDate,fcr.Processed,fcr.DiscrepancyReport,coalesce(fcr.VendorReportProcessed,0) as VendorReportProcessed from MSE_AppleForecastCommitReports fcr 
inner join mse_applevendorprocessed vp on vp.ReportID=fcr.ReportID
 where fcr.reportid in (
49,
50
)");
                    reportID = dr[0].ToString();
                    reportname = dr[1].ToString();
                }
                catch
                {
                    return;
                }



                DataRowCollection drc = Common.runSQLRows(string.Format(@"select vnd.* from MSE_AppleForecastCommitReports fcr 
inner join mse_applevendorprocessed vp on vp.ReportID=fcr.ReportID
inner join mse_applevendors vnd on vnd.VendorID=vp.VendorID

 where coalesce(vp.Processed,0)=0 and datediff(day,ReportDate,getdate())<=7 and fcr.reportid = {0}", reportID));


                drc = Common.runSQLRows(string.Format(@"select vnd.* from MSE_AppleForecastCommitReports fcr 
inner join mse_applevendorprocessed vp on vp.ReportID=fcr.ReportID
inner join mse_applevendors vnd on vnd.VendorID=vp.VendorID

 where vnd.VendorID=25 and fcr.reportid = {0}", reportID));
                foreach (DataRow drVendor in drc)
                {
                    int prodCount = int.Parse(Common.runSQLScalar(string.Format(@"select count(*) from MSE_AppleForecastCommitReportLines fcl 
	 inner join MSE_AppleProductMapping apm on apm.AppleCode=fcl.AppleCode
	 inner join mse_oracleproducts op on op.Product_code=apm.exertis_part_number
	 where fcl.ReportID={0} and 

op.manufacturer in (select  coalesce(VendorValue,vendorname)
	 from mse_applevendors vnd left outer join mse_applevendorvalues val on val.VendorID=vnd.VendorID
	 where vnd.VendorID={1}) ", reportID, drVendor[0].ToString())).ToString());
                    if (prodCount == 0)
                        continue;

                    List<string> fileList = new List<string>(); string filePath = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\apple\In\Temp files\";
                    string fileName = reportname + "_" + drVendor[1].ToString() + ".csv";
                    DataSet ds = Common.runSQLDataset(string.Format("exec [sp_appleVendorPOEmail] {0},{1}", drVendor[0].ToString(), reportID));

                    string forecastCommitLinesContent = Common.dataTableToTextFile(ds.Tables[0], ",", "\r\n", true);
                    if (File.Exists(filePath + fileName))
                        File.Delete(filePath + fileName);

                    File.AppendAllText(filePath + fileName, forecastCommitLinesContent);
                    fileList.Add(filePath + fileName);

                    try
                    {
                        string vendorID = drVendor[0].ToString();
                        string vendorName = drVendor[1].ToString();
                        string recipients = Common.runSQLScalar(string.Format(@"Select Left(Main.VendorEmailAddress,Len(Main.VendorEmailAddress)-1) As Recipients
From
    (
        Select distinct ST2.VendorID, 
            (
                Select ST1.VendorEmailAddress + ',' AS [text()]
                From mse_applevendoremails ST1
                Where ST1.VendorID = ST2.VendorID
                ORDER BY ST1.VendorID
                For XML PATH ('')
            ) [VendorEmailAddress]
        From mse_applevendoremails ST2
    ) [Main]


	where vendorid={0}", vendorID)).ToString();

                        //email stuff
                        string[] emailAddresses = recipients.Split(',');

                        MailMessage mail = new MailMessage();
                        SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                        mail.From = new System.Net.Mail.MailAddress("chris.hughes@exertis.co.uk");
                        mail.Bcc.Add("chris.hughes@exertis.co.uk");
                        mail.Bcc.Add("alina.gavenyte@exertis.co.uk");
                        mail.To.Add("Apple1@exertis.co.uk");
                        for (int i = 0; i < emailAddresses.Length; i++)
                        {
                            mail.To.Add(emailAddresses[i]);
                        }
                        mail.Subject = string.Format("Apple Weekly Forecast Commit File- Action Required", 0);
                        mail.Body = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='VendorEmailBody'").ToString().Replace("[VENDORNAME]", vendorName);

                        mail.IsBodyHtml = true;
                        AlternateView av = AlternateView.CreateAlternateViewFromString(Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='VendorEmailBodyHTML'").ToString().Replace("[VENDORNAME]", vendorName), null, "text/html");
                        mail.AlternateViews.Add(av);
                        foreach (string file in fileList)
                        {
                            try
                            {
                                System.Net.Mail.Attachment attachmentStockUpFile;
                                attachmentStockUpFile = new System.Net.Mail.Attachment(file);
                                mail.Attachments.Add(attachmentStockUpFile);

                            }
                            catch
                            {
                            }
                        }
                        SmtpServer.Port = 587;
                        SmtpServer.Credentials = new System.Net.NetworkCredential(Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString(), Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString());
                        SmtpServer.EnableSsl = true;
                        string updateSQL = string.Format(@"update mse_applevendorprocessed set Processed=1 where vendorid={0} and reportid={1}", vendorID, reportID);
                        try
                        {

                            if (mail.Attachments.Count == 0)
                                throw new Exception("No attachments aborting!!!");

                            SmtpServer.Send(mail);
                            Common.runSQLNonQuery(updateSQL);

                        }
                        catch (Exception ex)
                        {

                        }



                    }
                    catch (Exception ex1)
                    {

                    }

                }

            }


        }
        public static void deleteFileSFTP()
        {
            Common.log("Apple processing- now beginning forecast report pickup from Apple SFTP");

            string localDir = @"c:\appleprocessing\tempdir\";
            string ftpHost = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftphost'").ToString();
            string ftpPort = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftpport'").ToString();
            string ftpUsername = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftpusername'").ToString();
            string ftpPassword = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftppassword'").ToString();

            Renci.SshNet.SftpClient client = new Renci.SshNet.SftpClient(ftpHost, int.Parse(ftpHost), ftpUsername, ftpPassword);
            IFTP ftpClient = new FTP("ftp.msent.co.uk", "/In/Apple Forecast/processed/", "apple", "Apple1");
            

            
        }
                

        public static void getForecastCommitFilesSFTP()
        {
            Common.log("Apple processing- now beginning forecast report pickup from Apple SFTP");
            try
            {
                string localDir = @"c:\appleprocessing\tempdir\";
                string ftpHost = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftphost'").ToString();
                string ftpPort = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftpport'").ToString();
                string ftpUsername = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftpusername'").ToString();
                string ftpPassword = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='ftppassword'").ToString();
                try
                {
                    Renci.SshNet.SftpClient client = new Renci.SshNet.SftpClient(ftpHost, int.Parse(ftpHost), ftpUsername, ftpPassword);
                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/In/Apple Forecast/processed/", "apple", "Apple1");
                    client.Connect();

                    IEnumerable<Renci.SshNet.Sftp.SftpFile> lst = client.ListDirectory("/from_apple/");
                    foreach (Renci.SshNet.Sftp.SftpFile file in lst)
                    {
                        int count = 0;


                        try
                        {
                            if (file.Length == 0)
                                continue;
                            count = int.Parse(Common.runSQLScalar("select count(*) from ProductDataLoader_FtpDownloadTimes where filename='" + file.Name + "' and ftpserver='sftp://17.151.38.84'").ToString());
                            if (count == 0)
                            {




                                using (var file1 = File.OpenWrite(localDir + file.Name))
                                {
                                    client.DownloadFile("/from_apple/" + file.Name, file1);
                                }

                                if (File.Exists(localDir + file.Name))
                                {
                                    try
                                    {
                                        string reportData = File.ReadAllText(localDir + file.Name, Encoding.Default);
                                        reportData = reportData.Replace(",\n", ",\"\"\n");
                                        string newFileName = localDir+Path.GetFileNameWithoutExtension(file.Name) + "_amended" + Common.timestamp() + ".csv";
                                        File.AppendAllText(newFileName, reportData, Encoding.Default);
                                        if (File.Exists(newFileName))
                                        {
                                            ftpClient.uploadFile(newFileName);
                                            Common.runSQLNonQuery("insert into ProductDataLoader_FtpDownloadTimes values ('" + file.Name + "','sftp://17.151.38.84',getdate())");
                                        }
                                        
                                    }
                                    catch(Exception ex)
                                    {
                                    }
                                }

                            }
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                { }
            }
            catch
            {
            }
        }

        

        public static void generatePOSuggestionsEmail()
        {


            if (int.Parse(Common.runSQLScalar("select count(*) from mse_applescheduling where jobtype=1 and cast(getdate() as date)=JobDate and datepart(weekday,getdate())=2").ToString()) == 0)
            {


                List<string> fileList = new List<string>();
                fileList = new List<string>();
                string fileName = string.Format(@"PoSuggestions_Consolidated_{0}.csv", Common.timestamp());
                try
                {
                    DataSet vmiLines = Common.runSQLDataset(string.Format(@"select * from vw_ApplePoSuggestionsConsolidated", 0));
                    string vmiLinesContent = Common.dataTableToTextFile(vmiLines.Tables[0], ",", "\r\n", true);
                    string filePath = "C:\\AppleProcessing\\tempdir\\";
                    if (File.Exists(filePath + fileName))
                        File.Delete(filePath + fileName);

                    File.AppendAllText(filePath + fileName, vmiLinesContent);
                    fileList.Add(filePath + fileName);
                }
                catch { }
                DataRow drFC = Common.runSQLRow(@"select * from (
select *, ROW_NUMBER() OVER (ORDER BY reportdate DESC) AS row from MSE_AppleForecastCommitReports 
) a where a.row=1");
                DataRow drFC1 = Common.runSQLRow(@"select * from (
select *, ROW_NUMBER() OVER (ORDER BY reportdate DESC) AS row from MSE_AppleForecastCommitReports 
) a where a.row=2");
                DataRow drFC2 = Common.runSQLRow(@"select * from (
select *, ROW_NUMBER() OVER (ORDER BY reportdate DESC) AS row from MSE_AppleForecastCommitReports 
) a where a.row=3");




                string primaryFCReportFilname = drFC[2].ToString();
                string primaryFCReportTimeStamp = drFC[3].ToString();

                string FC1ReportFilname = drFC1[2].ToString();
                string FC1ReportTimeStamp = drFC1[3].ToString();

                string FC2ReportFilname = drFC2[2].ToString();
                string FC2ReportTimeStamp = drFC2[3].ToString();


                var bodyBuilder = new StringBuilder();


                bodyBuilder.AppendLine("Please find attached a copy of this weeks po Recommendations information on the reports used to generate this can be found below-");
                bodyBuilder.AppendLine("");
                bodyBuilder.AppendLine(string.Format("Forecast Report used: {0} processed on {1}", primaryFCReportFilname, primaryFCReportTimeStamp));
                bodyBuilder.AppendLine("");
                //bodyBuilder.AppendLine(string.Format("Comparison Report (last week) used: {0} processed on {1}", FC1ReportFilname, FC1ReportFilname));
                //bodyBuilder.AppendLine(string.Format("Comparison Report (previous week) used: {0} processed on {1}", FC2ReportFilname, FC2ReportTimeStamp));
                if (fileList.Count > 0)
                {
                    //email stuff
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                    mail.From = new System.Net.Mail.MailAddress("chris.hughes@exertis.co.uk");

                    //mail.CC.Add("alina.gavenyte@exertis.co.uk");
                    mail.To.Add("chris.hughes@exertis.co.uk");

                    mail.Subject = string.Format("Apple PO Recommendations {0}",Common.timestamp());
                    mail.Body = bodyBuilder.ToString();

                    foreach (string file in fileList)
                    {
                        try
                        {
                            System.Net.Mail.Attachment attachmentStockUpFile;
                            attachmentStockUpFile = new System.Net.Mail.Attachment(file);
                            mail.Attachments.Add(attachmentStockUpFile);

                        }
                        catch
                        {
                        }
                    }
                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new System.Net.NetworkCredential("chris.hughes@exertis.co.uk", "Shetland992");
                    SmtpServer.EnableSsl = true;

                    try
                    {

                        SmtpServer.Send(mail);
                    }
                    catch (Exception ex)
                    {

                    }

                }

            }
        }
    }

}
