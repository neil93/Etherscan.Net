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
        private readonly EScanNetwork _network = EScanNetwork.PolygonMainNet;

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
            // 測試Amoy鏈
            // USDC Contract - Fake USDC 合約地址
            var amoyUsdcContract = "0x5bC0720B80f66C8a0F0ba32F1f949D101C24171A";

            // 以下使用Dev環境測試資料
            // EOA Address
            string[] eoaAddress = GetDbEoaAddress();

            // Walet Contract Address
            string[] walletContractAddress = GetWalletContractAddress();

            // Game Contract Address
            string[] gameContractAddress = GetGameContractAddress();

            // Oracle Contract Address
            string[] oracleContractAddress = GetOracleContractAddress();

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

            // 每2秒掃描一個區塊
            while (true)
            {
                var currTimeStamp = DateTimeOffset.Now.Ticks;

                try
                {
                    // 質押交易

                    //取得區塊資訊
                    var blockResponse = await client.Proxy.EthGetBlockByNumber(currentNumber, true);

                    var blockInfo = blockResponse.GetBlockInfo();

                    Console.WriteLine($"BlockNumber:{ConvertHexToDecimal(currentNumber)},BlockTime:{FormatBlockTimestamp(blockInfo.Timestamp)}");

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

                        // 轉換成SyncBlock Worker的Receipt格式
                        var transactionReceipt = receiptResponse.GetTransactionReceipt();

                        var receiptInfos = receiptResponse.GetReceiptInfo();

                        // 質押
                        var transferEvent = ConvertLogsToEvent<UsdcEventTransfer>(receiptInfos.Logs);
                        var resultEvents = transferEvent.Where(e => e.Log.Address.Equals(amoyUsdcContract, StringComparison.OrdinalIgnoreCase)
                                                                && !eoaAddress.Contains(e.Event.From)
                                                                && walletContractAddress.Contains(e.Event.To, StringComparer.OrdinalIgnoreCase)).ToList();

                        foreach (var transfer in resultEvents)
                        {
                            Console.WriteLine($"Stake BlockNumber:{transfer.Log.BlockNumber}, From: {transfer.Event.From}, To: {transfer.Event.To}, Value: {ChainUnitUtil.ToDecimal(transfer.Event.Value)}");
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
                            Console.WriteLine($"PreSign From: {transaction.From}, To: {transaction.To} RequestId: {preSign.Event.RequestId}, Amount: {ChainUnitUtil.ToDecimal(preSign.Event.Amount)} ByUser: {preSign.Event.ByUser}");
                        }

                        // EOA
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

                        // 遊戲轉帳
                        var gameEvents = ConvertLogsToEvent<UsdcEventTransfer>(receiptInfos.Logs);
                        foreach (var game in gameEvents)
                        {
                            if (gameContractAddress.Contains(game.Event.To, StringComparer.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"遊戲 收入 From: {game.Event.From}, To: {game.Event.To}, Amount: {ChainUnitUtil.ToDecimal(game.Event.Value)}");
                            }

                            if (gameContractAddress.Contains(game.Event.From, StringComparer.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"遊戲 支出 From: {game.Event.From}, To: {game.Event.To}, Amount: {ChainUnitUtil.ToDecimal(game.Event.Value)}");
                            }
                        }

                        // 預言機合約轉帳
                        var oracleEvents = ConvertLogsToEvent<UsdcEventTransfer>(receiptInfos.Logs);
                        foreach (var oracle in oracleEvents)
                        {
                            if (oracleContractAddress.Contains(oracle.Event.To, StringComparer.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"預言機 收入 From: {oracle.Event.From}, To: {oracle.Event.To}, Amount: {ChainUnitUtil.ToDecimal(oracle.Event.Value)}");
                            }

                            if (oracleContractAddress.Contains(oracle.Event.From, StringComparer.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"預言機 支出 From: {oracle.Event.From}, To: {oracle.Event.To}, Amount: {ChainUnitUtil.ToDecimal(oracle.Event.Value)}");
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
                        Console.WriteLine($"處理時間超過2秒=>{currentNumber}");
                    }
                }
            }
        }

        /// <summary>
        /// 日誌
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task RunLogsCommandsAsync(EScanClient client)
        {
            // 正式鏈
            var bindWalletTopic0 = "0x0ca052931610b15a08f6d7b445a2be5e2d377dd2c8945678bb64fbecb2725708";
            var transferTopic0 = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";
            var redeemTopic0 = "0x378f55a9a0032096f81e501f6fba06e54947e956df2afe99d645ca71183fb269";
            var preSignedTopic0 = "0xbb8f597c6a23e718c7579b21e311c3daf7851a8456dbb20e97b3124cd3a66022";

            var usdcContractAddress = "0x3c499c542cef5e3811e1192ce70d8cc03d5c3359";     // USDC 合約地址

            Console.WriteLine("Logs test started");
            //EScanLogs logs = await client.Logs.GetLogsAsync(fromBlock: "0x1", toBlock: "latest", topic0: "0xbb8f597c6a23e718c7579b21e311c3daf7851a8456dbb20e97b3124cd3a66022", page:1,offset:100);

            // 質押
            EScanLogs logs = await client.Logs.GetLogsAsync(fromBlock: "78830144", toBlock: "78830144", topic0: transferTopic0, page: 1, offset: 10000);
            var transferEvent = await GetBoundWalletEvent<UsdcEventTransfer>(logs);

            var eoaAddress = new string[] { "0xF177B7F19aD64a9C04a45cd9E41505b1c9A5B4C6", "0xD02a7763cac2c95D013fBE8A93e406f37F83294f" };   // EOA

            var walletContractAddress = new string[] { "0x70D74B6548C0E8c524b2b2B0997E3E539C93D72d", "0x96e52de6892d4B4811cEaa929E912cCd90fd6041", "0x0F7B6aC80951B68301b4321a7D34f76E03AF06Fe" };  // �ϥΪ̿��]�X��

            var qq = transferEvent.Where(e => !eoaAddress.Contains(e.Event.From)
                && e.Log.Address == usdcContractAddress
                && walletContractAddress.Contains(e.Event.To));

            foreach (var item in qq)
            {
                Console.WriteLine($"質押:{item.Event.To},金額: {ChainUnitUtil.RoundTo10DecimalPlaces(item.Event.Value)} Usd.");
            }

            Console.WriteLine("Logs transferEvent test complete");

            // 贖回
            logs = await client.Logs.GetLogsAsync(fromBlock: "78535606", toBlock: "78535606", topic0: redeemTopic0, page: 1, offset: 1000);
            var redeemedEvent = await GetBoundWalletEvent<WalletContractEventRedeemed>(logs);
            Console.WriteLine("Logs redeemedEvent  test complete");

            // 綁定/解綁錢包
            logs = await client.Logs.GetLogsAsync(fromBlock: "78535522", toBlock: "78535522", topic0: bindWalletTopic0, page: 1, offset: 1000);
            var boundWalletEvent = await GetBoundWalletEvent<WalletContractEventWalletBound>(logs);
            Console.WriteLine("Logs boundWalletEvent test complete");

            // 預簽名
            logs = await client.Logs.GetLogsAsync(fromBlock: "78533675", toBlock: "78533675", topic0: preSignedTopic0, page: 1, offset: 1000);
            var preSignedEvent = await GetBoundWalletEvent<WalletContractEventPreSigned>(logs);
            Console.WriteLine("Logs boundWalletEvent test complete");

            Console.WriteLine("GetLogsAsync: " + logs.Message);
            Console.WriteLine("All Logs test complete");
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
            return new string[] {
"0x00ff9a25cd99A750C8dC6f1855A5bbBb4319a526",
"0x03d1EDb7413B6496068D10660369F722288857BA",
"0x048614388fc7968908F7125A6AEfcc2Ea1DC09ec",
"0x14546c58F2F20ec23aA79CeD158F09b32a32a386",
"0x15a0fB26C2db76FB3d0FbE39FCa492756B83f2c6",
"0x173387E42b7032cf3129e44C2d7493F32eFA3bdE",
"0x19362781C6bcaE7c259C2C72dcF055C15B9c6549",
"0x1C773A020C6eAe5e8DcA6e7e920635c922E4D73a",
"0x1fE4b64A68501c1166B65950F9fb12fE4ba3675C",
"0x2513f047da7e7A89E8Eb7e3D0650f1c1811cE459",
"0x27c5be593e53EB377C4A4CEF118d09D8373f0319",
"0x2b443fBb7C0AF069E6356d62e422d546466ea213",
"0x2Dd80C5A4CF9084CFaef3FDf0341c99C4809FCFC",
"0x39078CB10D6548074cBC1ed2a42DCB0214313185",
"0x3D60874072F3AbBD79e515a8Ee58EC6B13d93C45",
"0x4b1e186Fa185e3D2D52E99506DB1D6e9A41B6DEF",
"0x4E5FFcF1a716d2345090fFDDEf827b3A127449f2",
"0x4fFb462236465A400611defbE4C5857B6DEa17Cb",
"0x50e5A0cDB21D9fFb7d7f3dBc7fCe894Aa59A8048",
"0x525Cc5E82cd67be53D4baDFE659f3D42b3AF02A0",
"0x53879c0D9c909C64dACd973Ac08A51A888aC01F5",
"0x53c7A388cCb6f2BB6DAD2f0725838C4D27A49038",
"0x54e807c06c97F61b65160224e086997907216E83",
"0x5DcF821Cb2E1ff50D040c05a43c9CE9580cf3966",
"0x5f014686E589702E115130B59Fb8A69E56b26F21",
"0x61a042761D3FA9f0DF2A5f3B0AB62B7759625E39",
"0x655dd196026c21507fC83330D0E0Aa3C85E1b8b4",
"0x694a3FeFdB8202Ec535f6a201246F0fbA0958760",
"0x6C1B30b8C9EbE7AaD5B38257AC72B98dd8a0c561",
"0x6d2029013688eEC031F74990C468392DF4e8458F",
"0x6ff862f7BCd46d360Cd855159adcb3d2CC69efe4",
"0x7301638C3364a65333FB9Dc953CCB1D882DbfC30",
"0x7419392b3D0E2e4A5c3c524f6d11351db18f7Ebc",
"0x74Dac12117849bd761b1e69097F37CdD739aD7d1",
"0x7D971242576E3d7eB067e9443Ea05714D55539Fa",
"0x807bEba347219eC9f5Dd0C4a3AC872cfd5238Ea0",
"0x868C4B1eab60F099FC2acF9E730a2eD8A612b1A4",
"0x879D7182a3DeB8CE0389F121906E05d95b4b070B",
"0x897105407C2D79A065A25d885128388e28Fb22f0",
"0x89Ca974CdEaeCe2A26d23DE9BDf6d3C72f8344dB",
"0x8b7B9DeF778a39078c227BB69B9E375faB707321",
"0x90506a345B3242E3a8F949A4b937e983d5798c95",
"0x981a14A2ec8666c18bd75B9985c009E801918584",
"0x9b9e66D1FF9ca02a6321B11bEEb2E1C4c43FD3Ce",
"0x9D2a80bB52B0E74Bb66f34bf40Ce8E0C006561B1",
"0x9e8Df81A0dC7D2323802A37E46C81139851fE188",
"0xA1cBd2F311BA6d88C3F938ab132A5a710353506e",
"0xA4cB80811aACB2697e7D44F34Fd39fcAB027Bca1",
"0xa4E2F41b13F9856Ad1a39F6e30D9A95e2280a330",
"0xA9976aaBd5515E81c7a7bb6098F92ed773C1d807",
"0xACF81E69473e472b354f3d1aa982Ac4EBbC5461a",
"0xAE36988875980782dA6e9bdf413aEcf11351B541",
"0xaFd256DD244bBE1eF238611e5d6da728dCcC87e1",
"0xb023BE78DFC3fff43C141C425b7A38D4cba0EF62",
"0xb2a78333cE8bf4f09D7Ae357416D26166C6e5C4D",
"0xB2E2cc7FABC80Ce12BC81564978B4c227a71CB9F",
"0xb5Fc7ffbBc00989D57E2520f06ea8d90418005C3",
"0xb5fe9899E9EA0f1553af60e64886DCe1C96f2573",
"0xB65c5CeBf81d1Ac176A0078F1441cC4A68C1Bf1B",
"0xB80fe8Bd1D78544A096cCE3B04791104834602B0",
"0xb9654b1e7fe288eE632AD6928BdB76fdc437a383",
"0xbb53bb8Dc9f015A90E73544B10Efff4D97A48cC5",
"0xbD73d0722deCa6Cbd2e4DD5733a16Db3C35a7923",
"0xC118A734E2420b145c1855B19ff7e99E7e3536D0",
"0xc18871d80dA3C7dAa6a2B98f970b0BdeB4671a0f",
"0xc19f7d0ac6EA0a93282B423faf23Dcf5ea89793c",
"0xC5C067773C58d06fA178C22bD3161aB1e9106778",
"0xCFcBBc6eab172CC503eBa4e332f28C470285cD65",
"0xD39f0fA88eFaA1c1a06057da913F3bAf992Ecf46",
"0xDaE297aDf76162431aAd731368fB233605436d29",
"0xDC3d7944554018Eee82b666Ca60f12EE9A50eEd8",
"0xDEdbb0806059234400FC44B9949Ff1993F4AEC4F",
"0xE5716cfa9A88a43410D42b71e517d440BFBF0A62",
"0xec4bdd5c5681792ABb4268fae361911b3BCE6199",
"0xf40fD708478e950B1947956AA9405f244Ca317Aa",
"0xF4efa7506120512BE972612462f5ae2bB8572cC8",
"0xFeFC55c2993b527e62E7e947291b05d84316399A",
"0xFefFa1BAa1d32A54a351C8A9921FdA6Bf9Ec4F73"
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
"0x027C39e86014B5027f1128105589bE7f26A074B2",
"0x044FF1e03A8431E613a4104416221F3799bDD594",
"0x0551b073588254fE7CD25f89b214cdE03d5b2BfF",
"0x09b89D1B30b916cE07DD6c37ba03056707442B5E",
"0x1018e9Abe30751C189Ab8c792571d655EC9508df",
"0x13448F09b5ccBf2583DA2AA6576F6DA54B5Bb238",
"0x19d9066bD4BbE81f09e97570DBC6424d67D5670e",
"0x1cdaa416EE35374B104aaD15dF4A597c6551A4F5",
"0x202eB53c7D074e34C51223f198284FbF5fA6D63e",
"0x30fC2AF01033AdCF83bBd1ED0BbE2956210087d4",
"0x31D3aa532C5b9C422d600E6A63fcB4BaB506d9eC",
"0x3b9c80C47841f7988d512A601Cd4595EC97DeCdA",
"0x3c9D05E5e70A5723e85023F581c942a10EEe7717",
"0x3f1aE60f1358d70B6E69d844e7C9958F96Ad5b73",
"0x44A0E9103E1ACd05EAc7f3C4552284806cc63230",
"0x48fD8f35879e6cF622C71b13E69681E40115Db1B",
"0x551c584c7c29D2A68DBCD4478939c8504C33105a",
"0x5c67db7E1d2fFa5547b71cD36A4CB674dbD67B35",
"0x5d3697A3F9f9D825F2a54ec198977aAf9C7Be061",
"0x5f53eD8AC5B7416E8724386e110d9B04b36a4259",
"0x6Df77e4D5D25177795b0B60Ce85Bd4241B144D8E",
"0x704AF0f269add6f2C87cecacB7b940ECa3B491A0",
"0x7C1cB2876BA9824DA7c747647F7399Db220c4193",
"0x7D88BdBA692c0351929e408999b20a046234ed3c",
"0x8601bB78909C9d8AEA4c26836770012dD08273E8",
"0x881aDAc6608c994888bbE9076832Bf0D599d079F",
"0x8C35cc53129b446bE1b3857CEFEAFDA2A705407d",
"0x919a55d312Bb712c8B3fd75dEaf6a76b4095c542",
"0x97321fE4Fd2cC0700D9220f15D3db3F79a560980",
"0x9B4BC8180BF651fFa059A5bD6ad0e756F6C73588",
"0xA520d545Caca11aE07B6e8AeB90732292b77Af63",
"0xaC69e7C9900264D15C5B86226089aceE3D328b67",
"0xaD631e48856e6B82CAd13887A63a64E1B2001460",
"0xB1b3ef54188900Da533D9dEe9363dC540F8b016A",
"0xc29c82f27A06939726183Da23D868c00EC445fC0",
"0xcA089fa80804A2810FCcb5Fd215a72a682114AF1",
"0xCC5B015795401f4ecd4C161d96F7D9eB148EAEf6",
"0xcE99f93Ac77F353824163e2F7690bF7A050C64bD",
"0xD3452324cE44F2E2C716EeF0C6B8954FA402CC4E",
"0xD3F0425ACFAA289E6Baba7D5c19C752bdb6ECe8B",
"0xd68a5D67ECF43D2Eb7dC2D82f37B249aBcb5A0cd",
"0xDC1Ea715ab1f89Bcc5482463bD17bc5c5b1e8a3b",
"0xdc2b35d27d4b13489bA30Ec47360E3aFfa216E80",
"0xe82C13F9B1A40Caee85400A483c71EA229760736",
"0xEF68B914108Bf4FDE823FE219d0778bD7f45612d",
"0xF11E3006bE7fc443b637ac1cfE4B5d6809d3aaC3",
"0xF19c1b4A6a391667335edfacCbD3393E375787Dd",
"0xfC204dBE53932f95Ce6C104C45A10ca4706d81F0"
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