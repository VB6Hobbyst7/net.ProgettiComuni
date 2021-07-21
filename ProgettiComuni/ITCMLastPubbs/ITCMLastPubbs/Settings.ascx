<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="Christoc.Modules.ITCMLastPubbs.Settings" %>


<!-- uncomment the code below to start using the DNN Form pattern to create and update settings -->  

<%@ Register TagName="label" TagPrefix="dnn" Src="~/controls/labelcontrol.ascx" %>

	<h2 id="dnnSitePanel-BasicSettings" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("BasicSettings")%></a></h2>
	<fieldset>
        <div class="dnnFormItem">
            <dnn:Label ID="lblUsername" runat="server" /> 
            <asp:TextBox ID="txtUsername" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label ID="lblPassword" runat="server" />
            <asp:TextBox ID="txtPassword" runat="server" />
        </div>
		<div class="dnnFormItem">
            <dnn:label ID="lblTargetUser" runat="server" />
            <asp:TextBox ID="txtTargetUser" runat="server" />
        </div>
		<div class="dnnFormItem">
            <dnn:label ID="lblKnosURL" runat="server" />
            <asp:TextBox ID="txtKnosURL" runat="server" />
        </div>
		<div class="dnnFormItem">
            <dnn:label ID="lblPageSize" runat="server" />
            <asp:TextBox ID="txtPageSize" runat="server" />
        </div>
		<div class="dnnFormItem">
            <dnn:label ID="lblKnoSPage" runat="server" />
            <asp:TextBox ID="txtKnosPage" runat="server" />
        </div>
		<div class="dnnFormItem">
            <dnn:label ID="lblTypes" runat="server" />
            <asp:TextBox ID="txtTypes" runat="server" />
        </div>
		<div class="dnnFormItem">
            <dnn:label ID="lblToolTipField" runat="server" />
            <asp:TextBox ID="txtToolTipField" runat="server" />
        </div>
		<div class="dnnFormItem">
            <dnn:label ID="lblIdView" runat="server" />
            <asp:TextBox ID="txtIdView" runat="server" />
        </div>

    </fieldset>