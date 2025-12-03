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
                //await RunProxyFucntionCommandsAsync(client);

                // Logs 測試
                // Amoy質押
                //await RunLogsCommandsAsync(client, 29854007, 29854007);

                //// Amoy綁定錢包
                //await RunLogsCommandsAsync(client, 29855005, 29855005);

                //// Amoy 贖回
                //await RunLogsCommandsAsync(client, 29855133, 29855133);

                //// Amoy預簽名
                //await RunLogsCommandsAsync(client, 29856175, 29856175);

                // EOA
                //await RunLogsCommandsAsync(client, 29861713, 29861713);

                // 遊戲轉帳
                //await RunLogsCommandsAsync(client, 29722190, 29722190);

                // 預言機轉帳
                //await RunLogsCommandsAsync(client, 29852975, 29852975);
                

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
            var number = 29854007;  // 要測試特定區塊號時才輸入
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
                        await RunLogsCommandsAsync(client, startNumber, endNumber);

                        // 並發處理區塊，最高並發量為1 - 免費版最大併發量是3
                        //var semaphore = new System.Threading.SemaphoreSlim(1, 1);
                        //var tasks = new List<Task>();

                        //for (long i = startNumber; i <= endNumber; i++)
                        //{
                        //    var blockNumber = i; // 捕獲當前值

                        //    await semaphore.WaitAsync();

                        //    var task = Task.Run(async () =>
                        //    {
                        //        try
                        //        {
                        //            var n = "0x" + blockNumber.ToString("X");
                        //            var block = await GetBlockByNumber(client, n);

                        //            var info = block.GetBlockInfo();
                        //            Console.WriteLine($"=======>Number:{blockNumber},Time:{FormatBlockTimestamp(info.Timestamp)}");
                        //            await ExecuteBlock(client, usdcContract, eoaAddress, walletContractAddress, gameContractAddress, oracleContractAddress, info).ConfigureAwait(false);
                        //        }
                        //        finally
                        //        {
                        //            semaphore.Release();
                        //        }
                        //    });
                        //    tasks.Add(task);
                        //}

                        //await Task.WhenAll(tasks);

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
                                                                 || t.To.Equals(usdcContract, StringComparison.OrdinalIgnoreCase)
                                                                 //|| (t.To.Equals(usdcContract, StringComparison.OrdinalIgnoreCase) && walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase))

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
        /// 查詢日誌
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sBlock"></param>
        /// <param name="tBlock"></param>
        /// <returns></returns>
        private async Task RunLogsCommandsAsync(EScanClient client, long startBlock, long toBlock)
        {
            var bindWalletTopic0 = "0x0ca052931610b15a08f6d7b445a2be5e2d377dd2c8945678bb64fbecb2725708";
            var transferTopic0 = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";
            var redeemTopic0 = "0x378f55a9a0032096f81e501f6fba06e54947e956df2afe99d645ca71183fb269";
            var preSignedTopic0 = "0x0ed6ac937d387472d08eb1a22ae552d946a90c5db2895e40efa8f5ea66a2267a";

            var isTestAmoyChain = true;
            if (client.Network.ToString().Contains("137"))
            {
                // 正式鏈
                isTestAmoyChain = false;
            }

            // USDC Contract - USDC 合約地址
            string usdcContractAddress = ContractAddresses.GetUsdcContract(isTestAmoyChain);

            var sBlock = startBlock.ToString();
            var tBlock = toBlock.ToString();

            // 質押
            EScanLogs logs = await client.Logs.GetLogsAsync(fromBlock: sBlock, toBlock: tBlock, topic0: transferTopic0, page: 1, offset: 10000);
            var transferEvent = await GetBoundWalletEvent<UsdcEventTransfer>(logs);

            var eoaAddress = ContractAddresses.GetDbEoaAddress(isTestAmoyChain);

            var walletContractAddress = ContractAddresses.GetWalletContractAddress(isTestAmoyChain);

            var stakeEvent = transferEvent.Where(e => !eoaAddress.Contains(e.Event.From)
                && e.Log.Address.Equals(usdcContractAddress, StringComparison.OrdinalIgnoreCase)
                && walletContractAddress.Contains(e.Event.To));

            foreach (var item in stakeEvent)
            {
                Console.WriteLine($"質押:{item.Event.To},金額: {ChainUnitUtil.ToDecimal(item.Event.Value)} Usd.");
            }

            // 贖回
            logs = await client.Logs.GetLogsAsync(fromBlock: sBlock, toBlock: tBlock, topic0: redeemTopic0, page: 1, offset: 1000);
            var redeemedEvent = await GetBoundWalletEvent<WalletContractEventRedeemed>(logs);

            foreach (var redeem in redeemedEvent)
            {
                Console.WriteLine($"贖回 From: {redeem.Event.WalletContract}, To: {redeem.Event.Wallet}, Value: {redeem.Event.AmountInDecimal}, ByUser: {redeem.Event.ByUser}");
            }

            // 綁定/解綁錢包
            logs = await client.Logs.GetLogsAsync(fromBlock: sBlock, toBlock: tBlock, topic0: bindWalletTopic0, page: 1, offset: 1000);
            var boundWalletEvent = await GetBoundWalletEvent<WalletContractEventWalletBound>(logs);

            foreach (var bindWallet in boundWalletEvent)
            {
                Console.WriteLine($"綁定/解綁錢包 WalletWcontract: {bindWallet.Event.WalletContract}, Wallet: {bindWallet.Event.Wallet}, ByUser: {bindWallet.Event.ByUser}");
            }

            // 預簽名
            logs = await client.Logs.GetLogsAsync(fromBlock: sBlock, toBlock: tBlock, topic0: preSignedTopic0, page: 1, offset: 1000);
            var preSignedEvent = await GetBoundWalletEvent<WalletContractEventPreSigned>(logs);
            foreach (var preSign in preSignedEvent)
            {
                Console.WriteLine($"預簽名 Address:{preSign.Log.Address}, RequestId: {preSign.Event.RequestId}, Amount: {ChainUnitUtil.ToDecimal(preSign.Event.Amount)} ByUser: {preSign.Event.ByUser}");
            }

            // eoa
            logs = await client.Logs.GetLogsAsync(fromBlock: sBlock, toBlock: tBlock, topic0: "0x4dfe1bbbcf077ddc3e01291eea2d5c70c2b422b415d95645b9adcfd678cb1d63", page: 1, offset: 1000);
            var eoaGasEvents = await GetBoundWalletEvent<LogFeeTransfer>(logs);
            foreach (var gas in eoaGasEvents)
            {
                if (eoaAddress.Contains(gas.Event.From, StringComparer.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Eoa Gas From: {gas.Event.From}, To: {gas.Event.To}, Amount: {ChainUnitUtil.RoundTo10DecimalPlaces(gas.Event.Amount)}");
                }
            }

            logs = await client.Logs.GetLogsAsync(fromBlock: sBlock, toBlock: tBlock, topic0: "0xc4daea5836c7cbe831c2013ac003854178ffaf01893b1072220532228b51a41e", page: 1, offset: 1000);
            var eoaPolEvents = await GetBoundWalletEvent<LogTransfer>(logs);

            foreach (var pol in eoaPolEvents)
            {
                Console.WriteLine($"Eoa From: {pol.Event.From}, To: {pol.Event.To}, Amount: {ChainUnitUtil.RoundTo10DecimalPlaces(pol.Event.Amount)}");
            }

            // 遊戲轉帳
            // Game Contract Address
            string[] gameContractAddress = ContractAddresses.GetGameContractAddress(isTestAmoyChain);
            var gameEventCount = 0;
            foreach (var game in transferEvent)
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

            // 預言機合約轉帳
            string[] oracleContractAddress = ContractAddresses.GetOracleContractAddress(isTestAmoyChain);
            var oracleEventCount = 0;
            foreach (var oracle in transferEvent)
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