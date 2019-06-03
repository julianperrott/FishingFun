using System.Drawing;

namespace FishingFun
{
    public interface IBobberFinder
    {
        Point Find();

        void Reset();
    }
}