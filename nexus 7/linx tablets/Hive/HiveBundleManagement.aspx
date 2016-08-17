<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="HiveBundleManagement.aspx.cs" Inherits="linx_tablets.Hive.BundleManagement" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSLastBundleFileUpload" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="SELECT        TOP (100) PERCENT CASE WHEN reportid IN (7, 8, 11) THEN datediff(HOUR, DATEADD(month, DATEDIFF(month, 0, getdate()), 0), lastfiledate) ELSE datediff(hour, lastfiledate, getdate()) END AS dateDiffImport, 
                         ReportID, CustomerID, ReportName, LastFilename, lastfiledate, WarningDiff, Username, UserPopulated
FROM            dbo.mse_portalforecastreportmanagement
WHERE        (ReportID IN (22))
ORDER BY lastfiledate DESC"></asp:SqlDataSource>
    

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Hive Bundle Range Management</h1>
                    This is used to manage bundles in the system, A bundle is made up of a master product which in turn has component products required to make up this bundle
                    <h2>Last Import/Upload updates</h2>
                    <asp:GridView ID="gvBundleProductLastUpdate" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSLastBundleFileUpload" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedForecastPortal_RowDataBound">
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
<h2>Hive Bundle Management</h2>
                    <br>
                    (Purge and replace upload)
                    <ul>
                        <li>Four columns required for this upload </li>
                        <li>Exertis3PLProductCode - This is the Primary Exertis product code</li>
                        <li>HiveSku - This is the HIve Sku that will be associated with this bundle</li>
                        <li>ComponentPartCode - This is the child component Exertis product code (Oracle Product ID)</li>
                        <li>ComponentQty - The qty of the child component part to make up the parent product/bundle</li>
                    </ul>

                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing bundle</th>
                            <th>Download blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadBundleRange" runat="server" Text="Download" OnClick="btnDownloadBundleRange_Click" /></td>
                            <td>
                                <asp:Button ID="btnDownloadBundleRangeTemplate" runat="server" Text="Download" OnClick="btnDownloadBundleRangeTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fuBundleProducts" runat="server" /><br />
                    <asp:Button ID="btnUploadBundleProducts" runat="server" Text="Upload" OnClick="btnUploadBundleProducts_Click" />
                    </div>
                </div>
            </div>
        </div>
    </asp:Content>