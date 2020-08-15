using System;
namespace BDG
{
    public class GameStateMgr
    {
        public enum GameState
        {
            BDG_LOGO,
            TITLE_CARD,
            MAIN_MENU,
            ABOUT,
            READY,
            SMALL_MAZE,
            BIG_MAZE,
            GAME_OVER
        }

        public GameStateMgr ()
        {
        }

        public static GameStateMgr GameStateMgrSingleton {get; set;}
        public GameState CurrentGameState { get; internal set; }
    }
}
