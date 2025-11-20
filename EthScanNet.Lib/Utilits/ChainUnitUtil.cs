using Nethereum.Util;
using System;
using System.Net.NetworkInformation;
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

        public static decimal RoundTo10DecimalPlaces(decimal number)
        {
            return Math.Round(number, 10, MidpointRounding.AwayFromZero);
        }

        public static decimal RoundTo10DecimalPlaces(BigInteger number)
        {
            var amount = UnitConversion.Convert.FromWei(number);

            return RoundTo10DecimalPlaces(amount);
        }
    }
}