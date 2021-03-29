using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using System;
using System.Threading.Tasks;
using WinBle;

namespace ConsoleSampleApp
{
    class Program
    {
        private static WinBleConnectionManager _cm;
        private static bool _first = true;

        static void Main(string[] args)
        {
            Console.WriteLine("Started");

            try
            {
                var log = new EventLogger("ExampleApp");

                _cm = new WinBleConnectionManager(log);
                _cm.Start();

                _cm.ControllerAdded += _cm_ControllerAdded;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var cmd = Console.ReadLine();
            while (cmd != "q")
            {
                //if (cmd == "b")

            }
        }

        static void _cm_ControllerAdded(object sender, ControllerAddedEventArgs e)
        {
            Console.WriteLine(">>>> Added connection controller");

            if (!_first)
            {
                _first = false;

                Task.Run(async () =>
                {
                    Random rnd = new Random();

                    for (int i = 0; i < 10000; i++)
                    {
                        try
                        {
                            await _cm.Disconnect(e.Controller);
                            await Task.Delay(rnd.Next(1, 120) * 1000);
                            await _cm.Connect(e.Controller.Connection.ConnectionId);


                            //var t1 = _cm.Disconnect(e.Controller);
                            //var t2 = _cm.Connect(e.Controller.Connection.ConnectionId);
                            //var t3 = _cm.Connect(e.Controller.Connection.ConnectionId);
                            //var t4 = _cm.Connect(e.Controller.Connection.ConnectionId);

                            //await Task.WhenAll(t2, t3, t4);

                            await Task.Delay(rnd.Next(1, 120) * 1000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                });
            }
        }
    }
}
