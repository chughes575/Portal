<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="VendorLeadTimeManagement.aspx.cs" Inherits="linx_tablets.Reporting.SupplierManagement" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSorscleLastRunApplePOInvoice" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid=7
order by lastfiledate desc"></asp:SqlDataSource>


    <asp:SqlDataSource ID="sqlDSPOSuppliers" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select * from mse_appleposuppliers order by SupplierDesc"></asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlDSorscleLastLeadTime" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid=9
order by lastfiledate desc"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h2>Vendor Lead Time Management</h2>
                    
                    <h3>Red flag report (PO's where the PO has passed its' lead time)</h3>
                    <asp:Button ID="btnPoOverDueRed" runat="server" Text="Download" OnClick="btnPoOverDueRed_Click" />
                    
                    <h3>Amber flag report (PO's where the PO will pass its' lead time next week)</h3>
                    <asp:Button ID="btnPoOverDueAmber" runat="server" Text="Download" OnClick="btnPoOverDueAmber_Click" />
                    
                    
                    <h2>PO Supplier Management</h2>
                    <h3>Lead times</h3>
                    <asp:GridView ID="gvPOSupplierLeadTimes" DataKeyNames="supplierid" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false" OnRowEditing="gvPOSupplierLeadTimes_RowEditing" OnRowUpdating="ggvPOSupplierLeadTimes_RowUpdating" OnRowCancelingEdit="gvPOSupplierLeadTimes_RowCancelingEdit" AutoGenerateEditButton="true">
                        <Columns>
                            <asp:BoundField DataField="SupplierID" ReadOnly="true" HeaderText="SupplierID" />
                            <asp:BoundField DataField="SupplierDesc" ReadOnly="true" HeaderText="Supplier Name" />
                            <asp:TemplateField>
                                <HeaderTemplate>Lead time (Weeks)</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblLeadTime" runat="server" Text='<%# Eval("LeadTime") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtLeadTime" runat="server" Text='<%# Eval("LeadTime") %>'></asp:TextBox><asp:RegularExpressionValidator ID="regExLeadTime" runat="server" ControlToValidate="txtLeadTime" Text="Please enter a numerical value" ValidationExpression="^(0|[1-9][0-9]*)$"></asp:RegularExpressionValidator>
                                </EditItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <h2>Product Lead Time Management</h2>
                    *This is a purge and replace upload, any existing entries not featured in the upload will be removed.
                    <h3>Last Product Lead Time Update</h3>
                    <asp:GridView ID="gvAppleProductLeadTimesLastUpdate" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSorscleLastLeadTime" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
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
                    <h3>CSV Management</h3>

                    <asp:FileUpload ID="fuProductLeadTime" runat="server" /><br />

                    <asp:Button ID="btnProductLeadTimeUpload" runat="server" Text="Upload" OnClick="btnProductLeadTimeUpload_Click" />
                    &nbsp;
                     <asp:Button ID="btnProductLeadTimeDownload" runat="server" Text="Download existing lead times" OnClick="btnProductLeadTimeDownload_Click" />
                    
                    <asp:Button ID="btnRemoveAllLeadTimes" runat="server" Text="Remove all lead times" OnClick="btnRemoveAllLeadTimes_Click" OnClientClick = "confirmLeadTimePurge()" />
                    <%--<h4>Lead time CSV management</h4>
                        *Please ensure the file is saved as a csv before uploading
                    <br />
                        <asp:FileUpload ID="POInvoice" runat="server" /><br />
                        <asp:Button ID="btnUploadPOInvoice" runat="server" Text="Upload" OnClick="btnUploadPOInvoice_Click" />
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
                            <asp:BoundField ReadOnly="true" DataField="MissingCount" HeaderText="Prices missing" />
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
                       </div>--%>
                </div>

            </div>

        </div>

    </div>

</asp:Content>

