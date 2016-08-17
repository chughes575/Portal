<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="SDGDashboard.aspx.cs" Inherits="linx_tablets.SDG.SDGForecastManagement" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_portalforecastreportmanagement 
where customerid=1  and UserPopulated=0
order by lastfiledate desc"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlDSPortalAccountManagers" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="	select * from MSE_PortalAccountManagers order by business_area"></asp:SqlDataSource>




    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h2>Last Import/Upload updates (System populated/Generated)</h2>
                    <asp:GridView ID="gvKewillProductStockStatusLastUpdate" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSKewillProductStockStatusReport" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedForecastPortal_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
                            <asp:BoundField DataField="LastFileName" HeaderText="Filename" />
                            <asp:BoundField DataField="LastFileDate" DataFormatString="{0:f}" HeaderText="Import Date" />
                            <asp:BoundField DataField="dateDiffImport" HeaderText="Hours since last file received" />

                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>

                    </asp:GridView>                                    
                    <h2>Kewill Product Stock Status Reporting</h2>
                    <h3>Kewill Product Stock Status Download</h3>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download Current Product Stock Status File</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadKellPSSR" runat="server" Text="Download" OnClick="btnDownloadKellPSSR_onClick" /></td>
                        </tr>
                    </table>
                </div>
            </div>

        </div>
    </div>
</asp:Content>
