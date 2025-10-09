using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace EthScanNet.Lib.Models.Events
{
    /// <summary>
    /// 预签名事件
    /// </summary>
    [Event("PreSigned")]
    public class WalletContractEventPreSigned : IEventDTO
    {
        [Parameter("uint256", "requestId", 1)]
        public BigInteger RequestId { get; set; }

        [Parameter("uint256", "amount", 2, false)]
        public BigInteger Amount { get; set; }

        [Parameter("bool", "byUser", 3)]
        public bool ByUser { get; set; }
    }
}
