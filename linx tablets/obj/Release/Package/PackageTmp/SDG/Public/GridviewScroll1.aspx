<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="GridviewScroll1.aspx.cs" Inherits="linx_tablets.SDG.Public.GridviewScroll1" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <script type="text/javascript" src="http://code.jquery.com/jquery-2.1.1.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            /*Code to copy the gridview header with style*/
            var gridHeader = $('#<%=GridView1.ClientID%>').clone(true);
            /*Code to remove first ror which is header row*/
            $(gridHeader).find("tr:gt(0)").remove();
            $('#<%=GridView1.ClientID%> tr th').each(function (i) {
                /* Here Set Width of each th from gridview to new table th */
                $("th:nth-child(" + (i + 1) + ")", gridHeader).css('width', ($(this).width()).toString() + "px");
            });
            $("#controlHead").append(gridHeader);
            $('#controlHead').css('position', 'absolute');
            $('#controlHead').css('top', $('#<%=GridView1.ClientID%>').offset().top);

        });
    </script>

    <asp:SqlDataSource ID="sqlds" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="exec [sp_portal_Searchcustomerview1]"></asp:SqlDataSource>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div style="width: 1500px;">
                    <div id="controlHead">
                    </div>
                    <div style="height: 600px; overflow: auto">
                        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="True" DataSourceID="sqlds" EmptyDataText="There are no data records to display."
                            BorderStyle="Solid">

                           
                            <HeaderStyle BackColor="#66CCFF" />
                        </asp:GridView>
                    </div>
                </div>

            </div>



        </div>
    </div>
</asp:Content>
