<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="HivePoDashBoard.aspx.cs" Inherits="linx_tablets.Hive.VendorPO" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <script type="text/javascript" src="/Scripts/jquery.tablesorter.js"></script> 
    <script src="/Scripts/jquery.tablescroll.js"></script>
    <script src="/Scripts/jquery.tablesorter.pager.js"></script>
    <script type="text/javascript">
        function MakeStaticHeader(gridId, height, width, headerHeight, isFooter) {
            var tbl = document.getElementById(gridId);
            if (tbl) {
                var DivHR = document.getElementById('DivHeaderRow');
                var DivMC = document.getElementById('DivMainContent');
                var DivFR = document.getElementById('DivFooterRow');

                //*** Set divheaderRow Properties ****
                DivHR.style.height = (parseInt(headerHeight)) + 'px';
                DivHR.style.width = (parseInt(width) - 16) + 'px';
                DivHR.style.position = 'relative';
                DivHR.style.top = '0px';
                DivHR.style.zIndex = '10';
                DivHR.style.verticalAlign = 'top';

                //*** Set divMainContent Properties ****
                DivMC.style.width = width + 'px';
                DivMC.style.height = height + 'px';
                DivMC.style.position = 'relative';
                DivMC.style.top = -headerHeight + 'px';
                DivMC.style.zIndex = '1';

                //*** Set divFooterRow Properties ****
                DivFR.style.width = (parseInt(width) - 16) + 'px';
                DivFR.style.position = 'relative';
                DivFR.style.top = -headerHeight + 'px';
                DivFR.style.verticalAlign = 'top';
                DivFR.style.paddingtop = '2px';

                //if (isFooter) {
                //    var tblfr = tbl.cloneNode(true);
                //    tblfr.removeChild(tblfr.getElementsByTagName('tbody')[0]);
                //    var tblBody = document.createElement('tbody');
                //    tblfr.style.width = '100%';
                //    tblfr.cellSpacing = "0";
                //    tblfr.border = "0px";
                //    tblfr.rules = "none";
                //    //*****In the case of Footer Row *******
                //    tblBody.appendChild(tbl.rows[tbl.rows.length - 1]);
                //    tblfr.appendChild(tblBody);
                //    DivFR.appendChild(tblfr);
                //}
                //****Copy Header in divHeaderRow****
                DivHR.appendChild(tbl.cloneNode(true));
            }
        }



        function OnScrollDiv(Scrollablediv) {
            document.getElementById('DivHeaderRow').scrollLeft = Scrollablediv.scrollLeft;
            document.getElementById('DivFooterRow').scrollLeft = Scrollablediv.scrollLeft;
        }


    </script>
    

    
    <asp:SqlDataSource ID="sqlDSorscleLastRunPoStock" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_portalforecastreportmanagement 
where reportid in (3,25,23,
24)
order by lastfiledate desc"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Hive POs and Availability Dashboard</h1>
                    <b>Hive Current forecast / Sell through weeks used:</b> <asp:Label ID="lblWeeksUsed" runat="server"></asp:Label>
                    
                    <br />
                    <h2>Bundle Availability  Dashboard</h2>
                    <div id="dvClaimHeader" class="gridviewHeader" runat="server">
                       
                        <asp:DropDownList ID="ddlStockStatusGV_FilterExertisStock" Width="220px" CssClass="filter-field" runat="server">
                            <asp:ListItem Text="Hive Stock" Selected="True" Value="All"></asp:ListItem>
                            <asp:ListItem Text="In Stock" Value="Zero"></asp:ListItem>
                            <asp:ListItem Text="Out Of Stock" Value="Not Zero"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStockStatusGV_FilterExertisPO" Width="220px" CssClass="filter-field" runat="server">
                            <asp:ListItem Text="Exertis PO" Selected="True" Value="All"></asp:ListItem>
                            <asp:ListItem Text="Yes" Value="Has Pos"></asp:ListItem>
                            <asp:ListItem Text="No" Value="No Pos"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStockStatusGV_FilterCustomerOrders" Width="220px" CssClass="filter-field" runat="server">
                            <asp:ListItem Text="Backorders" Selected="True" Value="All"></asp:ListItem>
                            <asp:ListItem Text="Yes" Value="Backordered"></asp:ListItem>
                            <asp:ListItem Text="No" Value="No Backorders"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStockStatusGV_FilterCustomerAllocatedOrders" Width="220px" CssClass="filter-field" runat="server">
                            <asp:ListItem Text="Allocated Orders" Selected="True" Value="All"></asp:ListItem>
                            <asp:ListItem Text="Yes" Value="Allocated"></asp:ListItem>
                            <asp:ListItem Text="No" Value="No Allocated"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStockStatusGV_FilterSafetyRating" Visible="true" Width="220px" CssClass="filter-field" runat="server">
                            <asp:ListItem Text="Safety Rating (Show all)" Selected="True" Value="All"></asp:ListItem>
                            <asp:ListItem Text="Red" Value="Red"></asp:ListItem>
                            <asp:ListItem Text="Green" Value="Green"></asp:ListItem>
                            <asp:ListItem Text="Amber" Value="Amber"></asp:ListItem>
                            <asp:ListItem Text="Grey/N/A" Value="grey"></asp:ListItem>
                        </asp:DropDownList>
                        <br />
                        <br />

                        <asp:Button ID="btnFilterReport" runat="server" Text="Filter results" OnClick="btnFilterReport_Click" />
                        &nbsp;<asp:ImageButton ID="ImageButton1" runat="server" OnClick="excelImgIcon_Click" ImageUrl="~/Images/Excel-icon.png" Width="20px" />Download results
                      
                    </div>
                    <h5>Table legend (Safety Rating)</h5>
                    <table>
                        <tr>
                            <td style="background-color:#ff0000">Red</td>
                            <td style="padding-left:7px"> Between 0 and 8 Weeks Cover</td>
                        </tr>
                        <tr>
                            <td style="background-color:#ffcc00">Amber</td>
                            <td style="padding-left:7px"> Between 9 and 12</td>
                        </tr>
                        <tr>
                            <td style="background-color:#00cc66">Green</td>
                            <td style="padding-left:7px">  Weeks Cover 13 Weeks Or More</td>
                        </tr>
                        
                        <tr>
                            <td style="background-color:#b7aeae"> </td>
                            <td style="padding-left:7px"> No Current Allocated Orders/No Forecast/No Sell Through</td>


                            
                        </tr>
                    </table>
                    <br />
                    <h5>Table legend (Sell Thru / Fcst %)</h5>
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
                    <br />
                    
                     

                    <div id="DivRoot" align="left">
                        <div style="overflow: hidden;" id="DivHeaderRow">
                        </div>

                        <div style="overflow: scroll;" onscroll="OnScrollDiv(this)" id="DivMainContent">

                          <asp:GridView ID="gvBundleSuggestions"  Visible="true" runat="server" CssClass="CSSTableGenerator" OnDataBound="gvBundleSuggestions_DataBound" UseAccessibleHeader="true" Width="100%" OnPreRender="gvBundleSuggestions_PreRender"></asp:GridView>
                       


                        </div>

                        <div id="DivFooterRow" style="overflow: hidden">
                        </div>
                    </div>
                   
                    
                   
                    
                </div>
            </div>
        </div>
    </div>
</asp:Content>
