<%@ Page Language="C#"  MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="UserUploads.aspx.cs" Inherits="linx_tablets.SDG.UserUploads" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="SELECT case when reportid in (7,8,11) then  datediff(HOUR,DATEADD(month, DATEDIFF(month, 0, getdate()), 0),lastfiledate) else datediff(hour,lastfiledate,getdate()) end as dateDiffImport,*
from mse_portalforecastreportmanagement
where CustomerID=1 and UserPopulated=1 order by lastfiledate desc"></asp:SqlDataSource>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>User Uploads</h1>

                    <h2>Last Import/Upload updates</h2>
                    <asp:GridView ID="gvKewillProductStockStatusLastUpdate" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSKewillProductStockStatusReport" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedForecastPortal_RowDataBound">
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

                    <h2>SDG Product Range</h2>
                    <br>
                    <ul>
                        <li>The product range is based of the cat_no, this is how the range is mapped to entries in the product stock status report from Kewill.</li>
                    </ul>

                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing range</th>
                            <th>Dowload blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadProductRange" runat="server" Text="Download" OnClick="btnDownloadProductRange_Click" /></td>
                            <td>
                                <asp:Button ID="btnDownloadProductRangeTemplate" runat="server" Text="Download" OnClick="btnDownloadProductRangeTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fuProductRange" runat="server" /><br />
                    <asp:Button ID="btnUploadProductRange" runat="server" Text="Upload" OnClick="btnUploadProductRange_Click" />

                    <h2>SDG Forecast Management</h2>
                    <h3>SDG Forecast Upload</h3>
                    Current Forecast Week: <asp:Label ID="lblCurrentForecastWeek" runat="server" ForeColor="Green"></asp:Label>
                    <br />
                    <br>
                    <ul>
                        <li>The forecast can be input/uploaded for weeks 1-52 in the current calendar year for any given product. </li>
                        <li>Upon uploading any existing forecast entries for a product will be replaced with the values supplied in the file. E.g if week 1 is uploaded with a empty/blank value any existing week 1 forecast value will be removed</li>
                        <li>If you wish to completely remove a products forecast data (weeks 1-52) pupulate column B (Delete) with 'D'. </li>
                    </ul>

                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing forecast</th>
                            <th>Dowload blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadForecast" runat="server" Text="Download" OnClick="btnDownloadForecast_Click" /></td>
                            <td>
                                <asp:Button ID="btnDownloadRangeTemplate" runat="server" Text="Download" OnClick="btnDownloadForecastTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fupForecast" runat="server" /><br />
                    <asp:Button ID="btnUploadForecast" runat="server" Text="Upload" OnClick="btnUploadForecast_Click" />

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


                    <h2>SDG Account Manager Management</h2>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing Account Manager Data</th>
                            <th>Dowload blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadAccountManagerExisting" runat="server" Text="Download" OnClick="btnDownloadAccountManagerExisting_Click" /></td>
                            <td>
                                <asp:Button ID="btnDownloadAccountManagerTemplate" runat="server" Text="Download" OnClick="btnDownloadAccountManagerTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fuAccountManager" runat="server" /><br />
                    <asp:Button ID="btrnUploadAccountManagerData" runat="server" Text="Upload" OnClick="btrnUploadAccountManagerData_Click" />

                    
                    <asp:Panel ID="pnlAccountManagerGV" runat="server" Visible="false">
                    <asp:GridView ID="gvPortalAccountManagers" DataKeyNames="business_AreaID" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false" OnRowEditing="gvPortalAccountManagers_RowEditing" OnRowUpdating="gvPortalAccountManagers_RowUpdating" OnRowCancelingEdit="gvPortalAccountManagers_RowCancelingEdit" AutoGenerateEditButton="true">
                        <Columns>
                            <asp:BoundField DataField="business_AreaID" ReadOnly="true" HeaderText="Business Area ID" />
                            <asp:BoundField DataField="Business_Area" ReadOnly="true" HeaderText="Business Area" />
                            <asp:BoundField DataField="TopCat" ReadOnly="true" HeaderText="Top Cat" />
                            <asp:TemplateField>
                                <HeaderTemplate>Exertis Account Manager</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblExertisAccountManager" runat="server" Text='<%# Eval("ExertisAccountManager") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtExertisAccountManager" runat="server" Text='<%# Eval("ExertisAccountManager") %>'></asp:TextBox>
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Exertis Contact Email Address</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblExertisContactEmailAddress" runat="server" Text='<%# Eval("ExertisContactEmailAddress") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtExertisContactEmailAddress" runat="server" Text='<%# Eval("ExertisContactEmailAddress") %>'></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="regExpExertisAcctMngerEmail" runat="server" ControlToValidate="txtContactEmailAddress" ValidationExpression="[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?" ErrorMessage="Please enter a valid email address"></asp:RegularExpressionValidator>
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Exertis Contact Telephone</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblExertisContactTelephone" runat="server" Text='<%# Eval("ExertisContactTelephone") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtExertisContactTelephone" runat="server" Text='<%# Eval("ExertisTelephone") %>'></asp:TextBox>
                                </EditItemTemplate>
                            </asp:TemplateField>
			    
			    
			    
			    <asp:TemplateField>
                                <HeaderTemplate>SDG Account Manager</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblSDGAccountManager" runat="server" Text='<%# Eval("SDGAccountManager") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtSDGAccountManager" runat="server" Text='<%# Eval("SDGAccountManager") %>'></asp:TextBox>
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>SDG Contact Email Address</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblSDGContactEmailAddress" runat="server" Text='<%# Eval("SDGContactEmailAddress") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtSDGContactEmailAddress" runat="server" Text='<%# Eval("SDGContactEmailAddress") %>'></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="regExpSDGAcctMngerEmail" runat="server" ControlToValidate="txtContactEmailAddress" ValidationExpression="[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?" ErrorMessage="Please enter a valid email address"></asp:RegularExpressionValidator>
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>SDG Contact Telephone</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblSDGContactTelephone" runat="server" Text='<%# Eval("SDGContactTelephone") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtSDGContactTelephone" runat="server" Text='<%# Eval("SDGTelephone") %>'></asp:TextBox>
                                </EditItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView></asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </asp:Content>