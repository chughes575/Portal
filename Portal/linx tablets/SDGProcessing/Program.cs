using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MSE_Common;
using System.Data;
using System.Collections;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
namespace EmailReportExports
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "1":
                    processSDGVendorEmails();
                    break;
                case "2":
                    processDSGVendorEmails();
                    break;
                case "3":
                    processJLPVendorEmails();
                    break;
                    case "4":
                    emailTest();
                    break;
            }
        }
        public static void emailTest()
        {
            MailMessage mail = new MailMessage();
                    
                    SmtpClient SmtpServer = new SmtpClient("exertissap-co-uk01i.mail.protection.outlook.com");
                    
                    mail.From = new System.Net.Mail.MailAddress("noreply@exertis-sap.co.uk");
                    mail.To.Add("chris.hughes@exertis.co.uk");
                    
                    mail.Body = "test";
                    SmtpServer.Port = 25;
                    try
                    {

                        SmtpServer.Send(mail);

                    }
                    catch (Exception ex)
                    {
                        Common.log("Error: " + ex.Message);
                    }                    
        }
        public static void processJLPVendorEmails()
        {
            Common.log("Processing JLP Vendor  Emails");
            int customerID = 6;
            int outstandingCount = int.Parse(Common.runSQLScalar(@"select count(*) from MSE_PortalVendorEmails where datepart(week,getdate())=WeekNo and year=datepart(year,getdate()) and customerid=" + customerID).ToString());

            string reportID = "";
            if (outstandingCount == 0)
            {
                reportID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalVendorEmails
						output inserted.ID into @IDNL values (" + customerID + @",datepart(week,getdate()),datepart(year,getdate()))

						select ID from @IDNL")).ToString();

                
            }
            else
            {
                reportID = Common.runSQLScalar(string.Format(@"select id from MSE_PortalVendorEmails where datepart(week,getdate())=WeekNo and year=datepart(year,getdate()) and customerid=" + customerID)).ToString();
            }
            Common.runSQLNonQuery(@"insert into MSE_PortalVendorProcessed
select distinct vendorid," + reportID + @",0 from (
select distinct pv.vendorid
from mse_portalproductrange pr 
inner join mse_portalcustomers pc on pc.customerid=6
left outer join MSE_OracleCustomerSkuMapping sku on sku.CUSTOMER_ITEM_NUMBER=pr.customersku and sku.Customercode=pc.customercode
left outer join mse_oracleproducts op on op.itemid=sku.INVENTORY_ITEM_ID and op.InvOrgID=88
left outer join MSE_PortalVendors pv on pv.VendorName=op.Supplier and pv.Manufacturer=op.Manufacturer and pv.customerid=pc.customerid
where pr.Customersku<>'tbc'  and pv.vendorid is not null
and pv.vendorid not in (select pv.vendorid from MSE_PortalVendorProcessed pv WHERE REPORTID=" + reportID + @")
union
select distinct pv.vendorid from mse_Portaljlpvendorskumappings_tempload tl inner join mse_portalvendors pv on pv.customerid=6
and pv.vendorname=tl.vendorname and pv.manufacturer=tl.manufacturer 
WHERE pv.vendorid not in (select pv.vendorid from MSE_PortalVendorProcessed pv WHERE REPORTID=" + reportID + @")) a");
            int outstandingReportCount = int.Parse(Common.runSQLScalar("select count(*) from MSE_JLPVendorProcessed where processed=0 and reportid=" + reportID).ToString());

            if (outstandingReportCount > 0)
            {

                DataRowCollection drC = Common.runSQLRows(string.Format(@"select distinct pv.* from MSE_PortalVendorProcessed jlp inner join MSE_PortalVendors pv on pv.vendorid=jlp.vendorid
where jlp.reportid=" + reportID + @" and processed=0", reportID));

                foreach (DataRow dr in drC)
                {

                    List<string> fileList = new List<string>(); string filePath = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\sdg\In\Temp files\";
                    string fileName = "Vendor_Sales" + "_" + dr[2].ToString().Replace(":", "") + "_" + dr[3].ToString().Replace(":", "") + "_" + Common.timestamp() + ".csv";
                    DataSet ds = Common.runSQLDataset(string.Format("exec [sp_JLPVendorPOEmail] {0}", dr[0].ToString()));
                    if (ds.Tables[0].Rows.Count == 0 || ds.Tables[0].Rows.Count == 1)
                    {
                        bool ah = true;
                    }
                    string vendorPoLinesContent = Common.dataTableToTextFile(ds.Tables[0], ",", "\r\n", true);

                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 1 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(0*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 2 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(1*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 3 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(2*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 4 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(3*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 5 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(4*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 6 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(5*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 7 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(6*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 8 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(7*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 9 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(8*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 10 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(9*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 11 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(10*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 12 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(11*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 13 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(12*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 14 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(13*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 15 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(14*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 16 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(15*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 17 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(16*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 18 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(17*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 19 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(18*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 20 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(19*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 21 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(20*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 22 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(21*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 23 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(22*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 24 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(23*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 25 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(24*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 26 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(25*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 27 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(26*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 28 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(27*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 29 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(28*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 30 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(29*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 31 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(30*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 32 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(31*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 33 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(32*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 34 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(33*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 35 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(34*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 36 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(35*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 37 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(36*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 38 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(37*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 39 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(38*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 40 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(39*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 41 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(40*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 42 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(41*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 43 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(42*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 44 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(43*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 45 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(44*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 46 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(45*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 47 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(46*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 48 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(47*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 49 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(48*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 50 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(49*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 51 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(50*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 52 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(51*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());

                    if (File.Exists(filePath + fileName))
                        File.Delete(filePath + fileName);

                    File.AppendAllText(filePath + fileName, vendorPoLinesContent);
                    fileList.Add(filePath + fileName);


                    string vendorID = dr[0].ToString();
                    string vendorName = dr[2].ToString();
                    string manufacturerName = dr[3].ToString();
                    string recipients = dr[5].ToString();
                    string exertisContactEmails = dr[8].ToString();

                    string formattedExertisContactEmails = "";
                    //email stuff
                    string[] emailAddresses = recipients.Split(';');
                    string[] emailContactAddresses = exertisContactEmails.Split(';');
                    bool emailValidation = true;
                    bool emailExertisValidation = true;
                    if (exertisContactEmails == "")
                        emailExertisValidation = false;

                    if (recipients == "")
                        emailValidation = false;

                    Regex rgsEmail = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

                    for (int i = 0; i < emailContactAddresses.Length; i++)
                    {

                        if (rgsEmail.IsMatch(emailContactAddresses[i].ToLower()))
                        {
                            formattedExertisContactEmails += emailContactAddresses[i] + "/";
                        }
                        else
                        {
                            emailExertisValidation = false;
                        }
                    }
                    if (formattedExertisContactEmails.Length > 0)
                    {
                        formattedExertisContactEmails = formattedExertisContactEmails.Substring(0, formattedExertisContactEmails.Length - 1);
                    }
                    for (int i = 0; i < emailAddresses.Length; i++)
                    {

                        if (!rgsEmail.IsMatch(emailAddresses[i].ToLower()))
                        {
                            emailValidation = false;
                        }
                    }

                    MailMessage mail = new MailMessage();
                    mail.Subject = string.Format("{0}/{1} : JLP - Stock & Sales", vendorName, manufacturerName);
                    SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                    
                    mail.From = new System.Net.Mail.MailAddress(Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailUsername' and customerid=6").ToString());
                    mail.CC.Add("chris.hughes@exertismse.co.uk");
                    //mail.CC.Add("alina.gavenyte@exertismse.co.uk");
                    mail.CC.Add("alina.gavenyte@exertis.co.uk");
                    
                    mail.To.Add("sam.williams@exertis.co.uk");
                    mail.To.Add("steve.baldwin@exertis.co.uk");
                    bool testing = true;
                    if (emailValidation)
                    {
                        if (!testing)
                        {
                            for (int i1 = 0; i1 < emailAddresses.Length; i1++)
                            {
                                mail.To.Add(emailAddresses[i1]);
                            }
                        }
                    }
                    else
                    {
                        mail.Subject += string.Format(" Missing Vendor/Manuf Email To Forward", vendorName, manufacturerName);
                    }
                    string defaultEmailAddress = "JLPsupplychain@exertis.co.uk";
                    if (!emailExertisValidation)
                    {
                        formattedExertisContactEmails = defaultEmailAddress;
                    }
                    mail.Body = Common.runSQLScalar("select configvalue from portalconfig  where configkey='JLPVendorEmailBody'").ToString().Replace("[VENDORNAME]", vendorName).Replace("[MANUFACTURER]", manufacturerName);
                    mail.Body = mail.Body.Replace("[CONTACTEMAIL]", formattedExertisContactEmails);
                    mail.IsBodyHtml = true;
                    AlternateView av = AlternateView.CreateAlternateViewFromString(Common.runSQLScalar("select configvalue from portalconfig where configkey='JLPVendorEmailBodyHTML'").ToString().Replace("[VENDORNAME]", vendorName).Replace("MANUFACTURER", manufacturerName).Replace("[CONTACTEMAIL]", formattedExertisContactEmails), null, "text/html");
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
                    SmtpServer.Credentials = new System.Net.NetworkCredential(Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailUsername' and customerid=6").ToString(), Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailPassword' and customerid=6").ToString());
                    SmtpServer.EnableSsl = true;
                    string updateSQL = string.Format(@"update MSE_PortalVendorProcessed set Processed=1 where vendorid={0} and reportid={1}", vendorID, reportID);
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

            }
        }
        public static void processDSGVendorEmails()
        {


            int customerID = 2;
            Common.log("Processing DSG Vendor  Emails");
            int outstandingCount = int.Parse(Common.runSQLScalar(@"select count(*) from MSE_PortalVendorEmails where datepart(week,getdate())=WeekNo and customerid=" + customerID).ToString());

            string reportID = "";
            if (outstandingCount == 0)
            {
                reportID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalVendorEmails
						output inserted.ID into @IDNL values (" + customerID + @",datepart(week,getdate()),datepart(year,getdate()))

						select ID from @IDNL")).ToString();
            }
            else
            {
                reportID = Common.runSQLScalar(string.Format(@"select id from MSE_PortalVendorEmails where weekno=datepart(week,getdate()) and customerid="+customerID)).ToString();
            }
            Common.runSQLNonQuery(string.Format(@"sp_dsgVendorInsertVendors {0}", reportID));
            Common.runSQLNonQuery(string.Format(@"sp_dsgVendorDeleteVendors {0}", reportID));

            int outstandingReportCount = int.Parse(Common.runSQLScalar("select count(*) from MSE_PortalVendorProcessed where processed=0 and reportid=" + reportID).ToString());

            if (outstandingReportCount > 0)
            {

                DataRowCollection drC = Common.runSQLRows(string.Format(@"  select distinct pv.* from MSE_PortalVendors pv
inner join MSE_PortalVendorProcessed vp on vp.vendorid=pv.vendorid
where vp.processed=0  and pv.vendorid is not null and vp.reportid={0}", reportID));

                foreach (DataRow dr in drC)
                {

                    List<string> fileList = new List<string>(); string filePath = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\sdg\In\Temp files\";
                    string fileName = "Vendor_Sales" + "_" + dr[2].ToString() + "_" + dr[3].ToString() + "_" + Common.timestamp() + ".csv";
                    DataSet ds = Common.runSQLDataset(string.Format("exec [sp_DSGVendorPOEmail] {0}", dr[0].ToString()));
                    if (ds.Tables[0].Rows.Count == 0)
                    {
                        bool asd = true;
                        continue;
                    }

                    string vendorPoLinesContent = Common.dataTableToTextFile(ds.Tables[0], ",", "\r\n", true);

                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 1 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(0*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 2 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(1*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 3 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(2*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 4 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(3*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 5 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(4*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 6 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(5*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 7 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(6*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 8 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(7*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 9 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(8*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 10 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(9*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 11 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(10*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 12 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(11*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 13 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(12*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 14 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(13*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 15 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(14*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 16 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(15*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 17 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(16*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 18 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(17*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 19 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(18*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 20 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(19*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 21 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(20*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 22 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(21*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 23 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(22*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 24 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(23*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 25 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(24*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 26 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(25*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 27 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(26*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 28 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(27*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 29 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(28*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 30 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(29*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 31 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(30*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 32 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(31*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 33 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(32*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 34 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(33*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 35 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(34*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 36 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(35*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 37 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(36*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 38 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(37*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 39 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(38*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 40 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(39*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 41 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(40*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 42 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(41*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 43 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(42*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 44 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(43*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 45 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(44*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 46 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(45*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 47 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(46*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 48 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(47*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 49 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(48*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 50 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(49*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 51 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(50*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());
                    vendorPoLinesContent = vendorPoLinesContent.Replace("Week 52 Sell Thru", Common.runSQLScalar("select CONVERT(varchar, cast(dateadd(day,(51*7)*-1, dateadd(day, (datepart(weekday,getdate())+1)*-1,getdate()))  AS date), 103)").ToString());

                    if (File.Exists(filePath + fileName))
                        File.Delete(filePath + fileName);

                    File.AppendAllText(filePath + fileName, vendorPoLinesContent);
                    fileList.Add(filePath + fileName);


                    string vendorID = dr[0].ToString();
                    string vendorName = dr[2].ToString();
                    string manufacturerName = dr[3].ToString();
                    string recipients = dr[5].ToString();
                    string exertisContactEmails = dr[8].ToString();

                    string formattedExertisContactEmails = "";
                    //email stuff
                    string[] emailAddresses = recipients.Split(';');
                    string[] emailContactAddresses = exertisContactEmails.Split(';');
                    bool emailValidation = true;
                    bool emailExertisValidation = true;
                    if (exertisContactEmails == "")
                        emailExertisValidation = false;

                    if (recipients == "")
                        emailValidation = false;

                    Regex rgsEmail = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

                    for (int i = 0; i < emailContactAddresses.Length; i++)
                    {

                        if (rgsEmail.IsMatch(emailContactAddresses[i].ToLower()))
                        {
                            formattedExertisContactEmails += emailContactAddresses[i] + "/";
                        }
                        else
                        {
                            emailExertisValidation = false;
                        }
                    }
                    for (int i = 0; i < emailAddresses.Length; i++)
                    {

                        if (!rgsEmail.IsMatch(emailAddresses[i].ToLower()))
                        {
                            emailValidation = false;
                        }
                    }

                    MailMessage mail = new MailMessage();
                    mail.Subject = string.Format("{0}/{1} : DSG - Stock & Sales", vendorName, manufacturerName);
                    SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                    //SmtpClient SmtpServer = new SmtpClient("exertissap-co-uk01i.mail.protection.outlook.com");

                   // mail.From = new System.Net.Mail.MailAddress("noreply@exertis-sap.co.uk");
                    mail.From = new System.Net.Mail.MailAddress(Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailUsername' and customerid=6").ToString());
                    mail.CC.Add("chris.hughes@exertis.co.uk");
                    mail.CC.Add("alina.gavenyte@exertis.co.uk");
                    mail.To.Add("sam.williams@exertis.co.uk");
                    mail.To.Add("steve.baldwin@exertis.co.uk");
                    ;
                    bool testing = true;
                    if (emailValidation)
                    {
                        if (!testing)
                        {
                            for (int i1 = 0; i1 < emailAddresses.Length; i1++)
                            {
                                mail.To.Add(emailAddresses[i1]);
                            }
                        }
                    }
                    else
                    {
                        mail.Subject += string.Format(" Missing Vendor/Manuf Email To Forward", vendorName, manufacturerName);
                    }
                    string defaultEmailAddress = "sam.williams@exertis.co.uk";
                    if (!emailExertisValidation)
                    {
                        formattedExertisContactEmails = defaultEmailAddress;
                    }
                    mail.Body = Common.runSQLScalar("select configvalue from portalconfig  where configkey='DSGVendorEmailBody'").ToString().Replace("[VENDORNAME]", vendorName).Replace("MANUFACTURER", manufacturerName);
                    mail.Body = mail.Body.Replace("[CONTACTEMAIL]", formattedExertisContactEmails);
                    mail.IsBodyHtml = true;
                    AlternateView av = AlternateView.CreateAlternateViewFromString(Common.runSQLScalar("select configvalue from portalconfig where configkey='DSGVendorEmailBodyHTML'").ToString().Replace("[VENDORNAME]", vendorName).Replace("MANUFACTURER", manufacturerName).Replace("[CONTACTEMAIL]", formattedExertisContactEmails), null, "text/html");
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
                    SmtpServer.Port = 25;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailUsername' and customerid=6").ToString(), Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailPassword' and customerid=6").ToString());
                    SmtpServer.EnableSsl = true;
                    string updateSQL = string.Format(@"update MSE_PortalVendorProcessed set Processed=1 where vendorid={0} and reportid={1}", vendorID, reportID);
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

            }
        }
        public static void processSDGVendorEmails()
        {



            Common.log("Processing SDG Vendor  Emails");
            int customerID = 1;
            int outstandingCount = int.Parse(Common.runSQLScalar(@"select count(*) from MSE_PortalVendorEmails where datepart(week,getdate())=WeekNo and year=datepart(year,getdate()) and customerid="+customerID).ToString());

            string reportname = "";
            string reportID = "";
            if (outstandingCount == 0)
            {
                reportID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into MSE_PortalVendorEmails
						output inserted.ID into @IDNL values ("+customerID+@",datepart(week,getdate()),datepart(year,getdate()))

						select ID from @IDNL")).ToString();

                Common.runSQLNonQuery(@"insert into MSE_PortalVendorProcessed
select distinct pv.vendorid," + reportID + @",0 from MSE_SDGProductRange pr 
inner join mse_portalcustomers pc on pc.customerid=1
left outer join MSE_OracleCustomerSkuMapping sku on sku.CUSTOMER_ITEM_NUMBER=pr.Catno and sku.Customercode=pc.customercode
left outer join mse_oracleproducts op on op.itemid=sku.INVENTORY_ITEM_ID and op.InvOrgID=88
left outer join MSE_PortalVendors pv on pv.VendorName=op.Supplier and pv.Manufacturer=op.Manufacturer and pv.customerid=pc.customerid
where pr.Exertislive=1 and pr.Catno<>'tbc'  and pv.vendorid is not null");
            }
            else
            {
                reportID = Common.runSQLScalar(string.Format(@"select id from MSE_PortalVendorEmails where weekno=datepart(week,getdate())")).ToString();
            }

            int outstandingReportCount = int.Parse(Common.runSQLScalar("select count(*) from MSE_PortalVendorProcessed where processed=0 and reportid=" + reportID).ToString());

            if (outstandingReportCount > 0)
            {

                 DataRowCollection drC = Common.runSQLRows(string.Format(@"select pv.* from MSE_PortalVendorProcessed vp
inner join mse_portalvendors pv on pv.vendorid=vp.vendorid
 where vp.reportid={0} and processed=0", reportID));

                

                foreach (DataRow dr in drC)
                {

                    List<string> fileList = new List<string>(); string filePath = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\sdg\In\Temp files\";
                    string fileName = "Vendor_Sales" + "_" + dr[2].ToString() + "_" + dr[3].ToString() + "_" + Common.timestamp() + ".csv";
                    
                    DataSet ds = Common.runSQLDataset(string.Format("[sp_sdgVendorPOEmail] {0},{1}", dr[0].ToString(), reportID));
                    if (ds.Tables[0].Rows.Count == 0)
                        continue;

                    string vendorPoLinesContent = Common.dataTableToTextFile(ds.Tables[0], ",", "\r\n", true);
                    if (File.Exists(filePath + fileName))
                        File.Delete(filePath + fileName);

                    File.AppendAllText(filePath + fileName, vendorPoLinesContent);
                    fileList.Add(filePath + fileName);


                    string vendorID = dr[0].ToString();
                    string vendorName = dr[2].ToString();
                    string manufacturerName = dr[3].ToString();
                    string recipients = dr[5].ToString();
                    string exertisRecipients = dr[8].ToString();

                    //email stuff
                    string[] emailAddresses = recipients.Split(';');
                    string[] eertisEmailAddresses = exertisRecipients.Split(';');

                    bool emailValidation = true;
                    bool exertisEmailValidation = true;

                    if (recipients == "")
                        emailValidation = false;
                    if (exertisRecipients == "")
                        exertisEmailValidation = false;

                    Regex rgsEmail = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

                    //Vendor Recipients
                    for (int i = 0; i < emailAddresses.Length; i++)
                    {

                        if (!rgsEmail.IsMatch(emailAddresses[i].ToLower()))
                        {
                            emailValidation = false;
                        }
                    }

                    //Exertis Staff Recipients
                    for (int i = 0; i < eertisEmailAddresses.Length; i++)
                    {

                        if (!rgsEmail.IsMatch(eertisEmailAddresses[i].ToLower()))
                        {
                            exertisEmailValidation = false;
                        }
                    }


                    MailMessage mail = new MailMessage();
                    mail.Subject = string.Format("{0}/{1} : Shop Direct - Stock & Sales", vendorName, manufacturerName);
                    SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                    mail.From = new System.Net.Mail.MailAddress(Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailUsername' and customerid=1").ToString());
                    mail.CC.Add("chris.hughes@exertis.co.uk");
                    mail.CC.Add("alina.gavenyte@exertis.co.uk");
                    mail.To.Add("sdgsupplychain@exertis.co.uk");
                    bool testing = false;
                    if (emailValidation)
                    {
                        if (!testing)
                        {
                            for (int i1 = 0; i1 < emailAddresses.Length; i1++)
                            {
                                mail.To.Add(emailAddresses[i1]);
                            }
                        }
                    }
                    else
                    {
                        mail.Subject += string.Format(" Missing Vendor/Manuf Email To Forward", vendorName, manufacturerName);
                    }


                    if (exertisEmailValidation)
                    {

                        for (int i1 = 0; i1 < eertisEmailAddresses.Length; i1++)
                            {
                                mail.To.Add(eertisEmailAddresses[i1]);
                            }
                    }
                    mail.Body = Common.runSQLScalar("select configvalue from portalconfig  where configkey='SDGVendorEmailBody'").ToString().Replace("[VENDORNAME]", vendorName).Replace("MANUFACTURER", manufacturerName);

                    mail.IsBodyHtml = true;
                    AlternateView av = AlternateView.CreateAlternateViewFromString(Common.runSQLScalar("select configvalue from portalconfig where configkey='SDGVendorEmailBodyHTML'").ToString().Replace("[VENDORNAME]", vendorName).Replace("MANUFACTURER", manufacturerName), null, "text/html");
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
                    SmtpServer.Credentials = new System.Net.NetworkCredential(Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailUsername' and customerid=1").ToString(), Common.runSQLScalar("select configvalue from portalconfig where configkey='EmailPassword' and customerid=1").ToString());
                    SmtpServer.EnableSsl = true;
                    string updateSQL = string.Format(@"update MSE_PortalVendorProcessed set Processed=1 where vendorid={0} and reportid={1}", vendorID, reportID);
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

            }
        }





    }
}
