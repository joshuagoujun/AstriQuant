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

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Basic template algorithm simply initializes the date range and cash
    /// </summary>
    public class BasicTemplateAlgorithm : QCAlgorithm
    {
        //private Symbol _spy = QuantConnect.Symbol.Create("SPY", SecurityType.Equity, Market.USA);

        //private Symbol _hsbc = QuantConnect.Symbol.Create("00001-HK", SecurityType.Equity, Market.SZSE);

        
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2015, 04, 07);  //Set Start Date
            SetEndDate(2015, 07, 06);    //Set End Date
            AddData<AstriData>("002385-SZSE", Resolution.Minute, TimeZones.Shanghai, Market.SZSE, false, 1m);
            //AddData<AstriData>("002385-SZSE", Resolution.Minute);

                       //Set Strategy Cash
            // Find more symbols here: http://quantconnect.com/data
            //AddEquity("SPY", Resolution.Second);
            //AddEquity("00001-HK", Resolution.Minute);
        }

        /// <summary>
        /// Astri Data event handler
        /// </summary>
        /// <param name="data">AstriData</param>
        public void OnData(AstriData data)
        {
            if(LiveMode)
                SetRuntimeStatistic("002385-SZSE", data.Close.ToString("C"));
            if (!Portfolio.HoldStock)
            {
                //SetHoldings(_spy, 1);
                //SetHoldings(_hsbc, 1);
                //Debug("Purchased Stock");
                Order("002385-SZSE", 101);
            }
        }

    }
}