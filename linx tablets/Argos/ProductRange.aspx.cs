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
using System.Data.SqlClient;

namespace linx_tablets.Argos
{
    public partial class ProductRange : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

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
        protected void btnDownloadProductRange_Click(object sender, EventArgs e)
        {
            string filename = "Argos_Product_range_" + Common.timestamp() + ".csv";
            runReport("[sp_portal_productrange_download_existing] 4", filename);
        }
        protected void btnUploadArgosProduct_Click(object sender, EventArgs e)
        {
            string tempLoadTable = "productdataloader_argosProductRange_tempload";
            if (fupArgosProduct.HasFile)
            {
                Common.runSQLNonQuery("delete from " + tempLoadTable);
                string filename = Path.GetFileNameWithoutExtension(fupArgosProduct.FileName) + "_" + Common.timestamp() + Path.GetExtension(fupArgosProduct.FileName);
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fupArgosProduct.SaveAs(filePathLocale + filename);

                    }
                    catch
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('File save failure');", true);
                    }
                    string reportData = File.ReadAllText(filePathLocale + filename, Encoding.Default);
                    bool bypass = false;
                    //    if (!bypass)
                    //    {
                    //        reportData = Regex.Replace(reportData,
                    //@",(?=[^""]*""(?:[^""]*""[^""]*"")*[^""]*$)",
                    //String.Empty);
                    //    }

                    var parts = reportData.Split('"');

                    for (var i = 1; i < parts.Length; i += 2)
                    {
                        parts[i] = parts[i].Replace(",", "");
                    }

                    reportData = string.Join("\"", parts);
                    string amendedFileName = "Amended" + filename;

                    File.AppendAllText(filePathLocale + amendedFileName, reportData, Encoding.Default);

                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/portalUploadedFiles/", "exertissdg", "Exertissdg1");
                    try
                    {
                        ftpClient.uploadFile(filePathLocale + amendedFileName);
                    }
                    catch (Exception ex)
                    {
                    }
                    string newFilename = @"\\10.16.72.129\company\FTP\root\MSESRVDOM\exertissdg\portalUploadedFiles\" + amendedFileName;
                    string bulkInsert = string.Format(@"BULK INSERT " + tempLoadTable + @" FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);

                    
                    Common.runSQLNonQuery(bulkInsert);
                    Common.runSQLNonQuery("update productdataloader_argosProductRange_tempload set catno=replace(catno,'\"',''),Exertis_code=replace(Exertis_Code,'\"','')");
                    Common.runSQLNonQuery("update productdataloader_argosProductRange_tempload set catno=case when substring(catno,1,1)='''' then substring(catno,2,len(catno)-1) else catno end");

                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempLoadTable).ToString()) == 0)
                        throw new Exception("Table empty");

                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempLoadTable + @" where (catno is null)").ToString()) > 0)
                        throw new Exception("Ensure Catno (columns a) is populated for every line");

                    if (int.Parse(Common.runSQLScalar("select count(*) from " + tempLoadTable + @" where catno='tbc' and Exertis_code is null").ToString()) > 0)
                        throw new Exception("Ensure Entries where tbc is used for the catno have the exertis part code populated");

                    if (int.Parse(Common.runSQLScalar(@"select coalesce((select count(*) from " + tempLoadTable + @"  
where catno='tbc'
group by catno,Exertis_code having count(Mfr_Part_no)>1),0)

+ coalesce((select count(*) from " + tempLoadTable + @" 
where catno<>'tbc'
group by catno having count(Mfr_Part_no)>1),0)").ToString()) > 0)
                        throw new Exception("Ensure Catno only appears one in the uploaded file unless tbc is used and a exertis part code is supplied");



                    string updateSQL = string.Format("exec sp_argosproductrangeupload {0},'{1}','{2}'", 4, fupArgosProduct.FileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, product range has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report');", true);
                }
            }
        }
    }
}