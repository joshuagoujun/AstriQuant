using System;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Statistics;
using QuantConnect.Indicators;
using QuantConnect.Orders;

namespace QuantConnect.Algorithm.CSharp
{
    public class SampleAlgorithm5 : QCAlgorithm
    {
        string symbol = "000060-SZSE";
        string bar;
        int barNumber = 0;
        int entryBar = 0;
        int exitBar = 0;
        decimal tolerance = 0.1m; //0.1% safety margin in prices to avoid bouncing
        DateTime sampledToday = DateTime.Now;
        decimal TPPrice = 0;
        decimal SLPrice = 0;

        // position related
        int holdings = 0;
        int quantity = 0;
        decimal exposure = 0m;
        decimal openPrice, highPrice, lowPrice, closePrice;
        decimal nav = 10000000m;
        OrderTicket orderTicket;

        // indicators
        RelativeStrengthIndex rsi;
        MovingAverageConvergenceDivergence macd;

        // initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            SetStartDate(2015, 01, 01);
            SetEndDate(2015, 04, 28);  // DateTime.Now
            SetCash(nav);

            // add security
            AddData<AstriData>(symbol, Resolution.Minute, TimeZones.Shanghai, Market.SZSE, false, 1m);  // Tick, Second, Minute, Hour, Daily
            // indicators
            //rsi = RSI(symbol, 14, MovingAverageType.Wilders, Resolution.Hour);
            //macd = MACD(symbol, 12, 26, 9, MovingAverageType.Exponential, Resolution.Hour);

            rsi = RSI(symbol, 1, MovingAverageType.Wilders, Resolution.Hour);
            macd = MACD(symbol, 1, 1, 1, MovingAverageType.Exponential, Resolution.Hour);
        }

        //Handle TradeBar Events: a TradeBar occurs on every time-interval
        public void OnData(AstriData data)
        {
            // live mode
            //if (LiveMode)
            //    SetRuntimeStatistic(symbol, data.Close.ToString("C"));

            nav = Portfolio[symbol].HoldingsValue + Portfolio.Cash;
            // wait until EMA's are ready
            if (!rsi.IsReady || !macd.IsReady) return;

            // the opening time of the current bar
            bar = data.Time.ToString();
            barNumber += 1;
            openPrice = data.Open;
            highPrice = data.High;
            lowPrice = data.Low;
            closePrice = data.Close;

            // print OHLC for each bar
            // Log(bar + "," + openPrice + "," + highPrice + "," + lowPrice + "," + closePrice + "," + rsi);

            // get first bar for the day and report
            if (sampledToday.Date != data.Time.Date) Log(bar + ". Current NAV: " + nav);
            sampledToday = data.Time;

            holdings = Convert.ToInt32(Portfolio[symbol].Quantity);
            // no position, and after a 5-min interval
            if (holdings == 0 && barNumber - entryBar >= 60)
            {
                // buy if condition met
                //if (rsi > 50 && rsi < 75 && macd > tolerance)
                if (true)
                {
                    entryBar = barNumber;
                    // determine buy size
                    decimal rawQuantity = nav / closePrice;
                    // ensure multiples of 100
                    quantity = Convert.ToInt32(Math.Floor(rawQuantity / 100)) * 100;

                    // place buy order
                    orderTicket = Order(symbol, quantity);
                    Log("Buy " + quantity + " shares of " + symbol + " at " + closePrice);
                    exposure = quantity * closePrice / nav; // trade-based exposure
                    Log(bar + ". Quantity: " + quantity + ", Exposure: " + exposure);

                    // set take profit and stop loss levels
                    TPPrice = closePrice * 1.10m;
                    SLPrice = closePrice * 0.90m;
                    Log("Stop loss/take profit levels at (" + SLPrice + "," + TPPrice + ")");
                }
            }
            // position exists, and after a 5-min interval
            else if (holdings > 0 && barNumber - entryBar >= 60)
            {
                Boolean TPSL = highPrice >= TPPrice || lowPrice <= SLPrice;
                // sell if condition met, or TP/SL met
                //if ((rsi > 85 && macd < -tolerance) || TPSL)
                if (true)
                {
                    entryBar = barNumber;
                    // place sell order
                    orderTicket = Order(symbol, -holdings);
                    Log("Sell " + holdings + " shares of " + symbol + " at " + closePrice);
                    Log(bar + ". Quantity: 0, Exposure: 0");
                }
            }
        }

        // end of day reporting
        public override void OnEndOfDay()
        {
            // latest asset value
            nav = Portfolio[symbol].HoldingsValue + Portfolio.Cash;
        }

        // monitor event arrival
        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            // Log(orderEvent.ToString());
            // if order is filled
            if (orderEvent.Status.ToString() == "Filled")
            {
                // MKT order filled at closing price
                // Log("Filled at " + orderEvent.FillPrice);
            }

        }
    }
}
