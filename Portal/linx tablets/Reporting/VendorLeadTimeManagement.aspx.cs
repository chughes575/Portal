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
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using linx_tablets.SapService;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
namespace linx_tablets.Reporting
{
    public partial class SupplierManagement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            ////Manually reconfigure the binding for SAP-BasicAuth
            //BasicHttpBinding basicAuthBinding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
            //basicAuthBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            //EndpointAddress basicAuthEndpoint = new EndpointAddress("https://exeveed0.os.itelligence.de/sap/bc/srt/wsdl/flv_10002P111AD1/sdef_url/ZZWS_IN002_UPD_ZTAB?sap-client=220");

            //SapService.ZWS_IN002_UPD_ZTABClient client = new SapService.ZWS_IN002_UPD_ZTABClient(basicAuthBinding, basicAuthEndpoint);
            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };


            if (!Page.IsPostBack)
            {
                bindLeadTimes();
            }
        }
        private void test()
        {

            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                        System.Net.Security.SslPolicyErrors sslPolicyErrors)
{
    return true; // **** Always accept
};

            //WSHttpBinding binding = new WSHttpBinding();
            WebHttpBinding binding = new WebHttpBinding();
            binding.Security.Mode = WebHttpSecurityMode.Transport;
            //binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType =
                HttpClientCredentialType.Basic;
            EndpointAddress wsAuthEndpoint = new EndpointAddress("https://exeveed0.os.itelligence.de/sap/bc/srt/wsdl/flv_10002P111AD1/sdef_url/ZZWS_IN002_UPD_ZTAB?sap-client=220");

            SapService.ZWS_IN002_UPD_ZTABClient client = new SapService.ZWS_IN002_UPD_ZTABClient(binding, wsAuthEndpoint);


            client.ClientCredentials.UserName.UserName = "hadley.banks";
            client.ClientCredentials.UserName.Password = "password1!";

            SapService.Z_MMFM_IN002_UPD_ZTABResponse response = new Z_MMFM_IN002_UPD_ZTABResponse();

            SapService.Z_MMFM_IN002_UPD_ZTAB tableCols = new Z_MMFM_IN002_UPD_ZTAB();

            tableCols.IM_MANUF_PART_NO = "";
            tableCols.IM_MANUFACTURER = "";
            tableCols.IM_MATERIAL = "";
            tableCols.IM_TIMESTAMP = "";

            List<ZMMSTR_CHARACT> characList = new List<ZMMSTR_CHARACT>();

            //foreach(characteristic in characteristics)
            //{
            ZMMSTR_CHARACT charac = new ZMMSTR_CHARACT();
            charac.CHARACT_NAME = "";
            charac.CHARACT_VALUE = "";

            characList.Add(charac);
            //}


            tableCols.CH_CHARACT = characList.ToArray();
            try
            {
                client.Z_MMFM_IN002_UPD_ZTAB(tableCols);
            }
            catch (Exception ex)
            {
                string exc = ex.Message;
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
        private void bindLeadTimes()
        {
            gvPOSupplierLeadTimes.DataSource = Common.runSQLDataset(@"select * from mse_appleposuppliers order by SupplierDesc");
            gvPOSupplierLeadTimes.DataBind();
        }
        protected void gvPOSupplierLeadTimes_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.gvPOSupplierLeadTimes.EditIndex = -1;
            this.bindLeadTimes();
        }

        protected void gvPOSupplierLeadTimes_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.gvPOSupplierLeadTimes.EditIndex = e.NewEditIndex;
            this.bindLeadTimes();
        }

        protected void ggvPOSupplierLeadTimes_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow gridViewRow = this.gvPOSupplierLeadTimes.Rows[e.RowIndex];
                string poID = this.gvPOSupplierLeadTimes.DataKeys[e.RowIndex].Value.ToString();
                TextBox txtLeadTime = (TextBox)gridViewRow.FindControl("txtLeadTime");

                Common.runSQLNonQuery(string.Format("update mse_appleposuppliers set leadtime={1} where supplierid={0} ", poID, txtLeadTime.Text));
                this.gvPOSupplierLeadTimes.EditIndex = -1;
                bindLeadTimes();
            }
            catch
            {
            }
        }
        protected void btnPoOverDueRed_Click(object sender, EventArgs e)
        {
            string fileName = "Overdue_PO_report_Red_" + Common.timestamp() + ".csv";
            runReport("exec sp_apple_overduepo_red", fileName);
        }

        protected void btnPoOverDueAmber_Click(object sender, EventArgs e)
        {
            string fileName = "Overdue_PO_report_Amber_" + Common.timestamp() + ".csv";
            runReport("exec sp_apple_overduepo_amber", fileName);
        }

        protected void btnProductLeadTimeDownload_Click(object sender, EventArgs e)
        {
            string filename = "Apple_Product_LeadTime_" + Common.timestamp() + ".csv";
            runReport("select * from mse_appleproductleadtimes order by applecode", filename);

        }
        protected void btnProductLeadTimeUpload_Click(object sender, EventArgs e)
        {
            if (fuProductLeadTime.HasFile)
            {
                Common.runSQLNonQuery("delete from mse_appleproductleadtimes_tempload");
                string filename = Path.GetFileNameWithoutExtension(fuProductLeadTime.FileName) + "_" + Common.timestamp() + Path.GetExtension(fuProductLeadTime.FileName);
                string filePath = @"\\10.16.72.129\company\";
                string filePathLocale = "C:\\Linx-tablets\\replen files\\";
                //do some shit here
                try
                {
                    try
                    {
                        fuProductLeadTime.SaveAs(filePathLocale + filename);

                    }
                    catch
                    {
                        throw new Exception();
                    }

                    //
                    string reportData = File.ReadAllText(filePathLocale + filename, Encoding.Default);
                    string amendedFileName = "Amended" + filename;

                    File.AppendAllText(filePathLocale + amendedFileName, reportData, Encoding.Default);
                    IFTP ftpClient = new FTP("ftp.msent.co.uk", "/apple files/", "extranet", "Extranet1");

                    try
                    {
                        ftpClient.uploadFile(filePathLocale + amendedFileName);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception();
                    }

                    string newFilename = "\\\\10.16.72.129\\company\\ftp\\root\\msesrvdom\\extranet\\apple files\\" + amendedFileName;
                    string bulkInsert = string.Format(@"BULK INSERT mse_appleproductleadtimes_tempload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='0x0a', FIRSTROW = 2, FIRE_TRIGGERS  ) ", newFilename);
                    Common.runSQLNonQuery(bulkInsert);
                    if (int.Parse(Common.runSQLScalar("select count(*) from mse_appleproductleadtimes_tempload").ToString()) == 0)
                        throw new Exception();


                    if (int.Parse(Common.runSQLScalar("select count(*) from mse_appleproductleadtimes_tempload where leadtime is null or coalesce(AppleCode,'')=''").ToString()) > 0)
                        throw new Exception();

                    string updateSQL = string.Format("exec sp_productdataloader_productleadtime '{0}','{1}'", amendedFileName, HttpContext.Current.User.Identity.Name.ToString());
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the stock file has been imported');", true);

                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file');", true);
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

        protected void btnRemoveAllLeadTimes_Click(object sender, EventArgs e)
        {
            string confirmValue = Request.Form["confirm_value"];
            if (confirmValue == "Yes")
            {
                Common.runSQLNonQuery("delete from mse_appleproductleadtimes");
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Lead times removed');", true);
            }
        }
    }
}