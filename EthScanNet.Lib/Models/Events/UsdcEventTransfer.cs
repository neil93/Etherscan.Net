using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace EthScanNet.Lib.Models.Events
{
    /// <summary>
    /// ERC20代币转账事件
    /// </summary>
    [Event("Transfer")]
    public class UsdcEventTransfer : IEventDTO
    {
        [Parameter("address", "from", 1, true)]
        public string From { get; set; } = string.Empty;

        [Parameter("address", "to", 2, true)]
        public string To { get; set; } = string.Empty;

        [Parameter("uint256", "value", 3)]
        public BigInteger Value { get; set; }
    }
}
