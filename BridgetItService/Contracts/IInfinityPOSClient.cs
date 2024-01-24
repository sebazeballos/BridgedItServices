﻿using BridgetItService.Models.Inifnity;
using ShopifySharp;

namespace BridgetItService.Contracts
{
    public interface IInfinityPOSClient
    {
        Task<string> GetAuthentication();
        Task<InfinityPosProducts> GetProducts(string startDate);
        Task<PutProducts> GetPutProducts(string startDate);
        Task<InfinityPosProducts> AddStockSync(InfinityPosProducts products, string startDate);
        Task PutProductInInfinity(PutProductInInfinity product);
        Task<InfinityPosProducts> AddStock(InfinityPosProducts products, string startDate);
        Task PostTransaction(Invoice invoice);
        Task<List<Invoice>> PostTransactionS();
        Task PutProductListInInfinity(PutProducts products);
        Task<PutProducts> SetProductAsFalse(string startDate);  
    }
}
