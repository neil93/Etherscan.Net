using ADRaffy.ENSNormalize;
using EthScanNet.Lib;
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
using Nethereum.RPC.Eth.DTOs;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            EScanClient client = new(EScanNetwork.PolygonAmy, "BSSW4GUFFWEHWB8V4T6S66VFDEUXZ5RAEM");

            try
            {
                //await RunAccountCommandsAsync(client);
                //await RunTokenCommandsAsync(client);
                //await RunStatsCommandsAsync(client);
                //await RunContractCommandsAsync(client);
                //await RunLogsCommandsAsync(client);
                //await RunProxyCommandsAsync(client);
                //await RunProxyTestCommandsAsync(client);
                await RunProxyFucntionCommandsAsync(client);
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
            var eoaAddress = new string[] { "0xF177B7F19aD64a9C04a45cd9E41505b1c9A5B4C6", "0xD02a7763cac2c95D013fBE8A93e406f37F83294f" };
            var walletContractAddress = new string[] { "0x70D74B6548C0E8c524b2b2B0997E3E539C93D72d", "0x96e52de6892d4B4811cEaa929E912cCd90fd6041"
                , "0x0F7B6aC80951B68301b4321a7D34f76E03AF06Fe", "0xa3F2E192415934368EfdD420bd3196fA53988C5C","0x1C338272EA3b765B5642eA6dC1A518c8e2d0e837"
                , "0x4ee9A50608D8355d50730Bd6A4211074039709e1"
            };

            // 質押交易

            //取得區塊資訊
            var blockResponse = await client.Proxy.EthGetBlockByNumber("0x4b2da5b", true);
            var blockInfo = blockResponse.GetBlockInfo();

            // 取得質押USDC合約
            var stakeTransactions = blockInfo.Transactions.Where(t => walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase) || t.To.Equals(usdcContract, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var stake in stakeTransactions)
            {
                // 取得交易收據並直接轉換為 Nethereum TransactionReceipt
                var receiptResponse = await client.Proxy.EthGetTransactionReceipt(stake.Hash);
                var transactionReceipt = receiptResponse.GetTransactionReceipt();
                var receiptInfos = receiptResponse.GetReceiptInfo();

                var logs = receiptInfos.Logs.Where(l => l.Address.Equals(usdcContract, StringComparison.OrdinalIgnoreCase));
                var transferEvent = ConvertLogsToEvent<UsdcEventTransfer>(logs);
                var resultEvents = transferEvent.Where(e => e.Log.Address.Equals(usdcContract, StringComparison.OrdinalIgnoreCase) && !eoaAddress.Contains(e.Event.From) && walletContractAddress.Contains(e.Event.To, StringComparer.OrdinalIgnoreCase)).ToList();

                foreach (var transfer in resultEvents)
                {
                    Console.WriteLine($"Stake From: {transfer.Event.From}, To: {transfer.Event.To}, Value: {transfer.Event.Value}");
                }
            }

            //贖回交易
            //取得區塊資訊
            var blockRedeemResponse = await client.Proxy.EthGetBlockByNumber("0x4ae5bb6", true);
            var blockRedeemInfo = blockRedeemResponse.GetBlockInfo();
            var redeemTransactions = blockRedeemInfo.Transactions.Where(t => walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase)).ToList();
            foreach (var redeemTransaction in redeemTransactions)
            {
                // 取得交易收據
                var receiptResponse = await client.Proxy.EthGetTransactionReceipt(redeemTransaction.Hash);
                var redeemReceipt = receiptResponse.GetTransactionReceipt();
                var receiptInfos = receiptResponse.GetReceiptInfo();

                var redeemEvents = ConvertLogsToEvent<WalletContractEventRedeemed>(receiptInfos.Logs);

                foreach (var redeem in redeemEvents)
                {
                    Console.WriteLine($"Redeem From: {redeem.Event.WalletContract}, To: {redeem.Event.Wallet}, Value: {redeem.Event.AmountInDecimal}");
                }
            }

            // 解綁
            //0x4ae5b62
            var blockBindWalletResponse = await client.Proxy.EthGetBlockByNumber("0x4ae5b62", true);
            var blockBindWalletInfo = blockBindWalletResponse.GetBlockInfo();
            var bindWalletTransactions = blockBindWalletInfo.Transactions.Where(t => walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase)).ToList();
            foreach (var bindWalletTransaction in bindWalletTransactions)
            {
                // 取得交易收據
                var receiptResponse = await client.Proxy.EthGetTransactionReceipt(bindWalletTransaction.Hash);
                var bindWalletReceipt = receiptResponse.GetTransactionReceipt();
                var receiptInfos = receiptResponse.GetReceiptInfo();

                var bindWalletEvents = ConvertLogsToEvent<WalletContractEventWalletBound>(receiptInfos.Logs);

                foreach (var bindWallet in bindWalletEvents)
                {
                    Console.WriteLine($"UnBindWallet WalletWcontract: {bindWallet.Event.WalletContract}, Wallet: {bindWallet.Event.Wallet}, ByUser: {bindWallet.Event.ByUser}");
                }
            }

            // 預簽交易
            // 0x4ae542b
            var preSignResponse = await client.Proxy.EthGetBlockByNumber("0x4ae542b", true);
            var preSignInfo = preSignResponse.GetBlockInfo();
            var preSignTransactions = preSignInfo.Transactions.Where(t => walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase)).ToList();
            foreach (var PreSignTransaction in preSignTransactions)
            {
                // 取得交易收據
                var receiptResponse = await client.Proxy.EthGetTransactionReceipt(PreSignTransaction.Hash);
                var preSignReceipt = receiptResponse.GetTransactionReceipt();
                var receiptInfos = receiptResponse.GetReceiptInfo();

                var preSignEvents = ConvertLogsToEvent<WalletContractEventPreSigned>(receiptInfos.Logs);

                foreach (var preSign in preSignEvents)
                {
                    Console.WriteLine($"PreSign From: {PreSignTransaction.From}, To: {PreSignTransaction.To} RequestId: {preSign.Event.RequestId}, Amount: {preSign.Event.Amount} ByUser: {preSign.Event.ByUser}");
                }
            }

            // EOA 手續費
            //0xD02a7763cac2c95D013fBE8A93e406f37F83294f

            var eoaAddressSet = new HashSet<string>(new[] { "0xD02a7763cac2c95D013fBE8A93e406f37F83294f" }, StringComparer.OrdinalIgnoreCase);

            var eoaResponse = await client.Proxy.EthGetBlockByNumber("0x4aec05a", true);
            var eoaInfo = eoaResponse.GetBlockInfo();
            var eoaTransactions = eoaInfo.Transactions.Where(t => eoaAddressSet.Contains(t.To, StringComparer.OrdinalIgnoreCase)
            || eoaAddressSet.Contains(t.From, StringComparer.OrdinalIgnoreCase)).ToList();
            foreach (var eoaTransaction in eoaTransactions)
            {
                // 取得交易收據
                var receiptResponse = await client.Proxy.EthGetTransactionReceipt(eoaTransaction.Hash);
                var eoaReceipt = receiptResponse.GetTransactionReceipt();
                var receiptInfos = receiptResponse.GetReceiptInfo();

                // 支出POL
                var eoaPolEvents = ConvertLogsToEvent<LogTransfer>(receiptInfos.Logs);

                foreach (var eoa in eoaPolEvents)
                {
                    Console.WriteLine($"eoa POL From: {eoaTransaction.From}, To: {eoaTransaction.To} ,Amount: {eoa.Event.Amount}");
                }

                // 支出POL
                var eoaGasEvents = ConvertLogsToEvent<LogFeeTransfer>(receiptInfos.Logs);

                foreach (var eoa in eoaPolEvents)
                {
                    Console.WriteLine($"eoa GasFee From: {eoaTransaction.From}, To: {eoaTransaction.To} ,Amount: {eoa.Event.Amount}");
                }
            }
        }

        public string GetNumber(string inputNumber, int inputValue)
        {
            if (string.IsNullOrWhiteSpace(inputNumber))
                throw new ArgumentException("十六進位不可為空");

            // 去掉 0x
            var hex = inputNumber.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? inputNumber.Substring(2)
                : inputNumber;

            // 十六進位 → 10 進位
            long number = Convert.ToInt64(hex, 16);

            // 相加
            number += inputValue;

            // 回傳十六進位（含 0x）
            return "0x" + number.ToString("X");
        }

        private async Task<string> GetCurrentBlockNumber(EScanClient client)
        {
            var currentBlock = await client.Proxy.EthBlockNumber();
            return currentBlock.GetBlockNumberHex(); //0x1bad050
        }

        private async Task RunProxyFucntionCommandsAsync(EScanClient client)
        {
            // USDC Contract - Fake USDC 合約地址
            var amoyUsdcContract = "0x5bC0720B80f66C8a0F0ba32F1f949D101C24171A";

            // EOA Address
            string[] eoaAddress = GetDbEoaAddress();

            // Walet Contract Address
            string[] walletContractAddress = GetWalletContractAddress();

            // Game Contract Address
            string[] gameContractAddress = GetGameContractAddress();

            // Oracle Contract Address
            string[] oracleContractAddress = GetOracleContractAddress();

            //var currentNumber = "0x1bcbd1d";// await GetCurrentBlockNumber(client);
            var currentNumber = await GetCurrentBlockNumber(client);
            while (true)
            {
                var currTimeStamp = DateTimeOffset.Now.Ticks;

                try
                {
                    // 質押交易

                    //取得區塊資訊
                    var blockResponse = await client.Proxy.EthGetBlockByNumber(currentNumber, true);

                    var blockInfo = blockResponse.GetBlockInfo();

                    //Console.WriteLine($"BlockNumber:{currentNumber} (Decimal: {ConvertHexToDecimal(currentNumber)}),BlackHash:{blockInfo.Hash},BlockTime:{FormatBlockTimestamp(blockInfo.Timestamp)}");
                    Console.WriteLine($"BlockNumber:{ConvertHexToDecimal(currentNumber)},BlackHash:{blockInfo.Hash},BlockTime:{FormatBlockTimestamp(blockInfo.Timestamp)}");

                    // 查DB是否有存在這些合約地址
                    var transactionGroupTo = blockInfo.Transactions.GroupBy(o => o.To).ToList();
                    var transactionGroupFrom = blockInfo.Transactions.GroupBy(o => o.From).ToList();
                    // TODO - 查詢DB
                    // 質押 - 用GroupBy To地址查詢是否存在WalletContract裡的地址
                    // 更新EOA餘額 - 用GroupBy From地查詢是否存在ChainEoaPool裡的地址


                    //取得區塊資訊
                    var transactions = blockInfo.Transactions.Where(t => t.To != null
                                                                         && (walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase)
                                                                         || eoaAddress.Contains(t.From, StringComparer.OrdinalIgnoreCase)
                                                                         || t.To.Equals(amoyUsdcContract, StringComparison.OrdinalIgnoreCase))
                                                                   )
                                                             .ToList();

                    foreach (var transaction in transactions)
                    {
                        // 取得交易收據
                        var receiptResponse = await client.Proxy.EthGetTransactionReceipt(transaction.Hash);
                        var transactionReceipt = receiptResponse.GetTransactionReceipt();

                        var receiptInfos = receiptResponse.GetReceiptInfo();

                        // 質押
                        var transferEvent = ConvertLogsToEvent<UsdcEventTransfer>(receiptInfos.Logs);
                        var resultEvents = transferEvent.Where(e => e.Log.Address.Equals(amoyUsdcContract, StringComparison.OrdinalIgnoreCase)
                                                                && !eoaAddress.Contains(e.Event.From)
                                                                && walletContractAddress.Contains(e.Event.To, StringComparer.OrdinalIgnoreCase)).ToList();

                        foreach (var transfer in resultEvents)
                        {
                            Console.WriteLine($"Stake BlockNumber:{transfer.Log.BlockNumber}, From: {transfer.Event.From}, To: {transfer.Event.To}, Value: {transfer.Event.Value}");
                        }

                        // 贖回交易
                        var redeemEvents = ConvertLogsToEvent<WalletContractEventRedeemed>(receiptInfos.Logs);
                        foreach (var redeem in redeemEvents)
                        {
                            Console.WriteLine($"Redeem From: {redeem.Event.WalletContract}, To: {redeem.Event.Wallet}, Value: {redeem.Event.AmountInDecimal}, ByUser: {redeem.Event.ByUser}");
                        }

                        // 綁定錢包
                        var bindWalletEvents = ConvertLogsToEvent<WalletContractEventWalletBound>(receiptInfos.Logs);

                        foreach (var bindWallet in bindWalletEvents)
                        {
                            Console.WriteLine($"UnBindWallet WalletWcontract: {bindWallet.Event.WalletContract}, Wallet: {bindWallet.Event.Wallet}, ByUser: {bindWallet.Event.ByUser}");
                        }

                        // 預簽名
                        var preSignEvents = ConvertLogsToEvent<WalletContractEventPreSigned>(receiptInfos.Logs);

                        foreach (var preSign in preSignEvents)
                        {
                            Console.WriteLine($"PreSign From: {transaction.From}, To: {transaction.To} RequestId: {preSign.Event.RequestId}, Amount: {preSign.Event.Amount} ByUser: {preSign.Event.ByUser}");
                        }

                        // EOA
                        var eoaGasEvents = ConvertLogsToEvent<LogFeeTransfer>(receiptInfos.Logs);
                        foreach (var gas in eoaGasEvents)
                        {
                            if (eoaAddress.Contains(gas.Event.From, StringComparer.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"Eoa From: {gas.Event.From}, To: {gas.Event.To}, Amount: {gas.Event.Amount}");
                            }
                        }

                        var eoaPolEvents = ConvertLogsToEvent<LogTransfer>(receiptInfos.Logs);
                        foreach (var pol in eoaPolEvents)
                        {
                            Console.WriteLine($"Eoa From: {pol.Event.From}, To: {pol.Event.To}, Amount: {pol.Event.Amount}");
                        }

                        // 遊戲轉帳
                        var gameEvents = ConvertLogsToEvent<UsdcEventTransfer>(receiptInfos.Logs);
                        foreach (var game in gameEvents)
                        {
                            if (gameContractAddress.Contains(game.Event.To, StringComparer.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"遊戲 收入 From: {game.Event.From}, To: {game.Event.To}, Amount: {game.Event.Value}");
                            }

                            if (gameContractAddress.Contains(game.Event.From, StringComparer.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"遊戲 支出 From: {game.Event.From}, To: {game.Event.To}, Amount: {game.Event.Value}");
                            }
                        }

                        // 預言機合約轉帳
                        var oracleEvents = ConvertLogsToEvent<UsdcEventTransfer>(receiptInfos.Logs);
                        foreach (var oracle in oracleEvents)
                        {
                            if (oracleContractAddress.Contains(oracle.Event.To, StringComparer.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"預言機 收入 From: {oracle.Event.From}, To: {oracle.Event.To}, Amount: {oracle.Event.Value}");
                            }

                            if (oracleContractAddress.Contains(oracle.Event.From, StringComparer.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"預言機 支出 From: {oracle.Event.From}, To: {oracle.Event.To}, Amount: {oracle.Event.Value}");
                            }
                        }
                    }

                    currentNumber = GetNumber(currentNumber, 1);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    var intrvalMs = 2000;
                    var elapsedMs = ToInt(TimeSpan.FromTicks(DateTimeOffset.Now.Ticks - currTimeStamp).TotalMilliseconds, intrvalMs);
                    var delayMs = intrvalMs - elapsedMs;
                    if (delayMs > 100)
                    {
                        await Task.Delay(delayMs);
                    }
                    else
                    {
                        //currentNumber = await GetCurrentBlockNumber(client);
                        Console.WriteLine($"處理時間超過2秒=>{currentNumber}");
                    }
                }
            }
        }

        private static string[] GetOracleContractAddress()
        {
            // 預言機合約地址
            //SELECT
            //    ChainOracleId,
            //    OracleContractAddress,
            //    IsValid,
            //    Balance,
            //    UpdatedTime
            //FROM ChainOracle
            //ORDER BY ChainOracleId DESC;

            return new string[] { "0x3Aa4381ec8909508D1072494C31D258a097EcB70",
"0xd74b32cc4A0525dE63742B9afa3dBfdDbe0e8E2A",
"0x3b7F34d03f6257B5a963Bfc4B3df9c83Cc885527",
"0xf9bcD0E2CC9eE0ba644bB86ab4f92b4625CB0b6b",
"0x51aF0ae34c854779ec4c96C353a5f9524559D534",
"0x3B01FF840567720B64CA1074D06171317F569996",
"0x5F6Aa01ecFdDCE523bC7C7a3d01Cb40b58E33772",
"0x5e6C6CA88958c627c594A48B731Df83C929c353e",
"0xF4B4191d78B1b525C65A9A7f362004B23aA81d99",
"0x34c4c7B0F06934820f0b369e6a6192014938e6A9",
"0xBAC7C8A25d89a9AF3b4834d7aA42A52E9B43A5fc",
"0x783F7bba55baF0718691CcE0CE1Dc6097130Fa6c",
"0x76C488f959865b7aA60acF5E5Ea1938F20a1AbA6",
"0xdC6F6FD7cFfe46000d0AFFAa1c38B65BFe385f90",
"0x78facc6C97F0cE40b083464B91a6837E358CE6Ce",
"0xBfA2e13067fb252F2104F0D1E9E7514a77ba5DA4",
"0x3C4337Cc08C8F8dae832e0EF4f9891bF4834D21E",
"0x26878F7680c682eC22133B7Ab0f613b8BBC23bF0",
"0x245a946F42aadB519825326C0e791C49Df6EA8E2",
"0x61d003E572F069305Aa062d29e4607Ed7b26868A",
"0x45a95059741552d084D43604EEC6482728FF041b",
"0xcE29D24cAF9539522F3c9FfDE71254c7cD35E51E",
"0x3639B4DE3fd49670306989A870e5C79FBfcD665A",
"0x2876DB121763CA911ff9D024CE1103042dff3d42",
"0x8dACC6Fd3D4E19e2C04BEC4dCe95312DE94DaD54",
"0x67A35Cf1601Ee68B90F94079Af01413c25D3C0f8",
"0xF54F8d2B3B17D834099f83d644A161C73c94CC9B",
"0xf55bABc7986FBBcD2A10a4b4C21AD456b87760B0",
"0x76EC479F287B40B77aD4EcF305785D0da6d99DF6",
"0x8e0D4319E8aFA1152d52D400DB81Bb06A1a83b35",
"0xf1D5FCEf8bB525053b240FbdE82AC8df0516081e",
"0xa7aD1E6166cB61b8C545446914eA8fA6972b7256",
"0x0Fab101029C8925c60F9bB55b4378a651D4c20fd",
"0x52bAFb11d6f7b0DdE0EBe0B269e0190B152A5da3",
"0x3413c3F4DCf76f29234401034B7B97669E9185ca",
"0x1599eD0bC1e7CE632F4fbf0b2210Fc93a923f5ED"
 };
        }

        private static string[] GetGameContractAddress()
        {
            // 遊戲合約地址
            // SELECT ChainContractId, ChainContractAddress, Memo, IsUseBalance, Balance, UpdatedTime
            // FROM ChainContract
            // WHERE IsUseBalance = 1;

            return new string[] { "0x376C1DAf61e5aE8C71F3BcE00144CF8105957604",
"0x7a6E0203f766Eef0C142ab7C36D4F3ec78Cbf596",
"0x71fc36A2514655f9A44aE69840A6bA84d9c2B639",
"0x36f1AD513816d661458b6d1CF4136fE0D65aAfD2"
 };
        }

        private static string[] GetWalletContractAddress()
        {
            // @addrs使用GroupBy To的資料
            //SELECT WalletContractAddress FROM WalletContract
            //WHERE WalletContractAddress IN @addrs;

            // DEV 環境測試資料
            return new string[] { "0x01372eCef1854c9F83dBbA35d98101e908D57179",
"0x03Ecfe6Dec88136D43F37F8DFE34455126f8ea51",
"0x059Fd9eE8da2b4160d612bD498985b62F72eA917",
"0x067E92782371b44B191D5E4dAB2510a03dc25BfB",
"0x08A8E1A6d1F6F169f1b34BC3cD596d1607A6eA24",
"0x095eaca20b8FcA0CF28a142e0314D259BF288D6B",
"0x0adDd31CaD1D5e40F35727485628FeB3D225479a",
"0x0BfD4091DcDC548De0E5cF686554a51F2b2066B9",
"0x0F89F11F0D23E10aC9a975CD5053e41EbCd64595",
"0x11b3405C1F59FeFD73915F9D93D01c33981d3f34",
"0x12283eb630602Ff87A51301C2C1445993389C32A",
"0x14b2C21bD6C96f3e81955D9Aab7f32FC807eFCCb",
"0x152476b2D2438C1E3C81AB7ea9ad9da3e293d6Dc",
"0x1939a27fDA9488B72dE2e2AbDF4e49621A7a1485",
"0x1A7aAa49840987F1BAF1e0FB3DE87273f55e2dE4",
"0x1e6c8393e1879488b7d61756Bd371DD2F8a3438B",
"0x26FF5D7DcB20260165A32C363bf98B3AbEf144D1",
"0x2A9d2cA9C5841C773C72e21F4f4706027a769a99",
"0x34b51c0900c009F46FBC08b54e6EcEfF8206A58D",
"0x35a6aDA08b6E69135Dd77c1D3cc7006DeE340B9C",
"0x361D54657b0eA26C7dad335A957d1a0F1b7eC9e8",
"0x384a402Ac95474136E138a9042d2F52E7366AF6d",
"0x3FC507ABdE2CCfce6C922f4d53c5f57f4e0AFde4",
"0x40d28c9C3eC262d1694F36e2B5921886B2A4eA95",
"0x421Ce7C070EEF79a5b6BDA2498695123B3b6a750",
"0x433767e84578db5b689b49C11cf69bB051c20C8E",
"0x4B85791A29c4058dc1E37cE67b7c868547BFdd9e",
"0x50EB2608a87AcF39d8CFF72F104bCEd857127597",
"0x55EBB9152FEfE6846E24b144f5DA84CC37F248C9",
"0x580a64c6C9Efe9EcfA016B8a25F1Caa17b1acF85",
"0x6CaFb3E38B9eF32125fcdcfef1851e2EB23Fe9DA",
"0x72949b2D0491e5cBa5bf4FdC2b87Bd32567cBcbe",
"0x72f49050233Bb6B79177bD68DE23Bf5C92838d2D",
"0x76614BCcF56B316868647802d75C23991f705C66",
"0x788C2ed80Dd9acA3d79ff8d5A16BAd7a07F5F109",
"0x78cEaaFbc7aC51EE0fb24Bd4c9Ef87d379fb7105",
"0x7DaBFaA88a03DF2e68C32C2315b2bD8bA93c1b85",
"0x878eC148D532e1e00Ed4f90326e70bB0DBe935D8",
"0x8A3f8e74E3B2a63f013AA06579F765783892ED5c",
"0x8F94AF4fB6EE8401673DCaC6482F440Ec99E3417",
"0x92F398696337b202fFf0B1FdF051a9CE0921C34c",
"0x99110Be91fEF7FE47693877d92135fCeE798A1f9",
"0x9a4f28da15C98cD99D9752D2A045139f1ebF4Ff9",
"0x9B1BaD10BEdB04dd5a882A359594cAD8cD6d7331",
"0x9fbD77c5aaD32140eF890f8f7600A830e90627dA",
"0xa6Df4165b68Fd877589F5b739b5d35B8a0F27b5A",
"0xA84205Ec73783a1D81B37c6D28a0178702Df2432",
"0xa8932c4dc66BC2d90DA4c51aA0212BD16f904218",
"0xAa82626bA5a13E79fC1Bb377a2030d8D99772Db0",
"0xAaa9068fCAB399DFab21A6d97338f73b26af3c31",
"0xB675C33e9F81F79e7763865864cEa27301069677",
"0xbe208C7DEd637469F43297210Ca1A89633F1e971",
"0xc1b1A8dc51c867331E8903D8E2a8d73Db1DDc887",
"0xc94fB954E903Ba0Ce25C6068A78E811C0f1b1897",
"0xCAD7ED6F147797036604Aa5aeB3e192BE981B98F",
"0xcBe2C9Ce06652755312025A4E66f6F3079a88116",
"0xCc70fB183F0Fdd945d6CBBd4Fd16Fa77c303EbAF",
"0xD35788d9a6471CFf07A57a9A9647fAd42F180eBb",
"0xD4a7050cacbcD60a46BcA78BD225426aDdf089A3",
"0xd5b09e9E0B88db2Ee81Be3fBa53AF3bdb8726D86",
"0xD7859d3f3DA608F0db570A3631ceF5daF116FB8e",
"0xD9095027083da60D65D4bF4022FBe85522405ABB",
"0xDA1E20a97D7d6732cBEC82618dDc66826D73df82",
"0xDd9baE404287c7d88456C04e273Ac84D76Cb98D8",
"0xE34e08C9ee8cdC49e74e89c651afff51d32a624f",
"0xe45AeDEE54472c7b1b182D344C2f3601C8bc3a59",
"0xe5cD56fED7A526EE44d822e638578A2AB2d05727",
"0xede05992a88eD8192b4cF15F3C570f8EC4307a34",
"0xf0dBA0bdDc6f09d9B59F709D89C09662b24363F5",
"0xf647A9866aCEeB0d1CfCFb451Da825272bb247b8",
"0xF6856b87070B6F38Bc6fAE33052A4D95fCA88C3c",
"0xF96Df18f8ee8cd4ec831CeB6F336970B3F508307"
};
        }

        private static string[] GetDbEoaAddress()
        {
            // 查詢語法
            //SELECT cep.Address
            //FROM ChainEoaBalance ceb
            //INNER JOIN ChainEoaPool cep ON ceb.ChainEoaId = cep.ChainEoaId;

            // DEV環境測試資料
            return new string[] {
"0x5d3697A3F9f9D825F2a54ec198977aAf9C7Be061",
"0x919a55d312Bb712c8B3fd75dEaf6a76b4095c542",
"0x7D88BdBA692c0351929e408999b20a046234ed3c",
"0x3f1aE60f1358d70B6E69d844e7C9958F96Ad5b73",
"0x027C39e86014B5027f1128105589bE7f26A074B2",
"0x551c584c7c29D2A68DBCD4478939c8504C33105a",
"0x31D3aa532C5b9C422d600E6A63fcB4BaB506d9eC",
"0xcE99f93Ac77F353824163e2F7690bF7A050C64bD",
"0xA520d545Caca11aE07B6e8AeB90732292b77Af63",
"0x202eB53c7D074e34C51223f198284FbF5fA6D63e",
"0x30fC2AF01033AdCF83bBd1ED0BbE2956210087d4",
"0x5c67db7E1d2fFa5547b71cD36A4CB674dbD67B35",
"0xc29c82f27A06939726183Da23D868c00EC445fC0",
"0xfC204dBE53932f95Ce6C104C45A10ca4706d81F0",
"0x704AF0f269add6f2C87cecacB7b940ECa3B491A0",
"0x0551b073588254fE7CD25f89b214cdE03d5b2BfF",
"0xDC1Ea715ab1f89Bcc5482463bD17bc5c5b1e8a3b",
"0xCC5B015795401f4ecd4C161d96F7D9eB148EAEf6",
"0x881aDAc6608c994888bbE9076832Bf0D599d079F",
"0x48fD8f35879e6cF622C71b13E69681E40115Db1B",
"0xB1b3ef54188900Da533D9dEe9363dC540F8b016A",
"0x5f53eD8AC5B7416E8724386e110d9B04b36a4259",
"0x1018e9Abe30751C189Ab8c792571d655EC9508df",
"0xdc2b35d27d4b13489bA30Ec47360E3aFfa216E80",
"0xaC69e7C9900264D15C5B86226089aceE3D328b67",
"0x13448F09b5ccBf2583DA2AA6576F6DA54B5Bb238",
"0xd68a5D67ECF43D2Eb7dC2D82f37B249aBcb5A0cd",
"0x044FF1e03A8431E613a4104416221F3799bDD594",
"0xcA089fa80804A2810FCcb5Fd215a72a682114AF1",
"0xD3F0425ACFAA289E6Baba7D5c19C752bdb6ECe8B",
"0xaD631e48856e6B82CAd13887A63a64E1B2001460",
"0x1cdaa416EE35374B104aaD15dF4A597c6551A4F5",
"0x9B4BC8180BF651fFa059A5bD6ad0e756F6C73588",
"0xEF68B914108Bf4FDE823FE219d0778bD7f45612d",
"0xD3452324cE44F2E2C716EeF0C6B8954FA402CC4E",
"0x09b89D1B30b916cE07DD6c37ba03056707442B5E",
"0xF11E3006bE7fc443b637ac1cfE4B5d6809d3aaC3",
"0x97321fE4Fd2cC0700D9220f15D3db3F79a560980",
"0x44A0E9103E1ACd05EAc7f3C4552284806cc63230",
"0x8601bB78909C9d8AEA4c26836770012dD08273E8",
"0x3c9D05E5e70A5723e85023F581c942a10EEe7717",
"0x8C35cc53129b446bE1b3857CEFEAFDA2A705407d",
"0x19d9066bD4BbE81f09e97570DBC6424d67D5670e",
"0xe82C13F9B1A40Caee85400A483c71EA229760736",
"0x7C1cB2876BA9824DA7c747647F7399Db220c4193",
"0xF19c1b4A6a391667335edfacCbD3393E375787Dd",
"0x3b9c80C47841f7988d512A601Cd4595EC97DeCdA"
};
        }

        public static int ToInt(object? o, int defaultValue)
        {
            if (o == null || o == DBNull.Value)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt32(o);
            }
            catch
            {
                return defaultValue;
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
                Console.WriteLine($"   Timestamp: {FormatBlockTimestamp(blockInfo.Timestamp)}");
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

        private static string FormatBlockTimestamp(string? timestamp)
        {
            if (string.IsNullOrWhiteSpace(timestamp))
                return "N/A";

            try
            {
                var t = timestamp.Trim();
                long value;

                if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    // 解析十六進位（不含 0x）
                    value = Convert.ToInt64(t.Substring(2), 16);
                }
                else
                {
                    if (!long.TryParse(t, out value))
                        return t; // 解析失敗，回傳原始字串
                }

                // 如果數值很大則視為毫秒，否則視為秒
                DateTimeOffset dto = value > 1_000_000_000_000L
                    ? DateTimeOffset.FromUnixTimeMilliseconds(value)
                    : DateTimeOffset.FromUnixTimeSeconds(value);

                return dto.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                // 任何失敗都回傳原始字串，避免影響主流程
                return timestamp ?? "N/A";
            }
        }

        private static long ConvertHexToDecimal(string hexNumber)
        {
            if (string.IsNullOrWhiteSpace(hexNumber))
                throw new ArgumentException("十六進位不可為空");

            // 去掉 0x 前綴（如果有）
            var hex = hexNumber.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? hexNumber.Substring(2)
                : hexNumber;

            // 十六進位 → 十進位
            return Convert.ToInt64(hex, 16);
        }
    }
}