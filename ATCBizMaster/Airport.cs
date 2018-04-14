using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ATCBizMaster
{
    /**
     * Storage class for airport data.
     */
    [DataContract]
    public class Airport
    {
        [DataMember]
        public int AirportID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public int NextRoute { get; set; } //Position in route array to use next.

        [IgnoreDataMember]
        public List<AirRoute> Routes { get; set; } //AirRoutes of airport.

        [IgnoreDataMember]
        public List<Airplane> Airplanes { get; set; } //Airplanes currently owned.

        public Airport(int inID, string inNm)
        {
            AirportID = inID;
            Name = inNm;
            NextRoute = 0; //Set to first position of route list.
            Routes = new List<AirRoute>();
            Airplanes = new List<Airplane>();
        }

        public String toString() //For debugging purposes.
        {
            return "ID: " + AirportID + ", Name: " + Name;
        }
    }

    /**
     * Storage class for routes.
     */
    [DataContract]
    public class AirRoute
    {
        [DataMember]
        public int AirRouteID { get; set; }

        [DataMember]
        public int FromAirportID { get; set; }

        [DataMember]
        public int ToAirportID { get; set; }

        [DataMember]
        public double DistanceKM { get; set; }

        public AirRoute(int inID, int inFrom, int inTo, double inDist)
        {
            AirRouteID = inID;
            FromAirportID = inFrom;
            ToAirportID = inTo;
            DistanceKM = inDist;
        }

        public String toString() //For debugging purposes.
        {
            return "ID: " + AirRouteID + ", Route: " + FromAirportID + " to " + ToAirportID + ", Distance: " + DistanceKM;
        }
    }

    /**
     * Storage class for airport airplane.
     */
    [DataContract]
    public class Airplane
    {
        [DataMember]
        public int AirplaneID { get; set; }

        [DataMember]
        public string PlaneType { get; set; }

        [DataMember]
        public char State { get; set; } //L = landed, I = in-transit, E = entering circling, C = circling, X = crashed.

        [DataMember]
        public double CruisingKPH { get; set; } //Speed at which airplane flies in KM/H.

        [DataMember]
        public double FuelConsPerHour { get; set; }

        [DataMember]
        public double Fuel { get; set; } //Fuel in tank.

        [DataMember]
        public int CurrentAirRouteID { get; set; } //-1 if landed.

        [DataMember]
        public double DistanceAlongRoute { get; set; } //-1 if landed.

        [DataMember]
        public int CurrentAirportID { get; set; } //-1 if not landed.

        [DataMember]
        public int TimeLanded { get; set; } //In minutes. Must be landed for at least 60 minutes before taking off. -1 if landed.

        public Airplane(int inID, string inType, double inKPH, double inFuel, int inAirport)
        {
            AirplaneID = inID;
            PlaneType = inType;
            State = 'L';
            CruisingKPH = inKPH;
            FuelConsPerHour = inFuel;
            Fuel = 0.0;
            CurrentAirRouteID = -1;
            DistanceAlongRoute = 0.0;
            CurrentAirportID = inAirport;
            TimeLanded = 60; //Assume on initialization that plane is fuelled and ready to take off.
        }

        public String toString() //For debugging purposes.
        {
            return "ID: " + AirplaneID + ", Type: " + PlaneType + ", State " + State + ", Cruising Speed: " + CruisingKPH + ", Fuel Consumption: " + FuelConsPerHour + Environment.NewLine
                   + "Fuel: " + Fuel + ", Current Route: " + CurrentAirRouteID + ", Distance: " + DistanceAlongRoute + ", Current Airport: " + CurrentAirportID + ", Time Landed: " + TimeLanded;
        }
    }
}
