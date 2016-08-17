<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="ExertisHivePODashboard.aspx.cs" Inherits="linx_tablets.Hive.ExertisStockPO" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <script type="text/javascript" src="/Scripts/jquery.tablesorter.js"></script> 
    <script src="/Scripts/jquery.tablescroll.js"></script>
    <script src="/Scripts/jquery.tablesorter.pager.js"></script>
    
    <script type="text/javascript">



        jQuery(document).ready(function ($) {
            //jQuery('#MainContent_gvBundleSuggestions').tableScroll({ height: 550, flush: false });




            $("#MainContent_gvBundleSuggestions").tablesorter();


        });

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
                    <h1>Exertis Hive Availability Dashboard</h1>
                    <b>Exertis Current forecast / Sell through weeks used:</b> <asp:Label ID="lblWeeksUsed" runat="server"></asp:Label>
                    <br />
                    <b>Dashboard shows grouped Exertis Hive bundle sales variations by Hive Code (i.e. Exertis Manufacturer part). Stock, demand, sell through etc. will also be grouped value.</b>
                    <br /><br />
                    Dashboard download shows the breakdown by Exertis sales variations.
                    <h2>Bundle Availability  Dashboard</h2>
                    <h5>Table legend (Safety Rating)</h5>
                    <table>
                        <tr>
                            <td style="background-color:#ff0000">Red</td>
                            <td style="padding-left:7px"> Less than 3 Weeks Cover</td>
                        </tr>
                        <tr>
                            <td style="background-color:#00cc66">Green</td>
                            <td style="padding-left:7px"> Between 3 and 6 Weeks Cover</td>
                        </tr>
                        <tr>
                            <td style="background-color:#ffcc00">Amber</td>
                            <td style="padding-left:7px"> 7 Weeks Or More Cover</td>
                        </tr>
                        <tr>
                            <td style="background-color:#b7aeae"> </td>
                            <td style="padding-left:7px"> No Current Allocated Orders/No Forecast/No Sell Through</td>


                            
                        </tr>
                    </table>
                    <br />
                    <asp:ImageButton ID="excelImgIcon" runat="server" OnClick="excelImgIcon_Click" ImageUrl="~/Images/Excel-icon.png" Width="20px" /> Download Report
                    <br />
                    <asp:ImageButton ID="excelConsolidatedImgIcon" runat="server" OnClick="excelConsolidatedImgIcon_Click" ImageUrl="~/Images/Excel-icon.png" Width="20px" /> Download Report (Consolidated by Mfr Part No)
                    <br />
                    <div style="width:1300px; overflow:scroll" >
                    <asp:GridView ID="gvBundleSuggestions"  Visible="true" runat="server" CssClass="tablesorter" OnDataBound="gvBundleSuggestions_DataBound" UseAccessibleHeader="true" OnPreRender="gvBundleSuggestions_PreRender"></asp:GridView>
                        </div>
                    
                </div>
            </div>
        </div>
    </div>
</asp:Content>
