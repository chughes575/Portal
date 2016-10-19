<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="linx_tablets.Johnlewis.JohnLewisDashboard" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>script type="text/javascript" src="/Scripts/jquery.tablesorter.js"></script> 
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

    <link href="https://cdnjs.cloudflare.com/ajax/libs/jquery-footable/0.1.0/css/footable.min.css"
        rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/jquery-footable/0.1.0/js/footable.min.js"></script>


    <script type="text/javascript">
        $(function () {
            $('#<%=gvJohnLewisDashboard.ClientID %>').footable({
                breakpoints: {
                    phone: 480,
                    tablet: 1024
                }
            });
        });
    </script>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>JLP Availability Dashboard</h1>
                    <b>Current forecast / Sell through weeks used:</b>
                    <asp:Label ID="lblWeeksUsed" runat="server"></asp:Label>
                    <br />

                    <br />
                    <br />


                    <br />
                    <asp:ImageButton ID="excelImgIcon" runat="server" OnClick="excelImgIcon_Click" ImageUrl="~/Images/Excel-icon.png" Width="20px" />
                    Download Report
                   
                    <br />

                    <h5>Table legend (Safety Rating)</h5>
                    <table>
                        <tr>
                            <td style="background-color:#ff0000">Red</td>
                            <td style="padding-left:7px"> Less than 3 Weeks Cover</td>
                        </tr>
                        <tr>
                            <td style="background-color:#ffcc00">Green</td>
                            <td style="padding-left:7px"> Between 3 and 6</td>
                        </tr>
                        <tr>
                            <td style="background-color:#00cc66">Amber</td>
                            <td style="padding-left:7px"> 7 or More Weeks Of Cover</td>
                        </tr>
                        
                        <tr>
                            <td style="background-color:#b7aeae"> </td>
                            <td style="padding-left:7px"> No Sell Through</td>


                            
                        </tr>
                    </table>
                    <br />
                    <div id="DivRoot" align="left">
                        <div style="overflow: hidden;" id="DivHeaderRow">
                        </div>

                        <div style="overflow: scroll;" onscroll="OnScrollDiv(this)" id="DivMainContent">

                          <asp:GridView ID="gvJohnLewisDashboard" CssClass="CSSTableGenerator" runat="server" OnRowDataBound="gvBundleSuggestions_DataBound" AutoGenerateColumns="true"
                            Style="max-width: 500px">
                            
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
