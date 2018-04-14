using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace ATCBizMaster
{
    /**
     * Business master controller interface. Contains functions for slaves and GUI's.
     */
    [ServiceContract(CallbackContract = typeof(IATCBizControllerMasterCallback))]
    public interface IATCBizControllerMaster
    {
        /**
         * Slave interface functions.
         */

        [OperationContract]
        Airport RegisterSlave(); //Register new slave in masters list.

        [OperationContract]
        List<AirRoute> GetRoutes(int airportID); //Get routes for ATC slave.

        [OperationContract]
        List<Airplane> GetAirplanes(int airportID); //Get airplanes for ATC slave.

        [OperationContract]
        double GetRouteDist(int airRouteID); //Get airRoute distance for inbound plane for ATC slave.

        [OperationContract]
        void PassControl(Airplane airplane, int airportID); //Passes an airplanes control from one airport to another.

        /**
         * GUI interface functions.
         */

        [OperationContract]
        Airport[] GetAirports(); //Get list of airports to display in GUI.

        [OperationContract]
        List<AirRoute> GetAllRoutes(); //Get list of airroutes for displaying user friendly airroute info.

        [OperationContract]
        List<Airplane> AirportStatus(int airportID); //Get airports current status. For update of GUI lists.

        [OperationContract]
        void Step(); //Ask slaves to take a step forward in the simulation.
    }

    /**
     * Business controller slave interface. Contains functions to receive commands from master.
     * Gui's implement a dummy version (No functionality) because a duplex connection is used.
     */
    [ServiceContract]
    public interface IATCBizControllerMasterCallback
    {
        [OperationContract]
        void Step(); //Take a step forward in the simulation.

        [OperationContract]
        void AddPlane(Airplane airplane); //Gain control of inbound airplane.

        [OperationContract]
        List<Airplane> GetStatus(); //Get list of planes for display in GUI.

        [OperationContract]
        List<AirRoute> GetRoutes(); //Get list of routes for airport. For debugging purposes.

        [OperationContract]
        string Identify(); //Returns name of slave. For debugging purposes.
    }
}
