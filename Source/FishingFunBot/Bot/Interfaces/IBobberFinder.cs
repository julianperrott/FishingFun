using System.Drawing;
using System.Threading.Tasks;

namespace FishingFun
{
    public interface IBobberFinder
    {
        Task<Point> Find();

        void Reset();
    }
}