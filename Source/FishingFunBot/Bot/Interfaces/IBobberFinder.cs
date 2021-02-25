using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace FishingFun
{
    public interface IBobberFinder
    {
        Task<Point> FindAsync(CancellationToken cancellationToken);

        void Reset();
    }
}