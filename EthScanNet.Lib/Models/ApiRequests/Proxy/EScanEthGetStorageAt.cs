using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthGetStorageAt : EScanRequest
    {
        public string Address { get; set; }
        public string Position { get; set; }
        public string Tag { get; set; }

        internal EScanEthGetStorageAt(EScanClient eScanClient, string address, string position, string tag)
            : base(eScanClient, typeof(EScanEthStorageValue), EScanModules.Proxy, EScanActions.EthGetStorageAt)
        {
            this.Address = address;
            this.Position = position;
            this.Tag = tag;
        }
    }
}
