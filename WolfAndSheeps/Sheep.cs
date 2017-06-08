using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolfAndSheeps
{
    public class Sheep : Entity
    {
        public Sheep(Point position, Field parent, Bitmap sprite) :
            base(position, parent, sprite)
        {

        }

        public override void Tick()
        {
            Point offset = new Point();

            do
            {
                switch (rnd.Next(4))
                {
                    case 0:
                        offset = new Point(1, 0);
                        break;

                    case 1:
                        offset = new Point(0, 1);
                        break;

                    case 2:
                        offset = new Point(-1, 0);
                        break;

                    case 3:
                        offset = new Point(0, -1);
                        break;
                }
            } while (!Move(offset));
        }
    }
}
