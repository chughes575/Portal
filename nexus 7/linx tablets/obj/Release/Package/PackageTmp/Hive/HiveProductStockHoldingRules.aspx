<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="HiveProductStockHoldingRules.aspx.cs" Inherits="linx_tablets.Hive.HiveProductStockHoldingRules" %>


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
                    <h1>Hive Product Stock Holding Rules</h1>
                    Hive stock holding suggestions are based on forecasts and sell through data available against Hive and Exertis.
                    <br />
                     If there is no forecast or they are lower than sell through Portal will suggest stock holding based on sell through.   
                    <br /><br />
                    Suggestions will work off the total amount of Forecast weeks recorded on Portal. 
                    <br /><br />
                     The same amount of weeks set up will be used for Sell through data if there are no forecasts available.
Bundle and Component Set up is available

                    <h2>Bundle Forecast / Sell Through No# Weeks Used Set Up (Bundling Suggestions)</h2>
                    Exertis Hive No # of weeks used:
                      
                     <asp:DropDownList ID="ddlForecastAmountUsedBundlesExertisHive" runat="server">
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
                            </asp:DropDownList> <asp:Button ID="btnUpdateForecastUsedBundlesExertisHive" runat="server" OnClick="btnUpdateForecastUsedBundlesExertisHive_Click" Text="Update" />
                    <br />
                    <br />
                    Hive No# Of Weeks Forecast used:
                        <asp:DropDownList ID="ddlForecastAmountUsedBundlesExertis3PL" runat="server">
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
                            </asp:DropDownList> <asp:Button ID="btnUpdate3PLForecastWeeksUsedBundles" runat="server" OnClick="btnUpdate3PLForecastWeeksUsedBundles_Click" Text="Update" />


                    <h2>Hive Component Forecast / Sell Through No# Weeks Used Set Up (Supplier PO Suggestions)</h2>

                    <asp:Panel ID="pnlExertisHiveWeeksUsed" Visible="false" runat="server">
                    
                     Exertis Hive No # of Weeks used:
                        <asp:DropDownList ID="ddlForecastAmountUsedExertisHive" runat="server">
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
                            </asp:DropDownList> <asp:Button ID="btnUpdateExertisHiveForecastWeeksUsed" runat="server" OnClick="btnUpdateExertisHiveForecastWeeksUsed_Click" Text="Update" />
                    </asp:Panel><br />
                     Hive No # of Weeks used:
                        <asp:DropDownList ID="ddlForecastAmountUsedExertis3PL" runat="server">
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
                            </asp:DropDownList> <asp:Button ID="btnUpdate3PLForecastWeeksUsed" runat="server" OnClick="btnUpdate3PLForecastWeeksUsed_Click" Text="Update" />




                    
                    
                    
                    </div>
                </div>
            </div>
        </div>
    </asp:Content>