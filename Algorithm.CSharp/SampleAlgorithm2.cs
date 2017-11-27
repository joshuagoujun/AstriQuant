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
    *	Daily Constant Mix
    * 
    */
    public class SampleAlgorithm2 : QCAlgorithm
    {
        // define required variables
        // strategy related
        string symbol = "000060-SZSE";
        string bar;
        DateTime sampledToday = DateTime.Now;
        decimal constantMix = 0.8m;

        // position related
        int holdings = 0;
        int quantity = 0;
        decimal exposure = 0m;
        decimal closePrice;
        decimal nav = 1000000m;
        OrderTicket orderTicket;

        // indicators

        // initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            SetStartDate(2015, 01, 01);
            SetEndDate(2016, 02, 28);  // DateTime.Now
            SetCash(nav);

            // add security
            AddData<AstriData>(symbol, Resolution.Minute, TimeZones.Shanghai);  // Tick, Second, Minute, Hour, Daily
            // indicators
        }

        //Handle TradeBar Events: a TradeBar occurs on every time-interval
        public void OnData(AstriData data)
        {
            // live mode
            if (LiveMode)
                SetRuntimeStatistic(symbol, data.Close.ToString("C"));

            // the opening time of the current bar
            bar = data.Time.ToString();
            closePrice = data.Close;

            // get first bar for the day and rebalance
            if (sampledToday.Date != data.Time.Date)
            {
                Log(bar + ". Current NAV: " + nav);
                holdings = Convert.ToInt32(Portfolio[symbol].Quantity);
                decimal adj = nav * constantMix / closePrice;
                quantity = Convert.ToInt32(Math.Floor(adj / 100)) * 100 - holdings;
                
                if (quantity == 0) Log("No adjustment");
                // place rebalancing order
                else
                {   
                    orderTicket = Order(symbol, quantity);
                    Log("Trade " + quantity + " shares of " + symbol + " at " + closePrice);
                }
                holdings += quantity;
                exposure = holdings * closePrice / nav; // trade-based exposure
                Log(bar + ". Quantity: " + holdings + ", Exposure: " + exposure);
            }
            sampledToday = data.Time;
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