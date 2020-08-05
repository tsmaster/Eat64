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

        private bool[] _moveBits;

        public string Name { get; internal set; }

        public Tile (Texture2D srcTexture, int srcX, int srcY, 
            bool canMoveEast,
            bool canMoveNorth,
            bool canMoveWest,
            bool canMoveSouth)
        {
            _srcTexture = srcTexture;
            _srcX = srcX;
            _srcY = srcY;

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

        public bool CanMoveInDirection (MovementDirection dir)
        {
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

        public static Tile MakeTile (Texture2D srcTexture, int tileIndex)
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


            var tile = new Tile (srcTexture, tx, ty,
                cmEast,
                cmNorth,
                cmWest,
                cmSouth);

            tile.Name = String.Format ("t {0} {1} {2} {3} {4}", tileIndex, cmEast, cmNorth, cmWest, cmSouth);

            return tile;
        }

        internal List<MovementDirection> GetLegalDirections ()
        {
            var dirList = new List<MovementDirection> ();
            if (CanMoveInDirection (MovementDirection.EAST)) {
                dirList.Add (MovementDirection.EAST);
            }
            if (CanMoveInDirection (MovementDirection.NORTH)) {
                dirList.Add (MovementDirection.NORTH);
            }
            if (CanMoveInDirection (MovementDirection.WEST)) {
                dirList.Add (MovementDirection.WEST);
            }
            if (CanMoveInDirection (MovementDirection.SOUTH)) {
                dirList.Add (MovementDirection.SOUTH);
            }
            return dirList;
        }
    }
}
