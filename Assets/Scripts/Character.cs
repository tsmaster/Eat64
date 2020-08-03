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

        protected float Speed { get; set; } // pixels per second
        private int _stopX;
        private int _stopY;
        protected MovementDirection MoveDir { get; set; }

        public Character (Texture2D spritesheet, int width, int height, 
            int xPos, int yPos)
        {
            _spritesheet = spritesheet;
            XPos = xPos;
            YPos = yPos;

            _width = width;
            _height = height;

            Speed = 2.0f;
        }

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

        abstract protected void ReachedStop ();

        virtual public void Update (float dt)
        {
            float units = Speed * dt;
            MoveInDirection (MoveDir, units);
        }

        int tileFromPixel (float pxPos)
        {
            return Mathf.FloorToInt (pxPos / 8.0f);
        }

        protected void SetStops (MovementDirection dir, float x, float y)
        {
            int ix = 8 * tileFromPixel (x);
            int iy = 8 * tileFromPixel (y);

            switch (dir) {
            case MovementDirection.EAST:
                _stopX = ix + 8;
                break;
            case MovementDirection.NORTH:
                _stopY = iy + 8;
                break;
            case MovementDirection.WEST:
                _stopX = ix - 8;
                break;
            case MovementDirection.SOUTH:
                _stopY = iy - 8;
                break;
            }
        }
    }
}
