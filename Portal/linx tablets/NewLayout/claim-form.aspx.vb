Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Xml.Linq
Imports MySql.Data
Imports MySql.Data.MySqlClient

Imports DebitNote.Helpers
Imports DebitNote.MsgQUtils
Imports DebitNote.Claim

'-------------------------------------------

' Claim Form Class

'-------------------------------------------

Partial Class _ClaimForm

	Inherits System.Web.UI.Page

	Private ciGlobal As Globalization.CultureInfo
	Private ciGB As New Globalization.CultureInfo("en-GB", True)
	Private ciUS As New Globalization.CultureInfo("en-US", True)
	Private ciEU As New Globalization.CultureInfo("de-DE", True)
	Private ciHK As New Globalization.CultureInfo("zh-HK", True)
	Private ciKR As New Globalization.CultureInfo("sv-SE", True)

	Private mySqlDataAdapter As MySqlDataAdapter
	Private mySqlConnection As MySqlConnection
	Private mySqlCommand As New MySqlCommand()
	Private mySqlConnectionString As String = ConfigurationManager.ConnectionStrings("DBDebitNote").ConnectionString

	Private utils As Utils = New Utils
	Private gridviewExt As GridviewExt = New GridviewExt
	Private dbConnector As DBConnector = New DBConnector

	Private claimHeader As ClaimHeader = New ClaimHeader
	Private claimLine As ClaimLine = New ClaimLine

	Private requestID As Integer

	'-------------------------------------------

	' Page load & pre-render

	'-------------------------------------------

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

		'-------------------------------------------

		' Claim header

		'-------------------------------------------

		SetPostBackCount(Page.IsPostBack)

		If Page.IsPostBack Then

			ClearSystemMessage()

			If (hdnClaimStatus.Value = 0 Or hdnClaimStatus.Value = 10 Or hdnClaimStatus.Value = 40) Then ' DRAFT or REJECTED status
				SetClaimHeaderGetForm()	' get current claim header data from form
			Else

				claimHeader.SetClaimHeaderGetDB(tbRequestID.Text)	' read only form - get claim header from database

				' get data from hidden fields
				claimHeader.SetBusinessUnitID(hdnBusinessUnitID.Value)
				claimHeader.SetLocationID(hdnLocationID.Value)
				claimHeader.SetDNTypeID(hdnDNTypeID.Value)
				claimHeader.SetCurrencyID(hdnCurrencyID.Value)
				claimHeader.SetClaimStatusID(hdnClaimStatus.Value)
				claimHeader.SetNumberOfClaimLines(hdnNumberOfClaimLines.Value)

			End If

			'// controls config
			ManageValidatedControls()	' setup form config based on claim status

		Else

			ClearSystemMessage()

			If Not IsNothing(Request.QueryString("id")) Then ' get request_id from querystring
				requestID = Request.QueryString("id")
			Else
				requestID = 0
			End If

			claimHeader.SetClaimHeaderGetDB(requestID) ' set claim header data from database

			'// set ajax controls
			ddClaimTypeID.AutoPostBack = True
			ddVendorID.AutoPostBack = True
			ddDivisionID.AutoPostBack = True
			tbClipboard.AutoPostBack = True

			'// assign CssClass
			'tbDateRaised.CssClass = "datepicker validate date"

			If claimHeader.GetRequestID = 0 Then ' set config for new claim

				'// controls config / header
				claimHeader.SetClaimStatusID(0)	' set claim status to pre draft

				cbTransmissionRequired.Checked = True

				'// bind data
				tbRequestID.Text = "-"
				tbStatus.Text = "-"

				BindData_ClaimTypeID() ' claim type - call bind data sub
				ddClaimTypeID.SelectedValue = 0
				ManageClaimTypeControls("hide-all")

				BindData_VendorID(0)	' vendor - call bind data sub
				ddVendorID.SelectedValue = 0
				ManageVendorControls("hide-all")

				BindData_OwningUserID(0) ' call bind data sub
				ddOwningUserID.SelectedValue = Session("userID")

                tbDateRaised.Text = utils.ConvertToUIDate(Now)

				'// controls config
				ManageClaimControls(0) ' setup form config based on claim status

			Else ' set config for existing claim

				If claimHeader.GetTransmissionRequired() = "1" Then
					cbTransmissionRequired.Checked = True
				Else
					cbTransmissionRequired.Checked = False
				End If

				'// bind data
				tbRequestID.Text = claimHeader.GetRequestID() ' update ui control value - request id
				tbPriceProtectionNo.Text = claimHeader.GetPriceProtectionNo	' update ui control value - price protection number
				tbOriginalDnNo.Text = claimHeader.GetOriginalDnNo ' update ui control value - original debit note number
				tbReversingDnNo.Text = claimHeader.GetReversingDnNo	' update ui control value - reversing debit note number
				tbVendorReference.Text = claimHeader.GetVendorReference	' update control value - vendor
				tbRequestDescription.Text = claimHeader.GetRequestDescription ' update control value - description
				tbDateRaised.Text = utils.ConvertToUIDate(claimHeader.GetDateRaised)	' update control value - date
				tbPrivateNotes.Text = claimHeader.GetPrivateNotes ' update control value - private notes
                Dim claimStatusID As Integer = claimHeader.GetClaimStatusID()

                If (claimStatusID = 70 Or claimStatusID = 90 Or claimStatusID = 100) Then
                    tbPrivateNotes.Enabled = False
                End If
                BindData_OwningUserID(claimHeader.GetOwningUserID)  ' bind data to user dd
                ddOwningUserID.SelectedValue = claimHeader.GetOwningUserID ' update control value - user

                BindData_ClaimTypeID() ' bind data to claim type dd
                ddClaimTypeID.SelectedValue = claimHeader.GetClaimTypeID ' update control value - claim type

                If claimHeader.GetClaimTypeID() = 0 Then ' show hide/reset claim type controls if claim type (un)selected
                    ManageClaimTypeControls("hide-all")
                Else
                    ManageClaimTypeControls("show-all")
                End If

                BindData_VendorID(claimHeader.GetVendorID)  ' bind data to vendor type dd
                ddVendorID.SelectedValue = claimHeader.GetVendorID ' update control value - vendor

                If claimHeader.GetVendorID() = 0 Then ' show hide/reset vendor controls if vendor (un)selected
                    ManageVendorControls("hide-all")
                Else
                    ManageVendorControls("show-divisions")
                End If

                If claimHeader.GetDivisionID() = 0 Then ' show hide/reset vendor/division controls if division (un)selected
                    ManageVendorControls("hide-details")
                Else
                    ManageVendorControls("show-details")
                End If

                ManageClaimControls(claimHeader.GetClaimStatusID) ' setup form control config based on claim status

            End If
        End If

        Dim ddDataSetEmails As New DataSet()
        mySqlConnection = New MySqlConnection(mySqlConnectionString)
        mySqlCommand.CommandText = "select count(*) from history_t where object_id=" & claimHeader.GetRequestID & " and tablename='credit_request_header' and message='Claim Sent To Vendor';" 
       
        mySqlCommand.Connection = mySqlConnection
        mySqlConnection.Open()
        Dim noOfClaimEmails As Integer = Integer.Parse(mySqlCommand.ExecuteScalar().ToString())
        mySqlConnection.Close()

        If (noOfClaimEmails > 0) Then
            mySqlCommand.CommandText = "select concat('Claim Sent To Vendor: ', cast(date_created as char)) as Val  from history_t where object_id=" & claimHeader.GetRequestID & " and tablename='credit_request_header' and message='Claim Sent To Vendor';"

            mySqlCommand.Connection = mySqlConnection

            mySqlDataAdapter = New MySqlDataAdapter(mySqlCommand)
            mySqlConnection.Open()
            mySqlDataAdapter.Fill(ddDataSetEmails)
            mySqlConnection.Close()

            lstBoxVendorEmails.DataSource = ddDataSetEmails
            lstBoxVendorEmails.DataTextField = "Val"
            lstBoxVendorEmails.DataBind()
            pnlEmailHistory.Visible = True
            End If




            '-------------------------------------------

            ' Claim lines

            '-------------------------------------------

        claimLine.gvSettings = claimLine.GetGVTemplateSettings(claimHeader.GetClaimTypeID(), claimHeader.GetVendorID()) ' get gridview template settings
        btnClaimLineGV_Search.Text = "Go!"

        If Not Page.IsPostBack Then ' create gridview columns - assign boundfields to gridviews
            claimLine.CreateColumns_ClaimLineGV(ExistingClaimLineGV) ' assign existing claim lines
            claimLine.CreateColumns_ClaimLineGV(NewClaimLineGV) ' assign new claim lines
            End If

            '// check for expired elements		
        Dim dtCurrentDate As DateTime = DateTime.Now
        Dim sqlCurrentDate As String = utils.ConvertToSQLDate(dtCurrentDate.ToString())
        Dim invalidFields = ""

        If claimHeader.GetVendorID > 0 Then

            Dim VendorValid As Boolean = False
            Dim VendorTest As String = ""
            VendorTest = VendorTest & "select * from vendor_t "
            VendorTest = VendorTest & "join vendor_address_t on vendor_t.vendor_id = vendor_address_t.vendor_id "
            VendorTest = VendorTest & "where ("
            VendorTest = VendorTest & "vendor_t.start_date_active < '" & sqlCurrentDate & "' "
            VendorTest = VendorTest & "and (vendor_t.end_date_active IS NULL or vendor_t.end_date_active > '" & sqlCurrentDate & "') "
            VendorTest = VendorTest & ") "
            VendorTest = VendorTest & "and vendor_t.vendor_id = " & claimHeader.GetVendorID & " "
            VendorTest = VendorTest & "group by vendor_t.vendor_id "

            VendorValid = dbConnector.DoesDBRecordExist(VendorTest)

            If VendorValid = False Then
                invalidFields = invalidFields & "|ddVendorID"
                End If

            End If

        If claimHeader.GetDivisionID > 0 Then

            Dim VendorAddressValid As Boolean = False
            Dim VendorAddressTest As String = ""
            VendorAddressTest = VendorAddressTest & "select * from division_t "
            VendorAddressTest = VendorAddressTest & "join vendor_address_t on division_t.location_id = vendor_address_t.location_id "
            VendorAddressTest = VendorAddressTest & "join business_unit_t on division_t.business_unit_id = business_unit_t.business_unit_id "
            VendorAddressTest = VendorAddressTest & "where ("
            VendorAddressTest = VendorAddressTest & "vendor_address_t.vendor_id = " & claimHeader.GetVendorID & " "
            VendorAddressTest = VendorAddressTest & "and (vendor_address_t.inactive_date IS NULL or vendor_address_t.inactive_date > '" & sqlCurrentDate & "') " ' expired vendor address
            VendorAddressTest = VendorAddressTest & "and (business_unit_t.end_date_active IS NULL or business_unit_t.end_date_active > '" & sqlCurrentDate & "') " ' expired business unit
            VendorAddressTest = VendorAddressTest & "and business_unit_t.enabled_flag = 'Y' " ' disabled business unit
            VendorAddressTest = VendorAddressTest & ") "
            VendorAddressTest = VendorAddressTest & "and vendor_address_t.location_id = " & claimHeader.GetLocationID & " "
            VendorAddressTest = VendorAddressTest & "group by vendor_address_t.location_id "

            VendorAddressValid = dbConnector.DoesDBRecordExist(VendorAddressTest)

            If VendorAddressValid = False Then
                invalidFields = invalidFields & "|ddDivisionID"
                End If

            End If

        If claimHeader.GetReasonID > 0 Then

            Dim thisDebitNoteTypeID As Integer = dbConnector.ID2Field("claim_type_t", claimHeader.GetClaimTypeID, "claim_type_id", "dn_type_id")

            Dim ReasonValid As Boolean = False
            Dim ReasonTest As String = ""
            ReasonTest = ReasonTest & "select * from claim_reason_t "
            ReasonTest = ReasonTest & "where ("
            ReasonTest = ReasonTest & "dn_type_id = " & thisDebitNoteTypeID & " "
            ReasonTest = ReasonTest & "and enabled_flag = 'Y' "
            ReasonTest = ReasonTest & "and (end_date_active IS NULL or end_date_active > '" & sqlCurrentDate & "') "
            ReasonTest = ReasonTest & ") "
            ReasonTest = ReasonTest & "and reason_id = " & claimHeader.GetReasonID & " "

            ReasonValid = dbConnector.DoesDBRecordExist(ReasonTest)

            If ReasonValid = False Then
                invalidFields = invalidFields & "|ddReasonID"
                End If

            End If

        If claimHeader.GetPromotionID > 0 Then

            Dim PromotionValid As Boolean = False
            Dim PromotionTest As String = ""
            PromotionTest = PromotionTest & "select * from promotion_t "
            PromotionTest = PromotionTest & "where ("
            PromotionTest = PromotionTest & "enabled_flag = 'Y' "
            PromotionTest = PromotionTest & "and (end_date_active IS NULL or end_date_active > '" & sqlCurrentDate & "') "
            PromotionTest = PromotionTest & ") "
            PromotionTest = PromotionTest & "and promotion_id = " & claimHeader.GetPromotionID & " "

            PromotionValid = dbConnector.DoesDBRecordExist(PromotionTest)

            If PromotionValid = False Then
                invalidFields = invalidFields & "|ddPromotionID"
                End If

            End If

        If claimHeader.GetOwningUserID > 0 Then

            Dim UserValid As Boolean = False
            Dim UserTest As String = ""
            UserTest = UserTest & "select * from user_t "
            UserTest = UserTest & "where ("
            UserTest = UserTest & "enabled_flag = 'Y' "
            UserTest = UserTest & "and (user_end_date_active IS NULL or user_end_date_active > '" & sqlCurrentDate & "') "
            UserTest = UserTest & ") "
            UserTest = UserTest & "and user_id = " & claimHeader.GetOwningUserID & " "

            UserValid = dbConnector.DoesDBRecordExist(UserTest)

            If UserValid = False Then
                invalidFields = invalidFields & "|ddOwningUserID"
                End If

            End If

        hdnInvalidFields.Value = invalidFields

    End Sub

	Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender

		'// update new claim line reference
		Dim tbLineClaimReference As TextBox = TryCast(NewClaimLineGV.Rows(NewClaimLineGV.EditIndex).Cells(1).Controls(0), TextBox)
		tbLineClaimReference.Text = utils.DoubleZeroOne(claimHeader.GetNumberOfClaimLines + 1) & ".0"

		Dim tbLineID As TextBox = TryCast(NewClaimLineGV.Rows(NewClaimLineGV.EditIndex).Cells(27).Controls(0), TextBox)
		tbLineID.Text = (claimHeader.GetNumberOfClaimLines + 1)

		'// show claim lines?
		If (claimHeader.GetClaimTypeID > 0) And (claimHeader.GetVendorID > 0) And (claimHeader.GetRequestID > 0) Then ' both options are selected

			If (claimHeader.GetNumberOfClaimLines > 0) Then	 ' have any lines aready been created?
				ExistingClaimLineGV.Visible = True ' show existing line gridview
				dvClaimLineSearch.Visible = True ' show claim search field
			Else
				ExistingClaimLineGV.Visible = False	' hide existing lines
				dvClaimLineSearch.Visible = False ' hide claim search field
			End If

			' NewClaimLineGV visibility managed by 'ManageClaimControls' which shows/hides based on claim status

		Else ' hide claim line gridviews & search field
			NewClaimLineGV.Visible = False
			ExistingClaimLineGV.Visible = False
			dvClaimLineSearch.Visible = False
		End If

		Dim applyCIGlobal = False
		If claimHeader.GetCurrencyID() > 0 Then	' set global currency setting

			Dim thisCurrencyCode As String = dbConnector.ID2Field("currency_t", claimHeader.GetCurrencyID(), "currency_id", "currency_code")

			If InStr(thisCurrencyCode, "GBP") > 0 Then
				ciGlobal = ciGB
				applyCIGlobal = True

			ElseIf InStr(thisCurrencyCode, "USD") > 0 Then
				ciGlobal = ciUS
				applyCIGlobal = True

			ElseIf InStr(thisCurrencyCode, "EUR") > 0 Then
				ciGlobal = ciEU
				applyCIGlobal = True

			ElseIf InStr(thisCurrencyCode, "HKD") > 0 Then
				ciGlobal = ciHK
				applyCIGlobal = True

			ElseIf InStr(thisCurrencyCode, "SEK") > 0 Then
				ciGlobal = ciKR
				applyCIGlobal = True

			End If
		End If

		If applyCIGlobal Then
			tbOutstandingAmount.Text = CDbl(claimHeader.GetOutstandingAmount()).ToString("c", ciGlobal)	' apply currency to total
		Else
			tbOutstandingAmount.Text = CDbl(claimHeader.GetOutstandingAmount())
		End If

		hdnNumberOfClaimLines.Value = claimHeader.GetNumberOfClaimLines	' set number of claim lines

		If claimHeader.GetRequestID > 0 Then '// show hide claim header buttons - call jQuery triggers
			btnECLGVHeaderUpload.Visible = True	' show asset manager button
			' are downloads available?
			Dim assetsAvailable As Boolean = dbConnector.DoesDBRecordExist("select * from asset_manager_t where dir_id = " & claimHeader.GetRequestID & " and dir_label = 'claim-header'")
			If assetsAvailable Then
				btnECLGVHeaderUpload.ImageUrl = "images/icon-folder-full.png"
			Else
				btnECLGVHeaderUpload.ImageUrl = "images/icon-folder-empty.png"
			End If

			btnSaveAsXLSX.Visible = True ' show xls download button
			btnViewHistory.Visible = True ' show history button
		Else
			btnECLGVHeaderUpload.Visible = False ' hide asset manager button
			btnSaveAsXLSX.Visible = False ' hide xls download button
			btnViewHistory.Visible = False ' hide history button
		End If

		SetSystemMessage(claimHeader.GetSysMsg())

	End Sub

	'-------------------------------------------

	' Button actions

	'-------------------------------------------

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click '// save claim as it's current stage (header only)

		claimHeader.SetDBGetClaimHeader("update", "claim-form")	' update record

		If (claimHeader.GetClaimTypeID > 0) And (claimHeader.GetVendorID > 0) Then ' both claim header options are selected
			NewClaimLineGV.Visible = True ' show new claim line
		End If

		NewClaimLineGV.Attributes("data-request-id") = claimHeader.GetRequestID() '// update attributes to gridview
		NewClaimLineGV.Attributes("data-claim-type-id") = claimHeader.GetClaimTypeID()
		NewClaimLineGV.Attributes("data-vendor-id") = claimHeader.GetVendorID()

		ManageClaimControls(claimHeader.GetClaimStatusID) ' reset form config based on claim status

		claimHeader.SetDBHistory("Claim Update", "Blue", "Save")
		SetSystemMessage(claimHeader.GetSysMsg())

	End Sub

	Protected Sub btnProceed_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnProceed.Click '// move claim to next stage

		Dim claimStatus As Integer = claimHeader.GetClaimStatusID()
		claimHeader.SetSysMsg("") ' clear system message

		Select Case claimStatus

			Case 0 ' pre-draft (unsaved)
				claimHeader.SetClaimStatusID(10) ' set status to 'draft'
				claimHeader.SetDBGetClaimHeader("insert", "claim-form")	' insert record
				tbRequestID.Text = claimHeader.GetRequestID() ' update with AI ID

				NewClaimLineGV.Attributes("data-request-id") = claimHeader.GetRequestID() ' update attributes to gridview
				NewClaimLineGV.Attributes("data-claim-type-id") = claimHeader.GetClaimTypeID()
				NewClaimLineGV.Attributes("data-vendor-id") = claimHeader.GetVendorID()

				claimHeader.SetDBHistory("Claim Created", "Green", "User Action | Create | Claim ID " & claimHeader.GetRequestID())
				Response.Redirect("claim-form.aspx?id=" & claimHeader.GetRequestID()) ' show send to vendor dialogue -  progresses status once vendor contacted

			Case 10	' draft
				If (ValidateInOracle(claimHeader)) Then	' create XML version & produce message on valdation queue

					If claimHeader.GetTransmissionRequired = "1" Then
						claimHeader.SetClaimStatusID(20) ' set status to 'awaiting approval'
					Else
						claimHeader.SetClaimStatusID(25) ' set status to 'pre approved'
						ExportToExcel("system-action") ' save claim as XLS
					End If
					claimHeader.SetDBOval(True)	' flag that claim is pending Oracle validation result
					claimHeader.SetSysMsg("Info|||Claim request sent for Oracle validation")

				End If
				claimHeader.SetDBGetClaimHeader("update", "claim-form")	' update claim header record

			Case 20	' awaiting approval

				Response.Redirect("email-manager.aspx?action=send-claim-to-vendor&id=" & claimHeader.GetRequestID()) ' show send to vendor dialogue -  progresses status once vendor contacted

			Case 25	' pre approved || submit to Oracle (transission de-selected)

				If (SubmitToOracle(claimHeader)) Then ' create XML version & produce message on submission queue
					claimHeader.SetClaimStatusID(70) ' set status to 'sent to Oracle 
					claimHeader.SetDBClaimStatus(70) ' update status
				End If

			Case 30	' approved
				If (SubmitToOracle(claimHeader)) Then ' create XML version & produce message on submission queue
					claimHeader.SetClaimStatusID(70) ' set status to 'sent to Oracle'
					claimHeader.SetDBClaimStatus(70) ' update status
				End If

			Case 40	' rejected
				If (ValidateInOracle(claimHeader)) Then	' create XML version & produce message on valdation queue
					claimHeader.SetClaimStatusID(20) ' set status to 'awaiting approval'
					claimHeader.SetDBOval(True)	' flag that claim is pending Oracle validation result
					claimHeader.SetSysMsg("Info|||Claim request sent for Oracle validation")
				End If
				claimHeader.SetDBGetClaimHeader("update", "claim-form")	' update claim header record

			Case 50	' cancelled
				'// no action

			Case 60	' with vendor
				claimHeader.SetClaimStatusID(30) ' set status to 'approved'
				claimHeader.SetDBClaimStatus(30) ' update status
				claimHeader.SetSysMsg("")

			Case 70	' with oracle
				'// no action

			Case 80	' oracle error
				'// no action

			Case 90	' with ap dept
				'// no action

			Case 100 ' paid
				'// no action

		End Select

		ManageClaimControls(claimHeader.GetClaimStatusID) ' reset form config based on claim status

		ExistingClaimLineGV.EditIndex = -1 ' exit gridview edit index
		ExistingClaimLineGV.SelectedIndex = -1 ' cancel gridview selection
		claimLine.GVBindData(ExistingClaimLineGV, claimHeader) ' rebind gridview

		claimHeader.SetDBHistory("Claim Progression", "Green", "Status Change : " & dbConnector.ID2Field("claim_status_t", claimHeader.GetClaimStatusID, "claim_status_id", "claim_status") & " (Status ID : " & claimHeader.GetClaimStatusID & ")")

	End Sub

	Protected Sub btnRollBack_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRollBack.Click '// move claim back a stage / potentially delete, cancel

		Dim claimStatus As Integer = claimHeader.GetClaimStatusID()

		Select Case claimStatus

			Case 0 ' pre-draft (unsaved)
				'// no action

			Case 10	' draft
				' get claim data
				claimHeader.SetClaimHeaderGetDB(claimHeader.GetRequestID)

				' delete lines
				mySqlConnection = New MySqlConnection(mySqlConnectionString)
				mySqlCommand.CommandText = "delete from credit_request_line where request_id=" & claimHeader.GetRequestID
				mySqlCommand.Connection = mySqlConnection

				mySqlConnection.Open()
				mySqlCommand.ExecuteNonQuery()
				mySqlConnection.Close()

				'	delete header
				mySqlConnection = New MySqlConnection(mySqlConnectionString)
				mySqlCommand.CommandText = "delete from credit_request_header where request_id=" & claimHeader.GetRequestID
				mySqlCommand.Connection = mySqlConnection

				mySqlConnection.Open()
				mySqlCommand.ExecuteNonQuery()
				mySqlConnection.Close()

				' archive files
				'///TODO

				' record history
				Dim XMLClaim As XDocument = New XDocument	' create XML version of the claim
				XMLClaim = ClaimAsXML(claimHeader)
				claimHeader.SetDBHistory("Claim Deleted", "Blue", "Deleted via claim form", XMLClaim.ToString()) ' update history (including XML claim)

				Response.Redirect("claim-listing.aspx")	' redirect to claim listing


			Case 20	' awaiting approval
				claimHeader.SetClaimStatusID(10) ' set status to 'draft'
				claimHeader.SetDBClaimStatus(10) ' update status

				If (claimHeader.GetClaimTypeID > 0) And (claimHeader.GetVendorID > 0) Then ' both claim header options are selected
					NewClaimLineGV.Visible = True ' show new claim line
				End If


			Case 25	' pre approved
				claimHeader.SetClaimStatusID(10) ' set status to 'draft'
				claimHeader.SetDBClaimStatus(10) ' update status

				If (claimHeader.GetClaimTypeID > 0) And (claimHeader.GetVendorID > 0) Then ' both claim header options are selected
					NewClaimLineGV.Visible = True ' show new claim line
				End If

			Case 30	' approved
				'// no action

			Case 40	' rejected
				claimHeader.SetClaimStatusID(50) ' set status to 'cancelled'
				claimHeader.SetDBClaimStatus(50) ' update status

			Case 50	' cancelled
				'// no action

			Case 60	' with vendor
				claimHeader.SetClaimStatusID(40) ' set status to 'rejected'
				claimHeader.SetDBClaimStatus(40) ' update status
				claimHeader.SetSysMsg("")

			Case 70	' with oracle
				'// no action

			Case 80	' oracle error
				'// no action

			Case 90	' with ap dept
				'// no action

			Case 100 ' paid
				'// no action

		End Select

		ManageClaimControls(claimHeader.GetClaimStatusID) ' reset form config based on claim status

		ExistingClaimLineGV.EditIndex = -1 ' exit edit index
		ExistingClaimLineGV.SelectedIndex = -1 ' cancel selection
		claimLine.GVBindData(ExistingClaimLineGV, claimHeader) ' redraw grid

		claimHeader.SetDBHistory("Claim RollBack", "Green", "Status Change : " & dbConnector.ID2Field("claim_status_t", claimHeader.GetClaimStatusID, "claim_status_id", "claim_status") & " (Status ID : " & claimHeader.GetClaimStatusID & ")")

	End Sub

	Protected Sub btnSaveAsXLSX_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSaveAsXLSX.Click

		SetClaimHeaderGetForm()	' get current claim header data from form
		ExportToExcel("user-action")

	End Sub

	Protected Sub btnClaimLineGV_Search_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClaimLineGV_Search.Click	'// update gridview with search results / filtered data contact data

		ExistingClaimLineGV.Attributes("data-search-term") = txClaimLineGV_Search.Text ' write search term to gridview attributes		'//
		claimLine.GVBindData(ExistingClaimLineGV, claimHeader) ' rebind gridview & assign filtered contact data

	End Sub

	'-------------------------------------------

	' Claim controls managers

	'-------------------------------------------

	Protected Sub ManageClaimControls(ByVal claimStatus As Integer)
		'// sets labels / visibilty of ket claim buttons
		'// set the appropriate next status ID
		'// set key header fields as read only **TODO
		'// show | hide new line generation
		'// clears gridview edit / select options

		lblControlMessage.Visible = False

		Select Case claimStatus

			Case 0 ' create
				btnSave.Visible = False
				btnProceed.Visible = True
				btnRollBack.Visible = False
				btnSave.Text = ""
				btnProceed.Text = "Save"
				btnRollBack.Text = ""

			Case 10	' draft
				btnSave.Visible = True
				btnProceed.Visible = True
				btnRollBack.Visible = True
				btnSave.Text = "Save"
				If claimHeader.GetTransmissionRequired = "1" Then
					btnProceed.Text = "Ready For Approval"
				Else
					btnProceed.Text = "Ready For Submission"
				End If

				btnRollBack.Text = "Delete"
				btnRollBack.OnClientClick = "if (!window.confirm('Are you sure you want to delete this item?')) return false;"

				If (claimHeader.GetClaimTypeID > 0) And (claimHeader.GetVendorID > 0) Then	' both claim header options are selected
					NewClaimLineGV.Visible = True ' show new claim line
				End If

			Case 20	' awaiting approval

				If (claimHeader.GetPendingOval <> True) Then  ' don't proceed unless Oracle has successfully validatd the claim data

					btnSave.Visible = False
					btnProceed.Visible = True
					btnRollBack.Visible = True
					btnSave.Text = ""
					btnProceed.Text = "Send To Vendor"
					btnRollBack.Text = "Return to Draft"
					btnRollBack.OnClientClick = Nothing

				Else

					btnSave.Visible = False
					btnProceed.Visible = False
					btnRollBack.Visible = True
					btnSave.Text = ""
					btnProceed.Text = ""
					btnRollBack.Text = "Return to Draft"
					btnRollBack.OnClientClick = Nothing

					lblControlMessage.Visible = True ' show control message
					lblControlMessage.Text = "Pending Oracle Validation Result <span class='reload-page'>[ <a href='#' class='reload-page'>refresh</a> ]</span>"

				End If

			Case 25	' pre approved

				If (claimHeader.GetPendingOval <> True) Then  ' don't proceed unless Oracle has successfully validatd the claim data

					btnSave.Visible = False
					btnProceed.Visible = True
					btnRollBack.Visible = True
					btnSave.Text = ""
					btnProceed.Text = "Submit to Oracle"
					btnRollBack.Text = "Return to Draft"
					btnRollBack.OnClientClick = Nothing

				Else

					btnSave.Visible = False
					btnProceed.Visible = False
					btnRollBack.Visible = True
					btnSave.Text = ""
					btnProceed.Text = ""
					btnRollBack.Text = "Return to Draft"
					btnRollBack.OnClientClick = Nothing

					lblControlMessage.Visible = True ' show control message
					lblControlMessage.Text = "Pending Oracle Validation Result <span class='reload-page'>[ <a href='#' class='reload-page'>refresh</a> ]</span>"

				End If

			Case 30	' approved
				btnSave.Visible = False
				btnProceed.Visible = True
				btnRollBack.Visible = False
				btnSave.Text = ""
				btnProceed.Text = "Submit to Oracle"
				btnRollBack.Text = ""

				claimHeader.SetSysMsg("")

			Case 40	' rejected
				btnSave.Visible = True
				btnProceed.Visible = True
				btnRollBack.Visible = True
				btnSave.Text = "Save"
				If claimHeader.GetTransmissionRequired = "1" Then
					btnProceed.Text = "Ready For Approval"
				Else
					btnProceed.Text = "Ready For Submission"
				End If
				btnRollBack.Text = "Cancel Claim"

				claimHeader.SetSysMsg("")

			Case 50	' cancelled
				btnSave.Visible = False
				btnProceed.Visible = False
				btnRollBack.Visible = False
				btnSave.Text = ""
				btnProceed.Text = ""
				btnRollBack.Text = ""

			Case 60	' with vendor
				btnSave.Visible = False
				btnProceed.Visible = True
				btnRollBack.Visible = True
				btnSave.Text = ""
				btnProceed.Text = "Approved"
				btnRollBack.Text = "Rejected"

			Case 70	' with oracle
				btnSave.Visible = False
				btnProceed.Visible = False
				btnRollBack.Visible = False
				btnSave.Text = ""
				btnProceed.Text = ""
				btnRollBack.Text = ""

				lblControlMessage.Visible = True ' show control message
				lblControlMessage.Text = "Pending Oracle Submission Result <span class='reload-page'>[ <a href='#' class='reload-page'>refresh</a> ]</span>"

			Case 80	' oracle error
				btnSave.Visible = False
				btnProceed.Visible = False
				btnRollBack.Visible = False
				btnSave.Text = ""
				btnProceed.Text = ""
				btnRollBack.Text = ""

			Case 90	' with AP
				btnSave.Visible = False
				btnProceed.Visible = False
				btnRollBack.Visible = False
				btnSave.Text = ""
				btnProceed.Text = ""
				btnRollBack.Text = ""

				lblControlMessage.Visible = True ' show control message
				lblControlMessage.Text = "Service Request No. " & claimHeader.GetServiceRequestNo & " | Debit Note Request ID " & claimHeader.GetDebitNoteRequestID

			Case 100 ' dn created
				btnSave.Visible = False
				btnProceed.Visible = False
				btnRollBack.Visible = False
				btnSave.Text = ""
				btnProceed.Text = ""
				btnRollBack.Text = ""

				lblControlMessage.Visible = True ' show control message
				lblControlMessage.Text = "Service Request No. " & claimHeader.GetServiceRequestNo & " | Debit Note Number " & claimHeader.GetDebitNoteNumber

		End Select

		ManageValidatedControls() ' set controls as to be validated
		ManageReadOnlyControls(claimHeader.GetClaimStatusID) ' set controls as editable / readonly
		ManageClaimTypeControls("refresh") ' refresh claim type controls
		ManageVendorControls("refresh")	' refresh vendor controls

		If (claimHeader.GetClaimStatusID = 10 Or claimHeader.GetClaimStatusID = 40) Then
			NewClaimLineGV.Visible = True ' show new claim line (DRAFT or REJECTED claim)
		Else
			NewClaimLineGV.Visible = False ' hide new claim line
		End If

		hdnClaimStatus.Value = claimHeader.GetClaimStatusID

		' set status text
		Dim claimStatusText As String = dbConnector.ID2Field("claim_status_t", claimHeader.GetClaimStatusID, "claim_status_id", "claim_status")	' update ui control value - status
		If (claimHeader.GetPendingOval = True) And (claimHeader.GetClaimStatusID = 20 Or claimHeader.GetClaimStatusID = 25 Or claimHeader.GetClaimStatusID = 80) Then
			tbStatus.Text = claimStatusText & " (Pending)"
		Else
			tbStatus.Text = claimStatusText
		End If

	End Sub

	Public Sub ManageClaimTypeControls(ByVal thisCMD As String)	'// called by page & claim type events - show relevant controls or hides & reset controls

		If thisCMD = "hide-all" Then

			ddReasonID.CssClass = "hidden" ' hide fields
			lblReasonID.CssClass = "hidden"
			ddPromotionID.CssClass = "hidden"
			lblPromotionID.CssClass = "hidden"
			'ddInformFinanceID.CssClass = "hidden"
			'lblInformFinanceID.CssClass = "hidden"
			cbTransmissionRequired.CssClass = "hidden"
			lblTransmissionRequired.CssClass = "hidden"

			lblPriceProtectionNo.CssClass = "hidden"
			lblOriginalDnNo.CssClass = "hidden"
			lblReversingDnNo.CssClass = "hidden"
			tbPriceProtectionNo.CssClass = "hidden"
			tbOriginalDnNo.CssClass = "hidden"
			tbReversingDnNo.CssClass = "hidden"

			claimHeader.SetReasonID(0) ' reset values
			claimHeader.SetPromotionID(0)
			'claimHeader.SetInformFinanceID(0)
			claimHeader.SetDNTypeID(0)

		End If

		If thisCMD = "show-all" Then

			ddReasonID.CssClass = (ddReasonID.CssClass).Replace("hidden", "") ' claim reason
			lblReasonID.CssClass = (lblReasonID.CssClass).Replace("hidden", "")
			BindData_ReasonID(claimHeader.GetClaimTypeID(), claimHeader.GetReasonID())

			'// defaults
			'ddInformFinanceID.CssClass = "hidden"
			'lblInformFinanceID.CssClass = "hidden"
			'claimHeader.SetInformFinanceID(0) ' reset finance value

			Select Case claimHeader.GetClaimTypeID()

				Case 1 ' stock

					lblPriceProtectionNo.CssClass = (lblPriceProtectionNo.CssClass).Replace("hidden", "")
					tbPriceProtectionNo.CssClass = (tbPriceProtectionNo.CssClass).Replace("hidden", "")

					'ddInformFinanceID.CssClass = (ddInformFinanceID.CssClass).Replace("hidden", "")
					'lblInformFinanceID.CssClass = (lblInformFinanceID.CssClass).Replace("hidden", "")
					'BindData_FinanceID()

					ddPromotionID.CssClass = "hidden"
					lblPromotionID.CssClass = "hidden"
					claimHeader.SetPromotionID(0) ' reset promotion value

					lblOriginalDnNo.CssClass = "hidden"
					tbOriginalDnNo.CssClass = "hidden"

					lblReversingDnNo.CssClass = "hidden"
					tbReversingDnNo.CssClass = "hidden"

				Case 2 ' sales

					lblPriceProtectionNo.CssClass = (lblPriceProtectionNo.CssClass).Replace("hidden", "")
					tbPriceProtectionNo.CssClass = (tbPriceProtectionNo.CssClass).Replace("hidden", "")

					'ddInformFinanceID.CssClass = "hidden"
					'lblInformFinanceID.CssClass = "hidden"
					'claimHeader.SetInformFinanceID(0) ' reset finance value#

					ddPromotionID.CssClass = "hidden"
					lblPromotionID.CssClass = "hidden"
					claimHeader.SetPromotionID(0) ' reset promotion value

					lblOriginalDnNo.CssClass = "hidden"
					tbOriginalDnNo.CssClass = "hidden"

					lblReversingDnNo.CssClass = "hidden"
					tbReversingDnNo.CssClass = "hidden"

				Case 3 ' marketing

					lblPriceProtectionNo.CssClass = "hidden"
					tbPriceProtectionNo.CssClass = "hidden"

					'ddInformFinanceID.CssClass = "hidden"
					'lblInformFinanceID.CssClass = "hidden"
					'claimHeader.SetInformFinanceID(0) ' reset finance value

					If Not IsNumeric(claimHeader.GetPromotionID()) Then
						claimHeader.SetPromotionID(0)
					End If
					ddPromotionID.CssClass = (ddPromotionID.CssClass).Replace("hidden", "")
					lblPromotionID.CssClass = (lblPromotionID.CssClass).Replace("hidden", "")
					BindData_PromotionID(claimHeader.GetPromotionID) ' bind data to promotion dd

					lblOriginalDnNo.CssClass = "hidden"
					tbOriginalDnNo.CssClass = "hidden"

					lblReversingDnNo.CssClass = "hidden"
					tbReversingDnNo.CssClass = "hidden"

				Case 4 ' non marketing

					lblPriceProtectionNo.CssClass = "hidden"
					tbPriceProtectionNo.CssClass = "hidden"

					'ddInformFinanceID.CssClass = "hidden"
					'lblInformFinanceID.CssClass = "hidden"
					'claimHeader.SetInformFinanceID(0) ' reset finance value

					ddPromotionID.CssClass = "hidden"
					lblPromotionID.CssClass = "hidden"
					claimHeader.SetPromotionID(0) ' reset promotion value

					lblOriginalDnNo.CssClass = "hidden"
					tbOriginalDnNo.CssClass = "hidden"

					lblReversingDnNo.CssClass = "hidden"
					tbReversingDnNo.CssClass = "hidden"

				Case 5 ' rev stock

					lblPriceProtectionNo.CssClass = (lblPriceProtectionNo.CssClass).Replace("hidden", "")
					tbPriceProtectionNo.CssClass = (tbPriceProtectionNo.CssClass).Replace("hidden", "")

					'ddInformFinanceID.CssClass = (ddInformFinanceID.CssClass).Replace("hidden", "")
					'lblInformFinanceID.CssClass = (lblInformFinanceID.CssClass).Replace("hidden", "")
					'BindData_FinanceID()

					ddPromotionID.CssClass = "hidden"
					lblPromotionID.CssClass = "hidden"
					claimHeader.SetPromotionID(0) ' reset promotion value

					lblOriginalDnNo.CssClass = (lblOriginalDnNo.CssClass).Replace("hidden", "")
					tbOriginalDnNo.CssClass = (tbOriginalDnNo.CssClass).Replace("hidden", "")

					lblReversingDnNo.CssClass = (lblReversingDnNo.CssClass).Replace("hidden", "")
					tbReversingDnNo.CssClass = (tbReversingDnNo.CssClass).Replace("hidden", "")

				Case 6 ' rev sales

					lblPriceProtectionNo.CssClass = (lblPriceProtectionNo.CssClass).Replace("hidden", "")
					tbPriceProtectionNo.CssClass = (tbPriceProtectionNo.CssClass).Replace("hidden", "")

					'ddInformFinanceID.CssClass = "hidden"
					'lblInformFinanceID.CssClass = "hidden"
					'claimHeader.SetInformFinanceID(0) ' reset finance value

					ddPromotionID.CssClass = "hidden"
					lblPromotionID.CssClass = "hidden"
					claimHeader.SetPromotionID(0) ' reset promotion value

					lblOriginalDnNo.CssClass = (lblOriginalDnNo.CssClass).Replace("hidden", "")
					tbOriginalDnNo.CssClass = (tbOriginalDnNo.CssClass).Replace("hidden", "")

					lblReversingDnNo.CssClass = (lblReversingDnNo.CssClass).Replace("hidden", "")
					tbReversingDnNo.CssClass = (tbReversingDnNo.CssClass).Replace("hidden", "")

				Case 7 ' rev marketing

					lblPriceProtectionNo.CssClass = "hidden"
					tbPriceProtectionNo.CssClass = "hidden"

					'ddInformFinanceID.CssClass = "hidden"
					'lblInformFinanceID.CssClass = "hidden"
					'claimHeader.SetInformFinanceID(0) ' reset finance value

					If Not IsNumeric(claimHeader.GetPromotionID()) Then
						claimHeader.SetPromotionID(0)
					End If
					ddPromotionID.CssClass = (ddPromotionID.CssClass).Replace("hidden", "")
					lblPromotionID.CssClass = (lblPromotionID.CssClass).Replace("hidden", "")
					BindData_PromotionID(claimHeader.GetPromotionID) ' bind data to promotion dd

					lblOriginalDnNo.CssClass = (lblOriginalDnNo.CssClass).Replace("hidden", "")
					tbOriginalDnNo.CssClass = (tbOriginalDnNo.CssClass).Replace("hidden", "")

					lblReversingDnNo.CssClass = (lblReversingDnNo.CssClass).Replace("hidden", "")
					tbReversingDnNo.CssClass = (tbReversingDnNo.CssClass).Replace("hidden", "")

				Case 8 ' rev non marketing

					lblPriceProtectionNo.CssClass = "hidden"
					tbPriceProtectionNo.CssClass = "hidden"

					'ddInformFinanceID.CssClass = "hidden"
					'lblInformFinanceID.CssClass = "hidden"
					'claimHeader.SetInformFinanceID(0) ' reset finance value

					ddPromotionID.CssClass = "hidden"
					lblPromotionID.CssClass = "hidden"
					claimHeader.SetPromotionID(0) ' reset promotion value

					lblOriginalDnNo.CssClass = (lblOriginalDnNo.CssClass).Replace("hidden", "")
					tbOriginalDnNo.CssClass = (tbOriginalDnNo.CssClass).Replace("hidden", "")

					lblReversingDnNo.CssClass = (lblReversingDnNo.CssClass).Replace("hidden", "")
					tbReversingDnNo.CssClass = (tbReversingDnNo.CssClass).Replace("hidden", "")

			End Select

			cbTransmissionRequired.CssClass = (cbTransmissionRequired.CssClass).Replace("hidden", "") ' transmission required
			lblTransmissionRequired.CssClass = (lblTransmissionRequired.CssClass).Replace("hidden", "")

			claimHeader.SetDNTypeID(dbConnector.ID2Field("claim_type_t", claimHeader.GetClaimTypeID(), "claim_type_id", "dn_type_id")) ' set control values

		End If

		If thisCMD = "refresh" Then

			ddReasonID.SelectedValue = claimHeader.GetReasonID ' reset control values
			ddPromotionID.SelectedValue = claimHeader.GetPromotionID
			'ddInformFinanceID.SelectedValue = claimHeader.GetInformFinanceID
			hdnDNTypeID.Value = claimHeader.GetDNTypeID

		Else

			ddReasonID.SelectedValue = claimHeader.GetReasonID ' reset control values
			ddPromotionID.SelectedValue = claimHeader.GetPromotionID
			'ddInformFinanceID.SelectedValue = claimHeader.GetInformFinanceID
			hdnDNTypeID.Value = claimHeader.GetDNTypeID

			lblClaimTypeID.Attributes.Add("class", "label")	' flag labels
			lblReasonID.Attributes.Add("class", "label")
			lblPromotionID.Attributes.Add("class", "label")
			'lblInformFinanceID.Attributes.Add("class", "label")
			lblTransmissionRequired.Attributes.Add("class", "label")
			lblAdditionalRecipients.Attributes.Add("class", "label")

			lblPriceProtectionNo.Attributes.Add("class", "label")
			lblOriginalDnNo.Attributes.Add("class", "label")
			lblReversingDnNo.Attributes.Add("class", "label")

		End If

	End Sub

	Public Sub ManageVendorControls(ByVal thisCMD As String) '// called by page & vendor events

		If thisCMD = "hide-all" Then

			ddDivisionID.CssClass = "hidden ' hide fields"
			lblDivisionID.CssClass = "hidden"
			tbVendorAddress.CssClass = "hidden"
			lblVendorAddress.CssClass = "hidden"
			tbCurrency.CssClass = "hidden"
			lblCurrency.CssClass = "hidden"
			tbPrimaryContact.CssClass = "hidden"
			lblPrimaryContact.CssClass = "hidden"
			tbAccountContact.CssClass = "hidden"
			lblAccountContact.CssClass = "hidden"
			cblAdditionalRecipients.CssClass = "hidden"
			lblAdditionalRecipients.CssClass = "hidden"
			dvAdditionalRecipientsWrapper.Attributes.Add("class", "hidden")

			claimHeader.SetDivisionID(0) ' reset values
			claimHeader.SetLocationID(0)
			claimHeader.SetCurrencyID(0)
			claimHeader.SetBusinessUnitID(0)
			claimHeader.SetPrimaryContactID(0)
			claimHeader.SetAccountContactID(0)
			claimHeader.SetAdditionalRecipients(0)

			hdnLocationID.Value = "" ' reset system control values
			hdnCurrencyID.Value = ""
			hdnBusinessUnitID.Value = ""
			hdnPrimaryContactID.Value = ""
			hdnAccountContactID.Value = ""

			tbVendorAddress.Text = "" ' reset ui control values
			tbCurrency.Text = ""
			tbPrimaryContact.Text = ""
			tbAccountContact.Text = ""

		End If

		If thisCMD = "show-divisions" Then ' called by page, vendor & division events (vendor selected)
			ddDivisionID.CssClass = (ddDivisionID.CssClass).Replace("hidden", "")
			lblDivisionID.CssClass = (lblDivisionID.CssClass).Replace("hidden", "")
			BindData_DivisionID(claimHeader.GetVendorID(), claimHeader.GetDivisionID())	' call bind division
			ddDivisionID.SelectedValue = claimHeader.GetDivisionID() ' set control values

		End If

		If thisCMD = "show-details" Then

			tbVendorAddress.CssClass = (tbVendorAddress.CssClass).Replace("hidden", "")	' show fields
			lblVendorAddress.CssClass = (lblVendorAddress.CssClass).Replace("hidden", "")
			tbCurrency.CssClass = (tbCurrency.CssClass).Replace("hidden", "")
			lblCurrency.CssClass = (lblCurrency.CssClass).Replace("hidden", "")
			tbPrimaryContact.CssClass = (tbPrimaryContact.CssClass).Replace("hidden", "")
			lblPrimaryContact.CssClass = (lblPrimaryContact.CssClass).Replace("hidden", "")
			tbAccountContact.CssClass = (tbAccountContact.CssClass).Replace("hidden", "")
			lblAccountContact.CssClass = (lblAccountContact.CssClass).Replace("hidden", "")
			cblAdditionalRecipients.CssClass = (cblAdditionalRecipients.CssClass).Replace("hidden", "")
			lblAdditionalRecipients.CssClass = (lblAdditionalRecipients.CssClass).Replace("hidden", "")
			If dvAdditionalRecipientsWrapper.Attributes("class") <> "" Then
				dvAdditionalRecipientsWrapper.Attributes("class") = dvAdditionalRecipientsWrapper.Attributes("class").Replace("hidden", "")
			End If

			claimHeader.SetDivisionRelatedData(claimHeader.GetDivisionID) ' get data relating to division & assign to header object

			hdnLocationID.Value = claimHeader.GetLocationID	' set system control values
			hdnBusinessUnitID.Value = claimHeader.GetBusinessUnitID
			hdnCurrencyID.Value = claimHeader.GetCurrencyID
			hdnPrimaryContactID.Value = claimHeader.GetPrimaryContactID
			hdnAccountContactID.Value = claimHeader.GetAccountContactID

			tbVendorAddress.Text = dbConnector.ID2Field("vendor_address_t", claimHeader.GetLocationID, "location_id", "vendor_site_code") ' set control values
			tbCurrency.Text = dbConnector.ID2Field("currency_t", claimHeader.GetCurrencyID, "currency_id", "currency_name")
			tbPrimaryContact.Text = dbConnector.ID2Field("contact_t", claimHeader.GetPrimaryContactID, "contact_id", "contact_name")
			tbAccountContact.Text = dbConnector.ID2Field("contact_t", claimHeader.GetAccountContactID, "contact_id", "contact_name")

			BindData_AdditionalRecipients(claimHeader.GetVendorID(), claimHeader.GetPrimaryContactID, claimHeader.GetAccountContactID) ' get additional recipients - call bind data sub & filter out primary & existing contacts

		ElseIf thisCMD = "hide-details" Then

			tbVendorAddress.CssClass = "hidden"	' hide fields
			lblVendorAddress.CssClass = "hidden"
			tbCurrency.CssClass = "hidden"
			lblCurrency.CssClass = "hidden"
			tbPrimaryContact.CssClass = "hidden"
			lblPrimaryContact.CssClass = "hidden"
			tbAccountContact.CssClass = "hidden"
			lblAccountContact.CssClass = "hidden"
			cblAdditionalRecipients.CssClass = "hidden"
			lblAdditionalRecipients.CssClass = "hidden"
			dvAdditionalRecipientsWrapper.Attributes.Add("class", "hidden")

			claimHeader.SetLocationID(0) ' reset class values
			claimHeader.SetBusinessUnitID(0)
			claimHeader.SetCurrencyID(0)
			claimHeader.SetPrimaryContactID(0)
			claimHeader.SetAccountContactID(0)
			claimHeader.SetAdditionalRecipients(0)

			hdnLocationID.Value = "" ' reset system control values
			hdnBusinessUnitID.Value = ""
			hdnCurrencyID.Value = ""
			hdnPrimaryContactID.Value = ""
			hdnAccountContactID.Value = ""

			tbVendorAddress.Text = "" ' reset control values
			tbCurrency.Text = ""
			tbPrimaryContact.Text = ""
			tbAccountContact.Text = ""

		End If

		If thisCMD = "refresh" Then

		Else

			lblVendorID.Attributes.Add("class", "label") ' flag labels
			lblDivisionID.Attributes.Add("class", "label")
			lblVendorAddress.Attributes.Add("class", "label")
			lblCurrency.Attributes.Add("class", "label")
			lblPrimaryContact.Attributes.Add("class", "label")
			lblAccountContact.Attributes.Add("class", "label")

		End If

	End Sub

	Public Sub ManageReadOnlyControls(ByVal claimStatus As Integer)	'// set readonly controls

		tbRequestID.ReadOnly = True
		tbVendorAddress.ReadOnly = True
		tbCurrency.ReadOnly = True
		tbPrimaryContact.ReadOnly = True
		tbAccountContact.ReadOnly = True
		tbOutstandingAmount.ReadOnly = True

		If (claimStatus = 0 Or claimStatus = 10 Or claimStatus = 40) Then  ' DRAFT or REJECTED status
			ddClaimTypeID.Attributes.Remove("disabled")
			ddReasonID.Attributes.Remove("disabled")
			ddPromotionID.Attributes.Remove("disabled")
			ddVendorID.Attributes.Remove("disabled")
			ddDivisionID.Attributes.Remove("disabled")

			cbTransmissionRequired.Attributes.Remove("readonly")
			tbVendorReference.Attributes.Remove("readonly")
			tbDateRaised.Attributes.Remove("readonly")

			tbPriceProtectionNo.Attributes.Remove("readonly")
			tbOriginalDnNo.Attributes.Remove("readonly")
			tbReversingDnNo.Attributes.Remove("readonly")

		Else

			ddClaimTypeID.Attributes.Add("disabled", "disabled")
			ddReasonID.Attributes.Add("disabled", "disabled")
			ddPromotionID.Attributes.Add("disabled", "disabled")
			ddVendorID.Attributes.Add("disabled", "disabled")
			ddDivisionID.Attributes.Add("disabled", "disabled")

			cbTransmissionRequired.Attributes.Add("readonly", "readonly")
			tbVendorReference.Attributes.Add("readonly", "readonly")
			tbDateRaised.Attributes.Add("readonly", "readonly")

			tbPriceProtectionNo.Attributes.Add("readonly", "readonly")
			tbOriginalDnNo.Attributes.Add("readonly", "readonly")
			tbReversingDnNo.Attributes.Add("readonly", "readonly")

		End If

	End Sub

	Public Sub ManageValidatedControls() '//  set validation classes

		Select Case claimHeader.GetClaimTypeID

			Case 1, 2	' Stock Based Claim / Sales Based Claim
				tbPriceProtectionNo.Attributes.Add("class", "validate manditory")

			Case 5, 6	' Reversals /  Stock Based Claim / Sales Based Claim
				tbPriceProtectionNo.Attributes.Add("class", "validate manditory")
				tbOriginalDnNo.Attributes.Add("class", "validate manditory")
				'tbReversingDnNo.Attributes.Add("class", "validate manditory")

			Case 7, 8	'Reversals
				tbOriginalDnNo.Attributes.Add("class", "validate manditory")
				'tbReversingDnNo.Attributes.Add("class", "validate manditory")

		End Select

		ddOwningUserID.Attributes.Add("class", "validate manditory")
		ddClaimTypeID.Attributes.Add("class", "validate manditory")
		ddReasonID.Attributes.Add("class", "validate manditory")
		ddVendorID.Attributes.Add("class", "validate manditory")
		ddDivisionID.Attributes.Add("class", "validate manditory")
		ddPromotionID.Attributes.Add("class", "validate manditory")

		tbVendorReference.Attributes.Add("class", "validate manditory")
		'tbRequestDescription.Attributes.Add("class", "validate manditory")

		tbDateRaised.CssClass = "datepicker validate date"

	End Sub

	'-------------------------------------------

	' Claim line cut and paste trigger

	'-------------------------------------------

	Protected Sub tbClipboard_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tbClipboard.TextChanged
		'// hidden textarea changed - event called via jquery paste a claim line
		'// values should have been posted from excel spreadsheet (tab delimited - possible multiline)

		Dim clipboardValue As String = tbClipboard.Text

		If tbClipboard.Text <> "" Then

			claimLine.SetRequestID(NewClaimLineGV.Attributes("data-request-id")) '// get claim values/attributes not supplied by user's paste values
			claimLine.SetBlankRow(0)
			claimLine.SetProcessed(0)
			claimLine.SetApproved(0)

			'claimLine.SetDateRaised(utils.ConvertToSQLDate(Now))
			Dim tbLineID As TextBox = TryCast(NewClaimLineGV.Rows(NewClaimLineGV.EditIndex).Cells(1).Controls(0), TextBox)
			Dim thisLineID As Integer = tbLineID.Text

			'// get db keys to be used (gridview affected by template)
			Dim claimlineKeyPSV As String = claimLine.GetClaimLineGVTemplatePSV(NewClaimLineGV)	' get fields used in gridview
			Dim systemFieldPSV As String = claimLine.SetFieldPSV("system-fields") ' get system fields

			Dim systemKeyArray As Array = Split(utils.PSV2CSV(systemFieldPSV), ",")
			For i = 0 To UBound(systemKeyArray)
				claimlineKeyPSV = claimlineKeyPSV.Replace("|" & systemKeyArray(i) & "|", "") ' remove system fields from PSV
			Next
			claimlineKeyPSV = claimlineKeyPSV.Replace("|line_claim_reference|", "")	' remove line ref from PSV (considered system value & added later)

			Dim claimlineKeyArray As Array = Split(utils.PSV2CSV(claimlineKeyPSV), ",")
			Dim multiline As Boolean = False

			If InStr(clipboardValue, "~~~") > 0 Then ' cut & paste claim line - multi line pasted

				multiline = True

				Dim claimLinesArray As Array
				claimLinesArray = Split(clipboardValue, "~~~")

				For i = 0 To UBound(claimLinesArray)

					claimLine.SetLineID(thisLineID + i)	' update claim line values
					claimLine.SetLineClaimReference((thisLineID + i) & ".0")

					Dim claimLineArray As Array	' create claim line array
					If InStr(claimLinesArray(i), "|") > 0 Then
						claimLineArray = Split(claimLinesArray(i), "|")
						claimLine.SetDBGetClaimLineKVArrays("insert", claimlineKeyArray, claimLineArray) ' call function to write new line to database
					End If

				Next

			Else ' cut & paste claim line - single line pasted

				claimLine.SetLineID(thisLineID)	' set line reference
				claimLine.SetLineClaimReference(thisLineID & ".0")

				Dim claimLineArray As Array	' create line array
				claimLineArray = Split(utils.PSV2CSV(clipboardValue), ",")
				claimLine.SetDBGetClaimLineKVArrays("insert", claimlineKeyArray, claimLineArray) ' call function to write new line to database

			End If

			tbClipboard.Text = "" ' clear ui clipboard field

			claimLine.GVBindData(ExistingClaimLineGV, claimHeader) '// update gridview data (new & existing claim lines)
			claimLine.GVBindData(NewClaimLineGV, claimHeader)

		End If

	End Sub

	'-------------------------------------------

	' Claim header events

	'-------------------------------------------

	Protected Sub ddClaimTypeID_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddClaimTypeID.SelectedIndexChanged '// claim reason option selected, update related controls

		If ddClaimTypeID.SelectedItem.Value = "0" Then ' claim type is unselected
			claimHeader.SetClaimTypeID(0) ' reset claim type value
			ManageClaimTypeControls("hide-all")	' hide/reset claim type controls
		Else
			claimHeader.SetClaimTypeID(ddClaimTypeID.SelectedItem.Value) ' update claim type value
			claimHeader.SetReasonID(0) ' reset reason value
			ManageClaimTypeControls("show-all")	' show claim type controls
		End If

	End Sub

	Protected Sub ddVendorID_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddVendorID.SelectedIndexChanged '// vendor option selected, update related controls

		If ddVendorID.SelectedItem.Value = "0" Then	' vendor is unselected
			claimHeader.SetVendorID(0) ' reset vendor value
			ManageVendorControls("hide-all") ' show hide/reset vendor controls
		Else
			claimHeader.SetDivisionID(0) ' reset division value
			ManageVendorControls("show-divisions") ' show division control
			ManageVendorControls("hide-details") ' hide/reset vendor/division related controls
		End If

	End Sub

	Protected Sub ddDivisionID_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddDivisionID.SelectedIndexChanged '// division option selected, update related controls

		If ddDivisionID.SelectedItem.Value = "0" Then ' division is unselected
			claimHeader.SetVendorID(ddVendorID.SelectedItem.Value) ' assign vendor value
			claimHeader.SetDivisionID(0) ' reset division value
			ManageVendorControls("hide-details") ' hide/reset vendor/division controls
		Else
			claimHeader.SetVendorID(ddVendorID.SelectedItem.Value) ' update vendor value
			claimHeader.SetDivisionID(ddDivisionID.SelectedItem.Value) ' update division value
			ManageVendorControls("show-details") ' show vendor/division controls
		End If

	End Sub

	Protected Sub cblAdditionalRecipients_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles cblAdditionalRecipients.DataBound '// additional recipients checkbox list populated, tick appropriate checboxes

		Dim thisSelectedRecipients As String = claimHeader.GetAdditionalRecipients

		If Not String.IsNullOrEmpty(thisSelectedRecipients) Then
			thisSelectedRecipients = "|" & Replace(thisSelectedRecipients, ":", "|") & "|"
		End If

		For Each li As ListItem In cblAdditionalRecipients.Items
			If InStr(thisSelectedRecipients, "|" & li.Value & "|") Then
				li.Selected = True
			End If
		Next

	End Sub

	'-------------------------------------------

	' Control data binding

	'-------------------------------------------

	Private Sub BindData_OwningUserID(ByVal thisUserID As String)	'// assign user data to dropdownlist

		Dim dtCurrentDate As DateTime = DateTime.Now
		Dim sqlCurrentDate As String = utils.ConvertToSQLDate(dtCurrentDate.ToString())
		Dim ddDataSet As New DataSet()

		'// check for expired elements
		Dim dataSQL As String = ""
		dataSQL = dataSQL & "select * from user_t "
		dataSQL = dataSQL & "where ("
		dataSQL = dataSQL & "enabled_flag = 'Y' "
		dataSQL = dataSQL & "and (user_start_date_active IS NULL or user_start_date_active <= '" & sqlCurrentDate & "') "	' has started
		dataSQL = dataSQL & "and (user_end_date_active IS NULL or user_end_date_active > '" & sqlCurrentDate & "') "	' but not expired
		dataSQL = dataSQL & ") "

		If thisUserID > 0 Then	'// include the expired promotion if it was the one used previously
			dataSQL = dataSQL & "or (user_id = " & thisUserID & ") "
		End If

		dataSQL = dataSQL & "order by user_name "


		mySqlConnection = New MySqlConnection(mySqlConnectionString)
		mySqlCommand.CommandText = dataSQL
		mySqlCommand.Connection = mySqlConnection

		mySqlDataAdapter = New MySqlDataAdapter(mySqlCommand)
		mySqlDataAdapter.Fill(ddDataSet)

		mySqlConnection.Open()
		mySqlCommand.ExecuteNonQuery()
		mySqlConnection.Close()

		ddOwningUserID.DataSource = ddDataSet
		ddOwningUserID.DataValueField = "user_id"
		ddOwningUserID.DataTextField = "user_name"
		ddOwningUserID.DataBind()

		ddOwningUserID.Items.Insert(0, New ListItem("-- Please select --", "0")) ' add blank option

	End Sub

	Private Sub BindData_ClaimTypeID() '// assign claim type data to dropdownlist

		Dim ddDataSet As New DataSet()
		Dim dataSQL As String = ""
		dataSQL = dataSQL & "select * from claim_type_t "
		dataSQL = dataSQL & "order by claim_type "

		mySqlConnection = New MySqlConnection(mySqlConnectionString)
		mySqlCommand.CommandText = dataSQL
		mySqlCommand.Connection = mySqlConnection

		mySqlDataAdapter = New MySqlDataAdapter(mySqlCommand)
		mySqlDataAdapter.Fill(ddDataSet)

		mySqlConnection.Open()
		mySqlCommand.ExecuteNonQuery()
		mySqlConnection.Close()

		ddClaimTypeID.DataSource = ddDataSet
		ddClaimTypeID.DataValueField = "claim_type_id"
		ddClaimTypeID.DataTextField = "claim_type"
		ddClaimTypeID.DataBind()

		'// add blank option
		ddClaimTypeID.Items.Insert(0, New ListItem("-- Please select --", "0"))

	End Sub

	Private Sub BindData_ReasonID(ByVal thisClaimTypeID As String, ByVal thisReasonID As String) '// assign claim reason data to dropdownlist

		Dim dtCurrentDate As DateTime = DateTime.Now
		Dim sqlCurrentDate As String = utils.ConvertToSQLDate(dtCurrentDate.ToString())
		Dim ddDataSet As New DataSet()

		Dim thisDebitNoteTypeID As Integer = dbConnector.ID2Field("claim_type_t", thisClaimTypeID, "claim_type_id", "dn_type_id")

		'// check for expired elements
		Dim dataSQL As String = ""
		dataSQL = dataSQL & "select * from claim_reason_t "
		dataSQL = dataSQL & "where ("
		dataSQL = dataSQL & "dn_type_id = " & thisDebitNoteTypeID & " "
		dataSQL = dataSQL & "and enabled_flag = 'Y' "
		dataSQL = dataSQL & "and (end_date_active IS NULL or end_date_active > '" & sqlCurrentDate & "') "	' not expired
		dataSQL = dataSQL & ") "

		If thisReasonID > 0 Then	'// include the expired promotion if it was the one used previously
			dataSQL = dataSQL & "or (reason_id = " & thisReasonID & ") "
		End If

		dataSQL = dataSQL & "order by reason_name "


		mySqlConnection = New MySqlConnection(mySqlConnectionString)
		mySqlCommand.CommandText = dataSQL
		mySqlCommand.Connection = mySqlConnection

		mySqlDataAdapter = New MySqlDataAdapter(mySqlCommand)
		mySqlDataAdapter.Fill(ddDataSet)

		mySqlConnection.Open()
		mySqlCommand.ExecuteNonQuery()
		mySqlConnection.Close()

		ddReasonID.DataSource = ddDataSet
		ddReasonID.DataValueField = "reason_id"
		ddReasonID.DataTextField = "reason_name"
		ddReasonID.DataBind()

		ddReasonID.Items.Insert(0, New ListItem("-- Please select --", "0")) ' add blank option

	End Sub

	Protected Sub ddReasonID_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddReasonID.DataBound

		Dim ddList As DropDownList = TryCast(sender, DropDownList)
		For Each item As ListItem In ddList.Items

			If item.Text <> "" Then
				item.Text = utils.CleanUIString(item.Text)
			End If
		Next

	End Sub

	Private Sub BindData_PromotionID(ByVal thisPromotionID As String) '// assign promotion data to dropdownlist basd on claim type

		Dim dtCurrentDate As DateTime = DateTime.Now
		Dim sqlCurrentDate As String = utils.ConvertToSQLDate(dtCurrentDate.ToString())
		Dim ddDataSet As New DataSet()

		'// check for expired elements
		Dim dataSQL As String = ""
		dataSQL = dataSQL & "select promotion_id, CONCAT(promotion_name,'  ( ',promotion_description,' )  ') as displayValue, end_date_active from promotion_t "
		dataSQL = dataSQL & "where ("
		dataSQL = dataSQL & "enabled_flag = 'Y' "
		dataSQL = dataSQL & "and (end_date_active IS NULL or end_date_active > '" & sqlCurrentDate & "') "	' not expired
		dataSQL = dataSQL & ") "

		If thisPromotionID > 0 Then	'// include the expired promotion if it was the one used previously
			dataSQL = dataSQL & "or (promotion_id = " & thisPromotionID & ") "
		End If

		dataSQL = dataSQL & "order by promotion_name "

		mySqlConnection = New MySqlConnection(mySqlConnectionString)
		mySqlCommand.CommandText = dataSQL
		mySqlCommand.Connection = mySqlConnection

		mySqlDataAdapter = New MySqlDataAdapter(mySqlCommand)
		mySqlDataAdapter.Fill(ddDataSet)

		mySqlConnection.Open()
		mySqlCommand.ExecuteNonQuery()
		mySqlConnection.Close()

		ddPromotionID.DataSource = ddDataSet
		ddPromotionID.DataValueField = "promotion_id"
		ddPromotionID.DataTextField = "displayValue"
		ddPromotionID.DataBind()

		ddPromotionID.Items.Insert(0, New ListItem("-- Please select --", "0"))	' add blank option

	End Sub

	Protected Sub ddPromotionID_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddPromotionID.DataBound

		Dim ddList As DropDownList = TryCast(sender, DropDownList)
		For Each item As ListItem In ddList.Items

			If item.Text <> "" Then
				item.Text = utils.CleanUIString(item.Text)
			End If
		Next

	End Sub

	'Private Sub BindData_FinanceID() '// assign finance data to dropdownlist

	'Dim ddDataSet As New DataSet()

	'mySqlConnection = New MySqlConnection(mySqlConnectionString)
	'mySqlCommand.CommandText = "select * from inform_finance_t order by inform_finance_name"
	'mySqlCommand.Connection = mySqlConnection

	'mySqlDataAdapter = New MySqlDataAdapter(mySqlCommand)
	'mySqlDataAdapter.Fill(ddDataSet)

	'mySqlConnection.Open()
	'mySqlCommand.ExecuteNonQuery()
	'mySqlConnection.Close()

	'ddInformFinanceID.DataSource = ddDataSet
	'ddInformFinanceID.DataValueField = "inform_finance_id"
	'ddInformFinanceID.DataTextField = "inform_finance_name"
	'ddInformFinanceID.DataBind()

	'// add blank option
	'ddInformFinanceID.Items.Insert(0, New ListItem("-- Please select --", "0"))

	'End Sub

	Private Sub BindData_VendorID(ByVal thisVendorID As String)	'// assign vendor data to dropdownlist

		Dim dtCurrentDate As DateTime = DateTime.Now
		Dim sqlCurrentDate As String = utils.ConvertToSQLDate(dtCurrentDate.ToString())
		Dim ddDataSet As New DataSet()

		'// check for expired elements
		Dim dataSQL As String = ""
        dataSQL = dataSQL & "select vendor_t.vendor_id,vendor_t.vendor_name,concat(vendor_t.vendor_name, '-', cast(vendor_t.vendor_account as char)) as accountNameField,vendor_t.vendor_account,vendor_t.enabled_flag,vendor_t.start_date_active,vendor_t.end_date_active from vendor_t "
		dataSQL = dataSQL & "join division_t on vendor_t.vendor_id = division_t.vendor_id "
		dataSQL = dataSQL & "join vendor_address_t on division_t.location_id = vendor_address_t.location_id "

		dataSQL = dataSQL & "where ("
		dataSQL = dataSQL & "vendor_t.start_date_active < '" & sqlCurrentDate & "' "
		dataSQL = dataSQL & "and (vendor_t.end_date_active IS NULL or vendor_t.end_date_active > '" & sqlCurrentDate & "') "
		dataSQL = dataSQL & "and (vendor_address_t.inactive_date IS NULL or vendor_address_t.inactive_date > '" & sqlCurrentDate & "') " ' not expired
		dataSQL = dataSQL & ") "

		If thisVendorID > 0 Then '// include the expired vendor if it was the one used previously
			dataSQL = dataSQL & "or (vendor_t.vendor_id = " & thisVendorID & ") "
		End If

		dataSQL = dataSQL & "group by vendor_t.vendor_id "
		dataSQL = dataSQL & "order by vendor_t.vendor_name "

		mySqlConnection = New MySqlConnection(mySqlConnectionString)
		mySqlCommand.CommandText = dataSQL
		mySqlCommand.Connection = mySqlConnection

		mySqlDataAdapter = New MySqlDataAdapter(mySqlCommand)
		mySqlDataAdapter.Fill(ddDataSet)

		mySqlConnection.Open()
		mySqlCommand.ExecuteNonQuery()
		mySqlConnection.Close()

		ddVendorID.DataSource = ddDataSet
		ddVendorID.DataValueField = "vendor_id"
        ddVendorID.DataTextField = "accountNameField"
		ddVendorID.DataBind()

		ddVendorID.Items.Insert(0, New ListItem("-- Please select --", "0")) ' add blank option

	End Sub

	Protected Sub ddVendorID_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddVendorID.DataBound

		Dim ddList As DropDownList = TryCast(sender, DropDownList)
		For Each item As ListItem In ddList.Items

			If item.Text <> "" Then
				item.Text = utils.CleanUIString(item.Text)
			End If
		Next

	End Sub

	Private Sub BindData_DivisionID(ByVal thisVendorID As String, ByVal thisDivisionID As Integer)	'// assign division data to dropdownlist (based on vendor id)

		Dim dtCurrentDate As DateTime = DateTime.Now
		Dim sqlCurrentDate As String = utils.ConvertToSQLDate(dtCurrentDate.ToString())
		Dim ddDataSet As New DataSet()

		'// check for expired elements
		Dim dataSQL As String = ""
		dataSQL = dataSQL & "select d.division_id, d.division_name, d.location_id, va.inactive_date from division_t d "
		dataSQL = dataSQL & "join vendor_address_t va on d.location_id = va.location_id "
		dataSQL = dataSQL & "where ("
		dataSQL = dataSQL & "d.vendor_id = " & thisVendorID & " "
		dataSQL = dataSQL & "and (va.inactive_date IS NULL or va.inactive_date > '" & sqlCurrentDate & "') "
		dataSQL = dataSQL & ") "

		If thisDivisionID > 0 Then '// include the expired division if it was the one used previously
			dataSQL = dataSQL & "or (d.division_id = " & thisDivisionID & ") "
		End If

		dataSQL = dataSQL & "group by d.division_id "
		dataSQL = dataSQL & "order by d.division_name "

		mySqlConnection = New MySqlConnection(mySqlConnectionString)
		mySqlCommand.CommandText = dataSQL
		mySqlCommand.Connection = mySqlConnection

		mySqlDataAdapter = New MySqlDataAdapter(mySqlCommand)
		mySqlDataAdapter.Fill(ddDataSet)

		mySqlConnection.Open()
		mySqlCommand.ExecuteNonQuery()
		mySqlConnection.Close()

		ddDivisionID.DataSource = ddDataSet
		ddDivisionID.DataValueField = "division_id"
		ddDivisionID.DataTextField = "division_name"
		ddDivisionID.DataBind()

		ddDivisionID.Items.Insert(0, New ListItem("-- Please select --", "0")) ' add blank option

	End Sub

	Protected Sub ddDivisionID_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddDivisionID.DataBound

		Dim ddList As DropDownList = TryCast(sender, DropDownList)
		For Each item As ListItem In ddList.Items

			If item.Text <> "" Then
				item.Text = utils.CleanUIString(item.Text)
			End If
		Next

	End Sub

	Private Sub BindData_AdditionalRecipients(ByVal thisVendorID As Integer, Optional ByVal thisPrimaryContactID As Integer = 0, Optional ByVal thisAccountContactID As Integer = 0) '// assign data to checkbox list based on vendor id

		Dim contactFilterSQL As String = ""	' remove primary & account contacts
		If IsNumeric(thisPrimaryContactID) And IsNumeric(thisAccountContactID) Then
			contactFilterSQL = " and contact_id not in (" & thisPrimaryContactID & "," & thisAccountContactID & ")"
		End If

		Dim ddDataSet As New DataSet()
		Dim dataSQL As String = ""
		dataSQL = dataSQL & "select * from contact_t "
		dataSQL = dataSQL & "where vendor_id = " & thisVendorID & contactFilterSQL & " "
		dataSQL = dataSQL & "order by contact_name "

		mySqlConnection = New MySqlConnection(mySqlConnectionString)
		mySqlCommand.CommandText = dataSQL
		mySqlCommand.Connection = mySqlConnection

		mySqlDataAdapter = New MySqlDataAdapter(mySqlCommand)
		mySqlDataAdapter.Fill(ddDataSet)

		mySqlConnection.Open()
		mySqlCommand.ExecuteNonQuery()
		mySqlConnection.Close()

		cblAdditionalRecipients.DataSource = ddDataSet
		cblAdditionalRecipients.DataValueField = "contact_id"
		cblAdditionalRecipients.DataTextField = "contact_name"
		cblAdditionalRecipients.DataBind()

	End Sub

	'-------------------------------------------

	' Gridview events | New claim line

	'-------------------------------------------

	Protected Sub NCLGV_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles NewClaimLineGV.Load

		claimLine.GVLoad(Page.IsPostBack, NewClaimLineGV, claimHeader, sender, e)

	End Sub

	Protected Sub NCLGV_OnRowDataBound(ByVal sender As Object, ByVal e As GridViewRowEventArgs) Handles NewClaimLineGV.RowDataBound

		claimLine.GVRowDataBound(NewClaimLineGV, claimHeader, sender, e)

	End Sub

	Protected Sub NCLGV_RowEditing(ByVal sender As Object, ByVal e As GridViewEditEventArgs) Handles NewClaimLineGV.RowEditing

		'// add functional attributes
		NewClaimLineGV.EditIndex = e.NewEditIndex ' activate edit - row
		NewClaimLineGV.SelectedIndex = e.NewEditIndex ' activate select - row

		'// update gridview data (new claim lines)
		claimLine.GVBindData(NewClaimLineGV, claimHeader)

	End Sub

	Protected Sub NewClaimLineGV_RowCancelingEdit(ByVal sender As Object, ByVal e As GridViewCancelEditEventArgs) Handles NewClaimLineGV.RowCancelingEdit

		NewClaimLineGV.EditIndex = 0
		NewClaimLineGV.SelectedIndex = 0

		'// update gridview data (new claim lines)
		claimLine.GVBindData(NewClaimLineGV, claimHeader)

	End Sub

	Protected Sub NewClaimLineGV_RowUpdating(ByVal sender As Object, ByVal e As GridViewUpdateEventArgs) Handles NewClaimLineGV.RowUpdating

		If NewClaimLineGV_hdnRowValid.Value = "true" Then ' check line has passed validation
			'// save new claim line data to object then data to db
			SetClaimLineGetForm(NewClaimLineGV, 0)
			claimLine.SetDBGetClaimLine("insert", "claim-line-gridview")

			NewClaimLineGV.EditIndex = 0
			NewClaimLineGV.SelectedIndex = 0

			'// update gridview data (new & existing claim lines)
			claimLine.GVBindData(ExistingClaimLineGV, claimHeader)
			claimLine.GVBindData(NewClaimLineGV, claimHeader)

		End If

	End Sub

	'-------------------------------------------

	' Gridview events | Existing claim line

	'-------------------------------------------

	Protected Sub ECLGV_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles ExistingClaimLineGV.Load

		claimLine.GVLoad(Page.IsPostBack, ExistingClaimLineGV, claimHeader, sender, e)

	End Sub

	Protected Sub ECLGV_OnRowDataBound(ByVal sender As Object, ByVal e As GridViewRowEventArgs) Handles ExistingClaimLineGV.RowDataBound

		claimLine.GVRowDataBound(ExistingClaimLineGV, claimHeader, sender, e)

	End Sub

	Protected Sub ECLGV_RowSelecting(ByVal sender As Object, ByVal e As GridViewSelectEventArgs) Handles ExistingClaimLineGV.SelectedIndexChanging

		ExistingClaimLineGV.SelectedIndex = e.NewSelectedIndex ' activate select options

	End Sub

	Protected Sub ECLGV_RowEditing(ByVal sender As Object, ByVal e As GridViewEditEventArgs) Handles ExistingClaimLineGV.RowEditing

		Dim currentRowIndex As Integer = ExistingClaimLineGV.EditIndex

		If currentRowIndex <> -1 Then ' line previously selected, trigger update
			SetClaimLineGetForm(ExistingClaimLineGV, currentRowIndex)
			If ExistingClaimLineGV_hdnRowValid.Value = "true" Then	' check line has passed validation
				claimLine.SetDBGetClaimLine("update", "claim-line-gridview") ' save previously edited line
			End If
		End If

		ExistingClaimLineGV.EditIndex = e.NewEditIndex ' activate edit & select options
		ExistingClaimLineGV.SelectedIndex = e.NewEditIndex

		claimLine.GVBindData(ExistingClaimLineGV, claimHeader) ' update gridview data (existing claim lines)

	End Sub

	Protected Sub ECLGV_RowCancellingEdit(ByVal sender As Object, ByVal e As GridViewCancelEditEventArgs) Handles ExistingClaimLineGV.RowCancelingEdit

		ExistingClaimLineGV.EditIndex = -1
		ExistingClaimLineGV.SelectedIndex = -1
		claimLine.GVBindData(ExistingClaimLineGV, claimHeader) ' update gridview data

	End Sub

	Protected Sub ECLGV_RowUpdating(ByVal sender As Object, ByVal e As GridViewUpdateEventArgs) Handles ExistingClaimLineGV.RowUpdating

		Dim currentRowIndex As Integer = ExistingClaimLineGV.EditIndex

		SetClaimLineGetForm(ExistingClaimLineGV, currentRowIndex)
		If ExistingClaimLineGV_hdnRowValid.Value = "true" Then ' check line has passed validation
			claimLine.SetDBGetClaimLine("update", "claim-line-gridview")
		End If

		ExistingClaimLineGV.EditIndex = -1
		claimLine.GVBindData(ExistingClaimLineGV, claimHeader) ' update gridview data (existing claim lines)

	End Sub

    Protected Sub ECLGV_RowDeleting(ByVal sender As Object, ByVal e As GridViewDeleteEventArgs) Handles ExistingClaimLineGV.RowDeleting ' claim line deleted, get id, delete from database & refresh view

        Dim thisRequestLineID As String = ExistingClaimLineGV.Rows(e.RowIndex).Attributes("data-request-line-id")

        mySqlConnection = New MySqlConnection(mySqlConnectionString)
        mySqlCommand.CommandText = "delete from credit_request_line where request_line_id=" & thisRequestLineID
        mySqlCommand.Connection = mySqlConnection

        mySqlConnection.Open()
        mySqlCommand.ExecuteNonQuery()
        mySqlConnection.Close()

        claimLine.GVBindData(ExistingClaimLineGV, claimHeader) ' update gridview data (existing claim lines)

    End Sub

	'-------------------------------------------

	' paging

	'-------------------------------------------

	Protected Sub ECLGV_PageIndexChanging(ByVal sender As Object, ByVal e As GridViewPageEventArgs) Handles ExistingClaimLineGV.PageIndexChanging ' paging controls called


		ExistingClaimLineGV.PageIndex = e.NewPageIndex
		ExistingClaimLineGV.EditIndex = -1
		ExistingClaimLineGV.SelectedIndex = -1

		claimLine.GVBindData(ExistingClaimLineGV, claimHeader) ' update gridview data (existing claim lines)

	End Sub

	'-------------------------------------------

	' sorting

	'-------------------------------------------

	Protected Sub ECLGV_Sorting(ByVal sender As Object, ByVal e As GridViewSortEventArgs) Handles ExistingClaimLineGV.Sorting

		If e.SortExpression <> String.Empty Then
			ExistingClaimLineGV.EditIndex = -1
			ExistingClaimLineGV.SelectedIndex = -1

			claimLine.GVBindData(ExistingClaimLineGV, claimHeader, gridviewExt.GetSortExpression(ExistingClaimLineGV, e.SortExpression, e.SortDirection)) ' update gridview data (existing claim lines)

		End If

	End Sub

	'-------------------------------------------

	' MsgQ Operations

	'-------------------------------------------

	Protected Function ValidateInOracle(ByVal claimHeader As ClaimHeader) As Boolean

		Dim returnValue As Boolean

		Dim XMLClaim As XDocument = New XDocument
		XMLClaim = ClaimAsXML(claimHeader)

		Dim message() As Byte = System.Text.Encoding.ASCII.GetBytes(XMLClaim.ToString)

		Dim producer As New RabbitMQProducer
		producer.SetTarget("ma-mqd-02.micro-p.com", "qClaimValidationRequest")

		If (producer.ConnectToRabbitMQ) Then

			Try
				producer.SendMessage(message)
				returnValue = True
			Catch ex As Exception
				returnValue = False
			End Try

			claimHeader.SetDBHistory("Oracle Validation", "Blue", "Requested - Pending Response", XMLClaim.ToString())

		Else
			'// connection failed
			returnValue = False
			claimHeader.SetSysMsg("Error|||RabbitMQ Connection Failed")

		End If
		producer.Dispose()

		Return returnValue

	End Function

	Protected Function SubmitToOracle(ByVal claimHeader As ClaimHeader) As Boolean

		Dim returnValue As Boolean

		Dim XMLClaim As XDocument = New XDocument
		XMLClaim = ClaimAsXML(claimHeader)

		Dim message() As Byte = System.Text.Encoding.ASCII.GetBytes(XMLClaim.ToString)

		Dim producer As New RabbitMQProducer
		producer.SetTarget("ma-mqd-02.micro-p.com", "qClaimRequest")

		If (producer.ConnectToRabbitMQ) Then

			Try
				producer.SendMessage(message)
				returnValue = True
			Catch ex As Exception
				returnValue = False
			End Try

			claimHeader.SetDBHistory("Oracle Submission", "Blue", "Requested - Pending Response", XMLClaim.ToString())

		Else
			'// connection failed
			returnValue = False
			claimHeader.SetSysMsg("Error|||RabbitMQ Connection Failed")

		End If
		producer.Dispose()

		Return returnValue

	End Function

	'-------------------------------------------

	' XML operations

	'-------------------------------------------

	Public Function ClaimAsXML(ByVal claimHeader As ClaimHeader) As XDocument

		Dim returnValue As New XDocument
		Dim claimLine As New ClaimLine

		' get header & line content
		Dim headerXElement As XElement = claimHeader.GetXElementClaimHeader
		Dim linesXElement As XElement = claimLine.GetXElementClaimLines(claimHeader)

		' create XML document
		Dim claimXML As XDocument = _
			<?xml version="1.0" encoding="utf-8" standalone="no"?>
			<DBRequest></DBRequest>

		claimXML.Root.Add(headerXElement)
		claimXML.Root.Add(linesXElement)

		returnValue = claimXML

		Return returnValue

	End Function

	'-------------------------------------------

	' Excel operations

	'-------------------------------------------

	Public Sub ExportToExcel(ByVal thisAction As String)

		Dim thisCMD As String = "save"
		If thisAction = "user-action" Then
			thisCMD = "download"
		End If

		' create xls file cell formating styles
		Dim style As String = ""
		style = style & "<style>"
		style = style & ".string{mso-number-format:\@;}"
		style = style & ".number{mso-number-format:0}"
		style = style & ".one-decimal{mso-number-format:0\.0;}"
		style = style & ".two-decimal{mso-number-format:0\.00;}"
		style = style & ".date{mso-number-format:'Short Date';}"
		style = style & "</style>"

		' get header & line content
		Dim headerTable As String = claimHeader.GetXLSClaimHeader
		Dim lineTable As String = claimLine.GetXLSClaimLines(claimHeader, thisAction)
		Dim fileContent As String = style & vbCrLf & headerTable & vbCrLf & lineTable
		Dim SystemDirRoot As String = Server.MapPath("~/")

		' save excel version of the claim
		If (thisAction = "system-action") Or (thisAction = "user-action" And thisCMD = "save") Then	' system-action save file

			Dim assets As New Assets

			assets.SetDirID(claimHeader.GetRequestID)
			assets.SetDirLabel("claim-header")
			assets.SetRootDir("assets")
			assets.SetDirGroup("claim")
			assets.SetFileName("claim-" & claimHeader.GetRequestID & ".xls")
			assets.SetFileExt(".xls")
			assets.SetFileType("application/vnd.ms-excel")
			assets.SetFileGroup("document")
			assets.SetFilePath("claim-header-" & claimHeader.GetRequestID)
			assets.SetOwnerID(Session("userID"))
			assets.SetDateUpdated(utils.ConvertToUIDate(System.DateTime.Now))
			assets.SetAccessLevel(1)

			assets.SaveFileFromStream(fileContent, SystemDirRoot)

		End If

		'download excel version of the claim
		If thisCMD = "download" Then

			Response.Clear()
			Response.Charset = ""
			Response.ContentType = "application/vnd.ms-excel"
			Response.AppendHeader("Content-Disposition", "attachment; filename=claim-" & claimHeader.GetRequestID & ".xls")
			Response.ContentEncoding = System.Text.Encoding.Default
			Response.Write(fileContent)
			Response.Flush()
			Response.End()

		End If

	End Sub

	'-------------------------------------------

	' Get form values, set objects

	'-------------------------------------------

	Public Sub SetClaimHeaderGetForm()

		claimHeader.SetRequestID(tbRequestID.Text)
		claimHeader.SetClaimTypeID(ddClaimTypeID.SelectedValue)
		claimHeader.SetVendorID(ddVendorID.SelectedValue)
		claimHeader.SetOwningUserID(ddOwningUserID.SelectedValue)
		claimHeader.SetDivisionID(ddDivisionID.SelectedValue)
		claimHeader.SetReasonID(ddReasonID.SelectedValue)
		claimHeader.SetPromotionID(ddPromotionID.SelectedValue)
		'claimHeader.SetInformFinanceID(ddInformFinanceID.SelectedValue)

		If cbTransmissionRequired.Checked = True Then
			claimHeader.SetTransmissionRequired(1)
		Else
			claimHeader.SetTransmissionRequired(0)
		End If

		claimHeader.SetVendorReference(tbVendorReference.Text)
		claimHeader.SetRequestDescription(tbRequestDescription.Text)
		claimHeader.SetDateRaised(tbDateRaised.Text)

		' get additional recipients (checkboxlist)
		Dim arrAdditionalRecipients As New List(Of String)
		For Each li As ListItem In cblAdditionalRecipients.Items
			If li.Selected Then
				arrAdditionalRecipients.Add(li.Value)
			End If
		Next

		If arrAdditionalRecipients.Count > 0 Then
			claimHeader.SetAdditionalRecipients(String.Join(":", arrAdditionalRecipients.ToArray()))
		End If

		claimHeader.SetOutstandingAmount(tbOutstandingAmount.Text)
		claimHeader.SetPrivateNotes(tbPrivateNotes.Text)

		claimHeader.SetPriceProtectionNo(tbPriceProtectionNo.Text)
		claimHeader.SetOriginalDnNo(tbOriginalDnNo.Text)
		claimHeader.SetReversingDnNo(tbReversingDnNo.Text)

		' get data from hidden fields
		claimHeader.SetBusinessUnitID(hdnBusinessUnitID.Value)
		claimHeader.SetLocationID(hdnLocationID.Value)
		claimHeader.SetDNTypeID(hdnDNTypeID.Value)
		claimHeader.SetCurrencyID(hdnCurrencyID.Value)
		claimHeader.SetClaimStatusID(hdnClaimStatus.Value)
		claimHeader.SetNumberOfClaimLines(hdnNumberOfClaimLines.Value)


	End Sub

	'-------------------------------------------

	' Get gridview values, set object

	'-------------------------------------------

	Private Sub SetClaimLineGetForm(ByVal thisGridView As GridView, ByVal thisRowIndex As Integer)
		'// get data from passed gridview object & assign to object

		'// visible fields
		Dim tbLineClaimReference As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(1).Controls(0), TextBox)
		Dim tbProductCode As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(2).Controls(0), TextBox)
		Dim tbVendorPart As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(3).Controls(0), TextBox)
		Dim tbClaimReference As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(4).Controls(0), TextBox)
		Dim tbLineDescription As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(5).Controls(0), TextBox)
		Dim tbLineQuantity As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(6).Controls(0), TextBox)
		Dim tbLineOriginalCost As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(7).Controls(0), TextBox)
		Dim tbLineNewCost As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(8).Controls(0), TextBox)
		Dim tbLineDelta As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(9).Controls(0), TextBox)
		Dim tbLineValue As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(10).Controls(0), TextBox)
		Dim tbLineOrderNumber As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(11).Controls(0), TextBox)
		Dim tbLineVendor As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(12).Controls(0), TextBox)
		Dim tbLineComments As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(13).Controls(0), TextBox)
		Dim tbDateRaised As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(14).Controls(0), TextBox)
		Dim tbFlexVarchar1 As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(15).Controls(0), TextBox)
		Dim tbFlexVarchar2 As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(16).Controls(0), TextBox)
		Dim tbFlexVarchar3 As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(17).Controls(0), TextBox)
		Dim tbFlexVarchar4 As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(18).Controls(0), TextBox)
		Dim tbFlexVarchar5 As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(19).Controls(0), TextBox)
		Dim tbFlexVarchar6 As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(20).Controls(0), TextBox)
		Dim tbFlexVarchar7 As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(21).Controls(0), TextBox)
		Dim tbFlexVarchar8 As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(22).Controls(0), TextBox)
		Dim tbFlexVarchar9 As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(23).Controls(0), TextBox)
		Dim tbFlexVarchar10 As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(24).Controls(0), TextBox)
		Dim tbPrivateNotes As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(25).Controls(0), TextBox)

		'// system fields
		Dim tbRequestLineID As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(26).Controls(0), TextBox)
		Dim tbLineID As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(27).Controls(0), TextBox)
		Dim tbRequestID As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(28).Controls(0), TextBox)
		Dim tbProcessed As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(29).Controls(0), TextBox)
		Dim tbBlankRow As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(30).Controls(0), TextBox)
		Dim tbApproved As TextBox = TryCast(thisGridView.Rows(thisRowIndex).Cells(31).Controls(0), TextBox)


		If tbRequestID.Text > 0 Then
			claimLine.SetRequestID(tbRequestID.Text)
		Else
			claimLine.SetRequestID(thisGridView.Attributes("data-request-id"))
		End If

		claimLine.SetRequestLineID(tbRequestLineID.Text)
		claimLine.SetLineID(tbLineID.Text)
		claimLine.SetLineClaimReference(tbLineClaimReference.Text)
		claimLine.SetLineDescription(tbLineDescription.Text)
		claimLine.SetLineQuantity(tbLineQuantity.Text)
		claimLine.SetLineOriginalCost(tbLineOriginalCost.Text)
		claimLine.SetLineNewCost(tbLineNewCost.Text)
		claimLine.SetLineDelta(tbLineDelta.Text)
		claimLine.SetLineValue(tbLineValue.Text)
		claimLine.SetProductCode(tbProductCode.Text)
		claimLine.SetVendorPart(tbVendorPart.Text)
		claimLine.SetClaimReference(tbClaimReference.Text)
		claimLine.SetLineOrderNumber(tbLineOrderNumber.Text)
		claimLine.SetLineVendor(tbLineVendor.Text)
		claimLine.SetLineComments(tbLineComments.Text)
		claimLine.SetDateRaised(tbDateRaised.Text)
		claimLine.SetProcessed(tbProcessed.Text)
		claimLine.SetBlankRow(tbBlankRow.Text)
		claimLine.SetFlexVarchar1(tbFlexVarchar1.Text)
		claimLine.SetFlexVarchar2(tbFlexVarchar2.Text)
		claimLine.SetFlexVarchar3(tbFlexVarchar3.Text)
		claimLine.SetFlexVarchar4(tbFlexVarchar4.Text)
		claimLine.SetFlexVarchar5(tbFlexVarchar5.Text)
		claimLine.SetFlexVarchar6(tbFlexVarchar6.Text)
		claimLine.SetFlexVarchar7(tbFlexVarchar7.Text)
		claimLine.SetFlexVarchar8(tbFlexVarchar8.Text)
		claimLine.SetFlexVarchar9(tbFlexVarchar9.Text)
		claimLine.SetFlexVarchar10(tbFlexVarchar10.Text)
		claimLine.SetApproved(tbApproved.Text)
		claimLine.SetPrivateNotes(tbPrivateNotes.Text)

	End Sub

	Public Sub ClearSystemMessage()

		Dim hdnSysMsg As HiddenField = Master.FindControl("hdnSystemMessage") ' update hidden control value - system message
		claimHeader.SetSysMsg("")
		hdnSysMsg.Value = ""

	End Sub

	Public Sub SetSystemMessage(ByVal theMessage As String)

		Dim hdnSysMsg As HiddenField = Master.FindControl("hdnSystemMessage") ' update hidden control value - system message

		If theMessage <> "" Then

			If (InStr(theMessage, "Oracle Validation") > 0) Or (InStr(theMessage, "Email Manager") > 0) Then
				hdnSysMsg.Value = theMessage
			Else
				hdnSysMsg.Value = "Claim Form|||" & theMessage
			End If

		End If

	End Sub

	Public Sub SetPostBackCount(ByVal postBack As Boolean)

		Dim hdnPostBackCount As HiddenField = Master.FindControl("hdnPostBackCount") ' update hidden control value - postback counter
		Dim currentCount As Integer = 0
		Dim newCount As Integer = 0

		Integer.TryParse(hdnPostBackCount.Value, currentCount)
		If postBack Then
			newCount = currentCount + 1
		Else
			newCount = 0
		End If
		hdnPostBackCount.Value = newCount
		ScriptManager.RegisterStartupScript(Me, Page.GetType, "ClientScript", "postBackEvent(" & newCount & ");", True)

	End Sub

	Protected Sub Page_Unload(sender As Object, e As EventArgs) Handles Me.Unload

    End Sub
    Protected Sub btnECLGVDeleteAllLines_Click(sender As Object, e As ImageClickEventArgs)
        mySqlConnection = New MySqlConnection(mySqlConnectionString)
        mySqlCommand.CommandText = "delete from credit_request_line where request_id=" & claimHeader.GetRequestID()
        mySqlCommand.Connection = mySqlConnection

        mySqlConnection.Open()
        mySqlCommand.ExecuteNonQuery()
        mySqlConnection.Close()

        claimLine.GVBindData(ExistingClaimLineGV, claimHeader) ' update gridview data (existing claim lines)
        btnECLGVDeleteAllLines.Visible = False
    End Sub

    Protected Sub ExistingClaimLineGV_DataBound(sender As Object, e As EventArgs)
        Dim claimStatusIDLine As Integer = claimHeader.GetClaimStatusID()
        
        mySqlConnection = New MySqlConnection(mySqlConnectionString)
        mySqlCommand.CommandText = "select count(*) from credit_request_line where request_id=" & claimHeader.GetRequestID
        mySqlCommand.Connection = mySqlConnection
        mySqlConnection.Open()
        Dim noOfClaimLines As Integer = Integer.Parse(mySqlCommand.ExecuteScalar().ToString())
        mySqlConnection.Close()

        If ((claimStatusIDLine = 10 Or claimStatusIDLine = 40) And noOfClaimLines > 0) Then
            btnECLGVDeleteAllLines.Visible = True
        Else
            btnECLGVDeleteAllLines.Visible = False
        End If
    End Sub
End Class