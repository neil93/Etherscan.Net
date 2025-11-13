using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace EthScanNet.Lib.Models.Events
{
    /// <summary>
    /// POL转账事件
    /// </summary>
    [Event("LogTransfer")]
    public class LogTransfer : IEventDTO
    {
        [Parameter("address", "token", 1, true)]
        public string Token { get; set; } = string.Empty;

        [Parameter("address", "from", 2, true)]
        public string From { get; set; } = string.Empty;

        [Parameter("address", "to", 3, true)]
        public string To { get; set; } = string.Empty;

        [Parameter("uint256", "amount", 4)]
        public BigInteger Amount { get; set; }

        [Parameter("uint256", "input1", 5)]
        public BigInteger Input1 { get; set; }

        [Parameter("uint256", "input2", 6)]
        public BigInteger Input2 { get; set; }

        [Parameter("uint256", "output1", 7)]
        public BigInteger Output1 { get; set; }

        [Parameter("uint256", "output2", 8)]
        public BigInteger Output2 { get; set; }
    }
}