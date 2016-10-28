<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="ProductRange.aspx.cs" Inherits="linx_tablets.Johnlewis.ProductRange" %>

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
                    <h1>Jlp Product Range Management</h1>
                    
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
                    <asp:FileUpload ID="fupJlpProduct" runat="server" /><br />
                    <asp:Button ID="btnUploadJlpProduct" runat="server" Text="Upload" OnClick="btnUploadJlpProduct_Click" />
                    <h2>Branch Management</h2>
                    This is used to manage which JLP branch maps to the following Retailer types (The number represents the Retailer Type ID to use in the upload)-
                    <ol>
                        <li>1. Branch</li>
                        <li>2. Wharehouse</li>
                        <li>3. Online</li>

                    </ol>

                    <br />
                    The upload is purge and replace and requires the following columns <br />
                    Branch Number/Retailer Type ID
                    <br />
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing branch mapping</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadBranchMapping" runat="server" Text="Download" OnClick="btnDownloadBranchMapping_Click" /></td>
                                                       
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fupBranchMapping" runat="server" /><br />
                    <asp:Button ID="btnUploadBranchMapping" runat="server" Text="Upload" OnClick="btnUploadBranchMapping_Click" />
               
                     <h2>Vendor Sku Mapping</h2>
                    This section is used to map a Vendor/Manufacturer to a jlp sku that we do not have in the range.<br />
                    Download existing will pull existing mappings as well as customer sku's which feature in the weekly sales/stock email which are unmapped<br />

                    Upload is PURGE AND REPLACE! Use with caution
                <br />
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing vendorsku mapping</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadVendorSkuMapping" runat="server" Text="Download" OnClick="btnDownloadVendorSkuMapping_Click" /></td>
                                                       
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fuVendorSku" runat="server" /><br />
                    <asp:Button ID="btnUploadVendorSkuMapping" runat="server" Text="Upload" OnClick="btnUploadVendorSkuMapping_Click" />
                    
                </div>
            </div>
        </div>
    </div>
</asp:Content>
