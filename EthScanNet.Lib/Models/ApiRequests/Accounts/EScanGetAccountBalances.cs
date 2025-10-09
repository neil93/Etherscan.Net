using System.Collections.Generic;
using System.Linq;
using EthScanNet.Lib.Enums;
using EthScanNet.Lib.Models.ApiResponses.Accounts;
using EthScanNet.Lib.Models.EScan;

namespace EthScanNet.Lib.Models.ApiRequests.Accounts
{
    internal class EScanGetAccountBalances : EScanRequest
    {
        public string Address { get; set; }

        public EScanGetAccountBalances(IEnumerable<EScanAddress> addresses, EScanClient eScanClient)
            : base(eScanClient, typeof(EScanBalances), EScanModules.Account, EScanActions.BalanceMulti)
        {
            this.Address = string.Join(",", addresses.Select(a => a.ToString()));
        }
    }
}