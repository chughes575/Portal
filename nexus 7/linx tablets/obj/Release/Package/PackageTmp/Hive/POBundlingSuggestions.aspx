<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="POBundlingSuggestions.aspx.cs" Inherits="linx_tablets.Hive.WorkorderManagement" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <script src="/Scripts/jquery.tablescroll.js"></script>
    <script type="text/javascript">



        jQuery(document).ready(function ($) {
            jQuery('#MainContent_gvBundleSuggestions').tableScroll({ height: 550, flush: false });


            document.getElementsByClassName('tablescroll_wrapper').style.overflowX = 'hidden';

        });
    </script>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h2>PO & Bundling Suggestions</h2>
                    <b>ExertisHive Current forecast / Sell through weeks used:</b> <asp:Label ID="lblExertisHiveWeeksUsed" runat="server"></asp:Label><br />
                    <b>Hive Current forecast / Sell through weeks used:</b> <asp:Label ID="lblWeeksUsed" runat="server"></asp:Label>
                    
                    <br />
                    <br />
                    Tab 1 (Total finished products required) - This is the amount of finished products required to be assemgled to fufill the current forecasted/sell through demand <br />
                    Tab 2 (Total components required) - This is the amount of each component required to  fufill the current forecasted/sell through demand
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Description</th>
                            <th>Download Work Order suggestions</th>
                        </tr>
                        <tr>
                            <td>Download Work Order Suggestions</td>
                            <td>
                                <asp:Button ID="btnDownloadBundleSuggestions" runat="server" Text="Download" OnClick="btnDownloadBundleSuggestions_Click" CommandArgument="b-sug" /></td>
                        </tr>

                    </table>
                    <br />
                    <h2>Supplier PO Suggestions</h2>

                    Component PO Suggestions include Hive spares and replacement products.
                    <br />
                    <br />
                    <b>Current forecast / Sell through weeks used:</b> <asp:Label ID="Label1" runat="server"></asp:Label>
                    <table class="CSSTableGenerator">
                            <tr>
                                <th>Description</th>
                                <th>Download po suggestions</th>
                            </tr>
                            <tr>
                                <td>Download Component Suggestions</td>
                                <td>
                                    <asp:Button ID="btnDownloadCompSuggestions" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="c-sug" /></td>
                            </tr>
                            
                        </table>
                    <div style="width: 1300px; overflow: scroll">
                        <%--<asp:GridView ID="gvBundleSuggestions" Visible="true" runat="server"  UseAccessibleHeader="true" OnPreRender="gvBundleSuggestions_PreRender"></asp:GridView>--%>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
