using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    public struct DotRecord
    {
        public int x;
        public int y;
        public bool isEnergizer;
    }

    public class DotManager
    {
        List<DotRecord> _dots;
        Texture2D _spritesheet;

        public DotManager (Texture2D spritesheet)
        {
            _dots = new List<DotRecord> ();
            _spritesheet = spritesheet;
        }

        public static DotManager DotMgrSingleton {get; set;}

        public void AddDot (int x, int y, bool isEnergizer)
        {
            var dr = new DotRecord {
                x = x,
                y = y,
                isEnergizer = isEnergizer
            };

            _dots.Add (dr);
        }

        public void Draw (Texture2D destTexture)
        {
            var tx = 0;
            var ty = 0;
            foreach (DotRecord dr in _dots) {
                if (dr.isEnergizer) {
                    tx = 40;
                    ty = 16;
                } else {
                    tx = 56;
                    ty = 16;
                }
                DrawUtil.DrawSpriteAlpha (_spritesheet, destTexture, tx, ty, 8, 8, dr.x, dr.y);
            }

        }

        internal void EatAt (float x, float y)
        {
            var ix = Mathf.RoundToInt (x);
            var iy = Mathf.RoundToInt (y);

            for (int i = 0; i < _dots.Count; ++i) {
                var dr = _dots [i];
                if ((dr.x == ix) &&
                    (dr.y == iy)) {
                    _dots.RemoveAt (i);

                    if (dr.isEnergizer) {
                        GhostManager.GhostMgrSingleton.FrightenGhosts ();
                    }
                    return;
                }
            }
        }
    }
}
