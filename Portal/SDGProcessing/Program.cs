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
namespace SDGProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "1":
                    processVendorEmails();
                    break;
            }
        }
        public static void processVendorEmails()
        {



            Common.log("Processing SDG Vendor  Emails");
            int outstandingCount = int.Parse(Common.runSQLScalar(@"select count(*) from MSE_SDGVendorEmails where datepart(week,getdate())=WeekNo").ToString());

            string reportname = "";
            string reportID = "";
            if (outstandingCount == 0)
            {
                reportID = Common.runSQLScalar(string.Format(@"declare @IDNL table (ID int)
						insert into mse_SDgvendoremails
						output inserted.ID into @IDNL values (datepart(week,getdate()))

						select ID from @IDNL")).ToString();

                Common.runSQLNonQuery(@"insert into MSE_SDGVendorProcessed
select distinct pv.vendorid,1,0 from MSE_SDGProductRange pr 
inner join mse_portalcustomers pc on pc.customerid=1
left outer join MSE_OracleCustomerSkuMapping sku on sku.CUSTOMER_ITEM_NUMBER=pr.Catno and sku.Customercode=pc.customercode
left outer join mse_oracleproducts op on op.itemid=sku.INVENTORY_ITEM_ID and op.InvOrgID=88
left outer join MSE_PortalVendors pv on pv.VendorName=op.Supplier and pv.Manufacturer=op.Manufacturer and pv.customerid=pc.customerid
where pr.Exertislive=1 and pr.Catno<>'tbc'  and pv.vendorid is not null");
            }
            else
            {
                reportID = Common.runSQLScalar(string.Format(@"select id from mse_SDgvendoremails where weekno=datepart(week,getdate())")).ToString();
            }

            int outstandingReportCount = int.Parse(Common.runSQLScalar("select count(*) from MSE_SDGVendorProcessed where processed=0 and reportid=" + reportID).ToString());

            if (outstandingReportCount > 0)
            {

                DataRowCollection drC = Common.runSQLRows(string.Format(@"select distinct pv.* from MSE_SDGProductRange pr 
inner join mse_portalcustomers pc on pc.customerid=1
left outer join MSE_OracleCustomerSkuMapping sku on sku.CUSTOMER_ITEM_NUMBER=pr.Catno and sku.Customercode=pc.customercode
left outer join mse_oracleproducts op on op.itemid=sku.INVENTORY_ITEM_ID and op.InvOrgID=88
left outer join MSE_PortalVendors pv on pv.VendorName=op.Supplier and pv.Manufacturer=op.Manufacturer and pv.customerid=pc.customerid
left outer join MSE_SDGVendorProcessed vp on vp.vendorid=pv.vendorid
where pr.Exertislive=1 and pr.Catno<>'tbc'  and pv.vendorid is not null and vp.reportid={0}", reportID));

                foreach (DataRow dr in drC)
                {

                    List<string> fileList = new List<string>(); string filePath = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\sdg\In\Temp files\";
                    string fileName = "Vendor_Sales" + "_" + dr[2].ToString() + ".csv";
                    DataSet ds = Common.runSQLDataset(string.Format("exec [sp_sdgVendorPOEmail] {0},{1}", dr[0].ToString(), reportID));

                    string vendorPoLinesContent = Common.dataTableToTextFile(ds.Tables[0], ",", "\r\n", true);
                    if (File.Exists(filePath + fileName))
                        File.Delete(filePath + fileName);

                    File.AppendAllText(filePath + fileName, vendorPoLinesContent);
                    fileList.Add(filePath + fileName);


                    string vendorID = dr[0].ToString();
                    string vendorName = dr[2].ToString();
                    string recipients = dr[5].ToString();

                    //email stuff
                    string[] emailAddresses = recipients.Split(';');

                    bool emailValidation = true;

                    if (recipients == "")
                        emailValidation = false;

                    Regex rgsEmail = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

                    for (int i = 0; i < emailAddresses.Length; i++)
                    {

                        if (!rgsEmail.IsMatch(emailAddresses[i]))
                        {
                            emailValidation = false;
                        }
                    }


                    MailMessage mail = new MailMessage();
                    mail.Subject = string.Format("{0} : Shop Direct - Stock & Sales", vendorName);
                    SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                    mail.From = new System.Net.Mail.MailAddress("chris.hughes@exertis.co.uk");
                    mail.Bcc.Add("chris.hughes@exertis.co.uk");
                    //mail.Bcc.Add("alina.gavenyte@exertis.co.uk");
                    //mail.To.Add("sdgsupplychain@exertis.co.uk");
                    mail.To.Add("alina.gavenyte@exertis.co.uk");
                    mail.To.Add("alina.gavenyte@exertismse.co.uk");
                    mail.To.Add("chris.hughes@exertis.co.uk");
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
                        mail.Subject += " Missing Vendor/Manuf Email To Forward";
                    }

                    mail.Body = Common.runSQLScalar("select configvalue from portalconfig  where configkey='SDGVendorEmailBody'").ToString().Replace("[VENDORNAME]", vendorName);

                    mail.IsBodyHtml = true;
                    AlternateView av = AlternateView.CreateAlternateViewFromString(Common.runSQLScalar("select configvalue from portalconfig where configkey='SDGVendorEmailBodyHTML'").ToString().Replace("[VENDORNAME]", vendorName), null, "text/html");
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
                    string updateSQL = string.Format(@"update MSE_SDGVendorProcessed set Processed=1 where vendorid={0} and reportid={1}", vendorID, reportID);
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
