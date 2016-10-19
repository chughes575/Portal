<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="HubStockReplenishment.aspx.cs" Inherits="linx_tablets.Reporting.HubStockReplenishment" %>



<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
   
     <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <asp:SqlDataSource ID="sqlDSorscleLastRunOrderConfirmation" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid=5
order by lastfiledate desc"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlDSorscleLastRunASN" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid=1
order by lastfiledate desc"></asp:SqlDataSource>
    <asp:SqlDataSource ID="sqlDSorscleLastRunPoStock" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid in (2,3)
order by lastfiledate desc"></asp:SqlDataSource>

    <asp:SqlDataSource ID="sqlDSLastImportStockOverride" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid in (8)
order by lastfiledate desc"></asp:SqlDataSource>


    <asp:SqlDataSource ID="sqlDSReplenEntries" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select replenid,DateGenerated,SentToOracleDate,SentToOracleFilename,case when SentToOracle =1 then 'Yes' else 'No' end as SentToOracle,createdby,LocaleReplen,lm.LocaleID,lm.plantcode,lm.ExertisOutAccount,case when ManualOrder =1 then 'Yes' else 'No' end as ManualOrder
from mse_applereplens ar inner join mse_applelocalemapping lm on lm.localeid=ar.LocaleID
order by DateGenerated desc"></asp:SqlDataSource>


    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                   <h2>Hub stock replenishment</h2>
                    <h3>Safety stock level management</h3>

                    <asp:GridView ID="gvSafety" DataKeyNames="LocaleID" CssClass="CSSTableGenerator" runat="server" OnRowUpdating="gvSafety_RowUpdating" OnRowCancelingEdit="gvSafety_RowCancelingEdit" OnRowEditing="gvSafety_RowEditing"
                        AutoGenerateColumns="false" AutoGenerateEditButton="true">
                        <Columns>
                            <asp:BoundField ReadOnly="true" DataField="PlantCode" HeaderText="Plant Code" />
                            <asp:BoundField ReadOnly="true" DataField="PLantDescription" HeaderText="Plant Description" />
                            <asp:TemplateField>
                                <HeaderTemplate>Safety stock</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblSafety" runat="server" Text='<%# Eval("SafetyStockPercentage") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtSafety" runat="server" Text='<%# Eval("SafetyStockPercentage") %>'></asp:TextBox><asp:RegularExpressionValidator ID="revNumOnlySafety" runat="server" ControlToValidate="txtSafety" Text="Please enter a numerical value" ValidationExpression="^[0-9]*$"></asp:RegularExpressionValidator>

                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Safety stock EOL</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblSafetyEOL" runat="server" Text='<%# Eval("EOLSafetyStockPercentage") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtSafetyEOL" runat="server" Text='<%# Eval("EOLSafetyStockPercentage") %>'></asp:TextBox><asp:RegularExpressionValidator ID="revNumOnlySafetyEOL" runat="server" ControlToValidate="txtSafetyEOL" Text="Please enter a numerical value" ValidationExpression="^[0-9]*$"></asp:RegularExpressionValidator>

                                </EditItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>


                    <h3>Forecast Override management</h3>
                    <b>*The values below will be used when recommending which items be ordered.</b>
                    <asp:GridView ID="gvForecastOverride" CssClass="CSSTableGenerator" runat="server" OnRowUpdating="gvForecastOverride_RowUpdating" OnRowCancelingEdit="gvForecastOverride_RowCancelingEdit" OnRowEditing="gvForecastOverride_RowEditing"
                        AutoGenerateColumns="false" AutoGenerateEditButton="true">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>Week 1</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblWeek1" runat="server" Text='<%# Eval("Week1") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtWeek1" runat="server" Text='<%# Eval("Week1") %>'></asp:TextBox><asp:RegularExpressionValidator ID="revNumOnlySafetyWeek1" runat="server" ControlToValidate="txtWeek1" Text="Please enter a numerical value" ValidationExpression="^(1[0-2]|[1-9])$"></asp:RegularExpressionValidator>

                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Week 2</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblWeek2" runat="server" Text='<%# Eval("Week2") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtWeek2" runat="server" Text='<%# Eval("Week2") %>'></asp:TextBox><asp:RegularExpressionValidator ID="revNumOnlySafetyWeek2" runat="server" ControlToValidate="txtWeek2" Text="Please enter a numerical value" ValidationExpression="^(1[0-2]|[1-9])$"></asp:RegularExpressionValidator>

                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Week 1</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblWeek3" runat="server" Text='<%# Eval("Week3") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtWeek3" runat="server" Text='<%# Eval("Week3") %>'></asp:TextBox><asp:RegularExpressionValidator ID="revNumOnlySafetyWeek3" runat="server" ControlToValidate="txtWeek3" Text="Please enter a numerical value" ValidationExpression="^(1[0-2]|[1-9])$"></asp:RegularExpressionValidator>

                                </EditItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <h3>Stock Override management / Stock SI management</h3>
                    This allows the upload of a file containing the oracle part code and a corresponding qty which will overwirte the stock value currently being used.
                    *Please ensure the stock file is saved as a csv before uploading
                    
                    <h3>Latest Stock override File Uploaded</h3>
                    <asp:GridView ID="gvLastRunStockSiOverride" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSLastImportStockOverride" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
                                <asp:BoundField DataField="LastFileName" HeaderText="Filename" />
                                <asp:BoundField DataField="Username" HeaderText="Username" />
                                <asp:BoundField DataField="LastFileDate" DataFormatString="{0:f}" HeaderText="Import Date" />
                                <asp:BoundField DataField="dateDiffImport" HeaderText="Hours since last file received" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                            </Columns>
                       
                        </asp:GridView>
                    
                    
                    <br />
                        <asp:FileUpload ID="fuStockOverride" runat="server" /><br />
                       
                    <asp:Button ID="btnDownloadStock" runat="server" Text="Download current stock report" OnClick="btnDownloadStock_Click" />
                    <asp:Button ID="btnDownloadStockOverrideTemplate" runat="server" Text="Download template" OnClick="btnDownloadStockOverrideTemplate_Click" />-  <asp:Button ID="btntockOverride" runat="server" Text="Upload" OnClick="btntockOverride_Click" />
                    <br />
                    <br />
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Stock SI import status</th><td><asp:Label ID="lblStockSiImportStatus" runat="server"></asp:Label> </td>
                            </tr>
                        <tr>
                            <th>Enable/Disable</th><td><asp:Button ID="btnEnableStockSi" runat="server" OnClick="btnEnableStockSi_Click" Text="Enable" /> - <asp:Button ID="btnDisableStockSi" runat="server" OnClick="btnDisableStockSi_Click" Text="Disable" /> </td>

                        </tr>
                    </table>


                    <h3>Hub Order Creation</h3>
                    <table class="CSSTableGenerator">
                        <!--<tr>
    <th>Apple stock replen</th><td><asp:Button ID="btnRunStockReplen" runat="server" Text="Email replen" OnClick="btnRunStockReplen_Click" /></td>
    </tr>-->
                        <tr>
                            <th>Locale</th>
                            <th>Download replen report</th>
                            <th>Download replen report (Carton Qty)</th>
                            <th>Create replen entry</th>
                            <tr>
                                <td>Lu10- United Kingdom</td>
                                <td>
                                    <asp:Button ID="btnUkReplenReport" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="r-1" /></td>
                                <td>
                                    <asp:Button ID="btnUkReplenReportCarton" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="c-1" /></td>
                                <td>
                                    <asp:Button ID="btnUkReplenFile" runat="server" Text="Create replen" OnCommand="btnUkReplenReport_Command" CommandArgument="f-1"  /></td>
                            </tr>
                        <tr>
                            <td>Lu20- Netherlands</td>
                            <td>
                                <asp:Button ID="Button2" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="r-2" /></td>
                            <td>
                                <asp:Button ID="Button2Carton" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="c-2" /></td>
                            <td>
                                <asp:Button ID="btnNLReplenFile" runat="server" Text="Create replen" OnCommand="btnUkReplenReport_Command" CommandArgument="f-2" /></td>
                        </tr>
                        <tr>
                            <td>Lu30- Czech Republic</td>
                            <td>
                                <asp:Button ID="Button3" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="r-3" /></td>
                            <td>
                                <asp:Button ID="Button3Carton" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="c-3" /></td>
                            <td>
                                <asp:Button ID="btnCZReplenFile" runat="server" Text="Create replen" OnCommand="btnUkReplenReport_Command" CommandArgument="f-3" /></td>
                        </tr>
                        <tr>
                            <td>Lu40- Italy</td>
                            <td>
                                <asp:Button ID="Button4" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="r-4" /></td>
                            <td>
                                <asp:Button ID="Button4Carton" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="c-4" /></td>
                            <td>
                                <asp:Button ID="btnITReplenFile" runat="server" Text="Create replen" OnCommand="btnUkReplenReport_Command" CommandArgument="f-4" /></td>
                        </tr>
                        <tr>
                            <td>Lu50- United Arab Emirates</td>
                            <td>
                                <asp:Button ID="Button5" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="r-5" /></td>
                            <td>
                                <asp:Button ID="Button5Carton" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="c-5" /></td>
                            <td>
                                <asp:Button ID="btnUAEReplenFile" runat="server" Text="Create replen" OnCommand="btnUkReplenReport_Command" CommandArgument="f-5" /></td>
                        </tr>

                    </table>
                    <asp:Panel ID="pnlManualOrderCreation" runat="server" Visible="true">
                    <h3>Manual order creation</h3>
                    *This allows the creation of a manual replen order bypassing the reccomendations

                    The file to be uploaded should be Plant (LU10/20/30/40/50)/Apple Code/Qty to be ordered.
                    <br />
                    If multiple locales are included in a file these will be split into their own replen order.<br /><br />

                    <asp:FileUpload ID="fuManualOrderUpload" runat="server" /><br />
                    <asp:Button runat="server" ID="btnManualOrderUploadSubmit" Text="Upload order file" OnClick="btnManualOrderUploadSubmit_Click" />
                        <asp:Button runat="server" ID="btnManualOrderTemplate" Text="Download Template" OnClick="btnManualOrderTemplate_Click" />
                        </asp:Panel>
                    <h3>Replen entries</h3>
                    <div style="
    border: 5px solid gray;
    padding: 5px;
    background: white;
    width: 110%;
    height: 300px;
    overflow-y: scroll;">
                    <asp:GridView ID="gvReplenEntries" Style="height: 200px; overflow: auto" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSReplenEntries" DataKeyNames="ReplenID" AutoGenerateColumns="false" OnRowCommand="gvReplenEntries_RowCommand" OnRowDataBound="gvReplenEntries_RowDataBound">
                        <Columns>
                            <asp:BoundField ReadOnly="true" DataField="ReplenID" HeaderText="ReplenID" />
                            <asp:BoundField ReadOnly="true" DataField="DateGenerated" HeaderText="Date created" DataFormatString="{0:f}" />
                            <asp:BoundField ReadOnly="true" DataField="PlantCode" HeaderText="Plant Code" />
                            <asp:BoundField ReadOnly="true" DataField="ExertisOutAccount" HeaderText="Oracle Account Code" />
                            <asp:BoundField ReadOnly="true" DataField="ManualOrder" HeaderText="Manual Order" />
                            <asp:BoundField ReadOnly="true" DataField="SentToOracle" HeaderText="Submited to Oracle" />
                            <asp:BoundField ReadOnly="true" DataField="SentToOracleDate" DataFormatString="{0:f}" HeaderText="Submited to Oracle Date" />
                            <asp:BoundField ReadOnly="true" DataField="SentToOracleFilename" HeaderText="Submited to Oracle Filename" />
                            <asp:TemplateField>
                                <HeaderTemplate>Submit to Oracle</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnSubmitToOracle" runat="server" Text="Submit to Oracle" CommandName="submitReplenToOracle" CommandArgument='<%# Eval("ReplenID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Resubmit to Oracle</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnResubmitToOracle" runat="server" Text="Resubmit to Oracle" CommandName="resubmitReplenToOracle" CommandArgument='<%# Eval("ReplenID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Download Replen Lines</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnDownloadReplenLines" runat="server" Text="Download replen lines" CommandName="downloadReplenLines" CommandArgument='<%# Eval("ReplenID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Remove Replen</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnRemoveReplenLines" runat="server" Text="Remove replen" OnClientClick = "confirmReplenDelete()" CommandName="removeReplen" CommandArgument='<%# Eval("ReplenID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                    </asp:GridView>
                        </div>
                    <h3>Latest Oracle Hub order confirmation</h3>
                    <asp:GridView ID="gvAppleRangeLastOrderConfirmation" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSorscleLastRunOrderConfirmation" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
                                <asp:BoundField DataField="LastFileName" HeaderText="Filename" />
                                <asp:BoundField DataField="LastFileDate" DataFormatString="{0:f}" HeaderText="Import Date" />
                                <asp:BoundField DataField="dateDiffImport" HeaderText="Hours since last file received" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                            </Columns>
                       
                        </asp:GridView>
                    <h3>Latest Oracle Hub ASNs</h3>
                    <asp:GridView ID="gvAppleRangeLastOrderASN" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSorscleLastRunASN" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
                                <asp:BoundField DataField="LastFileName" HeaderText="Filename" />
                                <asp:BoundField DataField="LastFileDate" DataFormatString="{0:f}" HeaderText="Import Date" />
                                <asp:BoundField DataField="dateDiffImport" HeaderText="Hours since last file received" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                            </Columns>

                        </asp:GridView>
                    <br />
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download scheduled/in transit shipments</th><td><asp:Button ID="btnDownloadShipments" runat="server" Text="Download in transit/scheduled shipments" OnClick="btnDownloadShipments_ClickNew" /></td>
                        </tr>
                    </table>
                    </div>
                </div>

        </div>
        </div></asp:Content>