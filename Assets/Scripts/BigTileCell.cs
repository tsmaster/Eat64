using System;
using System.Collections.Generic;

namespace BDG
{
    internal class BigTileCell
    {
        readonly Dictionary<MovementDirection, bool> _canMoveDict;

        public BigTileCell ()
        {
            _canMoveDict = new Dictionary<MovementDirection, bool> {
                [MovementDirection.EAST] = false,
                [MovementDirection.NORTH] = false,
                [MovementDirection.WEST] = false,
                [MovementDirection.SOUTH] = false
            };
        }

        internal bool CanMove (MovementDirection direction)
        {
            return _canMoveDict [direction];
        }

        public void SetCanMove (MovementDirection dir, bool canMove)
        {
            _canMoveDict [dir] = canMove;
        }
    }
}