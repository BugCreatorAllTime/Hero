using UnityEngine;
using System.Collections;

public class Dungeon
{

    public Dungeon()
    {
    }

    public Dungeon(int piece, int level, int posX, int posY)
    {
        Piece = piece;
        Level = level;
        PosX = posX;
        PosY = posY;
    }

    public int Piece { get; set; }
    public int Level { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }

}
