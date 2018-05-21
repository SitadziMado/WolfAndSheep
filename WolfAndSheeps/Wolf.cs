using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WolfAndSheeps
{
    public class Wolf : Entity
    {
        public Wolf(Point position, Field parent, Bitmap sprite) : 
            base(position, parent, sprite)
        {
            Path = null;
        }

        public override void Tick()
        {
            if (Path != null)
            {
                var next = Path.Dequeue();

                if (Prey.Position == next)
                {
                    mParent.Win(this);
                }

                Move(new Point(next.X - Position.X, next.Y - Position.Y));
                //Position = next;

                if (Path.Count == 0)
                    Path = null;
            }

            /*var sheep = (
                from v in mParent.EnumEntitiesAtCell(Position.X, Position.Y)
                where v is Sheep
                select v)
                .ToArray();
            if (sheep.Length > 0)
                mParent.Win(this);*/
        }

        public Queue<Point> Path { get; set; }
        public Sheep Prey { get; set; }
    }
}
