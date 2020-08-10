using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    public class Tile
    {
        Texture2D _srcTexture;
        private int _srcX;
        private int _srcY;
        private bool [] _moveBits;

        public string Name { get; internal set; }
        public int DistToHome { get; internal set; }
        public int TileX { get; }
        public int TileY { get; }

        public bool IsCage { get; internal set; }
        public MovementDirection InCageDir { get; internal set; }

        public Tile (Texture2D srcTexture, int texSrcX, int texSrcY, int tileX, int tileY, 
            bool canMoveEast,
            bool canMoveNorth,
            bool canMoveWest,
            bool canMoveSouth)
        {
            _srcTexture = srcTexture;
            _srcX = texSrcX;
            _srcY = texSrcY;

            TileX = tileX;
            TileY = tileY;

            IsCage = false;
            InCageDir = MovementDirection.NONE;

            //Debug.LogFormat ("making tile with src x {0} y {1}", _srcX, _srcY);

            _moveBits = new bool[] { canMoveEast, canMoveNorth, canMoveWest, canMoveSouth};

            Name = string.Format ("tile {0} {1} {2} {3}", canMoveEast, canMoveNorth, canMoveWest, canMoveSouth);
        }

        public void Draw (Texture2D destTexture, int destX, int destY)
        {
            DrawUtil.DrawSpriteOpaque (_srcTexture, destTexture,
                _srcX, _srcY,
                8, 8, 
                destX, destY);
        }

        public bool CanCharMoveInDirection (Character c, MovementDirection dir)
        {
            bool charIsGhost = c is Ghost;

            var nt = NeighborInDirection (dir);
            if ((nt == null) &&
                (charIsGhost)) {
                return false;
            }

            if (charIsGhost) {
                var gc = c as Ghost;
                if (IsCage) {
                    if (gc.State == Ghost.GhostState.CAGED) {
                        return dir == InCageDir;
                    } else {
                        return (dir == MovementDirection.NORTH);
                    }
                }
            }

            switch (dir) {
            case MovementDirection.NONE:
                return false;
            case MovementDirection.EAST:
                return _moveBits [0];
            case MovementDirection.NORTH:
                return _moveBits [1];
            case MovementDirection.WEST:
                return _moveBits [2];
            case MovementDirection.SOUTH:
                if (charIsGhost) {
                    var g = c as Ghost;
                    if ((g.State == Ghost.GhostState.RETURN) &&
                        (nt.IsCage)) {
                        return true;
                    }
                }

                return _moveBits [3];
            default:
                return false;
            }
        }

        public static Tile MakeTile (Texture2D srcTexture, int tileIndex, int tileX, int tileY)
        {
            int tcX = tileIndex % 8;
            int tcY = tileIndex / 8;

            int tx = tcX * 8;
            int ty = 24 - tcY * 8;

            bool cmEast = false;
            bool cmNorth = false;
            bool cmWest = false;
            bool cmSouth = false;

            switch (tileIndex) {
            case 0:
                cmEast = true;
                cmNorth = true;
                break;
            case 1:
                cmNorth = true;
                cmWest = true;
                break;
            case 2:
                cmWest = true;
                cmSouth = true;
                break;
            case 3:
                cmEast = true;
                cmSouth = true;
                break;
            case 4:
                cmEast = true;
                cmNorth = true;
                cmWest = true;
                break;
            case 5:
                cmWest = true;
                cmNorth = true;
                cmSouth = true;
                break;
            case 6:
                cmEast = true;
                cmSouth = true;
                cmWest = true;
                break;
            case 7:
                cmEast = true;
                cmNorth = true;
                cmSouth = true;
                break;
            case 8:
                cmEast = true;
                cmWest = true;
                cmNorth = true;
                cmSouth = true;
                break;
            case 9:
                cmEast = true;
                cmWest = true;
                break;
            case 10:
                cmNorth = true;
                cmSouth = true;
                break;
            default:
                break;
            }

            var tile = new Tile (srcTexture, tx, ty, tileX, tileY,
                cmEast,
                cmNorth,
                cmWest,
                cmSouth);

            tile.Name = String.Format ("t {0} ({5} {6}) / {1} {2} {3} {4}", tileIndex, cmEast, cmNorth, cmWest, cmSouth, tileX, tileY);

            return tile;
        }

        internal bool CanGhostMoveInDirection (MovementDirection dir)
        {
            var nt = NeighborInDirection (dir);
            if (nt == null) {
                return false;
            }

            switch (dir) {
            case MovementDirection.NONE:
                return false;
            case MovementDirection.EAST:
                return _moveBits [0];
            case MovementDirection.NORTH:
                return _moveBits [1];
            case MovementDirection.WEST:
                return _moveBits [2];
            case MovementDirection.SOUTH:
                return _moveBits [3];
            default:
                return false;
            }
        }

        internal Tile NeighborInDirection (MovementDirection md)
        {
            var mm = MapManager.MapMgrSingleton;

            switch (md) {
            case MovementDirection.NONE:
                return this;
            case MovementDirection.EAST:
                return mm.GetTileAt (TileX + 1, TileY);
            case MovementDirection.NORTH:
                return mm.GetTileAt (TileX, TileY - 1);
            case MovementDirection.WEST:
                return mm.GetTileAt (TileX - 1, TileY);
            case MovementDirection.SOUTH:
                return mm.GetTileAt (TileX, TileY + 1);
            default:
                return null;
            }
        }

        internal List<MovementDirection> GetLegalDirectionsForChar (Character c)
        {
            var dirList = new List<MovementDirection> ();
            if (CanCharMoveInDirection (c, MovementDirection.EAST)) {
                dirList.Add (MovementDirection.EAST);
            }
            if (CanCharMoveInDirection (c, MovementDirection.NORTH)) {
                dirList.Add (MovementDirection.NORTH);
            }
            if (CanCharMoveInDirection (c, MovementDirection.WEST)) {
                dirList.Add (MovementDirection.WEST);
            }
            if (CanCharMoveInDirection (c, MovementDirection.SOUTH)) {
                dirList.Add (MovementDirection.SOUTH);
            }
            return dirList;
        }
    }
}
