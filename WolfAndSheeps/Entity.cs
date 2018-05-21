using System;
using System.Drawing;
using System.Threading;

namespace WolfAndSheeps
{
    public abstract class Entity
    {
        public Entity(Point position, Field parent, Bitmap sprite)
        {
            Position = position;
            mParent = parent;
            mSprite = sprite;
            Live = true;
            Lock(Position);
        }

        public abstract void Tick();
        
        public void Draw(Graphics graphics)
        {
            graphics.DrawImage(
                mSprite, 
                Position.X * mParent.CellWidth, 
                Position.Y * mParent.CellHeight,
                mParent.CellWidth,
                mParent.CellHeight
            );
        }

        public void Kill()
        {
            Live = false;
            Unlock(Position);
        }

        protected bool Move(Point offset)
        {
            var newPosition = new Point(Position.X + offset.X, Position.Y + offset.Y);

            if (mParent.InBounds(newPosition) && mParent.IsPassable(newPosition))
            {
                Lock(newPosition);

                if (mParent.IsBump(Position))
                {
                    if (++m_movementScore > 1)
                        m_movementScore = 0;
                    else
                        return true;
                }

                Unlock(Position);
                Position = new Point(Position.X + offset.X, Position.Y + offset.Y);

                return true;
            }
            else
                return false;
        }

        private void Lock(Point pt)
        {
            Monitor.Enter(mParent.CellAt(pt));
        }

        private void Unlock(Point pt)
        {
            if (Monitor.IsEntered(mParent.CellAt(Position)))
            {
                Monitor.Exit(mParent.CellAt(pt));
            }
        }

        protected static Random rnd = new Random();

        public Point Position { get; set; }
        public Thread Thread { get; set; }
        public bool Live { get; set; }

        protected Bitmap mSprite;
        protected Field mParent;

        private int m_movementScore = 0;
    }
}
