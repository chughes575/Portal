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
                    uploadForecastCommit();
                    break;
            }
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

        public static void uploadForecastCommit()
        {
            Common.log("Apple processing- now beginning forecast report pickup from Apple SFTP");
                string localDir = @"c:\appleprocessing\tempdir\";
                try
                {
                    Renci.SshNet.SftpClient client = new Renci.SshNet.SftpClient("17.151.38.84", 22, "00468801P", "ml74wpiqBO");
                    client.Connect();
                    client.ChangeDirectory("/to_apple/");
                    using (var fileStream = new FileStream(@"C:\Users\Chris\Documents\Test.fmt", FileMode.Open))
                    {
                        client.BufferSize = 4 * 1024; // bypass Payload error large files
                        client.UploadFile(fileStream, Path.GetFileName(@"C:\Users\Chris\Documents\Test.fmt"));
                    }
                    
                }
                catch (Exception ex)
                { }
            
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
