using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;

namespace MasterThesisCode1
{
    internal class ClassForOpenings
    {
        public static List<List<Point3d>> separatepoints(List<Point3d> points)
        {
            List<List<Point3d>> pts = new List<List<Point3d>>();
            int len = points.Count;
            int k = len / 4;
            List<Point3d> pts1 = new List<Point3d>();
            foreach (Point3d p in points)
            {
                if(p.Z < 2.448)
                {
                    pts1.Add(p);
                }
            }
            
            int h = pts1.Count;
            for (int i = 0; i < h/4; i++)
            {
                int a = h / 2;
                List<Point3d> v1 = points.Skip(4* i).Take(4).ToList();
                List<Point3d> v2 = points.Skip(4*i+a).Take(4).ToList();
                List<Point3d> v3 = new List<Point3d>();
                for (int j = 0; j<v1.Count; j++)
                {
                    v3.Add(v1[j]);
                }
                for (int j = 0; j < v2.Count; j++)
                {
                    v3.Add(v2[j]);
                }
                pts.Add(v3);
            }
            return pts;
        }

        public static List<Brep> CreateCutterFormpositivretning(List<List<Point3d>> pts)
        {
            List<Brep> brep = new List<Brep>();
            for (int i = 0; i < pts.Count; i++)
            {
                List<Point3d> p1 = new List<Point3d>();
                List<Point3d> p2 = new List<Point3d>();
                //Manuelt fordi jeg ikke fant en god måtte å lage koden på.
                p1.Add(pts[i][0]);
                p1.Add(pts[i][1]);
                p1.Add(pts[i][3]);
                p1.Add(pts[i][2]);
                p1.Add(pts[i][0]);

                p2.Add(pts[i][4]);
                p2.Add(pts[i][5]);
                p2.Add(pts[i][7]);
                p2.Add(pts[i][6]);
                p2.Add(pts[i][4]);

                Curve c1 = NurbsCurve.CreateControlPointCurve(p1, 1);
                Curve c2 = NurbsCurve.CreateControlPointCurve(p2, 1);
                List<Curve> crvs1 = new List<Curve>() { c1, c2 };
                Brep b_test = Brep.CreateFromLoft(crvs1, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
                brep.Add(b_test);
            }
            return brep;
        }
        /*
        public static List<Point3d> sortingPointsaccourdingZ(List<Brep> brep)
        {
            List<Point3d> sortedpts = new List<Point3d>();
            List<Point3d> listtest = new List<Point3d>();
            foreach (Brep b in brep)
            {
                int lenb = b.Vertices.Count - 8;
                for (int i = 0; i < lenb; i++)
                {
                    listtest.Add(b.Vertices[i].Location);
                }
            }

            sortedpts = listtest.OrderBy(p => p.Z).ThenBy(p => p.Y).ThenBy(p => p.X).ToList();
            return sortedpts;
        }
        */
        public static List<Point3d> sortingPointsaccourdingZYX(List<Brep> brep)
        {
            List<Point3d> sortedpts = new List<Point3d>();
            List<Point3d> listtest = new List<Point3d>();
            foreach (Brep b in brep)
            {
                int lenb = b.Vertices.Count - 8;
                for (int i = 0; i < lenb; i++)
                {
                    listtest.Add(b.Vertices[i].Location);
                }
            }

            sortedpts = listtest.OrderBy(p => p.Z).ThenBy(p => p.Y).ThenBy(p => p.X).ToList();
            return sortedpts;
        }
        public static List<Point3d> sortingPointsaccourdingZXY(List<Brep> brep)
        {
            List<Point3d> sortedpts = new List<Point3d>();
            List<Point3d> listtest = new List<Point3d>();
            foreach (Brep b in brep)
            {
                int lenb = b.Vertices.Count - 8;
                for (int i = 0; i < lenb; i++)
                {
                    listtest.Add(b.Vertices[i].Location);
                }
            }

            sortedpts = listtest.OrderBy(p => p.Z).ThenBy(p => p.X).ThenBy(p => p.Y).ToList();
            
            return sortedpts;
        }

        public static List<List<Point3d>> createlistforopnings(List<Point3d> points, double H, int n )
        {
            List<List<Point3d>> pts = new List<List<Point3d>>();
            List<List<Point3d>> testpts = new List<List<Point3d>>();
            for (int b = 0; b < n; b++)
            {
                List<Point3d> ptsn = new List<Point3d>();
                foreach (Point3d p in points)
                {
                    if(p.Z > H*b && p.Z < H*b + H+0.300)
                    {
                        ptsn.Add(p);
                    }
                }
                
                int h = ptsn.Count / 8;
                List<Point3d> Addedpts = new List<Point3d>();
                for (int i = 0; i < h; i++)
                {
                    List<Point3d> v1 = ptsn.Skip(4 * i).Take(4).ToList();
                    List<Point3d> v22 = new List<Point3d>();
                    foreach (Point3d p in v1)
                    {
                        Addedpts.Add(p);
                    }
                    foreach (Point3d v in v1)
                    {
                        foreach (Point3d p in ptsn)
                        {
                            if (p.X == v.X && p.Z != v.Z)
                            {
                                v22.Add(p);
                            }
                            else if(p.Y == v.Y && p.Z != v.Z)
                            {
                                v22.Add(p);
                            }
                        }
                    }
                    List<Point3d> v2 = v22.Distinct().ToList();
                    List<Point3d> v3 = new List<Point3d>();
                    for (int j = 0; j < v1.Count; j++)
                    {
                        v3.Add(v1[j]);
                    }
                    for (int j = 0; j < v2.Count; j++)
                    {
                        v3.Add(v2[j]);
                    }
                    pts.Add(v3);
                }
                testpts.Add(Addedpts);
            }

            return pts;
        }

        public static List<List<Point3d>> makesureptsare8ineverylist (List<List<Point3d>> pts)
        {
            List<Point3d> testingfindpts = new List<Point3d>();
            int testnum = pts.Count;
            for (int i = 0; i < testnum - 1; i++)
            {
                List<Point3d> l1 = pts[i];
                List<Point3d> l2 = pts[i + 1];
                List<int> pointsinl1 = new List<int>();
                List<int> pointsinl2 = new List<int>();
                foreach (Point3d p in l2)
                {
                    if (l1.Contains(p))
                    {
                        testingfindpts.Add(p);
                        int k1 = l1.IndexOf(p);
                        int k2 = l2.IndexOf(p);
                        pointsinl1.Add(k1);
                        pointsinl2.Add(k2);
                    }
                }
                for (int k = 0; k < pointsinl1.Count - 1; k++)
                {
                    int c = pointsinl1[k + 1];
                    pts[i].RemoveAt(c);
                    int b = pointsinl2[k];
                    pts[i + 1].RemoveAt(b);
                }
            }
            return pts;
        }
    }
}
