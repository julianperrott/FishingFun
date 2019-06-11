using System;
using System.Drawing;

namespace FishingFun
{
    public interface IBiteWatcher
    {
        void Reset(Point InitialBobberPosition);

        bool IsBite(Point currentBobberPosition);

        Action<FishingEvent> FishingEventHandler { set; get; }
    }
}