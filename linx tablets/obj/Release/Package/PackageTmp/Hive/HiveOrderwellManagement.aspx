<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="HiveOrderwellManagement.aspx.cs" Inherits="linx_tablets.Hive.HiveOrderwellManagement" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

   
    <asp:SqlDataSource ID="sqlDsWeeklyDispatches" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select a.OrderDate,count(Order_number) as Orders,shipunits.TotalShipped as Units from(
select distinct dateadd(day, (datepart(weekday,cast(despatch_date as date))*-1)+1,cast(despatch_date as date)) OrderDate,order_number
 from MSE_OracleddispatchadvicePortal where customerid=5
 )  a
 left outer join (select  dateadd(day, (datepart(weekday,cast(despatch_date as date))*-1)+1,cast(despatch_date as date)) OrderDate,sum(cast(Ship_Quantity as int)) as TotalShipped
 from MSE_OracleddispatchadvicePortal where customerid=5
 group by dateadd(day, (datepart(weekday,cast(despatch_date as date))*-1)+1,cast(despatch_date as date))) as shipunits on shipunits.OrderDate=a.OrderDate
 group by a.OrderDate,shipunits.TotalShipped
 order by a.OrderDate desc
"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlDsWeeklyVendorSalesOut" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from vw_vednorsalesoutweeks order by orderdate desc
"></asp:SqlDataSource>
    

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Hive Orderwell Management</h1>
                    <h2>Orders Received Today</h2>
                    <asp:GridView ID="gvOrdersSummary" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="EmailID"      HeaderText ="Email ID" />
                            <asp:BoundField DataField="Filename"     HeaderText ="Report Filename" />
                            <asp:BoundField DataField="DateImported" HeaderText ="Date Imported" DataFormatString="{0:f}" />
                            <asp:BoundField DataField="OrderCount"   HeaderText ="Order Count" />
                            <asp:BoundField DataField="Unit Count"   HeaderText ="Unit Count" />
                            <asp:BoundField DataField="Orders Ack"   HeaderText ="Units Ack" />
                        </Columns>

                    </asp:GridView>
                    <br />
                    <asp:Button runat="server" ID="btnDownloadRollingDocument" OnClick="btnDownloadRollingDocument_Click" Text="Download Rolling Order Report" />
                    &nbsp;<asp:Button runat="server" ID="Button1" OnClick="btnDownloadRollingDocument_outstanding_Click" Text="Download Rolling Order Report (Outstanding)" />
                  
                    <h2>Hive Weekly Dispatches</h2>
                    <div style="width: 100%; height: 100px; overflow: scroll">
                    <asp:GridView ID="gvWeeklyDispatches" DataSourceID="sqlDsWeeklyDispatches" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false">
                      <Columns>
                          <asp:BoundField DataField="Orderdate" DataFormatString="{0:f}"     HeaderText ="Week" />
                            <asp:BoundField DataField="Orders"     HeaderText ="Orders" />
                            <asp:BoundField DataField="Units" HeaderText ="Units"  />
                      </Columns>
                          </asp:GridView>
                        </div>
                    <br />
                    <asp:Button ID="btnDownloadDispatches" runat="server" OnClick="btnDownloadDispatches_Click" Text="Download Dispatches" />
                    <h2>Hive Weekly Vendor Sales Out</h2>
                    <div style="width: 100%; height: 200px; overflow: scroll">
                    <asp:GridView ID="GridView1" DataSourceID="sqlDsWeeklyVendorSalesOut" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false">
                      <Columns>
                          <asp:BoundField DataField="Orderdate" DataFormatString="{0:f}"     HeaderText ="Week" />
                          <asp:BoundField DataField="Customer Account Code"     HeaderText ="Customer Account Code" /> 
                          <asp:BoundField DataField="Customer Name"     HeaderText ="Customer Name" /> 
                          
                           <asp:BoundField DataField="Orders"     HeaderText ="Orders" />
                           <asp:BoundField DataField="Units"     HeaderText ="Units" />
                            
                      </Columns>
                          </asp:GridView>
                        </div>
                    <br />
                    <asp:Button ID="btnDownloadVendorSalesOut" runat="server" OnClick="btnDownloadVendorSalesOut_Click" Text="Download Vendor Sales Out" />
                                        </div>
            </div>
        </div>
    </div>
</asp:Content>
