using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using System.Threading;
using System.Data.SqlClient;
using MSE_Common;
using System.Data;
namespace AppleStockRetreive
{

    public class stockval
    {
        public int ID { get; set; }
        public string item { get; set; }
        public int stock { get; set; }
        public float scp { get; set; }
        public int org { get; set; }
        public string subinventory { get; set; }
        public string stockType { get; set; }
        public string created { get; set; }
        public string last_updated { get; set; }
    }
    public class ListStockval
    {
        public List<stockval> Stockvals { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Common.log("Beginng product fetch");
            bool testing = false;
            string prodSQL = @"select * from vw_oracleproductfetchitems";
            string temploadTableName = "productdataloader_oracle_stock_lookup_tempload";
            SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
            conn.Open();
            DataTable table = SqlBulkLoader.getSchemaForTable(temploadTableName, conn);
            SqlBulkLoader bcp = new SqlBulkLoader(table, temploadTableName, conn, true);
            Common.runSQLNonQuery("delete from " + temploadTableName);
            int counter = 0;


            var client = new RestClient();
            client.BaseUrl = new Uri("https://dataapp.exertis.io");
            client.Authenticator = new HttpBasicAuthenticator("chris.hughes", "D0nk3yKONColecoVI$ION");
            DataRowCollection drcProducts = Common.runSQLRows(prodSQL);


            foreach (DataRow productCode in drcProducts)
            {
                string prodCode = productCode[0].ToString();




                var request = new RestRequest();
                //?report=stockbyproductcode.sql&macros[productcode]={0}
                request.Resource = "report/json/?report={report}&{searchvaltype}={prodcode}";
                request.AddParameter("report", "stockbyproductcode.sql", ParameterType.UrlSegment);
                request.AddParameter("searchvaltype", "macros[productcode]", ParameterType.UrlSegment);
                request.AddParameter("prodcode", prodCode, ParameterType.UrlSegment);
                List<stockval> stockValList = new List<stockval>();
                if (!testing)
                {
                    try
                    {

                        IRestResponse response = client.Execute(request);

                        RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();
                        stockValList = deserial.Deserialize<List<stockval>>(response);
                        
                    }
                    catch
                    { }
                }
                else
                {
                    stockval sv1 = new stockval();
                    stockval sv2 = new stockval();
                    stockval sv3 = new stockval();

                    sv1.ID = 35921;
                    sv2.ID = 1071275;
                    sv3.ID = 1071275;

                    sv1.item = "TPTL-WR841N";
                    sv1.stock = 772;
                    sv1.scp = 14.1F;
                    sv1.org = 88;
                    sv1.subinventory = "S101";
                    sv1.created = "2015-09-23 14:14:42";
                    sv1.last_updated = "2016-04-22 15:22:48";

                    sv2.item = "PARMKI9200@LW";
                    sv2.stock = 7;
                    sv2.scp = 116F;
                    sv2.org = 88;
                    sv2.subinventory = "S101";
                    sv2.created = "2015-05-28 23:49:51";
                    sv2.last_updated = "2016-04-21 12:49:36";

                    sv3.item = "PARMKI9200@LW";
                    sv3.stock = 7;
                    sv3.scp = 116F;
                    sv3.org = 88;
                    sv3.subinventory = "S122";
                    sv3.created = "2016-02-19 00:29:37";
                    sv3.last_updated = "2016-02-19 00:29:37";

                    stockValList.Add(sv1);
                    stockValList.Add(sv2);
                    stockValList.Add(sv3);
                }

                foreach (stockval sv in stockValList)
                {
                    string loadString = string.Format(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                           sv.ID, sv.item, sv.stock, sv.scp, sv.org, sv.subinventory, sv.stockType, sv.created, sv.last_updated);
                    try
                    {
                        DataRow row = SqlBulkLoader.parseValuesIntoDataRow(table.NewRow(), loadString, ',');
                        bcp.LoadItem(row);
                        counter++;
                    }
                    catch
                    {

                    }
                }

            }
            if (counter > 0)
            {
                bcp.Flush();
                conn.Close();
                Common.log("Completing product fetch");
                Common.runSQLNonQuery("EXEC sp_updateoraclestockfiguresfetch");
            }

        }


    }
}
