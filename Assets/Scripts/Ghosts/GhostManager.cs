using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    public class GhostManager
    {
        private Texture2D _spritesheet;

        private List<Ghost> _ghosts;

        public GhostManager (Texture2D spritesheet)
        {
            _spritesheet = spritesheet;
            _ghosts = new List<Ghost> ();
        }

        public void AddGhost (Ghost.GhostName name, int px, int py, Ghost.GhostState gs) {
            var g = new Ghost (_spritesheet, name, px, py);
            g.SetState (gs);
            _ghosts.Add (g);
        }

        public static GhostManager GhostMgrSingleton { get; set; }

        public void Update (float dt)
        {
            foreach (var g in _ghosts) {
                g.Update (dt);
            }
        }

        public void Draw (Texture2D destTexture)
        {
            foreach (var g in _ghosts) {
                g.Draw (destTexture);
            }
        }

        public List<Ghost> GetGhostList ()
        {
            return _ghosts;
        }
    }
}
