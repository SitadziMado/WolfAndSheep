﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace WolfAndSheeps
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Init();
        }

        private void OutputPictureBox_Paint(object sender, PaintEventArgs e)
        {
            mField.Draw(e.Graphics);
        }

        private void Init()
        {
            mField = new Field(
                sizeTrackBar.Value,
                sizeTrackBar.Value,
                cellSizeTrackBar.Value,
                defaultTrackBar.Value * 8,
                bumpTrackBar.Value * 8,
                pitTrackBar.Value * 8
            )
            {
                Debug = DebugCheckBox.Checked
            };

            Size = new Size(
                mField.RequiredWidth + 100 + sideTable.Size.Width,
                Math.Max(mField.RequiredHeight + 100, sideTable.Size.Height)
            );

            outputPictureBox.Invalidate();
        }

        private void SizeTrackBar_Scroll(object sender, EventArgs e)
        {
            Init();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (mField.Live)
            {
                mField.Resume();
            }
            else
            {
                if (mField.Status != Field.StatusType.Game)
                    Init();

                UpdateTickInterval();

                mField.Start();

                var redraw = Task.Factory.StartNew(() =>
                {
                    while (mField.Live)
                    {
                        RedrawProcedure();

                        Thread.Sleep(mField.Interval);
                    }
                });
            }
        }

        private void IntervalTrackBar_Scroll(object sender, EventArgs e)
        {
            UpdateTickInterval();
        }

        private void UpdateTickInterval()
        {
            mField.Interval = 1000 - intervalTrackBar.Value * 100;
        }

        private void RedrawProcedure()
        {
            outputPictureBox.Invalidate();

            if (mField.Status == Field.StatusType.Won)
            {
                mField.Stop();
                MessageBox.Show("Волк съел овцу.", "Победа!", MessageBoxButtons.OK);
            }
            else if (mField.Status == Field.StatusType.Stuck)
            {
                mField.Stop();
                MessageBox.Show("Волку некуда идти.", "Волк застрял", MessageBoxButtons.OK);
                // Init();
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            mField.Suspend();
        }

        private void DebugCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            mField.Debug = DebugCheckBox.Checked;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            startButton.Focus();
        }

        private Field mField;

        private void NewMapButton_Click(object sender, EventArgs e)
        {
            Init();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mField.Stop();
        }
    }
}
