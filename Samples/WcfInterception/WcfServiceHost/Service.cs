using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace WcfInterception
{
    [ServiceContract]
    public interface IStockQuoteService
    {
        [OperationContract]
        DailyStockQuote GetQuote(string symbol);

        [OperationContract]
        bool PurchaseStock(string symbol, int quantity);
    }

    class Service : IStockQuoteService
    {
        Random _random = new Random();

        public DailyStockQuote GetQuote(string symbol)
        {
            int sleep = _random.Next(5, 10);
            Thread.Sleep(sleep); 
            
            DailyStockQuote quote;

            if (string.Compare(symbol, "msft", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                quote = new DailyStockQuote(symbol, 25.5, 100.1, 25.5, 99.9, 1234567);
            }
            else
            {
                quote = new DailyStockQuote(symbol, 10.5, 20.1, 5.5, 9.9, 1234);
            }

            Console.WriteLine("GetQuote({0}) -> {1}", symbol, quote);
            return quote;
        }

        public bool PurchaseStock(string symbol, int quantity)
        {
            Console.WriteLine("PurchaseStock(symbol={0},quantity={1})", symbol, quantity);

            int sleep = _random.Next(20, 30);
            Thread.Sleep(sleep); 

            if (string.Compare(symbol, "msft", StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [DataContract]
    public class DailyStockQuote
    {
        string symbol;
        double open;
        double high;
        double low;
        double close;
        int volume;

        public DailyStockQuote(string symbol, double open, double high, double low, double close, int volume)
        {
            this.symbol = symbol;
            this.open = open;
            this.high = high;
            this.low = low;
            this.close = close;
            this.volume = volume;
        }

        [DataMember]
        public string Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        [DataMember]
        public double Open
        {
            get { return open; }
            set { open = value; }
        }

        [DataMember]
        public double High
        {
            get { return high; }
            set { high = value; }
        }

        [DataMember]
        public double Low
        {
            get { return low; }
            set { low = value; }
        }

        [DataMember]
        public double Close
        {
            get { return close; }
            set { close = value; }
        }

        [DataMember]
        public int Volume
        {
            get { return volume; }
            set { volume = value; }
        }
    }
}
