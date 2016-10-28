using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using ICSharpCode.SharpZipLib;
namespace StockSalesPickup
{
    class Program
    {
        static void Main(string[] args)
        {

            switch (int.Parse(args[0].ToString()))
            {
                case 1:
                    runMaplinHomeSalesImport();
                    break;
                case 2:
                    runScrewfixSalesImport();

                    break;
                case 3:
                    runAmazonHomeSalesImport();
                    break;
                case 4:
                    runJLPStockSalesImport();
                    break;
                case 5:
                    runArgosSalesImport();
                    break;
                case 6:
                    runDSGStockSalesImport();
                    break;
                case 7:
                    runBritishGasOrdersImport();
                    break;
                case 8:
                    runArgosImports();
                    break;
                case 9:
                    runArgosIntakeImport();
                    break;




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
        public static void runArgosImports()
        {
            Common.log("Beginng Argos Email Import");
            string downloadPath = @"C:\linx-tablets\saleseposfiles\argos\";
            string temploadTableName = "productdataloader_portalbritishgasorders_tempload";
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";


            string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
            string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
            client.LoginFast(username, password);
            List<string> emailFolders = new List<string>();
            emailFolders.Add("sales");
            emailFolders.Add("stock");
            emailFolders.Add("rdc");
            foreach (string emailFolder in emailFolders)
            {
                Common.runSQLNonQuery("delete from " + string.Format("productdataloader_argos_{0}_tempload", emailFolder));
                Mailbox mailBox = client.SelectMailbox("inbox/Argos/" + emailFolder);
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {

                    Message msg = mailBox.Fetch.MessageObject(i);
                    Common.log("Processing message: " + msg.Subject);
                    if (msg.Attachments.Count > 0)
                    {
                        Common.log(msg.MessageId + ": " + msg.Subject);
                        foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                        {

                            string dateString = msg.Date.ToString();
                            string emailFilename = downloadAttachment(attachment, downloadPath, emailFolder, msg.MessageId);
                            if (emailFilename != "")
                            {
                                Common.log("Argos email Import: Processing message - " + msg.Subject + "_" + dateString.ToString());
                                ICSharpCode.SharpZipLib.Zip.FastZip fz = new ICSharpCode.SharpZipLib.Zip.FastZip();

                                fz.ExtractZip(downloadPath + @"\" + emailFolder + @"\" + emailFilename, downloadPath + @"\" + emailFolder + @"\unzip\" + Path.GetFileNameWithoutExtension(emailFilename), "");
                                string filename = "";
                                ArrayList files = new ArrayList(Directory.GetFiles(downloadPath + @"\" + emailFolder + @"\unzip\" + Path.GetFileNameWithoutExtension(emailFilename)));
                                foreach (string fileDot in files)
                                {

                                    filename = downloadPath + @"\" + emailFolder + @"\unzip\" + Path.GetFileNameWithoutExtension(emailFilename) + @"\" + Path.GetFileName(fileDot);
                                    break;
                                }
                                IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalEmailFiles/Argosmain/", "hive", "Hive123456");



                                string fileContents = File.ReadAllText(filename);
                                string newFileName = Path.GetFileNameWithoutExtension(Path.GetFileName(filename)) + "_amended_" + Common.timestamp() + Path.GetExtension(filename);
                                string[] cntArray = fileContents.Split(',');
                                string orderFileDate = cntArray[1].ToString();


                                char[] charArray = fileContents.ToCharArray();
                                int lstIndex = fileContents.LastIndexOf("\n");
                                List<int> lstLF = new List<int>();
                                char lf = (char)10;
                                for (int i1 = 0; i1 < charArray.Length; i1++)
                                {
                                    if (charArray[i1] == lf)
                                    {
                                        bool yay = true;
                                        lstLF.Add(i1);
                                    }
                                }
                                int[] arr = lstLF.ToArray();
                                int indOf = fileContents.IndexOf("\n");
                                int indFinish = arr[arr.Length - 2];
                                string subS = fileContents.Substring(indOf + 1, (indFinish - indOf));
                                if (File.Exists(downloadPath + @"\completedfiles\" + newFileName))
                                {
                                    File.Delete(downloadPath + @"\completedfiles\" + newFileName);
                                }
                                File.AppendAllText(downloadPath + @"\completedfiles\" + newFileName, subS);
                                ftpClient.uploadFile(downloadPath + @"\completedfiles\" + newFileName);
                                string tempLoadTable = string.Format("productdataloader_argos_{0}_tempload", emailFolder);
                                string sp = string.Format("sp_argosemailimport_{0}", emailFolder);

                                string blkInsert = string.Format(@"BULK INSERT " + tempLoadTable + @" FROM '\\10.16.72.129\company\FTP\root\MSESRVDOM\hive\portalEmailFiles\Argosmain\{0}'
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  )", newFileName);
                                //string newOrderFileDate = orderFileDate.Substring(0, orderFileDate.Length - orderFileDate.IndexOf("\n"));
                                string spRun = string.Format(sp + "'{0}','{1}'", attachment.Filename, dateString);
                                try
                                {
                                    Common.runSQLNonQuery(blkInsert);
                                    Common.runSQLNonQuery(spRun);
                                }
                                catch (Exception ex)
                                {
                                    Common.log("Argos email Import: Error processing message - " + msg.Subject + "_" + dateString.ToString());
                                    Common.runSQLNonQuery(string.Format("delete from mse_appleemailfiles where uid='{0}'", msg.MessageId));
                                }
                            }
                            else
                            {
                                Common.log("Argos email Import: Skipping message - " + msg.Subject + "_" + dateString.ToString());
                            }
                        }
                    }

                }
            }





        }
        public static void runArgosIntakeImport()
        {
            string testString = File.ReadAllText(@"C:\download (1).csv");
            testString = testString.Substring(testString.IndexOf("\n", 0) + 1, testString.Length - (testString.IndexOf("\n", 0) + 1));
            //testString = "This is a test \n anuidhsuidhiuashdiuhauishuiasjhsuidhasuidhishui";
            int indexCheck = 0;
           //int count =  testString.Select((c, i) => testString.Substring(i)).Count(sub => sub.StartsWith("\n"));
           int startIndex = 0;
           int indexCounter = 1;

            while (indexCheck > -2)
            {
                if (indexCounter == 28710)
                {
                    bool alertme = true;
                }
                int index = testString.IndexOf("\n", startIndex);
                if(index == testString.LastIndexOf("\n"))
                {
                    break;
                }
                startIndex = index + 3;
                string beforeSub = testString.Substring(0, index + 1);
                string afterSub = testString.Substring(index + 1, (testString.Length - index) - 1);

                testString = beforeSub + indexCounter.ToString() + "," + afterSub;
                indexCounter++;
                indexCheck = testString.IndexOf("\n", startIndex);
            }
             File.AppendAllText(@"C:\testdr\NewDownload.csv", testString);
            string fileContents1 = File.ReadAllText(@"C:\Download.csv");
            string[] result = fileContents1.Split(new string[] { "\n" }, StringSplitOptions.None);
            string newFileContents="";

            for (int i = 1; i < result.Length; i++)
            {
                result[i] = i.ToString() + "," + result[i]+"\n";
                newFileContents+=i.ToString() + "," + result[i]+"\n";
            }
           
            
            File.AppendAllText(@"C:\testdr\tst.csv",newFileContents);
           
            //string newFileLines = "";
            //string[][] jaggedArray = lines.Select(line => line.Split(',').ToArray()).ToArray();

            //string currentProductCode = "";
            //for (int i = 1; i < jaggedArray.Length; i++)
            //{
            //    if ((currentProductCode == "" || currentProductCode != jaggedArray[i][0]) && jaggedArray[i][0]!="")
            //    {
            //        currentProductCode = jaggedArray[i][0];
            //    }
            //    jaggedArray[i][0] = currentProductCode;
               
            //   newFileLines+= jaggedArray[i][20] ?? "";

            //    newFileLines += jaggedArray[i][0] + "," + jaggedArray[i][1] + "," + jaggedArray[i][2] + "," + jaggedArray[i][3] + "," + jaggedArray[i][4] + "," + jaggedArray[i][5] + "," + jaggedArray[i][7] + "," + jaggedArray[i][8] + "," + jaggedArray[i][9] + "," + jaggedArray[i][10] + "," + jaggedArray[i][11] + "," + jaggedArray[i][12] + "," + jaggedArray[i][13] + "," + jaggedArray[i][14] + "," + jaggedArray[i][15] + "," + jaggedArray[i][16] + "," + jaggedArray[i][17] + "," + jaggedArray[i][18] + "," + jaggedArray[i][19] + "," + jaggedArray[i][20] + "," + jaggedArray[i][21] + "\r\n";

            //}
            
            Common.log("Beginng Argos Intake Email Import");
            string downloadPath = @"C:\linx-tablets\saleseposfiles\argos\";
            string temploadTableName = "productdataloader_portal_argos_intake_tempload";
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;
            string emailFolder = "intake";

            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";


            string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
            string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
            client.LoginFast(username, password);
            
                
                Mailbox mailBox = client.SelectMailbox("inbox/Argos/intake");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {

                    Message msg = mailBox.Fetch.MessageObject(i);
                    Common.log("Processing message: " + msg.Subject);
                    if (msg.Attachments.Count > 0)
                    {
                        Common.log(msg.MessageId + ": " + msg.Subject);
                        foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                        {

                            string dateString = msg.Date.ToString();
                            string emailFilename = downloadAttachment(attachment, downloadPath, emailFolder, msg.MessageId);
                            if (emailFilename != "")
                            {
                                Common.log("Argos email Import: Processing message - " + msg.Subject + "_" + dateString.ToString());
                                
                                
                                IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalEmailFiles/Argosmain/", "hive", "Hive123456");
                                
                                    string filename = downloadPath + @"\" + emailFolder + @"\" + emailFilename;

                                string fileContents = File.ReadAllText(filename);
                                string newFileName = Path.GetFileNameWithoutExtension(Path.GetFileName(filename)) + "_amended_" + Common.timestamp() + Path.GetExtension(filename);
                                string[] cntArray = fileContents.Split(',');
                                string orderFileDate = cntArray[1].ToString();


                                char[] charArray = fileContents.ToCharArray();
                                int lstIndex = fileContents.LastIndexOf("\n");
                                List<int> lstLF = new List<int>();
                                char lf = (char)10;
                                for (int i1 = 0; i1 < charArray.Length; i1++)
                                {
                                    if (charArray[i1] == lf)
                                    {
                                        bool yay = true;
                                        lstLF.Add(i1);
                                    }
                                }
                                int[] arr = lstLF.ToArray();
                                
                                int indOf = fileContents.IndexOf("\n");
                                int indFinish = arr[arr.Length - 2];
                                string subS = fileContents.Substring(indOf + 1, (indFinish - indOf));
                                if (File.Exists(downloadPath + @"\completedfiles\" + newFileName))
                                {
                                    File.Delete(downloadPath + @"\completedfiles\" + newFileName);
                                }
                                File.AppendAllText(downloadPath + @"\completedfiles\" + newFileName, subS);
                                ftpClient.uploadFile(downloadPath + @"\completedfiles\" + newFileName);
                                string tempLoadTable = temploadTableName;
                                string sp = string.Format("sp_argosemailimport_intake_weeks", emailFolder);

                                string blkInsert = string.Format(@"BULK INSERT " + tempLoadTable + @" FROM '\\10.16.72.129\company\FTP\root\MSESRVDOM\hive\portalEmailFiles\Argosmain\{0}'
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  )", newFileName);
                                string spRun = string.Format(sp + "'{0}','{1}'", attachment.Filename, dateString);
                                try
                                {
                                    Common.runSQLNonQuery(blkInsert);
                                    Common.runSQLNonQuery(spRun);
                                }
                                catch (Exception ex)
                                {
                                    Common.log("Argos email Import: Error processing message - " + msg.Subject + "_" + dateString.ToString());
                                    Common.runSQLNonQuery(string.Format("delete from mse_appleemailfiles where uid='{0}'", msg.MessageId));
                                }
                            }
                            else
                            {
                                Common.log("Argos email Import: Skipping message - " + msg.Subject + "_" + dateString.ToString());
                            }
                        }
                    }

                }
            





        }
        public static void runBritishGasOrdersImport()
        {
            Common.log("Beginng British Gas Orders Import");


            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portalbritishgasorders_tempload";
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";


            string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
            string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
            client.LoginFast(username, password);

            Mailbox mailBox = client.SelectMailbox("inbox/Hive/Orders");
            for (int i = 1; i <= mailBox.MessageCount; i++)
            {

                Message msg = mailBox.Fetch.MessageObject(i);
                if (msg.Attachments.Count > 0)
                {
                    Common.log(msg.MessageId + ": " + msg.Subject);
                    foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                    {
                        DateTime dateString = msg.Date;
                        string emailFilename = downloadAttachment(attachment, downloadPath, "Orders", msg.MessageId);
                        if (emailFilename != "")
                        {
                            IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalEmailFiles/" + "Orders" + "/", "hive", "Hive123456");



                            string fileContents = File.ReadAllText(downloadPath + @"\" + "orders" + @"\" + emailFilename);
                            string newFileName = Path.GetFileNameWithoutExtension(emailFilename) + "_amended_" + Common.timestamp() + Path.GetExtension(emailFilename);
                            string[] cntArray = fileContents.Split(',');
                            string orderFileDate = cntArray[1].ToString();


                            char[] charArray = fileContents.ToCharArray();
                            int lstIndex = fileContents.LastIndexOf("\n");
                            List<int> lstLF = new List<int>();
                            char lf = (char)10;
                            for (int i1 = 0; i1 < charArray.Length; i1++)
                            {
                                if (charArray[i1] == lf)
                                {
                                    bool yay = true;
                                    lstLF.Add(i1);
                                }
                            }
                            int[] arr = lstLF.ToArray();
                            int indOf = fileContents.IndexOf("\n");
                            int indFinish = arr[arr.Length - 2];
                            string subS = fileContents.Substring(indOf + 1, (indFinish - indOf));
                            if (File.Exists(downloadPath + newFileName))
                            {
                                File.Delete(downloadPath + newFileName);
                            }
                            File.AppendAllText(downloadPath + @"\orders\" + newFileName, subS);
                            ftpClient.uploadFile(downloadPath + @"\" + "orders" + @"\" + newFileName);
                            string blkInsert = string.Format(@"BULK INSERT productdataloader_portalbritishgasorders_tempload FROM '\\10.16.72.129\company\FTP\root\MSESRVDOM\hive\portalEmailFiles\Orders\{0}'
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  )", newFileName);
                            string newOrderFileDate = orderFileDate.Substring(0, orderFileDate.Length - orderFileDate.IndexOf("\n"));
                            string spRun = string.Format("[sp_portalBatchOrdersimport_postload] '{0}','{1}'", attachment.Filename, orderFileDate.Substring(0, orderFileDate.Length - (orderFileDate.Length - orderFileDate.IndexOf("\n"))));
                            try
                            {
                                Common.runSQLNonQuery(blkInsert);
                                Common.runSQLNonQuery(spRun);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }

            }






        }
        public static void runArgosSalesImport()
        {
            Common.log("Beginng Argos Import");
            Common.runSQLNonQuery("DELETE from MSE_appleemailfiles where filename like '%Argos%'");
            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portalepossales_argos_tempload";
            SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
            conn.Open();
            DataTable table = SqlBulkLoader.getSchemaForTable(temploadTableName, conn);
            SqlBulkLoader bcp = new SqlBulkLoader(table, temploadTableName, conn, true);
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                Mailbox mailBox = client.SelectMailbox("inbox/Hive/Emails/Argos");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                            {
                                DateTime dateString = msg.Date;
                                string emailFilename = downloadAttachment(attachment, downloadPath, "Argos", msg.MessageId);
                                if (emailFilename != "")
                                {



                                    HSSFWorkbook hssfwb;
                                    XSSFWorkbook xssfwb;
                                    using (FileStream file = new FileStream(downloadPath + @"\" + "Argos" + @"\" + emailFilename, FileMode.Open, FileAccess.Read))
                                    {
                                        xssfwb = new XSSFWorkbook(file);
                                    }

                                    int intakeErrorCount = 0;
                                    ISheet sheet = xssfwb.GetSheet("Stock & Sales");


                                    for (int row = 3; row < sheet.LastRowNum; row++)
                                    {
                                        if (sheet.GetRow(row) != null) //null is when the row only contains empty cells 
                                        {
                                            IRow ir = sheet.GetRow(row);
                                            string nulltest = "";
                                            try
                                            {
                                                nulltest = sheet.GetRow(row).GetCell(0).ToString();
                                            }
                                            catch
                                            {
                                                continue;
                                            }
                                            try
                                            {

                                                string Range = sheet.GetRow(row).GetCell(0).ToString();

                                                sheet.GetRow(row).GetCell(6).SetCellType(CellType.String);
                                                sheet.GetRow(row).GetCell(7).SetCellType(CellType.String);
                                                sheet.GetRow(row).GetCell(8).SetCellType(CellType.String);
                                                sheet.GetRow(row).GetCell(9).SetCellType(CellType.String);
                                                sheet.GetRow(row).GetCell(10).SetCellType(CellType.String);
                                                string Brand = sheet.GetRow(row).GetCell(1).ToString();
                                                string Customer_Item = sheet.GetRow(row).GetCell(2).ToString();
                                                string Manufacturer_Part = sheet.GetRow(row).GetCell(3).ToString();
                                                string Exertis_Part_Code = sheet.GetRow(row).GetCell(4).ToString();
                                                string Description = sheet.GetRow(row).GetCell(5).ToString();
                                                string LW_Stock1 = sheet.GetRow(row).GetCell(6).ToString();
                                                string LW_Sales1 = sheet.GetRow(row).GetCell(7).ToString();
                                                string LW_Demand1 = sheet.GetRow(row).GetCell(8).ToString();
                                                string LW_Serviceability_1 = sheet.GetRow(row).GetCell(9).ToString();
                                                string _LW_Promotion1 = sheet.GetRow(row).GetCell(10).ToString();
                                                string LW_Stock2 = sheet.GetRow(row).GetCell(11).ToString();
                                                string LW_Sales2 = sheet.GetRow(row).GetCell(12).ToString();
                                                string LW_Demand2 = sheet.GetRow(row).GetCell(13).ToString();
                                                string LW_Serviceability_2 = sheet.GetRow(row).GetCell(14).ToString();
                                                string _LW_Promotion2 = sheet.GetRow(row).GetCell(15).ToString();
                                                string LW_Stock3 = sheet.GetRow(row).GetCell(16).ToString();
                                                string LW_Sales3 = sheet.GetRow(row).GetCell(17).ToString();
                                                string LW_Demand3 = sheet.GetRow(row).GetCell(18).ToString();
                                                string LW_Serviceability_3 = sheet.GetRow(row).GetCell(19).ToString();
                                                string _LW_Promotion3 = sheet.GetRow(row).GetCell(20).ToString();
                                                string LW_Stock4 = sheet.GetRow(row).GetCell(21).ToString();
                                                string LW_Sales4 = sheet.GetRow(row).GetCell(22).ToString();
                                                string LW_Demand4 = sheet.GetRow(row).GetCell(23).ToString();
                                                string LW_Serviceability_4 = sheet.GetRow(row).GetCell(24).ToString();
                                                string _LW_Promotion4 = sheet.GetRow(row).GetCell(25).ToString();
                                                string LW_Stock5 = sheet.GetRow(row).GetCell(26).ToString();
                                                string LW_Sales5 = sheet.GetRow(row).GetCell(27).ToString();
                                                string LW_Demand5 = sheet.GetRow(row).GetCell(28).ToString();
                                                string LW_Serviceability_5 = sheet.GetRow(row).GetCell(29).ToString();
                                                string _LW_Promotion5 = sheet.GetRow(row).GetCell(30).ToString();
                                                string LW_Stock6 = sheet.GetRow(row).GetCell(31).ToString();
                                                string LW_Sales6 = sheet.GetRow(row).GetCell(32).ToString();
                                                string LW_Demand6 = sheet.GetRow(row).GetCell(33).ToString();
                                                string LW_Serviceability_6 = sheet.GetRow(row).GetCell(34).ToString();
                                                string _LW_Promotion6 = sheet.GetRow(row).GetCell(35).ToString();
                                                string LW_Stock7 = sheet.GetRow(row).GetCell(36).ToString();
                                                string LW_Sales7 = sheet.GetRow(row).GetCell(37).ToString();
                                                string LW_Demand7 = sheet.GetRow(row).GetCell(38).ToString();
                                                string LW_Serviceability_7 = sheet.GetRow(row).GetCell(39).ToString();
                                                string _LW_Promotion7 = sheet.GetRow(row).GetCell(40).ToString();
                                                string LW_Stock8 = sheet.GetRow(row).GetCell(41).ToString();
                                                string LW_Sales8 = sheet.GetRow(row).GetCell(42).ToString();
                                                string LW_Demand8 = sheet.GetRow(row).GetCell(43).ToString();
                                                string LW_Serviceability_8 = sheet.GetRow(row).GetCell(44).ToString();
                                                string _LW_Promotion8 = sheet.GetRow(row).GetCell(45).ToString();
                                                string LW_Stock9 = sheet.GetRow(row).GetCell(46).ToString();
                                                string LW_Sales9 = sheet.GetRow(row).GetCell(47).ToString();
                                                string LW_Demand9 = sheet.GetRow(row).GetCell(48).ToString();
                                                string LW_Serviceability_9 = sheet.GetRow(row).GetCell(49).ToString();
                                                string _LW_Promotion9 = sheet.GetRow(row).GetCell(50).ToString();
                                                string LW_Stock10 = sheet.GetRow(row).GetCell(51).ToString();
                                                string LW_Sales10 = sheet.GetRow(row).GetCell(52).ToString();
                                                string LW_Demand10 = sheet.GetRow(row).GetCell(53).ToString();
                                                string LW_Serviceability_10 = sheet.GetRow(row).GetCell(54).ToString();
                                                string _LW_Promotion10 = sheet.GetRow(row).GetCell(55).ToString();

                                                string loadString = string.Format(@"{0}pdivp{1}pdivp{2}pdivp{3}pdivp{4}pdivp{5}pdivp{6}pdivp{7}pdivp{8}pdivp{9}pdivp{10}pdivp{11}pdivp{12}pdivp{13}pdivp{14}pdivp{15}pdivp{16}pdivp{17}pdivp{18}pdivp{19}pdivp{20}pdivp{21}pdivp{22}pdivp{23}pdivp{24}pdivp{25}pdivp{26}pdivp{27}pdivp{28}pdivp{29}pdivp{30}pdivp{31}pdivp{32}pdivp{33}pdivp{34}pdivp{35}pdivp{36}pdivp{37}pdivp{38}pdivp{39}pdivp{40}pdivp{41}pdivp{42}pdivp{43}pdivp{44}pdivp{45}pdivp{46}pdivp{47}pdivp{48}pdivp{49}pdivp{50}pdivp{51}pdivp{52}pdivp{53}pdivp{54}pdivp{55}",
                                                     Range, Brand, Customer_Item, Manufacturer_Part, Exertis_Part_Code, Description, LW_Stock1, LW_Sales1, LW_Demand1, LW_Serviceability_1, _LW_Promotion1, LW_Stock2, LW_Sales2, LW_Demand2, LW_Serviceability_2, _LW_Promotion2, LW_Stock3, LW_Sales3, LW_Demand3, LW_Serviceability_3, _LW_Promotion3, LW_Stock4, LW_Sales4, LW_Demand4, LW_Serviceability_4, _LW_Promotion4, LW_Stock5, LW_Sales5, LW_Demand5, LW_Serviceability_5, _LW_Promotion5, LW_Stock6, LW_Sales6, LW_Demand6, LW_Serviceability_6, _LW_Promotion6, LW_Stock7, LW_Sales7, LW_Demand7, LW_Serviceability_7, _LW_Promotion7, LW_Stock8, LW_Sales8, LW_Demand8, LW_Serviceability_8, _LW_Promotion8, LW_Stock9, LW_Sales9, LW_Demand9, LW_Serviceability_9, _LW_Promotion9, LW_Stock10, LW_Sales10, LW_Demand10, LW_Serviceability_10, _LW_Promotion10);

                                                loadString = loadString.Replace("-", "").Replace(",", "");
                                                loadString = loadString.Replace("pdivp", ",");
                                                try
                                                {
                                                    DataRow rowDR = SqlBulkLoader.parseValuesIntoDataRow(table.NewRow(), loadString, ',');
                                                    bcp.LoadItem(rowDR);
                                                    counter++;
                                                }
                                                catch
                                                {
                                                    bool failure = true;
                                                }


                                            }
                                            catch (Exception ex)
                                            {
                                                bool failure = true;
                                            }
                                        }
                                    }


                                    bool finish = true;
                                    bcp.Flush();
                                    conn.Close();
                                    int retailerID = 5;
                                    string sqlHeaderInsertID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalSalesEposFiles
						output inserted.ID into @IDNL values (5,{0},'Epos/Stock','{1}',getdate())

						select ID from @IDNL", retailerID, emailFilename)).ToString();

                                    string sqlLinesInsert = string.Format(@"insert into MSE_PortalargosReportLines
						select {0},* from productdataloader_portalepossales_argos_tempload", sqlHeaderInsertID);

                                    Common.runSQLNonQuery(sqlLinesInsert);


                                    string spRun = string.Format("sp_portalepossalesimport_postload_argos '{0}',{1}", emailFilename, sqlHeaderInsertID);
                                    Common.runSQLNonQuery(spRun);



                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }


        public static void runArgosImport()
        {
            Common.log("Beginng Argos Import");
            Common.runSQLNonQuery("DELETE from MSE_appleemailfiles where filename like '%Argos%'");
            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portalepossales_argos_tempload";
            SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
            conn.Open();
            DataTable table = SqlBulkLoader.getSchemaForTable(temploadTableName, conn);
            SqlBulkLoader bcp = new SqlBulkLoader(table, temploadTableName, conn, true);
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                Mailbox mailBox = client.SelectMailbox("inbox/Hive/Emails/Argos");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                            {
                                DateTime dateString = msg.Date;
                                string emailFilename = downloadAttachment(attachment, downloadPath, "Argos", msg.MessageId);
                                if (emailFilename != "")
                                {



                                    HSSFWorkbook hssfwb;
                                    XSSFWorkbook xssfwb;
                                    using (FileStream file = new FileStream(downloadPath + @"\" + "Argos" + @"\" + emailFilename, FileMode.Open, FileAccess.Read))
                                    {
                                        xssfwb = new XSSFWorkbook(file);
                                    }

                                    int intakeErrorCount = 0;
                                    ISheet sheet = xssfwb.GetSheet("Stock & Sales");


                                    for (int row = 3; row < sheet.LastRowNum; row++)
                                    {
                                        if (sheet.GetRow(row) != null) //null is when the row only contains empty cells 
                                        {
                                            IRow ir = sheet.GetRow(row);
                                            string nulltest = "";
                                            try
                                            {
                                                nulltest = sheet.GetRow(row).GetCell(0).ToString();
                                            }
                                            catch
                                            {
                                                continue;
                                            }
                                            try
                                            {

                                                string Range = sheet.GetRow(row).GetCell(0).ToString();

                                                sheet.GetRow(row).GetCell(6).SetCellType(CellType.String);
                                                sheet.GetRow(row).GetCell(7).SetCellType(CellType.String);
                                                sheet.GetRow(row).GetCell(8).SetCellType(CellType.String);
                                                sheet.GetRow(row).GetCell(9).SetCellType(CellType.String);
                                                sheet.GetRow(row).GetCell(10).SetCellType(CellType.String);
                                                string Brand = sheet.GetRow(row).GetCell(1).ToString();
                                                string Customer_Item = sheet.GetRow(row).GetCell(2).ToString();
                                                string Manufacturer_Part = sheet.GetRow(row).GetCell(3).ToString();
                                                string Exertis_Part_Code = sheet.GetRow(row).GetCell(4).ToString();
                                                string Description = sheet.GetRow(row).GetCell(5).ToString();
                                                string LW_Stock1 = sheet.GetRow(row).GetCell(6).ToString();
                                                string LW_Sales1 = sheet.GetRow(row).GetCell(7).ToString();
                                                string LW_Demand1 = sheet.GetRow(row).GetCell(8).ToString();
                                                string LW_Serviceability_1 = sheet.GetRow(row).GetCell(9).ToString();
                                                string _LW_Promotion1 = sheet.GetRow(row).GetCell(10).ToString();
                                                string LW_Stock2 = sheet.GetRow(row).GetCell(11).ToString();
                                                string LW_Sales2 = sheet.GetRow(row).GetCell(12).ToString();
                                                string LW_Demand2 = sheet.GetRow(row).GetCell(13).ToString();
                                                string LW_Serviceability_2 = sheet.GetRow(row).GetCell(14).ToString();
                                                string _LW_Promotion2 = sheet.GetRow(row).GetCell(15).ToString();
                                                string LW_Stock3 = sheet.GetRow(row).GetCell(16).ToString();
                                                string LW_Sales3 = sheet.GetRow(row).GetCell(17).ToString();
                                                string LW_Demand3 = sheet.GetRow(row).GetCell(18).ToString();
                                                string LW_Serviceability_3 = sheet.GetRow(row).GetCell(19).ToString();
                                                string _LW_Promotion3 = sheet.GetRow(row).GetCell(20).ToString();
                                                string LW_Stock4 = sheet.GetRow(row).GetCell(21).ToString();
                                                string LW_Sales4 = sheet.GetRow(row).GetCell(22).ToString();
                                                string LW_Demand4 = sheet.GetRow(row).GetCell(23).ToString();
                                                string LW_Serviceability_4 = sheet.GetRow(row).GetCell(24).ToString();
                                                string _LW_Promotion4 = sheet.GetRow(row).GetCell(25).ToString();
                                                string LW_Stock5 = sheet.GetRow(row).GetCell(26).ToString();
                                                string LW_Sales5 = sheet.GetRow(row).GetCell(27).ToString();
                                                string LW_Demand5 = sheet.GetRow(row).GetCell(28).ToString();
                                                string LW_Serviceability_5 = sheet.GetRow(row).GetCell(29).ToString();
                                                string _LW_Promotion5 = sheet.GetRow(row).GetCell(30).ToString();
                                                string LW_Stock6 = sheet.GetRow(row).GetCell(31).ToString();
                                                string LW_Sales6 = sheet.GetRow(row).GetCell(32).ToString();
                                                string LW_Demand6 = sheet.GetRow(row).GetCell(33).ToString();
                                                string LW_Serviceability_6 = sheet.GetRow(row).GetCell(34).ToString();
                                                string _LW_Promotion6 = sheet.GetRow(row).GetCell(35).ToString();
                                                string LW_Stock7 = sheet.GetRow(row).GetCell(36).ToString();
                                                string LW_Sales7 = sheet.GetRow(row).GetCell(37).ToString();
                                                string LW_Demand7 = sheet.GetRow(row).GetCell(38).ToString();
                                                string LW_Serviceability_7 = sheet.GetRow(row).GetCell(39).ToString();
                                                string _LW_Promotion7 = sheet.GetRow(row).GetCell(40).ToString();
                                                string LW_Stock8 = sheet.GetRow(row).GetCell(41).ToString();
                                                string LW_Sales8 = sheet.GetRow(row).GetCell(42).ToString();
                                                string LW_Demand8 = sheet.GetRow(row).GetCell(43).ToString();
                                                string LW_Serviceability_8 = sheet.GetRow(row).GetCell(44).ToString();
                                                string _LW_Promotion8 = sheet.GetRow(row).GetCell(45).ToString();
                                                string LW_Stock9 = sheet.GetRow(row).GetCell(46).ToString();
                                                string LW_Sales9 = sheet.GetRow(row).GetCell(47).ToString();
                                                string LW_Demand9 = sheet.GetRow(row).GetCell(48).ToString();
                                                string LW_Serviceability_9 = sheet.GetRow(row).GetCell(49).ToString();
                                                string _LW_Promotion9 = sheet.GetRow(row).GetCell(50).ToString();
                                                string LW_Stock10 = sheet.GetRow(row).GetCell(51).ToString();
                                                string LW_Sales10 = sheet.GetRow(row).GetCell(52).ToString();
                                                string LW_Demand10 = sheet.GetRow(row).GetCell(53).ToString();
                                                string LW_Serviceability_10 = sheet.GetRow(row).GetCell(54).ToString();
                                                string _LW_Promotion10 = sheet.GetRow(row).GetCell(55).ToString();

                                                string loadString = string.Format(@"{0}pdivp{1}pdivp{2}pdivp{3}pdivp{4}pdivp{5}pdivp{6}pdivp{7}pdivp{8}pdivp{9}pdivp{10}pdivp{11}pdivp{12}pdivp{13}pdivp{14}pdivp{15}pdivp{16}pdivp{17}pdivp{18}pdivp{19}pdivp{20}pdivp{21}pdivp{22}pdivp{23}pdivp{24}pdivp{25}pdivp{26}pdivp{27}pdivp{28}pdivp{29}pdivp{30}pdivp{31}pdivp{32}pdivp{33}pdivp{34}pdivp{35}pdivp{36}pdivp{37}pdivp{38}pdivp{39}pdivp{40}pdivp{41}pdivp{42}pdivp{43}pdivp{44}pdivp{45}pdivp{46}pdivp{47}pdivp{48}pdivp{49}pdivp{50}pdivp{51}pdivp{52}pdivp{53}pdivp{54}pdivp{55}",
                                                     Range, Brand, Customer_Item, Manufacturer_Part, Exertis_Part_Code, Description, LW_Stock1, LW_Sales1, LW_Demand1, LW_Serviceability_1, _LW_Promotion1, LW_Stock2, LW_Sales2, LW_Demand2, LW_Serviceability_2, _LW_Promotion2, LW_Stock3, LW_Sales3, LW_Demand3, LW_Serviceability_3, _LW_Promotion3, LW_Stock4, LW_Sales4, LW_Demand4, LW_Serviceability_4, _LW_Promotion4, LW_Stock5, LW_Sales5, LW_Demand5, LW_Serviceability_5, _LW_Promotion5, LW_Stock6, LW_Sales6, LW_Demand6, LW_Serviceability_6, _LW_Promotion6, LW_Stock7, LW_Sales7, LW_Demand7, LW_Serviceability_7, _LW_Promotion7, LW_Stock8, LW_Sales8, LW_Demand8, LW_Serviceability_8, _LW_Promotion8, LW_Stock9, LW_Sales9, LW_Demand9, LW_Serviceability_9, _LW_Promotion9, LW_Stock10, LW_Sales10, LW_Demand10, LW_Serviceability_10, _LW_Promotion10);

                                                loadString = loadString.Replace("-", "").Replace(",", "");
                                                loadString = loadString.Replace("pdivp", ",");
                                                try
                                                {
                                                    DataRow rowDR = SqlBulkLoader.parseValuesIntoDataRow(table.NewRow(), loadString, ',');
                                                    bcp.LoadItem(rowDR);
                                                    counter++;
                                                }
                                                catch
                                                {
                                                    bool failure = true;
                                                }


                                            }
                                            catch (Exception ex)
                                            {
                                                bool failure = true;
                                            }
                                        }
                                    }


                                    bool finish = true;
                                    bcp.Flush();
                                    conn.Close();
                                    int retailerID = 5;
                                    string sqlHeaderInsertID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalSalesEposFiles
						output inserted.ID into @IDNL values (5,{0},'Epos/Stock','{1}',getdate())

						select ID from @IDNL", retailerID, emailFilename)).ToString();

                                    string sqlLinesInsert = string.Format(@"insert into MSE_PortalargosReportLines
						select {0},* from productdataloader_portalepossales_argos_tempload", sqlHeaderInsertID);

                                    Common.runSQLNonQuery(sqlLinesInsert);


                                    string spRun = string.Format("sp_portalepossalesimport_postload_argos '{0}',{1}", emailFilename, sqlHeaderInsertID);
                                    Common.runSQLNonQuery(spRun);



                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        public static void runJLPSalesImport()
        {
            Common.log("Beginng JLP Import");
            Common.runSQLNonQuery("DELETE from MSE_appleemailfiles where filename like '%jlp%'");
            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portalepossales_jlp_tempload";
            SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
            conn.Open();
            DataTable table = SqlBulkLoader.getSchemaForTable(temploadTableName, conn);
            SqlBulkLoader bcp = new SqlBulkLoader(table, temploadTableName, conn, true);
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                Mailbox mailBox = client.SelectMailbox("inbox/Hive/Emails/jlp");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                            {


                                DateTime dateString = msg.Date;
                                string emailFilename = downloadAttachment(attachment, downloadPath, "jlp", msg.MessageId);
                                if (emailFilename != "")
                                {


                                    bool excel = false;
                                    if (excel)
                                    {
                                        HSSFWorkbook hssfwb;
                                        XSSFWorkbook xssfwb;
                                        using (FileStream file = new FileStream(downloadPath + @"\" + "jlp" + @"\" + emailFilename, FileMode.Open, FileAccess.Read))
                                        {
                                            xssfwb = new XSSFWorkbook(file);
                                        }

                                        int intakeErrorCount = 0;
                                        ISheet sheet = xssfwb.GetSheet("Sheet1");


                                        for (int row = 2; row < sheet.LastRowNum; row++)
                                        {
                                            if (sheet.GetRow(row) != null) //null is when the row only contains empty cells 
                                            {
                                                IRow ir = sheet.GetRow(row);
                                                string nulltest = "";
                                                try
                                                {
                                                    nulltest = sheet.GetRow(row).GetCell(0).ToString();
                                                }
                                                catch
                                                {
                                                    continue;
                                                }
                                                try
                                                {
                                                    string Product_Code = sheet.GetRow(row).GetCell(0).ToString();
                                                    string EAN = sheet.GetRow(row).GetCell(1).ToString();
                                                    string Product_Description = sheet.GetRow(row).GetCell(2).ToString();
                                                    string Branch_Number = sheet.GetRow(row).GetCell(3).ToString();
                                                    string Branch_Name = sheet.GetRow(row).GetCell(4).ToString();
                                                    string All_Sales_Units = sheet.GetRow(row).GetCell(5).ToString();
                                                    string All_Returns_Units = sheet.GetRow(row).GetCell(6).ToString();
                                                    string All_Branch_Stock = sheet.GetRow(row).GetCell(7).ToString();
                                                    string Available_Branch_Stock = sheet.GetRow(row).GetCell(8).ToString();
                                                    string All_WHS_Stock = sheet.GetRow(row).GetCell(9).ToString();
                                                    string Available_WHS_Stock = sheet.GetRow(row).GetCell(10).ToString();
                                                    string In_Transit_Stock = sheet.GetRow(row).GetCell(11).ToString();
                                                    string Cust_Orders = sheet.GetRow(row).GetCell(12).ToString();

                                                    string loadString = string.Format(@"{0}pdivp{1}pdivp{2}pdivp{3}pdivp{4}pdivp{5}pdivp{6}pdivp{7}pdivp{8}pdivp{9}pdivp{10}pdivp{11}pdivp{12}",
                                                          Product_Code, EAN, Product_Description, Branch_Number, Branch_Name, All_Sales_Units, All_Returns_Units, All_Branch_Stock, Available_Branch_Stock, All_WHS_Stock, Available_WHS_Stock, In_Transit_Stock, Cust_Orders);


                                                    loadString = loadString.Replace("-", "").Replace(",", "");
                                                    loadString = loadString.Replace("pdivp", ",");
                                                    try
                                                    {
                                                        DataRow rowDR = SqlBulkLoader.parseValuesIntoDataRow(table.NewRow(), loadString, ',');
                                                        bcp.LoadItem(rowDR);
                                                        counter++;
                                                    }
                                                    catch
                                                    {
                                                        bool failure = true;
                                                    }


                                                }
                                                catch (Exception ex)
                                                {
                                                    bool failure = true;
                                                }
                                            }

                                        }
                                    }
                                    else
                                    {
                                        IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalEmailFiles/" + "jlp" + "/", "hive", "Hive123456");
                                        ftpClient.uploadFile(downloadPath + @"\" + "jlp" + @"\" + emailFilename);
                                        string bulkInsert = string.Format(@"BULK INSERT productdataloader_portalepossales_jlp_tempload FROM '\\10.16.72.129\company\ftp\root\msesrvdom\hive\portalEmailFiles\jlp\{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", emailFilename);
                                        Common.runSQLNonQuery(bulkInsert);

                                    }


                                    bool finish = true;
                                    bcp.Flush();
                                    conn.Close();
                                    int retailerID = 9;
                                    string sqlHeaderInsertID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalSalesEposFiles
						output inserted.ID into @IDNL values (5,{0},'Epos/Stock','{1}',getdate())

						select ID from @IDNL", retailerID, emailFilename)).ToString();

                                    string sqlLinesInsert = string.Format(@"insert into MSE_PortaljlpfullReportLines
						select {0},* from productdataloader_portalepossales_jlp_tempload", sqlHeaderInsertID);

                                    Common.runSQLNonQuery(sqlLinesInsert);


                                    string spRun = string.Format("sp_portalepossalesimport_postload_jlp '{0}',{1}", emailFilename, sqlHeaderInsertID);
                                    Common.runSQLNonQuery(spRun);



                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static void runJLPStockSalesImport()
        {
            Common.log("Beginng JLP Import");
            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portal_jlp_stocksales";
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                //Mailbox mailBox = client.SelectMailbox("inbox/Hive/Emails/jlp");
                Mailbox mailBox = client.SelectMailbox("inbox/Stock Sales Emails Auto/jlp");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                            {


                                DateTime dateString = msg.Date;
                                string emailFilename = downloadAttachment(attachment, downloadPath, "jlp", msg.MessageId);
                                if (emailFilename != "")
                                {
                                    Common.log("JLP Import: Processing message - " + msg.Subject + "_" + dateString.ToString());


                                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalEmailFiles/" + "jlp" + "/", "hive", "Hive123456");
                                    ftpClient.uploadFile(downloadPath + @"\" + "jlp" + @"\" + emailFilename);
                                    string bulkInsert = string.Format(@"BULK INSERT productdataloader_portal_jlp_stocksales FROM '\\10.16.72.129\company\ftp\root\msesrvdom\hive\portalEmailFiles\jlp\{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", emailFilename);
                                    Common.runSQLNonQuery(bulkInsert);




                                    bool finish = true;
                                    int retailerID = 11;
                                    string sqlHeaderInsertID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalSalesEposFiles
						output inserted.ID into @IDNL values (6,{0},'Epos/Stock','{1}',getdate())

						select ID from @IDNL", retailerID, emailFilename)).ToString();

                                    //                                    string sqlLinesInsert = string.Format(@"insert into MSE_PortaljlpfullReportLines
                                    //						select {0},* from productdataloader_portalepossales_jlp_tempload", sqlHeaderInsertID);

                                    //                                    Common.runSQLNonQuery(sqlLinesInsert);

                                    try
                                    {
                                        string spRun = string.Format("sp_portalstocksalesimport_postload_jlp '{0}'", emailFilename);
                                        Common.runSQLNonQuery(spRun);
                                    }
                                    catch (Exception ex)
                                    {
                                        Common.log("JLP Import: Error processing message - " + msg.Subject + "_" + dateString.ToString());
                                        Common.runSQLNonQuery(string.Format("delete from mse_appleemailfiles where uid='{0}'", msg.MessageId));
                                    }                                   

                                }
                                else
                                {
                                    Common.log("JLP Import: Skipping message - " + msg.Subject + "_" + dateString.ToString());
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.log("JLP Import Error: Error message: - " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.log("JLP Import Error: Error message: - " + ex.Message);

            }
            Common.log("Completing JLP Import");
        }
        public static void runDSGStockSalesImport()
        {
            Common.log("Beginng DSG Import");
            
            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portal_dsg_stocksales";
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                Mailbox mailBox = client.SelectMailbox("inbox/Stock Sales Emails Auto/dsg");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                            {


                                DateTime dateString = msg.Date;
                                string emailFilename = downloadAttachment(attachment, downloadPath, "dsg", msg.MessageId);
                                if (emailFilename != "")
                                {
                                    Common.log("DSG Import: Processing message - " + msg.Subject + "_" + dateString.ToString());


                                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalEmailFiles/" + "dsg" + "/", "hive", "Hive123456");
                                    ftpClient.uploadFile(downloadPath + @"\" + "dsg" + @"\" + emailFilename);
                                    string bulkInsert = string.Format(@"BULK INSERT " + temploadTableName + @" FROM '\\10.16.72.129\company\ftp\root\msesrvdom\hive\portalEmailFiles\dsg\{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", emailFilename);
                                    Common.runSQLNonQuery(bulkInsert);




                                    bool finish = true;
                                    int retailerID = 11;
                                    string sqlHeaderInsertID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalSalesEposFiles
						output inserted.ID into @IDNL values (2,{0},'Epos/Stock','{1}',getdate())

						select ID from @IDNL", retailerID, emailFilename)).ToString();

                                    //                                    string sqlLinesInsert = string.Format(@"insert into MSE_PortaljlpfullReportLines
                                    //						select {0},* from productdataloader_portalepossales_jlp_tempload", sqlHeaderInsertID);

                                    //                                    Common.runSQLNonQuery(sqlLinesInsert);

                                    try
                                    {
                                    string spRun = string.Format("sp_portalstocksalesimport_postload_dsg '{0}'", emailFilename);
                                    Common.runSQLNonQuery(spRun);

                                    }
                                    catch (Exception ex)
                                    {
                                        Common.log("DSG Import: Error processing message - " + msg.Subject + "_" + dateString.ToString());
                                        Common.runSQLNonQuery(string.Format("delete from mse_appleemailfiles where uid='{0}'", msg.MessageId));
                                    }   

                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.log("DSG Import: Error processing message - " + ex.Message);

                    }
                }
            }
            catch (Exception ex)
            {
                Common.log("DSG Import: Error processing message - " + ex.Message);
            }
            Common.log("Completing DSG Import");
        }
        public static void runAmazonHomeSalesImport()
        {
            Common.log("Beginng Amazon Import");
            Common.runSQLNonQuery("DELETE from MSE_appleemailfiles where filename like '%full ama%'");
            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portalepossales_amazonfull_tempload";
            SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
            conn.Open();
            DataTable table = SqlBulkLoader.getSchemaForTable(temploadTableName, conn);
            SqlBulkLoader bcp = new SqlBulkLoader(table, temploadTableName, conn, true);
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                Mailbox mailBox = client.SelectMailbox("inbox/Hive/Emails/AmazonHome");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                            {
                                DateTime dateString = msg.Date;
                                string emailFilename = downloadAttachment(attachment, downloadPath, "AmazonHome", msg.MessageId);
                                if (emailFilename != "")
                                {



                                    HSSFWorkbook hssfwb;
                                    XSSFWorkbook xssfwb;
                                    using (FileStream file = new FileStream(downloadPath + @"\" + "AmazonHome" + @"\" + emailFilename, FileMode.Open, FileAccess.Read))
                                    {
                                        xssfwb = new XSSFWorkbook(file);
                                    }

                                    int intakeErrorCount = 0;
                                    ISheet sheetCE = xssfwb.GetSheet("CE");
                                    ISheet sheetSW = xssfwb.GetSheet("SW");
                                    ISheet sheetVG = xssfwb.GetSheet("VG");
                                    ISheet sheetDIY = xssfwb.GetSheet("DIY");
                                    ISheet sheet = sheetDIY;
                                    string localeValue = "CE";
                                    for (int iS = 0; iS < 1; iS++)
                                    {
                                        switch (iS)
                                        {
                                            case 1:
                                                sheet = sheetSW;
                                                localeValue = "SW";
                                                break;
                                            case 2:
                                                sheet = sheetVG;
                                                localeValue = "VG";
                                                break;
                                            case 3:
                                                sheet = sheetDIY;
                                                localeValue = "DIY";
                                                break;
                                        }

                                        for (int row = 1; row < sheet.LastRowNum; row++)
                                        {
                                            if (sheet.GetRow(row) != null) //null is when the row only contains empty cells 
                                            {
                                                IRow ir = sheet.GetRow(row);
                                                string nulltest = "";
                                                try
                                                {
                                                    nulltest = sheet.GetRow(row).GetCell(0).ToString();
                                                }
                                                catch
                                                {
                                                    continue;
                                                }
                                                try
                                                {
                                                    string ASIN = sheet.GetRow(row).GetCell(0).ToString();
                                                    string ASIN_name = sheet.GetRow(row).GetCell(1).ToString();
                                                    string Manufacturer = sheet.GetRow(row).GetCell(2).ToString();
                                                    string ID_type = sheet.GetRow(row).GetCell(3).ToString();
                                                    string External_ID = sheet.GetRow(row).GetCell(4).ToString();
                                                    string Product_group = sheet.GetRow(row).GetCell(5).ToString();
                                                    string Release_date = sheet.GetRow(row).GetCell(6).ToString();
                                                    string Units_shipped = sheet.GetRow(row).GetCell(7).ToString();
                                                    string Shipped_COGS = sheet.GetRow(row).GetCell(8).ToString();
                                                    string Units_at_Amazon_FC = sheet.GetRow(row).GetCell(9).ToString();
                                                    string EOM_Sellable_On_Hand_Cost = sheet.GetRow(row).GetCell(10).ToString();
                                                    string EOM_Unsellable_On_Hand_Units = sheet.GetRow(row).GetCell(11).ToString();
                                                    string EOM_Unsellable_On_Hand_Cost = sheet.GetRow(row).GetCell(12).ToString();
                                                    string Vendor_Units_Received = sheet.GetRow(row).GetCell(13).ToString();
                                                    string Open_PO_Qty = sheet.GetRow(row).GetCell(14).ToString();
                                                    string Unfilled_Customer_Ordered_Units = sheet.GetRow(row).GetCell(15).ToString();
                                                    string Units_returned_by_customers = sheet.GetRow(row).GetCell(16).ToString();
                                                    string Category = sheet.GetRow(row).GetCell(17).ToString();
                                                    string Sub_category = sheet.GetRow(row).GetCell(18).ToString();
                                                    string Model_Number = sheet.GetRow(row).GetCell(19).ToString();
                                                    string Catalogue_Number = sheet.GetRow(row).GetCell(20).ToString();
                                                    string Replenishment_Code_Item_Availability = sheet.GetRow(row).GetCell(21).ToString();
                                                    string IOG = "";
                                                    if (iS == 3)
                                                    {
                                                        IOG = sheet.GetRow(row).GetCell(3).ToString();
                                                        ID_type = sheet.GetRow(row).GetCell(4).ToString();
                                                        External_ID = sheet.GetRow(row).GetCell(5).ToString();
                                                        Product_group = sheet.GetRow(row).GetCell(6).ToString();
                                                        Release_date = sheet.GetRow(row).GetCell(7).ToString();
                                                        Units_shipped = sheet.GetRow(row).GetCell(8).ToString();
                                                        Shipped_COGS = sheet.GetRow(row).GetCell(9).ToString();
                                                        Units_at_Amazon_FC = sheet.GetRow(row).GetCell(10).ToString();
                                                        EOM_Sellable_On_Hand_Cost = sheet.GetRow(row).GetCell(11).ToString();
                                                        EOM_Unsellable_On_Hand_Units = sheet.GetRow(row).GetCell(12).ToString();
                                                        EOM_Unsellable_On_Hand_Cost = sheet.GetRow(row).GetCell(13).ToString();
                                                        Vendor_Units_Received = sheet.GetRow(row).GetCell(14).ToString();
                                                        Open_PO_Qty = sheet.GetRow(row).GetCell(15).ToString();
                                                        Unfilled_Customer_Ordered_Units = sheet.GetRow(row).GetCell(16).ToString();
                                                        Units_returned_by_customers = sheet.GetRow(row).GetCell(17).ToString();
                                                        Category = sheet.GetRow(row).GetCell(18).ToString();
                                                        Sub_category = sheet.GetRow(row).GetCell(19).ToString();
                                                        Model_Number = sheet.GetRow(row).GetCell(20).ToString();
                                                        Catalogue_Number = sheet.GetRow(row).GetCell(21).ToString();
                                                        Replenishment_Code_Item_Availability = sheet.GetRow(row).GetCell(22).ToString();
                                                    }








                                                    if (IOG != "" && localeValue != "DIY")
                                                    {
                                                        bool fal = true;
                                                    }



                                                    string loadString = string.Format(@"{0}pdivp{1}pdivp{2}pdivp{3}pdivp{4}pdivp{5}pdivp{6}pdivp{7}pdivp{8}pdivp{9}pdivp{10}pdivp{11}pdivp{12}pdivp{13}pdivp{14}pdivp{15}pdivp{16}pdivp{17}pdivp{18}pdivp{19}pdivp{20}pdivp{21}pdivp{22}pdivp{23}",
                                                           localeValue, ASIN, ASIN_name, Manufacturer, IOG, ID_type, External_ID, Product_group, Release_date, Units_shipped, Shipped_COGS, Units_at_Amazon_FC, EOM_Sellable_On_Hand_Cost, EOM_Unsellable_On_Hand_Units, EOM_Unsellable_On_Hand_Cost, Vendor_Units_Received, Open_PO_Qty, Unfilled_Customer_Ordered_Units, Units_returned_by_customers, Category, Sub_category, Model_Number, Catalogue_Number, Replenishment_Code_Item_Availability);


                                                    loadString = loadString.Replace("-", "").Replace(",", "");
                                                    loadString = loadString.Replace("pdivp", ",");
                                                    try
                                                    {
                                                        DataRow rowDR = SqlBulkLoader.parseValuesIntoDataRow(table.NewRow(), loadString, ',');
                                                        bcp.LoadItem(rowDR);
                                                        counter++;
                                                    }
                                                    catch
                                                    {
                                                        bool failure = true;
                                                    }


                                                }
                                                catch (Exception ex)
                                                {
                                                    bool failure = true;
                                                }
                                            }
                                        }
                                    }

                                    bool finish = true;
                                    bcp.Flush();
                                    conn.Close();
                                    int retailerID = 8;
                                    string sqlHeaderInsertID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalSalesEposFiles
						output inserted.ID into @IDNL values (5,{0},'Epos/Stock','{1}',getdate())

						select ID from @IDNL", retailerID, emailFilename)).ToString();

                                    string sqlLinesInsert = string.Format(@"insert into MSE_PortalamazonfullReportLines
						select {0},* from productdataloader_portalepossales_amazonfull_tempload", sqlHeaderInsertID);

                                    Common.runSQLNonQuery(sqlLinesInsert);
                                    string spRun = string.Format("sp_portalepossalesimport_postload_amazon '{0}',{1}", emailFilename, sqlHeaderInsertID);
                                    Common.runSQLNonQuery(spRun);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        public static void runScrewfixSalesImport()
        {
            Common.log("Beginng Screwfix Import");
            Common.runSQLNonQuery("DELETE from MSE_appleemailfiles where filename like '%screwfix%'");
            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portalepossales_screwfix_temploadnew";
            SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
            conn.Open();
            DataTable table = SqlBulkLoader.getSchemaForTable(temploadTableName, conn);
            SqlBulkLoader bcp = new SqlBulkLoader(table, temploadTableName, conn, true);
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                Mailbox mailBox = client.SelectMailbox("inbox/Hive/Emails/Screwfix");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                            {
                                DateTime dateString = msg.Date;
                                string emailFilename = downloadAttachment(attachment, downloadPath, "Screwfix", msg.MessageId);
                                if (emailFilename != "")
                                {
                                    //HSSFWorkbook hssfwb;
                                    XSSFWorkbook hssfwb;
                                    using (FileStream file = new FileStream(downloadPath + @"\" + "Screwfix" + @"\" + emailFilename, FileMode.Open, FileAccess.Read))
                                    {
                                        hssfwb = new XSSFWorkbook(file);
                                    }

                                    int intakeErrorCount = 0;
                                    ISheet sheet = hssfwb.GetSheet("Sheet1");
                                    ISheet sheetOrders = hssfwb.GetSheet("ORDERS");
                                    string headingtitle1 = sheet.GetRow(0).GetCell(7).ToString();
                                    string headingtitle2 = sheet.GetRow(0).GetCell(8).ToString();
                                    string headingtitle3 = sheet.GetRow(0).GetCell(9).ToString();
                                    string headingtitle4 = sheet.GetRow(0).GetCell(10).ToString();
                                    string headingtitle5 = sheet.GetRow(0).GetCell(11).ToString();
                                    string headingtitle6 = sheet.GetRow(0).GetCell(12).ToString();
                                    string headingtitle7 = sheet.GetRow(0).GetCell(13).ToString();
                                    string headingtitle8 = sheet.GetRow(0).GetCell(14).ToString();
                                    string headingtitle9 = sheet.GetRow(0).GetCell(15).ToString();
                                    string headingtitle10 = sheet.GetRow(0).GetCell(16).ToString();
                                    string headingtitle11 = sheet.GetRow(0).GetCell(17).ToString();
                                    string headingtitle12 = sheet.GetRow(0).GetCell(18).ToString();
                                    string headingtitle13 = sheet.GetRow(0).GetCell(19).ToString();
                                    string headingtitle14 = sheet.GetRow(0).GetCell(20).ToString();
                                    string headingtitle15 = sheet.GetRow(0).GetCell(21).ToString();
                                    string headingtitle16 = sheet.GetRow(0).GetCell(22).ToString();
                                    string headingtitle17 = sheet.GetRow(0).GetCell(23).ToString();
                                    string headingtitle18 = sheet.GetRow(0).GetCell(24).ToString();

                                    string headingtitle19 = sheet.GetRow(0).GetCell(25).ToString();
                                    string headingtitle20 = sheet.GetRow(0).GetCell(26).ToString();
                                    string headingtitle21 = sheet.GetRow(0).GetCell(27).ToString();
                                    string headingtitle22 = sheet.GetRow(0).GetCell(28).ToString();
                                    for (int row = 2; row <= sheet.LastRowNum; row++)
                                    {
                                        if (sheet.GetRow(row) != null) //null is when the row only contains empty cells 
                                        {
                                            IRow ir = sheet.GetRow(row);
                                            string nulltest = "";
                                            try
                                            {
                                                nulltest = sheet.GetRow(row).GetCell(0).ToString();
                                            }
                                            catch
                                            {
                                                continue;
                                            }
                                            try
                                            {

                                                string text = sheet.GetRow(row).GetCell(0).ToString();
                                                string prod_cd = sheet.GetRow(row).GetCell(1).ToString();
                                                string chan_id = sheet.GetRow(row).GetCell(2).ToString();
                                                string userstring_18 = sheet.GetRow(row).GetCell(3).ToString();
                                                string prod_descrp = sheet.GetRow(row).GetCell(4).ToString();
                                                string SFX_Current_Stock_holding = sheet.GetRow(row).GetCell(5).ToString();
                                                string Total_currenlty_on_order = sheet.GetRow(row).GetCell(6).ToString();

                                                string loadString = string.Format(@"{0}pdivp{1}pdivp{2}pdivp{3}pdivp{4}pdivp{5}pdivp{6}pdivp{7}pdivp{8}pdivp{9}pdivp{10}pdivp{11}pdivp{12}pdivp{13}pdivp{14}pdivp{15}pdivp{16}pdivp{17}pdivp{18}pdivp{19}pdivp{20}pdivp{21}pdivp{22}pdivp{23}pdivp{24}pdivp{25}pdivp{26}pdivp{27}pdivp{28}",
                                                        text, prod_cd, chan_id, userstring_18, prod_descrp, SFX_Current_Stock_holding, Total_currenlty_on_order,
                                                        headingtitle1,
headingtitle2,
headingtitle3,
headingtitle4,
headingtitle5,
headingtitle6,
headingtitle7,
headingtitle8,
headingtitle9,
headingtitle10,
headingtitle11,
headingtitle12,
headingtitle13,
headingtitle14,
headingtitle15,
headingtitle16,
headingtitle17,
headingtitle18,
headingtitle19,
headingtitle20,
headingtitle21,
headingtitle22);
                                                loadString = loadString.Replace("-", "").Replace(",", "");
                                                loadString = loadString.Replace("pdivp", ",");
                                                try
                                                {
                                                    DataRow rowDR = SqlBulkLoader.parseValuesIntoDataRow(table.NewRow(), loadString, ',');
                                                    bcp.LoadItem(rowDR);
                                                    counter++;
                                                }
                                                catch
                                                {
                                                    bool failure = true;
                                                }


                                            }
                                            catch (Exception ex)
                                            {
                                                bool failure = true;
                                            }
                                        }
                                    }
                                    bool finish = true;
                                    bcp.Flush();
                                    conn.Close();
                                    int retailerID = 7;
                                    string sqlHeaderInsertID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalSalesEposFiles
						output inserted.ID into @IDNL values (5,{0},'Epos/Stock','{1}',getdate())

						select ID from @IDNL", retailerID, emailFilename)).ToString();

                                    string sqlLinesInsert = string.Format(@"insert into MSE_PortalScrewfixnReportLinesnew
						select {0},* from productdataloader_portalepossales_screwfix_temploadnew", sqlHeaderInsertID);

                                    Common.runSQLNonQuery(sqlLinesInsert);
                                    string spRun = string.Format("sp_portalepossalesimport_postload_screwfixnew '{0}',{1}", emailFilename, sqlHeaderInsertID);
                                    Common.runSQLNonQuery(spRun);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }



        public static void runDixonsSalesImport()
        {
            Common.log("Beginng Dixons Import");
            Common.runSQLNonQuery("DELETE from MSE_appleemailfiles where filename like '%Book2%'");
            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portalepossales_dixons_tempload";
            SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
            conn.Open();
            DataTable table = SqlBulkLoader.getSchemaForTable(temploadTableName, conn);
            SqlBulkLoader bcp = new SqlBulkLoader(table, temploadTableName, conn, true);
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                Mailbox mailBox = client.SelectMailbox("inbox/Hive/Emails/Dixons");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                            {
                                DateTime dateString = msg.Date;
                                string emailFilename = downloadAttachment(attachment, downloadPath, "Dixons", msg.MessageId);
                                if (emailFilename != "")
                                {
                                    HSSFWorkbook hssfwb;
                                    //XSSFWorkbook hssfwb;
                                    using (FileStream file = new FileStream(downloadPath + @"\" + "Dixons" + @"\" + emailFilename, FileMode.Open, FileAccess.Read))
                                    {
                                        hssfwb = new HSSFWorkbook(file);
                                    }

                                    int intakeErrorCount = 0;
                                    ISheet sheet = hssfwb.GetSheet("Sheet1");

                                    for (int row = 1; row <= sheet.LastRowNum; row++)
                                    {
                                        if (sheet.GetRow(row) != null) //null is when the row only contains empty cells 
                                        {
                                            IRow ir = sheet.GetRow(row);
                                            string nulltest = "";
                                            try
                                            {
                                                nulltest = sheet.GetRow(row).GetCell(0).ToString();
                                            }
                                            catch
                                            {
                                                continue;
                                            }
                                            try
                                            {

                                                string Period = sheet.GetRow(row).GetCell(0).ToString();
                                                string Account_Number = sheet.GetRow(row).GetCell(1).ToString();
                                                string Customer_Name = sheet.GetRow(row).GetCell(2).ToString();
                                                string Customer_Xref = sheet.GetRow(row).GetCell(3).ToString();
                                                string Supplier = sheet.GetRow(row).GetCell(4).ToString();
                                                string Stk_Grp_Desc = sheet.GetRow(row).GetCell(5).ToString();
                                                string Whs_Item = sheet.GetRow(row).GetCell(6).ToString();
                                                string Barcode = sheet.GetRow(row).GetCell(7).ToString();
                                                string Part_Number = sheet.GetRow(row).GetCell(8).ToString();
                                                string Item_Name = sheet.GetRow(row).GetCell(9).ToString();
                                                string Item_Desc = sheet.GetRow(row).GetCell(10).ToString();
                                                string Week1SOH = sheet.GetRow(row).GetCell(11).ToString();
                                                string Week2SOH = sheet.GetRow(row).GetCell(12).ToString();
                                                string Week3SOH = sheet.GetRow(row).GetCell(13).ToString();
                                                string Week4SOH = sheet.GetRow(row).GetCell(14).ToString();
                                                string Week5SOH = sheet.GetRow(row).GetCell(15).ToString();
                                                string Total_Sales = sheet.GetRow(row).GetCell(16).ToString();
                                                string SalesWeek1 = sheet.GetRow(row).GetCell(17).ToString();
                                                string SalesWeek2 = sheet.GetRow(row).GetCell(18).ToString();
                                                string SalesWeek3 = sheet.GetRow(row).GetCell(19).ToString();
                                                string SalesWeek4 = sheet.GetRow(row).GetCell(20).ToString();
                                                string SalesWeek5 = sheet.GetRow(row).GetCell(21).ToString();
                                                string CurSales1 = sheet.GetRow(row).GetCell(22).ToString();
                                                string CurSales2 = sheet.GetRow(row).GetCell(23).ToString();
                                                string CurSales3 = sheet.GetRow(row).GetCell(24).ToString();
                                                string CurSales4 = sheet.GetRow(row).GetCell(25).ToString();
                                                string CurSales5 = sheet.GetRow(row).GetCell(26).ToString();
                                                string CurSales6 = sheet.GetRow(row).GetCell(27).ToString();
                                                string _6Months_Plus = sheet.GetRow(row).GetCell(28).ToString();
                                                string Status = sheet.GetRow(row).GetCell(29).ToString();
                                                string Consignment = sheet.GetRow(row).GetCell(30).ToString();


                                                string loadString = string.Format(@"{0}pdivp{1}pdivp{2}pdivp{3}pdivp{4}pdivp{5}pdivp{6}pdivp{7}pdivp{8}pdivp{9}pdivp{10}pdivp{11}pdivp{12}pdivp{13}pdivp{14}pdivp{15}pdivp{16}pdivp{17}pdivp{18}pdivp{19}pdivp{20}pdivp{21}pdivp{22}pdivp{23}pdivp{24}pdivp{25}pdivp{26}pdivp{27}pdivp{28}pdivp{29}pdivp{30}",
                                                       Period, Account_Number, Customer_Name, Customer_Xref, Supplier, Stk_Grp_Desc, Whs_Item, Barcode, Part_Number, Item_Name, Item_Desc, Week1SOH, Week2SOH, Week3SOH, Week4SOH, Week5SOH, Total_Sales, SalesWeek1, SalesWeek2, SalesWeek3, SalesWeek4, SalesWeek5, CurSales1, CurSales2, CurSales3, CurSales4, CurSales5, CurSales6, _6Months_Plus, Status, Consignment);
                                                loadString = loadString.Replace("-", "").Replace(",", "");
                                                loadString = loadString.Replace("pdivp", ",");
                                                try
                                                {
                                                    DataRow rowDR = SqlBulkLoader.parseValuesIntoDataRow(table.NewRow(), loadString, ',');
                                                    bcp.LoadItem(rowDR);
                                                    counter++;
                                                }
                                                catch
                                                {
                                                    bool failure = true;
                                                }


                                            }
                                            catch (Exception ex)
                                            {
                                                bool failure = true;
                                            }
                                        }
                                    }
                                    bool finish = true;
                                    bcp.Flush();
                                    conn.Close();
                                    int retailerID = 2;
                                    string sqlHeaderInsertID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalSalesEposFiles
						output inserted.ID into @IDNL values (5,{0},'Epos/Stock','{1}',getdate())

						select ID from @IDNL", retailerID, emailFilename)).ToString();

                                    string sqlLinesInsert = string.Format(@"insert into MSE_PortalDixonsLines
						select {0},* from productdataloader_portalepossales_dixons_tempload", sqlHeaderInsertID);

                                    Common.runSQLNonQuery(sqlLinesInsert);
                                    string spRun = string.Format("sp_portalepossalesimport_postload_dixons '{0}',{1}", emailFilename, sqlHeaderInsertID);
                                    Common.runSQLNonQuery(spRun);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        public static void runScrewfixSalesImportOld()
        {
            Common.log("Beginng Screwfix Import");
            Common.runSQLNonQuery("DELETE from MSE_appleemailfiles where filename like '%supplierinfo%'");
            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portalepossales_screwfix_tempload";
            SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
            conn.Open();
            DataTable table = SqlBulkLoader.getSchemaForTable(temploadTableName, conn);
            SqlBulkLoader bcp = new SqlBulkLoader(table, temploadTableName, conn, true);
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                Mailbox mailBox = client.SelectMailbox("inbox/Hive/Emails/Screwfix");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                            {
                                DateTime dateString = msg.Date;
                                string emailFilename = downloadAttachment(attachment, downloadPath, "Screwfix", msg.MessageId);
                                if (emailFilename != "")
                                {
                                    HSSFWorkbook hssfwb;

                                    using (FileStream file = new FileStream(downloadPath + @"\" + "Screwfix" + @"\" + emailFilename, FileMode.Open, FileAccess.Read))
                                    {
                                        hssfwb = new HSSFWorkbook(file);
                                    }

                                    int intakeErrorCount = 0;
                                    ISheet sheet = hssfwb.GetSheet("SupplierInfo");
                                    ISheet sheetOrders = hssfwb.GetSheet("ORDERS");
                                    string headingtitle1 = sheet.GetRow(0).GetCell(8).ToString();
                                    string headingtitle2 = sheet.GetRow(0).GetCell(9).ToString();
                                    string headingtitle3 = sheet.GetRow(0).GetCell(10).ToString();
                                    string headingtitle4 = sheet.GetRow(0).GetCell(11).ToString();
                                    string headingtitle5 = sheet.GetRow(0).GetCell(12).ToString();
                                    string headingtitle6 = sheet.GetRow(0).GetCell(13).ToString();
                                    string headingtitle7 = sheet.GetRow(0).GetCell(14).ToString();
                                    string headingtitle8 = sheet.GetRow(0).GetCell(15).ToString();
                                    string headingtitle9 = sheet.GetRow(0).GetCell(16).ToString();
                                    string headingtitle10 = sheet.GetRow(0).GetCell(17).ToString();
                                    string headingtitle11 = sheet.GetRow(0).GetCell(18).ToString();
                                    string headingtitle12 = sheet.GetRow(0).GetCell(19).ToString();
                                    string headingtitle13 = sheet.GetRow(0).GetCell(20).ToString();
                                    string headingtitle14 = sheet.GetRow(0).GetCell(21).ToString();
                                    string headingtitle15 = sheet.GetRow(0).GetCell(22).ToString();
                                    string headingtitle16 = sheet.GetRow(0).GetCell(23).ToString();
                                    string headingtitle17 = sheet.GetRow(0).GetCell(24).ToString();
                                    for (int row = 1; row < sheet.LastRowNum; row++)
                                    {
                                        if (sheet.GetRow(row) != null) //null is when the row only contains empty cells 
                                        {
                                            IRow ir = sheet.GetRow(row);
                                            string nulltest = "";
                                            try
                                            {
                                                nulltest = sheet.GetRow(row).GetCell(0).ToString();
                                            }
                                            catch
                                            {
                                                continue;
                                            }
                                            try
                                            {

                                                string Supplier_ID = sheet.GetRow(row).GetCell(0).ToString();
                                                string SKU = sheet.GetRow(row).GetCell(1).ToString();
                                                string Description = sheet.GetRow(row).GetCell(2).ToString();
                                                string Policy = sheet.GetRow(row).GetCell(3).ToString();
                                                string Live_Product = sheet.GetRow(row).GetCell(4).ToString();
                                                string Trentham_Warehouse_Current_Stock = sheet.GetRow(row).GetCell(5).ToString();
                                                string Stafford_Warehouse_Current_Stock = sheet.GetRow(row).GetCell(6).ToString();
                                                string TC_Stock = sheet.GetRow(row).GetCell(7).ToString();
                                                string _2013_Full_year_Sales = sheet.GetRow(row).GetCell(8).ToString();
                                                string _2014_Full_year_Sales = sheet.GetRow(row).GetCell(9).ToString();
                                                string _2015_Full_year_Sales = sheet.GetRow(row).GetCell(10).ToString();
                                                string _2016_W01 = sheet.GetRow(row).GetCell(11).ToString();
                                                string _2016_W02 = sheet.GetRow(row).GetCell(12).ToString();
                                                string _2016_W03 = sheet.GetRow(row).GetCell(13).ToString();
                                                string _2016_W04 = sheet.GetRow(row).GetCell(14).ToString();
                                                string _2016_W05 = sheet.GetRow(row).GetCell(15).ToString();
                                                string _2016_W06 = sheet.GetRow(row).GetCell(16).ToString();
                                                string _2016_W07 = sheet.GetRow(row).GetCell(17).ToString();
                                                string _2016_W08 = sheet.GetRow(row).GetCell(18).ToString();
                                                string _2016_W09 = sheet.GetRow(row).GetCell(19).ToString();
                                                string _2016_W10 = sheet.GetRow(row).GetCell(20).ToString();
                                                string _2016_W11 = sheet.GetRow(row).GetCell(21).ToString();
                                                string _2016_W12 = sheet.GetRow(row).GetCell(22).ToString();
                                                string _2016_W13 = sheet.GetRow(row).GetCell(23).ToString();
                                                string _2016_W14 = sheet.GetRow(row).GetCell(24).ToString();
                                                string Trade_point_Sales_WK = sheet.GetRow(row).GetCell(25).ToString();
                                                string Trade_Point_Sales__YTD1 = sheet.GetRow(row).GetCell(26).ToString();
                                                string All_YTD__Sales = sheet.GetRow(row).GetCell(27).ToString();

                                                string loadString = string.Format(@"{0}pdivp{1}pdivp{2}pdivp{3}pdivp{4}pdivp{5}pdivp{6}pdivp{7}pdivp{8}pdivp{9}pdivp{10}pdivp{11}pdivp{12}pdivp{13}pdivp{14}pdivp{15}pdivp{16}pdivp{17}pdivp{18}pdivp{19}pdivp{20}pdivp{21}pdivp{22}pdivp{23}pdivp{24}pdivp{25}pdivp{26}pdivp{27}pdivp{29}pdivp{30}pdivp{31}pdivp{32}pdivp{33}pdivp{34}pdivp{35}pdivp{36}pdivp{37}pdivp{38}pdivp{39}pdivp{40}pdivp{41}pdivp{42}pdivp{43}pdivp{44}",
                                                        Supplier_ID, SKU, Description, Policy, Live_Product, Trentham_Warehouse_Current_Stock, Stafford_Warehouse_Current_Stock, TC_Stock, _2013_Full_year_Sales, _2014_Full_year_Sales, _2015_Full_year_Sales, _2016_W01, _2016_W02, _2016_W03, _2016_W04, _2016_W05, _2016_W06, _2016_W07, _2016_W08, _2016_W09, _2016_W10, _2016_W11, _2016_W12, _2016_W13, _2016_W14, Trade_point_Sales_WK, Trade_Point_Sales__YTD1, All_YTD__Sales,
                                                        headingtitle1,
headingtitle2,
headingtitle3,
headingtitle4,
headingtitle5,
headingtitle6,
headingtitle7,
headingtitle8,
headingtitle9,
headingtitle10,
headingtitle11,
headingtitle12,
headingtitle13,
headingtitle14,
headingtitle15,
headingtitle16,
headingtitle17);
                                                loadString = loadString.Replace("-", "").Replace(",", "");
                                                loadString = loadString.Replace("pdivp", ",");
                                                try
                                                {
                                                    DataRow rowDR = SqlBulkLoader.parseValuesIntoDataRow(table.NewRow(), loadString, ',');
                                                    bcp.LoadItem(rowDR);
                                                    counter++;
                                                }
                                                catch
                                                {
                                                    bool failure = true;
                                                }


                                            }
                                            catch (Exception ex)
                                            {
                                                bool failure = true;
                                            }
                                        }
                                    }
                                    bool finish = true;
                                    bcp.Flush();
                                    conn.Close();
                                    int retailerID = 7;
                                    string sqlHeaderInsertID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalSalesEposFiles
						output inserted.ID into @IDNL values (5,{0},'Epos/Stock','{1}',getdate())

						select ID from @IDNL", retailerID, emailFilename)).ToString();

                                    string sqlLinesInsert = string.Format(@"insert into MSE_PortalScrewfixnReportLines
						select {0},* from productdataloader_portalepossales_screwfix_tempload", sqlHeaderInsertID);

                                    Common.runSQLNonQuery(sqlLinesInsert);
                                    string spRun = string.Format("sp_portalepossalesimport_postload_screwfix '{0}',{1}", emailFilename, sqlHeaderInsertID);
                                    Common.runSQLNonQuery(spRun);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }


        public static void runMaplinHomeSalesImport()
        {
            Common.log("Beginng Maplin Import");
            Common.runSQLNonQuery("DELETE from MSE_appleemailfiles where filename like '%maplin%'");
            string downloadPath = @"C:\Linx-tablets\saleseposfiles\";
            string temploadTableName = "productdataloader_portalepossales_maplin_tempload";
            SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
            conn.Open();
            DataTable table = SqlBulkLoader.getSchemaForTable(temploadTableName, conn);
            SqlBulkLoader bcp = new SqlBulkLoader(table, temploadTableName, conn, true);
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            Imap4Client client = new Imap4Client();
            client.ConnectSsl("outlook.office365.com", 993);
            string inventoryPath = @"\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\";

            try
            {
                string username = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailUsername'").ToString();
                string password = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='EmailPassword'").ToString();
                client.LoginFast(username, password);

                Mailbox mailBox = client.SelectMailbox("inbox/Hive/Emails/MaplinHome");
                for (int i = 1; i <= mailBox.MessageCount; i++)
                {
                    try
                    {
                        Message msg = mailBox.Fetch.MessageObject(i);
                        if (msg.Attachments.Count > 0)
                        {
                            Common.log(msg.MessageId + ": " + msg.Subject);
                            foreach (ActiveUp.Net.Mail.MimePart attachment in msg.Attachments)
                            {
                                DateTime dateString = msg.Date;
                                string emailFilename = downloadAttachment(attachment, downloadPath, "MaplinHome", msg.MessageId);
                                if (emailFilename != "")
                                {
                                    HSSFWorkbook hssfwb;

                                    using (FileStream file = new FileStream(downloadPath + @"\" + "MaplinHome" + @"\" + emailFilename, FileMode.Open, FileAccess.Read))
                                    {
                                        hssfwb = new HSSFWorkbook(file);
                                    }

                                    int intakeErrorCount = 0;
                                    ISheet sheet = hssfwb.GetSheet("Maplin Weekly Sales and Stock R");
                                    ISheet sheetOrders = hssfwb.GetSheet("ORDERS");
                                    string Sales_Period__1Heading = sheet.GetRow(6).GetCell(15).ToString();
                                    string Sales_Period__2Heading = sheet.GetRow(6).GetCell(16).ToString();
                                    string Sales_Period__3Heading = sheet.GetRow(6).GetCell(17).ToString();
                                    string Sales_Period__4Heading = sheet.GetRow(6).GetCell(18).ToString();
                                    string Sales_Period__5Heading = sheet.GetRow(6).GetCell(19).ToString();
                                    string Sales_Period__6Heading = sheet.GetRow(6).GetCell(20).ToString();
                                    string Sales_Period__7Heading = sheet.GetRow(6).GetCell(21).ToString();
                                    string Sales_Period__8Heading = sheet.GetRow(6).GetCell(22).ToString();
                                    string Sales_Period__9Heading = sheet.GetRow(6).GetCell(23).ToString();
                                    string Sales_Period__10Heading = sheet.GetRow(6).GetCell(24).ToString();
                                    string Sales_Period__11Heading = sheet.GetRow(6).GetCell(25).ToString();
                                    string Sales_Period__12Heading = sheet.GetRow(6).GetCell(26).ToString();
                                    string Sales_Period__13Heading = sheet.GetRow(6).GetCell(27).ToString();
                                    for (int row = 7; row < sheet.LastRowNum; row++)
                                    {
                                        if (sheet.GetRow(row) != null) //null is when the row only contains empty cells 
                                        {
                                            IRow ir = sheet.GetRow(row);
                                            string nulltest = "";
                                            try
                                            {
                                                nulltest = sheet.GetRow(row).GetCell(0).ToString();
                                            }
                                            catch
                                            {
                                                continue;
                                            }
                                            try
                                            {
                                                string As_at_date = sheet.GetRow(row).GetCell(0).ToString();
                                                string Maplin_YPW = sheet.GetRow(row).GetCell(2).ToString();
                                                string SKU = sheet.GetRow(row).GetCell(3).ToString();
                                                string SKU_Description = sheet.GetRow(row).GetCell(4).ToString();
                                                string Supplier_Product_Code = sheet.GetRow(row).GetCell(5).ToString();
                                                string Supplier_Description = sheet.GetRow(row).GetCell(8).ToString();
                                                string Sales_LW = sheet.GetRow(row).GetCell(10).ToString();
                                                string Sales_PTD = sheet.GetRow(row).GetCell(11).ToString();
                                                string Sales_YTD = sheet.GetRow(row).GetCell(12).ToString();
                                                string Sales_MAT = sheet.GetRow(row).GetCell(13).ToString();
                                                string Sales_WonW = sheet.GetRow(row).GetCell(14).ToString();
                                                string Sales_Period__1 = sheet.GetRow(row).GetCell(15).ToString();
                                                string Sales_Period__2 = sheet.GetRow(row).GetCell(16).ToString();
                                                string Sales_Period__3 = sheet.GetRow(row).GetCell(17).ToString();
                                                string Sales_Period__4 = sheet.GetRow(row).GetCell(18).ToString();
                                                string Sales_Period__5 = sheet.GetRow(row).GetCell(19).ToString();
                                                string Sales_Period__6 = sheet.GetRow(row).GetCell(20).ToString();
                                                string Sales_Period__7 = sheet.GetRow(row).GetCell(21).ToString();
                                                string Sales_Period__8 = sheet.GetRow(row).GetCell(22).ToString();
                                                string Sales_Period__9 = sheet.GetRow(row).GetCell(23).ToString();
                                                string Sales_Period__10 = sheet.GetRow(row).GetCell(24).ToString();
                                                string Sales_Period__11 = sheet.GetRow(row).GetCell(25).ToString();
                                                string Sales_Period__12 = sheet.GetRow(row).GetCell(26).ToString();
                                                string Sales_Period__13 = sheet.GetRow(row).GetCell(27).ToString();






                                                string Stock_WH = sheet.GetRow(row).GetCell(28).ToString();
                                                string Stock_Limbo = sheet.GetRow(row).GetCell(29).ToString();
                                                string Stock_Stores = sheet.GetRow(row).GetCell(30).ToString();
                                                string Stock_Returns_Centre = sheet.GetRow(row).GetCell(31).ToString();
                                                string Stock_Total = sheet.GetRow(row).GetCell(32).ToString();
                                                string Stock_Returns_At_Supplier = sheet.GetRow(row).GetCell(33).ToString();
                                                string Stock_Total_inc_Returns_At_Supplier = sheet.GetRow(row).GetCell(34).ToString();
                                                string Stock_On_Order = sheet.GetRow(row).GetCell(35).ToString();
                                                string Mail_Order_Buffer = sheet.GetRow(row).GetCell(26).ToString();
                                                string Lead_Time = sheet.GetRow(row).GetCell(37).ToString();
                                                string Weeks_Cover = sheet.GetRow(row).GetCell(38).ToString();
                                                string Weeks_Cover_inc_On_Order = sheet.GetRow(row).GetCell(39).ToString();
                                                string Supplier_No = sheet.GetRow(row).GetCell(40).ToString();
                                                string Supplier_Name = sheet.GetRow(row).GetCell(41).ToString();
                                                string Manufacturer = sheet.GetRow(row).GetCell(42).ToString();
                                                string Buy_Price_current_WAC = sheet.GetRow(row).GetCell(43).ToString();
                                                string Retail_Price = sheet.GetRow(row).GetCell(44).ToString();
                                                string WSL_DIS_Memo = sheet.GetRow(row).GetCell(45).ToString();
                                                string Stock_Outs = sheet.GetRow(row).GetCell(46).ToString();
                                                string Total_Stores = sheet.GetRow(row).GetCell(47).ToString();
                                                string Availablity = sheet.GetRow(row).GetCell(48).ToString();

                                                string loadString = string.Format(@"{0}pdivp{1}pdivp{2}pdivp{3}pdivp{4}pdivp{5}pdivp{6}pdivp{7}pdivp{8}pdivp{9}pdivp{10}pdivp{11}pdivp{12}pdivp{13}pdivp{14}pdivp{15}pdivp{16}pdivp{17}pdivp{18}pdivp{19}pdivp{20}pdivp{21}pdivp{22}pdivp{23}pdivp{24}pdivp{25}pdivp{26}pdivp{27}pdivp{28}pdivp{29}pdivp{30}pdivp{31}pdivp{32}pdivp{33}pdivp{34}pdivp{35}pdivp{36}pdivp{37}pdivp{38}pdivp{39}pdivp{40}pdivp{41}pdivp{42}pdivp{43}pdivp{44}pdivp{45}pdivp{46}pdivp{47}pdivp{48}pdivp{49}pdivp{50}pdivp{51}pdivp{52}pdivp{53}pdivp{54}pdivp{55}pdivp{56}",
                                                          As_at_date, Maplin_YPW, SKU, SKU_Description, Supplier_Product_Code, Supplier_Description, Sales_LW, Sales_PTD, Sales_YTD, Sales_MAT, Sales_WonW, Sales_Period__1, Sales_Period__2, Sales_Period__3, Sales_Period__4, Sales_Period__5, Sales_Period__6, Sales_Period__7, Sales_Period__8, Sales_Period__9, Sales_Period__10, Sales_Period__11, Sales_Period__12, Sales_Period__13, Stock_WH, Stock_Limbo, Stock_Stores, Stock_Returns_Centre, Stock_Total, Stock_Returns_At_Supplier, Stock_Total_inc_Returns_At_Supplier, Stock_On_Order, Mail_Order_Buffer, Lead_Time, Weeks_Cover, Weeks_Cover_inc_On_Order, Supplier_No, Supplier_Name, Manufacturer, Buy_Price_current_WAC, Retail_Price, WSL_DIS_Memo, Stock_Outs, Total_Stores, Availablity,
                                                          Sales_Period__1Heading,
                                                Sales_Period__2Heading,
                                                Sales_Period__3Heading,
                                                Sales_Period__4Heading,
                                                Sales_Period__5Heading,
                                                Sales_Period__6Heading,
                                                Sales_Period__7Heading,
                                                Sales_Period__8Heading,
                                                Sales_Period__9Heading,
                                                Sales_Period__10Heading,
                                                Sales_Period__11Heading,
                                                Sales_Period__12Heading,
                                                Sales_Period__13Heading);
                                                loadString = loadString.Replace("-", "").Replace(",", "");
                                                loadString = loadString.Replace("pdivp", ",");
                                                try
                                                {
                                                    DataRow rowDR = SqlBulkLoader.parseValuesIntoDataRow(table.NewRow(), loadString, ',');
                                                    bcp.LoadItem(rowDR);
                                                    counter++;
                                                }
                                                catch
                                                {
                                                    bool failure = true;
                                                }


                                            }
                                            catch (Exception ex)
                                            {
                                                bool failure = true;
                                            }
                                        }
                                    }
                                    bool finish = true;
                                    bcp.Flush();
                                    conn.Close();
                                    int retailerID = 6;
                                    string sqlHeaderInsertID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalSalesEposFiles
						output inserted.ID into @IDNL values (5,{0},'Epos/Stock','{1}',getdate())

						select ID from @IDNL", retailerID, emailFilename)).ToString();

                                    string sqlLinesInsert = string.Format(@"insert into MSE_PortalMaplinReportLines
						select {0},* from productdataloader_portalepossales_maplin_tempload", sqlHeaderInsertID);

                                    Common.runSQLNonQuery(sqlLinesInsert);
                                    string spRun = string.Format("sp_portalepossalesimport_postload_maplin '{0}',{1}", emailFilename, sqlHeaderInsertID);
                                    Common.runSQLNonQuery(spRun);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
