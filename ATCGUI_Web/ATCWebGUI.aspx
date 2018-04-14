<%@ Page Title="ATC Web GUI" Language="C#" AutoEventWireup="true"
    CodeFile="ATCWebGUI.aspx.cs" Inherits="_ATCWebGUI" %>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server"> <title>ATC Web GUI</title> </head>
<body>
<form id="frmATC" runat="server" >
    <div>
    <label id="lblAirport">Airports: </label>
        <select id="sltAirport" runat="server" datasourceid=""></select>
        <input id="btnShow" type="button" value="Show Status" runat="server" onserverclick="btnShow_ServerClick" />
        <input id="btnStep" type="button" value="->" runat="server" onserverclick="btnStep_ServerClick" /><br /><br />
    <div id="divAirplanes" runat="server">
    <table id="tblAirplanes" border="1"></table>
    </div>
    </div>
</form>
</body>
</html>
