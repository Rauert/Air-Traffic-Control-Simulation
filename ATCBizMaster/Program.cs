using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace ATCBizMaster
{
    /**
     * Initialize master business controller and slaves.
     */
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ATCBizControllerMasterImpl ATCBizContManImpl = new ATCBizControllerMasterImpl(); //Create instance of singleton server object.
                ServiceHost host;
                host = new ServiceHost(ATCBizContManImpl);
                NetTcpBinding tcpBinding = new NetTcpBinding();
                tcpBinding.MaxReceivedMessageSize = System.Int32.MaxValue; //Change max message & array size.
                tcpBinding.ReaderQuotas.MaxArrayLength = System.Int32.MaxValue;
                host.AddServiceEndpoint(typeof(IATCBizControllerMaster), tcpBinding, "net.tcp://localhost:50001/ATCBiz");
                host.Open(); //Begin hosting service.
                ATCBizContManImpl.Initialize(); //Initialize airports for offload.
                //for (int i = 0; i < 4; i++) //Used to initialize slaves before split.
                //{
                //    new ATCBizControllerSlaveImpl();
                //}
                System.Console.WriteLine(Environment.NewLine + "Press enter to exit.");
                System.Console.ReadLine(); //Prevent server from shutting down.
                System.Console.WriteLine(Environment.NewLine + "Shutting down..." + Environment.NewLine);
                host.Close();
            }
            catch (FaultException ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            catch (Exception ex) //Generic exception for unknown problem.
            {
                System.Console.WriteLine("Exception type: " + ex.GetType());
                System.Console.WriteLine("Exception message: " + ex.Message);
            }
        }
    }
}
