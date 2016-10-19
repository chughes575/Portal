<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="ExertisRetailerEposManagement.aspx.cs" Inherits="linx_tablets.Johnlewis.EposSales" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <script type="text/javascript" src="/Scripts/jquery.tablesorter.js"></script> 
    <script src="/Scripts/jquery.tablescroll.js"></script>
     <script type="text/javascript">



         jQuery(document).ready(function ($) {
             $("#MainContent_gvKewillProductStockStatusLastUpdate").tablesorter();



         });
        </script>
    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from vw_portal_jlp_consignmentuploadslastupdates"></asp:SqlDataSource>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Exertis Retailer Epos Management</h1>

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

                   <h3>Epos Upload</h3>
                    
                    
                 <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing epos data (All retailers)</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadExistingEposdata" runat="server" Text="Download" OnClick="btnDownloadExistingEposdata_Click" /></td>
                           
                        </tr>
                    </table>
                    <br />
                    <asp:Panel runat="server" Visible="false">
                    Format required for the upload is RetailerID*/CustomerSku*/Exertis Code/MFR Part No/Product Description/#Branch/StockQty*/EposDate*
                    <br />
                    <br />
                    *Starred values are required and the value in the RetailerID should be left as is not altered or the upload will fail. (RetailerID: 13)<br />
                    #Branch is optional and can be left blank if the value uploaded is not for a specific branch/store<br />
                    Epos data is stored on a weekly level.
                    <br />
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
