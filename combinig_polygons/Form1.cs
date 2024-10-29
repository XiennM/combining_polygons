using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;

namespace combinig_polygons
{
    public partial class Form1 : Form
    {

        List<List<Point>> _polygons;
        List<Point> _points;
        List<Tuple<Point, bool>> _pointsp1_intersect = new List<Tuple<Point, bool>>();
        List<Tuple<Point, bool>> _pointsp2_intersect = new List<Tuple<Point, bool>>();

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

        private void button1_Click(object sender, EventArgs e)
        {
            intersectPoints();
            Combine();
        }

        private void intersectPoints()
        {
            _pointsp1_intersect = new List<Tuple<Point, bool>>();
            _pointsp2_intersect = new List<Tuple<Point, bool>>();
            List<Point> _pointsp1 = _polygons[0];
            List<Point> _pointsp2 = _polygons[1];

            Point a;
            Point b;
            Point c;
            Point d;
            Point n;
            float t;
            List<Point> res = new List<Point>();

            for (int i = 0; i <= _pointsp1.Count - 1; i++)
            {
                _pointsp1_intersect.Add(new Tuple<Point, bool>(_pointsp1[i], false));
                for (int j = 0; j <= _pointsp2.Count - 1; j++)
                {
                    a = _pointsp1[i];
                    b = _pointsp1[(i + 1) % _pointsp1.Count];
                    c = _pointsp2[j];
                    d = _pointsp2[(j + 1) % _pointsp2.Count];
                    n = new Point(-(d.Y - c.Y), d.X - c.X);

                    t = -(float)(n.X * (a.X - c.X) + n.Y * (a.Y - c.Y)) / (n.X * (b.X - a.X) + n.Y * (b.Y - a.Y));


                    res.Add(new Point(a.X + (int)(t * (b.X - a.X)), a.Y + (int)(t * (b.Y - a.Y))));
                    if (res[res.Count - 1].X >= Math.Min(a.X, b.X) && res[res.Count - 1].X <= Math.Max(a.X, b.X) && res[res.Count - 1].Y >= Math.Min(a.Y, b.Y) && res[res.Count - 1].Y <= Math.Max(a.Y, b.Y)
                        && res[res.Count - 1].X >= Math.Min(c.X, d.X) && res[res.Count - 1].X <= Math.Max(c.X, d.X) && res[res.Count - 1].Y >= Math.Min(c.Y, d.Y) && res[res.Count - 1].Y <= Math.Max(c.Y, d.Y))
                    {
                        _g.DrawRectangle(new Pen(Color.Red), res[res.Count - 1].X, res[res.Count - 1].Y, 3, 3);
                        pictureBox1.Image = _bm;
                        _pointsp1_intersect.Add(new Tuple<Point, bool>(res[res.Count - 1], true));
                    }
                }
            }

            for (int i = 0; i < _pointsp1_intersect.Count - 1; i++)
            {
                // Находим две точки с флагом IsIntersect == true
                if (_pointsp1_intersect[i].Item2 == true && _pointsp1_intersect[(i + 1) % _pointsp1_intersect.Count].Item2 == true)
                {
                    Point temp1 = _pointsp1_intersect[(i - 1) % _pointsp1_intersect.Count].Item1;
                    Point temp2 = _pointsp1_intersect[(i + 2) % _pointsp1_intersect.Count].Item1;

                    // Извлекаем точки с IsIntersect == true между ними
                    var innerPoints = _pointsp1_intersect.GetRange(i, 2)
                        .OrderBy(p => dist(temp1.X, temp1.Y, p.Item1.X, p.Item1.Y)) // Сортировка по расстоянию к a
                        .ToList();

                    // Заменяем исходные точки на отсортированные
                    _pointsp1_intersect[i] = innerPoints[0];
                    _pointsp1_intersect[i + 1] = innerPoints[1];
                }
            }


            res = new List<Point>();


            for (int i = 0; i <= _pointsp2.Count - 1; i++)
            {
                _pointsp2_intersect.Add(new Tuple<Point, bool>(_pointsp2[i], false));
                for (int j = 0; j <= _pointsp1.Count - 1; j++)
                {
                    a = _pointsp1[j];
                    b = _pointsp1[(j + 1) % _pointsp1.Count];
                    c = _pointsp2[i];
                    d = _pointsp2[(i + 1) % _pointsp2.Count];
                    n = new Point(-(d.Y - c.Y), d.X - c.X);

                    t = -(float)(n.X * (a.X - c.X) + n.Y * (a.Y - c.Y)) / (n.X * (b.X - a.X) + n.Y * (b.Y - a.Y));


                    res.Add(new Point(a.X + (int)(t * (b.X - a.X)), a.Y + (int)(t * (b.Y - a.Y))));
                    if (res[res.Count - 1].X >= Math.Min(a.X, b.X) && res[res.Count - 1].X <= Math.Max(a.X, b.X) && res[res.Count - 1].Y >= Math.Min(a.Y, b.Y) && res[res.Count - 1].Y <= Math.Max(a.Y, b.Y)
                        && res[res.Count - 1].X >= Math.Min(c.X, d.X) && res[res.Count - 1].X <= Math.Max(c.X, d.X) && res[res.Count - 1].Y >= Math.Min(c.Y, d.Y) && res[res.Count - 1].Y <= Math.Max(c.Y, d.Y))
                    {
                        _g.DrawRectangle(new Pen(Color.Red), res[res.Count - 1].X, res[res.Count - 1].Y, 3, 3);
                        pictureBox1.Image = _bm;
                        _pointsp2_intersect.Add(new Tuple<Point, bool>(res[res.Count - 1], true));
                    }
                }

            }


            for (int i = 0; i < _pointsp2_intersect.Count - 1; i++)
            {
                // Находим две точки с флагом IsIntersect == true
                if (_pointsp2_intersect[i].Item2 == true && _pointsp2_intersect[(i + 1) % _pointsp2_intersect.Count].Item2 == true)
                {
                    Point temp1 = _pointsp2_intersect[(i - 1) % _pointsp2_intersect.Count].Item1;
                    Point temp2 = _pointsp2_intersect[(i + 2) % _pointsp1_intersect.Count].Item1;

                    // Извлекаем точки с IsIntersect == true между ними
                    var innerPoints = _pointsp2_intersect.GetRange(i, 2)
                        .OrderBy(p => dist(temp1.X, temp1.Y, p.Item1.X, p.Item1.Y)) // Сортировка по расстоянию к a
                        .ToList();

                    // Заменяем исходные точки на отсортированные
                    _pointsp2_intersect[i] = innerPoints[0];
                    _pointsp2_intersect[i + 1] = innerPoints[1];
                }
            }


            /*
            textBox1.Text = "1: " + _pointsp1_intersect.Count + " 2: " + _pointsp2_intersect.Count;
            int k = 0;
            foreach (Tuple<Point, bool> point in _pointsp2_intersect)
            {
                if (_pointsp1_intersect.FindIndex(x => x.Equals(point)) != -1)
                    k++;
            }
            textBox1.Text += "OOO: " + k;*/
            foreach (Tuple<Point, bool> point in _pointsp2_intersect)
            {
                //if (point.Item2 == true)
                    textBox2.Text += "  " + point.Item1.X + "," + point.Item1.Y;
                if (point.Item2 == true)
                    textBox2.Text += "!";

            }
            foreach (Tuple<Point, bool> point in _pointsp1_intersect)
            {
                //if (point.Item2 == true)
                    textBox1.Text += "  " + point.Item1.X + "," + point.Item1.Y;
                if (point.Item2 == true)
                    textBox1.Text += "!";

            }
        }

        private double dist(int x1, int x2, int y1, int y2)
        {
            return Math.Sqrt(((x2 - x1)^2 + (y2 - y1)^2));
        }

        private void Combine()
        {
            Point lowest_p = _pointsp1_intersect[0].Item1;
            bool inFirst = true;

            for (int i = 0; i <= _pointsp1_intersect.Count - 1; i++)
            {
                if (lowest_p.Y < _pointsp1_intersect[i].Item1.Y) {
                    lowest_p = _pointsp1_intersect[i].Item1;
                }
            }

            for (int i = 0; i <= _pointsp2_intersect.Count - 1; i++)
            {
                if (lowest_p.Y < _pointsp2_intersect[i].Item1.Y)
                {
                    lowest_p = _pointsp2_intersect[i].Item1;
                    inFirst = false;
                }
            }

            List<Point> res = new List<Point>();
            res.Add(lowest_p);

            if(inFirst)
            {
                int k = _pointsp1_intersect.FindIndex(x => x.Item1 == lowest_p);
                k = (k + 1) % _pointsp1_intersect.Count;
                while (_pointsp1_intersect[k].Item1 != lowest_p)
                {
                    while (_pointsp1_intersect[k].Item2 != true)
                    {
                        res.Add(_pointsp1_intersect[k].Item1);
                        k = (k + 1) % _pointsp1_intersect.Count;
                        if (_pointsp1_intersect[k].Item1 == lowest_p)
                            break;
                    }
                    if (_pointsp1_intersect[k].Item1 == lowest_p)
                        break;
                    k = _pointsp2_intersect.FindIndex(x => x.Item1.Equals(_pointsp1_intersect[k].Item1));
                    res.Add(_pointsp2_intersect[k].Item1);
                    k = (k + 1) % _pointsp2_intersect.Count;
                    while (_pointsp2_intersect[k].Item2 != true)
                    {
                        res.Add(_pointsp2_intersect[k].Item1);
                        k = (k + 1) % _pointsp2_intersect.Count;
                    }
                    k = _pointsp1_intersect.FindIndex(x => x.Item1.Equals(_pointsp2_intersect[k].Item1));
                    res.Add(_pointsp1_intersect[k].Item1);
                    k = (k + 1) % _pointsp1_intersect.Count;
                }
            }

            if (!inFirst)
            {
                int k = _pointsp2_intersect.FindIndex(x => x.Item1 == lowest_p);
                k = (k + 1) % _pointsp2_intersect.Count;
                while (_pointsp2_intersect[k].Item1 != lowest_p)
                {
                    while (_pointsp2_intersect[k].Item2 != true)
                    {
                        res.Add(_pointsp2_intersect[k].Item1);
                        k = (k + 1) % _pointsp2_intersect.Count;
                        if (_pointsp2_intersect[k].Item1 == lowest_p)
                            break;
                    }
                    if (_pointsp2_intersect[k].Item1 == lowest_p)
                        break;
                    k = _pointsp1_intersect.FindIndex(x => x.Item1.Equals(_pointsp2_intersect[k].Item1));
                    res.Add(_pointsp1_intersect[k].Item1);
                    k = (k + 1) % _pointsp1_intersect.Count;
                    while (_pointsp1_intersect[k].Item2 != true)
                    {
                        res.Add(_pointsp1_intersect[k].Item1);
                        k = (k + 1) % _pointsp1_intersect.Count;
                    }
                    k = _pointsp2_intersect.FindIndex(x => x.Item1.Equals(_pointsp1_intersect[k].Item1));
                    res.Add(_pointsp2_intersect[k].Item1);
                    k = (k + 1) % _pointsp2_intersect.Count;
                }
            }


            Pen pen = new Pen(Color.Red);

            List<Point> polygon = res;
                for (int i = 1; i < polygon.Count; i++)
                {
                    _g.DrawLine(pen, polygon[i - 1].X, polygon[i - 1].Y, polygon[i].X, polygon[i].Y);
                }
                _g.DrawLine(pen, polygon[0].X, polygon[0].Y, polygon[polygon.Count - 1].X, polygon[polygon.Count - 1].Y);
        }
    }
}
