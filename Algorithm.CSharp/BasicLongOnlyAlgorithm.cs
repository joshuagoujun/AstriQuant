/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Statistics;
using QuantConnect.Indicators;
using QuantConnect.Orders;

namespace QuantConnect.Algorithm.CSharp
{
    /*
    *   Long Only
    *   Minutely Data
	*	End of Date Reporting
    * 
    */
    public class BasicLongOnlyAlgorithm : QCAlgorithm
    {
        // define required variables
        // strategy related
        string symbol = "000060-SZSE";
        string bar;
        int barNumber = 0;
        int entryBar = 0;
        int exitBar = 0;
        decimal tolerance = 0m; //0.1% safety margin in prices to avoid bouncing
        DateTime sampledToday = DateTime.Now;

        // position related
        int holdings = 0;
        int quantity = 0;
        decimal exposure = 0m;
        decimal openPrice, highPrice, lowPrice, closePrice;
        decimal nav = 1000000m;
        OrderTicket orderTicket;

        // indicators
        ExponentialMovingAverage emaShort;
        ExponentialMovingAverage emaLong;

        // initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            SetStartDate(2015, 01, 01);
            SetEndDate(2016, 02, 28);  // DateTime.Now
            SetCash(nav);

            // add security
            AddData<AstriData>(symbol, Resolution.Minute);  // Tick, Second, Minute, Hour, Daily
            // EMA
            emaShort = EMA(symbol, 10, Resolution.Hour);
            emaLong = EMA(symbol, 50, Resolution.Hour);
        }

        //Handle TradeBar Events: a TradeBar occurs on every time-interval
        public void OnData(AstriData data)
        {
            // live mode
            if (LiveMode)
                SetRuntimeStatistic(symbol, data.Close.ToString("C"));
            
            // wait until EMA's are ready
            if (!emaShort.IsReady || !emaLong.IsReady) return;

            // the opening time of the current bar
            bar = data.Time.ToString();
            barNumber += 1;
            openPrice = data.Open;
            highPrice = data.High;
            lowPrice = data.Low;
            closePrice = data.Close;

            // print OHLC for each bar
            // Log(bar + "," + openPrice + "," + highPrice + "," + lowPrice + "," + closePrice + "," + emaShort + "," + emaLong);

            // get first bar for the day and report
            if (sampledToday.Date != data.Time.Date)
            {
                Log(bar + ". Current NAV: " + nav);
            }
            sampledToday = data.Time;
            
            holdings = Convert.ToInt32(Portfolio[symbol].Quantity);
            // no position, and after a 5-min interval
            if (holdings == 0 && barNumber - exitBar >= 5)
            {
                // buy if condition met
                if ((emaShort * (1 - tolerance)) > emaLong)
                {
                    entryBar = barNumber;
                    // determine buy size
                    decimal rawQuantity = nav / closePrice;
                    // ensure multiples of 100
                    quantity = Convert.ToInt32(Math.Floor(rawQuantity / 100)) * 100;
                    
                    // place buy order
                    orderTicket = Order(symbol, quantity);
                    Log("Buy " + quantity.ToString() + " shares of " + symbol + " at " + closePrice);
                    exposure = quantity * closePrice / nav; // trade-based exposure
                    Log(bar + ". Quantity: " + quantity + ", Exposure: " + exposure);
                }
            }
            // position exists, and after a 5-min interval
            else if (holdings > 0 && barNumber - entryBar >= 5)
            {
                Boolean TPSL = false;
                // sell if condition met, or TP/SL met
                if ((emaShort * (1 + tolerance)) < emaLong || TPSL)
                {
                    exitBar = barNumber;
                    // place sell order
                    orderTicket = Order(symbol, -holdings);
                    Log("Sell " + holdings.ToString() + " shares of " + symbol + " at " + closePrice);
                    Log(bar + ". Quantity: 0, Exposure: 0");
                }
            }

        }

        // end of day reporting
        public override void OnEndOfDay()
        {
            holdings = Convert.ToInt32(Portfolio[symbol].Quantity);
            // latest asset value
            nav = holdings * closePrice + Portfolio.Cash;
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