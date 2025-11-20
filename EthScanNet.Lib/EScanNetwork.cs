namespace EthScanNet.Lib
{
    public class EScanNetwork
    {
        public static readonly EScanNetwork PolygonMainNet = new("https://api.etherscan.io/v2/api?chainid=137");
        public static readonly EScanNetwork PolygonAmy = new("https://api.etherscan.io/v2/api?chainid=80002");

        public bool IsBsc { get; }
        private readonly string _networkString;

        public EScanNetwork(string networkString)
        {
            this._networkString = networkString;
            this.IsBsc = false;
        }

        public EScanNetwork(string networkString, bool isBsc = false)
        {
            this._networkString = networkString;
            this.IsBsc = isBsc;
        }

        public override string ToString()
        {
            return this._networkString;
        }

        public static implicit operator string(EScanNetwork network)
        {
            return network._networkString;
        }
    }
}