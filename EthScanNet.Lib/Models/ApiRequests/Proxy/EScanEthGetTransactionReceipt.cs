using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthGetTransactionReceipt : EScanRequest
    {
        public string TxHash { get; set; }

        internal EScanEthGetTransactionReceipt(EScanClient eScanClient, string txHash)
            : base(eScanClient, typeof(EScanEthTransactionReceipt), EScanModules.Proxy, EScanActions.EthGetTransactionReceipt)
        {
            this.TxHash = txHash;
        }
    }
}
