using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace EthScanNet.Lib.Models.ApiResponses.Proxy
{
    public class EScanEthTransactionReceipt : EScanJsonRpcResponse
    {
        /// <summary>
        /// Get the transaction receipt as a strongly-typed TransactionReceiptInfo object
        /// </summary>
        public TransactionReceiptInfo GetReceiptInfo()
        {
            if (Result == null) return null;
            
            var json = JsonConvert.SerializeObject(Result);
            return JsonConvert.DeserializeObject<TransactionReceiptInfo>(json);
        }

        /// <summary>
        /// Get the transaction receipt as a Nethereum TransactionReceipt object
        /// </summary>
        public TransactionReceipt GetTransactionReceipt()
        {
            var receiptInfo = GetReceiptInfo();
            if (receiptInfo == null) return null;

            var filterLogs = receiptInfo.Logs?.Select(l => new
            {
                address = l.Address,
                topics = l.Topics,
                data = l.Data,
                blockNumber = l.BlockNumber,
                transactionHash = l.TransactionHash,
                transactionIndex = l.TransactionIndex,
                blockHash = l.BlockHash,
                logIndex = l.LogIndex,
                removed = l.Removed
            }).ToArray();

            var receipt = new TransactionReceipt
            {
                TransactionHash = receiptInfo.TransactionHash,
                TransactionIndex = new HexBigInteger(receiptInfo.TransactionIndex),
                BlockHash = receiptInfo.BlockHash,
                BlockNumber = new HexBigInteger(receiptInfo.BlockNumber),
                CumulativeGasUsed = new HexBigInteger(receiptInfo.CumulativeGasUsed),
                GasUsed = new HexBigInteger(receiptInfo.GasUsed),
                ContractAddress = receiptInfo.ContractAddress,
                Status = new HexBigInteger(receiptInfo.Status),
                LogsBloom = receiptInfo.LogsBloom,
                From = receiptInfo.From,
                To = receiptInfo.To,
                Type = new HexBigInteger(receiptInfo.Type),
                EffectiveGasPrice = new HexBigInteger(receiptInfo.EffectiveGasPrice),
                Logs = filterLogs != null ? JArray.FromObject(filterLogs) : null
            };

            return receipt;
        }
    }
}
