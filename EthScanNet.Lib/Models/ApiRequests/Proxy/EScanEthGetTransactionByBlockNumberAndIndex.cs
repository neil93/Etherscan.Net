using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthGetTransactionByBlockNumberAndIndex : EScanRequest
    {
        public string Tag { get; set; }
        public string Index { get; set; }

        internal EScanEthGetTransactionByBlockNumberAndIndex(EScanClient eScanClient, string tag, string index)
            : base(eScanClient, typeof(EScanEthTransaction), EScanModules.Proxy, EScanActions.EthGetTransactionByBlockNumberAndIndex)
        {
            this.Tag = tag;
            this.Index = index;
        }
    }
}
