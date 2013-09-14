// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace StreamlinesDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            U = 0.5;
            R = 1;
            CreateModel();
        }

        public GeometryModel3D StreamLinesModel { get; set; }

        public double R { get; set; }
        public double U { get; set; }

        private void CreateModel()
        {
            const double dt = 0.1;
            const int nSteps = 100;
            var mb = new MeshBuilder(true, true);
            for (double y0 = -5; y0 <= 5; y0 += 0.25)
            {
                var p0 = new Point(-3, y0);
                Point[] pts = Solve(Velocity, p0, dt, nSteps);
                var vel = new double[pts.Length];
                var diam = new double[pts.Length];
                int i = 0;
                var pts3d = new Point3D[pts.Length];
                double vmax = 0;
                foreach (Point pt in pts)
                {
                    pts3d[i] = new Point3D(pt.X, pt.Y, 0);
                    double v = Velocity(pt.X, pt.Y).Length;
                    if (v > vmax) vmax = v;
                    vel[i++] = v;
                }
                for (int j = 0; j < vel.Length; j++)
                    vel[j] /= vmax;
                for (int j = 0; j < vel.Length; j++)
                    diam[j] = 0.075;

                mb.AddTube(pts3d, vel, diam, 12, false);
            }
            StreamLinesModel = new GeometryModel3D();
            StreamLinesModel.Geometry = mb.ToMesh();
            StreamLinesModel.Material = Materials.Hue;
            StreamLinesModel.BackMaterial = Materials.Hue;
        }

        // http://mathdemos.gcsu.edu/mathdemos/gowiththeflow/HTMLLinks/index_16.html
        public Vector Velocity(double x, double y)
        {
            double x2y2 = x*x + y*y;
            //   if (x2y2 < R * R)
            //      return new Vector(0, 0);
            double x2y22 = x2y2*2;
            double u = 1 - R*R*(x*x - y*y)/x2y22;
            double v = -R*R*2*x*y/x2y22;
            return new Vector(u, v);
        }

        public Point[] Solve(Func<double, double, Vector> f, Point p0, double dt, int n)
        {
            var origin = new Point();
            Point p = p0;
            var result = new Point[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = p;
                p = p + f(p.X, p.Y)*dt;

                // constrain to the outside of the cylinder...
                if ((p - origin).Length < R)
                {
                    Vector v = p - origin;
                    v.Normalize();
                    v *= R;
                    p = origin + v;
                }
            }
            return result;
        }
    }
}