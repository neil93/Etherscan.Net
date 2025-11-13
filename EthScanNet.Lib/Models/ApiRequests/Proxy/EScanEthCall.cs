using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthCall : EScanRequest
    {
        public string To { get; set; }
        public string Data { get; set; }
        public string Tag { get; set; }

        internal EScanEthCall(EScanClient eScanClient, string to, string data, string tag)
            : base(eScanClient, typeof(EScanEthCallResult), EScanModules.Proxy, EScanActions.EthCall)
        {
            this.To = to;
            this.Data = data;
            this.Tag = tag;
        }
    }
}
