<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="VMI.aspx.cs" Inherits="linx_tablets.Reporting.VMITest" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSVMILatest" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select top 1 * from mse_applevmidatareports_test order by datecreated desc"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlDsVMIReportSource" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select distinct vmi.* from mse_applevmidatareports_test vmi inner join mse_applevmidatareportlines_test vmil on vmil.VMIReportID=vmi.VMIReportID
order by datecreated desc"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h2>Apple VMI</h2>
                    <h3>Last Apple VMI update</h3>
                    <asp:GridView ID="gvVMI" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false" OnRowDataBound="gvVMI_RowDataBound" DataSourceID="sqlDSVMILatest">
                        <Columns>
                            <asp:BoundField DataField="Filename" HeaderText="Latest file (filename)" />
                            <asp:BoundField DataField="DateCreated" HeaderText="Latest file (date)" DataFormatString="{0:f}" />
                            <asp:BoundField DataField="UploadedBy" HeaderText="Uploaded By" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <h3>Apple VMI report upload</h3>
                        *Please ensure the excel VMI report is saved as a csv before uploading
                    <br />
                        <asp:FileUpload ID="fupProductVMI" runat="server" /><br />
                        <asp:Button ID="btnUploadVMI" runat="server" Text="Upload" OnClick="btnUploadVMI_Click" />
                    <h3>Apple VMI report Download</h3>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Report</th>
                            <td>
                                <asp:DropDownList ID="ddlVMIReports" DataTextField="Filename" DataValueField="vmireportid" runat="server" DataSourceID="sqlDsVMIReportSource" AppendDataBoundItems="true">
                                    <asp:ListItem Text="select a report" Selected="True" Value="0">select a report</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td>
                                <asp:Button ID="btnDownloadVMI" runat="server" Text="Download report" OnClick="btnDownloadVMI_ClickNew" /></td>
                        </tr>
                    </table>
                </div>

            </div>

        </div>

    </div>

</asp:Content>
