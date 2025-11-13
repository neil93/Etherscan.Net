using EthScanNet.Lib.Models.ApiRequests.Proxy;
using EthScanNet.Lib.Models.ApiResponses.Proxy;
using System.Threading.Tasks;

namespace EthScanNet.Lib.EScanApi
{
    public sealed class Proxy : BaseApi
    {
        public Proxy(EScanClient client) : base(client)
        {
        }
        
        public async Task<EScanEthCurrentBlock> CurrentBlock()
        {
            EScanEthBlockNumber ethBlockNumber = new(this.Client);
            return await ethBlockNumber.SendAsync();
        }

        /// <summary>
        /// Returns the number of most recent block
        /// </summary>
        public async Task<EScanEthCurrentBlock> EthBlockNumber()
        {
            EScanEthBlockNumber request = new(this.Client);
            return await request.SendAsync();
        }

        /// <summary>
        /// Returns information about a block by block number
        /// </summary>
        /// <param name="tag">Block number in hex format or "latest", "earliest", "pending"</param>
        /// <param name="fullTransactionObjects">If true, returns full transaction objects; if false, only transaction hashes</param>
        public async Task<EScanEthBlock> EthGetBlockByNumber(string tag, bool fullTransactionObjects = false)
        {
            EScanEthGetBlockByNumber request = new(this.Client, tag, fullTransactionObjects);
            return await request.SendAsync();
        }

        /// <summary>
        /// Returns information about an uncle by block number and index
        /// </summary>
        /// <param name="tag">Block number in hex format</param>
        /// <param name="index">Uncle index position in hex format</param>
        public async Task<EScanEthBlock> EthGetUncleByBlockNumberAndIndex(string tag, string index)
        {
            EScanEthGetUncleByBlockNumberAndIndex request = new(this.Client, tag, index);
            return await request.SendAsync();
        }

        /// <summary>
        /// Returns the number of transactions in a block
        /// </summary>
        /// <param name="tag">Block number in hex format or "latest", "earliest", "pending"</param>
        public async Task<EScanEthTransactionCount> EthGetBlockTransactionCountByNumber(string tag)
        {
            EScanEthGetBlockTransactionCountByNumber request = new(this.Client, tag);
            return await request.SendAsync();
        }

        /// <summary>
        /// Returns information about a transaction by transaction hash
        /// </summary>
        /// <param name="txHash">Transaction hash</param>
        public async Task<EScanEthTransaction> EthGetTransactionByHash(string txHash)
        {
            EScanEthGetTransactionByHash request = new(this.Client, txHash);
            return await request.SendAsync();
        }

        /// <summary>
        /// Returns information about a transaction by block number and transaction index position
        /// </summary>
        /// <param name="tag">Block number in hex format</param>
        /// <param name="index">Transaction index position in hex format</param>
        public async Task<EScanEthTransaction> EthGetTransactionByBlockNumberAndIndex(string tag, string index)
        {
            EScanEthGetTransactionByBlockNumberAndIndex request = new(this.Client, tag, index);
            return await request.SendAsync();
        }

        /// <summary>
        /// Returns the number of transactions sent from an address
        /// </summary>
        /// <param name="address">Address to check for transaction count</param>
        /// <param name="tag">Block number in hex format or "latest", "earliest", "pending"</param>
        public async Task<EScanEthTransactionCount> EthGetTransactionCount(string address, string tag)
        {
            EScanEthGetTransactionCount request = new(this.Client, address, tag);
            return await request.SendAsync();
        }

        /// <summary>
        /// Submits a pre-signed transaction for broadcast to the Ethereum network
        /// </summary>
        /// <param name="hex">Signed transaction data in hex format</param>
        public async Task<EScanEthTransactionHash> EthSendRawTransaction(string hex)
        {
            EScanEthSendRawTransaction request = new(this.Client, hex);
            return await request.SendAsync();
        }

        /// <summary>
        /// Returns the receipt of a transaction by transaction hash
        /// </summary>
        /// <param name="txHash">Transaction hash</param>
        public async Task<EScanEthTransactionReceipt> EthGetTransactionReceipt(string txHash)
        {
            EScanEthGetTransactionReceipt request = new(this.Client, txHash);
            return await request.SendAsync();
        }

        /// <summary>
        /// Executes a new message call immediately without creating a transaction on the blockchain
        /// </summary>
        /// <param name="to">Address to execute the call against</param>
        /// <param name="data">Hash of the method signature and encoded parameters</param>
        /// <param name="tag">Block number in hex format or "latest", "earliest", "pending"</param>
        public async Task<EScanEthCallResult> EthCall(string to, string data, string tag)
        {
            EScanEthCall request = new(this.Client, to, data, tag);
            return await request.SendAsync();
        }

        /// <summary>
        /// Returns code at a given address
        /// </summary>
        /// <param name="address">Address to get code from</param>
        /// <param name="tag">Block number in hex format or "latest", "earliest", "pending"</param>
        public async Task<EScanEthCode> EthGetCode(string address, string tag)
        {
            EScanEthGetCode request = new(this.Client, address, tag);
            return await request.SendAsync();
        }

        /// <summary>
        /// Returns the value from a storage position at a given address
        /// </summary>
        /// <param name="address">Address to get storage from</param>
        /// <param name="position">Storage position in hex format</param>
        /// <param name="tag">Block number in hex format or "latest", "earliest", "pending"</param>
        public async Task<EScanEthStorageValue> EthGetStorageAt(string address, string position, string tag)
        {
            EScanEthGetStorageAt request = new(this.Client, address, position, tag);
            return await request.SendAsync();
        }

        /// <summary>
        /// Returns the current price per gas in wei
        /// </summary>
        public async Task<EScanEthGasPriceResult> EthGasPrice()
        {
            EScanEthGasPrice request = new(this.Client);
            return await request.SendAsync();
        }

        /// <summary>
        /// Makes a call or transaction, which won't be added to the blockchain and returns the used gas
        /// </summary>
        /// <param name="to">Address the transaction is directed to</param>
        /// <param name="value">Value sent with this transaction in hex format (optional)</param>
        /// <param name="gasPrice">Gas price in hex format (optional)</param>
        /// <param name="gas">Gas provided for the transaction in hex format (optional)</param>
        /// <param name="data">Hash of the method signature and encoded parameters (optional)</param>
        public async Task<EScanEthEstimateGasResult> EthEstimateGas(string to, string value = null, string gasPrice = null, string gas = null, string data = null)
        {
            EScanEthEstimateGas request = new(this.Client, to, value, gasPrice, gas, data);
            return await request.SendAsync();
        }
    }
}