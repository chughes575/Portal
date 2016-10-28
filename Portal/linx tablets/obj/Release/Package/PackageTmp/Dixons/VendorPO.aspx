<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="True" CodeBehind="VendorPO.aspx.cs" Inherits="linx_tablets.Dixons.VendorPO" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    
    <asp:SqlDataSource ID="sqlDSorscleLastLeadTime" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_portalforecastreportmanagement 
where reportid=47
order by lastfiledate desc"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <p style="font-size:16pt;color:red">No# Of Weeks Forecast used:<asp:Label ID="lblWeeksUsed" runat="server"></asp:Label>
                        </p>
                    <h2>Vendor PO Recommendations</h2>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Recommendation type</th>
                            <th>Vendor</th>
                            <th>Download</th>
                        </tr>
                        <tr>
                            <td>All Ranged Products (Zero Suggested Qty Included)</td>

                            <td>
                                <asp:DropDownList ID="ddlAllSuppliers" runat="server">
                                    <asp:ListItem Text="All" Enabled="true" Value="All"></asp:ListItem>
                                </asp:DropDownList></td>
                            <td>
                                <asp:Button ID="btnDLAllReccommendations" runat="server" Text="Download" OnCommand="btnDLSuggestions_Command" CommandArgument="2" /></td>
                        </tr>
                        <tr>
                            <td>All Ranged Products (Suggested Qty's Only)</td>
                            <td>
                                <asp:DropDownList ID="ddlSuppliers" runat="server">
                                    <asp:ListItem Text="All" Enabled="true" Value="All"></asp:ListItem>
                                </asp:DropDownList></td>
                            <td>
                                <asp:Button ID="btnDLReccommendations" runat="server" Text="Download" OnCommand="btnDLSuggestions_Command" CommandArgument="1" /></td>
                        </tr>

                    </table>
                    
                        <h2>Vendor Lead Time Management</h2>

                        
                        <h3>Supplier Lead times</h3>
                        <div style="border: 5px solid gray; padding: 5px; background: white; width: 80%; height: 300px; overflow-y: scroll;">
                            <asp:GridView ID="gvPOSupplierLeadTimes" DataKeyNames="ID" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false" OnRowEditing="gvPOSupplierLeadTimes_RowEditing" OnRowUpdating="ggvPOSupplierLeadTimes_RowUpdating" OnRowCancelingEdit="gvPOSupplierLeadTimes_RowCancelingEdit" AutoGenerateEditButton="true">
                                <Columns>
                                    <asp:BoundField DataField="ID" ReadOnly="true" HeaderText="ID" />
                                    <asp:BoundField DataField="SupplierDesc" ReadOnly="true" HeaderText="Supplier Name" />
                                    <asp:TemplateField>
                                        <HeaderTemplate>Lead time (Days)</HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblLeadTime" runat="server" Text='<%# Eval("LeadTime") %>'></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:TextBox ID="txtLeadTime" runat="server" Text='<%# Eval("LeadTime") %>'></asp:TextBox><asp:RegularExpressionValidator ID="regExLeadTime" runat="server" ControlToValidate="txtLeadTime" Text="Please enter a numerical value" ValidationExpression="^(0|[1-9][0-9]*)$"></asp:RegularExpressionValidator>
                                        </EditItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    <asp:Panel ID="pnlLeadTimes" runat="server" Visible="True">
                        <h3>Product Lead Time Management</h3>
                        *This is a purge and replace upload, any existing entries not featured in the upload will be removed.
                    <h4>Last Product Lead Time Update</h4>
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
                        <h4>CSV Management</h4>

                        <asp:FileUpload ID="fuProductLeadTime" runat="server" /><br />

                        <asp:Button ID="btnProductLeadTimeUpload" runat="server" Text="Upload" OnClick="btnProductLeadTimeUpload_Click" />
                        &nbsp;
                     <asp:Button ID="btnProductLeadTimeDownload" runat="server" Text="Download existing lead times" OnClick="btnProductLeadTimeDownload_Click" />

                        <asp:Button ID="btnRemoveAllLeadTimes" runat="server" Text="Remove all lead times" OnClick="btnRemoveAllLeadTimes_Click" OnClientClick="confirmLeadTimePurge()" />
                    </asp:Panel>
                    <h2>Vendor Contacts Management</h2>
                    <br>
                    <ul>
                        <li>Use the download existing vendors button below to pull a list of all Oracle Vendors that appear in the current stock status file</li>
                        <li>You can then add any relevant contact information to an entry in this file.</li>
                        <li>Please ensdure the vendor name remains the same as in the download file.</li>
                    </ul>

                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing vendors</th>
                            <th>Dowload blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadVendorData" runat="server" Text="Download" OnClick="btnDownloadVendorData_Click" /></td>
                            <td>
                                <asp:Button ID="btnDownloadVndorTemplate" runat="server" Text="Download" OnClick="btnDownloadVndorTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fuVendorData" runat="server" /><br />
                    <asp:Button ID="btnUploadVendorData" runat="server" Text="Upload" OnClick="btnUploadVendorData_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
