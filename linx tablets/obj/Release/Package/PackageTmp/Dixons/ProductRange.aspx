<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="ProductRange.aspx.cs" Inherits="linx_tablets.Dixons.ProductRange" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from vw_portal_hive_consignmentuploadslastupdates order by lastfiledate desc"></asp:SqlDataSource>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Dixons Product Range Management</h1>
                    
                    The product range is automatically populated based on the customer sku references that exist in Oracle.
                    For sku's where the cross reference has not yet been confirmed/setup tbc can be used in column
                


                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing range</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadProductRange" runat="server" Text="Download" OnClick="btnDownloadProductRange_Click" /></td>
                                                       
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fupDixonsProduct" runat="server" /><br />
                    <asp:Button ID="btnUploadDixonsProduct" runat="server" Text="Upload" OnClick="btnUploadDixonsProduct_Click" />
                    <h2>Business Area Management</h2>
                    This is used to manage which Business Areas match to the following Stock Areas (The number represents the ID to use in the upload)-
                    <ol>
                        <li>1. DC Stock</li>
                        <li>2. Store stock</li>
                        <li>3. Travel Stock</li>

                    </ol>

                    <br />
                    The upload is purge and replace and requires the following columns <br />
                    Business Area/Stock Area ID
                    <br />
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing mapping</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadBusinessAreaMapping" runat="server" Text="Download" OnClick="btnDownloadBusinessAreaMappinge_Click" /></td>
                                                       
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fupBusinessArea" runat="server" /><br />
                    <asp:Button ID="btnUploadBusinessAreaMapping" runat="server" Text="Upload" OnClick="btnUploadBusinessAreaMapping_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
