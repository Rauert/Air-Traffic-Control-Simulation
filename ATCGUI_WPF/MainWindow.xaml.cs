using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.ServiceModel;
using ATCBizMaster;

namespace ATCGUI_WPF
{
    /**
     * Interaction logic for MainWindow.xaml
     * Connects to master and displays status of simulation and takes steps into the simulation.
     */
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public partial class MainWindow : Window
    {
        private IATCBizControllerMaster m_ATCBiz; //ATC Biz Controller Master.
        public Airport[] m_airports; //List of airports.
        private List<Airplane> m_inbound; //List of inbound planes for currently selected airport.
        private List<Airplane> m_outbound; //List of outbound planes for currently selected airport.

        public MainWindow()
        {
            InitializeComponent();
        }

        //Establish connection to business controller master and initialize list of airports.
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try //To create connection to Server.
            {
                DuplexChannelFactory<IATCBizControllerMaster> factory;
                NetTcpBinding tcpBinding = new NetTcpBinding();
                tcpBinding.MaxReceivedMessageSize = System.Int32.MaxValue; //Change max message & array size.
                tcpBinding.ReaderQuotas.MaxArrayLength = System.Int32.MaxValue;
                string sURL = "net.tcp://localhost:50001/ATCBiz";
                factory = new DuplexChannelFactory<IATCBizControllerMaster>(new dummyCallback(), tcpBinding, sURL);
                m_ATCBiz = factory.CreateChannel(); //Create connection.
                m_airports = m_ATCBiz.GetAirports();
                lvwAirports.ItemsSource = m_airports; //Set listview of airports.
            }
            catch (FaultException ex) //SOAP fault.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show("Communication error, cannot continue at this time.");
            }
            catch (CommunicationException ex) //Communication error.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show("Can't connect to Server");
            }
            catch (TimeoutException ex) //Server likely has died or halted.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show("Server timed out");
            }
            catch (Exception ex) //Generic exception for unknown problem.
            {
                System.Diagnostics.Debug.WriteLine("Exception type: " + ex.GetType());
                System.Diagnostics.Debug.WriteLine("Exception message: " + ex.Message);
                MessageBox.Show("Unexpected error, cannot continue at this time.");
            }
        }

        //Update inbound and outbound plane lists when selection changes.
        private void lvwAirports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                updateInOutbound();
            }
            catch (FaultException ex) //SOAP fault.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show("Communication error, cannot continue at this time.");
            }
            catch (CommunicationException ex) //Communication error.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show("Can't connect to Server");
            }
            catch (TimeoutException ex) //Server likely has died or halted.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show("Server timed out");
            }
            catch (Exception ex) //Generic exception for unknown problem.
            {
                System.Diagnostics.Debug.WriteLine("Exception type: " + ex.GetType());
                System.Diagnostics.Debug.WriteLine("Exception message: " + ex.Message);
                MessageBox.Show("Unexpected error, cannot continue at this time.");
            }
        }

        //Take step into simulation and update inbound and outbound plane lists.
        private void btnStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                m_ATCBiz.Step();
                updateInOutbound();
            }
            catch (FaultException ex) //SOAP fault.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show("Communication error, cannot continue at this time.");
            }
            catch (CommunicationException ex) //Communication error.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show("Can't connect to Server");
            }
            catch (TimeoutException ex) //Server likely has died or halted.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show("Server timed out");
            }
            catch (Exception ex) //Generic exception for unknown problem.
            {
                System.Diagnostics.Debug.WriteLine("Exception type: " + ex.GetType());
                System.Diagnostics.Debug.WriteLine("Exception message: " + ex.Message);
                MessageBox.Show("Unexpected error, cannot continue at this time.");
            }
        }

        //Update inbound and outbound plane lists.
        private void updateInOutbound()
        {
            if (lvwAirports.SelectedItem != null) //If an airport is selected.
            {
                Airport selectedAP = (Airport)lvwAirports.SelectedItem;
                List<Airplane> allAirplanes = m_ATCBiz.AirportStatus(selectedAP.AirportID); //All planes owned by airport.
                m_inbound = new List<Airplane>();
                m_outbound = new List<Airplane>();
                foreach (Airplane ap in allAirplanes)
                {
                    if (ap.State == 'I' || ap.State == 'L') m_outbound.Add(ap); //Separate planes into correct lists.
                    else m_inbound.Add(ap);
                }
                lvwInbound.ItemsSource = m_inbound;
                lvwOutbound.ItemsSource = m_outbound;
            }
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

    /**
     * Five Converter classes.
     * Used to alter the values displayed in the inbound and outbound listviews.
     */

    //Class to convert the State char value to the equivalent full string value.
    public class CharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string rtn = "";
            switch((char)value)
            {
                case 'L':
                    rtn = "Landed";
                    break;
                case 'I':
                    rtn = "In-transit";
                    break;
                case 'X':
                    rtn = "Crashed";
                    break;
                case 'E':
                    rtn = "Entering Circling";
                    break;
                case 'C':
                    rtn = "Circling";
                    break;
            }
            return rtn;
        }

        //Not required.
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //Class to round fuel value to 2 decimal places.
    public class DblConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Math.Round((double)value,2);
        }

        //Not required.
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //Class to replace int value of -1 with blank. For Air Route ID.
    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string rtn = value.ToString();
            if ((int)value == -1)
                rtn = "";
            return rtn;
        }

        //Not required.
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //Class to convert AirRouteID to equivalent destination names.
    public class RouteConverter : IValueConverter
    {
        Airport[] m_airports;
        List<AirRoute> m_routes;

        //Initialize and connect to ATC Biz Controller master.
        public RouteConverter()
        {
           try //Create connection to Server.
           {
                DuplexChannelFactory<IATCBizControllerMaster> factory;
                NetTcpBinding tcpBinding = new NetTcpBinding();
                tcpBinding.MaxReceivedMessageSize = System.Int32.MaxValue; //Change max message & array size.
                tcpBinding.ReaderQuotas.MaxArrayLength = System.Int32.MaxValue;
                string sURL = "net.tcp://localhost:50001/ATCBiz";
                factory = new DuplexChannelFactory<IATCBizControllerMaster>(new dummyCallback(), tcpBinding, sURL);
                IATCBizControllerMaster ATCBiz = factory.CreateChannel(); //Create connection.
                m_airports = ATCBiz.GetAirports();
                m_routes = ATCBiz.GetAllRoutes();
           }
           catch (FaultException ex) //SOAP fault.
           {
               System.Diagnostics.Debug.WriteLine(ex.Message);
           }
           catch (CommunicationException ex) //Communication error.
           {
               System.Diagnostics.Debug.WriteLine(ex.Message);
           }
           catch (TimeoutException ex) //Server likely has died or halted.
           {
               System.Diagnostics.Debug.WriteLine(ex.Message);
           }
           catch (Exception ex) //Generic exception for unknown problem.
           {
               System.Diagnostics.Debug.WriteLine("Exception type: " + ex.GetType());
               System.Diagnostics.Debug.WriteLine("Exception message: " + ex.Message);
           }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string rtn = ""; 
            int val = (int)value;
            AirRoute route = null;
            if (val != -1 && val != 0)
            {
                foreach(AirRoute ar in m_routes) //Find the route the plane is on.
                {
                    if (val == ar.AirRouteID)
                        route = ar;
                }
                if (route != null)
                    rtn = m_airports[route.FromAirportID - 1].Name + " to " + m_airports[route.ToAirportID - 1].Name;
            }
            return rtn;
        }

        //Not required.
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //Class to convert AirportID to name.
    public class AirportConverter : IValueConverter
    {
        Airport[] m_airports;

        //Initialize and connect to ATC Controller master.
        public AirportConverter()
        {
            try //Create connection to Server.
            {
                DuplexChannelFactory<IATCBizControllerMaster> factory;
                NetTcpBinding tcpBinding = new NetTcpBinding();
                tcpBinding.MaxReceivedMessageSize = System.Int32.MaxValue; //Change max message & array size.
                tcpBinding.ReaderQuotas.MaxArrayLength = System.Int32.MaxValue;
                string sURL = "net.tcp://localhost:50001/ATCBiz";
                factory = new DuplexChannelFactory<IATCBizControllerMaster>(new dummyCallback(), tcpBinding, sURL);
                IATCBizControllerMaster ATCBiz = factory.CreateChannel(); //Create connection.
                m_airports = ATCBiz.GetAirports();
            }
            catch (FaultException ex) //SOAP fault.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            catch (CommunicationException ex) //Communication error.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            catch (TimeoutException ex) //Server likely has died or halted.
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            catch (Exception ex) //Generic exception for unknown problem.
            {
                System.Diagnostics.Debug.WriteLine("Exception type: " + ex.GetType());
                System.Diagnostics.Debug.WriteLine("Exception message: " + ex.Message);
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string rtn = "";
            int val = (int)value;
            if (val != -1 && val != 0 && val == m_airports[val - 1].AirportID) //Verify airportID is valid.
                rtn = m_airports[val - 1].Name;
            return rtn;
        }

        //Not required.
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}