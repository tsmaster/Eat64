using System;
namespace BDG
{
    public class QuadTree : ArrayTargetBrain
    {
        public static readonly int [,] TargetOrder = {
            {21, 20, 17, 16, 05, 04, 01, 00 },
            {22, 23, 18, 19, 06, 07, 02, 03 },
            {25, 24, 29, 28, 09, 08, 13, 12 },
            {26, 27, 30, 31, 10, 11, 14, 15 },
            {37, 36, 33, 32, 53, 52, 49, 48 },
            {38, 39, 34, 35, 54, 55, 50, 51 },
            {41, 40, 45, 44, 57, 56, 61, 60 },
            {42, 43, 46, 47, 58, 59, 62, 63 }
        };

        public QuadTree ()
        {
        }
    }
}
