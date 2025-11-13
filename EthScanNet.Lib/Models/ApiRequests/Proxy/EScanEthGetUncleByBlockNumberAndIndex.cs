using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthGetUncleByBlockNumberAndIndex : EScanRequest
    {
        public string Tag { get; set; }
        public string Index { get; set; }

        internal EScanEthGetUncleByBlockNumberAndIndex(EScanClient eScanClient, string tag, string index)
            : base(eScanClient, typeof(EScanEthBlock), EScanModules.Proxy, EScanActions.EthGetUncleByBlockNumberAndIndex)
        {
            this.Tag = tag;
            this.Index = index;
        }
    }
}
