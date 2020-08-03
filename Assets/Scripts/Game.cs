using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BDG
{
    public class Game : MonoBehaviour
    {

        public Image displayImage;

        [SerializeField]
        Texture2D spriteSheet;


        Texture2D _displayTexture;

        private int [,] maze = {
            { 3, 9, 9, 6, 6, 9, 9, 2 },
            { 7, 2, 3, 1, 0, 2, 3, 5 },
            {10,10, 7, 6, 6, 5,10,10 },
            {10, 0, 5,11,12, 7, 1,10 },
            { 8, 9, 8, 9, 9, 8, 9, 8 },
            {10, 3, 8, 9, 9, 8, 2,10 },
            { 7, 8, 4, 9, 9, 4, 8, 5 },
            { 0, 4, 9, 9, 9, 9, 4, 1 } };

        PacMan pacMan;

        // Start is called before the first frame update
        void Start ()
        {
            _displayTexture = new Texture2D (64, 64) {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            Sprite sprite = Sprite.Create (_displayTexture, new Rect (0, 0, 64, 64), Vector2.zero);
            displayImage.sprite = sprite;

            pacMan = new PacMan (spriteSheet,
                28, 8);

            MapManager.MapMgrSingleton = new MapManager ();
            DotManager.DotMgrSingleton = new DotManager (spriteSheet);

            for (int x = 0; x < 8; ++x) {
                for (int y = 0; y < 8; ++y) {
                    var tileIndex = maze [y, x];
                    Tile tile = Tile.MakeTile (spriteSheet, tileIndex);
                    MapManager.MapMgrSingleton.SetTile (x, y, tile);
                }
            }

            // pellet test

            for (int x = 8; x <= 48; x += 8) {
                DotManager.DotMgrSingleton.AddDot (0, x, false);
                DotManager.DotMgrSingleton.AddDot (56, x, false);
                DotManager.DotMgrSingleton.AddDot (x, 0, false);
                DotManager.DotMgrSingleton.AddDot (x, 56, false);
            }

            DotManager.DotMgrSingleton.AddDot (0, 0, true);
            DotManager.DotMgrSingleton.AddDot (0, 56, true);
            DotManager.DotMgrSingleton.AddDot (56, 0, true);
            DotManager.DotMgrSingleton.AddDot (56, 56, true);
        }

        // Update is called once per frame
        void Update ()
        {
            if (Input.GetKeyDown (KeyCode.LeftArrow)) {
                pacMan.QueueMovementDirection (MovementDirection.WEST);
            }
            else if (Input.GetKeyDown (KeyCode.RightArrow)) {
                pacMan.QueueMovementDirection (MovementDirection.EAST);
            } else if (Input.GetKeyDown (KeyCode.UpArrow)) {
                pacMan.QueueMovementDirection (MovementDirection.NORTH);
            } else if (Input.GetKeyDown (KeyCode.DownArrow)) {
                pacMan.QueueMovementDirection (MovementDirection.SOUTH);
            }

            float dt = Time.deltaTime;
            pacMan.Update (dt);

            DotManager.DotMgrSingleton.EatAt (pacMan.XPos, pacMan.YPos);

            DrawMaze ();

            // Pac Man
            pacMan.Draw (_displayTexture);

            // Blinky (red)
            DrawSpriteAlpha (40, 8, 8, 8, 27, 40);

            // Pinky (pink)
            DrawSpriteAlpha (48, 8, 8, 8, 24, 32);

            // Inky (blue)
            DrawSpriteAlpha (56, 8, 8, 8, 28, 32);

            // Clyde (orange)
            DrawSpriteAlpha (0, 0, 8, 8, 32, 32);

            DotManager.DotMgrSingleton.Draw (_displayTexture);

            _displayTexture.Apply ();
        }

        void DrawMaze ()
        {
            MapManager.MapMgrSingleton.Draw (_displayTexture);
        }

        void DrawSpriteOpaque (int spriteX, int spriteY, int spriteWidth, int spriteHeight,
            int destX, int destY)
        {
            DrawUtil.DrawSpriteOpaque (spriteSheet, _displayTexture,
                spriteX, spriteY, spriteWidth, spriteHeight, destX, destY);
        }

        void DrawSpriteAlpha (int spriteX, int spriteY, int spriteWidth, int spriteHeight,
            int destX, int destY)
        {
            DrawUtil.DrawSpriteAlpha (spriteSheet, _displayTexture,
                spriteX, spriteY, spriteWidth, spriteHeight, destX, destY);
        }
    }
}