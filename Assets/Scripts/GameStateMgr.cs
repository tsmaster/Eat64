using System;
namespace BDG
{
    public class GameStateMgr
    {
        public enum GameState
        {
            BDG_LOGO,
            LOREZJAM_CARD,
            TITLE_CARD,
            MAIN_MENU,
            ABOUT,
            DEDICATION,
            RULES,
            READY,
            SMALL_MAZE,
            BIG_MAZE
        }

        public GameStateMgr ()
        {
        }

        public static GameStateMgr GameStateMgrSingleton {get; set;}
        public GameState CurrentGameState { get; internal set; }
    }
}
