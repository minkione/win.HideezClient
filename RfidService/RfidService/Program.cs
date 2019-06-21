using System.ServiceProcess;

namespace Hideez.RFID
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new RfidService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
