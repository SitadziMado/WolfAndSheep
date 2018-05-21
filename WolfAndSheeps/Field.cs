using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

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
            int cellSize = 32,
            int defaultWeight = DefaultWeight,
            int bumpWeight = BumpWeight,
            int pitWeight = PitWeight
        )
        {
            Utility.Assert(
                width >= WidthMinimum && height >= HeightMinimum &&
                width <= WidthMaximum && height <= HeightMaximum,
                "Слишком маленькое либо слишком большое поле."
            );

            Width = width;
            Height = height;
            CellWidth = CellHeight = cellSize;

            Status = StatusType.Game;
            mCells = new Cell[Width, Height];

            InternalFillCells(defaultWeight, bumpWeight, pitWeight);

            AddWolf();
            AddWolf();
            AddSheep();
            AddSheep();
            AddSheep();
        }

        public void Draw(Graphics graphics)
        {
            graphics.Clear(Color.White);

            foreach (var v in mCells)
            {
                v.Draw(graphics);
            }

            if (Debug && m_maze != null)
                foreach (var v in mCells)
                {
                    int c = 255 * m_maze[v.Position.X / CellWidth, v.Position.Y / CellHeight] / (m_mazeMax + 2);
                    if (c > 255)
                        c = 255;
                    if (c > 0)
                    {
                        Color color = Color.FromArgb(c, 0, 0);
                        var pt = new Point(v.Position.X, v.Position.Y);
                        using (Brush brush = new SolidBrush(color))
                            graphics.FillRectangle(
                                brush,
                                pt.X,
                                pt.Y,
                                CellWidth,
                                CellHeight
                            );
                    }
                }

            foreach (var v in mEntities)
            {
                v.Draw(graphics);
            }
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
            return mCells[x, y].Passable;
        }

        public bool IsPassable(Point pt)
        {
            return IsPassable(pt.X, pt.Y);
        }

        public bool IsBump(int x, int y)
        {
            return mCells[x, y].TerrainType == CellType.Bump;
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

            if (mCells[startX, startY].Passable)
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
                int nextValue = value + mCells[pt.X, pt.Y].Time;

                for (int i = 0; i < 4; ++i)
                {
                    var ptr = new Point(pt.X + rot.X, pt.Y + rot.Y);
                    rot = RotateCW(rot);

                    if (InBounds(ptr))
                        if (mCells[ptr.X, ptr.Y].Passable)
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

            m_mazeMax = value;
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
                        if ((temp < value) && (temp > 0) && !IsBump(ptr.X, ptr.Y))
                            if (temp < min)
                            {
                                min = temp;
                                next = ptr;
                            }
                    }
                }
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

            m_maze = maze;

            return rc;
        }

        public Queue<Point> Lee(Point start, Point finish)
        {
            return Lee(start.X, start.Y, finish.X, finish.Y);
        }

        public Entity[] EnumEntitiesAtCell(int x, int y)
        {
            var rc = (
                from v in mEntities
                where (v.Position.X == x && v.Position.Y == y)
                select v
            ).ToArray();

            return rc;
        }

        public void Win(Wolf self)
        {
            // Status = StatusType.Won;
            self.Prey.Kill();
            mEntities.Remove(self.Prey);
            self.Prey = null;
        }

        public void Start()
        {
            Live = true;

            foreach (var v in mEntities)
            {
                v.Thread.Start();
            }
        }

        public void Stop()
        {
            Live = false;
        }

        public void Suspend()
        {
            try
            {
                foreach (var v in mEntities)
                {
                    v.Thread.Suspend();
                }
            }
            catch (ThreadStateException)
            {
                MessageBox.Show("Сначала запустите игру");
            }
        }

        public void Resume()
        {
            try
            {
                foreach (var v in mEntities)
                {
                    v.Thread.Resume();
                }
            }
            catch (ThreadStateException)
            {
                MessageBox.Show("Игра уже идет");
            }
        }

        public void Boost()
        {
            // mWolfThread.Priority = ThreadPriority.AboveNormal;
        }

        public void AddWolf()
        {
            var wolf = new Wolf(NextPoint(), this, new Bitmap(Image.FromFile("wolf.png")));
            wolf.Thread = new Thread(() => { WolfProcedure(wolf); });
            mEntities.Add(wolf);
        }

        public void AddSheep()
        {
            var sheep = new Sheep(NextPoint(), this, new Bitmap(Image.FromFile("sheep.png")));
            sheep.Thread = new Thread(() => { SheepProcedure(sheep); });
            mEntities.Add(sheep);
        }

        public Cell CellAt(Point pos)
        {
            return mCells[pos.X, pos.Y];
        }

        private void WolfProcedure(Wolf self)
        { 
            while (self.Live && Live)
            {
                try
                {
                    if (self.Prey == null)
                    {
                        lock (mPreyFinderLock)
                        {
                            self.Prey = FindPrey();
                        }
                    }

                    lock (self.Prey)
                    {
                        var path = Lee(self.Position, self.Prey.Position);

                        if (path == null)
                            break;
                        // Status = StatusType.Stuck;

                        self.Path = path;
                        self.Tick();
                    }
                }
                catch (InvalidOperationException)
                {
                    Thread.Sleep(500);
                }

                Thread.Sleep(Interval);
            }
        }

        private void SheepProcedure(Sheep self)
        {
            while (self.Live && Live)
            {
                self.Tick();
                Thread.Sleep(Interval);
            }
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

                double type = mRnd.Next(defaultWeight + bumpWeight + pitWeight + 1);
                CellType cellType;

                if (type < defaultWeight)
                    cellType = CellType.Default;
                else if (type < defaultWeight + bumpWeight)
                    cellType = CellType.Bump;
                else
                    cellType = CellType.Pit;

                mCells[x, y] = new Cell(this, x, y, cellType);
            }
        }

        private Point NextPoint()
        {
            Point pos;
            bool restart = false;

            do
            {
                restart = false;
                pos = new Point(mRnd.Next(Width), mRnd.Next(Height));

                foreach (var v in mEntities)
                {
                    if (pos == v.Position)
                    {
                        restart = true;
                    }
                }
            } while (restart || !IsPassable(pos));

            return pos;
        }

        private Sheep FindPrey()
        {
            try
            {
                var result = (from v
                              in mEntities
                              where v is Sheep && !(v as Sheep).IsHunted
                              select v).First() as Sheep;

                result.IsHunted = true;
                return result;
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException(
                    "Нет доступной овцы",
                    e
                );
            }
        }

        private const int WidthMinimum = 5;
        private const int HeightMinimum = 5;
        private const int WidthMaximum = 50;
        private const int HeightMaximum = 50;
        private const int DefaultWeight = 80;
        private const int BumpWeight = 10;
        private const int PitWeight = 10;
        
        private static Random mRnd = new Random();

        public int CellWidth { get; set; }
        public int CellHeight { get; set; }
        public int RequiredWidth { get { return CellWidth * Width;  } }
        public int RequiredHeight { get { return CellHeight * Height; } }
        public int Width { get; set; }
        public int Height { get; set; }
        public StatusType Status { get; set; }
        public bool Debug { get; set; }
        public int Interval { get; set; }
        public bool Live { get; set; }

        private Cell[,] mCells;
        private List<Entity> mEntities = new List<Entity>();
        private int[,] m_maze;
        private int m_mazeMax = 1;

        private object mWolfLock = new object();
        private object mSheepLock = new object();
        private object mSuspendObject = new object();
        private object mPreyFinderLock = new object();
    }
}
