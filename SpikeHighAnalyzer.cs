using System;
using System.Collections.Generic;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators;
using System.IO;


namespace NinjaTrader.NinjaScript.Strategies
{
	public class SpikeHighAnalyzer : Strategy
	{
		
		private SpikeHighDetector spike1Min; //stores the 1-min spike dector instance
		private SpikeHighDetector spike5Min; //stores the 5-min spike dector instance
		
		
		private string outputFile1Min = "SpikeResults_1Min.csv"; //names the CSV files where the results will be stored
		private string outputFile5Min = "SpikeResults_5Min.csv"; //names the CSV files where the results will be stored
		
		private List<SpikeEvent> activeSpikes1Min = new List<SpikeEvent>(); // both of these are a temporary memory list to hold every spike until 1 hour of data has passed
		private List<SpikeEvent> activeSpikes5Min = new List<SpikeEvent>();
		
		[NinjaScriptProperty]
       
        public double KFactor1Min { get; set; }
		
		[NinjaScriptProperty]
       
        public double KFactor5Min { get; set; }
		
		
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{	
				Name = "SpikeHighAnalyzer";
			    Calculate = Calculate.OnBarClose;
			    IsOverlay = false;
				KFactor1Min = 0.75;
                KFactor5Min = 1.25;
			    
			}
			
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, 5); //adds a secondary 5-min chart to run the 5min strat
			}
			
		
		    //can intialize things like indicators here since the bars are loaded on the chart and we have access to the timeframes.
		   else if (State == State.DataLoaded)
		   {
			   // first 25# is the lookback period (how many bars to look back) second 25# is the ATR value.
			spike1Min = SpikeHighDetector(KFactor1Min, 25, 25);
            spike5Min = SpikeHighDetector(Closes[1], KFactor5Min, 25, 25);
			   
			   // Define file paths
		       outputFile1Min = @"C:\SpikeLogs\SpikeResults_1Min.csv";
		       outputFile5Min = @"C:\SpikeLogs\SpikeResults_5Min.csv";
			   
		   }
		   else if (State == State.Historical || State == State.Transition || State == State.Realtime)
		   {
			   File.WriteAllText(outputFile1Min, "Date,Time,Timeframe,KFactor,SignalPrice,High+15Min,Low+15Min,NetMove,BrokeAbove,BrokeBelow,Reentered,Breakout\n"); //creates a CSV file with the format
               File.WriteAllText(outputFile5Min, "Date,Time,Timeframe,KFactor,SignalPrice,High+15Min,Low+15Min,NetMove,BrokeAbove,BrokeBelow,Reentered,Breakout\n");
			   
			   // Add confirmation prints
		       Print("SpikeHighAnalyzer - CSV headers written to:");
		       Print(outputFile1Min);
		       Print(outputFile5Min);
			   
		   }
		   else if (State == State.Terminated)
	       {
		      // Optional final write or summary log
		      Print("SpikeHighAnalyzer - Strategy terminated. Final output written.");
		      Print("Check files here:");
		      Print(outputFile1Min);
		      Print(outputFile5Min);
	      }
		
	   }
		
		
		
		
		// This method is called automatically every time a bar closes (1-min or 5-min)
        // Because we set Calculate = Calculate.OnBarClose, this only runs once per bar, not on every tick
	protected override void OnBarUpdate()
    {
          // Print for confirmation that the strategy is running
         Print(Time[0] + " - SpikeHighAnalyzer is running on bar: " + CurrentBar + " | BIP: " + BarsInProgress);

         // Skip if we're still warming up (e.g., first 25 bars)
         if (CurrentBar < 25)
              return;

         // 1-Minute Series
         if (BarsInProgress == 0)
           {
               HandleSpikeLogic(0, spike1Min, activeSpikes1Min, outputFile1Min, "1Min", KFactor1Min);
           }

        // 5-Minute Series
        if (BarsInProgress == 1)
          {
             HandleSpikeLogic(1, spike5Min, activeSpikes5Min, outputFile5Min, "5Min", KFactor5Min);
          }
    }
		
		//bip stands for Bars in Progress, which tells us which chart timeframe this is.
		
		private void HandleSpikeLogic (int bip, SpikeHighDetector detector, List<SpikeEvent> activeSpikes, string logFile, String timeframe, double kFactor)
		{
			//this makes sure we have at least one bar loaded for this data series and if the bar is outside the hours specified 9:30 - 4:30 then it will stop processing
			if (CurrentBars[bip] < 1 || Times[bip][0].TimeOfDay < new TimeSpan ( 9, 0, 0 ) || Times[bip][0].TimeOfDay > new TimeSpan ( 16, 30, 0)) 
				
				return;
			
			double currentPrice = Closes[bip][0];     //the most recent close price on the 1 or 5 min chart
			DateTime signalTime = Times[bip][0]; //the timestamp of that bar
			
			
			Print("Spike detector raw value at " + Times[bip][0] + ": " + detector[0]); //test

			
			if (!double.IsNaN(detector[0])) 
			{
				//when the value returns 0 this means a spike just happend creates a new spike event to add to the list of active spikes. this checks each chart so 60 bars for the 1 min and 12 bars for the 5 min to equal an hour
				Print(timeframe + " spike detected at " + Times[bip][0] + " with price " + currentPrice);
				int duration = (bip == 0) ? 15 : 3;
                activeSpikes.Add(new SpikeEvent(signalTime, currentPrice, duration));
				
			}
			
			//this for loop goes backwards through the list to see if the spike is complete meaning 1 hour of bars have passed. once complete it logs then removes it
			for (int i = activeSpikes.Count - 1; i >= 0; i--)
			{
				//this updates the event with the latest price info and tracks how much time has passed. What the high / low is and if the price came back through the spike zone
				SpikeEvent se = activeSpikes[i];
				se.Update(Closes[bip][0], Highs[bip][0], Lows[bip][0], Times[bip][0]);
				
				//if the spike is now 1hour old then tracking is done. writes the results and removes it from the active list.
				if (se.IsComplete)
				{
					string row = se.ToCsvRow(timeframe, kFactor);				
					File.AppendAllText(logFile, row);
					Print("CSV ROW LOGGED: " + row);
					activeSpikes.RemoveAt(i);
					
				}

				
			}
			
			
		}
		
		private class SpikeEvent
		{
			 private int DurationBars;

            private DateTime _time;
            private double _signalPrice;

            public DateTime Time { get { return _time; } }
            public double SignalPrice { get { return _signalPrice; } }

             private double High = double.MinValue;
             private double Low = double.MaxValue;
             private int BarsPassed = 0;

             private bool CrossedAbove = false;
             private bool CrossedBelow = false;
             private bool ReenteredZone = false;
			
			double dipThreshold = 10.00;
			
			
			public SpikeEvent (DateTime time, double price, int durationBars)
			{
				
				 _time = time;
                 _signalPrice = price;
                 DurationBars = durationBars;
				
			}
			
			public void Update(double close, double high, double low, DateTime currentTime)
			{
				//this keeps the highest and lowest value seen since the spike.
				High = Math.Max(High, high);
				Low = Math.Min (Low, low);
				
				if (high > SignalPrice) CrossedAbove = true;
				if (low < SignalPrice) CrossedBelow = true;
				if (CrossedAbove && CrossedBelow) ReenteredZone = true;
				
				BarsPassed++;
				
			}
			
			//checks to see if the spike is done being tracked
			public bool IsComplete
             {
	            get { return BarsPassed >= DurationBars; }
             }
			
			
			//formats the spike even into a line of text to write into the .cvs file
			public string ToCsvRow(string timeframe, double kFactor)
			{
				
				double netMove = High - SignalPrice > SignalPrice - Low ? High - SignalPrice : -(SignalPrice - Low);
				string breakout =
                    (CrossedAbove && Low >= SignalPrice - dipThreshold) ? "BreakoutUp" :
                    (CrossedBelow && High <= SignalPrice + dipThreshold) ? "BreakoutDown" :
                    "Consolidation";
				
				return string.Format("{0:yyyy-MM-dd},{1:HH:mm},{2},{3},{4:F2},{5:F2},{6:F2},{7:F2},{8},{9},{10},{11}\n",
                      Time, Time, timeframe, kFactor, SignalPrice, High, Low, netMove,
                             CrossedAbove, CrossedBelow, ReenteredZone, breakout);
			}
			
			
			
			
		}
		
		

   }
  
}
	