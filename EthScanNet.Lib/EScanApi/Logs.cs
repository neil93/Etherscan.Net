using System.Threading.Tasks;
using EthScanNet.Lib.Models.ApiRequests.Logs;
using EthScanNet.Lib.Models.ApiResponses.Logs;
using EthScanNet.Lib.Models.EScan;

namespace EthScanNet.Lib.EScanApi
{
    public sealed class Logs : BaseApi
    {
        public Logs(EScanClient client) : base(client)
        {
        }

        /// <summary>
        /// Get Event Logs by Address
        /// </summary>
        /// <param name="address">The address to get logs for</param>
        /// <param name="fromBlock">The starting block number (e.g., "earliest", "latest", or a number)</param>
        /// <param name="toBlock">The ending block number (e.g., "earliest", "latest", or a number)</param>
        /// <param name="topic0">Topic 0 filter</param>
        /// <param name="topic1">Topic 1 filter</param>
        /// <param name="topic2">Topic 2 filter</param>
        /// <param name="topic3">Topic 3 filter</param>
        /// <param name="topicOperator">Logical operator for topics (AND/OR)</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="offset">Number of records to skip for pagination</param>
        /// <returns>A list of event logs</returns>
        public async Task<EScanLogs> GetLogsAsync(
            EScanAddress address = null,
            string fromBlock = null,
            string toBlock = null,
            string topic0 = null,
            string topic1 = null,
            string topic2 = null,
            string topic3 = null,
            string topicOperator = null,
            int? page = null,
            int? offset = null)
        {
            EScanGetLogs getLogs = new(address, fromBlock, toBlock, topic0, topic1, topic2, topic3, topicOperator, page, offset, this.Client);
            return await getLogs.SendAsync();
        }
    }
}