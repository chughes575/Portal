﻿<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="ProductStatusDashboard.aspx.cs" Inherits="linx_tablets.SDG.Public.CustomerView" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <script type="text/javascript">



        jQuery(document).ready(function ($) {
            //jQuery('#MainContent_gvBundleSuggestions').tableScroll({ height: 550, flush: false });




            $("#MainContent_gvCustomerViewResults").tablesorter();


        });

        </script>
    <asp:SqlDataSource ID="sqlDSorscleLastRunPoStock" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement 
where reportid in (2,3)
order by lastfiledate desc"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h2>Product Status Dashboard</h2>
                    <div id="dvClaimHeader" class="gridviewHeader" runat="server">
                        <asp:DropDownList ID="ddlStockStatusGV_FilterAccountManager" Width="220px" CssClass="filter-field" runat="server" />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterManufacturer" Width="220px" CssClass="filter-field" runat="server" />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterProductType" Width="220px" CssClass="filter-field" runat="server" />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterFcastSellThru" Width="220px" CssClass="filter-field" runat="server" >
                            <asp:ListItem Text="Show Sell Thru (Wk1-4)" Value="0"></asp:ListItem>
                            <asp:ListItem Text="Show Forecast (Wk1-4)" Value="1"></asp:ListItem>
                            <asp:ListItem Text="Show Forecast (Wk1-4) & Sell Thru (Wk1-4)" Value="2"></asp:ListItem>
                            
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStockStatusGV_FilterBusinessArea" Visible="false" Width="220px" CssClass="filter-field" runat="server" />

                        <br />
                        <br />
                        <asp:DropDownList ID="ddlStockStatusGV_FilterExertisStock" Width="220px" CssClass="filter-field" runat="server">
                            <asp:ListItem Text="Exertis Stock" Selected="True" Value="All"></asp:ListItem>
                            <asp:ListItem Text="In Stock" Value="Zero"></asp:ListItem>
                            <asp:ListItem Text="Out Of Stock" Value="Not Zero"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStockStatusGV_FilterExertisPO" Width="220px" CssClass="filter-field" runat="server">
                            <asp:ListItem Text="Exertis PO" Selected="True" Value="All"></asp:ListItem>
                            <asp:ListItem Text="Yes" Value="Has Pos"></asp:ListItem>
                            <asp:ListItem Text="No" Value="No Pos"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStockStatusGV_FilterCustomerOrders" Width="220px" CssClass="filter-field" runat="server">
                            <asp:ListItem Text="Current Orders" Selected="True" Value="All"></asp:ListItem>
                            <asp:ListItem Text="Yes" Value="Backordered"></asp:ListItem>
                            <asp:ListItem Text="No" Value="No Backorders"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStockStatusGV_FilterStockByWeeks" Visible="false" Width="220px" CssClass="filter-field" runat="server">
                            <asp:ListItem Text="Stock By Weeks" Selected="True" Value="All"></asp:ListItem>
                            <asp:ListItem Text="Less 4 Wks" Value="4"></asp:ListItem>
                            <asp:ListItem Text="4wks to 7wks" Value="7"></asp:ListItem>
                            <asp:ListItem Text="8wks +" Value="8"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStockStatusGV_FilterSafetyRating" Visible="true" Width="220px" CssClass="filter-field" runat="server">
                            <asp:ListItem Text="Safety Rating" Selected="True" Value="All"></asp:ListItem>
                            <asp:ListItem Text="Red" Value="Red"></asp:ListItem>
                            <asp:ListItem Text="Green" Value="Green"></asp:ListItem>
                            <asp:ListItem Text="Amber" Value="Amber"></asp:ListItem>
                            <asp:ListItem Text="Grey/N/A" Value="grey"></asp:ListItem>
                        </asp:DropDownList>
                        <br />
                        <br />

                        <asp:Button ID="btnFilterReport" runat="server" Text="Filter results" OnClick="btnFilterReport_Click" />
                        &nbsp;<asp:Button ID="btnDownloadFilteredResults" runat="server" Text="Download results" Enabled="false" OnClick="btnDownloadFilteredResults_Click" />
                      
                    </div>
                    <br />
                    <h5>Table legend (Safety Rating)</h5>
                    <table>
                        <tr>
                            <td style="background-color:#ff0000">Red</td>
                            <td style="padding-left:7px"> Less than 3 Weeks Cover</td>
                        </tr>
                        <tr>
                            <td style="background-color:#00cc66">Green</td>
                            <td style="padding-left:7px"> Between 4 and 7 Weeks Cover</td>
                        </tr>
                        <tr>
                            <td style="background-color:#ffcc00">Amber</td>
                            <td style="padding-left:7px"> 8 Weeks Or More Cover</td>
                        </tr>
                        <tr>
                            <td style="background-color:#b7aeae"> </td>
                            <td style="padding-left:7px"> No Current Allocated Orders/No Forecast/No Sell Through</td>


                            
                        </tr>
                    </table>
                    <br />
                    <h5>Table legend (Sales/Fcst %)</h5>
                    <br />
                    <table>
                        <tr>
                            <td >Positive % E.g 132%</td>
                            <td style="padding-left:7px">Sales exceeded the forecast (Sales were 132% of the forecast)</td>
                        </tr>
                        <tr>
                            <td >Negative % E.g -10%</td>
                            <td style="padding-left:7px">Sales were less than the forest (Sales were 90% of the forecast)</td>
                        </tr>
                        </table>
                    <br />
                    Weeks Forecast Used: <asp:Label ID="lblWeeksForecastUsed" runat="server"></asp:Label><br />
                    Days Into Current Week: <asp:Label ID="lblCurrWeekDays" runat="server"></asp:Label><br />
                    <br />
                    <h3>Search Results Summary (Totals are adjusted based on search criteria selected)</h3>
                    Total Qty: <asp:Label ID="lblTotalQty" runat="server"></asp:Label>
                    Total Value:£ <asp:Label ID="lblTotalValue" runat="server"></asp:Label>
                    <div id="DivRoot" align="left">
                        <div style="overflow: hidden;" id="DivHeaderRow">
                        </div>

                        <div style="overflow: scroll;" onscroll="OnScrollDiv(this)" id="DivMainContent">
=
                            <asp:GridView ID="gvCustomerViewResults" AllowSorting="False" OnPreRender="gvCustomerViewResults_PreRender" OnDataBound="gvCustomerViewResults_DataBound" CssClass="CSSTableGenerator" PageSize="100" AllowPaging="False" OnPageIndexChanging="gvProductStockStatus_PageIndexChanging" EmptyDataText="No matching records found" runat="server">
                            
                            </asp:GridView>
                        </div>
                        <div id="DivFooterRow" style="overflow: hidden">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>