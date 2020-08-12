using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    public class MapManager
    {
        Tile [,] _tiles;

        public MapManager ()
        {
            _tiles = new Tile [8, 8];
        }

        public static MapManager MapMgrSingleton { get; set; }

        public void SetTile (int x, int y, Tile t)
        {
            _tiles [y, x] = t;
        }

        public void Draw (Texture2D destTexture)
        {
            for (int x = 0; x < 8; ++x) {
                for (int y = 0; y < 8; ++y) {
                    Tile t = _tiles [y, x];
                    if (t != null) {
                        t.Draw (destTexture, x * 8, 56 - y * 8);
                    }
                }
            }
        }

        public Tile GetTileAt (int tx, int ty)
        {
            //Debug.LogFormat ("getting tile at {0} {1}", tx, ty);
            if ((tx < 0) || (tx >= 8) ||
                (ty < 0) || (ty >= 8)) {
                //Debug.LogFormat ("out of range: {0} {1}", tx, ty);
                return null;
            }
            var t = _tiles [ty, tx];
            if (t == null) {
                Debug.LogError ("null tile");
            } else {
                //Debug.LogFormat ("found tile {0}", t.Name);
            }
            return _tiles [ty, tx];
        }

        public Tile GetTileForPixel (float xPos, float yPos)
        {
            int ix = Mathf.FloorToInt (xPos / 8.0f);
            int iy = 7 - Mathf.FloorToInt (yPos / 8.0f);

            return GetTileAt (ix, iy);
        }

        public void GetPixelCoordsForTileCoords (int homeTileX, int homeTileY, out int homeXPixel, out int homeYPixel)
        {
            homeXPixel = 8 * homeTileX;
            homeYPixel = 56 - (8 * homeTileY);
        }

        public void MakeDistToHomeValues ()
        {
            foreach (var t in _tiles) {
                if (t != null) {
                    t.DistToHome = -1;
                }
            }

            var houseLeft = GetTileAt (3, 3);
            houseLeft.DistToHome = 0;
            houseLeft.IsCage = true;
            houseLeft.InCageDir = MovementDirection.EAST;
            var houseRight = GetTileAt (4, 3);
            houseRight.DistToHome = 0;
            houseRight.IsCage = true;
            houseRight.InCageDir = MovementDirection.WEST;
            var exitLeft = GetTileAt (3, 2);
            exitLeft.DistToHome = 1;
            var exitRight = GetTileAt (4, 2);
            exitRight.DistToHome = 1;

            List<Tile> openTiles = new List<Tile> { exitLeft, exitRight };

            List<MovementDirection> moveDirs = new List<MovementDirection> {
                MovementDirection.EAST,
                MovementDirection.NORTH,
                MovementDirection.WEST,
                MovementDirection.SOUTH };

            while (openTiles.Count > 0) {
                var t = openTiles [0];
                openTiles.RemoveAt (0);

                if ((t.TileX < 0) || (t.TileX >= 8) ||
                    (t.TileY < 0) || (t.TileY >= 8)) {
                    continue;
                }

                foreach (var md in moveDirs) {
                    if (t.CanGhostMoveInDirection (md)) {
                        var nextTile = t.NeighborInDirection (md);
                        if (nextTile == null) {
                            continue;
                        }

                        if ((nextTile.TileX < 0) || (nextTile.TileX >= 8) ||
                            (nextTile.TileY < 0) || (nextTile.TileY >= 8)) {
                            continue;
                        }

                        if (nextTile.DistToHome == -1) {
                            nextTile.DistToHome = t.DistToHome + 1;
                            openTiles.Add (nextTile);

                            //Debug.LogFormat ("Setting tile at {0} {1} to dist {2}", nextTile.TileX, nextTile.TileY, nextTile.DistToHome);
                        }
                    }
                }
            }
        }

        internal void ResetTiles ()
        {
            _tiles = new Tile [8, 8];
        }

        public static List<MovementDirection> MoveDirections ()
        {
            return new List<MovementDirection> {
                MovementDirection.EAST,
                MovementDirection.NORTH,
                MovementDirection.WEST,
                MovementDirection.SOUTH 
                };
        }
    }
}
