using System;
using System.Drawing;

namespace WolfAndSheeps
{
    public abstract class Entity
    {
        public Entity(Point position, Field parent, Bitmap sprite)
        {
            Position = position;
            m_parent = parent;
            m_sprite = sprite;
        }

        public abstract void Tick();
        
        public void Draw(Graphics graphics)
        {
            graphics.DrawImage(
                m_sprite, 
                Position.X * m_parent.CellWidth, 
                Position.Y * m_parent.CellHeight,
                m_parent.CellWidth,
                m_parent.CellHeight
            );
        }

        protected bool Move(Point offset)
        {
            var newPosition = new Point(Position.X + offset.X, Position.Y + offset.Y);
            if (m_parent.InBounds(newPosition) && m_parent.IsPassable(newPosition))
            {
                if (m_parent.IsBump(Position))
                {
                    if (++m_movementScore > 1)
                        m_movementScore = 0;
                    else
                        return true;
                }
                Position = new Point(Position.X + offset.X, Position.Y + offset.Y);
                return true;
            }
            else
                return false;
        }

        protected static Random rnd = new Random();

        public Point Position { get; set; }

        protected Bitmap m_sprite;
        protected Field m_parent;

        private int m_movementScore = 0;
    }
}
