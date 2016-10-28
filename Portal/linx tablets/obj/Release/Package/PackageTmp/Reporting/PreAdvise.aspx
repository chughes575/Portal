<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="PreAdvise.aspx.cs" Inherits="linx_tablets.Reporting.PreAdvise" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
   <asp:SqlDataSource ID="sqlDSPreAdviseASNS" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="


SELECT
     lm.PlantCode+'-'+lm.PlantDescription as Locale, cast(Document_date as date) as ASNDate,cast(asn.localeid as varchar(10))+'|'+cast(cast(Document_date as date) as varchar(20)) as Command,
     STUFF(
         (SELECT DISTINCT ',' + asn_id
          FROM MSE_OracleASNItems asni1 
inner join MSE_OracleASNS asn1 on asn1.mseasnid=asni1.mseasnid
          WHERE asn1.LocaleID = asn.[LocaleID] AND asn1.Document_date = asn.Document_date
		  
          FOR XML PATH (''))
          , 1, 1, '')  AS [ASNNUMBERS]
from MSE_OracleASNItems asni 
inner join MSE_OracleASNS asn on asn.mseasnid=asni.mseasnid
inner join MSE_ApplelocaleMapping lm on lm.LocaleID=asn.LocaleID
group by asn.LocaleID,lm.PlantCode,lm.PlantDescription,asn.Document_date
order by asn.Document_date desc

"></asp:SqlDataSource>


    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h2>Pre advise ASNS</h2>
                    <div style="
    border: 5px solid gray;
    padding: 5px;
    background: white;
    width: 100%;
    height: 300px;
    overflow-y: scroll;">
                    <asp:GridView ID="gvPreAdvise" CssClass="CSSTableGenerator" AutoGenerateColumns="false" runat="server" DataSourceID="sqlDSPreAdviseASNS" onRowCommand="gvPreAdvise_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="Locale" HeaderText="Locale" />
                            <asp:BoundField DataField="ASNDate" HeaderText="ASNDate" />
                            <asp:BoundField DataField="ASNNUMBERS" HeaderText="ASNs Number" />
                            <asp:TemplateField>
                                <HeaderTemplate>Pre-advise detail</HeaderTemplate>
                                <ItemTemplate>
                                     <asp:Button ID="btnGeneratePreAdvise" runat="server" Text="Download" CommandName="generatePreAdviseDetail" CommandArgument='<%# Eval("Command") %>' />
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