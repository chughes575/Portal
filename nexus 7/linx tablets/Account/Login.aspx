<%@ Page Title="Log in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="linx_tablets.Account.Login" %>

<%@ Register Src="~/Account/OpenAuthProviders.ascx" TagPrefix="uc" TagName="OpenAuthProviders" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <h2 class="h2-help">Login</h2>

                <section id="loginForm" style="padding: 0px 0">

                    <asp:Login runat="server" ViewStateMode="Disabled" RenderOuterTable="false">
                        <LayoutTemplate>
                            <p class="validation-summary-errors">
                                <asp:Literal runat="server" ID="FailureText" />
                            </p>
                            <fieldset>
                                
                                <ol>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="UserName">User name</asp:Label>
                                        <asp:TextBox runat="server" ID="UserName" />
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="UserName" CssClass="field-validation-error" ErrorMessage="The user name field is required." />
                                    </li>
                                    <li>
                                        <asp:Label runat="server" AssociatedControlID="Password">  Password</asp:Label>
                                        <asp:TextBox runat="server" ID="Password" TextMode="Password" />
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="Password" CssClass="field-validation-error" ErrorMessage="The password field is required." />
                                    </li>
                                    <li>
                                        <asp:CheckBox runat="server" ID="RememberMe" />
                                        <asp:Label runat="server" AssociatedControlID="RememberMe" CssClass="checkbox">Remember me?</asp:Label>
                                    </li>
                                </ol>
                                <asp:Button runat="server" CommandName="Login" Text="Log in" />
                            </fieldset>
                        </LayoutTemplate>
                    </asp:Login>
                </section>
                <asp:LoginView ID="defLoginView" runat="server">
                    <AnonymousTemplate>

                        <asp:PasswordRecovery ID="PasswordRecovery1" runat="server"></asp:PasswordRecovery>
                    </AnonymousTemplate>

                </asp:LoginView>

            </div>
        </div>
    </div>

</asp:Content>
