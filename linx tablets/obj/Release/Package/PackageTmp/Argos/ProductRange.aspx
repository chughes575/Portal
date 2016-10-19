<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="ProductRange.aspx.cs" Inherits="linx_tablets.Argos.ProductRange" %>

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
                    <h1>Argos Product Range Management</h1>
                    
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
                    <asp:FileUpload ID="fupArgosProduct" runat="server" /><br />
                    <asp:Button ID="btnUploadArgosProduct" runat="server" Text="Upload" OnClick="btnUploadArgosProduct_Click" />

                </div>
            </div>
        </div>
    </div>
</asp:Content>
