using System;
using UnityEngine;

namespace BDG
{
    public enum MovementDirection {
        NONE,
        EAST,
        NORTH,
        WEST,
        SOUTH
    };

    public abstract class Character
    {
        Texture2D _spritesheet;

        public float XPos { get; set; }
        public float YPos { get; set; }

        private int _width;
        private int _height;

        private int _stopX;
        private int _stopY;
        public MovementDirection MoveDir { get; set; }

        public Character (Texture2D spritesheet, int width, int height, 
            int xPos, int yPos)
        {
            _spritesheet = spritesheet;
            XPos = xPos;
            YPos = yPos;

            _width = width;
            _height = height;
        }

        public abstract float Speed (); // pixels per second

        public void SetPos (int x, int y) {
            XPos = x;
            YPos = y;
        }

        protected abstract int GetSourceX ();
        protected abstract int GetSourceY ();

        public void Draw (Texture2D destTexture)
        {
            DrawUtil.DrawSpriteAlpha (_spritesheet, destTexture,
                GetSourceX(), GetSourceY(),
                _width, _height,
                Mathf.RoundToInt(XPos), 
                Mathf.RoundToInt(YPos));
        }

        void MoveInDirection (MovementDirection dir, float units)
        {
            if (dir == MovementDirection.NONE) {
                return;
            }

            //Debug.LogFormat ("moving {0} units in dir {1}", units, dir);

            switch (dir) {
            case MovementDirection.EAST:
                XPos += units;
                if (XPos >= _stopX) {
                    XPos = _stopX;
                    ReachedStop ();
                }
                break;
            case MovementDirection.NORTH:
                YPos += units;
                if (YPos >= _stopY) {
                    YPos = _stopY;
                    ReachedStop ();
                }
                break;
            case MovementDirection.WEST:
                XPos -= units;
                if (XPos <= _stopX) {
                    XPos = _stopX;
                    ReachedStop ();
                }
                break;
            case MovementDirection.SOUTH:
                YPos -= units;
                if (YPos <= _stopY) {
                    YPos = _stopY;
                    ReachedStop ();
                }
                break;
            case MovementDirection.NONE:
                // do nothing
                break;
            }
        }

        public static MovementDirection OppositeMoveDirection (MovementDirection moveDir)
        {
            switch (moveDir) {
            case MovementDirection.EAST:
                return MovementDirection.WEST;
            case MovementDirection.NORTH:
                return MovementDirection.SOUTH;
            case MovementDirection.WEST:
                return MovementDirection.EAST;
            case MovementDirection.SOUTH:
                return MovementDirection.NORTH;
            default:
                return MovementDirection.NONE;
            }
        }

        public static void GetDeltaPixelsFromDirection (MovementDirection d, out int dx, out int dy)
        {
            switch (d) {
            case MovementDirection.EAST:
                dx = 8;
                dy = 0;
                break;
            case MovementDirection.NORTH:
                dx = 0;
                dy = 8;
                break;
            case MovementDirection.WEST:
                dx = -8;
                dy = 0;
                break;
            case MovementDirection.SOUTH:
                dx = 0;
                dy = -8;
                break;
            default:
                dx = 0;
                dy = 0;
                break;
            }
        }

        abstract protected void ReachedStop ();

        virtual public void Update (float dt)
        {
            float units = Speed() * dt;
            MoveInDirection (MoveDir, units);
        }

        int tileFromPixel (float pxPos)
        {
            return Mathf.FloorToInt (pxPos / 8.0f);
        }

        protected void SetStops (MovementDirection dir, float x, float y)
        {
            switch (dir) {
            case MovementDirection.EAST:
                _stopX = 8 * Mathf.CeilToInt((x+0.01f) / 8.0f);
                break;
            case MovementDirection.NORTH:
                _stopY = 8 * Mathf.CeilToInt ((y+0.01f) / 8.0f);
                break;
            case MovementDirection.WEST:
                _stopX = 8 * Mathf.FloorToInt ((x-0.01f) / 8.0f);
                break;
            case MovementDirection.SOUTH:
                _stopY = 8 * Mathf.FloorToInt ((y - 0.01f) / 8.0f);
                break;
            }
        }
    }
}
