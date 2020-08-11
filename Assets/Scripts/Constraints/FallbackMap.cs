using System;
using UnityEngine;
using static BDG.ConstraintMap;

namespace BDG
{
    public class FallbackMap : IBaseMapGenerator
    {
        private System.Random _randomGenerator;
        private ConstraintState _constraintState;
        private readonly int _mapX;
        private readonly int _mapY;

        public FallbackMap (int bmx, int bmy)
        {
            _mapX = bmx;
            _mapY = bmy;

            Color dbgColor;
            var exitEast = BigMapManager.BigMapMgrSingleton.CanMove (bmx, bmy, MovementDirection.EAST, out dbgColor);
            var exitNorth = BigMapManager.BigMapMgrSingleton.CanMove (bmx, bmy, MovementDirection.NORTH, out dbgColor);
            var exitWest = BigMapManager.BigMapMgrSingleton.CanMove (bmx, bmy, MovementDirection.WEST, out dbgColor);
            var exitSouth = BigMapManager.BigMapMgrSingleton.CanMove (bmx, bmy, MovementDirection.SOUTH, out dbgColor);

            string name = string.Format ("cm {0} {1}", _mapX, _mapY);
            var h = name.GetHashCode ();
            _randomGenerator = new System.Random (h);

            _constraintState = new ConstraintState ();

            Constrain (exitEast, exitNorth, exitWest, exitSouth, _randomGenerator);
        }

        private void Constrain (
            bool exitEast,
            bool exitNorth,
            bool exitWest,
            bool exitSouth,
            System.Random r)
        {
            _constraintState.Init ();

            // left cage
            _constraintState.ConstrainLoc (3, 3, 11);

            // right cage
            _constraintState.ConstrainLoc (4, 3, 12);

            if (exitEast) {
                // pick a loc with x=7, open East
                var y = r.Next (8);
                _constraintState.SetWall (7, y, MovementDirection.EAST, IsWall.NO);

            } if (exitWest) {
                // pick a loc with x=0, open West
                var y = r.Next (8);
                _constraintState.SetWall (0, y, MovementDirection.WEST, IsWall.NO);
            }

            if (exitNorth) {
                // pick a loc with y=0, open North

                var x = r.Next (8);
                _constraintState.SetWall (x, 0, MovementDirection.NORTH, IsWall.NO);

            } if (exitSouth) {
                // pick a loc with y=7, open South
                var x = r.Next (8);
                _constraintState.SetWall (x, 7, MovementDirection.SOUTH, IsWall.NO);
            }

            MakeEmptyBox (0, 0, 7, 7);
            MakeEmptyBox (1, 1, 6, 6);
            MakeEmptyBox (2, 2, 5, 5);

            MakeEnclosingBox (1, 1, 6, 6);
            MakeEnclosingBox (2, 2, 5, 5);

            switch (_randomGenerator.Next (4)) {
            case 0:
                _constraintState.ConstrainLoc (3, 4, 3);
                _constraintState.ConstrainLoc (4, 4, 2);
                break;
            case 1:
                _constraintState.ConstrainLoc (3, 4, 2);
                _constraintState.ConstrainLoc (4, 4, 3);
                break;
            case 2:
                _constraintState.ConstrainLoc (3, 4, 6);
                _constraintState.ConstrainLoc (4, 4, 6);
                break;
            case 3:
                _constraintState.ConstrainLoc (3, 4, 9);
                _constraintState.ConstrainLoc (4, 4, 9);
                break;
            }

            var ory = _randomGenerator.Next (1, 4);
            _constraintState.SetWall (0, ory, MovementDirection.EAST, IsWall.NO);
            _constraintState.SetWall (7, ory, MovementDirection.WEST, IsWall.NO);
            _constraintState.SetWall (0, 7-ory, MovementDirection.EAST, IsWall.NO);
            _constraintState.SetWall (7, 7-ory, MovementDirection.WEST, IsWall.NO);

            var orx = _randomGenerator.Next (1, 4);
            _constraintState.SetWall (orx, 0, MovementDirection.SOUTH, IsWall.NO);
            _constraintState.SetWall (orx, 7, MovementDirection.NORTH, IsWall.NO);
            _constraintState.SetWall (7 - orx, 0, MovementDirection.SOUTH, IsWall.NO);
            _constraintState.SetWall (7 - orx, 7, MovementDirection.NORTH, IsWall.NO);

            var irx = _randomGenerator.Next (2, 4);
            _constraintState.SetWall (irx, 1, MovementDirection.SOUTH, IsWall.NO);
            _constraintState.SetWall (irx, 6, MovementDirection.NORTH, IsWall.NO);
            _constraintState.SetWall (7 - irx, 1, MovementDirection.SOUTH, IsWall.NO);
            _constraintState.SetWall (7 - irx, 6, MovementDirection.NORTH, IsWall.NO);


            _constraintState.Tighten ();
        }

        void MakeEmptyBox (int left, int top, int right, int bottom)
        {
            for (int x = left; x < right; ++x) {
                _constraintState.SetWall (x, top, MovementDirection.EAST, IsWall.NO);
                _constraintState.SetWall (x, bottom, MovementDirection.EAST, IsWall.NO);
            }
            for (int y = top; y < bottom; ++y) {
                _constraintState.SetWall (left, y, MovementDirection.SOUTH, IsWall.NO);
                _constraintState.SetWall (right, y, MovementDirection.SOUTH, IsWall.NO);
            }
        }

        void MakeEnclosingBox (int left, int top, int right, int bottom)
        {
            for (int x = left; x <= right; ++x) {
                _constraintState.SetWall (x, top, MovementDirection.NORTH, IsWall.YES);
                _constraintState.SetWall (x, bottom, MovementDirection.SOUTH, IsWall.YES);
            }
            for (int y = top; y <= bottom; ++y) {
                _constraintState.SetWall (left, y, MovementDirection.WEST, IsWall.YES);
                _constraintState.SetWall (right, y, MovementDirection.EAST, IsWall.YES);
            }
        }


        public int GetTileIndex (int x, int y)
        {
            var ts = _constraintState._tileSets [y, x];

            return ts [0];
        }
    }
}
