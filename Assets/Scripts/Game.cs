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
        Texture2D wizardTexture;

        [SerializeField]
        Texture2D titleTexture;

        [SerializeField]
        Texture2D menuTexture;

        [SerializeField]
        Texture2D diffTexture;

        [SerializeField]
        Texture2D livesTexture;

        [SerializeField]
        Texture2D numbersTexture;

        [SerializeField]
        Texture2D widgetsTexture;

        [SerializeField]
        Texture2D aboutTexture;

        [SerializeField]
        Texture2D gameOverTexture;

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

        [SerializeField]
        List<DifficultyDesc> difficulties;

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
        private int _menuCursorRow;
        private float _menuCursorElapsed;
        private float _wizardElapsed;
        private float _wizardDuration = 2.0f;
        private float _aboutElapsed;

        private List<Ghost.GhostName> _paradeGhostNames;
        private float _paradeSpeed = 8.0f;
        private int _paradeSpacing = 8;
        private List<Ghost> _paradeGhosts;
        private float _gameOverElapsed;
        private float _gameOverDuration = 3.0f;

        // Start is called before the first frame update
        void Start ()
        {
            _displayTexture = new Texture2D (64, 64) {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            Sprite sprite = Sprite.Create (_displayTexture, new Rect (0, 0, 64, 64), Vector2.zero);
            displayImage.sprite = sprite;

            DiffMgr.Singleton = new DiffMgr (difficulties);
            DiffMgr.Singleton.Difficulty = Difficulty.Normal;

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
            SetGameState (GameStateMgr.GameState.BDG_LOGO);

            BigMapManager.BigMapMgrSingleton = new BigMapManager ();
        }

        void StartGame (int x, int y)
        {
            BigMapManager.BigMapMgrSingleton.ResetClearedList ();
            bigMapX = x;
            bigMapY = y;
            InitializeLevel (bigMapX, bigMapY);
            StartPacManLives ();
            ResetPacMan ();
            SetGameState (GameStateMgr.GameState.READY);
        }

        private void StartPacManLives ()
        {
            _curPacManLives = DiffMgr.Singleton.GetCurDiff ().startingLives;
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
            if (DiffMgr.Singleton.GetCurDiff ().outerDots) {
                for (int x = 8; x <= 48; x += 8) {
                    DotManager.DotMgrSingleton.AddDot (0, x, false);
                    DotManager.DotMgrSingleton.AddDot (56, x, false);
                    DotManager.DotMgrSingleton.AddDot (x, 0, false);
                    DotManager.DotMgrSingleton.AddDot (x, 56, false);
                }
            }

            if (DiffMgr.Singleton.GetCurDiff().middleDots) {
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

            if (DiffMgr.Singleton.GetCurDiff ().innerDots) {
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
            Ghost.GhostName g0 = MapManager.MapMgrSingleton.GetGhost (0);
            Ghost.GhostName g1 = MapManager.MapMgrSingleton.GetGhost (1);
            Ghost.GhostName g2 = MapManager.MapMgrSingleton.GetGhost (2);
            Ghost.GhostName g3 = MapManager.MapMgrSingleton.GetGhost (3);

            GhostManager.GhostMgrSingleton.AddGhost (g0, 40, 40, Ghost.GhostState.CHASE, 9, -1);
            GhostManager.GhostMgrSingleton.AddGhost (g1, 24, 32, Ghost.GhostState.CAGED, -1, -1);
            GhostManager.GhostMgrSingleton.AddGhost (g2, 28, 32, Ghost.GhostState.CAGED, -1, 9);
            GhostManager.GhostMgrSingleton.AddGhost (g3, 32, 32, Ghost.GhostState.CAGED, 9, 9);
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
            MapManager.MapMgrSingleton.SetGhost (0, Ghost.GhostName.BLINKY);
            MapManager.MapMgrSingleton.SetGhost (1, Ghost.GhostName.PINKY);
            MapManager.MapMgrSingleton.SetGhost (2, Ghost.GhostName.INKY);
            MapManager.MapMgrSingleton.SetGhost (3, Ghost.GhostName.CLYDE);
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

            //MapManager.MapMgrSingleton.SetGhost (0, Ghost.GhostName.JAMAAL);
            //MapManager.MapMgrSingleton.SetGhost (0, Ghost.GhostName.LEFTY);
            //MapManager.MapMgrSingleton.SetGhost (0, Ghost.GhostName.RIGHTY);
            //MapManager.MapMgrSingleton.SetGhost (0, Ghost.GhostName.BLINKY);
            //MapManager.MapMgrSingleton.SetGhost (1, Ghost.GhostName.LILBRO);
            //MapManager.MapMgrSingleton.SetGhost (1, Ghost.GhostName.INTERCEPTOR);
            //MapManager.MapMgrSingleton.SetGhost (0, Ghost.GhostName.HILBERT);

            for (int i = 0; i < 4; ++i) {
                Ghost.GhostName gn = mapGen.GetSelectedGhost (i);
                Debug.LogFormat ("Assigning {0} {1}", i, gn);
                MapManager.MapMgrSingleton.SetGhost (i, gn);
            }
            //Debug.Log ("made distances");
        }

        // Update is called once per frame
        void Update ()
        {
            float dt = Time.deltaTime;

            switch (GameStateMgr.GameStateMgrSingleton.CurrentGameState) {
            case GameStateMgr.GameState.BDG_LOGO:
                UpdateWizard (dt);
                break;
            case GameStateMgr.GameState.TITLE_CARD:
                UpdateTitle (dt);
                break;
            case GameStateMgr.GameState.MAIN_MENU:
                UpdateMainMenu (dt);
                break;
            case GameStateMgr.GameState.ABOUT:
                UpdateAbout (dt);
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
            case GameStateMgr.GameState.GAME_OVER:
                UpdateGameOver (dt);
                break;
            default:
                break;
            }
        }

        #region GAMEOVER
        private void UpdateGameOver (float dt)
        {
            _gameOverElapsed += dt;
            if (_gameOverElapsed >= _gameOverDuration) {
                SetGameState (GameStateMgr.GameState.MAIN_MENU);
            }

            DrawGameOver ();
        }

        private void DrawGameOver ()
        {
            DrawSmallMaze ();
            DrawUtil.DrawSpriteAlpha (gameOverTexture, _displayTexture, 0, 0, 25, 15, 20, 25);
            _displayTexture.Apply ();
        }

        #endregion // GAMEOVER

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

                if (Input.GetKeyDown (KeyCode.Escape)) {
                    SetGameState (GameStateMgr.GameState.GAME_OVER);
                }



                pacMan.Update (dt);
                GhostManager.GhostMgrSingleton.Update (dt);
            } else {
                _curPacManLives--;
                if (_curPacManLives <= 0) {
                    SetGameState (GameStateMgr.GameState.GAME_OVER);
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

            // Debug display
            //DrawUtil.SetPixel (_displayTexture, Ghost.DBG_x, Ghost.DBG_y, new Color (0, 1, 0));

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
            DrawUtil.FillTexture (_displayTexture, new Color (0.25f, 0.25f, 0.75f));

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

        #region WIZARD
        void UpdateWizard (float dt)
        {
            _wizardElapsed += dt;
            if (_wizardElapsed >= _wizardDuration) {
                SetGameState (GameStateMgr.GameState.TITLE_CARD);
                return;
            }
            DrawWizard ();
        }
        void DrawWizard ()
        {
            DrawUtil.DrawSpriteOpaque (wizardTexture, _displayTexture, 0, 0, 64, 64, 0, 0);
            _displayTexture.Apply ();
        }
        #endregion // WIZARD

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
            DrawUtil.FillTexture (_displayTexture, new Color (0, 0, 0));
            DrawUtil.DrawSpriteOpaque (titleTexture, _displayTexture, 0, 0, 64, 64, 4, 24);
            _displayTexture.Apply ();
        }
        #endregion // TITLE

        #region ABOUT
        void UpdateAbout (float dt)
        {
            _aboutElapsed += dt;

            var frontPos = _aboutElapsed * _paradeSpeed;
            var backPos = frontPos - 5 * _paradeSpacing;

            if ((backPos >= 64) || (Input.anyKeyDown)) {
                SetGameState (GameStateMgr.GameState.MAIN_MENU);
                return;
            }

            UpdateParade (dt);

            DrawAbout ();
        }

        private void UpdateParade (float dt)
        {
            var advance = dt * _paradeSpeed;

            foreach (var g in _paradeGhosts) {
                if (g.MoveDir == MovementDirection.EAST) {
                    g.XPos += advance;
                } else {
                    g.XPos -= advance;
                }
            }
        }

        void DrawAbout ()
        {
            DrawUtil.FillTexture (_displayTexture, new Color (0, 0, 0));
            DrawUtil.DrawSpriteOpaque (aboutTexture, _displayTexture, 0, 0, 64, 64, 0, 0);

            DrawParade ();
            _displayTexture.Apply ();
        }

        private void DrawParade ()
        {
            foreach (var g in _paradeGhosts) {
                if ((g.XPos >= 0) &&
                    (g.XPos <= 52)) {
                    g.Draw (_displayTexture);
                }
            }
        }
        #endregion // ABOUT


        #region MAIN_MENU
        void UpdateMainMenu (float dt)
        {
            _menuCursorElapsed += dt;
            DrawMainMenu ();

            if ((Input.GetKeyDown (KeyCode.Space)) ||
                (Input.GetKeyDown (KeyCode.Return))) {

                switch (_menuCursorRow) {
                case 0:
                    // go!
                    StartGame (2, 4);
                    break;
                case 1:
                    // diff
                    Difficulty d = DiffMgr.Singleton.Difficulty;

                    if (d == Difficulty.Extreme) {
                        DiffMgr.Singleton.Difficulty = Difficulty.Easy;
                    } else {
                        IncreaseDifficulty ();
                    }
                    break;
                case 2:
                    // sound
                    SoundMgr.Singleton.ToggleOn ();
                    break;
                case 3:
                    // about
                    SetGameState (GameStateMgr.GameState.ABOUT);
                    break;
                }

            }

            if (Input.GetKeyDown (KeyCode.LeftArrow)) {
                switch (_menuCursorRow) {
                case 0:
                    // go!
                    // do nothing?
                    break;
                case 1:
                    // diff
                    DecreaseDifficulty ();
                    break;
                case 2:
                    // sound
                    SoundMgr.Singleton.IsOn = false;
                    break;
                case 3:
                    // about
                    // do nothing
                    break;
                }
            } else if (Input.GetKeyDown (KeyCode.RightArrow)) {
                switch (_menuCursorRow) {
                case 0:
                    // go!
                    StartGame (2, 4);
                    break;
                case 1:
                    // diff
                    IncreaseDifficulty ();
                    break;
                case 2:
                    // sound
                    SoundMgr.Singleton.IsOn = true;
                    break;
                case 3:
                    // about
                    SetGameState (GameStateMgr.GameState.ABOUT);
                    break;
                }
            }

            if (Input.GetKeyDown (KeyCode.UpArrow)) {
                MoveMenuCursorUp ();
            } else if (Input.GetKeyDown (KeyCode.DownArrow)) {
                MoveMenuCursorDown ();
            }
        }

        private void MoveMenuCursorDown ()
        {
            _menuCursorRow = Math.Min(3, _menuCursorRow + 1);
        }

        private void MoveMenuCursorUp ()
        {
            _menuCursorRow = Math.Max (0, _menuCursorRow - 1);
        }

        private void IncreaseDifficulty ()
        {
            Difficulty d = DiffMgr.Singleton.Difficulty;
            switch (d) {
            case Difficulty.Easy:
                DiffMgr.Singleton.Difficulty = Difficulty.Normal;
                break;
            case Difficulty.Normal:
                DiffMgr.Singleton.Difficulty = Difficulty.Hard;
                break;
            case Difficulty.Hard:
                DiffMgr.Singleton.Difficulty = Difficulty.Extreme;
                break;
            case Difficulty.Extreme:
                break;
            }
        }

        private void DecreaseDifficulty ()
        {
            Difficulty d = DiffMgr.Singleton.Difficulty;
            switch (d) {
            case Difficulty.Easy:
                break;
            case Difficulty.Normal:
                DiffMgr.Singleton.Difficulty = Difficulty.Easy;
                break;
            case Difficulty.Hard:
                DiffMgr.Singleton.Difficulty = Difficulty.Normal;
                break;
            case Difficulty.Extreme:
                DiffMgr.Singleton.Difficulty = Difficulty.Hard;
                break;
            }
        }

        void DrawMainMenu ()
        {
            DrawUtil.FillTexture (_displayTexture, new Color (0, 0, 0));
            DrawUtil.DrawSpriteAlpha (menuTexture, _displayTexture, 0, 0, 64, 64, 0, 0);

            int soundOff = 0;
            if (SoundMgr.Singleton.IsOn) {
                soundOff = 2;
            } else {
                soundOff = 1;
            }
            DrawUtil.DrawSpriteAlpha (widgetsTexture, _displayTexture, 0, 7 * soundOff, 30, 7, 34, 14);

            int dispOff = 0;
            switch (DiffMgr.Singleton.Difficulty) {
            case Difficulty.Easy:
                dispOff = 3;
                break;
            case Difficulty.Normal:
                dispOff = 2;
                break;
            case Difficulty.Hard:
                dispOff = 1;
                break;
            case Difficulty.Extreme:
                dispOff = 0;
                break;
            }
            DrawUtil.DrawSpriteAlpha (diffTexture, _displayTexture, 0, 7 * dispOff, 25, 7, 28, 21);

            DrawMenuCursor ();
            _displayTexture.Apply ();
        }

        private void DrawMenuCursor ()
        {
            var cursorY = 0;
            bool fancy = false;
            switch (_menuCursorRow) {
            case 0: // play
                cursorY = 34;
                fancy = true;
                break;
            case 1: // diff
                cursorY = 21;
                break;
            case 2: // sound
                cursorY = 14;
                break;
            case 3: // about
                cursorY = 4;
                break;
            }

            if (!fancy) {
                DrawUtil.DrawSpriteAlpha (widgetsTexture, _displayTexture, 0, 0, 7, 7, 0, cursorY);
            } else {
                var _menuCursorLoopLength = 0.85f;
                var wrap = _menuCursorElapsed / _menuCursorLoopLength;
                var m01 = wrap - Mathf.FloorToInt (wrap);
                var width = 12;
                var steps = Mathf.FloorToInt (width * m01);

                DrawUtil.DrawSpriteAlpha (widgetsTexture, _displayTexture, 0, 0, 7, 7, 20 - width + steps, cursorY);
                DrawUtil.DrawSpriteAlpha (widgetsTexture, _displayTexture, 7, 0, 7, 7, 39 + width - steps, cursorY);
            }
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
            DrawUtil.DrawSpriteAlpha (numbersTexture, _displayTexture, 7*_curPacManLives, 0, 7, 7, 13, 30);
            DrawUtil.DrawSpriteAlpha (livesTexture, _displayTexture, 0, 0, 31, 7, 21, 30);
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
                _wizardElapsed = 0.0f;
                break;
            case GameStateMgr.GameState.MAIN_MENU:
                //_mainMenu.Reset ();
                _menuCursorRow = 0;
                _menuCursorElapsed = 0.0f;
                break;
            case GameStateMgr.GameState.ABOUT:
                _aboutElapsed = 0.0f;
                StartParade ();
                break;
            case GameStateMgr.GameState.READY:
                _readyElapsed = 0.0f;
                break;
            case GameStateMgr.GameState.SMALL_MAZE:
                break;
            case GameStateMgr.GameState.BIG_MAZE:
                _bigMazeElapsed = 0.0f;
                break;
            case GameStateMgr.GameState.GAME_OVER:
                _gameOverElapsed = 0.0f;
                break;
            }
        }

        private void StartParade ()
        {
            _paradeGhostNames = new List<Ghost.GhostName> {
                Ghost.GhostName.BLINKY,
                Ghost.GhostName.PINKY,
                Ghost.GhostName.INKY,
                Ghost.GhostName.CLYDE,

                Ghost.GhostName.JAMAAL,
                Ghost.GhostName.LEFTY,
                Ghost.GhostName.RIGHTY,
                Ghost.GhostName.INTERCEPTOR,

                Ghost.GhostName.QUADDY,
                Ghost.GhostName.CLOCKY,
                Ghost.GhostName.HILBERT,
                Ghost.GhostName.LILBRO
            };

            MathUtil.Shuffle (_paradeGhostNames, new System.Random ());

            int topRow = 28;
            int bottomRow = 15;

            _paradeGhosts = new List<Ghost> ();
            for (int i = 0; i < 12; ++i) {
                var gn = _paradeGhostNames [i];
                var placeInLine = i / 2;

                int pos = -(placeInLine * _paradeSpacing);

                if (i % 2 == 0) {
                    // top

                    var g = new Ghost (ghostSpriteSheet, gn, pos, topRow, Ghost.GhostState.CAGED) {
                        MoveDir = MovementDirection.EAST
                    };
                    _paradeGhosts.Add (g);
                } else {
                    // bottom

                    var g = new Ghost (ghostSpriteSheet, gn, 64 - pos, bottomRow, Ghost.GhostState.CAGED) {
                        MoveDir = MovementDirection.WEST
                    };
                    _paradeGhosts.Add (g);
                }
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