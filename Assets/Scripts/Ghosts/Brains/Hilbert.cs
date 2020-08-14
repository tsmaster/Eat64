using System;
namespace BDG
{
    public class Hilbert : ArrayTargetBrain
    {
        private readonly int [,] _targetOrder = {
            {21, 20, 19, 16, 15, 14, 01, 00 },
            {22, 23, 18, 17, 12, 13, 02, 03 },
            {25, 24, 29, 30, 11, 08, 07, 04 },
            {26, 27, 28, 31, 10, 09, 06, 05 },
            {37, 36, 35, 32, 53, 54, 57, 58 },
            {38, 39, 34, 33, 52, 55, 56, 59 },
            {41, 40, 45, 46, 51, 50, 61, 60 },
            {42, 43, 44, 47, 48, 49, 62, 63 },
        };

        public Hilbert ()
        {
        }
    }
}
