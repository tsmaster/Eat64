using System;
using UnityEngine;

namespace BDG
{
    [CreateAssetMenu(fileName = "DifficultyDesc", menuName = "BDG/Difficulty Level")]
    public class DifficultyDesc:ScriptableObject
    {
        public bool outerDots;
        public bool middleDots;
        public bool innerDots;

        public int startingLives;

        public float pacManSpeed;

        public float ghostChaseSpeed;
        public float ghostScatterSpeed;
        public float ghostFrightenedSpeed;

        public float ghostChaseDuration;
        public float ghostScatterDuration;
        public float ghostFrightenedDuration;
    }
}
