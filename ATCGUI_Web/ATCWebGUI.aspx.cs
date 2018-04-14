using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ServiceModel;
using ATCBizMaster;
using System.Runtime.Serialization;

/**
 * Logic for ATCWebGUI.aspx
 * Connects to master and displays status of simulation and takes steps into the simulation.
 */
public partial class _ATCWebGUI : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack) //Don't reload airport list on postback.
        {
            connectServer(0);
        }
    }

    //Show status of airport.
    protected void btnShow_ServerClick(object sender, EventArgs e)
    {
        connectServer(1);
    }

    //Take step into simulation.
    protected void btnStep_ServerClick(object sender, EventArgs e)
    {
        connectServer(2);
    }

    //Connect to ATC business controller master and take action.
    //Actions:  0=Initialize: Set Airplane list and build initial airplane table.
    //          1=Rebuild airplanes table.
    //          2=Take step and rebuild airplanes table.
    private void connectServer(int action)
    {
        try //To create connection to Server.
        {
            IATCBizControllerMaster ATCBiz;
            Airport[] airports;
            List<Airplane> airplanes;
            DuplexChannelFactory<IATCBizControllerMaster> factory;
            NetTcpBinding tcpBinding = new NetTcpBinding();
            string sURL = "net.tcp://localhost:50001/ATCBiz";
            factory = new DuplexChannelFactory<IATCBizControllerMaster>(new dummyCallback(), tcpBinding, sURL);
            ATCBiz = factory.CreateChannel(); //Create connection.
            switch (action)
            {
                case 0: //Initializing page with airport list and initial airplane table.
                    airports = ATCBiz.GetAirports();
                    sltAirport.DataSource = airports;
                    sltAirport.DataTextField = "Name";
                    sltAirport.DataValueField = "Name";
                    sltAirport.DataBind();
                    airplanes = ATCBiz.AirportStatus(sltAirport.SelectedIndex+1);
                    buildAirplaneTbl(airplanes);
                    break;
                case 1: //Rebuild airplane table.
                    airplanes = ATCBiz.AirportStatus(sltAirport.SelectedIndex+1);
                    buildAirplaneTbl(airplanes);
                    break;
                case 2: //Take simulation step and rebuild airplane table.
                    ATCBiz.Step();
                    airplanes = ATCBiz.AirportStatus(sltAirport.SelectedIndex+1);
                    buildAirplaneTbl(airplanes);
                    break;
                default:
                    break;
            }
        }
        catch (FaultException ex) //SOAP fault.
        {
            Response.Write(ex.Message);
        }
        catch (CommunicationException ex) //Communication error.
        {
            Response.Write("Can't connect to Server" + "<br />");
            Response.Write(ex.Message);
        }
        catch (TimeoutException ex) //Server likely has died or halted.
        {
            Response.Write("Server timed out" + "<br />");
            Response.Write(ex.Message);
        }
        catch (Exception ex) //Generic exception for unknown problem.
        {
            Response.Write("Exception type: " + ex.GetType() + "<br />");
            Response.Write("Exception message: " + ex.Message);
        }
    }

    //Build airplanes table from airplanes list.
    private void buildAirplaneTbl(List<Airplane> airplanes)
    {
        //Build table header.
        divAirplanes.InnerHtml = "<table id='tblAirplanes' border='1'>" +
                                    "<tr><th width='90'><p>Airplane ID</p></th>" +
                                        "<th width='120'><p>State Description</p></th>" +
                                        "<th width='120'><p>Type</p></th>" +
                                        "<th width='120'><p>Fuel remaining</p></th></tr>";
        //Build table contents.
        foreach (Airplane ap in airplanes)
        {
            divAirplanes.InnerHtml += "<tr><td width='90'><p>" + ap.AirplaneID + "</p></td>" +
                                            "<td width='120'><p>" + getState(ap.State) + "</p></td>" +
                                            "<td width='120'><p>" + ap.PlaneType + "</p></td>" +
                                            "<td width='120'><p>" + Math.Round(ap.Fuel, 2) + "</p></td></tr>";
        }
        divAirplanes.InnerHtml += "</table>"; //Add end table tag.
    }

    //Get State description from char appreviation.
    private string getState(char c)
    {
        string rtn;
        switch (c)
        {
            case 'I':
                rtn = "In transit";
                break;
            case 'E':
                rtn = "Entering circling";
                break;
            case 'C':
                rtn = "Circling";
                break;
            case 'L':
                rtn = "Landed";
                break;
            case 'X':
                rtn = "Crashed";
                break;
            default:
                rtn = c.ToString();
                break;
        }
        return rtn;
    }
}

/**
* Dummy implementation of callback interface. Never actually called.
* Required because the slaves and GUI use the same interface which has a callback interface.
*/
[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
internal class dummyCallback : IATCBizControllerMasterCallback
{
    public void Step() { }
    public void AddPlane(Airplane airplane) { }
    public List<Airplane> GetStatus() { return new List<Airplane>(); }
    public List<AirRoute> GetRoutes() { return new List<AirRoute>(); }
    public string Identify() { return ""; }
}
