<%@ Page Title="Debit Note Portal | Claim Form" Language="C#" MasterPageFile="~/NewLayout/DNP.master" AutoEventWireup="true" Inherits="linx_tablets.NewLayout.ClaimForm" Codebehind="ClaimForm.aspx.cs" %>
<asp:Content runat="server" ID="HeadContent" ContentPlaceHolderID="HeadContent">

	<script type="text/javascript" src="Scripts/claim-listing.js"></script>
	<link rel="stylesheet" type="text/css" href="Styles/MP/gridview.css" />
	<link rel="stylesheet" type="text/css" href="Styles/claim-listing.css" />

</asp:Content>
<asp:Content runat="server" ID="MainContent" ContentPlaceHolderID="MainContent">

	<asp:ScriptManager EnablePartialRendering="true" ID="ClaimForm_ScriptManager" runat="server" />

	<div class="section-header"></div>
	<div class="section-content">

		<div class="dashboard-wrapper claims">

			<asp:UpdatePanel ID="upClaimHeader" runat="server">

				<Triggers>
					<asp:ASyncPostBackTrigger ControlID="btnFilterReport" />
				</Triggers>

				<ContentTemplate>

					<asp:HiddenField ID="ContactGV_hdnRowValid" Value="true" runat="server" />

					<!-- contacts -->

					<div ID="dvClaimHeader" Class="gridviewHeader" runat="server">
						<asp:Button ID="btnFilterReport" runat="server" Text="Filter results" OnClick="btnFilterReport_Click" />
                        &nbsp;<asp:Button ID="btnDownloadFilteredResults" runat="server" Text="Download results" Enabled="false" OnClick="btnDownloadFilteredResults_Click" />
						<asp:DropDownList ID="ddlStockStatusGV_FilterAccountManager" CssClass="filter-field" runat="server" />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterManufacturer" CssClass="filter-field" runat="server" />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterProductType" CssClass="filter-field" runat="server" />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterBusinessArea" Visible="false" Width="220px" CssClass="filter-field" runat="server" />
                        <br />
                        <table>
                            <tr>

                                <td style="padding-right: 20px;"></td>
                                <th style="padding-right: 20px;">Exertis Stock</th>
                                <th style="padding-right: 20px;">Exertis PO</th>
                                <th style="padding-right: 20px;">Backorders</th>
                                <th style="padding-right: 20px;">Stock By Weeks</th>
                            </tr>
                            <tr>
                                <td style="padding-right: 4px;"></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnExertisStock" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="In Stock" Value="Zero"></asp:ListItem>
                                        <asp:ListItem Text="OOS" Value="Not Zero"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnExertisPO" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="Has Pos" Value="Has Pos"></asp:ListItem>
                                        <asp:ListItem Text="No Pos" Value="No Pos"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnBackOrders" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="Backordered" Value="Backordered"></asp:ListItem>
                                        <asp:ListItem Text="No Backorders" Value="No Backorders"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnSafetyRating" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="Less 4 Wks" Value="4"></asp:ListItem>
                                        <asp:ListItem Text="4wks to 7wks" Value="7"></asp:ListItem>
                                        <asp:ListItem Text="8wks +" Value="8"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                                
                            </tr>
                        </table>
					</div>

					<asp:GridView ID="gvCustomerViewResults" OnLoad="gvCustomerViewResults_Load" runat="server" OnDataBound="gvCustomerViewResults_DataBound" AutoGenerateColumns="True" ShowHeaderWhenEmpty="True" AllowSorting="False" AllowPaging="True" PageSize="30">
						<PagerSettings mode="NumericFirstLast" firstpagetext="First" lastpagetext="Last" pagebuttoncount="5" position="Bottom" />

						<Columns>
							
						</Columns>
					</asp:GridView>

				</ContentTemplate>
			</asp:UpdatePanel>

		</div>
	</div>

</asp:Content>