using System;

namespace ForestSolverPackages
{
    public class Hello
    {
        public bool IsVisualizator;
        public string Name;
    }

    #region Работа с игроком

    public class ClientInfo
    {
        public Point MapSize; // x - height, y - width
        public int Hp;
        public Point StartPosition;
        public Point Target;
        public int[,] VisibleMap; // видимая часть карты в начале игры. 
    }

    public class Move
    {
        public int Direction;
    }

    public class MoveResultInfo
    {
        public int Result; // 2 -- GameOver.
        public int[,] VisibleMap;
    }

    #endregion

    #region Работа с визуализатором


    public class WorldInfo
    {
        public Player[] Players;
        public int[,] Map;
    }

    public class Answer
    {
        public int AnswerCode;
    }

    public class LastMoveInfo
    {
        public bool GameOver;
        public Tuple<Point, int>[] ChangedCells;
        public Tuple<int, Point, int>[] PlayersChangedPosition; // <id, new position, new hp>
    }


    #endregion


    public class Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;
    }

    public class Player
    {
        public Player(int id, string nick, Point startPos, Point target, int hp)
        {
            Id = id;
            Nick = nick;
            StartPosition = startPos;
            Target = target;
            Hp = hp;
        }

        public int Id;
        public string Nick;
        public int Hp;
        public Point StartPosition;
        public Point Target;
    }

}