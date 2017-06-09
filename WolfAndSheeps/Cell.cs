using System.Collections.Generic;
using System.Drawing;

namespace WolfAndSheeps
{
    public class Cell
    {
        public Cell(Field parent, int x, int y, CellType type)
        {
            Utility.Assert(parent != null, "Пустой родитель.");
            Utility.Assert(x >= 0 && y >= 0, "Неверные координаты.");
            m_parent = parent;
            m_size = new Size(parent.CellWidth, parent.CellHeight);
            Position = new Point(x * parent.CellWidth, y * parent.CellHeight);
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
            /*(if (TerrainType == CellType.Pit)
            {*/
                var brush = brushes[TerrainType];
                graphics.FillRectangle(brush, new Rectangle(Position, m_size));
                graphics.DrawRectangle(Pens.Black, new Rectangle(Position, m_size));
            /*}
            else
            {
                graphics.DrawImage(
                    bitmaps[TerrainType],
                    Position.X,
                    Position.Y,
                    m_parent.CellWidth,
                    m_parent.CellHeight
                );
            }*/
        }

        private Size m_size;
        private static Dictionary<CellType, Brush> brushes = new Dictionary<CellType, Brush>()
        {
            { CellType.Default, Brushes.LawnGreen },
            { CellType.Bump, Brushes.SandyBrown },
            { CellType.Pit, Brushes.Black },
        };
        /*private static Dictionary<CellType, Bitmap> bitmaps = new Dictionary<CellType, Bitmap>()
        {
            { CellType.Default, new Bitmap(Image.FromFile("grass.png")) },
            { CellType.Bump, new Bitmap(Image.FromFile("bump.jpg")) },
        };*/

        public Point Position { get; set; }
        public bool Passable { get; set; }
        public int Time { get; set; }
        public CellType TerrainType { get; set; }

        private Field m_parent;
    }

    public enum CellType
    {
        Default,
        Bump,
        Pit,
    }
}
