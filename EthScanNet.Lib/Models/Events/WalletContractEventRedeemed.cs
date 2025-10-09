using EthScanNet.Lib.Utilits;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace EthScanNet.Lib.Models.Events
{
    /// <summary>
    /// 赎回事件
    /// </summary>
    [Event("Redeemed")]
    public class WalletContractEventRedeemed : IEventDTO
    {
        [Parameter("uint256", "requestId", 1)]
        public BigInteger RequestId { get; set; }

        [Parameter("address", "walletContract", 2)]
        public string WalletContract { get; set; } = string.Empty;

        [Parameter("address", "wallet", 3)]
        public string Wallet { get; set; } = string.Empty;

        [Parameter("uint256", "amount", 4)]
        public BigInteger Amount { get; set; }

        [Parameter("bool", "byUser", 5)]
        public bool ByUser { get; set; }

        public decimal AmountInDecimal { get { return ChainUnitUtil.ToDecimal(this.Amount); } }
    }
}
