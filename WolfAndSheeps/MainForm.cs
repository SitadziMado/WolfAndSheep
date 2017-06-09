using System;
using System.Drawing;
using System.Windows.Forms;

namespace WolfAndSheeps
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Init();
        }

        private void outputPictureBox_Paint(object sender, PaintEventArgs e)
        {
            m_field.Draw(e.Graphics);
        }

        private void Init()
        {
            tickTimer.Enabled = false;
            m_field = new Field(
                sizeTrackBar.Value, 
                sizeTrackBar.Value,
                cellSizeTrackBar.Value,
                defaultTrackBar.Value * 8,
                bumpTrackBar.Value * 8,
                pitTrackBar.Value * 8
            );
            Size = new Size(
                m_field.RequiredWidth + 100 + sideTable.Size.Width,
                Math.Max(m_field.RequiredHeight + 100, sideTable.Size.Height)
            );

            outputPictureBox.Invalidate();
        }

        private void sizeTrackBar_Scroll(object sender, EventArgs e)
        {
            Init();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (m_field.Status != Field.StatusType.Game)
                Init();
            UpdateTickInterval();
            tickTimer.Enabled = true;
        }

        private void intervalTrackBar_Scroll(object sender, EventArgs e)
        {
            UpdateTickInterval();
        }

        private void UpdateTickInterval()
        {
            tickTimer.Interval = 1000 - intervalTrackBar.Value * 100;
        }

        private void tickTimer_Tick(object sender, EventArgs e)
        {
            m_field.Debug = debugCheckBox.Checked;
            m_field.Tick();
            outputPictureBox.Invalidate();
            if (m_field.Status == Field.StatusType.Won)
            {
                tickTimer.Enabled = false;
                MessageBox.Show("Волк съел овцу.", "Победа!", MessageBoxButtons.OK);
            }
            else if (m_field.Status == Field.StatusType.Stuck)
            {
                tickTimer.Enabled = false;
                MessageBox.Show("Волку некуда идти.", "Волк застрял", MessageBoxButtons.OK);
                Init();
            }
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            tickTimer.Enabled = false;
        }

        private void debugCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            m_field.Debug = debugCheckBox.Checked;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            startButton.Focus();
        }

        private Field m_field;
    }
}
