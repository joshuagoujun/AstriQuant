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
using System.Collections.Generic;

namespace QuantConnect.Algorithm.CSharp
{
    /*
    *   Long Only
    *   Minutely Data
	*	End of Date Reporting
    *	Equal Weighting Portfolio
    * 
    */
    public class SampleAlgorithm3 : QCAlgorithm
    {
        // define required variables
        // strategy related
        List<string> symbols = new List<string>
        {
            "000002-SZSE",
            "000060-SZSE",
            "000156-SZSE",
            "000157-SZSE"
        };
        string bar;
        DateTime sampledToday = DateTime.Now;
        decimal p = 0m;

        // position related
        int holdings = 0;
        int quantity = 0;
        decimal exposure = 0m;
        decimal closePrice = 0m;
        decimal nav = 1000000m;
        OrderTicket orderTicket;

        // indicators

        // initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            SetStartDate(2015, 01, 01);
            SetEndDate(2016, 02, 28);  // DateTime.Now
            SetCash(nav);

            p = 1/(decimal)symbols.Count;
            foreach (var symbol in symbols)
            {
                // add security
                AddData<AstriData>(symbol, Resolution.Minute, TimeZones.Shanghai, Market.SZSE, false, 1m);  // Tick, Second, Minute, Hour, Daily
                // indicators
            }
        }

        //Handle TradeBar Events: a TradeBar occurs on every time-interval
        public void OnData(AstriData data)
        {
            // live mode
            if (LiveMode)
            {
                foreach (var symbol in symbols)
                {
                    SetRuntimeStatistic(symbol, data.Close.ToString("C"));
                }
            }
            
            // the opening time of the current bar
            bar = data.Time.ToString();
            
            // get first bar for the day and report
            if (sampledToday.Date != data.Time.Date)
            {
                Log(bar + ". Current NAV: " + nav);
                
                foreach (var symbol in symbols)
                {
                    closePrice = Portfolio[symbol].Price;
                    holdings = Convert.ToInt32(Portfolio[symbol].Quantity);
                    decimal adj = nav * p / closePrice;
                    quantity = Convert.ToInt32(Math.Floor(adj / 100)) * 100 - holdings;

                    // place rebalancing order
                    if (quantity != 0) 
                    {
                        orderTicket = Order(symbol, quantity);
                        Log("Trade " + quantity + " shares of " + symbol + " at " + closePrice);
                    }
                    holdings += quantity;
                    exposure = holdings * closePrice / nav; // trade-based exposure
                    Log(bar + "." + symbol + ": Quantity: " + holdings + ", Exposure: " + exposure);
                }
            }
            sampledToday = data.Time;
        }

        // end of day reporting
        public override void OnEndOfDay()
        {
            nav = Portfolio.Cash;
            foreach (var symbol in symbols)
            {   
                // latest asset value
                nav += Portfolio[symbol].HoldingsValue;
            }
            
        }
        
    }
}
