﻿<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="Home.aspx.cs" Inherits="linx_tablets.BPC.Home" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <script type="text/javascript" src="/Scripts/jquery.tablesorter.js"></script>
    <script src="/Scripts/jquery.tablescroll.js"></script>
    <script type="text/javascript">



        jQuery(document).ready(function ($) {
            $("#MainContent_gvKewillProductStockStatusLastUpdate").tablesorter();



        });
        </script>
    <asp:SqlDataSource ID="sqlDSBCWeeklyExports" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select * from MSE_BPCWeeklyExports order by ExportDate desc"></asp:SqlDataSource>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>BPC HOME</h1>

                    <h2>Last Import/Upload updates</h2>
                    <asp:GridView ID="gvBPCWeeklyExports" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSBCWeeklyExports" AutoGenerateColumns="true" >
                        

                    </asp:GridView>
                    <h2>BPC Export Management</h2>

                    There are three types of BPC exports detailed below-
                    <br />
                    <br />
                    <ul>
                        <li>Forecast Export: The product forecast for the customer is sent exported. </li>
                        <li>Sales Export (Epos): This is the sales that the customer has made of our stock based on epos data uploaded through the portal</li>
                        <li>Stock Export: This is current position of our stock that the customer holds based on epos data uploaded through the portal</li>
                        
                    </ul>
                    <br />
                    Values in green below are the Customer specific values sent to BPC to uniquely identify which customer the data relates to.
                    <asp:GridView
                        ID="gvBPCExports"
                        runat="server"
                        AutoGenerateColumns="False"
                        DataKeyNames="CustomerID" CssClass="CSSTableGenerator"
                        OnRowCancelingEdit="gvBPCExports_RowCancelingEdit" OnRowEditing="gvBPCExports_RowEditing" OnRowUpdating="gvBPCExports_RowUpdating">
                        <RowStyle BackColor="#EFF3FB" />
                        <Columns>
                            <asp:BoundField DataField="CustomerID" ReadOnly="true" HeaderText="Customer ID" />
                            <asp:BoundField DataField="CustomerName" ReadOnly="true" HeaderText="Customer Name" />
                            <asp:TemplateField HeaderText="Oracle Customer Code">
                                <ItemTemplate>
                                    <asp:Label ID="lblOracle_Customer_Code" runat="server" Text='<%# Bind("Oracle_Customer_Code") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="BPC_Customer_Code" HeaderStyle-ForeColor="DarkGreen" >
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtBPC_Customer_Code" runat="server" Text='<%# Bind("BPC_Customer_Code") %>'></asp:TextBox>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblBPC_Customer_Code" runat="server" Text='<%# Bind("BPC_Customer_Code") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Customer S" HeaderStyle-ForeColor="DarkGreen">
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtCustomer_S" runat="server" Text='<%# Bind("Customer_S") %>'></asp:TextBox>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblCustomer_S" runat="server" Text='<%# Bind("Customer_S") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Forecast Export Enabled">
                                <EditItemTemplate>
                                    <asp:CheckBox ID="chkForecastExportEdit" runat="server" Checked='<%#bool.Parse(Eval("ForecastExportEnabled").ToString())%>' />
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkForecastExport" runat="server" Enabled="false" Checked='<%#bool.Parse(Eval("ForecastExportEnabled").ToString())%>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Sales Export Enabled">
                                <EditItemTemplate>
                                    <asp:CheckBox ID="chkSalesExportEdit" runat="server" Checked='<%#bool.Parse(Eval("SalesExportEnabled").ToString())%>' />
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkSalesExport" runat="server" Enabled="false" Checked='<%#bool.Parse(Eval("SalesExportEnabled").ToString())%>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Stock Export Enabled">
                                <EditItemTemplate>
                                    <asp:CheckBox ID="chkStockExportEdit" runat="server" Checked='<%#bool.Parse(Eval("StockExportEnabled").ToString())%>' />
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkStockExport" runat="server" Enabled="false" Checked='<%#bool.Parse(Eval("StockExportEnabled").ToString())%>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ShowHeader="False">
                                <EditItemTemplate>
                                    <asp:Button ID="Button1" runat="server" CausesValidation="True" CommandName="Update" Text="Update" />&nbsp;
                    
                                    <asp:Button ID="Button2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" />

                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="EditNotesButton" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit" />

                                </ItemTemplate>
                                <ItemStyle Width="50px" />
                            </asp:TemplateField>

                        </Columns>

                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>