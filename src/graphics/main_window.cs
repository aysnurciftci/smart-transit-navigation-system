//ana ekran ve grafik işlemleri
using System;
using System.Drawing;
using System.Windows.Forms;
using SmartTransit.Models;
using SmartTransit.Generator;
using SmartTransit.MultiGraph;

namespace SmartTransit.UI
{
    public partial class MainWindow : Form
    {
        private TransitGraph graph;
        private Station? seciliDurak;
        private float scale = 1.0f;
        private int offsetX = 0;
        private int offsetY = 0;
        private bool isDragging = false;
        private Point lastMousePos;

        public MainWindow()
        {
            // 20 istasyonlu, 800x600 boyutlarında, 200 mesafe eşiği
            graph = GraphGenerator.CreateFullGraph(
                stationCount: 20,
                maxX: 800,
                maxY: 600,
                distanceThreshold: 200,
                enableVisualBundling: true
            );

            this.DoubleBuffered = true;
            this.Paint += MainWindow_Paint;
            this.MouseMove += MainWindow_MouseMove;
            this.MouseWheel += MainWindow_MouseWheel;
            this.MouseDown += MainWindow_MouseDown;
            this.MouseUp += MainWindow_MouseUp;
        }

        // Çizim (Paint)
        private void MainWindow_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Rotaları çiz
            foreach (var route in graph.Routes)
            {
                if (route.PathPoints.Count > 0)
                {
                    for (int i = 0; i < route.PathPoints.Count - 1; i++)
                    {
                        g.DrawLine(Pens.Gray,
                            (float)(route.PathPoints[i].X * scale + offsetX),
                            (float)(route.PathPoints[i].Y * scale + offsetY),
                            (float)(route.PathPoints[i + 1].X * scale + offsetX),
                            (float)(route.PathPoints[i + 1].Y * scale + offsetY));
                    }
                }
                else
                {
                    g.DrawLine(Pens.Gray,
                        (float)(route.Source.X * scale + offsetX),
                        (float)(route.Source.Y * scale + offsetY),
                        (float)(route.Target.X * scale + offsetX),
                        (float)(route.Target.Y * scale + offsetY));
                }
            }

            // İstasyonları çiz
            foreach (var s in graph.Stations)
            {
                bool isHovered = (seciliDurak != null && seciliDurak.Id == s.Id);
                Brush brush = isHovered ? Brushes.Red : Brushes.Green;
                int size = isHovered ? 15 : 10;

                g.FillEllipse(brush,
                    (float)(s.X * scale + offsetX) - size / 2,
                    (float)(s.Y * scale + offsetY) - size / 2,
                    size, size);

                if (isHovered)
                {
                    g.DrawString(s.Name, this.Font, Brushes.Black,
                        (float)(s.X * scale + offsetX) + 10,
                        (float)(s.Y * scale + offsetY));
                }
            }
        }

        // Ölçeklendirme (Mouse Wheel)
        private void MainWindow_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                scale += 0.1f;
            else if (e.Delta < 0 && scale > 0.2f)
                scale -= 0.1f;

            this.Invalidate();
        }

        // Fare ile hareket aşağı (Mouse Down)
        private void MainWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastMousePos = e.Location;
            }
        }

        // Fare ile hareket yukarı (Mouse Up)
        private void MainWindow_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        // Fare hareketi kontrolü (Mouse Move)
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                offsetX += e.X - lastMousePos.X;
                offsetY += e.Y - lastMousePos.Y;
                lastMousePos = e.Location;
                this.Invalidate();
            }
            else
            {
                seciliDurak = null;
                foreach (var s in graph.Stations)
                {
                    var rect = new Rectangle(
                        (int)(s.X * scale + offsetX) - 5,
                        (int)(s.Y * scale + offsetY) - 5,
                        10, 10);

                    if (rect.Contains(e.Location))
                    {
                        seciliDurak = s;
                        break;
                    }
                }
                this.Invalidate();
            }
        }
    }
}
