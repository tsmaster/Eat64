using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    public class BigTile
    {
        readonly private BigTileCell [,] _bigTileCells;

        const int TILE_WIDTH = BigMapManager.CELL_SIZE;

        private string[] _adjList = {
            "able",
            "blue",
            "canny",
            "deft",
            "early",
            "fast",
            "gry",
            "happy",
            "idle",
            "just",
            "kwik",
            "last",
            "many",
            "none",
            "often",
            "pure",
            "qwerty",
            "racer",
            "swift",
            "true",
            "unique",
            "very",
            "wily",
            "xoom",
            "yeti",
            "zany" };

        private string [] _nounList = {
            "ant",
            "bear",
            "cat",
            "dog",
            "eagle",
            "fox",
            "gull",
            "hippo",
            "ibex",
            "jam",
            "kelt",
            "llama",
            "mouse",
            "nutria",
            "otter",
            "penguin",
            "quail",
            "rat",
            "sorrel",
            "tarn",
            "ubit",
            "vole",
            "xarn",
            "yak",
            "zebra" };



        public int BaseX { get; internal set; }
        public int BaseY { get; internal set; }

        public string Name { get; set; }

        private string _fourcc;
        private string _adj;
        private string _noun;

        public BigTile (int baseX, int baseY)
        {
            BaseX = baseX;
            BaseY = baseY;

            _bigTileCells = new BigTileCell [TILE_WIDTH, TILE_WIDTH];

            for (int x = 0; x < TILE_WIDTH; ++x) {
                for (int y = 0; y < TILE_WIDTH; ++y) {
                    var c = new BigTileCell ();
                    _bigTileCells [y, x] = c;
                }
            }

            Name = string.Format ("Cell {0} {1}", baseX, baseY);
            var h = Name.GetHashCode ();
            var r = new System.Random (h);

            _fourcc = Make4CC (r);

            var adj = MakeAdj (r);
            var noun = MakeNoun (r);

            Debug.LogFormat ("Made BigTile {0} {1} {2} {3}", Name, _fourcc, adj, noun);

            DoPrim (r);
        }

        private void DoPrim (System.Random r)
        {
            List<KeyValuePair<int, int>> availableLocations = new List<KeyValuePair<int, int>> ();
            List<KeyValuePair<int, int>> visitedLocations = new List<KeyValuePair<int, int>> ();

            for (int x = BaseX; x < BaseX + TILE_WIDTH; ++x) {
                for (int y = BaseY; y < BaseY + TILE_WIDTH; ++y) {
                    availableLocations.Add (new KeyValuePair<int, int> (x, y));
                }
            }

            Shuffle (r, availableLocations);

            var startLoc = availableLocations [0];
            availableLocations.RemoveAt (0);
            visitedLocations.Add (startLoc);

            //Debug.LogFormat ("Starting at {0} {1}", startLoc.Key, startLoc.Value);

            List<MovementDirection> moveDirs = new List<MovementDirection>{
                MovementDirection.EAST,
                MovementDirection.NORTH,
                MovementDirection.WEST,
                MovementDirection.SOUTH };

            int dbgBailout = 1000;

            while (availableLocations.Count > 0) {
                if (dbgBailout <= 0) {
                    Debug.LogError ("Bailing out");
                    return;
                }
                dbgBailout--;

                var loc = availableLocations [0];
                availableLocations.RemoveAt (0);

                List<MovementDirection> dToVisited = new List<MovementDirection> ();
                foreach (var d in moveDirs) {
                    CalcNeighborPos (d, loc.Key, loc.Value, out int nx1, out int ny1);
                    var nLoc = new KeyValuePair<int, int> (nx1, ny1);
                    if (visitedLocations.Contains (nLoc)) {
                        dToVisited.Add (d);
                    }
                }

                if (dToVisited.Count == 0) {
                    availableLocations.Add (loc);
                    continue;
                }

                var selectedDir = dToVisited [r.Next (dToVisited.Count)];

                CalcNeighborPos (selectedDir, loc.Key, loc.Value, out int nx, out int ny);

                var myCell = GetCellAt (loc.Key- BaseX, loc.Value - BaseY);
                var nCell = GetCellAt (nx - BaseX, ny - BaseY);

                //Debug.LogFormat ("Linking {0} {1} in {4} to {2} {3}", loc.Key, loc.Value, nx, ny, selectedDir);

                myCell.SetCanMove (selectedDir, true);
                nCell.SetCanMove (Character.OppositeMoveDirection (selectedDir), true);
                visitedLocations.Add (loc);
                //Debug.LogFormat ("added visited {0} {1}", loc.Key, loc.Value);
            }
        }

        private void CalcNeighborPos (MovementDirection d, int x, int y, out int nx, out int ny)
        {
            switch (d) {
            case MovementDirection.EAST:
                nx = x + 1;
                ny = y;
                break;
            case MovementDirection.NORTH:
                nx = x;
                ny = y - 1;
                break;
            case MovementDirection.WEST:
                nx = x - 1;
                ny = y;
                break;
            case MovementDirection.SOUTH:
                nx = x;
                ny = y + 1;
                break;
            default:
                nx = -1;
                ny = -1;
                break;
            }
        }

        private void Shuffle<T> (System.Random r, List<T> tList)
        {
            for (int i = 0; i < tList.Count; ++i) {
                T tmpVal = tList [i];
                var randIdx = r.Next (tList.Count);
                tList [i] = tList [randIdx];
                tList [randIdx] = tmpVal;
            }
        }

        string Make4CC (System.Random r)
        {
            string s = "";

            for (int i = 0; i < 4; ++i) {
                int d = r.Next (65, 91);
                char c = (char)d;
                s += string.Format ("{0}", c);
            }

            return s;
        }

        string MakeAdj (System.Random r)
        {
            var idx = r.Next (_adjList.Length);
            return _adjList [idx];
        }

        string MakeNoun (System.Random r)
        {
            var idx = r.Next (_nounList.Length);
            return _nounList [idx];
        }

        public bool CanMove (int mx, int my, MovementDirection direction)
        {
            var tx = mx - BaseX;
            var ty = my - BaseY;

            var tCell = GetCellAt (tx, ty);
            if (tCell == null) {
                Debug.LogErrorFormat ("CanMove {0} {1} {2} found no cell", mx, my, direction);
            }
            return tCell.CanMove (direction);
        }

        /// <summary>
        /// Gets the cell at tx and ty.
        /// </summary>
        /// <returns>The <see cref="!:BigTileCell"/>.</returns>
        /// <param name="tx">Tx. positive to the right</param>
        /// <param name="ty">Ty. positive down</param>
        BigTileCell GetCellAt (int tx, int ty)
        {
            if ((tx < 0) || (tx >= TILE_WIDTH) ||
                (ty < 0) || (ty >= TILE_WIDTH)) {
                Debug.LogErrorFormat ("GetCellAt {0} {1}", tx, ty);
                return null;
            }
            return _bigTileCells [ty, tx];
        }
    }
}
