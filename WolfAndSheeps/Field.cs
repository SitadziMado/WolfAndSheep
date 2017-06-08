﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolfAndSheeps
{
    public class Field
    {
        public enum StatusType
        {
            Game,
            Won,
            Stuck,
        }

        public Field(
            int width = WidthMinimum,
            int height = HeightMinimum,
            int defaultWeight = DefaultWeight,
            int bumpWeight = BumpWeight,
            int pitWeight = PitWeight
        )
        {
            Utility.Assert(
                width >= WidthMinimum && height >= HeightMinimum &&
                width <= WidthMaximum && height <= HeightMaximum, 
                "Слишком маленькое поле."
            );
            Width = width;
            Height = height;

            Status = StatusType.Game;
            m_cells = new Cell[Width, Height];

            InternalFillCells(defaultWeight, bumpWeight, pitWeight);

            Point wolfPos;
            Point sheepPos;

            do
            {
                wolfPos = new Point(rnd.Next(Width), rnd.Next(Height));
            } while (!IsPassable(wolfPos));

            m_entities.Add(new Wolf(wolfPos, this, new Bitmap(Image.FromFile("wolf.png"))));

            do
            {
                sheepPos = new Point(rnd.Next(Width), rnd.Next(Height));
            } while (
                !IsPassable(sheepPos) || 
                wolfPos.X == sheepPos.X ||
                wolfPos.Y == sheepPos.Y
                );

            m_entities.Add(new Sheep(sheepPos, this, new Bitmap(Image.FromFile("sheep.png"))));

            m_nextTick = new Queue<Entity>(m_entities);
        }

        public void Draw(Graphics graphics)
        {
            graphics.Clear(Color.White);

            foreach (var v in m_cells)
            {
                v.Draw(graphics);
            }

            foreach (var v in m_entities)
            {
                v.Draw(graphics);
            }
        }

        public void Tick()
        {
            Entity entity;
            m_nextTick.Enqueue(entity = m_nextTick.Dequeue());

            Wolf wolf = entity as Wolf;

            if (wolf != null)
            {
                var path = Lee(wolf.Position, m_entities[1].Position);
                if (path == null)
                    Status = StatusType.Stuck;
                wolf.Path = path;
            }

            entity.Tick();
        }

        public bool InBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool InBounds(Point pt)
        {
            return InBounds(pt.X, pt.Y);
        }

        public bool IsPassable(int x, int y)
        {
            return m_cells[x, y].Passable;
        }

        public bool IsPassable(Point pt)
        {
            return IsPassable(pt.X, pt.Y);
        }

        public bool IsBump(int x, int y)
        {
            return m_cells[x, y].TerrainType == CellType.Bump;
        }

        public bool IsBump(Point pt)
        {
            return IsBump(pt.X, pt.Y);
        }

        public Queue<Point> Lee(int startX, int startY, int finishX, int finishY)
        {
            var rot = new Point(1, 0);
            int[,] maze = new int[Width, Height];

            int value = 1;

            if (m_cells[startX, startY].Passable)
                maze[startX, startY] = value++;
            else
                return null;

            if ((startX == finishX) && (startY == finishY))
                return null;

            var unmarked = new Queue<Point>();
            unmarked.Enqueue(new Point(startX, startY));

            while (maze[finishX, finishY] == 0)
            {
                if (unmarked.Count == 0)
                    return null;

                var pt = unmarked.Dequeue();

                value = maze[pt.X, pt.Y];
                int nextValue = value + m_cells[pt.X, pt.Y].Time;

                for (int i = 0; i < 4; ++i)
                {
                    var ptr = new Point(pt.X + rot.X, pt.Y + rot.Y);
                    rot = RotateCW(rot);

                    if (InBounds(ptr))
                        if (m_cells[ptr.X, ptr.Y].Passable)
                        {
                            if ((maze[ptr.X, ptr.Y] == 0 || maze[ptr.X, ptr.Y] > nextValue))
                            {
                                maze[ptr.X, ptr.Y] = nextValue;
                                unmarked.Enqueue(new Point(ptr.X, ptr.Y));
                            }
                        }
                        else
                        {
                            maze[ptr.X, ptr.Y] = -1;
                        }
                }
            }

            var route = new Stack<Point>();
            route.Push(new Point(finishX, finishY));
            value = maze[finishX, finishY];

            do
            {
                int temp = 0;
                int min = int.MaxValue;

                Point next = new Point();
                var pt = route.First();

                for (int i = 0; i < 4; ++i)
                {
                    var ptr = new Point(pt.X + rot.X, pt.Y + rot.Y);
                    rot = RotateCW(rot);

                    if (InBounds(ptr))
                    {
                        temp = maze[ptr.X, ptr.Y];
                        if ((temp < value) && (temp > 0))
                            if (temp < min)
                            {
                                min = temp;
                                next = ptr;
                            }
                    }
                }
                value = maze[next.X, next.Y];
                route.Push(next);
            } while (value != 1);

            var rc = new Queue<Point>(route);
            rc.Dequeue();

            return rc;
        }

        public Queue<Point> Lee(Point start, Point finish)
        {
            return Lee(start.X, start.Y, finish.X, finish.Y);
        }

        public Entity[] EnumEntitiesAtCell(int x, int y)
        {
            var rc = (
                from v in m_entities
                where (v.Position.X == x && v.Position.Y == y)
                select v
            ).ToArray();

            return rc;
        }

        public void Win()
        {
            Status = StatusType.Won;
        }

        private Point RotateCW(Point pt)
        {
            return new Point(pt.Y, -pt.X);
        }

        private void InternalFillCells(
            int defaultWeight,
            int bumpWeight,
            int pitWeight
        )
        {
            for (int i = 0; i < Width * Height; ++i)
            {
                int x = i % Width;
                int y = i / Width;

                double type = (double)rnd.Next(defaultWeight + bumpWeight + pitWeight + 1);
                CellType cellType;

                if (type < defaultWeight)
                    cellType = CellType.Default;
                else if (type < defaultWeight + bumpWeight)
                    cellType = CellType.Bump;
                else
                    cellType = CellType.Pit;

                m_cells[x, y] = new Cell(x, y, cellType);
            }
        }

        public const int CellWidth = 32;
        public const int CellHeight = CellWidth;

        private const int WidthMinimum = 5;
        private const int HeightMinimum = 5;
        private const int WidthMaximum = 25;
        private const int HeightMaximum = 25;
        private const int DefaultWeight = 80;
        private const int BumpWeight = 10;
        private const int PitWeight = 10;


        private static Random rnd = new Random();

        public int RequiredWidth { get { return CellWidth * Width;  } }
        public int RequiredHeight { get { return CellHeight * Height; } }
        public int Width { get; set; }
        public int Height { get; set; }
        public StatusType Status { get; set; }

        private Cell[,] m_cells;
        private List<Entity> m_entities = new List<Entity>();
        private Queue<Entity> m_nextTick;
    }
}
