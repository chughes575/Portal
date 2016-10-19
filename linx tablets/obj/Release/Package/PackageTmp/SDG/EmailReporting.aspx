<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="True" CodeBehind="EmailReporting.aspx.cs" Inherits="linx_tablets.SDG.EmailReporting" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <
    <script src="../scripts/OrderCancel.js" type="text/javascript"></script>
    <asp:SqlDataSource ID="sqlDSorscleLastLeadTime" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select em.Id,em.WeekNo,em.Year,cast(coalesce(pro.cnt,0) as varchar(10))+'/'+cast(coalesce(allv.cnt,0) as varchar(10)) as Processed from MSE_PortalVendorEmails em left outer join ( 
select Reportid,count(VendorID) as Cnt from MSE_PortalVendorProcessed
where processed=1
group by Reportid) as Pro on pro.reportid=em.id
left outer join ( 
select Reportid,count(VendorID) as Cnt from MSE_PortalVendorProcessed
group by Reportid) as AllV on AllV.reportid=em.id
where em.customerid=1
        order by em.year desc,em.weekno desc"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    
                    <h2>Weekly sales email management</h2>
                    The following table shows the status of the weekly report emails.<br /> A week can be expanded to view the full vendor breakdown. <br />To download all associated vendor information use the download vendors button below. <br />
                    
                        <asp:GridView ID="gvEmailReports" CssClass="CSSTableGenerator" OnRowDataBound="gvEmailReports_OnRowDataBound" OnRowCommand="gvEmailReports_RowCommand" runat="server" DataSourceID="sqlDSorscleLastLeadTime" AutoGenerateColumns="false">
                            <Columns>
                                <asp:TemplateField HeaderText="Daily breakdown">
                <ItemTemplate>
                    <a href="javascript:switchViews('div<%# Eval("ID") %>');">
                        <img id='imgdiv<%# Eval("ID") %>' alt="Click to show/hide Vendors" border="0"
                            src="../images/expand_button.png" /></a>
                </ItemTemplate>
            </asp:TemplateField>
                                <asp:BoundField DataField="ID" HeaderText="ID" />
                                <asp:BoundField DataField="WeekNo" HeaderText="WeekNo" />
                                <asp:BoundField DataField="Year" HeaderText="Year" />
                                <asp:BoundField DataField="Processed" HeaderText="Processed" />
                                <asp:TemplateField>
                                <HeaderTemplate>Download Vendors</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnDownloadReportVendors" runat="server" Text="Download Vendors" CommandName="downloadreportvendors" CommandArgument='<%# Eval("ID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                                <asp:TemplateField>
                <ItemTemplate>
                    </td></tr>
                    <tr>
                        <td colspan="100">
                            <div id='div<%# Eval("ID") %>' style="display: none; position: relative; left: 25px;">
                                <asp:GridView ID="gvReportVendors" runat="server"
                                    BackColor="ActiveCaption" AutoGenerateColumns="True">
                                </asp:GridView>
                            </div>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
