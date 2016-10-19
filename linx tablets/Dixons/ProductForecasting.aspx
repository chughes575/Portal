<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="ProductForecasting.aspx.cs" Inherits="linx_tablets.Dixons.ProductForecasting" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from vw_portal_hive_consignmentuploadslastupdates order by lastfiledate desc"></asp:SqlDataSource>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Dixons Product Forecasting</h1>
                    

                <br />
                    Current Forecast Week:
                    <asp:Label ID="lblCurrentForecastWeek" runat="server" ForeColor="Green"></asp:Label>
                    <ul>
                        <li>Forecasts can be input/uploaded for weeks 1-104 (Rolling 2 years) for any given bundle with week 1 being the current week. </li>
                        <li>Upon uploading any existing forecast entries for a product will be replaced with the values supplied in the file. E.g if week 1 is uploaded with a empty/blank value any existing week 1 forecast value will be removed</li>

                    </ul>
                    <h2>Forecast Upload</h2>

                    <br />
                    <br>


                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing forecast</th>
                            <th>Download historic forecast (By forecast year)</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownload3PLForecast" runat="server" Text="Download" OnClick="btnDownload3PLForecast_Click" /></td>
                            <td>
                                <asp:DropDownList ID="ddlHistoricForecastYears" runat="server" DataTextField="ForecastYear" DataValueField="ForecastYear" />&nbsp
                                <asp:Button ID="btnDownloadForecastHistoric" runat="server" Text="Download" OnClick="btnDownloadForecastHistoric_Click" />

                            </td>                            
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fupDixonsForecast" runat="server" /><br />
                    <asp:Button ID="btnUploadDixonsForecast" runat="server" Text="Upload" OnClick="btnUploadDixonsForecast_Click" />

                    <h2>Dixons Product Stock Holding Rules</h2>
                    Stock holding suggestions are based on forecasts and sell through data available.
                    <br />
                     If there is no forecast or they are lower than sell through Portal will suggest stock holding based on sell through.   
                    <br /><br />
                    Suggestions will work off the total amount of Forecast weeks recorded on Portal. 
                    <br /><br />

                    <h2>Forecast / Sell Through No# Weeks Used Set Up </h2>
                    No # of weeks used:
                      
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
                            </asp:DropDownList> <asp:Button ID="btnUpdateForecastUsed" runat="server" OnClick="btnUpdateForecastUsed_Click" Text="Update" />

                    <h2> Sell Through Overwrite</h2>
                    
                    Sell Through Overwrite Percentage (Whole number greater than zero required): <asp:TextBox ID="txtSellThroughPercentage" ValidationGroup="sellThrough" runat="server"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="sellThruRegex" runat="server" ValidationGroup="sellThrough" ValidationExpression="^[1-9][0-9]*$" ErrorMessage="Please enter a whole number greater than zero" ControlToValidate="txtSellThroughPercentage"></asp:RegularExpressionValidator>
                    <asp:Button ID="btnUpdateSellThroughOverwrite" ValidationGroup="sellThrough" runat="server" Text="Update Sell Through" OnClick="btnUpdateSellThroughOverwrite_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
