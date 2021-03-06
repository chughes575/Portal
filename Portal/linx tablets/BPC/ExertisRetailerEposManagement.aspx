﻿<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="false" AutoEventWireup="True" CodeBehind="ExertisRetailerEposManagement.aspx.cs" Inherits="linx_tablets.BPC.EposSales" %>

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
    <asp:SqlDataSource ID="sqlDSKewillProductStockStatusReport" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select CASE WHEN reportid IN (7, 8, 11) THEN datediff(HOUR, DATEADD(month, DATEDIFF(month, 0, getdate()), 0), lastfiledate) ELSE datediff(hour, lastfiledate, getdate()) END AS dateDiffImport, 
                         ReportID, CustomerID, ReportName, LastFilename, lastfiledate, WarningDiff, Username, UserPopulated from mse_portalforecastreportmanagement where reportid=40 order by lastfiledate desc"></asp:SqlDataSource>

    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Exertis Retailer Epos Management</h1>

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

                    <h3>Epos Retailers</h3>
                    To upload an epos report for a retailer first create an entry below. This will generate a RetailerID which is you can use to upload the stock figures against in the upload form below.
                  
                    <br />
                    <br />
                    <h4>Downloads</h4><br />
                    <b>Download stock lines-</b> This is a download of all the skus with stock qty's that have been received in the latest stock email. Non stock related data is stripped out.
                    <br />
                    <b>Download Original report -</b> This is a download of the full stock report that the inventory figures are based on. historical reports can be downloaded.
                   <br />
                    <br />
                    <br />
                    <b>Retailers in Amber have their data pulled from email and cannot be uploaded against only downloaded.</b>
                   
                    <asp:GridView 
         ID="gvConsignmentretailers" 
         runat="server" 
         AutoGenerateColumns="False"
         DataKeyNames="RetailerID" 
         DataSourceID="sqlDsConsignmentRetailers" 
         OnRowCommand="gvConsignmentretailers_RowCommand" OnDataBound="gvConsignmentretailers_DataBound"
         
         ShowFooter="True"
         >
         <RowStyle BackColor="#EFF3FB" />
         <Columns>
             <asp:TemplateField HeaderText="RetailerID" >
                     <ItemTemplate>
                         <asp:Label ID="lblRetailerID" runat="server" Text='<%# Bind("RetailerID") %>'></asp:Label>
                     </ItemTemplate>
                     </asp:TemplateField>
            
            <asp:TemplateField HeaderText="Retailer Description" >
                 <EditItemTemplate>
                     <asp:TextBox ID="txtRetailDescription" runat="server" Text='<%# Bind("RetailerDescription") %>'></asp:TextBox>
                 </EditItemTemplate>
                 <ItemTemplate>
                     <asp:Label ID="lblRetailDescription" runat="server" Text='<%# Bind("RetailerDescription") %>'></asp:Label>
                 </ItemTemplate>
                <FooterTemplate>
                     <asp:TextBox ID="txtRetailDescription" runat="server" Height="44px" Text='<%# Bind("RetailerDescription") %>' ></asp:TextBox>
                 </FooterTemplate>
            </asp:TemplateField>
             <asp:TemplateField HeaderText="Oracle Code">
                 <EditItemTemplate>
                     <asp:TextBox ID="txtOracleCode" runat="server" Text='<%# Bind("OracleCode") %>'></asp:TextBox>
                 </EditItemTemplate>
                 <ItemTemplate>
                     <asp:Label ID="lblOracleCode" runat="server" Text='<%# Bind("OracleCode") %>'></asp:Label>
                 </ItemTemplate>
                 <FooterTemplate>
                     <asp:TextBox ID="txtOracleCode" runat="server" Height="44px" Text='<%# Bind("OracleCode") %>' ></asp:TextBox>
                 </FooterTemplate>
            </asp:TemplateField>
            <asp:TemplateField ShowHeader="False">
                 <EditItemTemplate>
                     <asp:Button ID="Button1" runat="server" CausesValidation="True" CommandName="Update" Text="Update" />&nbsp;
                     <asp:Button ID="Button2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" />
                     
                 </EditItemTemplate>
                 <FooterTemplate>
                     <asp:Button ID="Button3" runat="server" CommandName="Insert" Text="Insert" />
                 </FooterTemplate>
                 <ItemTemplate>
                     <asp:Button ID="EditNotesButton" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit" />
                     
                 </ItemTemplate>
                 <ItemStyle Width="50px" />
            </asp:TemplateField>
             <asp:TemplateField ShowHeader="False">
                  <ItemTemplate>
                     <asp:Button ID="Button4" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" />
                 </ItemTemplate>
                 </asp:TemplateField>
             <asp:TemplateField ShowHeader="False">
                  <ItemTemplate>
                     <asp:Button ID="btnDownloadLines" runat="server" CausesValidation="False" CommandName="Download" CommandArgument='<%# Bind("RetailerID") %>' Text="Download Stock Lines" />&nbsp;
                 </ItemTemplate>
                 </asp:TemplateField>
             <asp:TemplateField ShowHeader="False">
                 <HeaderTemplate>Email File Details</HeaderTemplate>
                  <ItemTemplate>
                     
                      <asp:SqlDataSource ID="sqldsFileLines" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
        SelectCommand="select ID,Filename,IMportDate  from MSE_PortalSalesEposFiles where Retailer=@retailid order by Importdate desc">
                          <SelectParameters>
                              <asp:ControlParameter ControlID="lblRetailerID" PropertyName="Text" Name="retailid" />
                          </SelectParameters>


                      </asp:SqlDataSource>
                    
                     <asp:DropDownList ID="ddlFileNames" runat="server" DataSourceID="sqldsFileLines" DataTextField="Filename" DataValueField="Filename" Width="90px"></asp:DropDownList>&nbsp;
                     <asp:Label ID="lblReportDate" runat="server" Text='<%# Bind("ImportDate") %>'></asp:Label>
                       <asp:Button ID="btnDownloadLinesC" runat="server" CausesValidation="False" CommandName="DownloadOs" CommandArgument='<%# Bind("RetailerID") %>' Text="Download Original Report" />
                 </ItemTemplate>
                 </asp:TemplateField>
            </Columns>
    
    </asp:GridView>
    <asp:SqlDataSource 
         ID="sqlDsConsignmentRetailers" 
         runat="server" 
         ConflictDetection="CompareAllValues"
         ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>" 
         DeleteCommand="DELETE FROM [MSE_PortalConsignmentRetailers] WHERE [RetailerID] = @original_retailerid"
         InsertCommand="INSERT INTO [MSE_PortalConsignmentRetailers] ([CustomerID],[RetailerDescription], [OracleCode]) VALUES (5,@RetailerDescription,@OracleCode)"
         OldValuesParameterFormatString="original_{0}" 
         SelectCommand="select epos.*,dts.Importdate from MSE_PortalConsignmentRetailers epos left outer join (select retailer,max(importdate) as Importdate from MSE_PortalSalesEposFiles group by retailer) as dts on dts.retailer=epos.retailerid where epos.customerid=5"
         UpdateCommand="UPDATE [MSE_PortalConsignmentRetailers] SET [RetailerDescription] = @RetailerDescription, [OracleCode] = @OracleCode WHERE [RetailerID] = @original_retailerid " 
         OnInserting="sqlDsConsignmentRetailers_Inserting">
                <DeleteParameters>
                    <asp:Parameter Name="original_retailerid" Type="Int32" />
                </DeleteParameters>
                <UpdateParameters>
                    <asp:Parameter Name="OracleCode" Type="String" />
                    <asp:Parameter Name="RetailerDescription" Type="String" />
                    <asp:Parameter Name="original_retailerid" Type="Int32" />
                </UpdateParameters>
                <InsertParameters>
                    <asp:Parameter Name="OracleCode" Type="String" />
                    <asp:Parameter Name="RetailerDescription" Type="String" />
                </InsertParameters>
    </asp:SqlDataSource>
                    <br />
                    <br />
                    <h3>Epos Upload</h3>
                    Format required for the upload is RetailerID/CustomerSku/SaleDate/StockQty
                    <br />
                    Any existing stock entries for a sku on an existing date will be overwritten with the data in the file.
                    <br />
                 <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing epos data (All retailers)</th>
                            <th>Download blank template file</th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnDownloadExistingEposdata" runat="server" Text="Download" OnClick="btnDownloadExistingEposdata_Click" /></td>
                            <td>
                                <asp:Button ID="btnDownloadTemplateEposData" runat="server" Text="Download" OnClick="btnDownloadTemplateEposData_Click" /></td>
                        </tr>
                    </table>
                    <br />

                    <asp:FileUpload ID="fuConsignmentStock" runat="server" /><br />
                    <asp:Button ID="btnUploadConsignmentStock" runat="server" Text="Upload" OnClick="btnUploadConsignmentStock_Click" />   
                </div>
            </div>
        </div>
    </div>
</asp:Content>
