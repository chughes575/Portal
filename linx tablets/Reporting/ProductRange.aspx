<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="ProductRange.aspx.cs" Inherits="linx_tablets.Reporting.ProductRange" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
   <asp:SqlDataSource ID="sqlDSorscleLastRunAppleRange" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid=4
order by lastfiledate desc"></asp:SqlDataSource>


    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                   <h2>Apple Range</h2>
                    <h3>Last Apple Range update </h3>
                    <asp:GridView ID="gvAppleRangeLastUpdate" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSorscleLastRunAppleRange" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
                            <asp:BoundField DataField="LastFileName" HeaderText="Filename" />
                            <asp:BoundField DataField="LastFileDate" DataFormatString="{0:f}" HeaderText="Import Date" />
                            <asp:BoundField DataField="dateDiffImport" HeaderText="Hours since last file received" />
                            <asp:BoundField DataField="Username" HeaderText="Uploaded by" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>

                    </asp:GridView>
                    <h3>Apple product range upload</h3>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing range</th>
                            <th>Dowload blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadRange" runat="server" Text="Download" OnClick="btnDownloadRange_Click" /></td>
                            <td>
                                <asp:Button ID="btnDownloadRangeTemplate" runat="server" Text="Download" OnClick="btnDownloadRangeTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fupProductRange" runat="server" /><br />
                    <asp:Button ID="btnUploadProductRange" runat="server" Text="Upload" OnClick="btnUploadProductRange_Click" />
                    </div>
                </div>

        </div>
        </div></asp:Content>