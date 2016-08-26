<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="HiveReporting.aspx.cs" Inherits="linx_tablets.Hive.OrderSummary" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from vw_portal_hive_useruploadslastupdates order by lastfiledate desc"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Hive Reporting</h1>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Description</th>
                            <th>Download po suggestions</th>
                        </tr>
                        <tr>
                            <td>Download Full Component List</td>
                            <td>
                                <asp:Button ID="btnDownloadCompSuggestionsAll" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="sugall" /></td>
                        </tr>


                        <tr>
                            <td>Download Component Suggestions (Spares)</td>
                            <td>
                                <asp:Button ID="btnDownloadComSuggestionsSpares" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="sugs" /></td>
                        </tr>
                        <tr>
                            <td>Download Hive SOH Report</td>
                            <td>
                                <asp:Button ID="btnDownloadHiveSOHReport" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="hivesoh" /></td>
                        </tr>
                        <tr>
                                <td>Hive EOL Bundle Report</td>
                                <td>
                                    <asp:Button ID="btnDownloadHiveEOLBundleReport" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="eol1" /></td>
                            </tr>
                            <tr>
                                <td>Hive EOL Components Report</td>
                                <td>
                                    <asp:Button ID="btnDownloadHiveEOLComponentsReport" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="eol2" /></td>
                            </tr>
                         <tr>
                                <td>Exertis EOL Hive Report</td>
                                <td>
                                    <asp:Button ID="btnDownloadExertisHiveEOLReport" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="eol3" /></td>
                            </tr>
                        <tr>
                                <td>Goods Receipts Report (today)</td>
                                <td>
                                    <asp:Button ID="btnDownloadGoodsReceiptReport_today" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="gr1" /></td>
                            </tr>
                        <tr>
                                <td>Goods Receipts Report (historic)</td>
                                <td>
                                    <asp:Button ID="btnDownloadGoodsReceiptReport_historic" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="gr2" /></td>
                            </tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
