<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="ApplePOInvoiceManagement.aspx.cs" Inherits="linx_tablets.Reporting.VendorPOInvoiceManagement" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSorscleLastRunApplePOInvoice" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid=7
order by lastfiledate desc"></asp:SqlDataSource>


    <asp:SqlDataSource ID="sqlDSPOInvoiceFilenames" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select filename from (
select distinct Filename,importdate from mse_applepos ) as a
order by a.importdate desc"></asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlDSPOInvoiceFiles" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select po.POID,Purchasing_Doc, po.Filename as SourceFileName,po.ImportDate,po.processed, MissingPricing,po.OracleFilename,po.oracleTimeStamp,lm.PlantCode,lm.PlantDescription,
lm.ExertisEposAccount,lm.EdiLocationCode,coalesce(mis.MissingCount,0) as MissingCount
 from mse_applepos po inner join mse_applelocalemapping lm on lm.localeid=po.localeid
 left outer join (select pol.PoID,count(pol.PoLineID) as MissingCount 
 from mse_applepolines pol
 inner join mse_applepos po on po.poid=pol.poid left outer join MSE_AppleProductMapping apm on apm.AppleCode=pol.Material
		where case when po.localeid=5 then coalesce(apm.lu50price,apm.unitprice)  else apm.unitprice end is null
		group by pol.PoID) as mis on mis.poid=po.PoID
        order by po.importdate desc"></asp:SqlDataSource>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h2>Apple PO Invoice Management</h2>
                    <h3>Last Apple Purchase Order Invoicing Uploaded</h3>
                    <asp:GridView ID="gvPOInvoice" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false" OnRowDataBound="gvPOInvoice_RowDataBound" DataSourceID="sqlDSorscleLastRunApplePOInvoice">
                        <Columns>
                            <asp:BoundField DataField="LastFilename" HeaderText="Latest file (filename)" />
                            <asp:BoundField DataField="lastfiledate" HeaderText="Latest file (date)" DataFormatString="{0:f}" />
                            <asp:BoundField DataField="Username" HeaderText="Uploaded By" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <h3>Apple Purchase Order Invoicing Upload</h3>
                        *Please ensure the report is saved as a csv before uploading
                    <br />
                        <asp:FileUpload ID="POInvoice" runat="server" /><br />
                        <asp:Button ID="btnUploadPOInvoice" runat="server" Text="Upload" OnClick="btnUploadPOInvoice_Click" />&nbsp;<asp:Button ID="btnDownloadPOInvoiceTemplate" runat="server" Text="Download template" OnClick="btnDownloadPOInvoiceTemplate_Click" />
                    <h3>Apple Purchase Order Invoice Report Download</h3>
                    *Grouped by the original file they were uploaded against
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Report</th>
                            <td>
                                <asp:DropDownList ID="ddlApplePOInvoice" DataTextField="Filename" DataValueField="Filename" runat="server" DataSourceID="sqlDSPOInvoiceFilenames" AppendDataBoundItems="true">
                                    <asp:ListItem Text="select a report" Selected="True" Value="0">select a report</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td>
                                <asp:Button ID="btnDownloadPOInvoice" runat="server" Text="Download report" OnClick="btnDownloadPOInvoice_ClickNew" /></td>
                        </tr>
                    </table>


                    <h3>Existing Invoices</h3>
                    *Invoices are grouped by Purchasing Doc number when uploaded <br><br />

                    *Invoices uploaded where there is a product without a price on the apple range will be held until a price has been added for the affected lines. Once all lines on a held invoice have a price the invoice will be submitted.

                   <div style="
    border: 5px solid gray;
    padding: 5px;
    background: white;
    width: 110%;
    height: 300px;
    overflow-y: scroll;">
                     <asp:GridView ID="gvInvoiceFiles" Style="height: 200px; overflow: auto" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSPOInvoiceFiles" DataKeyNames="POID" AutoGenerateColumns="false" OnRowCommand="gvInvoiceFiles_RowCommand" OnRowDataBound="gvInvoiceFiles_RowDataBound">
                        <Columns>
                            <asp:BoundField ReadOnly="true" DataField="POID" HeaderText="POID (internal reference)" />
                            <asp:BoundField ReadOnly="true" DataField="PlantCode" HeaderText="Hub" />
                            <asp:BoundField ReadOnly="true" DataField="ExertisEposAccount" HeaderText="AccountCode" />
                            <asp:BoundField ReadOnly="true" DataField="EdiLocationCode" HeaderText="Edi Location Code" />
                            <asp:BoundField ReadOnly="true" DataField="Purchasing_Doc" HeaderText="PO Number" />
                            <asp:BoundField ReadOnly="true" DataField="ImportDate" HeaderText="Upload Date"  DataFormatString="{0:f}" />
                            <asp:BoundField ReadOnly="true" DataField="MissingPricing" HeaderText="Pricing Missing" />
                            <asp:BoundField ReadOnly="true" DataField="MissingCount" HeaderText=" No# Of Prices missing" />
                            <asp:BoundField ReadOnly="true" DataField="Processed" HeaderText="Exported To Oracle" />
                            <asp:BoundField ReadOnly="true" DataField="OracleFilename" HeaderText="Oracle Export Filename" />
                            <asp:BoundField ReadOnly="true" DataField="oracleTimeStamp" DataFormatString="{0:f}" HeaderText="Oracle Export Date" />
                            
                           <asp:TemplateField>
                                <HeaderTemplate>Resubmit to Oracle</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnResubmitToOracle" runat="server" Text="Resubmit to Oracle" CommandName="resubmitInvoiceToOracle" CommandArgument='<%# Eval("POID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Download Replen Lines</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnDownloadReplenLines" runat="server" Text="Download replen lines" CommandName="downloadInvoiceLines" CommandArgument='<%# Eval("POID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Download Missing Price Lines</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnDownloadMissingLines" runat="server" Text="Download replen lines" CommandName="downloadInvoiceLinesMissing" CommandArgument='<%# Eval("POID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                    </asp:GridView>
                       </div>
                </div>

            </div>

        </div>

    </div>

</asp:Content>

