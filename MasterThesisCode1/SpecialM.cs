using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;

namespace MasterThesisCode1
{
    internal class SpecialM
    {
        public List<double> pnts = new List<double>();
        public static List<Point3d> pointsatlength(Point3d p1, Point3d p2, double cc)
        {
            List<Point3d> pt = new List<Point3d>() { p1, p2 };
            List<Point3d> cpt = new List<Point3d>();
            Curve crv = Curve.CreateControlPointCurve(pt, 1);
            var pts = crv.DivideEquidistant(cc);
            for (int i = 0; i < pts.Length; i++)
            {
                cpt.Add(pts[i]);
            }
            return cpt;
        }

        /*
        public static List<Line> createcolumnsfromendpts(List<Point3d> a, double H, double st)
        {
            List<Line> colm = new List<Line>();
            for (int i = 0; i < a.Count - 4; i++)
            {
                if (a[i].Z == H+0.341+st)
                {
                    Point3d p11 = a[i];
                    Point3d p12 = a[i + 4];
                    Line l1 = new Line(p11, p12);
                    colm.Add(l1);
                }
                else if (a[i].Z == st)
                {
                    Point3d p11 = a[i];
                    Point3d p12 = a[i + 4];
                    Line l1 = new Line(p11, p12);
                    colm.Add(l1);

                }
            }
            return colm;
        }

        public static List<Point3d> FindMidPoints(double num, List<Point3d> lst)
        {
            List<Point3d> l1 = new List<Point3d>();
            if (num == 0)
            {
                for (int i = 1; i < lst.Count - 1; i++)
                {
                    l1.Add(lst[i]);
                }
            }
            else
            {
                for (int i = 1; i < lst.Count; i++)
                {
                    l1.Add(lst[i]);
                }
            }
            return l1;
        }
        */

        public static Brep LoftcurveLengthwise(Line column, double Height)
        {
            Brep brep = new Brep();
            Interval i = column.ToNurbsCurve().Domain;
            Rhino.Geometry.Plane plane0 = column.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[0];
            Rhino.Geometry.Plane plane1 = column.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[1];
            if (Height <= 2.4)
            {
                Rectangle3d rec0 = new Rectangle3d(plane0, 0.048, 0.198);
                Rectangle3d rec1 = new Rectangle3d(plane1, 0.048, 0.198);
                List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
                brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
            }
            else
            {
                Rectangle3d rec0 = new Rectangle3d(plane0, 0.098, 0.198);
                Rectangle3d rec1 = new Rectangle3d(plane1, 0.098, 0.198);
                List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
                brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
            }
            return brep;
        }

        public static Brep LoftcurveWidthwise(Line column, double Height)
        {
            Brep brep = new Brep();
            Interval i = column.ToNurbsCurve().Domain;
            Rhino.Geometry.Plane plane0 = column.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[0];
            Rhino.Geometry.Plane plane1 = column.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[1];
            if (Height <= 2.4)
            {
                Rectangle3d rec0 = new Rectangle3d(plane0, 0.198, 0.048);
                Rectangle3d rec1 = new Rectangle3d(plane1, 0.198, 0.048);
                List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
                brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
            }
            else
            {
                Rectangle3d rec0 = new Rectangle3d(plane0, 0.198, 0.098);
                Rectangle3d rec1 = new Rectangle3d(plane1, 0.198, 0.098);
                List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
                brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
            }
            return brep;
        }
        
        /*
        public static Brep Loftoutercolumns(Line column)
        {
            Brep brep = new Brep();
            Interval i = column.ToNurbsCurve().Domain;
            Rhino.Geometry.Plane pl0 = column.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[0];
            Rhino.Geometry.Plane pl1 = column.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[1];
            
            Rectangle3d rec0 = new Rectangle3d(pl0, 0.198, 0.198);
            Rectangle3d rec1 = new Rectangle3d(pl1, 0.198, 0.198);
            List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
            brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
            return brep;
        }
        */
        public static Brep LoftBeams(Curve thebeam)
        {
            Brep brep = new Brep();
            Interval i = thebeam.ToNurbsCurve().Domain;
            Rhino.Geometry.Plane pl0 = thebeam.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[0];
            Rhino.Geometry.Plane pl1 = thebeam.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[1];

            Rectangle3d rectangel0 = new Rectangle3d(pl0, 0.048, 0.198);
            Rectangle3d rectangel1 = new Rectangle3d(pl1, 0.048, 0.198);
            List<Curve> crvs = new List<Curve>() { rectangel0.ToNurbsCurve(), rectangel1.ToNurbsCurve() };
            Brep bp = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
            BoundingBox bx = bp.GetBoundingBox(true);
            brep = bx.ToBrep();
            return brep;
        }

        /*
        public static List<Line> moveptscreatecolumns(List<List<Point3d>> pt)
        {
            List<Point3d> up_pts = new List<Point3d>();
            List<Point3d> bt_pts = new List<Point3d>();
            for (int k = 0; k < pt.Count; k++)
            {
                if (k != 0 && k != pt.Count - 1)
                {
                    for (int j = 0; j < pt[k].Count; j++)
                    {
                        bt_pts.Add(pt[k][j]);
                        Point3d p = new Point3d(pt[k][j].X, pt[k][j].Y, pt[k][j].Z + 0.341);
                        up_pts.Add(p);
                    }

                }

                else if (k == pt.Count - 1 && k != 0)
                {
                    for (int j = 0; j < pt[k].Count; j++)
                    {
                        Point3d p = new Point3d(pt[k][j].X, pt[k][j].Y, pt[k][j].Z + 0.341);
                        up_pts.Add(p);
                    }
                }

                else
                {
                    for (int j = 0; j < pt[k].Count; j++)
                    {
                        bt_pts.Add(pt[k][j]);
                    }
                }
            }

            List<Line> colm = new List<Line>();
            int v = (bt_pts.Count / 2);
            for (int i = 0; i < bt_pts.Count - v; i++)
            {
                Point3d p11 = bt_pts[i];
                Point3d p12 = bt_pts[i+v];
                Line l1 = new Line(p11,p12);
                colm.Add(l1);
            }

            int w = (up_pts.Count / 2);
            for (int i = 0; i < up_pts.Count - w; i++)
            {
                Point3d p11 = up_pts[i];
                Point3d p12 = up_pts[i + w];
                Line l1 = new Line(p11, p12);
                colm.Add(l1);
            }
            return colm;
        }

       public static Brep Creatingbeamsacross(Line beam)
        {
            Brep brep = new Brep();
            Interval i = beam.ToNurbsCurve().Domain;
            Rhino.Geometry.Plane plane0 = beam.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[0];
            Rhino.Geometry.Plane plane1 = beam.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[1];

            Rectangle3d rec0 = new Rectangle3d(plane0, 0.223, 0.048);
            Rectangle3d rec1 = new Rectangle3d(plane1, 0.223, 0.048);
            List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
            brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
            return brep;
        }
        */
        public static Brep CreateCutter(Line beam)
        {
            Brep brep = new Brep();
            Interval i = beam.ToNurbsCurve().Domain;
            Rhino.Geometry.Plane plane0 = beam.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[0];
            Rhino.Geometry.Plane plane1 = beam.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { i.Min, i.Max })[1];

            Rectangle3d rec0 = new Rectangle3d(plane0, 0.300, 0.300);
            Rectangle3d rec1 = new Rectangle3d(plane1, 0.300, 0.300);
            List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
            Brep bp = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
            BoundingBox bx = bp.GetBoundingBox(true);
            brep = bx.ToBrep();
            return brep;
        }
    }

}
