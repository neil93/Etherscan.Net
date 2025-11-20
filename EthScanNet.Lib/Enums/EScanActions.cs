namespace EthScanNet.Lib.Enums
{
    // ReSharper disable once IdentifierTypo
    internal class EScanActions
    {
        public static readonly EScanActions Balance = new("Balance");
        public static readonly EScanActions BalanceMulti = new("BalanceMulti");
        public static readonly EScanActions TxList = new("TxList");
        public static readonly EScanActions TxListInternal = new("TxListInternal");
        public static readonly EScanActions TokenNftTx = new("TokenNftTx");
        public static readonly EScanActions GetMinedBlocks = new("GetMinedBlocks");
        public static readonly EScanActions TokenSupply = new("TokenSupply");
        public static readonly EScanActions TokenCirculatingSupply = new("TokenCSupply");
        public static readonly EScanActions TokenBalance = new("TokenBalance");
        public static readonly EScanActions TxErc20Token = new("TokenTx");
        public static readonly EScanActions GetLogs = new("GetLogs");
        public static readonly EScanActions GetContractAbi = new("GetAbi");
        public static readonly EScanActions GetContractSourceCode = new("GetSourceCode");
        public static readonly EScanActions VerifySourceCode = new("VerifySourceCode");
        public static readonly EScanActions CheckCodeVerificationStatus = new("CheckVerifyStatus");
        public static readonly EScanActions GasEstimate = new("GasEstimate");
        public static readonly EScanActions GasTracker = new("GasTracker");
        public static readonly EScanActions EthBlockNumber = new("Eth_BlockNumber");
        public static readonly EScanActions EthGetBlockByNumber = new("Eth_GetBlockByNumber");
        public static readonly EScanActions EthGetUncleByBlockNumberAndIndex = new("Eth_GetUncleByBlockNumberAndIndex");
        public static readonly EScanActions EthGetBlockTransactionCountByNumber = new("Eth_GetBlockTransactionCountByNumber");
        public static readonly EScanActions EthGetTransactionByHash = new("Eth_GetTransactionByHash");
        public static readonly EScanActions EthGetTransactionByBlockNumberAndIndex = new("Eth_GetTransactionByBlockNumberAndIndex");
        public static readonly EScanActions EthGetTransactionCount = new("Eth_GetTransactionCount");
        public static readonly EScanActions EthSendRawTransaction = new("Eth_SendRawTransaction");
        public static readonly EScanActions EthGetTransactionReceipt = new("Eth_GetTransactionReceipt");
        public static readonly EScanActions EthCall = new("Eth_Call");
        public static readonly EScanActions EthGetCode = new("Eth_GetCode");
        public static readonly EScanActions EthGetStorageAt = new("Eth_GetStorageAt");
        public static readonly EScanActions EthGasPrice = new("Eth_GasPrice");
        public static readonly EScanActions EthEstimateGas = new("Eth_EstimateGas");

        public static class BncScanSpecific
        {
            public static readonly EScanActions BnbSupply = new("BnbSupply");
        }

        public static class EtherscanSpecific
        {
            public static readonly EScanActions EthSupply = new("EthSupply");
        }

        private readonly string _actionName;

        private EScanActions(string networkString)
        {
            this._actionName = networkString.ToLower();
        }

        public override string ToString()
        {
            return this._actionName;
        }

        public static implicit operator string(EScanActions action)
        {
            return action._actionName;
        }
    }
}