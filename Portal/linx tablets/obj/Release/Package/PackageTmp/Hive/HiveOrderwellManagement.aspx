<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="HiveOrderwellManagement.aspx.cs" Inherits="linx_tablets.Hive.HiveOrderwellManagement" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

   
    



    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Hive Orderwell Management</h1>
                    <h2>Orders Received Today</h2>
                    <asp:GridView ID="gvOrdersSummary" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="EmailID"      HeaderText ="Email ID" />
                            <asp:BoundField DataField="Filename"     HeaderText ="Report Filename" />
                            <asp:BoundField DataField="DateImported" HeaderText ="Date Imported" DataFormatString="{0:f}" />
                            <asp:BoundField DataField="OrderCount"   HeaderText ="Order Count" />
                            <asp:BoundField DataField="Unit Count"   HeaderText ="Unit Count" />
                            <asp:BoundField DataField="Orders Ack"   HeaderText ="Units Ack" />
                        </Columns>

                    </asp:GridView>
                    <br />
                    <asp:Button runat="server" ID="btnDownloadRollingDocument" OnClick="btnDownloadRollingDocument_Click" Text="Download Rolling Order Report" />
                    &nbsp;<asp:Button runat="server" ID="Button1" OnClick="btnDownloadRollingDocument_outstanding_Click" Text="Download Rolling Order Report (Outstanding)" />
                  

                                        </div>
            </div>
        </div>
    </div>
</asp:Content>
