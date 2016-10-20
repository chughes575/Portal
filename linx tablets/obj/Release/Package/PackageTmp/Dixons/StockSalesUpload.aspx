<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="StockSalesUpload.aspx.cs" Inherits="linx_tablets.Dixons.StockSalesUpload" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqldsuploads" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from MSE_PortalSalesEposFiles where customerid=2 and importdate>='18/10/2016'"></asp:SqlDataSource>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Dixons Stock/Sales Management</h1>
                    
          
                    <h3>Existing Uploads</h3>
                    *latest upload will be used
                    <div style="
    border: 5px solid gray;
    padding: 5px;
    background: white;
    width: 70%;
    height: 200px;
    overflow-y: scroll;">
                    <asp:GridView ID="gvStockSalesUploads" Style=" overflow: auto" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqldsuploads" DataKeyNames="ID" AutoGenerateColumns="false" OnRowCommand="gvStockSalesUploads_RowCommand">
                        <Columns>
                            <asp:BoundField ReadOnly="true" DataField="ID" HeaderText="ID" />
                            <asp:BoundField ReadOnly="true" DataField="ImportDate" HeaderText="Date Uploaded" DataFormatString="{0:f}" />
                            <asp:BoundField ReadOnly="true" DataField="Filename" HeaderText="Filename" />
                            <asp:TemplateField>
                                <HeaderTemplate>Download File</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnDownloadFile" runat="server" Text="Download File" CommandName="downloadfile" CommandArgument='<%# Eval("ID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                    </asp:GridView>
                        </div>



                    <asp:FileUpload ID="fupStockSales" runat="server" /><br />
                    <asp:Button ID="btnUploadStockSales" runat="server" Text="Upload" OnClick="btnUploadStockSales_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
