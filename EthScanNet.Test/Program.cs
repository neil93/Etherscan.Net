using EthScanNet.Lib;

using System;
using System.Threading.Tasks;

namespace EthScanNet.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RunApiCommands().Wait();
        }

        private static async Task RunApiCommands()
        {
            try
            {
                string apiKey = "BSSW4GUFFWEHWB8V4T6S66VFDEUXZ5RAEM";
                EtherscanDemo demo = new EtherscanDemo(apiKey, EScanNetwork.PolygonAmy);
                await demo.RunApiCommandsAsync();


                //var mainnetRpc = "https://polygon-bor-rpc.publicnode.com";
                //var amoyRpc = "https://polygon-amoy-bor-rpc.publicnode.com";
                //NethereumDemo nethereumDemo = new NethereumDemo(mainnetRpc, false);
                ////NethereumDemo nethereumDemo = new NethereumDemo(amoyRpc);
                //await nethereumDemo.RunApiCommandsAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}