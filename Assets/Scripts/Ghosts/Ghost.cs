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
        GhostState _ghostState;

        float _scatterLength;
        float _scatterElapsed;
        float _chaseLength;
        float _chaseElapsed;
        float _frightenedLength;
        float _frightenedElapsed;
        float _blinkLength;
        int _numBlinks;

        int _homeXPixel;
        int _homeYPixel;

        public Ghost (Texture2D spritesheet, GhostName name, int xPos, int yPos) : base(spritesheet, 8,  8, xPos, yPos)
        {
            _name = name;
            _ghostState = GhostState.SCATTER;
            _scatterLength = 10.0f;
            _scatterElapsed = 0.0f;
            _chaseLength = 8.0f;
            _chaseElapsed = 0.0f;
            _frightenedLength = 6.0f;
            _frightenedElapsed = 0.0f;
            _blinkLength = 0.4f;
            _numBlinks = 4;

            _homeXPixel = -1;
            _homeYPixel = -1;

            Speed = 10.0f;
        }

        internal void SetFrightened ()
        {
            _ghostState = GhostState.FRIGHTENED;
            _frightenedElapsed = 0.0f; // TODO what if I'm already frightened?
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
            if (_ghostState == GhostState.FRIGHTENED) {
                int periods = PeriodsUntilFrightenedElapsed ();
                if ((periods > _numBlinks) || (periods % 2 == 0)) {
                    return 32;
                } else {
                    return 40;
                }
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
            if (_ghostState == GhostState.FRIGHTENED) {
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
            Debug.LogFormat ("Ghost {0} reached stop in state {1}", _name, _ghostState);

            switch (_ghostState) {
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
            throw new NotImplementedException ();
        }

        private void ReturnBrain ()
        {
            throw new NotImplementedException ();
        }

        private void ScatterBrain ()
        {
            MoveToTarget (_homeXPixel, _homeYPixel);
        }

        public void SetState (GhostState gs)
        {
            _ghostState = gs;
        }

        public override void Update (float dt)
        {
            if (MoveDir == MovementDirection.NONE) {
                Debug.LogFormat ("Ghost {0} updating when stopped", _name);
                var mm = MapManager.MapMgrSingleton;

                var tile = mm.GetTileForPixel (XPos, YPos);

                List<MovementDirection> dirs = tile.GetLegalDirections ();

                //var mi = UnityEngine.Random.Range (0, dirs.Count);
                //MoveDir = dirs [mi];

                MoveDir = dirs [0];
                SetStops (MoveDir, XPos, YPos);
                Debug.LogFormat ("Ghost {0} new dir: {1}", _name, MoveDir);
            }

            switch (_ghostState) {
            case GhostState.CHASE:
                _chaseElapsed += dt;
                if (_chaseElapsed >= _chaseLength) {
                    MoveDir = Character.OppositeMoveDirection (MoveDir);
                    SetStops (MoveDir, XPos, YPos);
                    _ghostState = GhostState.SCATTER;
                    _chaseElapsed = 0.0f;
                    _scatterElapsed = 0.0f;
                    _frightenedElapsed = 0.0f;
                }
                break;
            case GhostState.SCATTER:
                _scatterElapsed += dt;
                if (_scatterElapsed >= _scatterLength) {
                    MoveDir = Character.OppositeMoveDirection (MoveDir);
                    SetStops (MoveDir, XPos, YPos);
                    _ghostState = GhostState.CHASE;
                    _chaseElapsed = 0.0f;
                    _scatterElapsed = 0.0f;
                    _frightenedElapsed = 0.0f;
                }
                break;
            case GhostState.FRIGHTENED:
                _frightenedElapsed += dt;
                if (_frightenedElapsed >= _frightenedLength) {
                    _ghostState = GhostState.CHASE;
                    _chaseElapsed = 0.0f;
                    _scatterElapsed = 0.0f;
                    _frightenedElapsed = 0.0f;
                }
                break;
            }


            base.Update (dt);
        }

        private void RandomBrain ()
        {
            var mm = MapManager.MapMgrSingleton;

            var tile = mm.GetTileForPixel (XPos, YPos);

            Debug.LogFormat ("at tile {0} {1} {2}", XPos, YPos, tile.Name);

            List<MovementDirection> dirs = tile.GetLegalDirections ();
            dirs.Remove (Character.OppositeMoveDirection (MoveDir));

            foreach (var d in dirs) {
                Debug.LogFormat ("legal dir: {0}", d);
            }

            var mi = UnityEngine.Random.Range (0, dirs.Count);
            MoveDir = dirs [mi];
            SetStops (MoveDir, XPos, YPos);
            Debug.LogFormat ("Ghost {0} new dir: {1}", _name, MoveDir);
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

            Debug.LogFormat ("distSqr for Clyde: {0}", distSqr);

            float tx, ty;
            if (distSqr > threshSqr) {
                tx = pacX;
                ty = pacY;
                Debug.LogFormat ("steering to pac");
            } else {
                tx = _homeXPixel;
                ty = _homeYPixel;
                Debug.LogFormat ("steering home");
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

            List<MovementDirection> dirs = tile.GetLegalDirections ();
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
            Debug.LogFormat ("{0} Moving {1}", _name, MoveDir);
        }
    }
}
