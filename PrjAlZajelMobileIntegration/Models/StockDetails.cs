using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrjAlZajelMobileIntegration.Models
{
    public class StockDetails
    {
        public int iTransactionId { get; set; }
    }

    public class Datum
    {
        public string CompanyCode { get; set; }
        public string Identifier { get; set; }
        public string Secret { get; set; }
        public string Lng { get; set; }
    }

    public class Resultlogin
    {
        public string AccessToken { get; set; }
        public bool IsSuccess { get; set; }
        public string StatusMsg { get; set; }
    }

    public class StockList
    {
        public string AccessToken { get; set; }
        public string ObjectType { get; set; }
        public List<StockDetailsData> ItemList { get; set; }
    }

    public class StockDetailsData
    {
        public string SourceStockCode { get; set; }
        public string DestinationStockCode { get; set; }
        public int ProductId { get; set; }
        public string ProductUnit { get; set; }
        public string BatchId { get; set; }
        public string ExpiryDate { get; set; }
        public decimal Qty { get; set; }
        public string TransactionDateTime { get; set; }
        public string Comments { get; set; }
        public int Type { get; set; }
    }

    public class Result
    {
        public ResponseStatus ResponseStatus { get; set; }
        public List<string> ErrorMessages { get; set; }
        public List<FailedList> FailedList { get; set; }
    }
    public class ResponseStatus
    {
        public bool IsSuccess { get; set; }
        public string StatusMsg { get; set; }
        public string ErrorCode { get; set; }
    }
    public class FailedList
    {
        public string SourceStockCode { get; set; }
        public string DestinationStockCode { get; set; }
        public int ProductId { get; set; }
        public string ProductUnit { get; set; }
        public string BatchId { get; set; }
        public string ExpiryDate { get; set; }
        public decimal Qty { get; set; }
        public string TransactionDateTime { get; set; }
        public string Comments { get; set; }
        public int Type { get; set; }
    }

}