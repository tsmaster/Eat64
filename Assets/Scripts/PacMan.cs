using System;
using UnityEngine;

namespace BDG
{
    public class PacMan : Character
    {
        MovementDirection _queuedMovementDirection;
        bool _mouthOpen = false;
        float _mouthPeriod = 0.2f;
        float _mouthTimer = 0.0f;

        public PacMan (Texture2D spritesheet, int xPos, int yPos) :
            base (spritesheet, 8, 8, xPos, yPos)
        {
            _queuedMovementDirection = MovementDirection.NONE;
            Speed = 12.0f;
            IsAlive = true;
        }

        public static PacMan PacManSingleton {get; set;}
        public bool IsAlive { get; internal set; }

        public void QueueMovementDirection (MovementDirection dir)
        {
            _queuedMovementDirection = dir;

            var t = MapManager.MapMgrSingleton.GetTileForPixel (XPos, YPos);
            if ((MoveDir == MovementDirection.NONE) &&
                (t.CanCharMoveInDirection (this, dir))) {
                MoveDir = dir;
                SetStops (dir, XPos, YPos);
            }
        }

        protected override int GetSourceX ()
        {
            if (!IsAlive) {
                return 56;
            }

            if ((!_mouthOpen) || (MoveDir == MovementDirection.NONE)) {
                // draw mouth closed
                return 0;
            }

            switch (MoveDir) {
            case MovementDirection.EAST:
                return 8;
            case MovementDirection.NORTH:
                return 16;
            case MovementDirection.WEST:
                return 24;
            case MovementDirection.SOUTH:
                return 32;
            }
            return 0;
        }

        protected override int GetSourceY ()
        {
            if (!IsAlive) {
                return 0;
            }

            return 8;
        }

        public override void Update (float dt)
        {
            base.Update (dt);

            if (MoveDir != MovementDirection.NONE) {
                _mouthTimer += dt;
                if (_mouthTimer > _mouthPeriod) {
                    _mouthOpen = !_mouthOpen;
                    _mouthTimer = 0.0f;
                }
            }

            // Debug.LogFormat ("Pac Man Pos: {0} {1}", XPos, YPos);
        }

        override protected void ReachedStop ()
        {
            Debug.LogFormat ("reached stop");
            Debug.LogFormat ("pos {0} {1}", XPos, YPos);

            var tx = Mathf.FloorToInt (XPos / 8.0f);
            var ty = 7 - Mathf.FloorToInt (YPos / 8.0f);

            var mapMgr = MapManager.MapMgrSingleton;

            var tile = mapMgr.GetTileAt (tx, ty);

            if (tile == null) {
                // entered tunnel
                Debug.LogFormat ("Pac man entered tunnel");
                MoveDir = MovementDirection.NONE;
                return;
            }

            Debug.LogFormat ("tile: {0}", tile.Name);

            Debug.LogFormat ("existing movedir: {0}", MoveDir);

            if (tile.CanCharMoveInDirection (this, _queuedMovementDirection)) {
                Debug.LogFormat ("can move in queued dir: {0}", _queuedMovementDirection);
                MoveDir = _queuedMovementDirection;
            } else if (!tile.CanCharMoveInDirection (this, MoveDir)) {
                Debug.LogFormat ("can move in current movedir: {0}", MoveDir);
                MoveDir = MovementDirection.NONE;
            }
            SetStops (MoveDir, XPos, YPos);
        }

        internal void Stop ()
        {
            MoveDir = MovementDirection.NONE;
        }

        internal void Kill ()
        {
            IsAlive = false;
        }
    }
}
