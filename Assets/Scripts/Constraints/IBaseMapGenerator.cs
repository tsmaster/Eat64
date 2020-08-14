using System;
namespace BDG
{
    public interface IBaseMapGenerator
    {
        int GetTileIndex (int x, int y);
        Ghost.GhostName GetSelectedGhost (int i);
    }
}
