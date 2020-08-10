namespace BDG
{
    internal class ChunkBreaker
    {
        readonly public int BaseX;
        readonly public int BaseY;

        readonly public int BreakNorth;
        readonly public int BreakWest;

        public ChunkBreaker (int bx, int by)
        {
            BaseX = bx;
            BaseY = by;

            string s = string.Format ("CB {0} {1}", bx, by);
            var h = s.GetHashCode ();
            var r = new System.Random (h);

            var width = BigMapManager.CELL_SIZE;
            BreakNorth = bx + r.Next (width);
            BreakWest = by + r.Next (width);
        }
    }
}