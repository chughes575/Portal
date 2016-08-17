<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="True" CodeBehind="AppleReporting.aspx.cs" Inherits="linx_tablets.CustomerService.AppleReporting" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <script type="@"></script>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <asp:SqlDataSource ID="sqlDSOrderStatus" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select OrderStatusDesc,OrderStatusID from MSE_Orderstatus "></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSInventoryReports" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select ir.ReportID,lm.PlantCode,lm.PlantDescription,ir.Filename,ir.DateCreated 
from MSE_AppleInventoryreports ir inner join MSE_AppleLocaleMapping lm on lm.LocaleID=ir.localeid
order by datecreated desc"></asp:SqlDataSource>
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
                    <asp:SqlDataSource ID="sqlDsVMIReportSource" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select distinct vmi.* from mse_applevmidatareports vmi inner join mse_applevmidatareportlines vmil on vmil.VMIReportID=vmi.VMIReportID
order by datecreated desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDsFCReportSource" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select distinct fc.* from MSE_AppleForecastCommitReports fc inner join MSE_AppleForecastCommitReportLines fcl on fcl.ReportID=fc.ReportID
order by reportdate desc"></asp:SqlDataSource>

                    <asp:SqlDataSource ID="sqlDSInventoryreportsLatest" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select * from  (select ROW_NUMBER() OVER(PARTITION BY localeid ORDER BY datecreated desc) AS Row,* 
from MSE_AppleInventoryreports
) as a inner join MSE_AppleLocaleMapping lm on lm.localeid=a.LocaleID
where a.Row=1 order by a.datecreated desc"></asp:SqlDataSource>

                    <asp:SqlDataSource ID="sqlDSVMILatest" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select top 1 * from mse_applevmidatareports order by datecreated desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSFCLatest" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select top 1 * from MSE_AppleForecastCommitReports fc order by fc.ReportDate desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSReplenEntries" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select replenid,DateGenerated,SentToOracleDate,SentToOracleFilename,case when SentToOracle =1 then 'Yes' else 'No' end as SentToOracle,createdby,LocaleReplen,lm.LocaleID,lm.plantcode,lm.ExertisOutAccount
from mse_applereplens ar inner join mse_applelocalemapping lm on lm.localeid=ar.LocaleID
order by DateGenerated desc"></asp:SqlDataSource>

                    <asp:SqlDataSource ID="sqlDSorscleLastRunImports" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement order by lastfiledate desc"></asp:SqlDataSource>


                    <asp:SqlDataSource ID="sqlDSorscleLastRunAppleRange" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid=4
order by lastfiledate desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSorscleLastRunASN" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid=1
order by lastfiledate desc"></asp:SqlDataSource>

                    <asp:SqlDataSource ID="sqlDSorscleLastRunOrderConfirmation" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid=5
order by lastfiledate desc"></asp:SqlDataSource>


                    <asp:SqlDataSource ID="sqlDSorscleLastRunPoStock" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid in (2,3)
order by lastfiledate desc"></asp:SqlDataSource>
                    

                      
                    <asp:Panel runat="server" ID="pnlAdmin" Visible="false">
                        <h3>Inventory files outstanding</h3>
                        <table class="CSSTableGenerator">
                            <tr>

                                <td>United Kingdom</td>
                                <td>Italy</td>
                                <td>United Arab Emirates</td>
                                <td>Czech Republic</td>
                                <td>Netherlands</td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblUK" runat="server"></asp:Label></td>
                                <td>
                                    <asp:Label ID="lblIT" runat="server"></asp:Label></td>
                                <td>
                                    <asp:Label ID="lblUAE" runat="server"></asp:Label></td>
                                <td>0</td>
                                <td>
                                    <asp:Label ID="lblNL" runat="server"></asp:Label></td>
                            </tr>
                        </table>

                        <h3>Oracle files outstanding</h3>
                        <table class="CSSTableGenerator">
                            <tr>
                                <th>Oracle Purchase orders</th>
                                <th>Product range files</th>
                                <th>Oracle Stock files</th>
                                <th>Locale back order files</th>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblPO" runat="server"></asp:Label></td>
                                <td>
                                    <asp:Label ID="lblPR" runat="server"></asp:Label></td>
                                <td>
                                    <asp:Label ID="lblOS" runat="server"></asp:Label></td>
                                <td>
                                    <asp:Label ID="lblBO" runat="server"></asp:Label></td>
                            </tr>
                        </table>
                        <br />
                        <asp:Button ID="btnProcessAllFiles" runat="server" Text="Process all files" OnClick="btnProcessAllFiles_Click" />
                        
                    </asp:Panel>
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
                    


                    <h3>Apple Hubs Inventory balance reports</h3>
                    <p style="font-style: italic;">Details of most recent hub inventory reports processed by Portal</p>
                    <asp:GridView ID="gvLatestInventory" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false" DataSourceID="sqlDSInventoryreportsLatest" OnRowCommand="btnDownloadInvBalance_Command" OnRowDataBound="gvLatestInventory_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="PlantCode" HeaderText="Plant Code" />
                            <asp:BoundField DataField="PLantDescription" HeaderText="Plant Description" />
                            <asp:BoundField DataField="Filename" HeaderText="Latest file (filename)" />
                            <asp:BoundField DataField="DateCreated" HeaderText="Latest file (date)" DataFormatString="{0:f}" />
                           <asp:TemplateField>
                               <HeaderTemplate>Download report</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button runat="server" ID="btnDownloadInvBalance" CommandName="downloadInvBalance" CommandArgument='<%# Eval("ReportID") %>' Text="Download"  />
                                </ItemTemplate>
                            </asp:TemplateField>
                             <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <h3>Latest Oracle Hub order confirmation</h3>
                    <asp:GridView ID="gvAppleRangeLastOrderConfirmation" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSorscleLastRunOrderConfirmation" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
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
                    <h3>Latest Oracle Hub ASNs</h3>
                    <asp:GridView ID="gvAppleRangeLastOrderASN" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSorscleLastRunASN" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
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
                    

                   
                    <h3>Latest Oracle Exports</h3>
                    <asp:GridView ID="gvAppleRangeLastPoStock" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSorscleLastRunPoStock" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
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

                   
                        <asp:Panel ID="pnlHide" runat="server" Visible="false">

                            <h3>Forecast commit reporting</h3>
                            <h4>Forecast commit reports</h4>
                            <asp:GridView ID="gvForecastCommitReports" runat="server" DataSourceID="sqlDSForecastCommitReports"
                                AutoGenerateColumns="true">
                            </asp:GridView>
                            <br />
                            <h4>Forecast commit report discrepancie reports</h4>
                            <asp:GridView ID="gvForecastCommitDiscrepancieReports" runat="server" DataSourceID="sqlDSForecastDiscrepancieReports"
                                AutoGenerateColumns="true">
                            </asp:GridView>

                            <h3>Apple depot inventory reports</h3>
                            <asp:GridView ID="gvAppleInventoryReports" runat="server" DataSourceID="sqlDSInventoryReports"
                                AutoGenerateColumns="true">
                            </asp:GridView>

                            <h3>Oracle asns</h3>
                            *Expected delivery dates are based on the carrier lead times to that depot location
    <asp:GridView ID="gvOracleAsns" runat="server" DataSourceID="sqlDSInventoryReports"
        AutoGenerateColumns="true">
    </asp:GridView>
                        </asp:Panel>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
