using Nethereum.Util;
using System.Numerics;

namespace EthScanNet.Lib.Utilits
{
    public static class ChainUnitUtil
    {
        public static BigInteger ToWei(decimal value)
        {
            return UnitConversion.Convert.ToWei(value, 6);
        }

        public static decimal ToDecimal(BigInteger value)
        {
            return UnitConversion.Convert.FromWei(value, 6);
        }
    }
}
