<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="Forecast.aspx.cs" Inherits="linx_tablets.Reporting.Forecast" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSForecastCommitReports" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select fc.ReportID,fc.Filename,fc.ReportDate 
from MSE_AppleForecastCommitReports fc
order by ReportDate desc"></asp:SqlDataSource>
     <asp:SqlDataSource ID="sqlDSForecastDiscrepancieReports" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select fcr.DiscrepancieReportID,src.ReportID as SourceReportID, src.FileName as SourceFilename, src.ReportDate as SourceReportDate
,prv.ReportID as PreviousReportID, prv.FileName as PreviousFilename, prv.ReportDate as PreviousReportDate
 from mse_appleforecastcommitreportdiscrepencies fcr 
inner join MSE_AppleForecastCommitReports src on src.reportid=fcr.currentreportid
inner join MSE_AppleForecastCommitReports prv on prv.reportid=fcr.PreviousReportID"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlDsFCReportSource" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select distinct fc.* from MSE_AppleForecastCommitReports fc inner join MSE_AppleForecastCommitReportLines fcl on fcl.ReportID=fc.ReportID
order by reportdate desc"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlDSFCLatest" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select top 1 * from MSE_AppleForecastCommitReports fc order by fc.ReportDate desc"></asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlDSLastFcUpload" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid in (10)
order by lastfiledate desc"></asp:SqlDataSource>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h2>Forecast Commit</h2>
                    <h3>Last Apple Forecast Commit update</h3>
                    <asp:GridView CssClass="CSSTableGenerator" ID="gvFC" runat="server" AutoGenerateColumns="false" OnRowDataBound="gvFC_RowDataBound" DataSourceID="sqlDSFCLatest">
                        <Columns>
                            <asp:BoundField DataField="Filename" HeaderText="Latest file (filename)" />
                            <asp:BoundField DataField="ReportDate" HeaderText="Latest file (date)" DataFormatString="{0:f}" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <h3>Forecast commit report download</h3>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Report</th>
                            <td>
                                <asp:DropDownList ID="ddlForecastReports" DataTextField="Filename" DataValueField="reportid" runat="server" DataSourceID="sqlDsFCReportSource" AppendDataBoundItems="true">
                                    <asp:ListItem Text="select a report" Selected="True" Value="0">select a report</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td>
                                <asp:Button ID="Button1" runat="server" Text="Download report" OnClick="btnDownloadFC_Click" /></td>
                        </tr>
                    </table>
                    <h3>Forecast commit report upload</h3>
                  <h3>Latest Commit Report File Uploaded</h3>
                    <asp:GridView ID="gvFcUploaded" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSLastFcUpload" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="ReportName" HeaderText="Operation name" />
                                <asp:BoundField DataField="LastFileName" HeaderText="Filename" />
                                <asp:BoundField DataField="Username" HeaderText="Username" />
                                <asp:BoundField DataField="LastFileDate" DataFormatString="{0:f}" HeaderText="Upload Date" />
                                <asp:BoundField DataField="dateDiffImport" HeaderText="Hours since last file received" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                            </Columns>
                       
                        </asp:GridView>
                    
                    <b> 
                    <br />
                    Instructions:-</b><br />
Prior upload of completed Forecast commit report ready for submission to Apple the checks listed below must be completed.

                    <b>
                    <br />
                    <br />
                    Note:-</b><br />
                   <i> Failure to complete the checks will result Forecast Commit fail failure on Apple SFTP.</i>

                    <b>
                    <br />
                    <br />
                    Checks</b><br />
                    Make sure headers / columns <b>“ExertisReportID”</b>, <b>“Plant”</b>, <b>“Article”</b> are on the upload and they are not empty or duplicated.<br />
                    <br />
&nbsp;<b>Process</b>
                    <br />
                    Browse in and upload completed Apple Forecast commit file. Portal will update original Apple Forecast commit with uploaded information against Apple Forecast Commit headers.  If any of Apple Headers are missing from the upload, Portal will use the header / column detail from original Apple Forecast Commit file. After update is complete Portal will submit updated Apple Forecast Commit file to APPLE SFTP.<br />
&nbsp;<table>
                        
                        <tr>
                            <td><asp:FileUpload ID="fupFCUpload" runat="server" /></td>
                            <td>
                                &nbsp;</td>
                            <td>
                                <asp:Button ID="btnUploadFCProcessing" runat="server" Text="Upload FC Processing" OnClick="btnUploadFCProcessing_Click" /></td>
                        </tr>
                    </table>
                    </div>
                </div>

        </div>
        </div></asp:Content>