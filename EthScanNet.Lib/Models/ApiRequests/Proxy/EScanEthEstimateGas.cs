using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthEstimateGas : EScanRequest
    {
        public string To { get; set; }
        public string Value { get; set; }
        public string GasPrice { get; set; }
        public string Gas { get; set; }
        public string Data { get; set; }

        internal EScanEthEstimateGas(EScanClient eScanClient, string to, string value = null, string gasPrice = null, string gas = null, string data = null)
            : base(eScanClient, typeof(EScanEthEstimateGasResult), EScanModules.Proxy, EScanActions.EthEstimateGas)
        {
            this.To = to;
            this.Value = value;
            this.GasPrice = gasPrice;
            this.Gas = gas;
            this.Data = data;
        }
    }
}
