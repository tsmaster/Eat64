using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    public class Ghost : Character
    {
        public enum GhostName
        {
            BLINKY,
            PINKY,
            INKY,
            CLYDE
        }

        public enum GhostState
        {
            SCATTER,
            CHASE,
            FRIGHTENED,
            RETURN,
            CAGED
        }

        GhostName _name;
        float _scatterLength;
        float _scatterElapsed;
        float _chaseLength;
        float _chaseElapsed;
        float _frightenedLength;
        float _frightenedElapsed;
        float _cagedLength;
        float _cagedElapsed;
        float _blinkLength;
        int _numBlinks;

        int _homeXPixel;
        int _homeYPixel;
        private int _startPosX;
        private int _startPosY;
        private GhostState _startState;

        public GhostState State { get; private set; }

        public Ghost (Texture2D spritesheet, GhostName name, int xPos, int yPos, GhostState startState) : base(spritesheet, 8,  8, xPos, yPos)
        {
            _name = name;
            _scatterLength = 10.0f;
            _scatterElapsed = 0.0f;
            _chaseLength = 8.0f;
            _chaseElapsed = 0.0f;
            _frightenedLength = 6.0f;
            _frightenedElapsed = 0.0f;

            _cagedLength = 1.5f;
            _cagedElapsed = 0.0f;

            _blinkLength = 0.4f;
            _numBlinks = 4;

            _homeXPixel = -1;
            _homeYPixel = -1;

            _startPosX = xPos;
            _startPosY = yPos;
            _startState = startState;
            State = startState;

            Speed = 10.0f;
        }

        internal void ReturnToStart ()
        {
            XPos = _startPosX;
            YPos = _startPosY;
            MoveDir = MovementDirection.NONE;
            SetGhostState (_startState);
        }

        public void SetGhostState (GhostState gs)
        {
            State = gs;

            switch (gs) {
            case GhostState.SCATTER:
                _scatterElapsed = 0.0f;
                break;
            case GhostState.CHASE:
                _chaseElapsed = 0.0f;
                break;
            case GhostState.FRIGHTENED:
                _frightenedElapsed = 0.0f;
                break;
            case GhostState.RETURN:
                break;
            case GhostState.CAGED:
                _cagedElapsed = 0.0f;
                break;
            }
        }

        public void SetHomeTile (int homeTileX, int homeTileY)
        {
            MapManager.MapMgrSingleton.GetPixelCoordsForTileCoords (homeTileX, homeTileY, out _homeXPixel, out _homeYPixel);
        }


        int PeriodsUntilFrightenedElapsed ()
        {
            float floatSecs = _frightenedLength - _frightenedElapsed;
            return Mathf.FloorToInt (floatSecs / _blinkLength);
        }

        protected override int GetSourceX ()
        {
            if (State == GhostState.FRIGHTENED) {
                int periods = PeriodsUntilFrightenedElapsed ();
                if ((periods > _numBlinks) || (periods % 2 == 0)) {
                    return 32;
                } else {
                    return 40;
                }
            }
            if (State == GhostState.RETURN) {
                return 48;
            }

            switch (_name) {
            case GhostName.BLINKY:
                return 0;
            case GhostName.PINKY:
                return 8;
            case GhostName.INKY:
                return 16;
            case GhostName.CLYDE:
                return 24;
            default:
                return 0;
            }
        }

        protected override int GetSourceY ()
        {
            if (State == GhostState.FRIGHTENED) {
                return 0;
            }
            if (State == GhostState.RETURN) {
                return 0;
            }

            switch (_name) {
            case GhostName.BLINKY:
            case GhostName.PINKY:
            case GhostName.INKY:
            case GhostName.CLYDE:
                return 8;
            default:
                return 0;
            }
        }

        protected override void ReachedStop ()
        {
            //Debug.LogFormat ("Ghost {0} reached stop in state {1}", _name, State);

            switch (State) {
            case GhostState.SCATTER:
                ScatterBrain ();
                return;
            case GhostState.CHASE:
                switch (_name) {
                case GhostName.BLINKY:
                    BlinkyBrain ();
                    return;
                case GhostName.PINKY:
                    PinkyBrain ();
                    return;
                case GhostName.INKY:
                    InkyBrain ();
                    return;
                case GhostName.CLYDE:
                    ClydeBrain ();
                    return;
                default:
                    RandomBrain ();
                    return;
                }
            case GhostState.FRIGHTENED:
                RandomBrain ();
                break;
            case GhostState.RETURN:
                ReturnBrain ();
                break;
            case GhostState.CAGED:
                CagedBrain ();
                break;
            }
        }

        private void CagedBrain ()
        {
            var curTile = MapManager.MapMgrSingleton.GetTileForPixel (XPos, YPos);

            //Debug.LogFormat ("in cage tile {0}", curTile.Name);

            //Debug.LogFormat ("check cage time {0} vs limit {1}", _cagedElapsed, _cagedLength);

            if ((_cagedElapsed >= _cagedLength) && (!DotManager.DotMgrSingleton.IsCleared())) {
                //Debug.LogFormat ("cage dur elapsed");
                MoveDir = MovementDirection.NORTH;
                SetGhostState(GhostState.SCATTER);
            } else {
                MoveDir = curTile.InCageDir;
                //Debug.LogFormat ("moving in dir {0}", MoveDir);
            }
            SetStops (MoveDir, XPos, YPos);
        }

        private void ReturnBrain ()
        {
            var curTile = MapManager.MapMgrSingleton.GetTileForPixel (XPos, YPos);
            if (curTile.IsCage) {
                SetGhostState(GhostState.CAGED);
                return;
            }

            var moveDirs = curTile.GetLegalDirectionsForChar (this);

            var bestDir = moveDirs [0];
            int bestDist = 6000;

            foreach (var md in moveDirs) {
                var nt = curTile.NeighborInDirection (md);
                if (nt == null) {
                    //Debug.LogFormat ("got null neighbor of {0} in direction {1}", curTile.Name, md);
                    continue;
                }
                var ntdth = nt.DistToHome;
                if (ntdth < bestDist) {
                    bestDir = md;
                    bestDist = ntdth;
                }
            }

            MoveDir = bestDir;
            SetStops (MoveDir, XPos, YPos);
        }

        private void ScatterBrain ()
        {
            MoveToTarget (_homeXPixel, _homeYPixel);
        }

        public override void Update (float dt)
        {
            var pm = PacMan.PacManSingleton;

            if (pm.IsAlive && CollidedWithPacMan ()) {
                if (State == GhostState.FRIGHTENED) {
                    SetGhostState(GhostState.RETURN);
                    SoundMgr.Singleton.Play (SoundMgr.Sound.EatGhost);
                } else if (State != GhostState.RETURN) {
                    pm.Kill ();
                    SoundMgr.Singleton.Play (SoundMgr.Sound.EatPacMan);
                }
            }

            if ((State != GhostState.CAGED) && (DotManager.DotMgrSingleton.IsCleared ())) {
                SetGhostState (GhostState.RETURN);
            }

            if (MoveDir == MovementDirection.NONE) {
                //Debug.LogFormat ("Ghost {0} updating when stopped", _name);
                var mm = MapManager.MapMgrSingleton;

                var tile = mm.GetTileForPixel (XPos, YPos);

                List<MovementDirection> dirs = tile.GetLegalDirectionsForChar (this);

                //var mi = UnityEngine.Random.Range (0, dirs.Count);
                //MoveDir = dirs [mi];

                MoveDir = dirs [0];
                SetStops (MoveDir, XPos, YPos);
                //Debug.LogFormat ("Ghost {0} new dir: {1}", _name, MoveDir);
            }

            switch (State) {
            case GhostState.CHASE:
                _chaseElapsed += dt;
                if (_chaseElapsed >= _chaseLength) {
                    MoveDir = Character.OppositeMoveDirection (MoveDir);
                    SetStops (MoveDir, XPos, YPos);
                    SetGhostState(GhostState.SCATTER);
                }
                break;
            case GhostState.SCATTER:
                _scatterElapsed += dt;
                if (_scatterElapsed >= _scatterLength) {
                    MoveDir = Character.OppositeMoveDirection (MoveDir);
                    SetStops (MoveDir, XPos, YPos);
                    SetGhostState(GhostState.CHASE);
                }
                break;
            case GhostState.FRIGHTENED:
                _frightenedElapsed += dt;
                if (_frightenedElapsed >= _frightenedLength) {
                    SetGhostState(GhostState.CHASE);
                }
                break;
            case GhostState.CAGED:
                _cagedElapsed += dt;
                break;
            }


            base.Update (dt);
        }

        private bool CollidedWithPacMan ()
        {
            var pm = PacMan.PacManSingleton;

            var dx = pm.XPos - XPos;
            var dy = pm.YPos - YPos;

            return Mathf.Abs (dx) + Mathf.Abs (dy) < 4;
        }

        private void RandomBrain ()
        {
            var mm = MapManager.MapMgrSingleton;

            var tile = mm.GetTileForPixel (XPos, YPos);

            if (tile == null) {
                //Debug.LogFormat ("no tile for pos {0} {1}", XPos, YPos);
                MoveDir = MovementDirection.NONE;
                return;
            }

            //Debug.LogFormat ("at tile {0} {1} {2}", XPos, YPos, tile.Name);

            List<MovementDirection> dirs = tile.GetLegalDirectionsForChar (this);
            if (dirs.Count > 1) {
                dirs.Remove (Character.OppositeMoveDirection (MoveDir));
            }

            var mi = UnityEngine.Random.Range (0, dirs.Count);
            MoveDir = dirs [mi];
            SetStops (MoveDir, XPos, YPos);
            //Debug.LogFormat ("Ghost {0} new dir: {1}", _name, MoveDir);
        }

        private void BlinkyBrain ()
        {
            var pacMan = PacMan.PacManSingleton;

            MoveToTarget (pacMan.XPos, pacMan.YPos);
        }

        private void PinkyBrain ()
        {
            // steer towards location n spaces ahead of pac man

            int ahead = 3;

            var pacMan = PacMan.PacManSingleton;
            var pacDir = pacMan.MoveDir;

            var pacX = pacMan.XPos;
            var pacY = pacMan.YPos;

            var pacDX = 0;
            var pacDY = 0;
            GetDeltaPixelsFromDirection (pacDir, out pacDX, out pacDY);

            var targetX = pacX + pacDX * ahead;
            var targetY = pacY + pacDY * ahead;

            MoveToTarget (targetX, targetY);
        }

        private void InkyBrain ()
        {
            // T0 = PM + 2
            // T = T0 + T0 - B

            var gm = GhostManager.GhostMgrSingleton;
            var ghosts = gm.GetGhostList ();
            var bg = ghosts [0];

            int ahead = 2;

            var pacMan = PacMan.PacManSingleton;
            var pacDir = pacMan.MoveDir;

            var pacX = pacMan.XPos;
            var pacY = pacMan.YPos;

            var pacDX = 0;
            var pacDY = 0;
            GetDeltaPixelsFromDirection (pacDir, out pacDX, out pacDY);

            var t0X = pacX + pacDX * ahead;
            var t0Y = pacY + pacDY * ahead;

            var tx = t0X * 2 - bg.XPos;
            var ty = t0Y * 2 - bg.YPos;

            MoveToTarget (tx, ty);
        }

        private void ClydeBrain ()
        {
            // if dist > thresh, move towards PM
            // else, move towards home

            var threshSqr = 400;

            var pacMan = PacMan.PacManSingleton;

            var pacX = pacMan.XPos;
            var pacY = pacMan.YPos;

            var dx = pacX - XPos;
            var dy = pacY - YPos;

            var distSqr = dx * dx + dy * dy;

            //Debug.LogFormat ("distSqr for Clyde: {0}", distSqr);

            float tx, ty;
            if (distSqr > threshSqr) {
                tx = pacX;
                ty = pacY;
                //Debug.LogFormat ("steering to pac");
            } else {
                tx = _homeXPixel;
                ty = _homeYPixel;
                //Debug.LogFormat ("steering home");
            }

            MoveToTarget (tx, ty);
        }

        /// <summary>
        /// Moves to target.
        /// </summary>
        /// <param name="targetX">Target x pixel Coordinate.</param>
        /// <param name="targetY">Target y pixel coordinate.</param>
        private void MoveToTarget (float targetX, float targetY)
        {
            var mm = MapManager.MapMgrSingleton;
            var tile = mm.GetTileForPixel (XPos, YPos);

            List<MovementDirection> dirs = tile.GetLegalDirectionsForChar (this);
            dirs.Remove (Character.OppositeMoveDirection (MoveDir));

            float bestDistance = -1.0f;
            MovementDirection bestDir = MovementDirection.NONE;

            foreach (var d in dirs) {
                int dx, dy;
                GetDeltaPixelsFromDirection (d, out dx, out dy);
                var px = XPos + dx;
                var py = YPos + dy;

                var distX = px - targetX;
                var distY = py - targetY;

                var distSqr = distX * distX + distY * distY;
                if ((bestDir == MovementDirection.NONE) ||
                    (bestDistance > distSqr)) {
                    bestDistance = distSqr;
                    bestDir = d;
                }
            }

            MoveDir = bestDir;
            SetStops (MoveDir, XPos, YPos);
            //Debug.LogFormat ("{0} Moving {1}", _name, MoveDir);
        }
    }
}
