using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace combinig_polygons
{
    public partial class Form1 : Form
    {

        List<List<Point>> _polygons;
        List<Point> _points;

        Bitmap _bm;
        Graphics _g;
        Size _size;

        public Form1()
        {
            InitializeComponent();

            _polygons = new List<List<Point>>();
            _points = new List<Point>();

            _size = pictureBox1.Size;
            _bm = new Bitmap(_size.Width, _size.Height);
            pictureBox1.Image = _bm;
            _g = Graphics.FromImage(pictureBox1.Image);
        }

        private void DrawPolygons()
        {
            _bm = new Bitmap(_size.Width, _size.Height);
            pictureBox1.Image = _bm;
            _g = Graphics.FromImage(pictureBox1.Image);

            for (int j = 0; j < _polygons.Count; j++)
            {
                Pen pen = new Pen(Color.Black);

                List<Point> polygon = _polygons[j];
                if (polygon.Count == 1) _g.DrawRectangle(pen, new Rectangle(polygon[0].X, polygon[0].Y, 1, 1));
                else if (polygon.Count == 2) _g.DrawLine(pen, polygon[0].X, polygon[0].Y, polygon[1].X, polygon[1].Y);
                else
                {
                    for (int i = 1; i < polygon.Count; i++)
                        _g.DrawLine(pen, polygon[i - 1].X, polygon[i - 1].Y, polygon[i].X, polygon[i].Y);
                    _g.DrawLine(pen, polygon[0].X, polygon[0].Y, polygon[polygon.Count - 1].X, polygon[polygon.Count - 1].Y);
                }
            }

            pictureBox1.Image = _bm;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !_points.Contains(e.Location))
            {
                _points.Add(e.Location);
                _bm.SetPixel(e.Location.X, e.Location.Y, Color.Black);
                pictureBox1.Image = _bm;
            }
            else if (e.Button == MouseButtons.Right && _points.Count != 0)
            {
                _polygons.Add(new List<Point>(_points));
                _points.Clear();

                DrawPolygons();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _polygons.Clear();
            _g.Clear(pictureBox1.BackColor);

            _bm = new Bitmap(_size.Width, _size.Height);
            pictureBox1.Image = _bm;
            _g = Graphics.FromImage(pictureBox1.Image);
        }
    }
}
