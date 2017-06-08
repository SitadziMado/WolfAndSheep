using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Position.X * Field.CellWidth, 
                Position.Y * Field.CellHeight, 
                Field.CellWidth, 
                Field.CellHeight
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
