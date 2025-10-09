using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Logs;
using EthScanNet.Lib.Models.EScan;

namespace EthScanNet.Lib.Models.ApiRequests.Logs
{
    internal class EScanGetLogs : EScanRequest
    {
        public string Address { get; set; }
        public string FromBlock { get; set; }
        public string ToBlock { get; set; }
        public string Topic0 { get; set; }
        public string Topic1 { get; set; }
        public string Topic2 { get; set; }
        public string Topic3 { get; set; }
        public string TopicOperator { get; set; }
        public int? Page { get; set; }
        public int? Offset { get; set; }

        public EScanGetLogs(
            EScanAddress address = null,
            string fromBlock = null,
            string toBlock = null,
            string topic0 = null,
            string topic1 = null,
            string topic2 = null,
            string topic3 = null,
            string topicOperator = null,
            int? page = null,
            int? offset = null,
            EScanClient eScanClient = null)
            : base(eScanClient, typeof(EScanLogs), EScanModules.Logs, EScanActions.GetLogs)
        {
            this.Address = address?.ToString();
            this.FromBlock = fromBlock;
            this.ToBlock = toBlock;
            this.Topic0 = topic0;
            this.Topic1 = topic1;
            this.Topic2 = topic2;
            this.Topic3 = topic3;
            this.TopicOperator = topicOperator;
            this.Page = page;
            this.Offset = offset;
        }
    }
}