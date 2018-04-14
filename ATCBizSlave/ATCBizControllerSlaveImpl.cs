using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.CompilerServices;
using ATCBizMaster;

namespace ATCBizSlave
{
    /**
     * Implementation of Business slave controller.
     * Connects to master, receives and initializes airport, waits for commands from master.
     */
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal class ATCBizControllerSlaveImpl : IATCBizControllerMasterCallback
    {
        private IATCBizControllerMaster m_ATCBiz; //ATC Biz Controller Master.
        private Airport m_airport; //Airport owned by slave.

        //Constructor. Create connection to Server.
        public ATCBizControllerSlaveImpl()
        {
            try //To create connection to Server.
            {
                DuplexChannelFactory<IATCBizControllerMaster> factory;
                NetTcpBinding tcpBinding = new NetTcpBinding();
                tcpBinding.MaxReceivedMessageSize = System.Int32.MaxValue; //Change max message & array size.
                tcpBinding.ReaderQuotas.MaxArrayLength = System.Int32.MaxValue;
                string sURL = "net.tcp://localhost:50001/ATCBiz";
                factory = new DuplexChannelFactory<IATCBizControllerMaster>(this, tcpBinding, sURL); //Use this object for callback.
                m_ATCBiz = factory.CreateChannel(); //Create connection.
                m_airport = m_ATCBiz.RegisterSlave(); //Add Callback interface to masters list.
                if (m_airport != null)
                {
                    m_airport.Routes = m_ATCBiz.GetRoutes(m_airport.AirportID); //Load routes from database.
                    m_airport.Airplanes = m_ATCBiz.GetAirplanes(m_airport.AirportID); //Load airplanes from database.
                    System.Console.WriteLine(m_airport.Name + " Airport ATC Running (ID: " + m_airport.AirportID + ")");
                    System.Console.ReadLine();
                }
                else
                    System.Console.WriteLine("No airport returned"); //Program ends.
            }
            catch (FaultException ex) //SOAP fault.
            {
                System.Console.WriteLine(ex.Message);
            }
            catch (CommunicationException ex) //Communication error.
            {
                System.Console.WriteLine("Can't connect to Server");
                System.Console.WriteLine(ex.Message);
            }
            catch (TimeoutException ex) //Server likely has died or halted.
            {
                System.Console.WriteLine("Server timed out");
                System.Console.WriteLine(ex.Message);
            }
            catch (Exception ex) //Generic exception for unknown problem.
            {
                System.Console.WriteLine("Exception type: " + ex.GetType());
                System.Console.WriteLine("Exception message: " + ex.Message);
            }
        }

        //Destructor.
        ~ATCBizControllerSlaveImpl()
        {
            if (m_airport != null)
                System.Console.WriteLine(m_airport.Name + " Airport ATC Closed (ID: " + m_airport.AirportID + ")");
        }

        //Add inbound plane to list.
        public void AddPlane(Airplane airplane)
        {
            m_airport.Airplanes.Add(airplane);
        }

        //Take a 15 minute step into the simulation.
        //Loops through each airplane under airport control, determines state and process.
        //During loop determine next plane to take off and land (If any).
        [MethodImpl(MethodImplOptions.Synchronized)] //Alters state of simulation, therefore access synchronized.
        public void Step()
        {
            try
            {
                Airplane outgoing = null; //Next plane to takeoff.
                Airplane incoming = null; //Next plane to land.
                AirRoute currRoute = null; //Airplanes current AirRoute.
                Airplane ap; //Current airplane of loop.
                double currRouteDist;
                for (int i = m_airport.Airplanes.Count - 1; i >= 0; i--) //Reverse for loop used because airplanes are removed from list.
                {
                    ap = m_airport.Airplanes[i];
                    switch (ap.State)
                    {
                        case 'I': //In transit
                            ap.Fuel -= (ap.FuelConsPerHour / 4.0); //Decrement fuel.
                            ap.DistanceAlongRoute += (ap.CruisingKPH / 4.0); //Increment distance along route.
                            currRoute = GetRoute(ap.CurrentAirRouteID); //Get airplane's current route.
                            if (currRoute != null)
                            {
                                if (ap.DistanceAlongRoute >= currRoute.DistanceKM) //If reached airport pass control.
                                {
                                    ap.DistanceAlongRoute = currRoute.DistanceKM;
                                    ap.State = 'C';
                                    m_ATCBiz.PassControl(ap, currRoute.ToAirportID);
                                    m_airport.Airplanes.Remove(ap);
                                }
                                else if (ap.DistanceAlongRoute + 300.0 >= currRoute.DistanceKM) //If within 300km of airport pass control.
                                {
                                    ap.State = 'E';
                                    m_ATCBiz.PassControl(ap, currRoute.ToAirportID);
                                    m_airport.Airplanes.Remove(ap);
                                }
                            }
                            else System.Console.WriteLine("Error: No assigned route for In Transit Airplane " + ap.AirplaneID); //Should never reach this state.
                            currRoute = null; //Reset.
                            break;
                        case 'E': //Entering circling
                            SelectLanding(ap, ref incoming); //Test if this plane should land next.
                            ap.Fuel -= (ap.FuelConsPerHour / 4.0); //Decrement fuel.
                            ap.DistanceAlongRoute += (ap.CruisingKPH / 4.0); //Increment distance along route.
                            currRouteDist = m_ATCBiz.GetRouteDist(ap.CurrentAirRouteID); //Get airplane's current route.
                            if (ap.Fuel <= 0.0) //Test if crashed.
                            {
                                ap.Fuel = 0.0;
                                ap.State = 'X';
                            }
                            else if (currRouteDist != 0)
                            {
                                if (ap.DistanceAlongRoute >= currRouteDist) //Can either land this time step (From Entering Circling state) or start Circling.
                                {
                                    ap.DistanceAlongRoute = currRouteDist;
                                    ap.State = 'C'; //Set to Circling in case plane can't land this time step.
                                }
                            }
                            else System.Console.WriteLine("Error: No assigned route for Entering Circling Airplane " + ap.AirplaneID); //Should never reach this state.
                            break;
                        case 'C': //Circling
                            SelectLanding(ap, ref incoming); //Test if this plane should land next.
                            ap.Fuel -= (ap.FuelConsPerHour / 4.0); //Decrement fuel.
                            if (ap.Fuel <= 0.0) //Test if crashed.
                            {
                                ap.Fuel = 0.0;
                                ap.State = 'X';
                            }
                            break;
                        case 'L': //Landed
                            if (ap.TimeLanded >= 60) //If refuelled.
                            {
                                if (outgoing != null)
                                {
                                    if (ap.TimeLanded > outgoing.TimeLanded) outgoing = ap; //Select airplane thats been waiting the longest.
                                }
                                else
                                {
                                    outgoing = ap;
                                }
                            }
                            ap.TimeLanded += 15; //Increment time landed.
                            break;
                        case 'X': //Crashed
                            break; //Ignore. No processing required.
                        default: //Should never reach this.
                            System.Console.WriteLine("Airplane " + ap.AirplaneID + " in unexpected state: " + ap.State);
                            break;
                    }
                }

                //Finished updating planes status. Now process planes selected to take off & land.
                if (outgoing != null) //If a plane has been selected to take off.
                {
                    outgoing.State = 'I'; //Set to In transit and update values.
                    outgoing.DistanceAlongRoute = 0;
                    outgoing.CurrentAirRouteID = m_airport.Routes[m_airport.NextRoute].AirRouteID;
                    outgoing.CurrentAirportID = -1;
                    outgoing.TimeLanded = -1;
                    outgoing.Fuel = (m_airport.Routes[m_airport.NextRoute].DistanceKM / outgoing.CruisingKPH) * outgoing.FuelConsPerHour * 1.15;
                    if (m_airport.Routes[m_airport.NextRoute].DistanceKM <= 300) //If airport within 300km then pass control (Unlikely).
                    {
                        m_ATCBiz.PassControl(outgoing, m_airport.Routes[m_airport.NextRoute].ToAirportID);
                        m_airport.Airplanes.Remove(outgoing);
                    }
                    if (m_airport.NextRoute + 2 > m_airport.Routes.Count) m_airport.NextRoute = 0; //Set next airroute.
                    else m_airport.NextRoute += 1;
                }
                if (incoming != null) //If a plane has been selected to land.
                {
                    incoming.State = 'L'; //Set to Landed and update values.
                    incoming.CurrentAirRouteID = -1;
                    incoming.DistanceAlongRoute = -1;
                    incoming.CurrentAirportID = m_airport.AirportID;
                    incoming.TimeLanded = 0;
                }
            }
            catch (FaultException ex) //SOAP fault.
            {
                System.Console.WriteLine(ex.Message);
            }
            catch (CommunicationException ex) //Communication error.
            {
                System.Console.WriteLine("Can't connect to Server");
                System.Console.WriteLine(ex.Message);
            }
            catch (TimeoutException ex) //Server likely has died or halted.
            {
                System.Console.WriteLine("Server timed out");
                System.Console.WriteLine(ex.Message);
            }
            catch (Exception ex) //Generic exception for unknown problem.
            {
                System.Console.WriteLine("Exception type: " + ex.GetType());
                System.Console.WriteLine("Exception message: " + ex.Message);
            }
        }

        //Determines which airroute the airplane is on for In-transit planes.
        public AirRoute GetRoute(int airRouteID)
        {
            AirRoute currRoute = null;
            foreach (AirRoute ar in m_airport.Routes)
            {
                if (airRouteID == ar.AirRouteID)
                    currRoute = ar;
            }
            return currRoute;
        }

        //Select plane with lowest fuel to land next.
        private void SelectLanding(Airplane ap, ref Airplane incoming)
        {
            if (incoming != null)
            {
                if (ap.Fuel < incoming.Fuel) incoming = ap; //Select airplane with lowest fuel.
            }
            else
            {
                incoming = ap;
            }
        }

        //Get list of planes for display in GUI.
        public List<Airplane> GetStatus()
        {
            return m_airport.Airplanes;
        }

        //Get list of routes for airport. For debugging purposes.
        public List<AirRoute> GetRoutes()
        {
            return m_airport.Routes;
        }

        //Return slave identity. For troubleshooting.
        public string Identify()
        {
            return m_airport.Name;
        }
    }
}
