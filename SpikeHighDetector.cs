#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
#region Using declarations
using System;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class SpikeHighDetector : Indicator
    {
        // Parameters
        private double k = 0.75;   // Multiplicative factor
        private int atrPeriod = 10; // ATR period
        private int lookbackPeriod = 50; // Lookback for significant highs

        private Series<double> atrSeries;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Spike High Detector";
                Name = "SpikeHighDetector";
                IsOverlay = true; // Show marker on the price chart
                AddPlot(Brushes.Red, "SpikeHigh");
            }
            else if (State == State.DataLoaded)
            {
                atrSeries = new Series<double>(this);
            }
        }

       protected override void OnBarUpdate()
       {
          // Ensure enough bars are available for calculations
          if (CurrentBar < Math.Max(atrPeriod + 1, lookbackPeriod + 1))
              return;

          // Calculate ATR
          double trueRange = Math.Max(High[0] - Low[0], Math.Max(Math.Abs(High[0] - Close[1]), Math.Abs(Close[1] - Low[0])));
          atrSeries[0] = ((atrPeriod - 1) * atrSeries[1] + trueRange) / atrPeriod;

          // Condition 1: Wide difference between the current high and the preceding high
          bool condition1 = (High[0] - High[1]) > k * atrSeries[0];

         // Condition 2: Large upper wick relative to the day's range
         bool condition2 = (High[0] - Close[0]) > 3 * (Close[0] - Low[0]);

        // Condition 3: Current high is greater than the max high of the past N days
        bool condition3 = High[0] > MAX(High, lookbackPeriod)[1];

        // Check all conditions
        if (condition1 && condition2 && condition3)
            {
               // Mark the spike high with a plot or visual element
               Values[0][0] = High[0];

              // Draw a marker on the chart
              Draw.ArrowDown(this, "SpikeHigh" + CurrentBar, false, 0, High[0] + TickSize, Brushes.Red);
           }
           else
             {
                // No spike high, clear the value
                Values[0][0] = double.NaN;
             }
       }

        #region Properties
        [Range(0.1, 5), NinjaScriptProperty]
        [Display(Name = "K Factor", Order = 1, GroupName = "Parameters")]
        public double K
        {
            get { return k; }
            set { k = value; }
        }

        [Range(5, 50), NinjaScriptProperty]
        [Display(Name = "ATR Period", Order = 2, GroupName = "Parameters")]
        public int ATRPeriod
        {
            get { return atrPeriod; }
            set { atrPeriod = value; }
        }

        [Range(10, 100), NinjaScriptProperty]
        [Display(Name = "Lookback Period", Order = 3, GroupName = "Parameters")]
        public int LookbackPeriod
        {
            get { return lookbackPeriod; }
            set { lookbackPeriod = value; }
        }
        #endregion
    }
	
	
	
 
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SpikeHighDetector[] cacheSpikeHighDetector;
		public SpikeHighDetector SpikeHighDetector(double k, int aTRPeriod, int lookbackPeriod)
		{
			return SpikeHighDetector(Input, k, aTRPeriod, lookbackPeriod);
		}

		public SpikeHighDetector SpikeHighDetector(ISeries<double> input, double k, int aTRPeriod, int lookbackPeriod)
		{
			if (cacheSpikeHighDetector != null)
				for (int idx = 0; idx < cacheSpikeHighDetector.Length; idx++)
					if (cacheSpikeHighDetector[idx] != null && cacheSpikeHighDetector[idx].K == k && cacheSpikeHighDetector[idx].ATRPeriod == aTRPeriod && cacheSpikeHighDetector[idx].LookbackPeriod == lookbackPeriod && cacheSpikeHighDetector[idx].EqualsInput(input))
						return cacheSpikeHighDetector[idx];
			return CacheIndicator<SpikeHighDetector>(new SpikeHighDetector(){ K = k, ATRPeriod = aTRPeriod, LookbackPeriod = lookbackPeriod }, input, ref cacheSpikeHighDetector);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SpikeHighDetector SpikeHighDetector(double k, int aTRPeriod, int lookbackPeriod)
		{
			return indicator.SpikeHighDetector(Input, k, aTRPeriod, lookbackPeriod);
		}

		public Indicators.SpikeHighDetector SpikeHighDetector(ISeries<double> input , double k, int aTRPeriod, int lookbackPeriod)
		{
			return indicator.SpikeHighDetector(input, k, aTRPeriod, lookbackPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SpikeHighDetector SpikeHighDetector(double k, int aTRPeriod, int lookbackPeriod)
		{
			return indicator.SpikeHighDetector(Input, k, aTRPeriod, lookbackPeriod);
		}

		public Indicators.SpikeHighDetector SpikeHighDetector(ISeries<double> input , double k, int aTRPeriod, int lookbackPeriod)
		{
			return indicator.SpikeHighDetector(input, k, aTRPeriod, lookbackPeriod);
		}
	}
}

#endregion
