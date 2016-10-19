<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="linx_tablets.Argos.ArgosDashboard" %>


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


    <link href="https://cdnjs.cloudflare.com/ajax/libs/jquery-footable/0.1.0/css/footable.min.css"
        rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/jquery-footable/0.1.0/js/footable.min.js"></script>


    <script type="text/javascript">
        $(function () {
            $('#<%=gvArgosDashboard.ClientID %>').footable({
                breakpoints: {
                    phone: 480,
                    tablet: 1024
                }
            });
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
                    <h1>Argos Availability Dashboard</h1>
                    <b>Current forecast / Sell through weeks used:</b>
                    <asp:Label ID="lblWeeksUsed" runat="server"></asp:Label>
                    <br />

                    <br />
                    <br />


                    <br />
                    <asp:ImageButton ID="excelImgIcon" runat="server" OnClick="excelImgIcon_Click" ImageUrl="~/Images/Excel-icon.png" Width="20px" />
                    Download Report
                   
                    <br />
                    <div style="width: 100%; height: 700px; overflow: scroll">
                        <asp:GridView ID="gvArgosDashboard" CssClass="footable" runat="server" AutoGenerateColumns="true"
                            Style="max-width: 500px">
                           <%-- <Columns>
                                <asp:BoundField DataField="Argos Sku" HeaderText="Argos Sku" />
                                <asp:BoundField DataField="Exertis Sku" HeaderText="Exertis Sku" />
                                <asp:BoundField DataField="Description" HeaderText="Description" />
                                <asp:BoundField DataField="Exertis SOH" HeaderText="Exertis SOH" />
                                <asp:BoundField DataField="Exertis BO " HeaderText="Exertis BO " />
                                <asp:BoundField DataField="Exertis Alloc" HeaderText="Exertis Alloc" />
                                <asp:BoundField DataField="Argos Demand (Fcast)" HeaderText="Argos Demand (Fcast)" />
                                <asp:BoundField DataField="Argos Demand (Sell thru)" HeaderText="Argos Demand (Sell thru)" />
                                <asp:BoundField DataField="OOS PO" HeaderText="OOS PO" />
                                <asp:BoundField DataField="Sugg PO Qty" HeaderText="Sugg PO Qty" />
                                <asp:BoundField DataField="Argos Stock Units" HeaderText="Argos Stock Units" />
                                <asp:BoundField DataField="Argos Wks Cover (Stock + Overdue)" HeaderText="Argos Wks Cover (Stock + Overdue)" />
                                <asp:BoundField DataField="Argos Wks Cover (Stock Only)" HeaderText="Argos Wks Cover (Stock Only)" />
                                <asp:BoundField DataField="Argos Demand" HeaderText="Argos Demand" />
                                <asp:BoundField DataField="Argos Sales" HeaderText="Argos Sales" />
                                <asp:BoundField DataField="Argos Overdue Orders Units" HeaderText="Argos Overdue Orders Units" />
                                <asp:BoundField DataField="Argos Orders OOS Incl Overdue" HeaderText="Argos Orders OOS Incl Overdue" />
                                <asp:BoundField DataField="Argos Serviceability %" HeaderText="Argos Serviceability %" />
                                <asp:BoundField DataField="Manuf Part Code" HeaderText="Manuf Part Code" />
                                <asp:BoundField DataField="Vendor Name" HeaderText="Vendor Name" />
                                <asp:BoundField DataField="Top Cat" HeaderText="Top Cat" />
                                <asp:BoundField DataField="PO #1" HeaderText="PO #1" />
                                <asp:BoundField DataField="PO #1 No" HeaderText="PO #1 No" />
                                <asp:BoundField DataField="PO #1 Date" HeaderText="PO #1 Date" />
                                <asp:BoundField DataField="PO #2" HeaderText="PO #2" />
                                <asp:BoundField DataField="PO #2 No" HeaderText="PO #2 No" />
                                <asp:BoundField DataField="PO #2 Date" HeaderText="PO #2 Date" />
                                <asp:BoundField DataField="PO #1" HeaderText="PO #1" />
                                <asp:BoundField DataField="PO #3 No" HeaderText="PO #3 No" />
                                <asp:BoundField DataField="PO #3 Date" HeaderText="PO #3 Date" />
                                <asp:BoundField DataField="602 ACTON GATE DC Stock Units" HeaderText="602 ACTON GATE DC Stock Units" />
                                <asp:BoundField DataField="602 ACTON GATE DC Wks Cover (Stock + Overdue)" HeaderText="602 ACTON GATE DC Wks Cover (Stock + Overdue)" />
                                <asp:BoundField DataField="602 ACTON GATE DC Wks Cover (Stock Only)" HeaderText="602 ACTON GATE DC Wks Cover (Stock Only)" />
                                <asp:BoundField DataField="605 MOSSEND Stock Units" HeaderText="605 MOSSEND Stock Units" />
                                <asp:BoundField DataField="605 MOSSEND Wks Cover (Stock + Overdue)" HeaderText="605 MOSSEND Wks Cover (Stock + Overdue)" />
                                <asp:BoundField DataField="605 MOSSEND Wks Cover (Stock Only)" HeaderText="605 MOSSEND Wks Cover (Stock Only)" />
                                <asp:BoundField DataField="607 BRIDGWATER Stock Units" HeaderText="607 BRIDGWATER Stock Units" />
                                <asp:BoundField DataField="607 BRIDGWATER Wks Cover (Stock + Overdue)" HeaderText="607 BRIDGWATER Wks Cover (Stock + Overdue)" />
                                <asp:BoundField DataField="607 BRIDGWATER Wks Cover (Stock Only)" HeaderText="607 BRIDGWATER Wks Cover (Stock Only)" />
                                <asp:BoundField DataField="609 MAGNA PARK Stock Units" HeaderText="609 MAGNA PARK Stock Units" />
                                <asp:BoundField DataField="609 MAGNA PARK Wks Cover (Stock + Overdue)" HeaderText="609 MAGNA PARK Wks Cover (Stock + Overdue)" />
                                <asp:BoundField DataField="609 MAGNA PARK Wks Cover (Stock Only)" HeaderText="609 MAGNA PARK Wks Cover (Stock Only)" />
                                <asp:BoundField DataField="612 BASILDON Stock Units" HeaderText="612 BASILDON Stock Units" />
                                <asp:BoundField DataField="612 BASILDON Wks Cover (Stock + Overdue)" HeaderText="612 BASILDON Wks Cover (Stock + Overdue)" />
                                <asp:BoundField DataField="612 BASILDON Wks Cover (Stock Only)" HeaderText="612 BASILDON Wks Cover (Stock Only)" />
                                <asp:BoundField DataField="615 HEYWOOD Stock Units" HeaderText="615 HEYWOOD Stock Units" />
                                <asp:BoundField DataField="615 HEYWOOD Wks Cover (Stock + Overdue)" HeaderText="615 HEYWOOD Wks Cover (Stock + Overdue)" />
                                <asp:BoundField DataField="615 HEYWOOD Wks Cover (Stock Only)" HeaderText="615 HEYWOOD Wks Cover (Stock Only)" />
                                <asp:BoundField DataField="616 BARTON Stock Units" HeaderText="616 BARTON Stock Units" />
                                <asp:BoundField DataField="616 BARTON Wks Cover (Stock + Overdue)" HeaderText="616 BARTON Wks Cover (Stock + Overdue)" />
                                <asp:BoundField DataField="616 BARTON Wks Cover (Stock Only)" HeaderText="616 BARTON Wks Cover (Stock Only)" />
                                <asp:BoundField DataField="633 Castleford Stock Units" HeaderText="633 Castleford Stock Units" />
                                <asp:BoundField DataField="633 Castleford Wks Cover (Stock + Overdue)" HeaderText="633 Castleford Wks Cover (Stock + Overdue)" />
                                <asp:BoundField DataField="633 Castleford Wks Cover (Stock Only)" HeaderText="633 Castleford Wks Cover (Stock Only)" />


                            </Columns>--%>
                        </asp:GridView>
                    </div>



                </div>
            </div>
        </div>
    </div>
</asp:Content>
