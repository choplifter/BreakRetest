#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; 
using System.Linq;
using System.Windows.Media;
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

// HIER IST DER MAGISCHE FIX: DIE ENUMS SIND JETZT AUSSERHALB DES NAMESPACES!
public enum ZoneState { Active, Broken, Retesting, Confirmed, Invalidated }
public enum ZoneType { Support, Resistance }
public enum TriggerMode { SimplePriceAction, Engulfing, VolumeBackedEngulfing } 

namespace NinjaTrader.NinjaScript.Indicators
{
    public class TradingZone
    {
        public string Id { get; set; }
        public ZoneType Type { get; set; }
        public ZoneState State { get; set; }
        public double UpperBound { get; set; }
        public double LowerBound { get; set; }
        public int StartBar { get; set; }
        public int EndBar { get; set; }
        
        public TradingZone(string id, ZoneType type, double price, double toleranceTicks, double tickSize, int startBar)
        {
            Id = id;
            Type = type;
            State = ZoneState.Active;
            StartBar = startBar;
            EndBar = startBar;
            double buffer = toleranceTicks * tickSize;
            UpperBound = price + buffer;
            LowerBound = price - buffer;
        }
    }

    public interface ITriggerCondition
    {
        bool IsConfirmed(TradingZone zone, BreakRetest host);
    }

    public class SimplePriceActionTrigger : ITriggerCondition
    {
        public bool IsConfirmed(TradingZone zone, BreakRetest host)
        {
            if (zone.Type == ZoneType.Resistance) return host.Close[0] > host.Open[0] && host.Close[0] > zone.UpperBound;
            else return host.Close[0] < host.Open[0] && host.Close[0] < zone.LowerBound;
        }
    }

    public class EngulfingTrigger : ITriggerCondition
    {
        public bool IsConfirmed(TradingZone zone, BreakRetest host)
        {
            if (host.CurrentBar < 1) return false;

            if (zone.Type == ZoneType.Resistance)
            {
                bool isBullishEngulfing = host.Close[0] > host.Open[0] && host.Close[1] < host.Open[1] && host.Close[0] > host.Open[1] && host.Open[0] < host.Close[1];
                return isBullishEngulfing && host.Close[0] > zone.UpperBound;
            }
            else
            {
                bool isBearishEngulfing = host.Close[0] < host.Open[0] && host.Close[1] > host.Open[1] && host.Close[0] < host.Open[1] && host.Open[0] > host.Close[1];
                return isBearishEngulfing && host.Close[0] < zone.LowerBound;
            }
        }
    }

    public class VolumeBackedEngulfingTrigger : ITriggerCondition
    {
        public bool IsConfirmed(TradingZone zone, BreakRetest host)
        {
            if (host.CurrentBar < 21) return false;

            double volSum = 0;
            for (int i = 1; i <= 20; i++) volSum += host.Volume[i];
            double avgVolume = volSum / 20;

            bool hasVolumeSpike = host.Volume[0] > (avgVolume * 1.2);

            if (!hasVolumeSpike) return false; 

            if (zone.Type == ZoneType.Resistance)
            {
                bool isBullishEngulfing = host.Close[0] > host.Open[0] && host.Close[1] < host.Open[1] && host.Close[0] > host.Open[1] && host.Open[0] < host.Close[1];
                return isBullishEngulfing && host.Close[0] > zone.UpperBound;
            }
            else
            {
                bool isBearishEngulfing = host.Close[0] < host.Open[0] && host.Close[1] > host.Open[1] && host.Close[0] < host.Open[1] && host.Open[0] > host.Close[1];
                return isBearishEngulfing && host.Close[0] < zone.LowerBound;
            }
        }
    }

    // ACHTUNG: Der Klassenname muss dem Dateinamen entsprechen (z.B. BreakRetest)
    public class BreakRetest : Indicator
    {
        private List<TradingZone> activeZones;
        private int zoneCounter = 0;
        private ITriggerCondition activeTrigger;
        
        private double priorDayHigh = 0;
        private double priorDayLow = 0;

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Swing Strength", Description="Anzahl Kerzen links/rechts für Mikro-Level", Order=1, GroupName="1. Level Settings")]
        public int SwingStrength { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name="Zone Tolerance (Ticks)", Description="Dicke der Zone", Order=2, GroupName="1. Level Settings")]
        public int ToleranceTicks { get; set; }

        [NinjaScriptProperty]
        [Display(Name="Use Prior Day Levels", Description="Zeichnet automatisch Vortages-Hoch/Tief", Order=3, GroupName="1. Level Settings")]
        public bool UsePriorDayLevels { get; set; }

        [NinjaScriptProperty]
        [Display(Name="Trigger Logic", Description="Wähle die Bestätigungs-Logik", Order=1, GroupName="2. Trigger Settings")]
        public TriggerMode SelectedTrigger { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                 = "Break & Retest Indicator";
                Name                        = "BreakRetest";
                Calculate                   = Calculate.OnBarClose;
                IsOverlay                   = true;
                
                SwingStrength               = 5;
                ToleranceTicks              = 4;
                UsePriorDayLevels           = true; 
                SelectedTrigger             = TriggerMode.VolumeBackedEngulfing; 
            }
            else if (State == State.Configure)
            {
                activeZones = new List<TradingZone>();

                switch (SelectedTrigger)
                {
                    case TriggerMode.SimplePriceAction: activeTrigger = new SimplePriceActionTrigger(); break;
                    case TriggerMode.Engulfing: activeTrigger = new EngulfingTrigger(); break;
                    case TriggerMode.VolumeBackedEngulfing: activeTrigger = new VolumeBackedEngulfingTrigger(); break;
                }
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 21) return; 

            if (UsePriorDayLevels && Bars.IsFirstBarOfSession)
            {
                priorDayHigh = PriorDayOHLC().PriorHigh[0];
                priorDayLow = PriorDayOHLC().PriorLow[0];

                if (priorDayHigh > 0 && priorDayLow > 0)
                {
                    CreateZone(ZoneType.Resistance, priorDayHigh, CurrentBar, "PDH_");
                    CreateZone(ZoneType.Support, priorDayLow, CurrentBar, "PDL_");
                }
            }

            DetectSwingLevels();
            ProcessZones();
            
            activeZones.RemoveAll(z => z.State == ZoneState.Confirmed || z.State == ZoneState.Invalidated);
        }

        private void DetectSwingLevels()
        {
            int checkBar = SwingStrength; 
            bool isSwingHigh = true, isSwingLow = true;

            for (int i = 0; i <= SwingStrength * 2; i++)
            {
                if (i == checkBar) continue;
                if (High[i] >= High[checkBar]) isSwingHigh = false;
                if (Low[i] <= Low[checkBar]) isSwingLow = false;
            }

            if (isSwingHigh) CreateZone(ZoneType.Resistance, High[checkBar], CurrentBar - checkBar, "SwingRes_");
            if (isSwingLow) CreateZone(ZoneType.Support, Low[checkBar], CurrentBar - checkBar, "SwingSup_");
        }

        private void CreateZone(ZoneType type, double price, int startBarIndex, string prefix)
        {
            zoneCounter++;
            activeZones.Add(new TradingZone(prefix + zoneCounter, type, price, ToleranceTicks, TickSize, startBarIndex));
        }

        private void ProcessZones()
        {
            foreach (var zone in activeZones)
            {
                zone.EndBar = CurrentBar; 

                if (zone.Type == ZoneType.Resistance)
                {
                    switch (zone.State)
                    {
                        case ZoneState.Active:
                            if (Close[0] > zone.UpperBound) zone.State = ZoneState.Broken;
                            break;
                        case ZoneState.Broken:
                            if (Low[0] <= zone.UpperBound && Close[0] >= zone.LowerBound) zone.State = ZoneState.Retesting;
                            else if (Close[0] < zone.LowerBound) zone.State = ZoneState.Invalidated;
                            break;
                        case ZoneState.Retesting:
                            if (activeTrigger.IsConfirmed(zone, this))
                            {
                                zone.State = ZoneState.Confirmed;
                                Draw.ArrowUp(this, "Entry_" + zone.Id, true, 0, Low[0] - TickSize * 10, Brushes.Lime);
                                if(SelectedTrigger == TriggerMode.VolumeBackedEngulfing)
                                    Draw.Text(this, "Txt_" + zone.Id, "VOL", 0, Low[0] - TickSize * 20, Brushes.Lime);
                            }
                            else if (Close[0] < zone.LowerBound) zone.State = ZoneState.Invalidated;
                            break;
                    }
                }
                else if (zone.Type == ZoneType.Support)
                {
                    switch (zone.State)
                    {
                        case ZoneState.Active:
                            if (Close[0] < zone.LowerBound) zone.State = ZoneState.Broken;
                            break;
                        case ZoneState.Broken:
                            if (High[0] >= zone.LowerBound && Close[0] <= zone.UpperBound) zone.State = ZoneState.Retesting;
                            else if (Close[0] > zone.UpperBound) zone.State = ZoneState.Invalidated;
                            break;
                        case ZoneState.Retesting:
                            if (activeTrigger.IsConfirmed(zone, this))
                            {
                                zone.State = ZoneState.Confirmed;
                                Draw.ArrowDown(this, "Entry_" + zone.Id, true, 0, High[0] + TickSize * 10, Brushes.Red);
                                if(SelectedTrigger == TriggerMode.VolumeBackedEngulfing)
                                    Draw.Text(this, "Txt_" + zone.Id, "VOL", 0, High[0] + TickSize * 20, Brushes.Red);
                            }
                            else if (Close[0] > zone.UpperBound) zone.State = ZoneState.Invalidated;
                            break;
                    }
                }

                DrawZoneVisuals(zone);
            }
        }

        private void DrawZoneVisuals(TradingZone zone)
        {
            Brush areaBrush = Brushes.Transparent;
            Brush outlineBrush = Brushes.Transparent;

            switch (zone.State)
            {
                case ZoneState.Active:
                    if(zone.Id.StartsWith("PDH") || zone.Id.StartsWith("PDL"))
                        areaBrush = zone.Type == ZoneType.Resistance ? Brushes.OrangeRed : Brushes.SeaGreen;
                    else
                        areaBrush = zone.Type == ZoneType.Resistance ? Brushes.LightCoral : Brushes.LightGreen;
                    break;
                case ZoneState.Broken:
                    areaBrush = Brushes.LightSkyBlue; 
                    outlineBrush = Brushes.Blue;
                    break;
                case ZoneState.Retesting:
                    areaBrush = Brushes.Yellow; 
                    outlineBrush = Brushes.Goldenrod;
                    break;
                default: return; 
            }

            Draw.Rectangle(this, zone.Id, false, CurrentBar - zone.StartBar, zone.UpperBound, CurrentBar - zone.EndBar, zone.LowerBound, outlineBrush, areaBrush, 30);
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BreakRetest[] cacheBreakRetest;
		public BreakRetest BreakRetest(int swingStrength, int toleranceTicks, bool usePriorDayLevels, TriggerMode selectedTrigger)
		{
			return BreakRetest(Input, swingStrength, toleranceTicks, usePriorDayLevels, selectedTrigger);
		}

		public BreakRetest BreakRetest(ISeries<double> input, int swingStrength, int toleranceTicks, bool usePriorDayLevels, TriggerMode selectedTrigger)
		{
			if (cacheBreakRetest != null)
				for (int idx = 0; idx < cacheBreakRetest.Length; idx++)
					if (cacheBreakRetest[idx] != null && cacheBreakRetest[idx].SwingStrength == swingStrength && cacheBreakRetest[idx].ToleranceTicks == toleranceTicks && cacheBreakRetest[idx].UsePriorDayLevels == usePriorDayLevels && cacheBreakRetest[idx].SelectedTrigger == selectedTrigger && cacheBreakRetest[idx].EqualsInput(input))
						return cacheBreakRetest[idx];
			return CacheIndicator<BreakRetest>(new BreakRetest(){ SwingStrength = swingStrength, ToleranceTicks = toleranceTicks, UsePriorDayLevels = usePriorDayLevels, SelectedTrigger = selectedTrigger }, input, ref cacheBreakRetest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BreakRetest BreakRetest(int swingStrength, int toleranceTicks, bool usePriorDayLevels, TriggerMode selectedTrigger)
		{
			return indicator.BreakRetest(Input, swingStrength, toleranceTicks, usePriorDayLevels, selectedTrigger);
		}

		public Indicators.BreakRetest BreakRetest(ISeries<double> input , int swingStrength, int toleranceTicks, bool usePriorDayLevels, TriggerMode selectedTrigger)
		{
			return indicator.BreakRetest(input, swingStrength, toleranceTicks, usePriorDayLevels, selectedTrigger);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BreakRetest BreakRetest(int swingStrength, int toleranceTicks, bool usePriorDayLevels, TriggerMode selectedTrigger)
		{
			return indicator.BreakRetest(Input, swingStrength, toleranceTicks, usePriorDayLevels, selectedTrigger);
		}

		public Indicators.BreakRetest BreakRetest(ISeries<double> input , int swingStrength, int toleranceTicks, bool usePriorDayLevels, TriggerMode selectedTrigger)
		{
			return indicator.BreakRetest(input, swingStrength, toleranceTicks, usePriorDayLevels, selectedTrigger);
		}
	}
}

#endregion
