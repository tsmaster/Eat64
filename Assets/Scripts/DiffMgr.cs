using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDG
{
    public class DiffMgr
    {
        private List<DifficultyDesc> _diffs;

        public DiffMgr (List<DifficultyDesc> diffs)
        {
            _diffs = diffs;
        }

        public static DiffMgr Singleton { get; set; }
        public Game.Difficulty Difficulty { get; internal set; }

        public DifficultyDesc GetCurDiff ()
        {
            switch (Difficulty) {
            case Game.Difficulty.Easy:
                return _diffs [0];
            case Game.Difficulty.Normal:
                return _diffs [1];
            case Game.Difficulty.Hard:
                return _diffs [2];
            case Game.Difficulty.Extreme:
                return _diffs [3];
            }

            Debug.Assert (false, "unknown diff");
            // should not happen
            return _diffs [0];
        }
    }
}
