using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace snake
{
    public class Utils
    {
        public static int getRandomValue(float point1, float point2)
        {
            return (int)UnityEngine.Random.Range(point1, point2);
        }
    }
}
