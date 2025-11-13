using EthScanNet.Lib;
using EthScanNet.Lib.Models.ApiRequests.Contracts;
using EthScanNet.Lib.Models.ApiResponses.Accounts;
using EthScanNet.Lib.Models.ApiResponses.Contracts;
using EthScanNet.Lib.Models.ApiResponses.Logs;
using EthScanNet.Lib.Models.ApiResponses.Stats;
using EthScanNet.Lib.Models.ApiResponses.Tokens;
using EthScanNet.Lib.Models.EScan;
using EthScanNet.Lib.Models.Events;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
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
                await RunProxyCommandsAsync(client);
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
            Console.WriteLine("GetBalanceAsync: " +  apiBalance.Message);
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

            var usdcContractAddress = "0x3c499c542cef5e3811e1192ce70d8cc03d5c3359";     // USDC 合約地址

            Console.WriteLine("Logs test started");
            //EScanLogs logs = await client.Logs.GetLogsAsync(fromBlock: "0x1", toBlock: "latest", topic0: "0xbb8f597c6a23e718c7579b21e311c3daf7851a8456dbb20e97b3124cd3a66022", page:1,offset:100);

            // 轉帳
            EScanLogs logs = await client.Logs.GetLogsAsync(fromBlock: "78830144", toBlock: "78830144", topic0: transferTopic0, page: 1, offset: 10000);
            var transferEvent = await GetBoundWalletEvent<UsdcEventTransfer>(logs);

            var eoaAddress = new string[] { "0xF177B7F19aD64a9C04a45cd9E41505b1c9A5B4C6", "0xD02a7763cac2c95D013fBE8A93e406f37F83294f" };   // EOA

            var walletContractAddress = new string[] { "0x70D74B6548C0E8c524b2b2B0997E3E539C93D72d", "0x96e52de6892d4B4811cEaa929E912cCd90fd6041", "0x0F7B6aC80951B68301b4321a7D34f76E03AF06Fe" };  // 使用者錢包合約

            var qq = transferEvent.Where(e => !eoaAddress.Contains(e.Event.From)
                && e.Log.Address == usdcContractAddress
                && walletContractAddress.Contains(e.Event.To));

            foreach (var item in qq)
            {
                Console.WriteLine($"收到:{item.Event.To},轉帳: {item.Event.Value / 1000000} Usd.");
            }


            Console.WriteLine("Logs transferEvent test complete");
            

            // 贖回
            logs = await client.Logs.GetLogsAsync(fromBlock: "78535606", toBlock: "78535606", topic0: redeemTopic0, page: 1, offset: 1000);
            var redeemedEvent = await GetBoundWalletEvent<WalletContractEventRedeemed>(logs);
            Console.WriteLine("Logs redeemedEvent  test complete");

            // 解綁/綁定錢包
            logs = await client.Logs.GetLogsAsync(fromBlock: "78535522", toBlock: "78535522", topic0: bindWalletTopic0, page:1,offset:1000);
            var boundWalletEvent = await GetBoundWalletEvent<WalletContractEventWalletBound>(logs);
            Console.WriteLine("Logs boundWalletEvent test complete");

            // 預簽名
            logs = await client.Logs.GetLogsAsync(fromBlock: "78533675", toBlock: "78533675", topic0: preSignedTopic0, page: 1, offset: 1000);
            var preSignedEvent = await GetBoundWalletEvent<WalletContractEventPreSigned>(logs);
            Console.WriteLine("Logs boundWalletEvent test complete");

            Console.WriteLine("GetLogsAsync: " + logs.Message);
            Console.WriteLine("All Logs test complete");
        }


        private async Task RunProxyCommandsAsync(EScanClient client)
        {
            // 最新區塊
            //var currentBlock = await client.Proxy.CurrentBlock();
            //await Console.Out.WriteLineAsync($"current block: {currentBlock.Result}");


            var stake = await client.Proxy.EthGetBlockByNumber("0x4b2da5b", true);
            Console.WriteLine(stake);

        }

        /// <summary>
        /// 轉成Event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logs"></param>
        /// <returns></returns>
        private async Task<List<EventLog<T>>> GetBoundWalletEvent<T>(EScanLogs logs) where T : IEventDTO, new()
        {
            var filterLogs = new List<FilterLog>();
            filterLogs.AddRange(logs.Logs.Select(l => new FilterLog()
            {
                BlockNumber = new HexBigInteger(l.BlockNumber),
                BlockHash = l.BlockHash,
                LogIndex = new HexBigInteger(l.LogIndex),
                Address = l.Address,
                Data = l.Data,
                Topics = l.Topics.ToArray(),
                TransactionHash = l.TransactionHash,
                TransactionIndex = new HexBigInteger(l.TransactionIndex),
            }));
            var decoded = Event<T>.DecodeAllEvents(filterLogs.ToArray());
            return decoded;
        }
    }
}