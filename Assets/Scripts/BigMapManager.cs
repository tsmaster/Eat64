using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    public class BigMapManager
    {
        public const int CELL_SIZE = 5;
        public const int CACHE_SIZE = 50;

        List<BigTile> _bigTiles;
        List<ChunkBreaker> _chunkBreakers;
        List<KeyValuePair<int, int>> _hasCleared;

        public BigMapManager ()
        {
            _bigTiles = new List<BigTile> ();
            _chunkBreakers = new List<ChunkBreaker> ();
            _hasCleared = new List<KeyValuePair<int, int>> ();
        }

        static public BigMapManager BigMapMgrSingleton { get; set; }

        public bool GetHasCleared (int x, int y)
        {
            foreach (var loc in _hasCleared) {
                if ((loc.Key == x) && (loc.Value == y)) {
                    return true;
                }
            }
            return false;
        }

        public void SetHasCleared (int x, int y)
        {
            var kv = new KeyValuePair<int, int> (x, y);
            if (!_hasCleared.Contains (kv)) {
                _hasCleared.Add (kv);
            }

            var rad = 5;
            var radSqr = rad * rad;
            var newList = new List<KeyValuePair<int, int>> ();
            foreach (var rkv in _hasCleared) {
                var kx = rkv.Key;
                var ky = rkv.Value;
                var dx = kx - x;
                var dy = ky - y;
                var dsqr = dx * dx + dy * dy;
                if (dsqr <= radSqr) {
                    newList.Add (rkv);
                }
            }
            _hasCleared = newList;
        }

        public List<KeyValuePair<int, int>> GetHasClearedList ()
        {
            return _hasCleared;
        }

        internal bool CanMove (int mx, int my, MovementDirection direction, out Color outColor)
        {
            //Debug.LogFormat ("Checking canmove from {0} {1} in {2}", mx, my, direction);
            int nx = mx;
            int ny = my;

            switch (direction) {
            case MovementDirection.EAST:
                nx = mx + 1;
                break;
            case MovementDirection.NORTH:
                ny = my - 1;
                break;
            case MovementDirection.WEST:
                nx = mx - 1;
                break;
            case MovementDirection.SOUTH:
                ny = my + 1;
                break;
            }

            if (CrossesWall (mx, my, nx, ny)) {
                outColor = new Color (1.0f, 0.0f, 0.0f);
                return CanMoveThroughWall (mx, my, nx, ny);
            }

            CalcBase (mx, my, out int mbx, out int mby);
            BigTile t = GetBigTile (mbx, mby);

            if (t.CanMove (mx, my, direction)) {
                outColor = new Color (0, 0, 0);
                return true;
            }

            outColor = new Color (0, 1, 0);
            return false;
        }

        private bool CanMoveThroughWall (int mx, int my, int nx, int ny)
        {
            var maxX = Math.Max (mx, nx);
            var maxY = Math.Max (my, ny);

            var cb = GetChunkBreaker (maxX, maxY);

            if (mx == nx) {
                // north from mx, maxY to mx, maxY -1
                return cb.BreakNorth == mx;
            } else {
                // west from maxX, my to maxX - 1, my
                return cb.BreakWest == my;
            }
        }

        private ChunkBreaker GetChunkBreaker (int maxX, int maxY)
        {
            CalcBase (maxX, maxY, out int bx, out int by);
            foreach (var cb in _chunkBreakers) {
                if ((cb.BaseX == bx) &&
                    (cb.BaseY == by)) {
                    return cb;
                }
            }

            var ncb = new ChunkBreaker (bx, by);
            _chunkBreakers.Insert (0, ncb);

            while (_chunkBreakers.Count >= CACHE_SIZE) {
                _chunkBreakers.RemoveAt (CACHE_SIZE);
            }

            return ncb;
        }

        internal void ResetClearedList ()
        {
            _hasCleared.Clear ();
        }

        private bool CrossesWall (int mx, int my, int nx, int ny)
        {
            CalcBase (mx, my, out int mBaseX, out int mBaseY);
            CalcBase (nx, ny, out int nBaseX, out int nBaseY);

            return ((mBaseX != nBaseX) || (mBaseY != nBaseY));
        }

        int floorMod (int a, int b)
        {
            return a - Mathf.FloorToInt(b * Mathf.Floor (a / (float) b));
        }

        private void CalcBase (int mx, int my, out int mBaseX, out int mBaseY)
        {
            mBaseX = mx - floorMod(mx, CELL_SIZE);
            mBaseY = my - floorMod(my, CELL_SIZE);
        }

        BigTile GetBigTile (int baseX, int baseY)
        {
            //Debug.LogFormat ("Getting Big Tile for {0} {1}", baseX, baseY);

            foreach (var bt in _bigTiles) {
                if ((bt.BaseX == baseX) &&
                    (bt.BaseY == baseY)) {
                    //Debug.LogFormat ("Found Big tile");
                    return bt;
                }
            }

            var newTile = new BigTile (baseX, baseY);
            //Debug.LogFormat ("made new tile");

            // add to the beginning of the list
            _bigTiles.Insert (0, newTile);

            // make sure our cache doesn't get out of control
            while (_bigTiles.Count > CACHE_SIZE) {
                _bigTiles.RemoveAt (CACHE_SIZE);
                Debug.LogFormat ("Removed big tile from cache");
            }

            return newTile;
        }
    }
}
