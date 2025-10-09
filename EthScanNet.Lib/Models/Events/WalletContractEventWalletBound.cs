using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace EthScanNet.Lib.Models.Events
{
    /// <summary>
    /// 绑定钱包事件
    /// </summary>
    [Event("WalletBound")]
    public class WalletContractEventWalletBound : IEventDTO
    {
        [Parameter("uint256", "requestId", 1)]
        public BigInteger RequestId { get; set; }

        [Parameter("address", "walletContract", 2)]
        public string WalletContract { get; set; } = string.Empty;

        [Parameter("address", "wallet", 3)]
        public string Wallet { get; set; } = string.Empty;

        [Parameter("bool", "byUser", 4)]
        public bool ByUser { get; set; }
    }
}
