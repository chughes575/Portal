<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="True" CodeBehind="WarrantyManagement.aspx.cs" Inherits="linx_tablets.WarrantyPortal.WarrantyManagement" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <script type='text/javascript' id='dispatchToggle'>
        function switchViews(obj) {
            var div = document.getElementById(obj);

            if (div.style.display == "none") {
                div.style.display = "inline";
            }
            else {
                div.style.display = "none";
            }
        }
    </script>
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <script type="@"></script>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <asp:SqlDataSource ID="sqlDSddlCustomers" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select distinct customer from mse_warrantyportalterms order by customer"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSddlVendors" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select distinct Vendor from mse_warrantyportalterms order by Vendor"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSApprovals" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select * from mse_warrantyportalapproval where salesapproval=0 or returnsapproval=0 or customersapproval=0 order by uploaddate desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSApprovalsComplete" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select * from mse_warrantyportalapproval where salesapproval=1 and returnsapproval=1 and customersapproval=1 order by uploaddate desc"></asp:SqlDataSource>


                    
                    <h1>Vendor Customer terms </h1>
                    <h2>Terms Management</h2>
                    *Please ensure the warranty file is saved as a csv before uploading
                    <br />
                    <asp:FileUpload ID="fuWarrantyUpload" runat="server" /><br />
                    <asp:Button ID="btnWarrantyUpload" runat="server" Text="Upload" OnClick="btnWarrantyUpload_Click" />
                    &nbsp;
                    <asp:Button ID="btnWarrantyDownload" runat="server" Text="Downloading Existing terms" OnClick="btnWarrantyDownload_Click" />

                    <h3>Term approvals</h3>

                    With your security access you can carry out the following approval actions
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Sales Approval</th><th>Returns Approval</th><th>Customer Services Approval</th>
                        </tr>
                        <tr>
                            <td><asp:Image ID="imgSales" runat="server" ImageUrl="~/Images/tick.png" Width="10px" /></td>
                            <td><asp:Image ID="imgReturns" runat="server" ImageUrl="~/Images/tick.png" Width="10px" /></td>
                            <td><asp:Image ID="imgCustomers" runat="server" ImageUrl="~/Images/tick.png" Width="10px" /></td>
                        </tr>
                    </table>
                    <br />
                    <h4>Outstanding Approvals</h4>
                    <asp:GridView ID="gvApprovals" CssClass="CSSTableGenerator" EmptyDataText="No outstanding approvals" runat="server" DataSourceID="sqlDSApprovals" AutoGenerateColumns="false" OnRowCommand="gvApprovals_RowCommand" OnRowDataBound="gvApprovals_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="ApprovalID" HeaderText="ApprovalID" />
                            <asp:BoundField DataField="UploadDate" HeaderText="Upload Date" DataFormatString="{0:d}" />
                            <asp:BoundField DataField="UploadedBy" HeaderText="Uploaded By" />
                            <asp:BoundField DataField="SalesApproval" HeaderText="Sales Approval" />
                            <asp:BoundField DataField="SalesApprovalUser" HeaderText="Sales Approval by" />
                            <asp:TemplateField>
                                <HeaderTemplate>Approve Changes- Sales</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnApproveSales" runat="server" CommandName="approveTermsSales" CommandArgument='<%# Eval("ApprovalID") %>' Text="Approve" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ReturnsApproval" HeaderText="Returns Approval" />
                            <asp:BoundField DataField="ReturnsApprovalUser" HeaderText="Returns Approval by" />
                            <asp:TemplateField>
                                <HeaderTemplate>Approve Changes- Returns</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnApproveReturns" runat="server" CommandName="approveTermsReturns" CommandArgument='<%# Eval("ApprovalID") %>' Text="Approve" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="CustomersApproval" HeaderText="Customer Services Approval" />
                            <asp:BoundField DataField="CustomersApprovalUser" HeaderText="Customer Services Approval by" />
                            <asp:TemplateField>
                                <HeaderTemplate>Approve Changes- Customer Service</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnApproveCustomers" runat="server" CommandName="approveTermsCustomers" CommandArgument='<%# Eval("ApprovalID") %>' Text="Approve" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Download modified terms</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnDownloadModifiedTerms" runat="server" CommandName="downloadModifiedTerms" CommandArgument='<%# Eval("ApprovalID") %>' Text="Download" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <h4>Completed Approvals</h4>
                    <asp:GridView ID="gvApprovalsComplete" EmptyDataText="No completed approvals" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSApprovalsComplete" AutoGenerateColumns="false" OnRowCommand="gvApprovals_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="ApprovalID" HeaderText="ApprovalID" />
                            <asp:BoundField DataField="UploadDate" HeaderText="Upload Date" DataFormatString="{0:d}" />
                            <asp:BoundField DataField="UploadedBy" HeaderText="Uploaded By" />
                            <asp:BoundField DataField="SalesApproval" HeaderText="Sales Approval" />
                            <asp:BoundField DataField="SalesApprovalUser" HeaderText="Sales Approval by" />
                            <asp:BoundField DataField="ReturnsApprovalUser" HeaderText="Returns Approval by" />
                            <asp:BoundField DataField="CustomersApprovalUser" HeaderText="Customer Services Approval by" />
                            <asp:TemplateField>
                                <HeaderTemplate>Download modified terms</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnDownloadModifiedTerms" runat="server" CommandName="downloadModifiedTerms" CommandArgument='<%# Eval("ApprovalID") %>' Text="Download" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <h2>View terms</h2>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Customer</th>
                            <th>Manufacturers</th>
                            <th>Product Code</th>
                            <th>Customer Product Code</th>
                            <th>Search</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:DropDownList ID="ddlCustomers" OnSelectedIndexChanged="ddlCustomers_SelectedIndexChanged" runat="server" DataTextField="Customer" DataValueField="Customer" DataSourceID="sqlDSddlCustomers" AppendDataBoundItems="true">
                                    <asp:ListItem Value="">All customers</asp:ListItem>
                                </asp:DropDownList></td>
                            <td>
                                <asp:DropDownList ID="ddlVendors" OnSelectedIndexChanged="ddlVendors_SelectedIndexChanged" runat="server" DataTextField="Vendor" DataValueField="Vendor" DataSourceID="sqlDSddlVendors" AppendDataBoundItems="true">
                                    <asp:ListItem Value="">All vendors</asp:ListItem>
                                </asp:DropDownList></td>
                            <td>
                                <asp:TextBox ID="txtProductCode" runat="server" Text=""></asp:TextBox></td>
                            <td>
                                <asp:TextBox ID="txtCustomerProductCode" runat="server" Text=""></asp:TextBox></td>
                            <td>
                                <asp:Button ID="btnViewTerms" runat="Server" Text="View terms" OnClick="btnViewTerms_Click" /></td>
                        </tr>
                    </table>

                    <h3>Warranty terms search results</h3>
                    <asp:GridView ID="gvWarrantySearchTerms" ShowFooter="true" EmptyDataText="No matching terms found" runat="server" CssClass="CSSTableGenerator" AutoGenerateColumns="false" OnRowDataBound="gvWarrantySearchTerms_RowDataBound">
                        <Columns>
                            <asp:TemplateField>
                                <FooterTemplate>
                                    <asp:Button ID="btnDownloadSearchResults" OnClick="btnDownloadSearchResults_Click" runat="server" Text="Download Search Results" />
                                </FooterTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="View full terms">
                                <ItemTemplate>
                                    <a href="javascript:switchViews('div<%# Eval("TermID") %>');">
                                        <img id='imgdiv<%# Eval("TermID") %>' alt="toggle" width="10px" border="0"
                                            src="/images/view.png" />
                                    </a>

                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Customer" HeaderText="Customer" />
                            <asp:BoundField DataField="Vendor" HeaderText="Manufacturer" />
                            <asp:BoundField DataField="Business_Unit" HeaderText="Business Unit" />
                            <asp:BoundField DataField="Faulty" HeaderText="Faulty" />
                            <asp:BoundField DataField="Faulty_Terms" HeaderText="Faulty" />
                            <asp:BoundField DataField="Change_Of_Mind_Remorse" HeaderText="Change Of Mind Remorse" />
                            <asp:BoundField DataField="COM_Remorse_Terms" HeaderText="COM Terms" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <tr>
                                        <td colspan="100">
                                            <div id='div<%# Eval("TermID") %>' style="display: none; position: relative; left: 8px;">

                                                <asp:FormView ID="fvTermDataFull" runat="server" OnItemCommand="fvTermDataFull_ItemCommand">
                                                    <ItemTemplate>
                                                        <div class="datagrid">
                                                            <table>
                                                                <thead>
                                                                    <tr>
                                                                        <th>TermID</th>
                                                                        <td>
                                                                            <asp:Label ID="lblTermID" runat="server" Text='<%# Eval("TermID") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Faulty</th>
                                                                        <td>
                                                                            <asp:Label ID="lblFaulty" runat="server" Text='<%# Eval("Faulty") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Faulty Terms</th>
                                                                        <td>
                                                                            <asp:Label ID="lblFaultyTerms" runat="server" Text='<%# Eval("Faulty_Terms") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Change Of Mind Remorse</th>
                                                                        <td>
                                                                            <asp:Label ID="lblCOMRemorse" runat="server" Text='<%# Eval("Change_Of_Mind_Remorse") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Change Of Mind Remorse Terms</th>
                                                                        <td>
                                                                            <asp:Label ID="lblCOMRemorseTerms" runat="server" Text='<%# Eval("COM_Remorse_Terms") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>

                                                                <thead>
                                                                    <tr>
                                                                        <th>Stock Rotations</th>
                                                                        <td>
                                                                            <asp:Label ID="lblStockRotations" runat="server" Text='<%# Eval("Stock_Rotations") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Rotations Terms</th>
                                                                        <td>
                                                                            <asp:Label ID="lblStockRotationsTerms" runat="server" Text='<%# Eval("Rotations_Terms") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>

                                                                <thead>
                                                                    <tr>
                                                                        <th>Exertis Deal With NFF</th>
                                                                        <td>
                                                                            <asp:Label ID="lblDealWithNFF" runat="server" Text='<%# Eval("Exertis_Deal_With_NFF") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Exertis Deal With CID</th>
                                                                        <td>
                                                                            <asp:Label ID="lblDealWithCID" runat="server" Text='<%# Eval("Exertis_Deal_With_CID") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>

                                                                <thead>
                                                                    <tr>
                                                                        <th>CID Terms</th>
                                                                        <td>
                                                                            <asp:Label ID="lblCIDTerms" runat="server" Text='<%# Eval("CID_Terms") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Who Deals With Warranty Repaired Devices</th>
                                                                        <td>
                                                                            <asp:Label ID="lblWhoDealsWithWarrantyRepairedDevices" runat="server" Text='<%# Eval("Who_Deals_With_Warranty_Repaired_Devices") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>

                                                                <thead>
                                                                    <tr>
                                                                        <th>Vendor Terms DOA</th>
                                                                        <td>
                                                                            <asp:Label ID="lblVendorTermsDoa" runat="server" Text='<%# Eval("Vendor_Term___DOA") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Vendor Terms Warranty</th>
                                                                        <td>
                                                                            <asp:Label ID="lblVendorTermsWarranty" runat="server" Text='<%# Eval("Vendor_Term___Warranty") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>

                                                                <thead>
                                                                    <tr>
                                                                        <th>Last Updated</th>
                                                                        <td>
                                                                            <asp:Label ID="lblLastUpdated" runat="server" Text='<%# Eval("Last_Update") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Agreed By</th>
                                                                        <td>
                                                                            <asp:Label ID="lblAgrredBy" runat="server" Text='<%# Eval("Agreed_By") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>

                                                                <thead>
                                                                    <tr>
                                                                        <th>Review Date</th>
                                                                        <td>
                                                                            <asp:Label ID="lblReviewDate" runat="server" Text='<%# Eval("Review_Date") %>'></asp:Label></td>
                                                                    </tr>
                                                                    <thead>
                                                                        <tr>
                                                                            <th>Exposure</th>
                                                                            <td>
                                                                                <asp:Label ID="lblExposure" runat="server" Text='<%# Eval("Exposure") %>'></asp:Label></td>
                                                                        </tr>
                                                                    </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Bridge</th>
                                                                        <td>
                                                                            <asp:Label ID="lblBridge" runat="server" Text='<%# Eval("Bridge") %>'></asp:Label></td>
                                                                    </tr>
                                                                </thead>
                                                                <thead>
                                                                    <tr>
                                                                        <th>Download term data</th>
                                                                        <td>
                                                                            
                                                                            <asp:LinkButton ID="lnkDownloadTermData" runat="server" CommandArgument='<%# Eval("TermID") %>' CommandName="downloadTermData" Text="Download"></asp:LinkButton></td>
                                                                    </tr>
                                                                </thead>
                                                            </table>
                                                        </div>
                                                    </ItemTemplate>
                                                </asp:FormView>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                    </asp:GridView>

                </div>
            </div>
        </div>
    </div>
</asp:Content>
