using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI;

public static class Utility
{
    public static bool PointNearbyOtherPoint(Point point, int maxDistance, Point second)
        => Math.Pow(point.x - second.x, 2) + Math.Pow(point.y - second.y, 2) <= Math.Pow(maxDistance, 2);


}
