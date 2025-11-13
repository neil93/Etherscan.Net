using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthSendRawTransaction : EScanRequest
    {
        public string Hex { get; set; }

        internal EScanEthSendRawTransaction(EScanClient eScanClient, string hex)
            : base(eScanClient, typeof(EScanEthTransactionHash), EScanModules.Proxy, EScanActions.EthSendRawTransaction)
        {
            this.Hex = hex;
        }
    }
}
