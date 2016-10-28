<%@ Page Title="Debit Note Portal | Claim Form" Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeFile="claim-form.aspx.vb" Inherits="_ClaimForm" %>
<asp:Content runat="server" ID="HeadContent" ContentPlaceHolderID="HeadContent">
	
	<script type="text/javascript" src="Scripts/claim-form.js?v=2"></script>
	<script type="text/javascript" src="Scripts/MP/cut-and-paste.js"></script>
	<link rel="stylesheet" type="text/css" href="Styles/MP/gridview.css" />
	<link rel="stylesheet" type="text/css" href="Styles/claim-form.css" />

</asp:Content>
<asp:Content runat="server" ID="MainContent" ContentPlaceHolderID="MainContent">

	<asp:ScriptManager EnablePartialRendering="true" ID="ClaimForm_ScriptManager" runat="server" />

	<div class="claim-header row-1">
		<div id="header-title">
			Credit Request Details: <asp:TextBox ID="tbStatus" runat="server" />
			Date Raised: <asp:TextBox ID="tbDateRaised" runat="server" />&nbsp;&nbsp;|&nbsp;&nbsp;System ID: <asp:TextBox ID="tbRequestID" runat="server" /></div>
		<div id="header-owning-user"><asp:DropDownList ID="ddOwningUserID" runat="server" /></div>
	</div>

	<div class="claim-header row-2">

		<asp:UpdatePanel ID="upClaimTypeData" runat="server">
			<ContentTemplate>

				<div class="box box-1">
					<ul>
						<li>
							<asp:Label ID="lblClaimTypeID" runat="server">Claim Type</asp:Label>
							<asp:DropDownList ID="ddClaimTypeID" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblReasonID" runat="server">Claim Reason</asp:Label>
							<asp:DropDownList ID="ddReasonID" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblPromotionID" runat="server">Promotion</asp:Label>
							<asp:DropDownList ID="ddPromotionID" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblPriceProtectionNo" runat="server">Price Protection Number</asp:Label>
							<asp:TextBox ID="tbPriceProtectionNo" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblOriginalDnNo" runat="server">Original DN Number</asp:Label>
							<asp:TextBox ID="tbOriginalDnNo" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblReversingDnNo" runat="server">Reversing DN Number</asp:Label>
							<asp:TextBox ID="tbReversingDnNo" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblTransmissionRequired" runat="server">Send to Vendor</asp:Label>
							<asp:Checkbox ID="cbTransmissionRequired" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblAdditionalRecipients" runat="server">Additional Contacts</asp:Label>
							<div id="dvAdditionalRecipientsWrapper" runat="server"><asp:CheckboxList ID="cblAdditionalRecipients" runat="server" /></div>
						</li>
                        <li>
                           <asp:Panel ID="pnlEmailHistory" Visible="false" runat="server">

                           
                            Vendor Email History <br />
                            <asp:ListBox ID="lstBoxVendorEmails" Width="290px" runat="server">
                            </asp:ListBox>
                               </asp:Panel></li>
					</ul>

					<asp:HiddenField ID="hdnDNTypeID" runat="server" />

				</div>

			</ContentTemplate>
		</asp:UpdatePanel>


		<asp:UpdatePanel ID="upVendorData" runat="server">
			<ContentTemplate>

				<div class="box box-2">
					<ul>
						<li>
							<asp:Label ID="lblVendorID" runat="server">Vendor</asp:Label>
							<asp:DropDownList ID="ddVendorID" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblDivisionID" runat="server">Division</asp:Label>
							<asp:DropDownList ID="ddDivisionID" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblVendorAddress" runat="server">Address</asp:Label>
							<asp:TextBox ID="tbVendorAddress" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblCurrency" runat="server">Currency</asp:Label>
							<asp:TextBox ID="tbCurrency" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblPrimaryContact" runat="server">Primary Contact</asp:Label>
							<asp:TextBox ID="tbPrimaryContact" runat="server" />
						</li>
						<li>
							<asp:Label ID="lblAccountContact" runat="server">Account Contact</asp:Label>
							<asp:TextBox ID="tbAccountContact" runat="server" />
						</li>
					</ul>

					<asp:HiddenField ID="hdnLocationID" runat="server" />
					<asp:HiddenField ID="hdnBusinessUnitID" runat="server" />
					<asp:HiddenField ID="hdnCurrencyID" runat="server" />
					<asp:HiddenField ID="hdnPrimaryContactID" runat="server" />
					<asp:HiddenField ID="hdnAccountContactID" runat="server" />
					<asp:HiddenField ID="hdnClaimStatus" runat="server" />

				</div>

			</ContentTemplate>
		</asp:UpdatePanel>

		<div class="box box-3">
			<ul>
				<li>
					<asp:Label ID="lblVendorReference" runat="server">Vendor Ref:</asp:Label>
					<asp:TextBox ID="tbVendorReference" runat="server" />
				</li>
				<li>
					<asp:Label ID="lblRequestDescription" runat="server">Description</asp:Label>
					<asp:TextBox ID="tbRequestDescription" TextMode = "Multiline" height="80px" runat="server" />
				</li>
				<li>
					<asp:Label ID="lblPrivateNotes" runat="server">Private Notes</asp:Label>
					<asp:TextBox ID="tbPrivateNotes" TextMode = "Multiline" height="50px" runat="server" />
				</li>
			</ul>
		</div>
	
	</div>

	<div class="claim-header row-3">
		
		<asp:HiddenField ID="hdnCmd" runat="server" />
		<asp:HiddenField ID="hdnInvalidFields" runat="server" />

		<asp:UpdatePanel ID="upFooter" runat="server">

			<Triggers>
				<asp:PostBackTrigger ControlID="btnSave" />
				<asp:PostBackTrigger ControlID="btnProceed" />
				<asp:PostBackTrigger ControlID="btnRollBack" />
				<asp:PostBackTrigger ControlID="btnSaveAsXLSX" />
			</Triggers>

			<ContentTemplate>

				<div class="footer-content">
					
					<div id="btn-actions">
						<asp:Button ID="btnRollBack" CssClass="btn" runat="server" />
						<asp:Button ID="btnSave" CssClass="btn" runat="server" />
						<asp:Button ID="btnProceed" CssClass="btn" runat="server" />
					</div>
					<div id="linked-actions">
						<asp:ImageButton ID="btnViewHistory" CssClass="header-icon" OnClientClick="modalViewHistory(this.id)" ToolTip="View History" ImageURL="images/icon-calendar-30.png" runat="server" />
						<asp:ImageButton ID="btnECLGVHeaderUpload" CssClass="header-icon" OnClientClick="modalHeaderUploadForm(this.id)" ToolTip="Manage Claim Header Files" runat="server" />
						<asp:ImageButton ID="btnSaveAsXLSX" CssClass="header-icon" ToolTip="Save As Excel Document" ImageURL="images/icon-save-xlsx.png" runat="server" />
					</div>					
					<div id="control-message"><asp:Label ID="lblControlMessage" runat="server" /></div>
					<div id="claim-total">
						<asp:Label ID="lblOutstandingAmount" runat="server">Outstanding Amount: </asp:Label>
						<asp:TextBox ID="tbOutstandingAmount" runat="server" />
					</div>

				</div>

			</ContentTemplate>
		</asp:UpdatePanel>
	</div>

	<div class="claim-lines">

		<asp:UpdatePanel ID="upClaimLine" runat="server">

			<Triggers>
				<asp:ASyncPostBackTrigger ControlID="btnClaimLineGV_Search" />
			</Triggers>

			<ContentTemplate>

				<asp:TextBox ID="tbClipboard" CssClass="hidden" TextMode = "Multiline" Text="" runat="server" />
				<asp:HiddenField ID="NewClaimLineGV_hdnRowValid" Value="false" runat="server" />
				<asp:HiddenField ID="ExistingClaimLineGV_hdnRowValid" Value="true" runat="server" />

				<!-- new claim line -->
				<asp:GridView ID="NewClaimLineGV" runat="server" AutoGenerateColumns="False" ShowHeaderWhenEmpty="True">
					<PagerSettings mode="NumericFirstLast" firstpagetext="First" previouspagetext="Previous" nextpagetext="Next" lastpagetext="Last" pagebuttoncount="5" position="Bottom" />

					<Columns>

						<asp:TemplateField HeaderText="" ShowHeader="False">

							<EditItemTemplate>
								<asp:ImageButton ID="btnNCLGVUpdate" CssClass="line-icon submit-btn" CommandName="Update" ToolTip="Save Claim" ImageURL="Images/icon-tick.png" runat="server" />
								<asp:ImageButton ID="btnNCLGVCancel" CssClass="line-icon" CommandName="Cancel" ToolTip="Cancel Changes" ImageURL="Images/icon-cancel.png" runat="server" />
								<div id="paste-tool">
									<label for='NewClaimLineGV_tbPasteClaimLine_0' class='paste-field-label'>Paste Line</label>
									<asp:TextBox ID="tbPasteClaimLine" CssClass="paste-field" TextMode="Multiline" runat="server" />
								</div>
							</EditItemTemplate>

						</asp:TemplateField>

					</Columns>
				</asp:GridView>


				<!-- search claim lines -->
				<div ID="dvClaimLineSearch" Class="gridviewHeader" runat="server">
					<div id="search-label">Search Existing Claim Lines: </div><asp:TextBox ID="txClaimLineGV_Search" runat="server" />
					<asp:Button ID="btnClaimLineGV_Search" CssClass="search-btn" runat="server" />
				</div>
                <br />
                <br />
                <asp:ImageButton ID="btnECLGVDeleteAllLines" CssClass="line-icon delete-btn"  OnClick="btnECLGVDeleteAllLines_Click" OnClientClick="if (!window.confirm('Are you sure you want to delete all claim items?')) return false;" ToolTip="Delete All Claim Line" ImageURL="Images/icon-delete.png" runat="server" />
				<!-- existing claim lines -->
				<asp:GridView ID="ExistingClaimLineGV" runat="server" OnDataBound="ExistingClaimLineGV_DataBound" AutoGenerateColumns="False" ShowHeaderWhenEmpty="True" AllowSorting="True" AllowPaging="True" PageSize="10">
					<PagerSettings mode="NumericFirstLast" firstpagetext="First" lastpagetext="Last" pagebuttoncount="5" position="Bottom" />

					<Columns>
                        
						<asp:TemplateField HeaderText="" ShowHeader="False">

							<EditItemTemplate>
								<asp:ImageButton ID="btnECLGVDelete" CssClass="line-icon delete-btn" OnClientClick="if (!window.confirm('Are you sure you want to delete this item?')) return false;" CommandName="Delete" ToolTip="Delete Claim Line" ImageURL="Images/icon-delete.png" runat="server" />
								<asp:ImageButton ID="btnECLGVCancel" CssClass="line-icon cancel-btn" CommandName="Cancel" ToolTip="Cancel Changes" ImageURL="Images/icon-cancel.png" runat="server" />
								<asp:ImageButton ID="btnECLGVUpdate" CssClass="line-icon submit-btn" CommandName="Update" ToolTip="Save Changes" ImageURL="Images/icon-tick.png" runat="server" />
								<asp:ImageButton ID="btnECLGVLineUpload" CssClass="line-icon line-attachment" OnClientClick="modalLineUploadForm(this.id)" ToolTip="Manage Claim Line Files" ImageURL="Images/shim.gif" runat="server" value=" " />
							</EditItemTemplate>

							<ItemTemplate>
								<asp:ImageButton ID="btnECLGVLineUpload" CssClass="line-icon line-attachment" OnClientClick="modalLineUploadForm(this.id)" ToolTip="Manage Claim Line Files" ImageURL="Images/shim.gif" runat="server" value=" " />
							</ItemTemplate>

						</asp:TemplateField>

					</Columns>
				</asp:GridView>
                
                                
                            
				<asp:HiddenField ID="hdnNumberOfClaimLines" runat="server" />

			</ContentTemplate>
		</asp:UpdatePanel>

	</div>

</asp:Content>