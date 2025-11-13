# Proxy API Models and Methods

## 完整的 Response Models 和轉換方法

### 1. EScanEthCurrentBlock (區塊編號)
用於 `EthBlockNumber()` 和 `CurrentBlock()` 方法

**方法:**
- `GetBlockNumberHex()` - 取得 16 進位格式的區塊編號
- `GetBlockNumber()` - 取得 BigInteger 格式的區塊編號

**範例:**
```csharp
var response = await client.Proxy.EthBlockNumber();
var blockNumberHex = response.GetBlockNumberHex();  // "0x4b2da5b"
var blockNumber = response.GetBlockNumber();        // 78830171
```

---

### 2. EScanEthBlock (區塊資訊)
用於 `EthGetBlockByNumber()` 和 `EthGetUncleByBlockNumberAndIndex()` 方法

**方法:**
- `GetBlockInfo()` - 取得強型別的 BlockInfo 物件

**BlockInfo 屬性:**
- `Number` - 區塊編號
- `Hash` - 區塊雜湊
- `ParentHash` - 父區塊雜湊
- `Timestamp` - 時間戳
- `Miner` - 礦工地址
- `GasUsed` - 使用的 Gas
- `GasLimit` - Gas 限制
- `Transactions` - 交易列表（可能是雜湊或完整交易物件）
- `Difficulty` - 難度
- `TotalDifficulty` - 總難度
- `Size` - 區塊大小
- `Nonce` - Nonce
- `Uncles` - 叔塊列表
- `BaseFeePerGas` - 基礎費用（EIP-1559）
- `Withdrawals` - 提款列表（ETH 2.0）

**範例:**
```csharp
var response = await client.Proxy.EthGetBlockByNumber("0x4b2da5b", true);
var block = response.GetBlockInfo();
Console.WriteLine($"Block {block.Number} mined by {block.Miner}");
Console.WriteLine($"Transactions: {block.Transactions?.Count}");
```

---

### 3. EScanEthTransaction (交易資訊)
用於 `EthGetTransactionByHash()` 和 `EthGetTransactionByBlockNumberAndIndex()` 方法

**方法:**
- `GetTransactionInfo()` - 取得強型別的 TransactionInfo 物件

**TransactionInfo 屬性:**
- `Hash` - 交易雜湊
- `From` - 發送者地址
- `To` - 接收者地址
- `Value` - 轉帳金額（Wei，16進位）
- `Gas` - Gas 限制
- `GasPrice` - Gas 價格
- `MaxFeePerGas` - 最大費用（EIP-1559）
- `MaxPriorityFeePerGas` - 最大優先費用（EIP-1559）
- `Input` - 輸入資料
- `Nonce` - Nonce
- `BlockNumber` - 區塊編號
- `BlockHash` - 區塊雜湊
- `TransactionIndex` - 交易索引
- `Type` - 交易類型
- `ChainId` - 鏈 ID
- `V`, `R`, `S` - 簽名參數

**範例:**
```csharp
var response = await client.Proxy.EthGetTransactionByHash("0x27b7bd4f...");
var tx = response.GetTransactionInfo();
Console.WriteLine($"From: {tx.From}");
Console.WriteLine($"To: {tx.To}");
Console.WriteLine($"Value: {tx.Value}");
```

---

### 4. EScanEthTransactionReceipt (交易收據)
用於 `EthGetTransactionReceipt()` 方法

**方法:**
- `GetReceiptInfo()` - 取得強型別的 TransactionReceiptInfo 物件

**TransactionReceiptInfo 屬性:**
- `TransactionHash` - 交易雜湊
- `Status` - 狀態（"0x1" = 成功, "0x0" = 失敗）
- `BlockNumber` - 區塊編號
- `BlockHash` - 區塊雜湊
- `From` - 發送者地址
- `To` - 接收者地址
- `ContractAddress` - 合約地址（如果是創建合約）
- `GasUsed` - 使用的 Gas
- `CumulativeGasUsed` - 累計使用的 Gas
- `EffectiveGasPrice` - 實際 Gas 價格
- `Logs` - 事件日誌列表
- `LogsBloom` - Logs Bloom Filter
- `Type` - 交易類型

**LogInfo 屬性:**
- `Address` - 合約地址
- `Topics` - 主題列表
- `Data` - 資料
- `LogIndex` - 日誌索引
- `TransactionIndex` - 交易索引
- `BlockNumber` - 區塊編號

**範例:**
```csharp
var response = await client.Proxy.EthGetTransactionReceipt("0x27b7bd4f...");
var receipt = response.GetReceiptInfo();
Console.WriteLine($"Status: {(receipt.Status == "0x1" ? "Success" : "Failed")}");
Console.WriteLine($"Gas Used: {receipt.GasUsed}");
Console.WriteLine($"Logs: {receipt.Logs?.Count}");
```

---

### 5. EScanEthTransactionCount (交易數量)
用於 `EthGetBlockTransactionCountByNumber()` 和 `EthGetTransactionCount()` 方法

**方法:**
- `GetCountHex()` - 取得 16 進位格式的數量
- `GetCount()` - 取得 BigInteger 格式的數量

**範例:**
```csharp
// 取得區塊的交易數量
var response = await client.Proxy.EthGetBlockTransactionCountByNumber("0x4b2da5b");
var count = response.GetCount();

// 取得地址的交易數量（nonce）
var response2 = await client.Proxy.EthGetTransactionCount("0x742d35Cc...", "latest");
var nonce = response2.GetCount();
```

---

### 6. EScanEthTransactionHash (交易雜湊)
用於 `EthSendRawTransaction()` 方法

**方法:**
- `GetTransactionHash()` - 取得交易雜湊字串

**範例:**
```csharp
var response = await client.Proxy.EthSendRawTransaction("0x...");
var txHash = response.GetTransactionHash();
Console.WriteLine($"Transaction sent: {txHash}");
```

---

### 7. EScanEthCallResult (Call 結果)
用於 `EthCall()` 方法

**方法:**
- `GetResultHex()` - 取得 16 進位格式的結果

**範例:**
```csharp
var response = await client.Proxy.EthCall(
    to: "0x3c499c542cef5e3811e1192ce70d8cc03d5c3359",
    data: "0x70a08231...",  // balanceOf(address)
    tag: "latest"
);
var result = response.GetResultHex();
```

---

### 8. EScanEthCode (合約代碼)
用於 `EthGetCode()` 方法

**方法:**
- `GetCode()` - 取得合約 bytecode
- `IsContract()` - 檢查地址是否為合約

**範例:**
```csharp
var response = await client.Proxy.EthGetCode("0x3c499c542...", "latest");
if (response.IsContract())
{
    var code = response.GetCode();
    Console.WriteLine($"Contract bytecode length: {code.Length}");
}
```

---

### 9. EScanEthStorageValue (儲存值)
用於 `EthGetStorageAt()` 方法

**方法:**
- `GetValueHex()` - 取得 16 進位格式的儲存值
- `GetValue()` - 取得 BigInteger 格式的儲存值

**範例:**
```csharp
var response = await client.Proxy.EthGetStorageAt(
    address: "0x3c499c542...",
    position: "0x0",
    tag: "latest"
);
var value = response.GetValue();
Console.WriteLine($"Storage at position 0: {value}");
```

---

### 10. EScanEthGasPriceResult (Gas 價格)
用於 `EthGasPrice()` 方法

**方法:**
- `GetGasPriceHex()` - 取得 16 進位格式的 Gas 價格（Wei）
- `GetGasPrice()` - 取得 BigInteger 格式的 Gas 價格（Wei）
- `GetGasPriceGwei()` - 取得 Decimal 格式的 Gas 價格（Gwei）

**範例:**
```csharp
var response = await client.Proxy.EthGasPrice();
var priceWei = response.GetGasPrice();
var priceGwei = response.GetGasPriceGwei();
Console.WriteLine($"Gas Price: {priceGwei} Gwei ({priceWei} Wei)");
```

---

### 11. EScanEthEstimateGasResult (預估 Gas)
用於 `EthEstimateGas()` 方法

**方法:**
- `GetEstimatedGasHex()` - 取得 16 進位格式的預估 Gas
- `GetEstimatedGas()` - 取得 BigInteger 格式的預估 Gas

**範例:**
```csharp
var response = await client.Proxy.EthEstimateGas(
    to: "0x3c499c542...",
    value: "0x0",
    data: "0x..."
);
var estimatedGas = response.GetEstimatedGas();
Console.WriteLine($"Estimated Gas: {estimatedGas}");
```

---

## 完整使用範例

```csharp
public async Task DemoAllProxyMethods()
{
    var client = new EScanClient(EScanNetwork.PolygonMainNet, "YOUR_API_KEY");
    
    // 1. 取得當前區塊編號
    var currentBlock = await client.Proxy.EthBlockNumber();
    Console.WriteLine($"Current Block: {currentBlock.GetBlockNumber()}");
    
    // 2. 取得區塊詳細資訊
    var block = await client.Proxy.EthGetBlockByNumber("latest", true);
    var blockInfo = block.GetBlockInfo();
    Console.WriteLine($"Latest block has {blockInfo.Transactions?.Count} transactions");
    
    // 3. 取得交易資訊
    var tx = await client.Proxy.EthGetTransactionByHash("0x...");
    var txInfo = tx.GetTransactionInfo();
    Console.WriteLine($"Transaction from {txInfo.From} to {txInfo.To}");
    
    // 4. 取得交易收據
    var receipt = await client.Proxy.EthGetTransactionReceipt("0x...");
    var receiptInfo = receipt.GetReceiptInfo();
    bool success = receiptInfo.Status == "0x1";
    
    // 5. 檢查地址是否為合約
    var code = await client.Proxy.EthGetCode("0x...", "latest");
    if (code.IsContract())
    {
        Console.WriteLine("This is a contract address");
    }
    
    // 6. 取得當前 Gas 價格
    var gasPrice = await client.Proxy.EthGasPrice();
    Console.WriteLine($"Gas Price: {gasPrice.GetGasPriceGwei()} Gwei");
    
    // 7. 預估 Gas 使用量
    var estimateGas = await client.Proxy.EthEstimateGas(
        to: "0x...",
        data: "0x..."
    );
    Console.WriteLine($"Estimated Gas: {estimateGas.GetEstimatedGas()}");
    
    // 8. 取得地址的 nonce
    var nonce = await client.Proxy.EthGetTransactionCount("0x...", "latest");
    Console.WriteLine($"Address nonce: {nonce.GetCount()}");
}
```

## 注意事項

1. 所有 16 進位數值都以 "0x" 開頭
2. BigInteger 用於處理大數值（如 Wei 單位的金額）
3. Gas 價格通常用 Gwei 表示（1 Gwei = 10^9 Wei）
4. 交易狀態："0x1" = 成功, "0x0" = 失敗
5. Block tag 可以是：
   - 16 進位區塊編號（如 "0x4b2da5b"）
   - "latest" - 最新區塊
   - "earliest" - 最早區塊
   - "pending" - 待處理區塊
