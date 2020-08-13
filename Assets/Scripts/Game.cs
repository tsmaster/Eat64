﻿using System;
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

        [SerializeField]
        Texture2D titleTexture;

        [SerializeField]
        Texture2D livesTexture;

        [SerializeField]
        Texture2D numbersTexture;

        [SerializeField]
        AudioClip eatDotSound;

        [SerializeField]
        AudioClip eatEnergizerSound;

        [SerializeField]
        AudioClip eatGhostSound;

        [SerializeField]
        AudioClip eatPacManSound;

        [SerializeField]
        AudioClip clearLevelSound;

        Texture2D _displayTexture;

        public enum Difficulty
        {
            Easy,
            Normal,
            Hard,
            Extreme
        }

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

        // good 0, 0
        // good 0, 1 - no symmetry, got a nullref in random brain?
        // good 0, 2

        // good gen 1, 4 - l/r symmetry
        // good gen 2, 2 - no symmetry
        // good gen 2, 3 - l/r symmetry

        // almost good 0, 4 - ns symmetry, unreachable area
        // almost good 1, 1 - 4x symmetry, unreachable

        int bigMapX = 0;
        int bigMapY = 4;
        private float _bigMazeElapsed;

        private float _titleElapsed;
        private float _titleDuration = 3.0f;
        private const float _bigMazeLength = 2.5f;
        private const int _maxPacManLives = 9;
        private int _curPacManLives = 3;
        private float _readyElapsed;
        private const float _readyDuration = 2.0f;

        private bool _hasCompletedLevel = false;
        private Difficulty _difficulty = Difficulty.Normal;

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

            SoundMgr.Singleton = new SoundMgr ();
            AudioSource audioSource = GetComponent<AudioSource> ();
            SoundMgr.Singleton.SetAudioSource (audioSource);
            SoundMgr.Singleton.AddEffect (SoundMgr.Sound.EatDot, eatDotSound);
            SoundMgr.Singleton.AddEffect (SoundMgr.Sound.EatEnergizer, eatEnergizerSound);
            SoundMgr.Singleton.AddEffect (SoundMgr.Sound.EatGhost, eatGhostSound);
            SoundMgr.Singleton.AddEffect (SoundMgr.Sound.EatPacMan, eatPacManSound);
            SoundMgr.Singleton.AddEffect (SoundMgr.Sound.ClearLevel, clearLevelSound);

            SoundMgr.Singleton.IsOn = true;
            MapManager.MapMgrSingleton = new MapManager ();
            DotManager.DotMgrSingleton = new DotManager (spriteSheet);
            GhostManager.GhostMgrSingleton = new GhostManager (ghostSpriteSheet);
            GameStateMgr.GameStateMgrSingleton = new GameStateMgr ();
            SetGameState (GameStateMgr.GameState.TITLE_CARD);

            BigMapManager.BigMapMgrSingleton = new BigMapManager ();
        }

        void StartGame (int x, int y)
        {
            bigMapX = x;
            bigMapY = y;
            InitializeLevel (bigMapX, bigMapY);
            StartPacManLives ();
            ResetPacMan ();
            SetGameState (GameStateMgr.GameState.READY);
            BigMapManager.BigMapMgrSingleton.ResetClearedList ();
        }

        private void StartPacManLives ()
        {
            switch (_difficulty) {
            case Difficulty.Easy:
                _curPacManLives = 5;
                break;
            case Difficulty.Normal:
                _curPacManLives = 3;
                break;
            case Difficulty.Hard:
                _curPacManLives = 2;
                break;
            case Difficulty.Extreme:
                _curPacManLives = 1;
                break;
            }
        }

        void InitializeLevel (int bmx, int bmy) {
            MapManager.MapMgrSingleton.ResetTiles ();
            GhostManager.GhostMgrSingleton.Clear ();
            DotManager.DotMgrSingleton.Clear ();
            pacMan.SetPos (28, 8);
            pacMan.Stop ();

            if ((bmx == 2) && (bmy == 4)) {
                Initialize24Level ();
            } else {
                InitializeConstrainedLevel (bmx, bmy);
            }

            if (!BigMapManager.BigMapMgrSingleton.GetHasCleared (bmx, bmy)) {
                AddDots ();
                AddGhosts ();
                _hasCompletedLevel = false;
            } else {
                _hasCompletedLevel = true;
            }
        }

        void AddDots ()
        {
            for (int x = 8; x <= 48; x += 8) {
                DotManager.DotMgrSingleton.AddDot (0, x, false);
                DotManager.DotMgrSingleton.AddDot (56, x, false);
                DotManager.DotMgrSingleton.AddDot (x, 0, false);
                DotManager.DotMgrSingleton.AddDot (x, 56, false);
            }

            if (_difficulty != Difficulty.Easy) {
                for (int x = 16; x <= 40; x += 8) {
                    DotManager.DotMgrSingleton.AddDot (8, x, false);
                    DotManager.DotMgrSingleton.AddDot (48, x, false);
                    DotManager.DotMgrSingleton.AddDot (x, 8, false);
                    DotManager.DotMgrSingleton.AddDot (x, 48, false);
                }
                DotManager.DotMgrSingleton.AddDot (8, 8, false);
                DotManager.DotMgrSingleton.AddDot (48, 8, false);
                DotManager.DotMgrSingleton.AddDot (8, 48, false);
                DotManager.DotMgrSingleton.AddDot (48, 48, false);
            }

            if ((_difficulty == Difficulty.Hard) ||
                (_difficulty == Difficulty.Extreme)) {
                for (int x = 24; x <= 32; x += 8) {
                    DotManager.DotMgrSingleton.AddDot (16, x, false);
                    DotManager.DotMgrSingleton.AddDot (40, x, false);
                    DotManager.DotMgrSingleton.AddDot (x, 16, false);
                    DotManager.DotMgrSingleton.AddDot (x, 40, false);
                }
                DotManager.DotMgrSingleton.AddDot (16, 16, false);
                DotManager.DotMgrSingleton.AddDot (40, 16, false);
                DotManager.DotMgrSingleton.AddDot (16, 40, false);
                DotManager.DotMgrSingleton.AddDot (40, 40, false);
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

        void ResetPacMan ()
        {
            pacMan.IsAlive = true;
            pacMan.SetPos (28, 8);
            pacMan.MoveDir = MovementDirection.NONE;
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
            //Debug.Log ("made distances");
        }

        void InitializeConstrainedLevel (int bmx, int bmy)
        {
            var cm = new ConstraintMap (bmx, bmy);
            IBaseMapGenerator mapGen = cm;
            var didConstrain = cm.Constrain ();

            if (!didConstrain) {
                Debug.LogWarning ("did not constrain");
                mapGen = new FallbackMap (bmx, bmy);
            }

            for (int x = 0; x < 8; ++x) {
                for (int y = 0; y < 8; ++y) {
                    var tileIndex = mapGen.GetTileIndex (x, y);
                    Tile tile = Tile.MakeTile (spriteSheet, tileIndex, x, y);
                    MapManager.MapMgrSingleton.SetTile (x, y, tile);
                }
            }

            MapManager.MapMgrSingleton.MakeDistToHomeValues ();
            //Debug.Log ("made distances");
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
                UpdateTitle (dt);
                break;
            case GameStateMgr.GameState.MAIN_MENU:
                UpdateMainMenu (dt);
                break;
            case GameStateMgr.GameState.ABOUT:
                break;
            case GameStateMgr.GameState.DEDICATION:
                break;
            case GameStateMgr.GameState.RULES:
                break;
            case GameStateMgr.GameState.SMALL_MAZE:
                UpdateSmallMaze (dt);
                break;
            case GameStateMgr.GameState.BIG_MAZE:
                UpdateBigMaze (dt);
                break;
            case GameStateMgr.GameState.READY:
                UpdateReady (dt);
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
            Debug.LogFormat ("moving map based on pac man pos {0} {1}", pacMan.XPos, pacMan.YPos);
            Debug.LogFormat ("old bmpos {0} {1}", bigMapX, bigMapY);
            if (pacMan.XPos >= 64) {
                // move east
                Debug.LogFormat ("moving east");
                bigMapX += 1;
                return MovementDirection.EAST;
            } else if (pacMan.YPos >= 64) {
                // move north
                Debug.LogFormat ("moving north");
                bigMapY -= 1;
                return MovementDirection.NORTH;
            } else if (pacMan.XPos < 0) {
                // move west
                Debug.LogFormat ("moving west");
                bigMapX -= 1;
                return MovementDirection.WEST;
            } else if (pacMan.YPos < 0) {
                // move south
                Debug.LogFormat ("moving south");
                bigMapY += 1;
                return MovementDirection.SOUTH;
            } else {
                Debug.LogWarningFormat ("unknown direction to move on big map: {0} {1}", pacMan.XPos, pacMan.YPos);
                return MovementDirection.NONE;
            }
        }

        #region SMALL MAZE

        void UpdateSmallMaze (float dt)
        {
            if (pacMan.IsAlive) {
                if (LeftMap ()) {
                    var moveDir = MoveBigMap ();
                    Debug.LogFormat ("Big Map: moved in dir {0} to {1} {2}", moveDir, bigMapX, bigMapY);
                    SetGameState (GameStateMgr.GameState.BIG_MAZE);
                    return;
                }

                if ((!_hasCompletedLevel) &&
                    (DotManager.DotMgrSingleton.IsCleared ())) {
                    _hasCompletedLevel = true;
                    ExtraLife ();
                    BigMapManager.BigMapMgrSingleton.SetHasCleared (bigMapX, bigMapY);
                    SoundMgr.Singleton.Play (SoundMgr.Sound.ClearLevel);
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

                if (Input.GetKeyDown (KeyCode.X)) {
                    DotManager.DotMgrSingleton.Clear ();
                }

                if (Input.GetKeyDown (KeyCode.S)) {
                    SoundMgr.Singleton.ToggleOn ();
                }


                pacMan.Update (dt);
                GhostManager.GhostMgrSingleton.Update (dt);
            } else {
                _curPacManLives--;
                if (_curPacManLives <= 0) {
                    // TODO probably GAME OVER
                    SetGameState (GameStateMgr.GameState.MAIN_MENU);
                } else {
                    GhostManager.GhostMgrSingleton.SendGhostsHome ();
                    ResetPacMan ();
                    SetGameState (GameStateMgr.GameState.READY);
                }
            }


            DotManager.DotMgrSingleton.EatAt (pacMan.XPos, pacMan.YPos);

            // draw

            DrawSmallMaze ();

            DotManager.DotMgrSingleton.Draw (_displayTexture);

            // Pac Man
            pacMan.Draw (_displayTexture);

            GhostManager.GhostMgrSingleton.Draw (_displayTexture);

            _displayTexture.Apply ();
        }

        private void ExtraLife ()
        {
            _curPacManLives = Math.Min (9, _curPacManLives + 1);
        }

        void DrawSmallMaze ()
        {
            MapManager.MapMgrSingleton.Draw (_displayTexture);
        }

        #endregion // SMALL MAZE

        #region BIG MAZE
        void UpdateBigMaze (float dt)
        {
            _bigMazeElapsed += dt;

            DrawBigMaze ();
            _displayTexture.Apply ();

            if (_bigMazeElapsed >= _bigMazeLength) {
                InitializeLevel (bigMapX, bigMapY);
                SetGameState (GameStateMgr.GameState.READY);
            }
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

                    if (BigMapManager.BigMapMgrSingleton.GetHasCleared (mx, my)) {
                        DrawUtil.SetPixel (_displayTexture, px, py, new Color (0, 1, 0));
                    }

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


        #region TITLE
        void UpdateTitle (float dt)
        {
            _titleElapsed += dt;
            if (_titleElapsed >= _titleDuration) {
                SetGameState (GameStateMgr.GameState.MAIN_MENU);
                return;
            }
            DrawTitle ();
        }
        void DrawTitle ()
        {
            DrawUtil.DrawSpriteOpaque (titleTexture, _displayTexture, 0, 0, 64, 64, 0, 0);
            _displayTexture.Apply ();
        }
        #endregion // TITLE

        #region MAIN_MENU
        void UpdateMainMenu (float dt)
        {
            // TODO add selection here

            DrawMainMenu ();

            // TODO remove this
            StartGame (2, 4);
        }

        void DrawMainMenu ()
        {
        }
        #endregion // MAIN_MENU

        #region READY
        void UpdateReady (float dt)
        {
            _readyElapsed += dt;

            if (_readyElapsed >= _readyDuration) {
                SetGameState (GameStateMgr.GameState.SMALL_MAZE);
            }
            DrawReady ();
        }

        void DrawReady ()
        {
            DrawSmallMaze ();

            // draw number of lives remaining
            DrawUtil.DrawSpriteAlpha (numbersTexture, _displayTexture, 7*_curPacManLives, 0, 7, 7, 20, 30);
            DrawUtil.DrawSpriteAlpha (livesTexture, _displayTexture, 0, 0, 31, 7, 30, 30);
            _displayTexture.Apply ();
        }
        #endregion // READY

        private void SetGameState (GameStateMgr.GameState state)
        {
            var oldState = GameStateMgr.GameStateMgrSingleton.CurrentGameState;
            GameStateMgr.GameStateMgrSingleton.CurrentGameState = state;

            switch (state) {
            case GameStateMgr.GameState.TITLE_CARD:
                _titleElapsed = 0.0f;
                break;
            case GameStateMgr.GameState.BDG_LOGO:
                break;
            case GameStateMgr.GameState.LOREZJAM_CARD:
                break;
            case GameStateMgr.GameState.MAIN_MENU:
                //_mainMenu.Reset ();
                break;
            case GameStateMgr.GameState.ABOUT:
                break;
            case GameStateMgr.GameState.DEDICATION:
                break;
            case GameStateMgr.GameState.RULES:
                break;
            case GameStateMgr.GameState.READY:
                _readyElapsed = 0.0f;
                break;
            case GameStateMgr.GameState.SMALL_MAZE:
                break;
            case GameStateMgr.GameState.BIG_MAZE:
                _bigMazeElapsed = 0.0f;
                break;
            }
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