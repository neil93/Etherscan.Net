using System;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Proxy;

namespace EthScanNet.Lib.Models.ApiRequests.Proxy
{
    internal class EScanEthGetTransactionByHash : EScanRequest
    {
        public string TxHash { get; set; }

        internal EScanEthGetTransactionByHash(EScanClient eScanClient, string txHash)
            : base(eScanClient, typeof(EScanEthTransaction), EScanModules.Proxy, EScanActions.EthGetTransactionByHash)
        {
            this.TxHash = txHash;
        }
    }
}
