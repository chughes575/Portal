<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="InternalReporting.aspx.cs" Inherits="linx_tablets.Reporting.InternalReporting" %>



<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
 
    <asp:SqlDataSource ID="sqlDSInventoryreportsLatest" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select * from  (select ROW_NUMBER() OVER(PARTITION BY localeid ORDER BY datecreated desc) AS Row,* 
from MSE_AppleInventoryreports
) as a inner join MSE_AppleLocaleMapping lm on lm.localeid=a.LocaleID
where a.Row=1 order by a.datecreated desc"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                     <h2>Apple Hubs Inventory balance reports</h2>
                    <p style="font-style: italic;">Details of most recent hub inventory reports processed by Portal</p>
                    <asp:GridView ID="gvLatestInventory" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false" DataSourceID="sqlDSInventoryreportsLatest" OnRowCommand="btnDownloadInvBalance_Command" OnRowDataBound="gvLatestInventory_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="PlantCode" HeaderText="Plant Code" />
                            <asp:BoundField DataField="PLantDescription" HeaderText="Plant Description" />
                            <asp:BoundField DataField="Filename" HeaderText="Latest file (filename)" />
                            <asp:BoundField DataField="DateCreated" HeaderText="Latest file (date)" DataFormatString="{0:f}" />
                           <asp:TemplateField>
                               <HeaderTemplate>Download report</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button runat="server" ID="btnDownloadInvBalance" CommandName="downloadInvBalance" CommandArgument='<%# Eval("ReportID") %>' Text="Download"  />
                                </ItemTemplate>
                            </asp:TemplateField>
                             <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <h2>Availibility Tracker</h2>
                    *Availibility data is availible from 01/10/2015 onwards

                    <asp:Button ID="btnAvailTrackDownload" runat="server" Text="Download Availibility Report" OnClick="btnAvailTrackDownload_Click" />

                    <h2>Forecast commit grouped product report</h2>
                    <asp:Button ID="btn_fcgroupedproduct" runat="server" Text="Download FC Grouped Product Report" OnClick="btn_fcgroupedproduct_Click" />


                     <h2>Overstock Report</h2>
                    

                    <asp:Button ID="btn_DonloadOverStockReport" runat="server" Text="Download Overstock Report" OnClick="btn_DonloadOverStockReport_Click" />

                    <h2>Overdue PO reporting</h2>
                    
                    <h3>Red flag report (PO's where the PO has passed its' lead time)</h3>
                    <asp:Button ID="btnPoOverDueRed" runat="server" Text="Download" OnClick="btnPoOverDueRed_Click" />
                    
                    <h3>Amber flag report (PO's where the PO will pass its' lead time next week)</h3>
                    <asp:Button ID="btnPoOverDueAmber" runat="server" Text="Download" OnClick="btnPoOverDueAmber_Click" />
                    </div>
                </div>
            </div>
        </div>
    </asp:Content>