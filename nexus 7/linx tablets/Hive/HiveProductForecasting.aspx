<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="HiveProductForecasting.aspx.cs" Inherits="linx_tablets.Hive.CustomerForecasting" %>

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
                    <h1>Hive Product Forecasting</h1>
                    This page is used to input a forecast figure for a bundle. This can be done for Exertis or Hives forecast requirements and the forecast amount of weeks used can be independantly controlled.

                <br />
                    Current Forecast Week:
                    <asp:Label ID="lblCurrentForecastWeek" runat="server" ForeColor="Green"></asp:Label>
                    <ul>
                        <li>Forecasts can be input/uploaded for weeks 1-104 (Rolling 2 years) for any given bundle with week 1 being the current week. </li>
                        <li>Upon uploading any existing forecast entries for a product will be replaced with the values supplied in the file. E.g if week 1 is uploaded with a empty/blank value any existing week 1 forecast value will be removed</li>

                    </ul>
                    <h2>Hive 3PL Forecast Upload</h2>

                    <br />
                    <br>


                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing forecast</th>
                            <th>Download historic forecast (By forecast year)</th>
                            <th>Dowload blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownload3PLForecast" runat="server" Text="Download" OnClick="btnDownload3PLForecast_Click" /></td>
                            <td>
                                <asp:DropDownList ID="ddlHistoric3PLForecastYears" runat="server" DataTextField="ForecastYear" DataValueField="ForecastYear" />&nbsp
                                <asp:Button ID="btnDownload3PLForecastHistoric" runat="server" Text="Download" OnClick="btnDownload3PLForecastHistoric_Click" />

                            </td>
                            <td>
                                <asp:Button ID="btnDownload3plForecastTemplate" runat="server" Text="Download" OnClick="btnDownload3plForecastTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fup3PLForecast" runat="server" /><br />
                    <asp:Button ID="btnUpload3PLForecast" runat="server" Text="Upload" OnClick="btnUpload3PLForecast_Click" />



                    <h2>Exertis Hive Forecast Upload</h2>
                    <br />


                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing forecast</th>
                            <th>Download historic forecast (By forecast year)</th>
                            <th>Dowload blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadexertishiveForecast" runat="server" Text="Download" OnClick="btnDownloadexertishiveForecast_Click" /></td>
                            <td>
                                <asp:DropDownList ID="ddlHistoricExertisHiveForecastYears" runat="server" DataTextField="ForecastYear" DataValueField="ForecastYear" />&nbsp
                                <asp:Button ID="btnDownloadExertishiveForecastHistoric" runat="server" Text="Download" OnClick="btnDownloadExertishiveForecastHistoric_Click" />
                            </td>
                            <td>
                                <asp:Button ID="btnDownloadexertishiveForecastTemplate" runat="server" Text="Download" OnClick="btnDownloadexertishiveForecastTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fupExertisHiveForecast" runat="server" /><br />
                    <asp:Button ID="btnUploadExertisHiveForecast" runat="server" Text="Upload" OnClick="btnUploadExertisHiveForecast_Click" />
                    <h2>Forecast Exceptions</h2>
                    This sections allows you to modify how a products forecast can be modified. Example for a product you could enter 50% and the system would only use 50% of the actual inputted forecast when suggesting products.
                    <asp:GridView
                        ID="gvForecastPercentages"
                        runat="server"
                        AutoGenerateColumns="False"
                        DataKeyNames="ProductCode"
                        DataSourceID="sqlDsForecastException"
                        OnRowCommand="gvForecastPercentages_RowCommand"
                        ShowFooter="True">
                        <RowStyle BackColor="#EFF3FB" />
                        <Columns>
                            <asp:TemplateField HeaderText="Product Code">
                                <ItemTemplate>
                                    <asp:Label ID="lblProductCode" runat="server" Text='<%# Bind("ProductCode") %>'></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:TextBox ID="txtProductCode" runat="server" Height="44px" Text='<%# Bind("ProductCode") %>'></asp:TextBox>
                                </FooterTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Forecast Percentage">
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtForecastPercentage" runat="server" Text='<%# Bind("ForecastAmount") %>'></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" ControlToValidate="txtForecastPercentage" ValidationExpression="^(100\.00|100\.0|100)|([0-9]{1,2}){0,1}(\.[0-9]{1,2}){0,1}$" runat="server" ErrorMessage="Please enter a numeric value"></asp:RegularExpressionValidator>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblForecastPercentage" runat="server" Text='<%# Bind("ForecastAmount") %>'></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:TextBox ID="txtRetailDescription" runat="server" Height="44px" Text='<%# Bind("ForecastAmount") %>'></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" ControlToValidate="txtRetailDescription" ValidationExpression="^(100\.00|100\.0|100)|([0-9]{1,2}){0,1}(\.[0-9]{1,2}){0,1}$" runat="server" ErrorMessage="Please enter a numeric value"></asp:RegularExpressionValidator>
                                </FooterTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ShowHeader="False">
                                <EditItemTemplate>
                                    <asp:Button ID="Button1" runat="server" CausesValidation="True" CommandName="Update" Text="Update" />&nbsp;
                     <asp:Button ID="Button2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" />

                                </EditItemTemplate>
                                <FooterTemplate>
                                    <asp:Button ID="Button3" runat="server" CommandName="Insert" Text="Insert" />
                                </FooterTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="EditNotesButton" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit" />

                                </ItemTemplate>
                                <ItemStyle Width="50px" />
                            </asp:TemplateField>
                            <asp:TemplateField ShowHeader="False">
                                <ItemTemplate>
                                    <asp:Button ID="Button4" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>

                            <br />
                            </em>Add Entry:<em>&nbsp;<br />
                            </em>Product Code:
                            <asp:TextBox ID="txtNoDataProductCode" runat="server" Height="58px" TextMode="SingleLine"
                                Width="150px"></asp:TextBox>
                            </em>Forecast Percentage:
                            <asp:TextBox ID="txtNoDataForecast" runat="server" Height="58px" TextMode="SingleLine"
                                Width="150px"></asp:TextBox>
                            <asp:RegularExpressionValidator ID="RegularExpressionValidator1" ControlToValidate="txtNoDataForecast" ValidationExpression="^(100\.00|100\.0|100)|([0-9]{1,2}){0,1}(\.[0-9]{1,2}){0,1}$" runat="server" ErrorMessage="Please enter a numeric value"></asp:RegularExpressionValidator>
                            <asp:Button ID="btn" runat="server" CommandName="NoDataInsert" Text="Insert" />
                        </EmptyDataTemplate>
                    </asp:GridView>

                    <asp:SqlDataSource
                        ID="sqlDsForecastException"
                        runat="server"
                        ConflictDetection="CompareAllValues"
                        ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        DeleteCommand="DELETE FROM [MSE_PortalForecastExceptions] WHERE [ProductCode] = @original_productcode"
                        InsertCommand="INSERT INTO [MSE_PortalForecastExceptions] ([productcode], [ForecastAmount]) VALUES (@productcode,@forecastamount)"
                        OldValuesParameterFormatString="original_{0}"
                        SelectCommand="select * from MSE_PortalForecastExceptions"
                        UpdateCommand="UPDATE [MSE_PortalForecastExceptions] SET [forecastamount] = @forecastamount WHERE [ProductCode] = @original_productcode"
                        OnInserting="sqlDsForecastException_Inserting">
                        <DeleteParameters>
                            <asp:Parameter Name="original_productcode" Type="String" />
                        </DeleteParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="forecastamount" Type="Int32" />
                            <asp:Parameter Name="original_productcode" Type="String" />
                        </UpdateParameters>
                        <InsertParameters>
                            <asp:Parameter Name="productcode" Type="String" />
                            <asp:Parameter Name="forecastamount" Type="Int32" />
                        </InsertParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
