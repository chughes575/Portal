using System;
using System.Collections.Generic;
using System.Text;
using ActiveUp.Net.Mail;
using System.Threading;
using MSE_Common;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
namespace AppleInventoryPickup
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "1":
                    getInventoryEmailFiles();
                    break;
            }
        }

        public static void getInventoryEmailFiles()
        {
            
            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                Mailbox mailBox = client.SelectMailbox("inbox/invrpt");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {

                        if (i == mailBox.MessageCount)
                        {
                            bool alertme = true;
                        }
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            if (msg.Subject.Contains("Inventory Balance Report for UKDC MIP"))
                            {
                                continue;
                                foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                                {
                                    DateTime dateString = msg.Date;
                                    bool success = downloadAttachment(attachment, inventoryPath, "UK", msg.MessageId);


                                }
                            }
                            else if (msg.Subject.Contains("CZECH DC VMI REPORT VENDOR 00468801P"))
                            {
                                continue;
                                foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                                {
                                    DateTime dateString = msg.Date;
                                    bool success = downloadAttachment(attachment, inventoryPath, "CZ", msg.MessageId);
                                }
                            }
                            else if (msg.Subject.Contains("Inventory Balance Report for NLDC MIP"))
                            {
                                foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                                {
                                    DateTime dateString = msg.Date;
                                    bool success = downloadAttachment(attachment, inventoryPath, "NL", msg.MessageId);
                                }
                            }
                            else if (msg.Subject.Contains("Inventory Balance Report for AEDC-FG MIP"))
                            {
                                foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                                {
                                    DateTime dateString = msg.Date;
                                    bool success = downloadAttachment(attachment, inventoryPath, "UAE", msg.MessageId);
                                }
                            }
                            else if (msg.Subject.Contains("Hub-Stk Spreadsheet") && msg.BodyText.Text.ToLower().Contains("stock analysis for supplier: em"))
                            {
                                foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                                {

                                    DateTime dateString = msg.Date;
                                    bool success = downloadAttachment(attachment, inventoryPath, "IT", msg.MessageId);



                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        bool blah = true;
                    }
                }
            }
            catch (Exception ex)
            {
                bool blah = true;
            }
            processInventoryFiles();
            processOtherAppleFiles();
        }
        public static void processInventoryFiles()
        {
            string newFilePath = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\IT\\";
            string filePathUK = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\UK\\";
            string filePathUAE = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\UAE\\";
            string filePathNL = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\NL\\";

            try
            {

                Common.log("Processing Apple UK Inventory Balance reports");
                ArrayList filesInventoryUK = new ArrayList(Directory.GetFiles(filePathUK));
                foreach (string fileInventoryUK in filesInventoryUK)
                {
                    string reportData = File.ReadAllText(fileInventoryUK, Encoding.Default);
                    string result = Regex.Replace(reportData,
        @",(?=[^""]*""(?:[^""]*""[^""]*"")*[^""]*$)",
        String.Empty);
                    string newFileName = Path.GetFileNameWithoutExtension(fileInventoryUK) + "_amended" + Common.timestamp() + ".csv";
                    try
                    {
                        File.AppendAllText(filePathUK + "\\tempfiles\\" + newFileName, result, Encoding.Default);
                        File.Move(filePathUK + "\\tempfiles\\" + newFileName, filePathUK + "\\processed\\" + newFileName);
                        if (File.Exists(filePathUK + "\\original files\\" + Path.GetFileName(fileInventoryUK)))
                            File.Delete(filePathUK + "\\original files\\" + Path.GetFileName(fileInventoryUK));

                        File.Move(fileInventoryUK, filePathUK + "\\original files\\" + Path.GetFileName(fileInventoryUK));

                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.Move(fileInventoryUK, filePathUK + "\\rejects\\" + Path.GetFileName(fileInventoryUK));

                        }
                        catch
                        {
                            throw new Exception("error");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Common.log("Error processing Apple Inventory Balance reports: " + ex.Message);
            }

            try
            {
                Common.log("Processing Apple UAE Inventory Balance reports");
                ArrayList filesInventoryUAE = new ArrayList(Directory.GetFiles(filePathUAE));
                foreach (string fileInventoryUAE in filesInventoryUAE)
                {
                    string reportData = File.ReadAllText(fileInventoryUAE, Encoding.Default);
                    string result = Regex.Replace(reportData,
        @",(?=[^""]*""(?:[^""]*""[^""]*"")*[^""]*$)",
        String.Empty);
                    string newFileName = Path.GetFileNameWithoutExtension(fileInventoryUAE) + "_amended" + Common.timestamp() + ".csv";
                    try
                    {
                        File.AppendAllText(filePathUAE + "\\tempfiles\\" + newFileName, result, Encoding.Default);
                        File.Move(filePathUAE + "\\tempfiles\\" + newFileName, filePathUAE + "\\processed\\" + newFileName);

                        if (File.Exists(filePathUAE + "\\original files\\" + Path.GetFileName(fileInventoryUAE)))
                            File.Delete(filePathUAE + "\\original files\\" + Path.GetFileName(fileInventoryUAE));

                        File.Move(fileInventoryUAE, filePathUAE + "\\original files\\" + Path.GetFileName(fileInventoryUAE));
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.Move(fileInventoryUAE, filePathUAE + "\\rejects\\" + Path.GetFileName(fileInventoryUAE));

                        }
                        catch
                        {
                            throw new Exception("error");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Common.log("Error processing Apple Inventory Balance reports: " + ex.Message);
            }

            try
            {
                Common.log("Processing Apple NL Inventory Balance reports");
                ArrayList filesInventoryNL = new ArrayList(Directory.GetFiles(filePathNL));
                foreach (string fileInventoryNL in filesInventoryNL)
                {
                    string reportData = File.ReadAllText(fileInventoryNL, Encoding.Default);
                    string result = Regex.Replace(reportData,
        @",(?=[^""]*""(?:[^""]*""[^""]*"")*[^""]*$)",
        String.Empty);
                    string newFileName = Path.GetFileNameWithoutExtension(fileInventoryNL) + "_amended" + Common.timestamp() + ".csv";
                    try
                    {
                        File.AppendAllText(filePathNL + "\\tempfiles\\" + newFileName, result, Encoding.Default);
                        File.Move(filePathNL + "\\tempfiles\\" + newFileName, filePathNL + "\\processed\\" + newFileName);
                        if (File.Exists(filePathNL + "\\original files\\" + Path.GetFileName(fileInventoryNL)))
                            File.Delete(filePathNL + "\\original files\\" + Path.GetFileName(fileInventoryNL));
                        File.Move(fileInventoryNL, filePathNL + "\\original files\\" + Path.GetFileName(fileInventoryNL));
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.Move(fileInventoryNL, filePathNL + "\\rejects\\" + Path.GetFileName(fileInventoryNL));

                        }
                        catch
                        {
                            throw new Exception("error");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Common.log("Error processing Apple Inventory Balance reports: " + ex.Message);
            }

            try
            {
                Common.log("Processing Apple IT Inventory Balance reports");
                ArrayList filesItaly = new ArrayList(Directory.GetFiles(newFilePath));
                foreach (string fileIt in filesItaly)
                {
                    string reportData = File.ReadAllText(fileIt, Encoding.Default);
                    reportData = reportData.Replace("\n", "\r\n");
                    int prodIndex = reportData.IndexOf("Product");
                    reportData = reportData.Substring(prodIndex, reportData.Length - prodIndex);
                    string newFileName = Path.GetFileNameWithoutExtension(fileIt) + "_amended" + Common.timestamp() + ".xls";
                    try
                    {
                        File.AppendAllText(newFilePath + "\\tempfiles\\" + newFileName, reportData, Encoding.Default);
                        File.Move(newFilePath + "\\tempfiles\\" + newFileName, newFilePath + "\\processed\\" + newFileName);
                        if (File.Exists(newFilePath + "\\original files\\" + Path.GetFileName(fileIt)))
                            File.Delete(newFilePath + "\\original files\\" + Path.GetFileName(fileIt));
                        File.Move(fileIt, newFilePath + "\\original files\\" + Path.GetFileName(fileIt));
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.Move(fileIt, newFilePath + "\\rejects\\" + Path.GetFileName(fileIt));

                        }
                        catch (Exception ex1)
                        {
                            throw new Exception("error");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Common.log("Error processing Apple Inventory Balance reports: " + ex.Message);
            }
        }
        public static void processOtherAppleFiles()
        {

            string filePathForecastCommits = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Forecast\\";
            string filePathVMI = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple VMI Report\\";



            try
            {
                Common.log("Processing Apple Forecast Commit reports");
                ArrayList filesForecastCommits = new ArrayList(Directory.GetFiles(filePathForecastCommits));
                foreach (string fileForecast in filesForecastCommits)
                {
                    string reportData = File.ReadAllText(fileForecast, Encoding.Default);
                    reportData = reportData.Replace(",\n", ",\"\"\n");
                    string newFileName = Path.GetFileNameWithoutExtension(fileForecast) + "_amended" + Common.timestamp() + ".csv";
                    try
                    {
                        File.AppendAllText(filePathForecastCommits + "\\tempfiles\\" + newFileName, reportData, Encoding.Default);
                        File.Move(filePathForecastCommits + "\\tempfiles\\" + newFileName, filePathForecastCommits + "\\processed\\" + newFileName);
                        File.Move(fileForecast, filePathForecastCommits + "\\original files\\" + Path.GetFileName(fileForecast));
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.Move(fileForecast, filePathForecastCommits + "\\rejects\\" + Path.GetFileName(fileForecast));

                        }
                        catch
                        {

                        }
                    }

                }
            }
            catch
            {
            }
            try
            {
                Common.log("Processing Apple VMI reports");
                ArrayList filesAppleVMI = new ArrayList(Directory.GetFiles(filePathVMI));
                foreach (string fileVMI in filesAppleVMI)
                {
                    string reportData = File.ReadAllText(fileVMI, Encoding.Default);
                    reportData = Regex.Replace(reportData,
        @",(?=[^""]*""(?:[^""]*""[^""]*"")*[^""]*$)",
        String.Empty);
                    string newFileName = Path.GetFileNameWithoutExtension(fileVMI) + "_amended" + Common.timestamp() + ".csv";
                    try
                    {
                        File.AppendAllText(filePathVMI + "\\tempfiles\\" + newFileName, reportData, Encoding.Default);
                        File.Move(filePathVMI + "\\tempfiles\\" + newFileName, filePathVMI + "\\processed\\" + newFileName);
                        File.Move(fileVMI, filePathVMI + "\\original files\\" + Path.GetFileName(fileVMI));
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.Move(fileVMI, filePathVMI + "\\rejects\\" + Path.GetFileName(fileVMI));

                        }
                        catch
                        {

                        }
                    }

                }
            }
            catch { }
        }
        public static bool downloadAttachment(ActiveUp.Net.Mail.MimePart att, string downloadPatch, string localePath, string uid)
        {
            bool success = false;
            int count = 0;
            try
            {
                count = int.Parse(Common.runSQLScalar(string.Format("select count(*) from MSE_appleemailfiles where uid='{0}'", uid)).ToString());
                if (count == 0)
                {

                    string newFileName = @"";
                    string filename = att.Filename;
                    string filenameNoExt = Path.GetFileNameWithoutExtension(filename);
                    string ext = Path.GetExtension(filename);
                    string insertSql = string.Format(@"insert into MSE_appleemailfiles
output inserted.ID 
values ('{0}','{1}','{2}')", uid, filename, att.ParentMessage.Date);

                    string id = Common.runSQLScalar(insertSql).ToString();
                    newFileName = filenameNoExt + "_uid_" + id + ext;
                    Common.runSQLNonQuery(string.Format(@"update MSE_appleemailfiles set filename='{0}' where id={1}", newFileName, id));
                    att.StoreToFile(downloadPatch + localePath + @"\\" + newFileName);
                    Console.WriteLine("File: " + newFileName + " downloaded");
                }


            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }
    }
}
