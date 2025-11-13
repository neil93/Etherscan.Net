using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthGasPrice : EScanRequest
    {
        internal EScanEthGasPrice(EScanClient eScanClient)
            : base(eScanClient, typeof(EScanEthGasPriceResult), EScanModules.Proxy, EScanActions.EthGasPrice)
        {
        }
    }
}
