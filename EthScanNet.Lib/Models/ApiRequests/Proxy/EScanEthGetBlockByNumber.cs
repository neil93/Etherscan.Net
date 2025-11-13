using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthGetBlockByNumber : EScanRequest
    {
        public string Tag { get; set; }
        public string Boolean { get; set; }

        internal EScanEthGetBlockByNumber(EScanClient eScanClient, string tag, bool fullTransactionObjects = false)
            : base(eScanClient, typeof(EScanEthBlock), EScanModules.Proxy, EScanActions.EthGetBlockByNumber)
        {
            this.Tag = tag;
            this.Boolean = fullTransactionObjects.ToString().ToLower();
        }
    }
}
