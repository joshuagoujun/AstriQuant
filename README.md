# AstriQuant
Astri Quant desktop

## Introduction ##

Astri Quant desktop is based off of the [Lean Trading Engine](https://github.com/QuantConnect/Lean) open sourced by [QuantConnect](https://www.quantconnect.com/). The targeted markets are A stock in shenzhen and shanghai exchanges at this stage. Quite a few enhancements have been applied according to contest rules stated in the [China(Hengqin) International University Quantitative Finance Competition](http://social.sz.tsinghua.edu.cn/hq/home.html)

## Spinup Instructions ##

### Windows

- Install [Visual Studio](https://www.visualstudio.com/downloads/)
- Open **Algorithm.CSharp.sln** in Visual Studio.
- Add a new class into the **QuantConnect.Algorithm.CSharp** project, with the naming convention \*\*\*Algorithm.cs.
- This new class needs to inherit class **QCAlgorithm** and wherein we can craft a trading strategy.
- Override the **Initialize()**, to set up backtesting start and end date, subscribe datafeed for a particular symbol, etc.
- Add a method **OnData()** with an **AstriData** object as input parameter.
- The property **LiveMode** can tell whether we are in the backtesting mode or live trade otherwise.
- Refer to **BasicTemplateAlgorithm.cs** and **BasicMACDTrendLongOnly.cs** for further details.
- Once your strategy is ready, build the **QuantConnect.Algorithm.CSharp** project and make sure process completed without any error.
- Open Launcher\bin\Debug\config.json with any editor you prefer, update the key **algorithm-type-name** to the class name you just added, and the key **environment** to **backtesting-desktop** if you would like to perform backtesting, or **live-desktop** for live trade.
- Launch **Launcher\bin\Debug\QuantConnect.Lean.Launcher.exe**
