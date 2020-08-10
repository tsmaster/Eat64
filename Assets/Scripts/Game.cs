using System;
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

        [SerializeField]
        Texture2D ghostSpriteSheet;

        Texture2D _displayTexture;

        private int [,] maze = {
            { 3, 9, 9, 6, 6, 9, 9, 2 },
            { 7, 2, 3, 1, 0, 2, 3, 5 },
            {10,10, 7, 9, 9, 5,10,10 },
            {10, 0, 5,11,12, 7, 1,10 },
            { 8, 9, 8, 9, 9, 8, 9, 8 },
            {10, 3, 8, 9, 9, 8, 2,10 },
            { 7, 8, 4, 9, 9, 4, 8, 5 },
            { 0, 4, 9, 9, 9, 9, 4, 1 } };

        PacMan pacMan;

        // known good 2,4
        // good gen 1, 4 - l/r symmetry
        // good gen 2, 2 - no symmetry
        // good gen 2, 3 - l/r symmetry
        // good 0, 1 - no symmetry, got a nullref in random brain?

        // almost good 0, 0  - not connected
        // almost good 0, 4 - ns symmetry, unreachable area
        // almost good 1, 1 - 4x symmetry, unreachable

        int bigMapX = 0;
        int bigMapY = 2;

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
            PacMan.PacManSingleton = pacMan;

            MapManager.MapMgrSingleton = new MapManager ();
            DotManager.DotMgrSingleton = new DotManager (spriteSheet);
            GhostManager.GhostMgrSingleton = new GhostManager (ghostSpriteSheet);
            GameStateMgr.GameStateMgrSingleton = new GameStateMgr {
                CurrentGameState = GameStateMgr.GameState.SMALL_MAZE
            };
            BigMapManager.BigMapMgrSingleton = new BigMapManager ();


            InitializeLevel (bigMapX, bigMapY);

            AddDots ();
            AddGhosts ();
        }

        void InitializeLevel (int bmx, int bmy) {
            MapManager.MapMgrSingleton.ResetTiles ();

            if ((bmx == 2) && (bmy == 4)) {
                Initialize24Level ();
                return;
            }

            var cm = new ConstraintMap (bmx, bmy);
            var didConstrain = cm.Constrain ();

            if (!didConstrain) {
                Debug.LogError ("did not constrain");
            }

            for (int x = 0; x < 8; ++x) {
                for (int y = 0; y < 8; ++y) {
                    var tileIndex = cm.GetTileIndex (x, y);
                    Tile tile = Tile.MakeTile (spriteSheet, tileIndex, x, y);
                    MapManager.MapMgrSingleton.SetTile (x, y, tile);
                }
            }

            MapManager.MapMgrSingleton.MakeDistToHomeValues ();
            Debug.Log ("made distances");
        }

        void AddDots ()
        {
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

        void AddGhosts ()
        {
            GhostManager.GhostMgrSingleton.AddGhost (Ghost.GhostName.BLINKY, 40, 40, Ghost.GhostState.CHASE, 9, -1);
            GhostManager.GhostMgrSingleton.AddGhost (Ghost.GhostName.PINKY, 24, 32, Ghost.GhostState.CAGED, -1, -1);
            GhostManager.GhostMgrSingleton.AddGhost (Ghost.GhostName.INKY, 28, 32, Ghost.GhostState.CAGED, -1, 9);
            GhostManager.GhostMgrSingleton.AddGhost (Ghost.GhostName.CLYDE, 32, 32, Ghost.GhostState.CAGED, 9, 9);
        }

        void Initialize24Level ()
        {
            for (int x = 0; x < 8; ++x) {
                for (int y = 0; y < 8; ++y) {
                    var tileIndex = maze [y, x];
                    Tile tile = Tile.MakeTile (spriteSheet, tileIndex, x, y);
                    MapManager.MapMgrSingleton.SetTile (x, y, tile);
                }
            }

            MapManager.MapMgrSingleton.MakeDistToHomeValues ();
            Debug.Log ("made distances");

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

            GhostManager.GhostMgrSingleton.AddGhost (Ghost.GhostName.BLINKY, 40, 40, Ghost.GhostState.CHASE, 9, -1);
            GhostManager.GhostMgrSingleton.AddGhost (Ghost.GhostName.PINKY, 24, 32, Ghost.GhostState.CAGED, -1, -1);
            GhostManager.GhostMgrSingleton.AddGhost (Ghost.GhostName.INKY, 28, 32, Ghost.GhostState.CAGED, -1, 9);
            GhostManager.GhostMgrSingleton.AddGhost (Ghost.GhostName.CLYDE, 32, 32, Ghost.GhostState.CAGED, 9, 9);
        }



        // Update is called once per frame
        void Update ()
        {
            float dt = Time.deltaTime;

            switch (GameStateMgr.GameStateMgrSingleton.CurrentGameState) {
            case GameStateMgr.GameState.BDG_LOGO:
                break;
            case GameStateMgr.GameState.LOREZJAM_CARD:
                break;
            case GameStateMgr.GameState.TITLE_CARD:
                break;
            case GameStateMgr.GameState.MAIN_MENU:
                break;
            case GameStateMgr.GameState.ABOUT:
                break;
            case GameStateMgr.GameState.DEDICATION:
                break;
            case GameStateMgr.GameState.RULES:
                break;
            case GameStateMgr.GameState.SMALL_MAZE:
                SmallMazeUpdate (dt);
                break;
            case GameStateMgr.GameState.BIG_MAZE:
                BigMazeUpdate (dt);
                break;
            default:
                break;
            }
        }

        bool LeftMap ()
        {
            var pacTile = MapManager.MapMgrSingleton.GetTileForPixel (pacMan.XPos, pacMan.YPos);

            if (pacTile == null) {
                Debug.LogFormat ("LeftMap returning true");
                return true;
            }

            return false;
        }

        MovementDirection MoveBigMap ()
        {
            if (pacMan.XPos >= 64) {
                // move east
                bigMapX += 1;
                return MovementDirection.EAST;
            } else if (pacMan.YPos >= 64) {
                // move north
                bigMapY -= 1;
                return MovementDirection.NORTH;
            } else if (pacMan.XPos <= 0) {
                // move west
                bigMapX -= 1;
                return MovementDirection.WEST;
            } else if (pacMan.YPos <= 0) {
                // move south
                bigMapY += 1;
                return MovementDirection.SOUTH;
            } else {
                Debug.LogWarningFormat ("unknown direction to move on big map: {0} {1}", pacMan.XPos, pacMan.YPos);
                return MovementDirection.NONE;
            }
        }

        void SetBigMapMode ()
        {
            GameStateMgr.GameStateMgrSingleton.CurrentGameState = GameStateMgr.GameState.BIG_MAZE;
        }

        #region SMALL MAZE

        void SmallMazeUpdate (float dt)
        {
            if (LeftMap ()) {
                var moveDir = MoveBigMap ();
                Debug.LogFormat ("Big Map: moved in dir {0} to {1} {2}", moveDir, bigMapX, bigMapY);
                SetBigMapMode ();
                return;
            }

            if (Input.GetKeyDown (KeyCode.LeftArrow)) {
                pacMan.QueueMovementDirection (MovementDirection.WEST);
            } else if (Input.GetKeyDown (KeyCode.RightArrow)) {
                pacMan.QueueMovementDirection (MovementDirection.EAST);
            } else if (Input.GetKeyDown (KeyCode.UpArrow)) {
                pacMan.QueueMovementDirection (MovementDirection.NORTH);
            } else if (Input.GetKeyDown (KeyCode.DownArrow)) {
                pacMan.QueueMovementDirection (MovementDirection.SOUTH);
            }

            pacMan.Update (dt);
            GhostManager.GhostMgrSingleton.Update (dt);
            DotManager.DotMgrSingleton.EatAt (pacMan.XPos, pacMan.YPos);

            // draw

            DrawSmallMaze ();

            DotManager.DotMgrSingleton.Draw (_displayTexture);

            // Pac Man
            pacMan.Draw (_displayTexture);

            GhostManager.GhostMgrSingleton.Draw (_displayTexture);

            _displayTexture.Apply ();
        }

        void DrawSmallMaze ()
        {
            MapManager.MapMgrSingleton.Draw (_displayTexture);
        }

        #endregion // SMALL MAZE

        #region BIG MAZE
        void BigMazeUpdate (float dt)
        {
            DrawBigMaze ();
            _displayTexture.Apply ();
        }

        void DrawBigMaze ()
        {
            Color wallColor = new Color (0.5f, 0.5f, 1.0f);
            Color openColor = new Color (0, 0, 0);

            var window = 13;

            for (int dx = -window; dx <= window; ++dx) {
                int mx = bigMapX + dx;
                for (int dy = -window; dy <= window; ++dy) {
                    int my = bigMapY + dy;

                    var px = 32 + 2 * dx;
                    var py = 32 - 2 * dy;

                    DrawUtil.SetPixel (_displayTexture, px, py, openColor);
                    if ((dx == 0) && (dy == 0)) {
                        DrawUtil.SetPixel (_displayTexture, px, py, new Color(1, 1, 0));
                    }

                    DrawUtil.SetPixel (_displayTexture, px - 1, py - 1, wallColor);
                    DrawUtil.SetPixel (_displayTexture, px - 1, py + 1, wallColor);
                    DrawUtil.SetPixel (_displayTexture, px + 1, py - 1, wallColor);
                    DrawUtil.SetPixel (_displayTexture, px + 1, py + 1, wallColor);

                    Color passageColor;
                    Color dbgColor;

                    if (BigMapManager.BigMapMgrSingleton.CanMove (mx, my, MovementDirection.NORTH, out dbgColor)) {
                        passageColor = openColor;
                    } else {
                        passageColor = wallColor;
                    }
                    DrawUtil.SetPixel (_displayTexture, px, py + 1, passageColor);

                    if (BigMapManager.BigMapMgrSingleton.CanMove (mx, my, MovementDirection.EAST, out dbgColor)) {
                        passageColor = openColor;
                    } else {
                        passageColor = wallColor;
                    }
                    DrawUtil.SetPixel (_displayTexture, px + 1, py, passageColor);

                    if (BigMapManager.BigMapMgrSingleton.CanMove (mx, my, MovementDirection.SOUTH, out dbgColor)) {
                        passageColor = openColor;
                    } else {
                        passageColor = wallColor;
                    }
                    DrawUtil.SetPixel (_displayTexture, px, py - 1, passageColor);

                    if (BigMapManager.BigMapMgrSingleton.CanMove (mx, my, MovementDirection.WEST, out dbgColor)) {
                        passageColor = openColor;
                    } else {
                        passageColor = wallColor;
                    }
                    DrawUtil.SetPixel (_displayTexture, px - 1, py, passageColor);

                }
            }
        }
        #endregion // BIG MAZE

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