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
                    <h1>Hive Bundle Range Download</h1>
                    

                    <table class="CSSTableGenerator">
                        <tr>
                            <th class="auto-style1">Download existing bundle</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadBundleRange" runat="server" Text="Download" OnClick="btnDownloadBundleRange_Click" /></td>
                            
                        </tr>
                    </table>
                    <br />
                    <asp:Panel ID="pnlUpload" runat="server" Visible="false">
                    <asp:FileUpload ID="fuBundleProducts" runat="server" /><br />
                    <asp:Button ID="btnUploadBundleProducts" runat="server" Text="Upload" OnClick="btnUploadBundleProducts_Click" />
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </asp:Content>
<asp:Content ID="Content1" runat="server" contentplaceholderid="HeadContent">
    <style type="text/css">
        .auto-style1
        {
            height: 21px;
        }
    </style>
</asp:Content>
