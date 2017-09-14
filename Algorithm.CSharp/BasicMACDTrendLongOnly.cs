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
    *   MACD Trend Long Only
    *   
    *   The algorithm uses allows for a safety margin so you can reduce the noise 
    *   of the signal.
    */
    public class BasicMACDTrendLongOnly : QCAlgorithm
    {
        //Define required variables:
        int quantity = 0;
        decimal price = 0;
        decimal tolerance = 0m; //0.1% safety margin in prices to avoid bouncing.
        decimal exposure = 0.8m;
        // string symbol = "000651-SZSE";
        string symbol = "002385-SZSE";
        DateTime sampledToday = DateTime.Now;

        OrderTicket orderTicket;
        OrderTicket limitOrderTicket;
        OrderTicket stopOrderTicket;
        int limitOrderId;
        int stopOrderId;
        decimal limitPrice = 0;
        decimal stopPrice = 0;

        //Set up the EMA Class:
        ExponentialMovingAverage emaShort;
        ExponentialMovingAverage emaLong;

        //Initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            SetStartDate(2015, 01, 01);
            // SetEndDate(DateTime.Now);
            SetEndDate(2015, 12, 31);
            SetCash(1000000);
            // add security, define resolution here
            // finer resolution improves order execution
            AddData<AstriData>(symbol, Resolution.Minute);
            // AddData<AstriData>(symbol, Resolution.Hour);

            // daily ema
            // (stays the same for the whole trading day)
            emaShort = EMA(symbol, 10, Resolution.Daily);
            emaLong = EMA(symbol, 50, Resolution.Daily);

        }

        //Handle TradeBar Events: a TradeBar occurs on every time-interval
        public void OnData(AstriData data)
        {

            // live mode
            if (LiveMode)
                SetRuntimeStatistic(symbol, data.Close.ToString("C"));


            // arrival according to resolution defined in AddSecurity
            // Log(data[symbol].Time.ToString());
            // Log(emaShort.ToString());

            // Only process 1 bar per day
            if (sampledToday.Date == data.Time.Date) return;

            Log("-" + Time.ToShortDateString() + "-");
            price = Securities[symbol].Open;
            Log("Open price: " + price.ToString());
            // only take one data point per day (opening price)
            // Update time after first bar such that remaining bars are not processed
            sampledToday = data.Time;

            // data could contain a lot of securities
            // in this case data[symbol] = Securities[symbol]

            // wait until EMA's are ready:
            if (!emaShort.IsReady || !emaLong.IsReady) return;

            int holdings = Convert.ToInt32(Portfolio[symbol].Quantity);

            // no position
            if (holdings == 0)
            {

                // short above long: buy
                if ((emaShort * (1 - tolerance)) > emaLong)
                {

                    //Get fresh cash balance
                    decimal cash = Portfolio.Cash;
                    Log("-" + Time.ToShortDateString() + "-");
                    Log("Current cash: " + cash.ToString());
                    // determine buy size
                    quantity = Convert.ToInt32(cash * exposure / price);

                    // place buy order
                    orderTicket = Order(symbol, quantity);
                    Log("Buy " + quantity.ToString() + " shares of " + symbol + 
                        " at " + price.ToString());

                    // place take profit limit order
                    limitPrice = decimal.Round(price * 1.05m, 2);  // 5% take profit, rounded
                    limitOrderTicket = LimitOrder(symbol, -quantity, limitPrice);
                    limitOrderId = limitOrderTicket.OrderId;

                    // place stop loss order
                    stopPrice = decimal.Round(price * 0.90m, 2);  // 10% stop loss, rounded
                    stopOrderTicket = StopMarketOrder(symbol, -quantity, stopPrice);
                    stopOrderId = stopOrderTicket.OrderId;

                    // take profit stop loss log
                    Log("Stop loss/take profit levels at (" + stopPrice.ToString() + ","
                        + limitPrice.ToString() + ")");


                }
            }
            // position exists
            else if (holdings > 0)
            {

                // short below long: sell
                if ((emaShort * (1 + tolerance)) < emaLong)
                {
                    Log("-" + Time.ToShortDateString() + "-");
                    // cancel the existing take profit and stop loss orders
                    limitOrderTicket.Cancel();
                    stopOrderTicket.Cancel();

                    // place sell order
                    orderTicket = Order(symbol, -holdings);
                    Log("Sell " + holdings.ToString() + " shares of " + symbol + " at " + price.ToString());
                }
            }

        }

        // monitor event arrival
        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            // Log(orderEvent.ToString());

            // if order is filled
            if (orderEvent.Status.ToString() == "Filled")
            {
                Log("-" + Time.ToShortDateString() + "-");
                // Log(orderEvent.UtcTime);
                // if take profit order
                if (orderEvent.OrderId == limitOrderId)
                {
                    Log("Take profit at " + orderEvent.FillPrice);
                    // cancel outstanding stop loss order
                    stopOrderTicket.Cancel();
                    
                }
                // if stop loss order
                else if (orderEvent.OrderId == stopOrderId)
                {
                    Log("Stop loss at " + orderEvent.FillPrice);
                    // cancel outstanding take profit order
                    limitOrderTicket.Cancel();
                }
            }

        }
        
    }
}