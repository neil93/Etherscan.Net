using EthScanNet.Lib.Models.Events;
using EthScanNet.Lib.Utilits;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace EthScanNet.Test
{
    public class NethereumDemo
    {
        private readonly string _rpcUrl;
        private readonly bool _isTestNet;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="rpcUrl">RPC節點URL (例如: https://polygon-amoy.infura.io/v3/YOUR_KEY 或 https://polygon-mainnet.infura.io/v3/YOUR_KEY)</param>
        /// <param name="isTestNet">是否為測試網 (Amoy)</param>
        public NethereumDemo(string rpcUrl, bool isTestNet = true)
        {
            _rpcUrl = rpcUrl;
            _isTestNet = isTestNet;
        }

        public async Task RunApiCommandsAsync()
        {
            Console.WriteLine($"Running NethereumDemo with RPC:{_rpcUrl}, IsTestNet:{_isTestNet}");
            var web3 = new Web3(_rpcUrl);

            try
            {
                await RunBlockScanningAsync(web3);

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
        /// <param name="web3"></param>
        /// <returns></returns>
        private async Task RunBlockScanningAsync(Web3 web3)
        {
            // USDC Contract - USDC 合約地址
            string usdcContract = ContractAddresses.GetUsdcContract(_isTestNet);

            // EOA Address
            string[] eoaAddress = ContractAddresses.GetDbEoaAddress(_isTestNet);

            // Wallet Contract Address
            string[] walletContractAddress = ContractAddresses.GetWalletContractAddress(_isTestNet);

            // Game Contract Address
            string[] gameContractAddress = ContractAddresses.GetGameContractAddress(_isTestNet);

            // Oracle Contract Address
            string[] oracleContractAddress = ContractAddresses.GetOracleContractAddress(_isTestNet);

            BigInteger currentBlockNumber;
            var specificBlockNumber = 0;  // 要測試特定區塊號時才輸入
            if (specificBlockNumber > 0)
            {
                currentBlockNumber = specificBlockNumber;
            }
            else
            {
                currentBlockNumber = await GetCurrentBlockNumber(web3);
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
                        var oldBlockNumber = currentBlockNumber;
                        currentBlockNumber = await GetCurrentBlockNumber(web3);
                        Console.WriteLine($"批次處理:{oldBlockNumber}-{currentBlockNumber}");

                        // 並發處理區塊，最高並發量為1
                        var semaphore = new System.Threading.SemaphoreSlim(1, 1);
                        var tasks = new List<Task>();

                        for (var i = oldBlockNumber; i <= currentBlockNumber; i++)
                        {
                            var blockNumber = i;

                            await semaphore.WaitAsync();

                            var task = Task.Run(async () =>
                            {
                                try
                                {
                                    var block = await GetBlockByNumber(web3, new HexBigInteger(blockNumber));
                                    if (block != null)
                                    {
                                        Console.WriteLine($"=======>Number:{blockNumber},Time:{FormatBlockTimestamp(block.Timestamp)}");
                                        await ExecuteBlock(web3, usdcContract, eoaAddress, walletContractAddress, gameContractAddress, oracleContractAddress, block).ConfigureAwait(false);
                                    }
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
                        currentBlockNumber++;
                    }

                    //取得區塊資訊
                    var blockResponse = await GetBlockByNumber(web3, new HexBigInteger(currentBlockNumber));

                    if (blockResponse == null)
                    {
                        continue;
                    }

                    Console.WriteLine($"BlockNumber:{currentBlockNumber},BlockTime:{FormatBlockTimestamp(blockResponse.Timestamp)}");

                    // 查DB是否有存在這些合約地址
                    var transactionGroupTo = blockResponse.Transactions.GroupBy(o => o.To).ToList();
                    var transactionGroupFrom = blockResponse.Transactions.GroupBy(o => o.From).ToList();
                    // TODO - 查詢DB
                    // 質押 - 用GroupBy To地址查詢是否存在WalletContract裡的地址
                    // 更新EOA餘額 - 用GroupBy From地查詢是否存在ChainEoaPool裡的地址

                    await ExecuteBlock(web3, usdcContract, eoaAddress, walletContractAddress, gameContractAddress, oracleContractAddress, blockResponse);

                    currentBlockNumber++;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    var intervalMs = 2000;
                    var elapsedMs = ToInt(TimeSpan.FromTicks(DateTimeOffset.Now.Ticks - currTimeStamp).TotalMilliseconds, intervalMs);
                    var delayMs = intervalMs - elapsedMs;
                    if (delayMs > 100)
                    {
                        await Task.Delay(delayMs);
                    }
                    else
                    {
                        Console.WriteLine($"處理時間超過2秒=>{currentBlockNumber}");
                        isNeedGetNewBlock = true;
                    }
                }
            }
        }

        private static async Task<BlockWithTransactions?> GetBlockByNumber(Web3 web3, HexBigInteger blockNumber)
        {
            var blockStopwatch = System.Diagnostics.Stopwatch.StartNew();

            var blockResponse = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(blockNumber);

            blockStopwatch.Stop();

            Console.WriteLine($"[GetBlockByNumber] blockStopwatch 耗時: {blockStopwatch.ElapsedMilliseconds}ms");
            return blockResponse;
        }

        private async Task ExecuteBlock(Web3 web3, string usdcContract, string[] eoaAddress, string[] walletContractAddress, string[] gameContractAddress, string[] oracleContractAddress, BlockWithTransactions blockInfo)
        {
            var blockStopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int performanceThresholdMs = 1000; // 效能門檻：1秒

            //取得區塊資訊
            var filterStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var transactions = blockInfo.Transactions.Where(t => t.To != null
                                                                 && (walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase)
                                                                 || eoaAddress.Contains(t.From, StringComparer.OrdinalIgnoreCase)
                                                                 || (t.To.Equals(usdcContract, StringComparison.OrdinalIgnoreCase) && walletContractAddress.Contains(t.To, StringComparer.OrdinalIgnoreCase))
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
                Console.WriteLine($"取得交易收據:{transaction.TransactionHash}");
                var transactionReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction.TransactionHash);
                receiptStopwatch.Stop();
                if (receiptStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 取得交易收據耗時: {receiptStopwatch.ElapsedMilliseconds}ms (TxHash: {transaction.TransactionHash})");

                if (transactionReceipt == null || transactionReceipt.Logs == null)
                    continue;

                var logs = transactionReceipt.Logs.Select(l => l.ToObject<FilterLog>()).Where(l => l != null).Cast<FilterLog>().ToList();

                // 質押
                var stakeStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var transferEvent = ConvertLogsToEvent<UsdcEventTransfer>(logs);
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
                var redeemEvents = ConvertLogsToEvent<WalletContractEventRedeemed>(logs);
                foreach (var redeem in redeemEvents)
                {
                    Console.WriteLine($"Redeem From: {redeem.Event.WalletContract}, To: {redeem.Event.Wallet}, Value: {redeem.Event.AmountInDecimal}, ByUser: {redeem.Event.ByUser}");
                }
                redeemStopwatch.Stop();
                if (redeemEvents.Any() && redeemStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理贖回事件耗時: {redeemStopwatch.ElapsedMilliseconds}ms");

                // 綁定錢包
                var bindStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var bindWalletEvents = ConvertLogsToEvent<WalletContractEventWalletBound>(logs);

                foreach (var bindWallet in bindWalletEvents)
                {
                    Console.WriteLine($"UnBindWallet WalletContract: {bindWallet.Event.WalletContract}, Wallet: {bindWallet.Event.Wallet}, ByUser: {bindWallet.Event.ByUser}");
                }
                bindStopwatch.Stop();
                if (bindWalletEvents.Any() && bindStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理綁定錢包事件耗時: {bindStopwatch.ElapsedMilliseconds}ms");

                // 預簽名
                var preSignStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var preSignEvents = ConvertLogsToEvent<WalletContractEventPreSigned>(logs);

                foreach (var preSign in preSignEvents)
                {
                    Console.WriteLine($"PreSign From: {transaction.From}, To: {transaction.To} RequestId: {preSign.Event.RequestId}, Amount: {ChainUnitUtil.ToDecimal(preSign.Event.Amount)} ByUser: {preSign.Event.ByUser}");
                }
                preSignStopwatch.Stop();
                if (preSignEvents.Any() && preSignStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理預簽名事件耗時: {preSignStopwatch.ElapsedMilliseconds}ms");

                // EOA
                var eoaStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var eoaGasEvents = ConvertLogsToEvent<LogFeeTransfer>(logs);
                foreach (var gas in eoaGasEvents)
                {
                    if (eoaAddress.Contains(gas.Event.From, StringComparer.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Eoa From: {gas.Event.From}, To: {gas.Event.To}, Amount: {ChainUnitUtil.RoundTo10DecimalPlaces(gas.Event.Amount)}");
                    }
                }

                var eoaPolEvents = ConvertLogsToEvent<LogTransfer>(logs);
                foreach (var pol in eoaPolEvents)
                {
                    Console.WriteLine($"Eoa From: {pol.Event.From}, To: {pol.Event.To}, Amount: {ChainUnitUtil.RoundTo10DecimalPlaces(pol.Event.Amount)}");
                }
                eoaStopwatch.Stop();
                if ((eoaGasEvents.Any() || eoaPolEvents.Any()) && eoaStopwatch.ElapsedMilliseconds > performanceThresholdMs)
                    Console.WriteLine($"[效能警告] 處理EOA事件耗時: {eoaStopwatch.ElapsedMilliseconds}ms");

                // 遊戲轉帳
                var gameStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var gameEvents = ConvertLogsToEvent<UsdcEventTransfer>(logs);
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
                var oracleEvents = ConvertLogsToEvent<UsdcEventTransfer>(logs);
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

        private async Task<BigInteger> GetCurrentBlockNumber(Web3 web3)
        {
            var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            return blockNumber.Value;
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
        /// 轉換 FilterLog 為 EventLog
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logs"></param>
        /// <returns></returns>
        private List<EventLog<T>> ConvertLogsToEvent<T>(IEnumerable<FilterLog> logs) where T : IEventDTO, new()
        {
            var decoded = Event<T>.DecodeAllEvents(logs.ToArray());
            return decoded;
        }

        private static string FormatBlockTimestamp(HexBigInteger timestamp)
        {
            if (timestamp == null)
                return "N/A";

            try
            {
                var value = (long)timestamp.Value;

                // 如果數值很大則視為毫秒，否則視為秒
                DateTimeOffset dto = value > 1_000_000_000_000L
                    ? DateTimeOffset.FromUnixTimeMilliseconds(value)
                    : DateTimeOffset.FromUnixTimeSeconds(value);

                return dto.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                return "N/A";
            }
        }
    }
}