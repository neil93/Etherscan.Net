using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthGetBlockTransactionCountByNumber : EScanRequest
    {
        public string Tag { get; set; }

        internal EScanEthGetBlockTransactionCountByNumber(EScanClient eScanClient, string tag)
            : base(eScanClient, typeof(EScanEthTransactionCount), EScanModules.Proxy, EScanActions.EthGetBlockTransactionCountByNumber)
        {
            this.Tag = tag;
        }
    }
}
