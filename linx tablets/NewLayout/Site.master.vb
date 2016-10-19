
Partial Class Site

	Inherits System.Web.UI.MasterPage

	Public user As User = New User
	Public bMatch As Boolean = False

	'-------------------------------------------

	' Page load & pre-render

	'-------------------------------------------

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

		bMatch = user.isUserLoggedIn(Request.QueryString("userID"))

		If bMatch = False Then
			'Response.Redirect("Account/Login.aspx")
		End If

	End Sub

End Class

