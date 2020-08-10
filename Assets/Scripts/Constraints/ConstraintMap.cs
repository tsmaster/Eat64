using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    internal class ConstraintMap
    {
        private int _mapX;
        private int _mapY;
        private System.Random _randomGenerator;
        private List<ConstraintState> _constraintStates;
        private bool _lrSymmetry;
        private bool _nsSymmetry;
        private ConstraintState _foundSolution;

        public enum IsWall
        {
            NO,
            YES,
            MAYBE
        }

        public ConstraintMap (int bmx, int bmy)
        {
            _mapX = bmx;
            _mapY = bmy;

            Color dbgColor;
            var exitEast = BigMapManager.BigMapMgrSingleton.CanMove (bmx, bmy, MovementDirection.EAST, out dbgColor);
            var exitNorth = BigMapManager.BigMapMgrSingleton.CanMove (bmx, bmy, MovementDirection.NORTH, out dbgColor);
            var exitWest = BigMapManager.BigMapMgrSingleton.CanMove (bmx, bmy, MovementDirection.WEST, out dbgColor);
            var exitSouth = BigMapManager.BigMapMgrSingleton.CanMove (bmx, bmy, MovementDirection.SOUTH, out dbgColor);

            _lrSymmetry = false;
            _nsSymmetry = false;

            if (exitWest && exitEast) {
                _lrSymmetry = true;
            }
            if (exitNorth && exitSouth) {
                _nsSymmetry = true;
            }

            string name = string.Format ("cm {0} {1}", _mapX, _mapY);
            var h = name.GetHashCode ();
            _randomGenerator = new System.Random (h);

            _constraintStates = new List<ConstraintState> ();

            for (int i = 0; i < 1; ++i) {
                AddInitialConstraintState (exitEast, exitNorth, exitWest, exitSouth, _randomGenerator);
            }
        }

        public bool Constrain ()
        {
            int dbgBailOutCount = 6000;

            while (_constraintStates.Count > 0) {
                if (dbgBailOutCount <= 0) {
                    Debug.LogError ("bailed out in constrain");
                    return false;
                }
                dbgBailOutCount--;

                var workState = _constraintStates [0];
                _constraintStates.RemoveAt (0);

                if (workState.IsOverTight ()) {
                    continue;
                }

                if (workState.IsSolution ()) {
                    Debug.LogFormat ("found solution after {0} steps", dbgBailOutCount);
                    _foundSolution = workState;
                    return true;
                }

                workState.FindMostConstrainedLoc (out int cx, out int cy);
                if ((cx == -1) || (cy == -1)) {
                    continue;
                }

                var possibilities = workState.GetTileIndexPossibilities (cx, cy);
                MathUtil.Shuffle (possibilities, _randomGenerator);
                foreach (var tileIndex in possibilities) {
                    //Debug.LogFormat ("considering {0} for {1} {2}", tileIndex, cx, cy);
                    var workStateClone = workState.Clone ();
                    workStateClone.ConstrainWithSymmetry (cx, cy, tileIndex, _lrSymmetry, _nsSymmetry);
                    workStateClone.Tighten ();
                    if (!workStateClone.IsOverTight ()) {
                        _constraintStates.Insert (0, workStateClone);
                    }
                }
            }
            Debug.LogError ("ran out of states in constrain");
            return false;
        }

        internal int GetTileIndex (int x, int y)
        {
            Debug.LogFormat ("getting tile index for {0} {1}", x, y);
            var ts = _foundSolution._tileSets [y, x];
            if (ts == null) {
                Debug.LogError ("null tile set");
            } else if (ts.Count == 0) {
                Debug.LogError ("empty tile set");
            } else {
                Debug.LogFormat ("tile: {0}", ts[0]);
            }

            return ts [0];
        }

        private void AddInitialConstraintState (
            bool exitEast,
            bool exitNorth,
            bool exitWest,
            bool exitSouth,
            System.Random r)
        {
            var s = new ConstraintState ();
            s.Init ();

            // left cage
            s.ConstrainLoc (3, 3, 11);

            // right cage
            s.ConstrainLoc (4, 3, 12);

            if (_nsSymmetry) {
                // S/E 
                s.ConstrainLoc (3, 4, 3);

                // S/W 
                s.ConstrainLoc (4, 4, 2);
            }

            if (exitEast) {
                // pick a loc with x=7, open East

                var y = r.Next (8);
                s.SetWall (7, y, MovementDirection.EAST, IsWall.NO);

                if (_lrSymmetry) {
                    // and then with x = 0, open West
                    s.SetWall (0, y, MovementDirection.WEST, IsWall.NO);
                }
            } else if (exitWest) {
                // pick a loc with x=0, open West
                var y = r.Next (8);
                s.SetWall (0, y, MovementDirection.WEST, IsWall.NO);
            }

            if (exitNorth) {
                // pick a loc with y=0, open North

                var x = r.Next (8);
                s.SetWall (x, 0, MovementDirection.NORTH, IsWall.NO);

                if (_nsSymmetry) {
                    // and then with y=7, open South
                    s.SetWall (x, 7, MovementDirection.SOUTH, IsWall.NO);
                }
            } else if (exitSouth) {
                // pick a loc with y=7, open South
                var x = r.Next (8);
                s.SetWall (x, 7, MovementDirection.SOUTH, IsWall.NO);
            }

            s.Tighten ();

            if (!s.IsOverTight ()) {
                _constraintStates.Add (s);
            }
        }
    }

    internal class MathUtil
    {
        internal static void Shuffle<T> (List<T> tList, System.Random randomGenerator)
        {
            for (int i = 0; i < tList.Count; ++i) {
                T tmpVal = tList [i];
                var randIdx = randomGenerator.Next (tList.Count);
                tList [i] = tList [randIdx];
                tList [randIdx] = tmpVal;
            }
        }
    }
}