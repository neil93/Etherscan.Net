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
            string apiKey = "BSSW4GUFFWEHWB8V4T6S66VFDEUXZ5RAEM";

            EScanNetwork eScanNetwork = new("https://api-rinkeby.etherscan.io/api");
            EtherscanDemo demo = new EtherscanDemo(apiKey, EScanNetwork.PolygonAmy);

            try
            {
                await demo.RunApiCommandsAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}