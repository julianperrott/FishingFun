using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FishingFun
{
    public class PositionBiteWatcher : IBiteWatcher
    {
        private static ILog logger = LogManager.GetLogger("Fishbot");

        private List<int> yPositions = new List<int>();
        private int strikeValue;
        private int yDiff;
        private TimedAction timer;

        public event EventHandler<FishingEvent> FishingEventHandler;

        public PositionBiteWatcher(int strikeValue)
        {
            this.strikeValue = strikeValue;
        }

        public void Reset(Point InitialBobberPosition)
        {
            FishingEventHandler?.Invoke(this, new FishingEvent { Action = FishingAction.Reset });

            yPositions = new List<int>();
            yPositions.Add(InitialBobberPosition.Y);
            timer = new TimedAction((a) =>
            {
                FishingEventHandler?.Invoke(this, new FishingEvent { Amplitude = yDiff, Action = FishingAction.BobberMove });
            }, 500, 25);
        }

        public bool IsBite(Point currentBobberPosition)
        {
            if (!yPositions.Contains(currentBobberPosition.Y))
            {
                yPositions.Add(currentBobberPosition.Y);
                yPositions.Sort();
            }

            yDiff = yPositions[(int)((((double)yPositions.Count) + 0.5) / 2)] - currentBobberPosition.Y;

            bool thresholdReached = yDiff <= -strikeValue;

            timer.ExecuteIfDue();

            if (thresholdReached)
            {
                FishingEventHandler?.Invoke(this, new FishingEvent { Action = FishingAction.Loot });
                timer.ExecuteNow();
                return true;
            }

            return false;
        }
    }
}