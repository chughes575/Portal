<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="UserUploads.aspx.cs" Inherits="linx_tablets.John_Lewis.UserUploads1" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSPortal_JohnLewis_LastUpdates" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from vw_portal_jlp_useruploadslastupdates order by lastfiledate desc"></asp:SqlDataSource>
    
    <asp:SqlDataSource ID="sqlDSPortalLastLeadTime" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_portalforecastreportmanagement 
where reportid=35
order by lastfiledate desc"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>User Uploads</h1>

                    <h2>Last Import/Upload updates</h2>
                    <asp:GridView ID="gvPortal_JohnLewis_LastUpdate" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSPortal_JohnLewis_LastUpdates" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedForecastPortal_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
                            <asp:BoundField DataField="Username" HeaderText="Uploaded/Imported by" />
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

                    <h2>Product Range Management</h2>
                    <br>
                    (Purge and replace upload)
                    <ul>
                        <li>Three columns required for this upload </li>
                        <li>ExertisProductCode - This is the Primary Exertis product code</li>
                        <li>CustomerSku - If you wish to override the current top ranked customer sku mapping from Oracle</li>
                        <li>Active - Products status</li>
                    </ul>

                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing products</th>
                            <th>Download blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadProdctRange" runat="server" Text="Download" OnClick="btnDownloadProdctRange_Click" /></td>
                            <td>
                                <asp:Button ID="btnDownloadProductRangeTemplate" runat="server" Text="Download" OnClick="btnDownloadProductRangeTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br /> 
                    <asp:FileUpload ID="fuProductRange" runat="server" /><br />
                    <asp:Button ID="btnUploadProductRange" runat="server" Text="Upload" OnClick="btnUploadProductRange_Click" />

                    
                                        <h2>Forecast Maintenance</h2>
                     <h3>Forecast Weeks Used Management</h3>

                        No# Of Weeks Forecast used:
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
                            </asp:DropDownList> <asp:Button ID="btnUpdateForecastUsed" runat="server" OnClick="btnUpdateForecastWeeksUsed_Click" Text="Update" />
                    <br />
                    <br />
                     Current Forecast Week: <asp:Label ID="lblCurrentForecastWeek" runat="server" ForeColor="Green"></asp:Label>
                    <ul>
                        <li>Forecasts can be input/uploaded for weeks 1-52 in the current calendar year for any given product. </li>
                        <li>Upon uploading any existing forecast entries for a product will be replaced with the values supplied in the file. E.g if week 1 is uploaded with a empty/blank value any existing week 1 forecast value will be removed</li>
                        <li>If you wish to completely remove a products forecast data (weeks 1-52) pupulate column B (Delete) with 'D'. </li>
                    </ul>
                    <h3>Forecast Upload</h3>
                    <br />
                    

                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing forecast</th>
                            <th>Dowload blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadForecast" runat="server" Text="Download" OnClick="btnDownloadForecast_Click" /></td>
                            <td>
                                <asp:Button ID="btnDownloadForecastTemplate" runat="server" Text="Download" OnClick="btnDownloadForecastTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fupForecast" runat="server" /><br />
                    <asp:Button ID="btnUploadForecast" runat="server" Text="Upload" OnClick="btnUploadForecast_Click" />

                    <h3>Vendor Lead Time Management</h3>
                    *Vendor entries are prepopulated from the product range. All the suppliers that belong to the product range are automatiocally populated in this section.
                    <h3>Last Product Lead Time Update</h3>
                    <asp:GridView ID="gvportalLeadTimesLastUpdate" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSPortalLastLeadTime" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
                            <asp:BoundField DataField="LastFileName" HeaderText="Filename" />
                            <asp:BoundField DataField="LastFileDate" DataFormatString="{0:f}" HeaderText="Import Date" />
                            <asp:BoundField DataField="dateDiffImport" HeaderText="Hours since last file received" />
                            <asp:BoundField DataField="Username" HeaderText="Uploaded by" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                        </asp:GridView>
                    <br />
                    <asp:FileUpload ID="fuVendorLeadTime" runat="server" /><br />

                    <asp:Button ID="btnProductLeadTimeUpload" runat="server" Text="Upload" OnClick="btnProductLeadTimeUpload_Click" />
                    &nbsp;
                     <asp:Button ID="btnVendorLeadTimeDownload" runat="server" Text="Download existing vendor lead times" OnClick="btnVendorLeadTimeDownload_Click" />
               
                    <h2>Vendor Contacts Management</h2>
                    <br>
                    <ul>
                        <li>Use the download existing vendors button below to pull a list of all Oracle Vendors that appear in the current stock status file</li>
                        <li>You can then add any relevant contact information to an entry in this file.</li>
                        <li>Please ensdure the vendor name remains the same as in the download file.</li>
                    </ul>

                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing vendors</th>
                            <th>Dowload blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadVendorData" runat="server" Text="Download" OnClick="btnDownloadVendorData_Click" /></td>
                            <td>
                                <asp:Button ID="btnDownloadVndorTemplate" runat="server" Text="Download" OnClick="btnDownloadVndorTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fuVendorData" runat="server" /><br />
                    <asp:Button ID="btnUploadVendorData" runat="server" Text="Upload" OnClick="btnUploadVendorData_Click" />
                     </div>
            </div>
        </div>
    </div>
</asp:Content>
