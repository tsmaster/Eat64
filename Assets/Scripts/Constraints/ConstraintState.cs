using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    internal class ConstraintState
    {
        public ConstraintMap.IsWall [,] _westWalls;
        public ConstraintMap.IsWall [,] _northWalls;
        public List<int> [,] _tileSets;
        public bool [,] _isLocked;

        public ConstraintState ()
        {

        }

        public void Init ()
        {
            _westWalls = new ConstraintMap.IsWall [9, 9];
            _northWalls = new ConstraintMap.IsWall [9, 9];
            _isLocked = new bool [9, 9];
            _tileSets = new List<int> [9, 9];

            for (int x = 0; x <= 8; ++x) {
                for (int y = 0; y <= 8; ++y) {
                    _westWalls [y, x] = ConstraintMap.IsWall.MAYBE;
                    _northWalls [y, x] = ConstraintMap.IsWall.MAYBE;
                    _tileSets [y, x] = MakeAllTiles ();
                    _isLocked[y,x] = false;
                }
            }

            for (int i = 0; i < 8; ++i) {
                _westWalls [i, 0] = ConstraintMap.IsWall.YES;
                _westWalls [i, 8] = ConstraintMap.IsWall.YES;
                _northWalls [0, i] = ConstraintMap.IsWall.YES;
                _northWalls [8, i] = ConstraintMap.IsWall.YES;
            }

            _westWalls [6, 4] = ConstraintMap.IsWall.NO;
        }

        internal ConstraintState Clone ()
        {
            var clone = new ConstraintState {
                _westWalls = new ConstraintMap.IsWall [9, 9],
                _northWalls = new ConstraintMap.IsWall [9, 9],
                _isLocked = new bool [9, 9],
                _tileSets = new List<int> [9, 9]
            };

            for (int x = 0; x <= 8; ++x) {
                for (int y = 0; y <= 8; ++y) {
                    clone._westWalls [y, x] = _westWalls [y, x];
                    clone._northWalls [y, x] = _northWalls [y, x];
                    clone._tileSets [y, x] = new List<int> (_tileSets [y, x]);
                    clone._isLocked [y, x] = _isLocked [y, x];
                }
            }
            return clone;
        }

        private List<int> MakeAllTiles ()
        {
            return new List<int> {
                0, 1, 2, 3,
                4, 5, 6, 7,
                8, 9, 10};
        }

        internal bool IsSolution ()
        {
            // see if all locations are locked
            for (int x = 0; x < 8; ++x) {
                for (int y = 0; y < 8; ++y) {
                    if (!_isLocked [y, x]) {
                        return false;
                    }
                }
            }

            for (int x = 0; x < 8; ++x) {
                for (int y = 0; y < 8; ++y) {
                    var ts = _tileSets [y, x];
                    if (ts == null) {
                        Debug.Assert (ts != null);
                        return false;
                    }
                    if (ts.Count != 1) {
                        Debug.Assert (ts.Count == 1);
                        return false;
                    }
                }
            }

            // check to see if a flood fill hits all squares

            int cff = CountFloodFill ();
            if (CountFloodFill () < 62) {
                Debug.LogFormat ("reachable: {0}", cff);
                return false;
            }

            return true;
        }

        public bool FloodFillFromLocReachedUnlockedLoc (int x, int y, out int visitedCount)
        {
            List<KeyValuePair<int, int>> frontier = new List<KeyValuePair<int, int>> {
                new KeyValuePair<int, int> (x, y)
            };

            List<KeyValuePair<int, int>> visited = new List<KeyValuePair<int, int>> ();

            while (frontier.Count > 0) {
                var workNode = frontier [0];
                frontier.RemoveAt (0);
                visited.Add (workNode);

                var wx = workNode.Key;
                var wy = workNode.Value;

                //Debug.LogFormat ("Checking {0} {1}", wx, wy);

                if (!_isLocked [wy, wx]) {
                    visitedCount = visited.Count;
                    return true;
                }

                foreach (var md in MapManager.MoveDirections ()) {
                    var nx = wx;
                    var ny = wy;

                    if (!CanMoveInDirForTileIndex (md, _tileSets [ny, nx] [0])) {
                        continue;
                    }

                    switch (md) {
                    case MovementDirection.EAST:
                        nx = wx + 1;
                        break;
                    case MovementDirection.NORTH:
                        ny = wy - 1;
                        break;
                    case MovementDirection.WEST:
                        nx = wx - 1;
                        break;
                    case MovementDirection.SOUTH:
                        ny = wy + 1;
                        break;
                    }

                    if ((nx < 0) || (nx >= 8) ||
                        (ny < 0) || (ny >= 8)) {
                        continue;
                    }

                    var newLoc = new KeyValuePair<int, int> (nx, ny);
                    if ((visited.Contains (newLoc)) ||
                        (frontier.Contains (newLoc))) {
                        continue;
                    }
                    frontier.Add (newLoc);
                }
            }

            Debug.LogFormat ("Final count: {0}", visited.Count);
            visitedCount = visited.Count;
            return false;
        }

        int CountFloodFill ()
        {
            var unlocked = FloodFillFromLocReachedUnlockedLoc (0, 0, out int count);
            return count;
        }

        internal void FindMostConstrainedLoc (out int cx, out int cy)
        {
            int count = -1;
            int bestX = -1;
            int bestY = -1;

            for (int x = 0; x < 8; ++x) {
                for (int y = 0; y < 8; ++y) {
                    if (_isLocked [y, x]) {
                        continue;
                    }

                    var c = _tileSets [y, x].Count;
                    if ((count == -1) || (c < count)) {
                        bestX = x;
                        bestY = y;
                        count = c;
                    }
                }
            }

            cx = bestX;
            cy = bestY;
        }

        internal List<int> GetTileIndexPossibilities (int x, int y)
        {
            return _tileSets [y, x];
        }

        internal void ConstrainWithSymmetry (int cx, int cy, int tileIndex, bool lrSymmetry, bool nsSymmetry)
        {
            //Debug.LogFormat ("constraining with symmetry {0} {1} tile {2} lr {3} ns {4}", cx, cy, tileIndex, lrSymmetry, nsSymmetry);
            ConstrainLoc (cx, cy, tileIndex);

            var mx = 7 - cx;
            var my = 7 - cy;

            if (lrSymmetry) {
                ConstrainWalls (mx, cy, FlipLeftRight (tileIndex));
            }
            if (nsSymmetry) {
                ConstrainWalls (cx, my, FlipUpDown (tileIndex));
            }
            if (lrSymmetry && nsSymmetry) {
                ConstrainWalls (mx, my, FlipLeftRight(FlipUpDown (tileIndex)));
            }
        }

        private int FlipUpDown (int tileIndex)
        {
            switch (tileIndex) {
            case 0:
                return 3;
            case 1:
                return 2;
            case 2:
                return 1;
            case 3:
                return 0;
            case 4:
                return 6;
            case 5:
                return 5;
            case 6:
                return 4;
            case 7:
                return 7;
            case 8:
                return 8;
            case 9:
                return 9;
            case 10:
                return 10;
            default:
                var errMsg = string.Format ("Flipping {0} U/D", tileIndex);
                Debug.LogError (errMsg);
                return -1;
            }
        }

        private int FlipLeftRight (int tileIndex)
        {
            switch (tileIndex) {
            case 0:
                return 1;
            case 1:
                return 0;
            case 2:
                return 3;
            case 3:
                return 2;
            case 4:
                return 4;
            case 5:
                return 7;
            case 6:
                return 6;
            case 7:
                return 5;
            case 8:
                return 8;
            case 9:
                return 9;
            case 10:
                return 10;
            default:
                var errMsg = string.Format ("Flipping {0} L/R", tileIndex);
                Debug.LogError (errMsg);
                return -1;
            }
        }

        internal void Tighten ()
        {
            for (int x = 0; x < 8; ++x) {
                for (int y = 0; y < 8; ++y) {
                    List<int> workList = new List<int> ();

                    foreach (var tileIndex in _tileSets [y, x]) {
                        if (DoesFit (tileIndex, x, y)) {
                            workList.Add (tileIndex);
                        }
                    }

                    _tileSets [y, x] = workList;
                }
            }
        }

        private bool DoesFit (int tileIndex, int x, int y)
        {
            //Debug.LogFormat ("checking DoesFit for {0} {1} {2}", tileIndex, x, y);

            // see if this tile conflicts with the existing walls

            // east
            if (_westWalls [y, x + 1] != ConstraintMap.IsWall.MAYBE) {
                //Debug.LogFormat ("WW: {0}", _westWalls [y, x + 1]);

                bool eCanMove = _westWalls [y, x + 1] == ConstraintMap.IsWall.NO;

                //Debug.LogFormat ("ECM: {0}", eCanMove);

                var cm = CanMoveInDirForTileIndex (MovementDirection.EAST, tileIndex);

                //Debug.LogFormat ("cm {0}", cm);

                if (cm != eCanMove) {
                    //Debug.Log ("fail for East");
                    return false;
                }
            }

            // north
            if (_northWalls [y, x] != ConstraintMap.IsWall.MAYBE) {
                bool nCanMove = _northWalls [y, x] == ConstraintMap.IsWall.NO;
                if (CanMoveInDirForTileIndex (MovementDirection.NORTH, tileIndex) != nCanMove) {
                    //Debug.Log ("fail for North");
                    return false;
                }
            }

            // west
            if (_westWalls [y, x] != ConstraintMap.IsWall.MAYBE) {
                bool wCanMove = _westWalls [y, x] == ConstraintMap.IsWall.NO;
                if (CanMoveInDirForTileIndex (MovementDirection.WEST, tileIndex) != wCanMove) {
                    //Debug.Log ("fail for West");
                    return false;
                }
            }

            // south
            if (_northWalls [y + 1, x] != ConstraintMap.IsWall.MAYBE) {
                bool sCanMove = _northWalls [y + 1, x] == ConstraintMap.IsWall.NO;
                if (CanMoveInDirForTileIndex (MovementDirection.SOUTH, tileIndex) != sCanMove) {
                    //Debug.Log ("fail for South");
                    return false;
                }
            }

            return true;
        }

        internal bool IsOverTight ()
        {
            // see if any locations have no possible tiles

            for (int x = 0; x < 8; ++x) {
                for (int y = 0; y < 8; ++y) {
                    if (_isLocked [y, x]) {
                        Debug.Assert (_tileSets [y, x] != null);
                        Debug.Assert (_tileSets [y, x].Count == 1);
                        continue;
                    }
                    if (_tileSets [y, x].Count == 0) {
                        return true;
                    }
                }
            }
            return false; 
        }

        internal void ConstrainLoc (int x, int y, int tileIndex)
        {
            //Debug.LogFormat ("Constraining loc {0} {1} tile {2}", x, y, tileIndex);
            Debug.Assert (_isLocked [y, x] == false);
            _isLocked [y, x] = true;
            _tileSets [y, x] = new List<int> { tileIndex };

            List<MovementDirection> moveDirs = new List<MovementDirection> {
                MovementDirection.EAST,
                MovementDirection.NORTH,
                MovementDirection.WEST,
                MovementDirection.SOUTH 
                };

            foreach (var md in moveDirs) {
                bool cm = CanMoveInDirForTileIndex (md, tileIndex);

                ConstraintMap.IsWall isWall = cm ? ConstraintMap.IsWall.NO : ConstraintMap.IsWall.YES;
                SetWall (x, y, md, isWall);
            }
        }

        internal void ConstrainWalls (int x, int y, int tileIndex)
        {
            //Debug.LogFormat ("Constraining loc {0} {1} tile {2}", x, y, tileIndex);
            List<MovementDirection> moveDirs = new List<MovementDirection> {
                MovementDirection.EAST,
                MovementDirection.NORTH,
                MovementDirection.WEST,
                MovementDirection.SOUTH
                };

            foreach (var md in moveDirs) {
                if (GetWall (x, y, md) != ConstraintMap.IsWall.MAYBE) {
                    continue;
                }

                bool cm = CanMoveInDirForTileIndex (md, tileIndex);

                ConstraintMap.IsWall isWall = cm ? ConstraintMap.IsWall.NO : ConstraintMap.IsWall.YES;
                SetWall (x, y, md, isWall);
            }
        }


        private bool CanMoveInDirForTileIndex (MovementDirection md, int tileIndex)
        {
            bool canMoveEast = false;
            bool canMoveNorth = false;
            bool canMoveWest = false;
            bool canMoveSouth = false;

            switch (tileIndex) {
            case 0:
                // E/N
                canMoveEast = true;
                canMoveNorth = true;
                break;
            case 1:
                // N/W
                canMoveNorth = true;
                canMoveWest = true;
                break;
            case 2:
                // W/S
                canMoveWest = true;
                canMoveSouth = true;
                break;
            case 3:
                // S/E
                canMoveEast = true;
                canMoveSouth = true;
                break;
            case 4:
                // T E/N/W
                canMoveEast = true;
                canMoveNorth = true;
                canMoveWest = true;
                break;
            case 5:
                // T N/W/S
                canMoveNorth = true;
                canMoveWest = true;
                canMoveSouth = true;
                break;
            case 6:
                // T W/S/E
                canMoveWest = true;
                canMoveSouth = true;
                canMoveEast = true;
                break;
            case 7:
                // S/E/N
                canMoveEast = true;
                canMoveSouth = true;
                canMoveNorth = true;
                break;
            case 8:
                // All Way
                canMoveEast = true;
                canMoveNorth = true;
                canMoveWest = true;
                canMoveSouth = true;
                break;
            case 9:
                // E/W
                canMoveEast = true;
                canMoveWest = true;
                break;
            case 10:
                // N/S
                canMoveNorth = true;
                canMoveSouth = true;
                break;
            default:
                // includes Cage
                break;
            }

            if (md == MovementDirection.EAST) {
                return canMoveEast;
            }
            if (md == MovementDirection.NORTH) {
                return canMoveNorth;
            }
            if (md == MovementDirection.WEST) {
                return canMoveWest;
            }
            if (md == MovementDirection.SOUTH) {
                return canMoveSouth;
            }
            Debug.LogError ("should not get here");
            return false;
        }

        internal void SetWall (int x, int y, MovementDirection md, ConstraintMap.IsWall isWall)
        {
            var tx = x;
            var ty = y;
            var td = md;

            if (td == MovementDirection.EAST) {
                tx = x + 1;
                td = MovementDirection.WEST;
            } else if (td == MovementDirection.SOUTH) {
                ty = y + 1;
                td = MovementDirection.NORTH;
            }

            if (td == MovementDirection.WEST) {
                _westWalls [ty, tx] = isWall;
            } else if (td == MovementDirection.NORTH) {
                _northWalls [ty, tx] = isWall;
            }
        }

        internal ConstraintMap.IsWall GetWall (int x, int y, MovementDirection md)
        {
            var tx = x;
            var ty = y;
            var td = md;

            if (td == MovementDirection.EAST) {
                tx = x + 1;
                td = MovementDirection.WEST;
            } else if (td == MovementDirection.SOUTH) {
                ty = y + 1;
                td = MovementDirection.NORTH;
            }

            if (td == MovementDirection.WEST) {
                return _westWalls [ty, tx];
            } else if (td == MovementDirection.NORTH) {
                return _northWalls [ty, tx];
            }

            Debug.LogError ("bad direction");
            return ConstraintMap.IsWall.MAYBE;
        }
    }
}