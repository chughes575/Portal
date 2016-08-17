<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="VendorPO.aspx.cs" Inherits="linx_tablets.SDG.VendorPO" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSorscleLastRunPoStock" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid in (2,3)
order by lastfiledate desc"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlDSSuppliers" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="SELECT distinct pv.VendorName from MSE_PortalVendors pv order by  pv.VendorName"></asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlDSorscleLastRunApplePOInvoice" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_portalforecastreportmanagement 
where reportid=12
order by lastfiledate desc"></asp:SqlDataSource>


    <asp:SqlDataSource ID="sqlDSPOSuppliers" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from mse_PortaVendorLeadTimes where customerid=1 order by supplierdesc"></asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlDSorscleLastLeadTime" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_portalforecastreportmanagement 
where reportid=13
order by lastfiledate desc"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
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
                                <asp:Button ID="btnDLAllReccommendations" runat="server" Text="Download" OnCommand="btnDLSuggestions_Command" CommandArgument="1" /></td>
                        </tr>
                        <tr>
                            <td>All Ranged Products (Suggested Qty's Only)</td>
                            <td>
                                <asp:DropDownList ID="ddlSuppliers" runat="server">
                                    <asp:ListItem Text="All" Enabled="true" Value="All"></asp:ListItem>
                                </asp:DropDownList></td>
                            <td>
                                <asp:Button ID="btnDLReccommendations" runat="server" Text="Download" OnCommand="btnDLSuggestions_Command" CommandArgument="2" /></td>
                        </tr>

                    </table>
                    
                        <h2>Vendor Lead Time Management</h2>

                        No# Of Weeks Forecast used:
                        <asp:DropDownList ID="ddlForecastAmountUsed" runat="server">
                            <asp:ListItem Value="1" Text="1"></asp:ListItem>
                            <asp:ListItem Value="2" Text="2"></asp:ListItem>
                            <asp:ListItem Value="3" Text="3"></asp:ListItem>
                            <asp:ListItem Value="4" Text="4"></asp:ListItem>
                            <asp:ListItem Value="5" Text="5"></asp:ListItem>
                            <asp:ListItem Value="6" Text="6"></asp:ListItem>
                            <asp:ListItem Value="7" Text="7"></asp:ListItem>
                            <asp:ListItem Value="8" Text="8"></asp:ListItem>
                            <asp:ListItem Value="9" Text="9"></asp:ListItem>
                            <asp:ListItem Value="10" Text="10"></asp:ListItem>
                            <asp:ListItem Value="11" Text="11"></asp:ListItem>
                            <asp:ListItem Value="12" Text="12"></asp:ListItem>
                            <asp:ListItem Value="13" Text="13"></asp:ListItem>
                            <asp:ListItem Value="14" Text="14"></asp:ListItem>
                            <asp:ListItem Value="15" Text="15"></asp:ListItem>

                        </asp:DropDownList>
                        <asp:Button ID="btnUpdateForecastWeeksUsed" runat="server" Text="Update" OnClick="btnUpdateForecastWeeksUsed_Click" />
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
                    <asp:Panel ID="pnlLeadTimes" runat="server" Visible="False">
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
                </div>
            </div>
        </div>
    </div>
</asp:Content>
