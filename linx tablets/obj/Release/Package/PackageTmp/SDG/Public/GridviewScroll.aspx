<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="GridviewScroll.aspx.cs" Inherits="linx_tablets.SDG.Public.GridviewScroll" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <style type="text/css">
        .FixedHeader {
            position: absolute;
            font-weight: bold;
            vertical-align:text-bottom;
        }      
    </style>

    <asp:SqlDataSource ID="sqlds" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="exec [sp_portal_Searchcustomerview1]"></asp:SqlDataSource>
     <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    
            <div style="height: 400px; overflow: auto" align="center">
                <asp:GridView ID="GridView1" runat="server" HeaderStyle-CssClass="FixedHeader" DataSourceID="sqlds" HeaderStyle-BackColor="YellowGreen" 
                    AutoGenerateColumns="false" AlternatingRowStyle-BackColor="WhiteSmoke" OnRowDataBound="gvDistricts_RowDataBound">
                    <Columns>
                        <asp:TemplateField HeaderText="SDG Cat No" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl1" runat="server" Text='<%#Eval("SDG Cat No")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Exertis Part Number" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl2" runat="server" Text='<%#Eval("Exertis Part Number")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Description" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl3" runat="server" Text='<%#Eval("Description")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Safety Rating" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl4" runat="server" Text='<%#Eval("Safety Rating")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Exertis Stock" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl5" runat="server" Text='<%#Eval("Exertis Stock")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Exertis Stock Value" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl6" runat="server" Text='<%#Eval("Exertis Stock Value")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Stock Cover (wks)" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl7" runat="server" Text='<%#Eval("[Stock Cover wks]")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Total Forecast Used" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl8" runat="server" Text='<%#Eval("Total Forecast Used")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Curr Wk" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl9" runat="server" Text='<%#Eval("Curr Wk")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Sell Thru wk 1" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl10" runat="server" Text='<%#Eval("Sell Thru wk 1")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Sell Thru wk 2" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl11" runat="server" Text='<%#Eval("Sell Thru wk 2")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Sell Thru wk 3" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl12" runat="server" Text='<%#Eval("Sell Thru wk 3")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Sell Thru wk 4" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl13" runat="server" Text='<%#Eval("Sell Thru wk 4")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Exertis OOS PO qty" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl14" runat="server" Text='<%#Eval("Exertis OOS PO qty")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="PO_1 Qty" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl15" runat="server" Text='<%#Eval("PO_1 Qty")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="PO_1 Due Date" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl16" runat="server" Text='<%#Eval("PO_1 Due Date")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="PO_2 Qty" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl17" runat="server" Text='<%#Eval("PO_2 Qty")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="PO_2 Due Date" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl18" runat="server" Text='<%#Eval("PO_2 Due Date")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="PO_3 Qty" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl19" runat="server" Text='<%#Eval("PO_3 Qty")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="PO_3 Due Date" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl20" runat="server" Text='<%#Eval("PO_3 Due Date")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>	<asp:TemplateField HeaderText="Current Orders" HeaderStyle-Width="120px" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lbl21" runat="server" Text='<%#Eval("Current Orders")%>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>
                </asp:GridView>
            </div>



                    </div>
                </div>
            </div>
         </div>
    </asp:Content>