<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="Home.aspx.cs" Inherits="linx_tablets.Hive.UserUploads" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from vw_portal_hive_useruploadslastupdates order by lastfiledate desc"></asp:SqlDataSource>
    



    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Home</h1>
                    
                    <h2>Last Import/Upload updates</h2>
                    
                    <asp:GridView ID="gvKewillProductStockStatusLastUpdate" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSKewillProductStockStatusReport" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedForecastPortal_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
                            <asp:BoundField DataField="Username" HeaderText="Uploaded/Imported by" />
                            <asp:BoundField DataField="LastFileName" HeaderText="Filename" />
                            <asp:BoundField DataField="LastFileDate" DataFormatString="{0:f}" HeaderText="Import Date" />
                            <asp:BoundField DataField="dateDiffImport" HeaderText="Hours since last file received" />

                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:Image runat="server" ID="imgImportStatus" Width="20px" />
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>

                    </asp:GridView>

                    <h3>Stock Level setup </h3>
                    Use check buttons below to setup how stock levels are updated for products-<br /><br />
                    <asp:RadioButtonList runat="server" ID="rbtnlstStockSetup">
                        <asp:ListItem Value="0">  Stock SI:  Stock Levels are updated once a day.</asp:ListItem>
                        <asp:ListItem Value="1">  Stock API: Stock Levels are updated once hourly. This relies on the processing of pricat files to prompt and up to date. </asp:ListItem>
                    </asp:RadioButtonList><br />
                    <asp:Button runat="server" ID="btnUpdateStockLevels" OnClick="btnUpdateStockLevels_Click" Text="Update"/>

                                        </div>
            </div>
        </div>
    </div>
</asp:Content>
