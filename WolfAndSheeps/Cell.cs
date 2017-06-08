using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WolfAndSheeps
{
    public class Cell
    {
        public Cell(int x, int y, CellType type)
        {
            Utility.Assert(x >= 0 && y >= 0, "Неверные координаты.");
            Position = new Point(x * Field.CellWidth, y * Field.CellHeight);
            TerrainType = type;

            if (type == CellType.Pit)
                Passable = false;
            else
                Passable = true;

            if (type == CellType.Default)
                Time = 1;
            else if (type == CellType.Bump)
                Time = 2;
            else
                Time = int.MaxValue;
        }

        public void Draw(Graphics graphics)
        {
            var brush = brushes[TerrainType];
            graphics.FillRectangle(brush, new Rectangle(Position, size));
        }

        private static Size size = new Size(Field.CellWidth, Field.CellHeight);
        private static Dictionary<CellType, Brush> brushes = new Dictionary<CellType, Brush>()
        {
            { CellType.Default, Brushes.LawnGreen },
            { CellType.Bump, Brushes.SandyBrown },
            { CellType.Pit, Brushes.Black },
        };

        public Point Position { get; set; }
        public bool Passable { get; set; }
        public int Time { get; set; }
        public CellType TerrainType { get; set; }
    }

    public enum CellType
    {
        Default,
        Bump,
        Pit,
    }
}
