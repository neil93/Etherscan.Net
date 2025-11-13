using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthGetCode : EScanRequest
    {
        public string Address { get; set; }
        public string Tag { get; set; }

        internal EScanEthGetCode(EScanClient eScanClient, string address, string tag)
            : base(eScanClient, typeof(EScanEthCode), EScanModules.Proxy, EScanActions.EthGetCode)
        {
            this.Address = address;
            this.Tag = tag;
        }
    }
}
