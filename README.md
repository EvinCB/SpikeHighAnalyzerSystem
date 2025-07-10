# Spike High Analyzer (NinjaTrader Strategy)

This is an **ongoing project** for a custom-built NinjaTrader 8 strategy designed to detect and analyze price spikes. The project includes two core scripts:

- `SpikeHighDetector.cs` – an indicator for detecting sudden price spikes based on ATR and wick conditions.
- `SpikeHighAnalyzer.cs` – a strategy that uses the detector to log and analyze price behavior after spike events across different timeframes (1-min and 5-min).

---

## ⚙️ Project Description

Due to NinjaTrader's use of an older .NET framework, this project had to be structured in a certain way to ensure compatibility. Some techniques and patterns used here are specific to the NinjaScript environment and may differ from modern C# conventions.

This is one of my first large C# projects, so the code is heavily commented. These comments are both for learning and reference as the system evolves.

---

## 📈 What It Does

- **SpikeHighDetector** identifies potential spike highs based on:
  - A significant jump in high price over the previous bar.
  - An extended upper wick.
  - A breakout above the recent `LookbackPeriod`.

- **SpikeHighAnalyzer** uses the detector across 1-minute and 5-minute timeframes to:
  - Track spikes.
  - Monitor price behavior after spikes (for a set number of bars).
  - Log data into a `.csv` file for further study.
  - Classify events as `BreakoutUp`, `BreakoutDown`, or `Consolidation` based on movement and reentry behavior.

---

## 📂 Output

Each spike event is saved into a CSV file with the following structure:

Date,Time,Timeframe,KFactor,SignalPrice,High+XBars,Low+XBars,NetMove,BrokeAbove,BrokeBelow,Reentered,Breakout



Each row captures:
- The spike timestamp and timeframe
- The signal price
- The highest and lowest points after the spike
- Whether price broke above, below, or reentered the spike level
- The classification (`BreakoutUp`, `BreakoutDown`, or `Consolidation`)
- A dip threshold is used to avoid false breakouts from minor reentries

---

## 📊 Example Use Cases

- Calculate how often spikes result in clean breakouts vs. traps or consolidations
- Analyze net move direction and strength
- Develop risk management rules for breakout trading
- Combine with other tools (e.g. Opening Range, Supply/Demand zones) for confluence

---

## 📁 How to Use

1. Clone or download the repo to your PC.
2. Import the `.cs` files into your NinjaTrader 8 folder:
Documents\NinjaTrader 8\bin\Custom\Indicators
Documents\NinjaTrader 8\bin\Custom\Strategies\

yaml
Copy
Edit
3. Recompile the NinjaScript Editor in NinjaTrader.
4. Add the `SpikeHighAnalyzer` strategy to any chart with 1-minute data (and allow it to add the 5-minute series).
5. CSV logs will be generated at the path specified in the code (e.g., `C:\SpikeLogs\`).

---

## 📊 Visualization & Analysis

Once CSV files are created, import them into Excel or Python to:
- Chart net move distributions
- Plot frequency of breakout types
- Calculate average moves for each outcome type
- Evaluate false breakout rate (`BreakoutUp` with negative net move, etc.)

---

## 🚧 Future Goals

- Improve detection with trend filters or volatility context
- Add real-time alerts or visual chart markers
- Integrate with order management for live/automated trading
- Analyze trade performance per hour/day of week

---

## 📌 Notes

- This project is a **learning exercise** and not financial advice.
- Comments are intentionally verbose to help the author (and others) understand every step.
- Feedback and contributions are welcome.

---

## 🧠 Learn More

This is a great base project if you're:
- Learning NinjaTrader NinjaScript
- Practicing C# in a live market environment
- Exploring signal-based trading strategies
- Developing a personal trading system from the ground up
