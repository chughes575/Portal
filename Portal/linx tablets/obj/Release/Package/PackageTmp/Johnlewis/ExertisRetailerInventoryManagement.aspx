<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="ExertisRetailerInventoryManagement.aspx.cs" Inherits="linx_tablets.Johnlewis.ConsignmetnUploads" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from vw_portal_jlp_consignmentstockuploadslastupdates order by lastfiledate desc"></asp:SqlDataSource>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Exertis Retailer Inventory Management</h1>
                    This page is used to view the stock position against a product at branch (WHS/ONLINE/Branch) level. This information is pulled from weekly sales/stock exports.
                    <h2>Last Import/Upload updates</h2>
                    <asp:GridView ID="gvKewillProductStockStatusLastUpdate" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSKewillProductStockStatusReport" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedForecastPortal_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
                            <asp:BoundField DataField="Username" HeaderText="Uploaded/Imported by" />
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

                    <h2>Inventory Download</h2>
                    <h5>Table legend (Branch Type)</h5>
                    <table class="CSSTableGenerator">
                        <tr>
                            <td>Online</td>
                            <td style="padding-left:7px"> Online Stock level</td>
                        </tr>
                        <tr>
                            <td>Bra</td>
                            <td style="padding-left:7px"> Stock Level at Branches</td>
                        </tr>
                        <tr>
                            <td>WHS</td>
                            <td style="padding-left:7px"> Stock Level at WHS</td>
                        </tr>


                            
                        </tr>
                    </table>
                    <br />
                   <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing Stock (All retailers)</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadExistingConsignmentStock" runat="server" Text="Download" OnClick="btnDownloadExistingConsignmentStock_Click" /></td>
                            
                        </tr>
                    </table>
                    <asp:Panel Visible="false" runat="server">
                    <br />
                     Format required for the upload is RetailerID*/CustomerSku*/Exertis Code/MFR Part No/Product Description/#Branch/StockQty*
                    <br />
                    <br />
                    *Starred values are required <br />
                    #Branch is optional and can be left blank if the value uploaded is not for a specific branch/store<br />
                    Any existing stock entries for a sku will be overwritten when a file is uploaded
                    <br />
                 
                    <asp:FileUpload ID="fuConsignmentStock" runat="server" /><br />
                    <asp:Button ID="btnUploadConsignmentStock" runat="server" Text="Upload" OnClick="btnUploadConsignmentStock_Click" />   
                </asp:Panel>
                     </div>
            </div>
        </div>
    </div>
</asp:Content>
