﻿using System;
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
            return _tiles [ty, tx];
        }
    }
}
