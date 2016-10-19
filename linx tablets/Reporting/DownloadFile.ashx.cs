using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;
using System.Text;
using MSE_Common;
namespace linx_tablets.Reporting
{
    /// <summary>
    /// Summary description for Handler1
    /// </summary>
    public class DownloadFile : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
            string localeid = request.QueryString["localeid"];
            string sql = "exec [sp_StockUpLocaleCarton]" + localeid;


            DataSet ds = Common.runSQLDataset(sql);

            DataTable dt = ds.Tables[0];
            int counter = 0;
            foreach (DataTable dt1 in ds.Tables)
            {
                if (dt1.Rows.Count > 1)
                    break;
                    counter++;
            }
            dt = ds.Tables[counter];
            StringBuilder sb = new StringBuilder();
            string fieldDelimiter = ",";
            string rowDelimiter = "\r\n";
            int iColCount = dt.Columns.Count;

            // First we will write the headers.
            if (true)
            {
                for (int i = 0; i < iColCount; i++)
                {
                    sb.Append(dt.Columns[i]);
                    if (i < iColCount - 1)
                    {
                        sb.Append(fieldDelimiter);
                    }
                }
                sb.Append(rowDelimiter);
            }

            // Now write all the rows.
            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i < iColCount; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        sb.Append(dr[i].ToString().Replace(fieldDelimiter, "").Replace(rowDelimiter, ""));
                    }

                    if (i < iColCount - 1)
                    {
                        sb.Append(fieldDelimiter);
                    }
                }

                sb.Append(rowDelimiter);
            }

            string filename = "StockUp_Report_Carton_" + 1 + "_" + Common.timestamp() + ".csv";
            File.AppendAllText(context.Server.MapPath(filename),sb.ToString());
                
            
            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
            response.ClearContent();
            response.Clear();
            response.ContentType = "text/plain";
            response.AddHeader("Content-Disposition",
                               "attachment; filename=" + filename + ";");
            response.TransmitFile(context.Server.MapPath(filename));
            
            response.Flush();
            response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}