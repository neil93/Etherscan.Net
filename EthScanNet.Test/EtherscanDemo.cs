using EthScanNet.Lib;
using EthScanNet.Lib.EScanApi;
using EthScanNet.Lib.Models.ApiRequests.Contracts;
using EthScanNet.Lib.Models.ApiResponses.Accounts;
using EthScanNet.Lib.Models.ApiResponses.Contracts;
using EthScanNet.Lib.Models.ApiResponses.Logs;
using EthScanNet.Lib.Models.ApiResponses.Proxy;
using EthScanNet.Lib.Models.ApiResponses.Stats;
using EthScanNet.Lib.Models.ApiResponses.Tokens;
using EthScanNet.Lib.Models.EScan;
using EthScanNet.Lib.Models.Events;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Model;
using Nethereum.RPC.Eth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EthScanNet.Test
{
    public class EtherscanDemo
    {
        private readonly string _apiKey = "BSSW4GUFFWEHWB8V4T6S66VFDEUXZ5RAEM";
        private readonly EScanNetwork _network = EScanNetwork.PolygonMainNet;

        public EtherscanDemo(string apiKey, EScanNetwork network)
        {
            this._apiKey = apiKey ?? "BSSW4GUFFWEHWB8V4T6S66VFDEUXZ5RAEM";
            this._network = network ?? EScanNetwork.PolygonMainNet;
        }

        public async Task RunApiCommandsAsync()
        {
            Console.WriteLine("Running EtherscanDemo with APIKey: " + this._apiKey);
            EScanClient client = new(EScanNetwork.PolygonMainNet, "BSSW4GUFFWEHWB8V4T6S66VFDEUXZ5RAEM");

            try
            {
                //await RunAccountCommandsAsync(client);
                //await RunTokenCommandsAsync(client);
                //await RunStatsCommandsAsync(client);
                //await RunContractCommandsAsync(client);
                //await RunLogsCommandsAsync(client);
                //await RunProxyCommandsAsync(client);
                await RunProxyTestCommandsAsync(client);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task RunAccountCommandsAsync(EScanClient client)
        {
            Console.WriteLine("Account test started");
            EScanBalance apiBalance = await client.Accounts.GetBalanceAsync(new("0x0000000000000000000000000000000000001004"));
            Console.WriteLine("GetBalanceAsync: " + apiBalance.Message);
            EScanTransactions normalApiTransaction = await client.Accounts.GetNormalTransactionsAsync(new("0x0000000000000000000000000000000000001004"));
            Console.WriteLine("GetNormalTransactionsAsync: " + normalApiTransaction.Message);
            EScanTransactions internalApiTransaction = await client.Accounts.GetInternalTransactionsAsync(new("0x0000000000000000000000000000000000001004"));
            Console.WriteLine("GetInternalTransactionsAsync: " + internalApiTransaction.Message);
            EScanMinedBlocks apiMinedBlocks = await client.Accounts.GetMinedBlocksAsync(new("0x78f3adfc719c99674c072166708589033e2d9afe"));
            Console.WriteLine("GetMinedBlocksAsync: " + apiMinedBlocks.Message);
            EScanTokenTransferEvents apiTokenTransferEvents = await client.Accounts.GetTokenEvents(new("0xf09f5e21f86692c614d2d7b47e3b9729dc1c436f"));
            Console.WriteLine("GetTokenEvents: " + apiTokenTransferEvents.Message);
            Console.WriteLine("Account test complete");
            EScanERC20TokenTransferEvents apiERC20TokenTransferEvents = await client.Accounts.GetERC20TokenEvents(new("0xf09f5e21f86692c614d2d7b47e3b9729dc1c436f"));
            Console.WriteLine("GetTokenEvents: " + apiTokenTransferEvents.Message);
            Console.WriteLine("Account test complete");
        }

        private async Task RunTokenCommandsAsync(EScanClient client)
        {
            EScanAddress holderAddress = new("0x2b7fc60fd13f32fed8730113a14e3468d2f17cdc");
            EScanAddress contractAddress = new("0xf7844cb890f4c339c497aeab599abdc3c874b67a");
            Console.WriteLine("Token test started");
            EScanTokenSupply apiTokenSupplyM = await client.Tokens.GetMaxSupply(contractAddress);
            Console.WriteLine("GetMaxSupply: " + apiTokenSupplyM.Message);
            EScanBalance balance = await client.Accounts.GetTokenBalanceForAddress(holderAddress, contractAddress);
            Console.WriteLine("GetTokenBalanceForAddress: " + balance.Message);
            Console.WriteLine("Token test complete");
        }

        private async Task RunStatsCommandsAsync(EScanClient client)
        {
            Console.WriteLine("Stats test started");
            EScanTotalCoinSupply totalSupply = await client.Stats.GetTotalSupply();
            Console.WriteLine("GetTotalSupply: " + totalSupply.Message);
            Console.WriteLine("Stats test complete");
        }

        private async Task RunContractCommandsAsync(EScanClient client)
        {
            Console.WriteLine("Contracts test started");

            EScanAddress contractAddress = new EScanAddress("0xfB6916095ca1df60bB79Ce92cE3Ea74c37c5d359");
            EScanAbiResponse abiResponse = await client.Contracts.GetAbiAsync(contractAddress);
            Console.WriteLine("ABI: " + abiResponse.Message);

            EScanSourceCodeResponse sourceCodeResponse = await client.Contracts.GetSourceCodeAsync(contractAddress);
            Console.WriteLine("Source Code: " + sourceCodeResponse.Message);

            // EScanNetwork.RinkebyNet
            string guid = "brv6gjya7rne8rvyniysycu8qcvb5nqn49akwx7wdxgx5udpgj";
            EScanSourceCodeVerificationStatusResponse verificationStatusResponse = await client.Contracts.GetSourceCodeVerificationStatusAsync(guid);
            Console.WriteLine("Verification status: " + verificationStatusResponse.Message);

            var verificationPayload = new EScanContractCodeVerificationModel
            {
                ContractAddress = "0x29137a31592885EF4E6Ab2C1A7BB81d0D4311954",
                SourceCode = @"
                // SPDX-License-Identifier: MIT

                pragma solidity >=0.7.0 <0.9.0;

                contract Storage {
                    uint256 number;

                    constructor(uint defaultNum_) {
                        number = defaultNum_;
                    }

                    function store(uint256 num) public {
                        number = num;
                    }

                    function retrieve() public view returns (uint256){
                        return number;
                    }
                }",
                CodeFormat = "solidity-single-file",
                ContractName = "Storage",
                CompilerVersion = "v0.8.7+commit.e28d00a7",
                OptimizationUsed = "1",
                Runs = "200",
                ContstructorArguments = "uint defaultNum_",
                EvmVersion = "3",
                LicenseType = "1"
            };
            EScanSourceCodeVerificationResponse verificationResponse = await client.Contracts.VerifySmartContractAsync(verificationPayload);
            Console.WriteLine("Verification: " + verificationResponse.Guid);

            Console.WriteLine("Contracts test complete");
        }

        private async Task RunLogsCommandsAsync(EScanClient client)
        {
            var bindWalletTopic0 = "0x0ca052931610b15a08f6d7b445a2be5e2d377dd2c8945678bb64fbecb2725708";
            var transferTopic0 = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";
            var redeemTopic0 = "0x378f55a9a0032096f81e501f6fba06e54947e956df2afe99d645ca71183fb269";
            var preSignedTopic0 = "0xbb8f597c6a23e718c7579b21e311c3daf7851a8456dbb20e97b3124cd3a66022";

            var usdcContractAddress = "0x3c499c542cef5e3811e1192ce70d8cc03d5c3359";     // USDC �X���a�}

            Console.WriteLine("Logs test started");
            //EScanLogs logs = await client.Logs.GetLogsAsync(fromBlock: "0x1", toBlock: "latest", topic0: "0xbb8f597c6a23e718c7579b21e311c3daf7851a8456dbb20e97b3124cd3a66022", page:1,offset:100);

            // ��b
            EScanLogs logs = await client.Logs.GetLogsAsync(fromBlock: "78830144", toBlock: "78830144", topic0: transferTopic0, page: 1, offset: 10000);
            var transferEvent = await GetBoundWalletEvent<UsdcEventTransfer>(logs);

            var eoaAddress = new string[] { "0xF177B7F19aD64a9C04a45cd9E41505b1c9A5B4C6", "0xD02a7763cac2c95D013fBE8A93e406f37F83294f" };   // EOA

            var walletContractAddress = new string[] { "0x70D74B6548C0E8c524b2b2B0997E3E539C93D72d", "0x96e52de6892d4B4811cEaa929E912cCd90fd6041", "0x0F7B6aC80951B68301b4321a7D34f76E03AF06Fe" };  // �ϥΪ̿��]�X��

            var qq = transferEvent.Where(e => !eoaAddress.Contains(e.Event.From)
                && e.Log.Address == usdcContractAddress
                && walletContractAddress.Contains(e.Event.To));

            foreach (var item in qq)
            {
                Console.WriteLine($"����:{item.Event.To},��b: {item.Event.Value / 1000000} Usd.");
            }

            Console.WriteLine("Logs transferEvent test complete");

            // ū�^
            logs = await client.Logs.GetLogsAsync(fromBlock: "78535606", toBlock: "78535606", topic0: redeemTopic0, page: 1, offset: 1000);
            var redeemedEvent = await GetBoundWalletEvent<WalletContractEventRedeemed>(logs);
            Console.WriteLine("Logs redeemedEvent  test complete");

            // �Ѹj/�j�w���]
            logs = await client.Logs.GetLogsAsync(fromBlock: "78535522", toBlock: "78535522", topic0: bindWalletTopic0, page: 1, offset: 1000);
            var boundWalletEvent = await GetBoundWalletEvent<WalletContractEventWalletBound>(logs);
            Console.WriteLine("Logs boundWalletEvent test complete");

            // �wñ�W
            logs = await client.Logs.GetLogsAsync(fromBlock: "78533675", toBlock: "78533675", topic0: preSignedTopic0, page: 1, offset: 1000);
            var preSignedEvent = await GetBoundWalletEvent<WalletContractEventPreSigned>(logs);
            Console.WriteLine("Logs boundWalletEvent test complete");

            Console.WriteLine("GetLogsAsync: " + logs.Message);
            Console.WriteLine("All Logs test complete");
        }

        private async Task RunProxyTestCommandsAsync(EScanClient client)
        {
            var usdcContract = "0x3c499c542cef5e3811e1192ce70d8cc03d5c3359";
            var eoaAddress = new string[] { "0xF177B7F19aD64a9C04a45cd9E41505b1c9A5B4C6", "0xD02a7763cac2c95D013fBE8A93e406f37F83294f" };   // EOA
            var walletContractAddress = new string[] { "0x70D74B6548C0E8c524b2b2B0997E3E539C93D72d", "0x96e52de6892d4B4811cEaa929E912cCd90fd6041", "0x0F7B6aC80951B68301b4321a7D34f76E03AF06Fe", "0xa3F2E192415934368EfdD420bd3196fA53988C5C" };

            // 質押交易

            //// 取得區塊資訊
            //var blockResponse = await client.Proxy.EthGetBlockByNumber("0x4b2da5b", true);
            //var blockInfo = blockResponse.GetBlockInfo();

            //// 取得質押USDC合約
            //var stakeTransactions = blockInfo.Transactions.Where(t => t.To != null && t.To.Equals(usdcContract, StringComparison.OrdinalIgnoreCase)).ToList();

            //foreach (var stake in stakeTransactions)
            //{
            //    // 取得交易收據
            //    var receiptResponse = await client.Proxy.EthGetTransactionReceipt(stake.Hash);
            //    var receiptInfos = receiptResponse.GetReceiptInfo();

            //    var logs = receiptInfos.Logs.Where(l => l.Address.Equals(usdcContract, StringComparison.OrdinalIgnoreCase));
            //    var transferEvent = ConvertLogsToEvent<UsdcEventTransfer>(logs);
            //    var resultEvents  = transferEvent.Where(e => e.Log.Address.Equals(usdcContract, StringComparison.OrdinalIgnoreCase) && !eoaAddress.Contains(e.Event.From) && walletContractAddress.Contains(e.Event.To, StringComparer.OrdinalIgnoreCase)).ToList();

            //    foreach (var transfer in resultEvents)
            //    {
            //        Console.WriteLine($"Stake From: {transfer.Event.From}, To: {transfer.Event.To}, Value: {transfer.Event.Value}");
            //    }
            //}

            // 贖回交易
            // 取得區塊資訊
            var blockRedeemResponse = await client.Proxy.EthGetBlockByNumber("0x4ae5bb6", true);
            var blockRedeemInfo = blockRedeemResponse.GetBlockInfo();
            var redeemTransactions = blockRedeemInfo.Transactions.Where(t => walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase)).ToList();
            foreach (var redeemTransaction in redeemTransactions)
            {
                // 取得交易收據
                var receiptResponse = await client.Proxy.EthGetTransactionReceipt(redeemTransaction.Hash);
                var receiptInfos = receiptResponse.GetReceiptInfo();

                var redeemEvents = ConvertLogsToEvent<WalletContractEventRedeemed>(receiptInfos.Logs);

                foreach (var redeem in redeemEvents)
                {
                    Console.WriteLine($"Redeem From: {redeem.Event.WalletContract}, To: {redeem.Event.Wallet}, Value: {redeem.Event.AmountInDecimal}");
                }
            }
        }

        private async Task RunProxyCommandsAsync(EScanClient client)
        {
            Console.WriteLine("=== Proxy API Tests ===\n");

            // 1. Get current block number
            Console.WriteLine("1. Current Block Number:");
            var currentBlock = await client.Proxy.EthBlockNumber();
            Console.WriteLine($"   Hex: {currentBlock.GetBlockNumberHex()}");
            Console.WriteLine($"   Decimal: {currentBlock.GetBlockNumber()}\n");

            // 2. Get block by number
            Console.WriteLine("2. Block Information:");
            var blockResponse = await client.Proxy.EthGetBlockByNumber("0x4b2da5b", true);
            var blockInfo = blockResponse.GetBlockInfo();

            if (blockInfo != null)
            {
                Console.WriteLine($"   Block Number: {blockInfo.Number}");
                Console.WriteLine($"   Block Hash: {blockInfo.Hash}");
                Console.WriteLine($"   Miner: {blockInfo.Miner}");
                Console.WriteLine($"   Timestamp: {blockInfo.Timestamp}");
                Console.WriteLine($"   Gas Used: {blockInfo.GasUsed} / {blockInfo.GasLimit}");
                Console.WriteLine($"   Transaction Count: {blockInfo.Transactions?.Count ?? 0}\n");
            }

            // 3. Get block transaction count
            Console.WriteLine("3. Block Transaction Count:");
            var txCount = await client.Proxy.EthGetBlockTransactionCountByNumber("0x4b2da5b");
            Console.WriteLine($"   Hex: {txCount.GetCountHex()}");
            Console.WriteLine($"   Decimal: {txCount.GetCount()}\n");

            // 4. Get transaction by hash
            Console.WriteLine("4. Transaction Information:");
            var txResponse = await client.Proxy.EthGetTransactionByHash("0x27b7bd4f7b0ab41ee3e8054df3a3def1e2851707ed5bbb304e3e75e49b34e9ec");
            var txInfo = txResponse.GetTransactionInfo();

            if (txInfo != null)
            {
                Console.WriteLine($"   Transaction Hash: {txInfo.Hash}");
                Console.WriteLine($"   From: {txInfo.From}");
                Console.WriteLine($"   To: {txInfo.To}");
                Console.WriteLine($"   Value: {txInfo.Value}");
                Console.WriteLine($"   Gas: {txInfo.Gas}");
                Console.WriteLine($"   Gas Price: {txInfo.GasPrice}\n");
            }

            // 5. Get transaction receipt
            Console.WriteLine("5. Transaction Receipt:");
            var receiptResponse = await client.Proxy.EthGetTransactionReceipt("0x27b7bd4f7b0ab41ee3e8054df3a3def1e2851707ed5bbb304e3e75e49b34e9ec");
            var receiptInfo = receiptResponse.GetReceiptInfo();

            if (receiptInfo != null)
            {
                Console.WriteLine($"   Transaction Hash: {receiptInfo.TransactionHash}");
                Console.WriteLine($"   Status: {receiptInfo.Status}");
                Console.WriteLine($"   Block Number: {receiptInfo.BlockNumber}");
                Console.WriteLine($"   From: {receiptInfo.From}");
                Console.WriteLine($"   To: {receiptInfo.To}");
                Console.WriteLine($"   Gas Used: {receiptInfo.GasUsed}");
                Console.WriteLine($"   Contract Address: {receiptInfo.ContractAddress ?? "N/A"}");
                Console.WriteLine($"   Logs Count: {receiptInfo.Logs?.Count ?? 0}\n");
            }

            // 6. Get transaction count for address
            Console.WriteLine("6. Address Transaction Count:");
            var addrTxCount = await client.Proxy.EthGetTransactionCount("0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb", "latest");
            Console.WriteLine($"   Hex: {addrTxCount.GetCountHex()}");
            Console.WriteLine($"   Decimal: {addrTxCount.GetCount()}\n");

            // 7. Get gas price
            Console.WriteLine("7. Current Gas Price:");
            var gasPriceResponse = await client.Proxy.EthGasPrice();
            Console.WriteLine($"   Hex (Wei): {gasPriceResponse.GetGasPriceHex()}");
            Console.WriteLine($"   Wei: {gasPriceResponse.GetGasPrice()}");
            Console.WriteLine($"   Gwei: {gasPriceResponse.GetGasPriceGwei()}\n");

            // 8. Get code (check if address is contract)
            Console.WriteLine("8. Check if Address is Contract:");
            var codeResponse = await client.Proxy.EthGetCode("0x3c499c542cef5e3811e1192ce70d8cc03d5c3359", "latest");
            Console.WriteLine($"   Is Contract: {codeResponse.IsContract()}");
            Console.WriteLine($"   Code Length: {codeResponse.GetCode()?.Length ?? 0} characters\n");

            // 9. Estimate gas
            Console.WriteLine("9. Estimate Gas:");
            var estimateGasResponse = await client.Proxy.EthEstimateGas(
                to: "0x3c499c542cef5e3811e1192ce70d8cc03d5c3359",
                value: "0x0",
                data: "0x70a08231000000000000000000000000742d35Cc6634C0532925a3b844Bc9e7595f0bEb"
            );
            Console.WriteLine($"   Hex: {estimateGasResponse.GetEstimatedGasHex()}");
            Console.WriteLine($"   Decimal: {estimateGasResponse.GetEstimatedGas()}\n");

            Console.WriteLine("=== All Proxy Tests Complete ===");
        }

        /// <summary>
        /// 建立Event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logs"></param>
        /// <returns></returns>
        private Task<List<EventLog<T>>> GetBoundWalletEvent<T>(EScanLogs logs) where T : IEventDTO, new()
        {
            var filterLogs = new List<FilterLog>();
            filterLogs.AddRange(logs.Logs.Select(l => new FilterLog()
            {
                BlockNumber = new HexBigInteger(l.BlockNumber),
                BlockHash = l.BlockHash,
                LogIndex = new HexBigInteger(l.LogIndex),
                Address = l.Address,
                Data = l.Data,
                Topics = l.Topics.Cast<object>().ToArray(),
                TransactionHash = l.TransactionHash,
                TransactionIndex = new HexBigInteger(l.TransactionIndex),
            }));
            var decoded = Event<T>.DecodeAllEvents(filterLogs.ToArray());
            return Task.FromResult(decoded);
        }

        /// <summary>
        /// 轉換 LogInfo 為 EventLog
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logs"></param>
        /// <returns></returns>
        private List<EventLog<T>> ConvertLogsToEvent<T>(IEnumerable<LogInfo> logs) where T : IEventDTO, new()
        {
            var filterLogs = new List<FilterLog>();
            filterLogs.AddRange(logs.Select(l => new FilterLog()
            {
                BlockNumber = new HexBigInteger(l.BlockNumber),
                BlockHash = l.BlockHash,
                LogIndex = new HexBigInteger(l.LogIndex),
                Address = l.Address,
                Data = l.Data,
                Topics = l.Topics.Cast<object>().ToArray(),
                TransactionHash = l.TransactionHash,
                TransactionIndex = new HexBigInteger(l.TransactionIndex),
            }));
            var decoded = Event<T>.DecodeAllEvents(filterLogs.ToArray());
            return decoded;
        }
    }
}