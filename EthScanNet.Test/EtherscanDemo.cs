using EthScanNet.Lib;
using EthScanNet.Lib.Models.ApiResponses.Logs;
using EthScanNet.Lib.Models.ApiResponses.Proxy;
using EthScanNet.Lib.Models.Events;
using EthScanNet.Lib.Utilits;
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
        private readonly string _apiKey;
        private readonly EScanNetwork _network;

        public EtherscanDemo(string apiKey, EScanNetwork network)
        {
            this._apiKey = apiKey;
            this._network = network;
        }

        public async Task RunApiCommandsAsync()
        {
            Console.WriteLine($"Running EtherscanDemo with NetWork:{_network}, APIKey:{this._apiKey}");
            EScanClient client = new(_network, _apiKey);

            try
            {
                await RunProxyFucntionCommandsAsync(client);

                //await RunLogsCommandsAsync(client, "0x1bad4fc", "0x1bad4fc");

                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// 掃描區塊交易
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task RunProxyFucntionCommandsAsync(EScanClient client)
        {
            var isTestAmoyChain = true;
            if (client.Network.ToString().Contains("137"))
            {
                // 正式鏈
                isTestAmoyChain = false;
            }

            // USDC Contract - USDC 合約地址
            string usdcContract = ContractAddresses.GetUsdcContract(isTestAmoyChain);

            // EOA Address
            string[] eoaAddress = ContractAddresses.GetDbEoaAddress(isTestAmoyChain);

            // Walet Contract Address
            string[] walletContractAddress = ContractAddresses.GetWalletContractAddress(isTestAmoyChain);

            // Game Contract Address
            string[] gameContractAddress = ContractAddresses.GetGameContractAddress(isTestAmoyChain);

            // Oracle Contract Address
            string[] oracleContractAddress = ContractAddresses.GetOracleContractAddress(isTestAmoyChain);

            string currentNumber;
            var number = 0;  // 要測試特定區塊號時才輸入
            if (number > 0)
            {
                currentNumber = "0x" + number.ToString("X");
            }
            else
            {
                currentNumber = await GetCurrentBlockNumber(client);
            }
            var isNeedGetNewBlock = false;

            // 每2秒掃描一個區塊
            while (true)
            {
                var currTimeStamp = DateTimeOffset.Now.Ticks;

                try
                {
                    if (isNeedGetNewBlock)
                    {
                        var oldCurrentNumber = currentNumber;
                        currentNumber = await GetCurrentBlockNumber(client);
                        var startNumber = ConvertHexToDecimal(oldCurrentNumber);
                        var endNumber = ConvertHexToDecimal(currentNumber);
                        Console.WriteLine($"批次處理:{ConvertHexToDecimal(oldCurrentNumber)}-{ConvertHexToDecimal(currentNumber)}");

                        // TODO 使用Logs - 待測試
                        //await RunLogsCommandsAsync(client, startNumber.ToString(), endNumber.ToString());

                        // 並發處理區塊，最高並發量為1 - 免費版最大併發量是3
                        var semaphore = new System.Threading.SemaphoreSlim(1, 1);
                        var tasks = new List<Task>();

                        for (long i = startNumber; i <= endNumber; i++)
                        {
                            var blockNumber = i; // 捕獲當前值

                            await semaphore.WaitAsync();

                            var task = Task.Run(async () =>
                            {
                                try
                                {
                                    var n = "0x" + blockNumber.ToString("X");
                                    var block = await GetBlockByNumber(client, n);

                                    var info = block.GetBlockInfo();
                                    Console.WriteLine($"=======>Number:{blockNumber},Time:{FormatBlockTimestamp(info.Timestamp)}");
                                    await ExecuteBlock(client, usdcContract, eoaAddress, walletContractAddress, gameContractAddress, oracleContractAddress, info).ConfigureAwait(false);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            });
                            tasks.Add(task);
                        }

                        await Task.WhenAll(tasks);

                        isNeedGetNewBlock = false;
                        currentNumber = GetNumber(currentNumber, 1);
                    }

                    //取得區塊資訊
                    EScanEthBlock blockResponse = await GetBlockByNumber(client, currentNumber);

                    var blockInfo = blockResponse.GetBlockInfo();

                    if (blockInfo == null)
                    {
                        continue;
                    }

                    Console.WriteLine($"BlockNumber:{ConvertHexToDecimal(currentNumber)},BlockTime:{FormatBlockTimestamp(blockInfo.Timestamp)}");

                    // 查DB是否有存在這些合約地址
                    var transactionGroupTo = blockInfo.Transactions.GroupBy(o => o.To).ToList();
                    var transactionGroupFrom = blockInfo.Transactions.GroupBy(o => o.From).ToList();
                    // TODO - 查詢DB
                    // 質押 - 用GroupBy To地址查詢是否存在WalletContract裡的地址
                    // 更新EOA餘額 - 用GroupBy From地查詢是否存在ChainEoaPool裡的地址

                    await ExecuteBlock(client, usdcContract, eoaAddress, walletContractAddress, gameContractAddress, oracleContractAddress, blockInfo);

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
                        Console.WriteLine($"處理時間超過2秒=>{ConvertHexToDecimal(currentNumber)}");
                        isNeedGetNewBlock = true;
                    }
                }
            }
        }

        private static async Task<EScanEthBlock> GetBlockByNumber(EScanClient client, string currentNumber)
        {
            var blockStopwatch = System.Diagnostics.Stopwatch.StartNew();

            var blockResponse = await client.Proxy.EthGetBlockByNumber(currentNumber, true);

            blockStopwatch.Stop();

            Console.WriteLine($"[EthGetBlockByNumber] blockStopwatch 耗時: {blockStopwatch.ElapsedMilliseconds}ms");
            return blockResponse;
        }

        private async Task ExecuteBlock(EScanClient client, string usdcContract, string[] eoaAddress, string[] walletContractAddress, string[] gameContractAddress, string[] oracleContractAddress, BlockInfo blockInfo)
        {
            var blockStopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int performanceThresholdMs = 1000; // 效能門檻：1秒

            //取得區塊資訊
            var filterStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var transactions = blockInfo.Transactions.Where(t => t.To != null
                                                                 && (walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase)
                                                                 || eoaAddress.Contains(t.From, StringComparer.OrdinalIgnoreCase)
                                                                 //|| t.To.Equals(usdcContract, StringComparison.OrdinalIgnoreCase)
                                                                 ||(t.To.Equals(usdcContract, StringComparison.OrdinalIgnoreCase) && walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase))

                                                                 )
                                                           )
                                                     .ToList();
            filterStopwatch.Stop();
            if (filterStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                Console.WriteLine($"[效能警告] 篩選交易耗時: {filterStopwatch.ElapsedMilliseconds}ms");

            foreach (var transaction in transactions)
            {
                var transactionStopwatch = System.Diagnostics.Stopwatch.StartNew();

                // 取得交易收據
                var receiptStopwatch = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine($"取得交易收據:{transaction.Hash}");
                var receiptResponse = await client.Proxy.EthGetTransactionReceipt(transaction.Hash);
                var transactionReceipt = receiptResponse.GetTransactionReceipt();
                var receiptInfos = receiptResponse.GetReceiptInfo();
                receiptStopwatch.Stop();
                if (receiptStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 取得交易收據耗時: {receiptStopwatch.ElapsedMilliseconds}ms (TxHash: {transaction.Hash})");

                // 質押
                var stakeStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var transferEvent = ConvertLogsToEvent<UsdcEventTransfer>(receiptInfos.Logs);
                var resultEvents = transferEvent.Where(e => e.Log.Address.Equals(usdcContract, StringComparison.OrdinalIgnoreCase)
                                                        && !eoaAddress.Contains(e.Event.From)
                                                        && walletContractAddress.Contains(e.Event.To, StringComparer.OrdinalIgnoreCase)).ToList();

                foreach (var transfer in resultEvents)
                {
                    Console.WriteLine($"Stake BlockNumber:{transfer.Log.BlockNumber}, From: {transfer.Event.From}, To: {transfer.Event.To}, Value: {ChainUnitUtil.ToDecimal(transfer.Event.Value)}");
                }
                stakeStopwatch.Stop();
                if (resultEvents.Any() && stakeStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理質押事件耗時: {stakeStopwatch.ElapsedMilliseconds}ms");

                // 贖回交易
                var redeemStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var redeemEvents = ConvertLogsToEvent<WalletContractEventRedeemed>(receiptInfos.Logs);
                foreach (var redeem in redeemEvents)
                {
                    Console.WriteLine($"Redeem From: {redeem.Event.WalletContract}, To: {redeem.Event.Wallet}, Value: {redeem.Event.AmountInDecimal}, ByUser: {redeem.Event.ByUser}");
                }
                redeemStopwatch.Stop();
                if (redeemEvents.Any() && redeemStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理贖回事件耗時: {redeemStopwatch.ElapsedMilliseconds}ms");

                // 綁定錢包
                var bindStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var bindWalletEvents = ConvertLogsToEvent<WalletContractEventWalletBound>(receiptInfos.Logs);

                foreach (var bindWallet in bindWalletEvents)
                {
                    Console.WriteLine($"UnBindWallet WalletWcontract: {bindWallet.Event.WalletContract}, Wallet: {bindWallet.Event.Wallet}, ByUser: {bindWallet.Event.ByUser}");
                }
                bindStopwatch.Stop();
                if (bindWalletEvents.Any() && bindStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理綁定錢包事件耗時: {bindStopwatch.ElapsedMilliseconds}ms");

                // 預簽名
                var preSignStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var preSignEvents = ConvertLogsToEvent<WalletContractEventPreSigned>(receiptInfos.Logs);

                foreach (var preSign in preSignEvents)
                {
                    Console.WriteLine($"PreSign From: {transaction.From}, To: {transaction.To} RequestId: {preSign.Event.RequestId}, Amount: {ChainUnitUtil.ToDecimal(preSign.Event.Amount)} ByUser: {preSign.Event.ByUser}");
                }
                preSignStopwatch.Stop();
                if (preSignEvents.Any() && preSignStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理預簽名事件耗時: {preSignStopwatch.ElapsedMilliseconds}ms");

                // EOA
                var eoaStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var eoaGasEvents = ConvertLogsToEvent<LogFeeTransfer>(receiptInfos.Logs);
                foreach (var gas in eoaGasEvents)
                {
                    if (eoaAddress.Contains(gas.Event.From, StringComparer.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Eoa From: {gas.Event.From}, To: {gas.Event.To}, Amount: {ChainUnitUtil.RoundTo10DecimalPlaces(gas.Event.Amount)}");
                    }
                }

                var eoaPolEvents = ConvertLogsToEvent<LogTransfer>(receiptInfos.Logs);
                foreach (var pol in eoaPolEvents)
                {
                    Console.WriteLine($"Eoa From: {pol.Event.From}, To: {pol.Event.To}, Amount: {ChainUnitUtil.RoundTo10DecimalPlaces(pol.Event.Amount)}");
                }
                eoaStopwatch.Stop();
                if ((eoaGasEvents.Any() || eoaPolEvents.Any()) && eoaStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理EOA事件耗時: {eoaStopwatch.ElapsedMilliseconds}ms");

                // 遊戲轉帳
                var gameStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var gameEvents = ConvertLogsToEvent<UsdcEventTransfer>(receiptInfos.Logs);
                var gameEventCount = 0;
                foreach (var game in gameEvents)
                {
                    if (gameContractAddress.Contains(game.Event.To, StringComparer.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"遊戲 收入 From: {game.Event.From}, To: {game.Event.To}, Amount: {ChainUnitUtil.ToDecimal(game.Event.Value)}");
                        gameEventCount++;
                    }

                    if (gameContractAddress.Contains(game.Event.From, StringComparer.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"遊戲 支出 From: {game.Event.From}, To: {game.Event.To}, Amount: {ChainUnitUtil.ToDecimal(game.Event.Value)}");
                        gameEventCount++;
                    }
                }
                gameStopwatch.Stop();
                if (gameEventCount > 0 && gameStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理遊戲轉帳事件耗時: {gameStopwatch.ElapsedMilliseconds}ms");

                // 預言機合約轉帳
                var oracleStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var oracleEvents = ConvertLogsToEvent<UsdcEventTransfer>(receiptInfos.Logs);
                var oracleEventCount = 0;
                foreach (var oracle in oracleEvents)
                {
                    if (oracleContractAddress.Contains(oracle.Event.To, StringComparer.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"預言機 收入 From: {oracle.Event.From}, To: {oracle.Event.To}, Amount: {ChainUnitUtil.ToDecimal(oracle.Event.Value)}");
                        oracleEventCount++;
                    }

                    if (oracleContractAddress.Contains(oracle.Event.From, StringComparer.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"預言機 支出 From: {oracle.Event.From}, To: {oracle.Event.To}, Amount: {ChainUnitUtil.ToDecimal(oracle.Event.Value)}");
                        oracleEventCount++;
                    }
                }
                oracleStopwatch.Stop();
                if (oracleEventCount > 0 && oracleStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理預言機轉帳事件耗時: {oracleStopwatch.ElapsedMilliseconds}ms");

                transactionStopwatch.Stop();
                if (transactionStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理單筆交易總耗時: {transactionStopwatch.ElapsedMilliseconds}ms");
            }

            blockStopwatch.Stop();
            if (blockStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                Console.WriteLine($"[效能警告] ExecuteBlock 總耗時: {blockStopwatch.ElapsedMilliseconds}ms (區塊: {blockInfo.Number}, 交易數: {transactions.Count})");
        }

        /// <summary>
        /// 日誌
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        //private async Task RunLogsCommandsAsync(EScanClient client)
        //{
        //    // 正式鏈
        //    var bindWalletTopic0 = "0x0ca052931610b15a08f6d7b445a2be5e2d377dd2c8945678bb64fbecb2725708";
        //    var transferTopic0 = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";
        //    var redeemTopic0 = "0x378f55a9a0032096f81e501f6fba06e54947e956df2afe99d645ca71183fb269";
        //    var preSignedTopic0 = "0x0ed6ac937d387472d08eb1a22ae552d946a90c5db2895e40efa8f5ea66a2267a";

        //    var usdcContractAddress = "0x3c499c542cef5e3811e1192ce70d8cc03d5c3359";     // USDC 合約地址

        //    Console.WriteLine("Logs test started");
        //    //EScanLogs logs = await client.Logs.GetLogsAsync(fromBlock: "0x1", toBlock: "latest", topic0: "0xbb8f597c6a23e718c7579b21e311c3daf7851a8456dbb20e97b3124cd3a66022", page:1,offset:100);

        //    // 質押
        //    EScanLogs logs = await client.Logs.GetLogsAsync(fromBlock: "78830144", toBlock: "78830144", topic0: transferTopic0, page: 1, offset: 10000);
        //    var transferEvent = await GetBoundWalletEvent<UsdcEventTransfer>(logs);

        //    var eoaAddress = new string[] { "0xF177B7F19aD64a9C04a45cd9E41505b1c9A5B4C6", "0xD02a7763cac2c95D013fBE8A93e406f37F83294f" };   // EOA

        //    var walletContractAddress = new string[] { "0x70D74B6548C0E8c524b2b2B0997E3E539C93D72d", "0x96e52de6892d4B4811cEaa929E912cCd90fd6041", "0x0F7B6aC80951B68301b4321a7D34f76E03AF06Fe" };  // �ϥΪ̿��]�X��

        //    var qq = transferEvent.Where(e => !eoaAddress.Contains(e.Event.From)
        //        && e.Log.Address == usdcContractAddress
        //        && walletContractAddress.Contains(e.Event.To));

        //    foreach (var item in qq)
        //    {
        //        Console.WriteLine($"質押:{item.Event.To},金額: {ChainUnitUtil.RoundTo10DecimalPlaces(item.Event.Value)} Usd.");
        //    }

        //    Console.WriteLine("Logs transferEvent test complete");

        //    // 贖回
        //    logs = await client.Logs.GetLogsAsync(fromBlock: "78535606", toBlock: "78535606", topic0: redeemTopic0, page: 1, offset: 1000);
        //    var redeemedEvent = await GetBoundWalletEvent<WalletContractEventRedeemed>(logs);
        //    Console.WriteLine("Logs redeemedEvent  test complete");

        //    // 綁定/解綁錢包
        //    logs = await client.Logs.GetLogsAsync(fromBlock: "78535522", toBlock: "78535522", topic0: bindWalletTopic0, page: 1, offset: 1000);
        //    var boundWalletEvent = await GetBoundWalletEvent<WalletContractEventWalletBound>(logs);
        //    Console.WriteLine("Logs boundWalletEvent test complete");

        //    // 預簽名
        //    logs = await client.Logs.GetLogsAsync(fromBlock: "78533675", toBlock: "78533675", topic0: preSignedTopic0, page: 1, offset: 1000);
        //    var preSignedEvent = await GetBoundWalletEvent<WalletContractEventPreSigned>(logs);
        //    Console.WriteLine("Logs boundWalletEvent test complete");

        //    Console.WriteLine("GetLogsAsync: " + logs.Message);
        //    Console.WriteLine("All Logs test complete");
        //}

        private async Task RunLogsCommandsAsync(EScanClient client, string startBlock, string toBlock)
        {
            // Amoy
            var bindWalletTopic0 = "0x0ca052931610b15a08f6d7b445a2be5e2d377dd2c8945678bb64fbecb2725708";
            var transferTopic0 = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";
            var redeemTopic0 = "0x378f55a9a0032096f81e501f6fba06e54947e956df2afe99d645ca71183fb269";
            var preSignedTopic0 = "0xbb8f597c6a23e718c7579b21e311c3daf7851a8456dbb20e97b3124cd3a66022";

            var usdcContractAddress = "0x5bC0720B80f66C8a0F0ba32F1f949D101C24171A";     // USDC 合約地址

            // 質押
            EScanLogs logs = await client.Logs.GetLogsAsync(fromBlock: startBlock, toBlock: toBlock, topic0: transferTopic0, page: 1, offset: 10000);
            var transferEvent = await GetBoundWalletEvent<UsdcEventTransfer>(logs);

            var eoaAddress = ContractAddresses.GetDbEoaAddress(true);

            var walletContractAddress = ContractAddresses.GetWalletContractAddress(true);

            var qq = transferEvent.Where(e => !eoaAddress.Contains(e.Event.From)
                && e.Log.Address == usdcContractAddress
                && walletContractAddress.Contains(e.Event.To));

            foreach (var item in qq)
            {
                Console.WriteLine($"質押:{item.Event.To},金額: {ChainUnitUtil.RoundTo10DecimalPlaces(item.Event.Value)} Usd.");
            }

            //Console.WriteLine("Logs transferEvent test complete");

            // 贖回
            logs = await client.Logs.GetLogsAsync(fromBlock: startBlock, toBlock: toBlock, topic0: redeemTopic0, page: 1, offset: 1000);
            var redeemedEvent = await GetBoundWalletEvent<WalletContractEventRedeemed>(logs);
            //Console.WriteLine("Logs redeemedEvent  test complete");

            // 綁定/解綁錢包
            logs = await client.Logs.GetLogsAsync(fromBlock: startBlock, toBlock: toBlock, topic0: bindWalletTopic0, page: 1, offset: 1000);
            var boundWalletEvent = await GetBoundWalletEvent<WalletContractEventWalletBound>(logs);
            //Console.WriteLine("Logs boundWalletEvent test complete");

            // 預簽名
            logs = await client.Logs.GetLogsAsync(fromBlock: startBlock, toBlock: toBlock, topic0: preSignedTopic0, page: 1, offset: 1000);
            var preSignedEvent = await GetBoundWalletEvent<WalletContractEventPreSigned>(logs);
            //Console.WriteLine("Logs boundWalletEvent test complete");

            //Console.WriteLine("GetLogsAsync: " + logs.Message);
            //Console.WriteLine("All Logs test complete");
        }

        private string GetNumber(string inputNumber, int inputValue)
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

        /// <summary>
        /// 預言機合約地址
        /// </summary>
        /// <param name="isAmoyTestNet"></param>
        /// <returns></returns>
        private static string[] GetOracleContractAddress(bool isAmoyTestNet)
        {
            // 預言機合約地址
            //SELECT
            //    ChainOracleId,
            //    OracleContractAddress,
            //    IsValid,
            //    Balance,
            //    UpdatedTime
            //FROM BcGamePredict.ChainOracle
            //ORDER BY ChainOracleId DESC;

            if (isAmoyTestNet)
            {
                // DEV
                return new string[] {
"0x7e47aBff49a61A68a0ce3E6CaF9B2Dd045bA3512",
"0x3Aa4381ec8909508D1072494C31D258a097EcB70",
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

            // 生產
            return new string[] {
"0xD8846C0e08fA96db2E6F6C2C747055065378C282"
            };
        }

        /// <summary>
        /// 遊戲合約地址
        /// </summary>
        /// <param name="isAmoyTestNet"></param>
        /// <returns></returns>
        private static string[] GetGameContractAddress(bool isAmoyTestNet)
        {
            // 遊戲合約地址
            // SELECT ChainContractId, ChainContractAddress, Memo, IsUseBalance, Balance, UpdatedTime
            // FROM BcChain.ChainContract
            // WHERE IsUseBalance = 1;

            if (isAmoyTestNet)
            {
                // dev
                return new string[] {
"0x574795e696654Bb69e93DC180521699Ddf85e325",
"0x4981a86fb363e1BBEa124863F959E312801d588a",
"0x336Fc2704Ee63B704BDF80f128a509A773b6d936",
"0x9820EdEd7d978C4A037bD4139f5C9faD8C4Fd992"
 };
            }

            // 生產
            return new string[] {
"0xaD13cCbACB8bd5162F110968b50AD53d06A5ED4d",
"0xf681b197Be290C3244C378F68F35F1Ff0605DdcE",
"0x07196422E0b303384E652037629E79BBE3da9886",
"0x8C27cE280B11E116783bb00C1463bD09dA20d671",
"0x3d3EB175f0a9952156d09FA862c43F925a5670b2",
"0xc0A3ec04cA20941a78B6c16F6FFdD75823F96B1E"

            };
        }

        /// <summary>
        /// 系統錢包合約地址
        /// </summary>
        /// <param name="isAmoyTestNet"></param>
        /// <returns></returns>
        private static string[] GetWalletContractAddress(bool isAmoyTestNet)
        {
            // @addrs使用GroupBy To的資料
            //SELECT WalletContractAddress FROM BcFramework.WalletContract
            //WHERE WalletContractAddress IN @addrs;

            if (isAmoyTestNet)
            {
                // DEV 環境測試資料
                return new string[] {
"0x0937EfF0810EE7908d143350ab59215a18989F82",
"0x098B6b71B1b54320A4788c89Ab59ce42264a3c54",
"0x10091d89d4A9991b9BEfd35A2C5Ae78f812486e9",
"0x144d3f454eC1e12F407183F4297E651B7d3225e4",
"0x15cf14aDF9b0561C712bAaa9abEd6280AD3BfCF7",
"0x21aD5B06774BCae1d34Cde7486100d7cA5B1cDDc",
"0x2b451752c5bb1CEfD47b9E79A027430e07a85436",
"0x2cb0E45caeC81DD8EA30956C1711d8D40F642CE8",
"0x33607397370C3cEDB3226aac08759b34E9550922",
"0x35E20BDBFe45089CA0cDc99845d7cB1349C16512",
"0x3b4a9C993b5Ae6d36FDDd768A91d3024101fA26E",
"0x3ecb9FB95cBb30Cda2dB9E11C337489D14F2832c",
"0x4266A1A39b8e0D8bC5Cd2648C8CC97202719bCf1",
"0x4b5A71cEA38360f92397aC496D1E4AA6984dF2A2",
"0x4Dd7a02C92A080907380A3099D1af112112110C3",
"0x4E9FeCA4386faFf935B0149D8990426746890Baf",
"0x4eaf3C17513CD56B28Eb5B4deD74f2C6B2Bc9c91",
"0x5077A3B39Fd212E181d1032175c5A75BF58a5096",
"0x514404f9F338E7532d92E9CCe0305bD30d984d79",
"0x5a69dAD59E45066D1eAf6854d394F230754524E4",
"0x5e86EC9BfA65B51b06fC5C5F02231e1B4836B27a",
"0x647f8fACa515b3b762ed3F3A1d5ec37e6d310d83",
"0x648B712a71950d26Dd89823Cd0C2BF84721A39A6",
"0x682361e8E9588630833e76899B10B310FF11198e",
"0x6BB281839C70C260fE371e84F65711d8Ec4d8edf",
"0x714f690893856974952AcCe020660F10F102aEfE",
"0x738A3a097f34fF576588E4863a86D979C1c00AD7",
"0x7b1C12B3df7f759A14d42ED8d255bb49d1d818e2",
"0x7bb6cC0072dA255958FBc2deda14dB1109CFA0E5",
"0x7d7aDe2272b70c6088F7cfDc6c59D546dBd0e628",
"0x8b9d3BE364b1BD2392AFa2dFd71D544AaA05Ba91",
"0x8bca2d18b4C4c8C6Fe4446Fe5Ee3c4FAad91D551",
"0x8F1b64C4C0d01EDb4B0Bd4834f05504839F7ADe7",
"0x91BA0b43018f409DD358F38fe4b0A655f763a269",
"0x963F24d5A4362b97d388fA207B3b72f619d0926B",
"0x969E12C5a199f71610421Ba5e5E97ccb91981C01",
"0x9840AD33E6266859538aaf6cFE3B6B2991b8A97e",
"0x9dB608148fc3368ebF3b2F816e3E4A39b09286A4",
"0x9e5ce3430302F59739F38Ca308A9A95275548Cc5",
"0xA0bE1B546f2AaAa295B93991c93a49068a165E52",
"0xA52981a34AFA1C881e93Ff126800af4c3ebC943e",
"0xa8c4627cf738557d5dAbad3D71eb1C39a0cff77a",
"0xa8D6A9a5ABa5F97a299df4D69BEbadFd8E88dD52",
"0xAA16AE5Ae596a2906f735B723eF161054a458A3A",
"0xAA8d4235C57424107CD34d5c73FfE6F932657AEB",
"0xb4C5ec18ebC25980C37816BcAF9F66193b9b079a",
"0xBa802D9BF5106C674c1bAEe8b8542B0A25c38a62",
"0xc13E33a77b69D1cFd24B53650FD350efB6D34de1",
"0xC4Ae493ac3600b30366ae1f36Aac77c7D1A7e050",
"0xD09d3326C7fA83Ac2c87381C6AFcf17AE21EbEc0",
"0xd1412448D4d8F1FC8867C607026b59a1D0bDe416",
"0xd5E34aeA2673f214f66026aD15d023A7daF5a45F",
"0xDEc674E5b4392b3b2d498BdC25883a5cc64DCb99",
"0xDFC6b0938e21411117Dff40a8A3BdD94a33D9ED7",
"0xe393Cd0c0B6d0bd4a2259878FaA1a0282DcD485B",
"0xe42a28C58a3674Ac99D2dfDCfF61703b97434E4e",
"0xe9dDC248972cc8aDe2985FdAE4a1a6f751E5C729",
"0xeA902100dAb619bDCC55C92E7561bB049831A24b",
"0xECb3b3d44b656397d887361c33Ed9E517Aac111b",
"0xeCF9A6Eade0FaeCb902D22f4fEa06a64866Aa2b3",
"0xf2242c1e304d774A1E35D03244658c9e17c39210",
"0xF5413E4975A3ceCF077E91776d4fF2BF1c78eaB6",
"0xf5Ce60B326188F43d9847F18d8F1E44f07DBd54F",
"0xf7ea5Ee811aA3846BE7a5792c28B24decF006f6D"
};
            }

            // 生產
            return new string[] {
"0x00a8857B318FD982D851e9479Bf14BAD6a97dAc5",
"0x032453F0bc6b47737A0Ff365770Adf1187a02427",
"0x0520C7206A92b0e2Ec9dAc3a218629426928fa4f",
"0x0a6F2371f5aAafBEaE90b07E7B82C9c39FBDf03c",
"0x11e8c96d5220941de18408bFA6Ff4b2660f78D64",
"0x1c6e67C38a5d596876A75b2bA163EB23991b1185",
"0x1E887672FCdDBc15Cb9E333Dd0eD5270ea7DBF96",
"0x2075968DC74cE26085e314eaD60A749915Bf6709",
"0x20F52fA27e271d8BB3da803816D19b89fd915B2c",
"0x210440b63B213d0Ef4610baB7d13AE9288C59Ba8",
"0x2329f2A0e089a39A7F90c9c2dF256a77B9C705D0",
"0x2553451F102ac62C4746CC0f6ACa352893dFDB5a",
"0x2aD4ca615891ceE3537A48A90b8d75F15921818A",
"0x2B0DdD462b1f5e6c3247ce84cF6926Bbc401AabE",
"0x2BA7cC08Ed4AB3F273B8290F5D1A62B4CC001224",
"0x2FF28c882793759Ff0007f920e2707475c7Fb997",
"0x311494E4A63C8824ac1156ffb19d4e0b44E43E7d",
"0x32De07A0C50e370523E250686E16f43802AFF6F4",
"0x33e9cd9760d4109f3c5A0dAEF16Ec4540EEb327e",
"0x3A5963A079EBc3d03Db173BD2C0E7CBcd8f07B46",
"0x45B09b0911a218770332192fc2b17fBb4344AB9d",
"0x4918204d860c7DeFd9623dEc9663fdc11a774b68",
"0x4Ad9400223F81f8EfE44F76784531f13AEEc8677",
"0x4b2Bce197514e29d5261BeE90E3Aa51Cbfc85CD3",
"0x51f1E63055fb0241BA3cec2BC120D33343056020",
"0x5625246c50066205c32D9fBb9ce75bbc8C291f88",
"0x5d9704561eA0D7DE25Eff4651DadF9F352619413",
"0x6607a9dc64103AaD306cf02005Cee32372789157",
"0x661EaE22CBA9eCB3e5034a70bF40ef7147B094cf",
"0x66E3AdA72dBE3be61f07087834202E18089d857f",
"0x6aD719c49ECbf0ab5cBe8d7Df0C780961A61c52D",
"0x6ADbcF356520a5C23A7E6bA1a6e00092ae6D5e2a",
"0x73eE120Cc56a170bfF6D3C901D9fe92D1c4178c3",
"0x75b938a9Edf420f5a45f12e480DaDEbFd18d2C94",
"0x76f842c19Dbc9d85D99A985E86fe076fAfce7873",
"0x7837E098DF1d0828C917F1b9c7053924c4B4A838",
"0x79B1A08439c0Bb40B531644fA3C0E6c913033802",
"0x7bA1A794B9f2B789cC4e0367C4FAe73d67f22576",
"0x7d20b3F4483DE8BA5C4A7A07df938997962e6198",
"0x88FF80CDf97a07205F830eAF11C86bff81d34E2d",
"0x8dc6eF08285C7ea08cbd9fb9e825C648D739ff94",
"0x91Bb8D7F722D609f1950B6A8f5FBA5e450C38e88",
"0x9d4b3F99ac6771c27358830159a49247CC99F643",
"0xa13bE7781dB4304C1BAEDA797708Da50239aaD22",
"0xA4E18855Db3b80869D438B9924da8a5219CB32a3",
"0xa7AFCa84B3028d3C35279baBe4Cf4Bc393CB87c7",
"0xA933e36A4df66051C79C448d91590a92A2151A77",
"0xaE3CF1629Dcd78553C80E7E0de52cE1B8c87f67A",
"0xB3Ee21B1E42C3317363adf6DBbaa0f7F47BB4F63",
"0xb75Cba438685130ba19443B8Fc5E462b947291B2",
"0xc1545700C3D7ba17396d8f2D5bA6941492a42297",
"0xc725a2D88f3C3a7D9FE80fe491DBdD2Fc7008100",
"0xcbC288d47044C324Eec3120b0C16bd61b9bBA083",
"0xD377263bF3Bf40Ca29361FF64D8100Ae1Ea7117B",
"0xd5B2706bc7A705Ce3d03E273CA5b0437bE6a1C02",
"0xdB70eF1eF81d2b096EB787803fb7b53AcbD9D726",
"0xDED91228e085a80B5e0c1554bFeD84DA85c4DD7c",
"0xdf2A56221B9aDD6502B11eAa78cd740553684Dc2",
"0xe21088f69CDDF05799d9F138940287ecF893f6AD",
"0xe531669ed912B5AaC74a3536DAc374797C312033",
"0xE5D228845B79aB77544abc278F84465BE02e0ebb",
"0xE7406058df7a99448B403be3Ce8cE92474a7000D",
"0xEc813c1F846ec66be75bda16bED5dfC5b9b44aA8",
"0xEF2b3E8a7232396B9D4570BE68131391026c7e30",
"0xF14364460716dC07767d1D98EceA95479665cbe7",
"0xf3791839D948b01de88A38735a94E97dA80B3E28",
"0xF74d0907Ab56bD1DC24cffD21567AA4cD8EeEFFf",
"0xf9C9ab0B9f926a1501b230545a01325F46934ebd"

            };
        }

        /// <summary>
        /// 取得Eoa合約地址清單
        /// </summary>
        /// <param name="isAmoyTestNet"></param>
        /// <returns></returns>
        private static string[] GetDbEoaAddress(bool isAmoyTestNet)
        {
            // 查詢語法
            //SELECT cep.Address
            //FROM BcChain.ChainEoaBalance ceb
            //INNER JOIN ChainEoaPool cep ON ceb.ChainEoaId = cep.ChainEoaId;

            if (isAmoyTestNet)
            {
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
"0x3b9c80C47841f7988d512A601Cd4595EC97DeCdA",
"0x6Df77e4D5D25177795b0B60Ce85Bd4241B144D8E"
};
            }

            // 生產環境資料
            return new string[] {
"0xD02a7763cac2c95D013fBE8A93e406f37F83294f",
"0xF177B7F19aD64a9C04a45cd9E41505b1c9A5B4C6",
"0x78C887cEa45cC0348FfeD7d8e608450e9208A3C9",
"0x6BbeAbA00f355f47273371da616cCbAf48A4D4df",
"0x42274356b960839ea80B579288a95761De3ed286",
"0x4d4dE502c5BCEeB3485e74742167BB256652Fb5f",
"0xA52A6A0B068eC13e84d9799F501a2Beb2B1f151a",
"0xD84e75E8b8d778B9A479b983eD96CfB53AAAD11F",
"0xEc879b1F782792fd810187f70ae1EA281cF94B36",
"0x7da24D11a7435AC8FAb23c21aC0c54DD4d8D9DA0",
"0x9D36d09bEbdA35f8296A7ad8203AadB41bC6d05F",
"0x50d724BC85A3Ac0C53a5C193301ff76dDDF7cF65",
"0xcCDc837E87593F45a6f22b32f12044c1b77B618F",
"0xD65FA62A13A62d4c030B988098a82B2091E3044d",
"0xCbceEce03F3B9F0bCB4a6e16EA6cA9ee8B4dA21D",
"0x6cDEA987Cb6b44784Fc72F771286f47c243FA4c5",
"0x83EA875B024CE611f97e57f92a100e13ee3A5837",
"0x78373d52E327F0463481cE252bDf78008781557C",
"0x0219f11B57A3529e13B86f6F117e8099aFb91F79",
"0x238cA150A5923134E5bA235C6986d9CF6689C71D",
"0x3A5202c23e3E23C6DDA0900F700AAA9508854462",
"0xB5c6410541306CaC3551C170A33E344215484DE4",
"0x92376364cebf755c718C536F065be2f6Bf2Ed1dB",
"0x280426b6cD6C6cB90bcDa3ac8867362a370b4555",
"0x3B203e70472BA9e0a428d04773FCC6fF2452f202",
"0x360B58C907081B26D45317A2fEed56840a5887f0",
"0x2a94bf8BEA54a3D15F5Df6dd01D8aF9f774Ff5aD",
"0xd5f4150ABF60DfAAA0D6Be277310d4c989AeAA01",
"0x30D1Ffa04003E075f4C8d06f1a8E4c4aC56514E0",
"0x66627f60561964B2148ef9eEf3C3F7Dc074038a1",
"0x437eA7D2339825d142e8EEAD2d326E9C1Ef17658"

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