<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ProductStatusDashboard.aspx.cs" Inherits="linx_tablets.SDG.ProductStockStatusSearch" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">

    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_portalforecastreportmanagement 
where customerid=1
order by lastfiledate desc"></asp:SqlDataSource>




    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Product Status Dashboard</h1>
                    <h2></h2>
                    <h2>Product Stock Status Report Search</h2>
                    <div id="dvClaimHeader" class="gridviewHeader" runat="server">
                        <asp:DropDownList ID="ddlStockStatusGV_FilterBusinessArea" Width="220px" CssClass="filter-field" runat="server" />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterBuyer" Width="220px" CssClass="filter-field" runat="server" />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterAccountManager" Width="220px" CssClass="filter-field" runat="server" />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterManufacturer" Width="220px" CssClass="filter-field" runat="server" />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterProductType" Width="220px" CssClass="filter-field" runat="server" />

                        
                        <asp:DropDownList ID="ddlStockStatusGV_FilterExertisLive" Width="220px" CssClass="filter-field" runat="server" >
                            <asp:ListItem Text="All Exertis Live/Not Live" Value="All"></asp:ListItem>
                            <asp:ListItem Text="Live" Value="Live"></asp:ListItem>
                            <asp:ListItem Text="Not Live" Value="Not Live"></asp:ListItem>
                        </asp:DropDownList>
                        <br />
                        Qty exclusions (Selecting will exclude records where that metric is zero)
                        <table>
                            <tr>

                                <td style="padding-right: 20px;"></td>
                                <th style="padding-right: 20px;">Exertis Stock</th>
                                <th style="padding-right: 20px;">Exertis PO</th>
                                <th style="padding-right: 20px;">Backorders</th>
                                <th style="padding-right: 20px;">Vendor LT</th>
                                <th style="padding-right: 20px;">7 Day Runrate</th>
                                <th style="padding-right: 20px;">Stock By Weeks</th>
                                <th style="padding-right: 20px;">Aged Stock Qty</th>
                            </tr>
                            <tr>
                                <td style="padding-right: 4px;"></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnExertisStock" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="Zero" Value="Zero"></asp:ListItem>
                                        <asp:ListItem Text="Not Zero" Value="Not Zero"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnExertisPO" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="Zero" Value="Zero"></asp:ListItem>
                                        <asp:ListItem Text="Not Zero" Value="Not Zero"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnBackOrders" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="Zero" Value="Zero"></asp:ListItem>
                                        <asp:ListItem Text="Not Zero" Value="Not Zero"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnVendorLT" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="Zero" Value="Zero"></asp:ListItem>
                                        <asp:ListItem Text="Not Zero" Value="Not Zero"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnRunrate" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="Zero" Value="Zero"></asp:ListItem>
                                        <asp:ListItem Text="Not Zero" Value="Not Zero"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnStockByWeeks" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="Zero" Value="Zero"></asp:ListItem>
                                        <asp:ListItem Text="Not Zero" Value="Not Zero"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                                <td style="padding-right: 4px;">
                                    <asp:RadioButtonList id="rbtnAgedStock" runat="server">
                                        <asp:ListItem Text="All" Selected="True" Value="All"></asp:ListItem>
                                        <asp:ListItem Text="Zero" Value="Zero"></asp:ListItem>
                                        <asp:ListItem Text="Not Zero" Value="Not Zero"></asp:ListItem>
                                    </asp:RadioButtonList></td>
                            </tr>
                        </table>
                        <br />
                        <asp:Button ID="btnFilterReport" runat="server" Text="Filter results" OnClick="btnFilterReport_Click" />
                        &nbsp;<asp:Button ID="btnDownloadFilteredResults" runat="server" Text="Download results" Enabled="false" OnClick="btnDownloadFilteredResults_Click" />
                    </div>
                    <asp:Panel ID="tstPanel" runat="server" ScrollBars="Both" Height="700px" Width="90%">
                        <asp:GridView ID="gvProductStockStatus" OnDataBound="gvProductStockStatus_DataBound" CssClass="CSSTableGenerator" Width="100%" PageSize="100" AllowPaging="True" OnPageIndexChanging="gvProductStockStatus_PageIndexChanging" EmptyDataText="No matching records found" runat="server">
                            </asp:GridView>
                    </asp:Panel>


                    <%--<div id="DivRoot" align="left">
                        <div style="overflow: hidden;" id="DivHeaderRow">
                        </div>

                        <div style="overflow: scroll;" onscroll="OnScrollDiv(this)" id="DivMainContent">

                            <asp:GridView ID="gvProductStockStatus" OnDataBound="gvProductStockStatus_DataBound" CssClass="CSSTableGenerator" Width="100%" PageSize="100" AllowPaging="False" OnPageIndexChanging="gvProductStockStatus_PageIndexChanging" EmptyDataText="No matching records found" runat="server">
                            </asp:GridView>
                        </div>

                        <div id="DivFooterRow" style="overflow: hidden">
                        </div>
                    </div>--%>







                </div>
            </div>
        </div>
    </div>
</asp:Content>
