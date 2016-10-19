<%@ Page Language="C#"  MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="VendorPO.aspx.cs" Inherits="linx_tablets.Reporting.VendorPO" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSorscleLastRunPoStock" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid in (2,3)
order by lastfiledate desc"></asp:SqlDataSource>


    <asp:SqlDataSource ID="sqlDSWeeksCover" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select 1 as weekval
union
select 2 as weekval
union
select 3  as weekval
union
select 4  as weekval
union
select 5  as weekval
union
select 6  as weekval
union
select 7  as weekval
union
select 8  as weekval
union
select 9  as weekval
union
select 10  as weekval
union
select 11  as weekval
union
select 12 as weekval
union 
select 13   as weekval"></asp:SqlDataSource>
      
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h2>Exertis Vendor PO Management</h2>
                    <h3>Latest Oracle Exports</h3>
                    <asp:GridView ID="gvAppleRangeLastPoStock" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSorscleLastRunPoStock" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
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
                    <h3>Vendor PO Suggestions Weeks Cover Management</h3>
                    No# of weeks cover to use when generating po suggestions. <br />
                    Forecast weeks to use: <asp:DropDownList ID="ddlForecastWeeks" DataSourceID="sqlDSWeeksCover" DataTextField="weekval" DataValueField="weekval" runat="server"></asp:DropDownList> <asp:Button ID="btnUpdateWeeks" runat="server" Text="Update" OnClick="btnUpdateWeeks_Click" />
                    <h3>Exertis vendor PO Suggestions</h3>
                    <table class="CSSTableGenerator">
                        <table class="CSSTableGenerator">
                            <tr>
                                <th>Locale</th>
                                <th>Download po suggestions report by locale</th>
                            </tr>
                            <tr>
                                <td>All locales- Consolidated</td>
                                <td>
                                    <asp:Button ID="Button11" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-0" /></td>
                            </tr>
                            <tr>
                                <td>All locales- Consolidated including non suggested products</td>
                                <td>
                                    <asp:Button ID="Button12" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-9" /></td>
                            </tr>
                            <tr>
                                <td>Lu10- United Kingdom</td>
                                <td>
                                    <asp:Button ID="Button6" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-1" /></td>
                            </tr>
                            <tr>
                                <td>Lu20- Netherlands</td>
                                <td>
                                    <asp:Button ID="Button7" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-2" /></td>
                            </tr>
                            <tr>
                                <td>Lu30- Czech Republic</td>
                                <td>
                                    <asp:Button ID="Button8" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-3" /></td>
                            </tr>
                            <tr>
                                <td>Lu40- Italy</td>
                                <td>
                                    <asp:Button ID="Button9" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-4" /></td>
                            </tr>
                            <tr>
                                <td>Lu50- United Arab Emirates</td>
                                <td>
                                    <asp:Button ID="Button10" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-5" /></td>
                            </tr>
                        </table>






                        <h3>Exertis vendor PO Suggestions Bretford products</h3>
                    <table class="CSSTableGenerator">
                        <table class="CSSTableGenerator">
                            <tr>
                                <th>Locale</th>
                                <th>Download po suggestions report by locale</th>
                            </tr>
                            <tr>
                                <td>All locales- Consolidated</td>
                                <td>
                                    <asp:Button ID="Button1Bret" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command_Bretford" CommandArgument="r-0" /></td>
                            </tr>
                            <tr>
                                <td>All locales- Consolidated including non suggested products</td>
                                <td>
                                    <asp:Button ID="Button2" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command_Bretford" CommandArgument="r-9" /></td>
                            </tr>
                            <tr>
                                <td>Lu10- United Kingdom</td>
                                <td>
                                    <asp:Button ID="Button3Bret" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command_Bretford" CommandArgument="r-1" /></td>
                            </tr>
                            <tr>
                                <td>Lu20- Netherlands</td>
                                <td>
                                    <asp:Button ID="Button4Bret" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command_Bretford" CommandArgument="r-2" /></td>
                            </tr>
                            <tr>
                                <td>Lu30- Czech Republic</td>
                                <td>
                                    <asp:Button ID="Button5Bret" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command_Bretford" CommandArgument="r-3" /></td>
                            </tr>
                            <tr>
                                <td>Lu40- Italy</td>
                                <td>
                                    <asp:Button ID="Button13Bret" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command_Bretford" CommandArgument="r-4" /></td>
                            </tr>
                            <tr>
                                <td>Lu50- United Arab Emirates</td>
                                <td>
                                    <asp:Button ID="Button14Bret" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command_Bretford" CommandArgument="r-5" /></td>
                            </tr>
                        </table>
                    </div>
                </div>

        </div>
        </div></asp:Content>