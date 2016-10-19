<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="ProductForecasting.aspx.cs" Inherits="linx_tablets.Argos.ProductForecasting" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from vw_portal_hive_consignmentuploadslastupdates order by lastfiledate desc"></asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlDSArgosIntakeUploads" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from MSE_PortalSalesEposFiles where customerid=4 and reporttype='intake' order by ImportDate desc"></asp:SqlDataSource>
    
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Argos Product Forecasting</h1>
                    

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
                    <asp:FileUpload ID="fupArgosForecast" runat="server" /><br />
                    <asp:Button ID="btnUploadArgosForecast" runat="server" Text="Upload" OnClick="btnUploadArgosForecast_Click" />
                    <h2>Argos Intake Upload</h2>
                    <h3>Existing Uploads</h3>
                    *latest upload will be used
                    <div style="
    border: 5px solid gray;
    padding: 5px;
    background: white;
    width: 70%;
    height: 200px;
    overflow-y: scroll;">
                    <asp:GridView ID="gvArgosUploads" Style=" overflow: auto" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSArgosIntakeUploads" DataKeyNames="ID" AutoGenerateColumns="false" OnRowCommand="gvArgosUploads_RowCommand">
                        <Columns>
                            <asp:BoundField ReadOnly="true" DataField="ID" HeaderText="ID" />
                            <asp:BoundField ReadOnly="true" DataField="ImportDate" HeaderText="Date Uploaded" DataFormatString="{0:f}" />
                            <asp:BoundField ReadOnly="true" DataField="Filename" HeaderText="Filename" />
                            <asp:TemplateField>
                                <HeaderTemplate>Download File</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnDownloadFile" runat="server" Text="Download File" CommandName="downloadfile" CommandArgument='<%# Eval("ID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                    </asp:GridView>
                        </div>



                    <asp:FileUpload ID="fupArgosIntake" runat="server" /><br />
                    <asp:Button ID="btnUploadArgosIntake" runat="server" Text="Upload" OnClick="btnUploadArgosIntake_Click" />
                    <h2>Argos Product Stock Holding Rules</h2>
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
                </div>
            </div>
        </div>
    </div>
</asp:Content>
