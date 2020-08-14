using System;
namespace BDG
{
    public class Clocky : ArrayTargetBrain
    {
        private readonly int [,] _targetOrder = {
            {24, 25, 26, 27, 00, 01, 02, 03 },
            {23, -1, -1, -1, -1, -1, -1, 04 },
            {22, -1, -1, -1, -1, -1, -1, 05 },
            {21, -1, -1, -1, -1, -1, -1, 06 },
            {20, -1, -1, -1, -1, -1, -1, 07 },
            {19, -1, -1, -1, -1, -1, -1, 08 },
            {18, -1, -1, -1, -1, -1, -1, 09 },
            {17, 16, 15, 14, 13, 12, 11, 10 }
        };

        public Clocky ()
        {
        }
    }
}
