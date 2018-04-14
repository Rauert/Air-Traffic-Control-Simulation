using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.CompilerServices;

namespace ATCBizMaster
{
    /**
     * Singleton Server implementation of Business tier master controller.
     * Connects to database, initializes slaves, issues instructions to slaves and provides data to the GUI's.
     */
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)] //Singleton server.
    internal class ATCBizControllerMasterImpl : IATCBizControllerMaster
    {
        ATCDatabase.ATCDB m_db; //Database of airports, airroutes and aircraft for initialization;
        Airport[] m_airports; //List of airports in system. For sending to GUI. ID of airport is array position + 1. Planes and routes not stored.
        IATCBizControllerMasterCallback[] m_ATCSlaves; //List of running slaves interfaces. ID of airport is array position + 1.
        List<AirRoute> m_routes; //List of all routes for display in GUI.
        int airportsOffloaded;

        //Constructor.
        public ATCBizControllerMasterImpl()
        {
            m_db = new ATCDatabase.ATCDB(); //Create database object.
            m_routes = new List<AirRoute>();
            System.Console.WriteLine("ATC Master running");
        }

        //Create airports to offload.
        public void Initialize()
        {
            try
            {
                int numAirports = m_db.GetNumAirports(); //Determine how many airports in system.
                airportsOffloaded = 0; //Number of airports to offload.
                int[] airportIDs = m_db.GetAirportIDList(); //Get list of airport ids. (1 based, not 0 based).
                m_airports = new Airport[numAirports]; //Initialize airport and callback object arrays to number of airports.
                m_ATCSlaves = new IATCBizControllerMasterCallback[numAirports];
                string airportName; //Stores out value of airport name from database.

                foreach (int id in airportIDs) //Create each Airport.
                {
                    m_db.LoadAirport(id, out airportName);
                    m_airports[id - 1] = new Airport(id, airportName); //Database is 1 based so must decrement ID by 1.
                }
            }
            catch (Exception ex) //Generic exception for unknown problem.
            {
                System.Console.WriteLine("Exception type: " + ex.GetType());
                System.Console.WriteLine("Exception message: " + ex.Message);
            }
        }

        //Destructor.
        ~ATCBizControllerMasterImpl()
        {
            System.Console.WriteLine("Master closed");
        }

        /**
         * Method implementations for Slave clients.
         */

        //Register new slave in Callback list.
        public Airport RegisterSlave()
        {
            Airport rtn = null;
            if (airportsOffloaded < m_airports.Length) //Return null is all airports already offloaded.
            {
                m_ATCSlaves[airportsOffloaded] = OperationContext.Current.GetCallbackChannel<IATCBizControllerMasterCallback>();
                airportsOffloaded++;
                rtn = m_airports[airportsOffloaded - 1];
            }
            return rtn;
        }

        //Get the routes from the database for the slave.
        public List<AirRoute> GetRoutes(int airportID)
        {
            List<AirRoute> routes = new List<AirRoute>();
            int[] airRouteIDs = m_db.GetDepartingAirRouteIDsForAirport(airportID); //List of route id's.
            int fromID, toID; //Variables to store outs.
            double distance;
            foreach (int id in airRouteIDs) //Populate routes list.
            {
                m_db.LoadAirRoute(id, out fromID, out toID, out distance);
                routes.Add(new AirRoute(id, fromID, toID, distance));
                m_routes.Add(new AirRoute(id, fromID, toID, distance)); //Populate local route list at same time.
            }
            return routes;
        }

        //Get the airplanes from the database.
        public List<Airplane> GetAirplanes(int airportID)
        {
            List<Airplane> planes = new List<Airplane>();
            int[] airplaneIDs = m_db.GetAirplaneIDsForAirport(airportID); //List of airplane ID's.
            string planeType; //Variables to store outs.
            int startID;
            double cruisingSpeed, fuelCons;
            foreach (int id in airplaneIDs) //Populate planes list.
            {
                m_db.LoadAirplane(id, out planeType, out cruisingSpeed, out fuelCons, out startID);
                planes.Add(new Airplane(id, planeType, cruisingSpeed, fuelCons, startID));
            }
            return planes;
        }

        //Get the distance for a particular route ID.
        //For use when a airplane has been passed to another airport.
        //The new airport does not contain the planes air route id, so must call master and get distance.
        public double GetRouteDist(int airRouteID)
        {
            double rtn = 0;
            foreach (AirRoute ar in m_routes) //Find air route. List is not ordered.
            {
                if (airRouteID == ar.AirRouteID)
                    rtn = ar.DistanceKM;
            }
            return rtn;
        }

        //Move plane from one slave to another.
        public void PassControl(Airplane airplane, int airportID)
        {
            try
            {
                m_ATCSlaves[airportID - 1].AddPlane(airplane);
            }
            catch (Exception ex) //Generic exception for unknown problem.
            {
                System.Console.WriteLine("Failed to pass control of plane.");
                System.Console.WriteLine("Exception type: " + ex.GetType());
                System.Console.WriteLine("Exception message: " + ex.Message);
            }
        }

        /**
         * Method implementations for GUI clients.
         */

        //Get all airports. For displaying list in GUI.
        public Airport[] GetAirports()
        {
            return m_airports;
        }

        //Get all air routes. Used for displaying user friendly airroute for user.
        public List<AirRoute> GetAllRoutes()
        {
            return m_routes;
        }

        //Get list of airplanes owned by airport for display in GUI.
        public List<Airplane> AirportStatus(int airportID)
        {
            return m_ATCSlaves[airportID - 1].GetStatus();
        }

        //Progress simulation by 15 mins.
        //Command each slave to take a step forward in simulation.
        public delegate void TakeStep(); //Delegate definition.
        [MethodImpl(MethodImplOptions.Synchronized)] //Alters state of simulation, therefore access synchronized.
        public void Step()
        {
            TakeStep[] step = new TakeStep[m_ATCSlaves.Length];
            IAsyncResult[] asyncObj = new IAsyncResult[m_ATCSlaves.Length];

            try
            {
                //Call each Slave Callback channel Asynchronously w/ Blocking.
                for (int i = 0; i < m_ATCSlaves.Length; i++)
                {
                    step[i] = m_ATCSlaves[i].Step;
                    asyncObj[i] = step[i].BeginInvoke(null, null); //Make asynch call.
                }
                for (int i = 0; i < asyncObj.Length; i++)
                {
                    step[i].EndInvoke(asyncObj[i]); //Block till complete.
                }
                for (int i = 0; i < asyncObj.Length; i++)
                {
                    asyncObj[i].AsyncWaitHandle.Close(); //Cleanup.
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

        //Outputs status of airports to console. For debugging purposes.
        public void outputAirports()
        {
            List<AirRoute> routes;
            List<Airplane> airplanes;
            try
            {
                for (int i = 0; i < m_ATCSlaves.Length; i++)
                {
                    System.Console.WriteLine("Status of: " + m_ATCSlaves[i].Identify() + " (ID: " + (i + 1) + ")");
                    routes = m_ATCSlaves[i].GetRoutes();
                    airplanes = m_ATCSlaves[i].GetStatus();
                    System.Console.WriteLine(routes.Count + " Routes");
                    foreach (AirRoute ar in routes)
                    {
                        System.Console.WriteLine(ar.toString());
                    }
                    System.Console.WriteLine(airplanes.Count + " Airplanes");
                    foreach (Airplane pl in airplanes)
                    {
                        System.Console.WriteLine(pl.toString());
                    }
                    System.Console.WriteLine(Environment.NewLine);
                }
            }
            catch (Exception ex) //Generic exception for unknown problem.
            {
                System.Console.WriteLine("Exception type: " + ex.GetType());
                System.Console.WriteLine("Exception message: " + ex.Message);
            }
        }
    }
}
