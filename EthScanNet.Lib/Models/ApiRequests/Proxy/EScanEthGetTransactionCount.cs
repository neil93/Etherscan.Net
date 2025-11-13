using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthGetTransactionCount : EScanRequest
    {
        public string Address { get; set; }
        public string Tag { get; set; }

        internal EScanEthGetTransactionCount(EScanClient eScanClient, string address, string tag)
            : base(eScanClient, typeof(EScanEthTransactionCount), EScanModules.Proxy, EScanActions.EthGetTransactionCount)
        {
            this.Address = address;
            this.Tag = tag;
        }
    }
}
