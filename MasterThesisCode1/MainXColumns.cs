using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Geometry.SpatialTrees;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;

namespace MasterThesisCode1
{
    internal class MainXColumns
    {
        public static List<List<Brep>> minxlen(List<Brep> minx, double cc, double height, int numfloors, double startingheight) 
        {
            List<List<Brep>> output = new List<List<Brep>>();
            List<Brep> minxlen = new List<Brep>();
            List<Brep> allbeams = new List<Brep>();

            List<Point3d> minxpts = ClassForOpenings.sortingPointsaccourdingZYX(minx);
            List<List<Point3d>> minxpts1 = ClassForOpenings.createlistforopnings(minxpts, height, numfloors);
            minxpts1 = ClassForOpenings.makesureptsare8ineverylist(minxpts1);
            List<Brep> O_minx = ClassForOpenings.CreateCutterFormpositivretning(minxpts1);

            double w_len = 0;
            double sh1 = 0;
            double sh2 = 0;

            for (int i = 0; i < minx.Count; i++)
            {
                w_len = Todeconstruct_brep.FindShortestlength(minx[i]);
                sh1 = w_len / 2 + 0.05;
                sh2 = w_len - sh1;
                Curve lenmbx = Todeconstruct_brep.FindlongestlengthCurve(Todeconstruct_brep.Findbottombrep(minx[i].GetBoundingBox(true).ToBrep()));
                Point3d p1b = new Point3d(lenmbx.PointAtStart.X + sh2, lenmbx.PointAtStart.Y, lenmbx.PointAtStart.Z);
                Point3d p2b = new Point3d(lenmbx.PointAtEnd.X + sh2, lenmbx.PointAtEnd.Y, lenmbx.PointAtEnd.Z);
                List<Point3d> bpts = SpecialM.pointsatlength(p1b, p2b, cc);

                Curve lenmx = Todeconstruct_brep.FindlongestlengthCurve(Todeconstruct_brep.FindTopbrep(minx[i].GetBoundingBox(true).ToBrep()));
                Point3d p1t = new Point3d(lenmx.PointAtStart.X - sh1, lenmx.PointAtStart.Y, lenmx.PointAtStart.Z);
                Point3d p2t = new Point3d(lenmx.PointAtEnd.X - sh1, lenmx.PointAtEnd.Y, lenmx.PointAtEnd.Z);
                List<Point3d> tpts = SpecialM.pointsatlength(p1t, p2t, cc);
                for (int j = 0; j < tpts.Count; j++)
                {
                    Line l1 = new Line(bpts[j], tpts[j]);
                    Point3d p1 = new Point3d(bpts[j].X + sh2, bpts[j].Y + 0.024, bpts[j].Z);
                    Point3d p2 = new Point3d(tpts[j].X + sh2, tpts[j].Y + 0.024, tpts[j].Z);
                    Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p1);
                    Brep column = SpecialM.LoftcurveWidthwise(l1, height);
                    Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p2);
                    List<Brep> columjoin = new List<Brep>() { bjoint, column, tjoint };
                    Brep bp = Brep.MergeBreps(columjoin, 0.01);
                    minxlen.Add(bp);
                }

            }

            List<int> indexes = new List<int>();
            List<Brep> newColumns = new List<Brep>();
            List<Brep> Beams_maxxlen = new List<Brep>();
            List<Brep> maxx_vindu_tp = new List<Brep>();
            List<Brep> maxx_vindu_bp = new List<Brep>();
            List<Brep> Columnsintheway = new List<Brep>();

            foreach (Brep bp in O_minx)
            {
                int Counter = 0;
                BoundingBox bx = bp.GetBoundingBox(true);
                for (int k = 0; k < minxlen.Count; k++)
                {
                    Brep column = minxlen[k];
                    Point3d pt = AreaMassProperties.Compute(column).Centroid;
                    bool intersec = bx.Contains(pt);
                    foreach (Brep wall in minx)
                    {
                        BoundingBox wallx = wall.GetBoundingBox(true);
                        Point3d wdx = AreaMassProperties.Compute(bx.ToBrep()).Centroid;
                        if (wallx.Contains(wdx))
                        {
                            if (wallx.Contains(pt))
                            {
                                if (wdx.Z > pt.Z)
                                {
                                    Point3d p1 = new Point3d(pt.X, pt.Y, wdx.Z);
                                    intersec = bx.Contains(p1);
                                }
                            }
                        }
                    }
                    double b = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                    double delta = 0.01;

                    if (intersec == true)
                    {
                        if (Math.Abs(startingheight - b) < delta)
                        {
                            int index = minxlen.IndexOf(column);
                            Point3d outerpoint = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(bx.ToBrep())).Centroid;

                            double oY = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterYbrep(bx.ToBrep())).Centroid.Y;
                            double iY = AreaMassProperties.Compute(Todeconstruct_brep.FindInnerYbrep(bx.ToBrep())).Centroid.Y;
                            Point3d midpt = AreaMassProperties.Compute(bx.ToBrep()).Centroid;
                            double Ztp = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                            double Zbp = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;

                            Point3d bottompoint = new Point3d(pt.X - sh2, pt.Y - w_len / 2, Zbp);
                            Point3d toppoint = new Point3d(pt.X - sh2, pt.Y - w_len / 2, Ztp);

                            Line cutterline = new Line(bottompoint, toppoint);
                            BoundingBox cutter = SpecialM.CreateCutter(cutterline).GetBoundingBox(true);
                            Brep btpart = column.Split(cutter.ToBrep(), 0.01)[1];
                            Brep tpart = column.Split(cutter.ToBrep(), 0.01)[3];
                            minxlen.RemoveAt(index);
                            minxlen.Insert(index, tpart);
                            minxlen.Insert(index, btpart);
                            Point3d x1 = AreaMassProperties.Compute(minxlen[k - 1]).Centroid;
                            Point3d x2 = AreaMassProperties.Compute(minxlen[k + 1]).Centroid;
                            double tol = 0.048;

                            if (Counter == 0)
                            {
                                double doorheight = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp1 = new Point3d(x1.X + sh2, oY, doorheight);
                                Point3d bp2 = new Point3d(x2.X + sh2, iY, doorheight);
                                Curve c1 = new Line(bp1, bp2).ToNurbsCurve();
                                Brep beam = SpecialM.LoftBeams(c1);
                                Beams_maxxlen.Add(beam);
                                allbeams.Add(beam);
                            }

                            if (bx.Contains(x1) != true && bx.Contains(x2) != true)
                            {
                                if (Counter != 2)
                                {
                                    if (Math.Abs(iY - x1.Y) < tol)
                                    {
                                        Brep columnintheway = minxlen[k - 1];
                                        int index1 = minxlen.IndexOf(columnintheway);
                                        minxlen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(x1.X - sh2, iY - 0.096, startingheight);
                                        Point3d p2 = new Point3d(x1.X - sh2, iY - 0.096, height + startingheight);
                                        Point3d p11 = new Point3d(x1.X, iY - 0.072, startingheight);
                                        Point3d p21 = new Point3d(x1.X, iY - 0.072, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(iY - x1.Y) > tol)
                                    {
                                        Brep columnintheway = minxlen[k - 1];
                                        int index1 = minxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x1.X - sh2, iY - 0.048, startingheight);
                                        Point3d p2 = new Point3d(x1.X - sh2, iY - 0.048, height + startingheight);
                                        Point3d p11 = new Point3d(x1.X, iY - 0.024, startingheight);
                                        Point3d p21 = new Point3d(x1.X, iY - 0.024, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    if (Math.Abs(oY - x2.Y) < tol)
                                    {
                                        Brep columnintheway = minxlen[k + 1];
                                        int index1 = minxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x2.X - sh2, oY + 0.048, startingheight);
                                        Point3d p2 = new Point3d(x2.X - sh2, oY + 0.048, height + startingheight);
                                        Point3d p11 = new Point3d(x2.X, oY + 0.072, startingheight);
                                        Point3d p21 = new Point3d(x2.X, oY + 0.072, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(oY - x2.Y) > tol)
                                    {
                                        Brep columnintheway = minxlen[k + 1];
                                        int index1 = minxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x2.X - sh2, oY, startingheight);
                                        Point3d p2 = new Point3d(x2.X - sh2, oY, startingheight + height);
                                        Point3d p11 = new Point3d(x2.X, oY + 0.024, startingheight);
                                        Point3d p21 = new Point3d(x2.X, oY + 0.024, startingheight + height);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                }

                            }

                        }

                        else
                        {
                            int index = minxlen.IndexOf(column);
                            Point3d outerpoint = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterYbrep(bx.ToBrep())).Centroid;
                            Point3d outerpoint2 = AreaMassProperties.Compute(Todeconstruct_brep.FindInnerYbrep(bx.ToBrep())).Centroid;
                            double oY = outerpoint.Y;
                            double iY = outerpoint2.Y;

                            Point3d midpt = AreaMassProperties.Compute(bx.ToBrep()).Centroid;
                            double Ztp = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                            double Zbp = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                            Point3d bottompoint = new Point3d(pt.X - sh2, pt.Y - w_len / 2, Zbp);
                            Point3d toppoint = new Point3d(pt.X - sh2, pt.Y - w_len / 2, Ztp);
                            Line cutterline = new Line(bottompoint, toppoint);
                            BoundingBox cutter = SpecialM.CreateCutter(cutterline).GetBoundingBox(true);
                            Brep tpart = column.Split(cutter.ToBrep(), 0.01)[0];
                            Brep btpart = column.Split(cutter.ToBrep(), 0.01)[1];
                            minxlen.RemoveAt(index);
                            minxlen.Insert(index, btpart);
                            minxlen.Insert(index, tpart);
                            Point3d x1 = AreaMassProperties.Compute(minxlen[k - 1]).Centroid;
                            Point3d x2 = AreaMassProperties.Compute(minxlen[k + 1]).Centroid;
                            double tol = 0.058;

                            if (Counter == 0)
                            {
                                double windowheight1 = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp1 = new Point3d(x1.X + sh2, oY, windowheight1);
                                Point3d bp2 = new Point3d(x2.X + sh2, iY, windowheight1);
                                Curve c1 = new Line(bp1, bp2).ToNurbsCurve();
                                Brep beam1 = SpecialM.LoftBeams(c1);
                                maxx_vindu_tp.Add(beam1);
                                allbeams.Add(beam1);

                                double windowheight2 = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp11 = new Point3d(x1.X + sh2, oY, windowheight2 - 0.048);
                                Point3d bp12 = new Point3d(x2.X + sh2, iY, windowheight2 - 0.048);
                                Curve c11 = new Line(bp11, bp12).ToNurbsCurve();
                                Brep beam2 = SpecialM.LoftBeams(c11);
                                maxx_vindu_bp.Add(beam2);
                                allbeams.Add(beam2);
                            }


                            if (bx.Contains(x1) != true && bx.Contains(x2) != true)
                            {
                                if (Counter != 2)
                                {
                                    if (Math.Abs(iY - x1.Y) < tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = minxlen[k - 1];
                                        int index1 = minxlen.IndexOf(columnintheway);
                                        minxlen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(x1.X - sh2, iY - 0.048, h1);
                                        Point3d p2 = new Point3d(x1.X - sh2, iY - 0.048, h2);
                                        Point3d p11 = new Point3d(x1.X, iY - 0.024, h1);
                                        Point3d p21 = new Point3d(x1.X, iY - 0.024, h2);
                                        Line l1 = new Line(p1, p2);

                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }

                                    else if (Math.Abs(iY - x1.Y) > tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = minxlen[k - 1];
                                        int index1 = minxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x1.X - sh2, iY - 0.048, h1);
                                        Point3d p2 = new Point3d(x1.X - sh2, iY - 0.048, h2);
                                        Point3d p11 = new Point3d(x1.X, iY - 0.024, h1);
                                        Point3d p21 = new Point3d(x1.X, iY - 0.024, h2);
                                        Line l1 = new Line(p1, p2);

                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }

                                    if (Math.Abs(oY - x2.Y) < tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = minxlen[k + 1];
                                        int index1 = minxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x2.X - sh2, oY, h1);
                                        Point3d p2 = new Point3d(x2.X - sh2, oY, h2);
                                        Point3d p11 = new Point3d(x2.X, oY + 0.024, h1);
                                        Point3d p21 = new Point3d(x2.X, oY + 0.024, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(oY - x2.Y) > tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = minxlen[k + 1];
                                        int index1 = minxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x2.X - sh2, oY, h1);
                                        Point3d p2 = new Point3d(x2.X - sh2, oY, h2);
                                        Point3d p11 = new Point3d(x2.X, oY + 0.024, h1);
                                        Point3d p21 = new Point3d(x2.X, oY + 0.024, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }

                                }
                            }

                        }

                    }

                }

            }

            foreach (Brep wall in minx)
            {
                double wallZ = AreaMassProperties.Compute(wall).Centroid.Z;
                BoundingBox thewall = wall.GetBoundingBox(true);
                foreach (Brep op in O_minx)
                {
                    BoundingBox box = op.GetBoundingBox(true);
                    Point3d thept = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(box.ToBrep())).Centroid;
                    Point3d mainpt = box.Center;
                    if (thewall.Contains(mainpt))
                    {
                        List<int> indexesofremove = new List<int>();
                        List<Brep> colmtoadd = new List<Brep>();
                        foreach (Brep colm in minxlen)
                        {
                            Point3d pt = AreaMassProperties.Compute(colm).Centroid;
                            if (Math.Abs(wallZ - pt.Z) < 0.1)
                            {
                                double diff = Math.Abs(thept.X - pt.X);
                                if (diff < 0.048)
                                {
                                    int index = minxlen.IndexOf(colm);
                                    Point3d p1 = new Point3d(pt.X + diff, pt.Y - sh2, pt.Z - (height / 2));
                                    Point3d p2 = new Point3d(pt.X + diff, pt.Y - sh2, pt.Z + (height / 2));
                                    Point3d p11 = new Point3d(pt.X + diff + 0.024, pt.Y, pt.Z - (height / 2));
                                    Point3d p21 = new Point3d(pt.X + diff + 0.024, pt.Y, pt.Z + (height / 2));
                                    Line l1 = new Line(p1, p2);
                                    Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                    Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                    Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                    List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                    Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                    indexesofremove.Add(index);
                                    colmtoadd.Add(Newcolumn);
                                }
                            }
                        }
                        for (int i = 0; i < indexesofremove.Count; i++)
                        {
                            minxlen.RemoveAt(indexesofremove[i]);
                            minxlen.Insert(indexesofremove[i], colmtoadd[i]);
                        }
                    }

                }
            }
            for (int b = 0; b < newColumns.Count; b++)
            {
                minxlen.Insert(indexes[b], newColumns[b]);
            }

            //For dører 
            foreach (Brep beam in Beams_maxxlen)
            {
                BoundingBox beambx = beam.GetBoundingBox(true);
                for (int n = 0; n < minxlen.Count; n++)
                {
                    Brep column = minxlen[n];
                    Point3d pt = AreaMassProperties.Compute(column).Centroid;
                    double bbZ = AreaMassProperties.Compute(beam).Centroid.Z;
                    Point3d truept = new Point3d();
                    if (pt.Z > bbZ && pt.Z < height)
                    {
                        truept = new Point3d(pt.X, pt.Y, bbZ);
                    }
                    bool intersect = beambx.Contains(truept);
                    if (intersect == true)
                    {
                        int index = minxlen.IndexOf(column);
                        minxlen.RemoveAt(index);
                        Point3d stpoint = new Point3d(pt.X - sh2, pt.Y - 0.11, bbZ - 0.024);
                        Point3d endpoint = new Point3d(pt.X - sh2, pt.Y - 0.11, bbZ + 0.024);
                        Line ll1 = new Line(stpoint, endpoint);
                        Brep cutter = SpecialM.CreateCutter(ll1);
                        Brep tpart = column.Split(cutter, 0.01)[0];
                        minxlen.Insert(index, tpart);
                    }
                }
            }
            foreach (Brep wall in minx)
            {
                double topheightofwall = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(wall)).Centroid.Z;
                double bottomofwall = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(wall)).Centroid.Z;
                BoundingBox Wallbox = wall.GetBoundingBox(true);
                Point3d pl1 = Wallbox.GetCorners()[0];
                Point3d pl2 = Wallbox.GetCorners()[3];
                Point3d pfl1 = new Point3d(pl1.X + sh2, pl1.Y, pl1.Z - 0.048);
                Point3d pfl2 = new Point3d(pl2.X + sh2, pl2.Y, pl2.Z - 0.048);
                Curve l1 = new Line(pfl1, pfl2).ToNurbsCurve();
                allbeams.Add(SpecialM.LoftBeams(l1));
                foreach (Brep beam in maxx_vindu_tp)
                {
                    BoundingBox beambx = beam.GetBoundingBox(true);
                    for (int n = 0; n < minxlen.Count; n++)
                    {
                        Brep column = minxlen[n];
                        Point3d pt = AreaMassProperties.Compute(column).Centroid;
                        double bbZ = AreaMassProperties.Compute(beam).Centroid.Z;
                        Point3d truept = new Point3d();
                        if (pt.Z > bbZ && pt.Z < topheightofwall && bbZ > bottomofwall)
                        {
                            truept = new Point3d(pt.X, pt.Y, bbZ);
                        }
                        bool intersect = beambx.Contains(truept);
                        if (intersect == true)
                        {
                            int index = minxlen.IndexOf(column);
                            Point3d stpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ - 0.024);
                            Point3d endpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ + 0.024);
                            Line ll1 = new Line(stpoint, endpoint);
                            Brep cutter = SpecialM.CreateCutter(ll1);
                            Brep tpart = column.Split(cutter, 0.01)[1];
                            minxlen.RemoveAt(index);
                            minxlen.Insert(index, tpart);
                        }

                    }
                }

                foreach (Brep beam in maxx_vindu_bp)
                {
                    BoundingBox beambx = beam.GetBoundingBox(true);
                    BoundingBox thewall = wall.GetBoundingBox(true);
                    Point3d testpoint = beambx.Center;
                    if (thewall.Contains(testpoint))
                    {
                        for (int n = 0; n < minxlen.Count; n++)
                        {
                            Brep column = minxlen[n];
                            Point3d pt = AreaMassProperties.Compute(column).Centroid;
                            double bbZ = AreaMassProperties.Compute(beam).Centroid.Z;
                            Point3d truept = new Point3d();
                            if (pt.Z < bbZ && pt.Z > bottomofwall)
                            {
                                truept = new Point3d(pt.X, pt.Y, bbZ);
                            }
                            bool intersect = beambx.Contains(truept);
                            if (intersect == true)
                            {
                                int index = minxlen.IndexOf(column);
                                Point3d stpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ - 0.024);
                                Point3d endpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ + 0.024);
                                Line ll1 = new Line(stpoint, endpoint);
                                Brep cutter = SpecialM.CreateCutter(ll1);
                                Brep bpart = column.Split(cutter, 0.01)[0];
                                minxlen.RemoveAt(index);
                                minxlen.Insert(index, bpart);
                            }

                        }
                    }
                }
                Point3d pu1 = Wallbox.GetCorners()[4];
                Point3d pu2 = Wallbox.GetCorners()[7];
                Point3d pfu1 = new Point3d(pu1.X + sh2, pu1.Y, pu1.Z);
                Point3d pfu2 = new Point3d(pu2.X + sh2, pu2.Y, pu2.Z);
                Curve u1 = new Line(pfu1, pfu2).ToNurbsCurve();
                allbeams.Add(SpecialM.LoftBeams(u1));
            }
            output.Add(minxlen);
            output.Add(allbeams);
            return output; 
        }

        public static List<List<Brep>> maxxlen (List<Brep> maxx, double cc,double height, int numfloors, double startingheight)
        {
            List<List<Brep>> output = new List<List<Brep>>();
            List<Brep> maxxlen = new List<Brep>();
            List<Brep> allbeams = new List<Brep>();

            List<Point3d> maxxpts = ClassForOpenings.sortingPointsaccourdingZYX(maxx);
            List<List<Point3d>> maxxpts1 = ClassForOpenings.createlistforopnings(maxxpts, height, numfloors);
            maxxpts1 = ClassForOpenings.makesureptsare8ineverylist(maxxpts1);
            List<Brep> O_maxx = ClassForOpenings.CreateCutterFormpositivretning(maxxpts1);

            double w_len = 0;
            double sh1 = 0;
            double sh2 = 0;

            for (int i = 0; i < maxx.Count; i++)
            {
                w_len = Todeconstruct_brep.FindShortestlength(maxx[i]);
                sh1 = w_len / 2 + 0.05;
                sh2 = w_len - sh1;
                Curve lenmbx = Todeconstruct_brep.FindlongestlengthCurve(Todeconstruct_brep.Findbottombrep(maxx[i].GetBoundingBox(true).ToBrep()));
                Point3d p1b = new Point3d(lenmbx.PointAtStart.X + sh2, lenmbx.PointAtStart.Y, lenmbx.PointAtStart.Z);
                Point3d p2b = new Point3d(lenmbx.PointAtEnd.X + sh2, lenmbx.PointAtEnd.Y, lenmbx.PointAtEnd.Z);
                List<Point3d> bpts = SpecialM.pointsatlength(p1b, p2b, cc);

                Curve lenmx = Todeconstruct_brep.FindlongestlengthCurve(Todeconstruct_brep.FindTopbrep(maxx[i].GetBoundingBox(true).ToBrep()));
                Point3d p1t = new Point3d(lenmx.PointAtStart.X - sh1, lenmx.PointAtStart.Y, lenmx.PointAtStart.Z);
                Point3d p2t = new Point3d(lenmx.PointAtEnd.X - sh1, lenmx.PointAtEnd.Y, lenmx.PointAtEnd.Z);
                List<Point3d> tpts = SpecialM.pointsatlength(p1t, p2t, cc);
                for (int j = 0; j < tpts.Count; j++)
                {
                    Line l1 = new Line(bpts[j], tpts[j]);
                    Point3d p1 = new Point3d(bpts[j].X + sh2, bpts[j].Y + 0.024, bpts[j].Z);
                    Point3d p2 = new Point3d(tpts[j].X + sh2, tpts[j].Y + 0.024, tpts[j].Z);
                    Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p1);
                    Brep column = SpecialM.LoftcurveWidthwise(l1, height);
                    Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p2);
                    List<Brep> columjoin = new List<Brep>() { bjoint, column, tjoint };
                    Brep bp = Brep.MergeBreps(columjoin, 0.01);
                    maxxlen.Add(bp);
                }

            }

            List<int> indexes = new List<int>();
            List<Brep> newColumns = new List<Brep>();
            List<Brep> Beams_maxxlen = new List<Brep>();
            List<Brep> maxx_vindu_tp = new List<Brep>();
            List<Brep> maxx_vindu_bp = new List<Brep>();
            List<Brep> Columnsintheway = new List<Brep>();

            foreach (Brep bp in O_maxx)
            {
                int Counter = 0;
                BoundingBox bx = bp.GetBoundingBox(true);
                for (int k = 0; k < maxxlen.Count; k++)
                {
                    Brep column = maxxlen[k];
                    Point3d pt = AreaMassProperties.Compute(column).Centroid;
                    bool intersec = bx.Contains(pt);
                    foreach (Brep wall in maxx)
                    {
                        BoundingBox wallx = wall.GetBoundingBox(true);
                        Point3d wdx = AreaMassProperties.Compute(bx.ToBrep()).Centroid;
                        if (wallx.Contains(wdx))
                        {
                            if (wallx.Contains(pt))
                            {
                                if (wdx.Z > pt.Z)
                                {
                                    Point3d p1 = new Point3d(pt.X, pt.Y, wdx.Z);
                                    intersec = bx.Contains(p1);
                                }
                            }
                        }
                    }
                    double b = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                    double delta = 0.01;

                    if (intersec == true)
                    {
                        if (Math.Abs(startingheight - b) < delta)
                        {
                            int index = maxxlen.IndexOf(column);
                            Point3d outerpoint = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(bx.ToBrep())).Centroid;

                            double oY = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterYbrep(bx.ToBrep())).Centroid.Y;
                            double iY = AreaMassProperties.Compute(Todeconstruct_brep.FindInnerYbrep(bx.ToBrep())).Centroid.Y;
                            Point3d midpt = AreaMassProperties.Compute(bx.ToBrep()).Centroid;
                            double Ztp = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                            double Zbp = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;

                            Point3d bottompoint = new Point3d(pt.X - sh2, pt.Y - w_len / 2, Zbp);
                            Point3d toppoint = new Point3d(pt.X - sh2, pt.Y - w_len / 2, Ztp);

                            Line cutterline = new Line(bottompoint, toppoint);
                            BoundingBox cutter = SpecialM.CreateCutter(cutterline).GetBoundingBox(true);
                            Brep btpart = column.Split(cutter.ToBrep(), 0.01)[1];
                            Brep tpart = column.Split(cutter.ToBrep(), 0.01)[3];
                            maxxlen.RemoveAt(index);
                            maxxlen.Insert(index, tpart);
                            maxxlen.Insert(index, btpart);
                            Point3d x1 = AreaMassProperties.Compute(maxxlen[k - 1]).Centroid;
                            Point3d x2 = AreaMassProperties.Compute(maxxlen[k + 1]).Centroid;
                            double tol = 0.048;

                            if (Counter == 0)
                            {
                                double doorheight = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp1 = new Point3d(x1.X + sh2, oY, doorheight);
                                Point3d bp2 = new Point3d(x2.X + sh2, iY, doorheight);
                                Curve c1 = new Line(bp1, bp2).ToNurbsCurve();
                                Brep beam = SpecialM.LoftBeams(c1);
                                Beams_maxxlen.Add(beam);
                                allbeams.Add(beam);
                            }

                            if (bx.Contains(x1) != true && bx.Contains(x2) != true)
                            {
                                if (Counter != 2)
                                {
                                    if (Math.Abs(iY - x1.Y) < tol)
                                    {
                                        Brep columnintheway = maxxlen[k - 1];
                                        int index1 = maxxlen.IndexOf(columnintheway);
                                        maxxlen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(x1.X - sh2, iY - 0.096, startingheight);
                                        Point3d p2 = new Point3d(x1.X - sh2, iY - 0.096, height + startingheight);
                                        Point3d p11 = new Point3d(x1.X, iY - 0.072, startingheight);
                                        Point3d p21 = new Point3d(x1.X, iY - 0.072, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(iY - x1.Y) > tol)
                                    {
                                        Brep columnintheway = maxxlen[k - 1];
                                        int index1 = maxxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x1.X - sh2, iY - 0.048, startingheight);
                                        Point3d p2 = new Point3d(x1.X - sh2, iY - 0.048, height + startingheight);
                                        Point3d p11 = new Point3d(x1.X, iY - 0.024, startingheight);
                                        Point3d p21 = new Point3d(x1.X, iY - 0.024, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    if (Math.Abs(oY - x2.Y) < tol)
                                    {
                                        Brep columnintheway = maxxlen[k + 1];
                                        int index1 = maxxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x2.X - sh2, oY + 0.048, startingheight);
                                        Point3d p2 = new Point3d(x2.X - sh2, oY + 0.048, height + startingheight);
                                        Point3d p11 = new Point3d(x2.X, oY + 0.072, startingheight);
                                        Point3d p21 = new Point3d(x2.X, oY + 0.072, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(oY - x2.Y) > tol)
                                    {
                                        Brep columnintheway = maxxlen[k + 1];
                                        int index1 = maxxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x2.X - sh2, oY, startingheight);
                                        Point3d p2 = new Point3d(x2.X - sh2, oY, startingheight + height);
                                        Point3d p11 = new Point3d(x2.X, oY + 0.024, startingheight);
                                        Point3d p21 = new Point3d(x2.X, oY + 0.024, startingheight + height);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                }

                            }

                        }

                        else
                        {
                            int index = maxxlen.IndexOf(column);
                            Point3d outerpoint = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterYbrep(bx.ToBrep())).Centroid;
                            Point3d outerpoint2 = AreaMassProperties.Compute(Todeconstruct_brep.FindInnerYbrep(bx.ToBrep())).Centroid;
                            double oY = outerpoint.Y;
                            double iY = outerpoint2.Y;

                            Point3d midpt = AreaMassProperties.Compute(bx.ToBrep()).Centroid;
                            double Ztp = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                            double Zbp = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                            Point3d bottompoint = new Point3d(pt.X - sh2, pt.Y - w_len / 2, Zbp);
                            Point3d toppoint = new Point3d(pt.X - sh2, pt.Y - w_len / 2, Ztp);
                            Line cutterline = new Line(bottompoint, toppoint);
                            BoundingBox cutter = SpecialM.CreateCutter(cutterline).GetBoundingBox(true);
                            Brep tpart = column.Split(cutter.ToBrep(), 0.01)[0];
                            Brep btpart = column.Split(cutter.ToBrep(), 0.01)[1];
                            maxxlen.RemoveAt(index);
                            maxxlen.Insert(index, btpart);
                            maxxlen.Insert(index, tpart);
                            Point3d x1 = AreaMassProperties.Compute(maxxlen[k - 1]).Centroid;
                            Point3d x2 = AreaMassProperties.Compute(maxxlen[k + 1]).Centroid;
                            double tol = 0.058;

                            if (Counter == 0)
                            {
                                double windowheight1 = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp1 = new Point3d(x1.X + sh2, oY, windowheight1);
                                Point3d bp2 = new Point3d(x2.X + sh2, iY, windowheight1);
                                Curve c1 = new Line(bp1, bp2).ToNurbsCurve();
                                Brep beam1 = SpecialM.LoftBeams(c1);
                                maxx_vindu_tp.Add(beam1);
                                allbeams.Add(beam1);

                                double windowheight2 = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp11 = new Point3d(x1.X + sh2, oY, windowheight2 - 0.048);
                                Point3d bp12 = new Point3d(x2.X + sh2, iY, windowheight2 - 0.048);
                                Curve c11 = new Line(bp11, bp12).ToNurbsCurve();
                                Brep beam2 = SpecialM.LoftBeams(c11);
                                maxx_vindu_bp.Add(beam2);
                                allbeams.Add(beam2);
                            }


                            if (bx.Contains(x1) != true && bx.Contains(x2) != true)
                            {
                                if (Counter != 2)
                                {
                                    if (Math.Abs(iY - x1.Y) < tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = maxxlen[k - 1];
                                        int index1 = maxxlen.IndexOf(columnintheway);
                                        maxxlen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(x1.X - sh2, iY - 0.048, h1);
                                        Point3d p2 = new Point3d(x1.X - sh2, iY - 0.048, h2);
                                        Point3d p11 = new Point3d(x1.X, iY - 0.024, h1);
                                        Point3d p21 = new Point3d(x1.X, iY - 0.024, h2);
                                        Line l1 = new Line(p1, p2);

                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }

                                    else if (Math.Abs(iY - x1.Y) > tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = maxxlen[k - 1];
                                        int index1 = maxxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x1.X - sh2, iY - 0.048, h1);
                                        Point3d p2 = new Point3d(x1.X - sh2, iY - 0.048, h2);
                                        Point3d p11 = new Point3d(x1.X, iY - 0.024, h1);
                                        Point3d p21 = new Point3d(x1.X, iY - 0.024, h2);
                                        Line l1 = new Line(p1, p2);

                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }

                                    if (Math.Abs(oY - x2.Y) < tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = maxxlen[k + 1];
                                        int index1 = maxxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x2.X - sh2, oY, h1);
                                        Point3d p2 = new Point3d(x2.X - sh2, oY, h2);
                                        Point3d p11 = new Point3d(x2.X, oY + 0.024, h1);
                                        Point3d p21 = new Point3d(x2.X, oY + 0.024, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(oY - x2.Y) > tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = maxxlen[k + 1];
                                        int index1 = maxxlen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(x2.X - sh2, oY, h1);
                                        Point3d p2 = new Point3d(x2.X - sh2, oY, h2);
                                        Point3d p11 = new Point3d(x2.X, oY + 0.024, h1);
                                        Point3d p21 = new Point3d(x2.X, oY + 0.024, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointWidth(p11);
                                        Brep maincolm = SpecialM.LoftcurveWidthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointWidth(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }

                                }
                            }

                        }

                    }

                }

            }

            foreach (Brep wall in maxx)
            {
                double wallZ = AreaMassProperties.Compute(wall).Centroid.Z;
                BoundingBox thewall = wall.GetBoundingBox(true);
                foreach (Brep op in O_maxx)
                {
                    BoundingBox box = op.GetBoundingBox(true);
                    Point3d thept = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(box.ToBrep())).Centroid;
                    Point3d mainpt = box.Center;
                    if (thewall.Contains(mainpt))
                    {
                        List<int> indexesofremove = new List<int>();
                        List<Brep> colmtoadd = new List<Brep>();
                        foreach (Brep colm in maxxlen)
                        {
                            Point3d pt = AreaMassProperties.Compute(colm).Centroid;
                            if (Math.Abs(wallZ - pt.Z) < 0.1)
                            {
                                double diff = Math.Abs(thept.X - pt.X);
                                if (diff < 0.048)
                                {
                                    int index = maxxlen.IndexOf(colm);
                                    Point3d p1 = new Point3d(pt.X + diff, pt.Y - sh2, pt.Z - (height / 2));
                                    Point3d p2 = new Point3d(pt.X + diff, pt.Y - sh2, pt.Z + (height / 2));
                                    Point3d p11 = new Point3d(pt.X + diff + 0.024, pt.Y, pt.Z - (height / 2));
                                    Point3d p21 = new Point3d(pt.X + diff + 0.024, pt.Y, pt.Z + (height / 2));
                                    Line l1 = new Line(p1, p2);
                                    Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                    Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                    Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                    List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                    Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                    indexesofremove.Add(index);
                                    colmtoadd.Add(Newcolumn);
                                }
                            }
                        }
                        for (int i = 0; i < indexesofremove.Count; i++)
                        {
                            maxxlen.RemoveAt(indexesofremove[i]);
                            maxxlen.Insert(indexesofremove[i], colmtoadd[i]);
                        }
                    }

                }
            }
            for (int b = 0; b < newColumns.Count; b++)
            {
                maxxlen.Insert(indexes[b], newColumns[b]);
            }

            //For dører 
            foreach (Brep beam in Beams_maxxlen)
            {
                BoundingBox beambx = beam.GetBoundingBox(true);
                for (int n = 0; n < maxxlen.Count; n++)
                {
                    Brep column = maxxlen[n];
                    Point3d pt = AreaMassProperties.Compute(column).Centroid;
                    double bbZ = AreaMassProperties.Compute(beam).Centroid.Z;
                    Point3d truept = new Point3d();
                    if (pt.Z > bbZ && pt.Z < height)
                    {
                        truept = new Point3d(pt.X, pt.Y, bbZ);
                    }
                    bool intersect = beambx.Contains(truept);
                    if (intersect == true)
                    {
                        int index = maxxlen.IndexOf(column);
                        maxxlen.RemoveAt(index);
                        Point3d stpoint = new Point3d(pt.X - sh2, pt.Y - 0.11, bbZ - 0.024);
                        Point3d endpoint = new Point3d(pt.X - sh2, pt.Y - 0.11, bbZ + 0.024);
                        Line ll1 = new Line(stpoint, endpoint);
                        Brep cutter = SpecialM.CreateCutter(ll1);
                        Brep tpart = column.Split(cutter, 0.01)[0];
                        maxxlen.Insert(index, tpart);
                    }
                }
            }

            foreach (Brep wall in maxx)
            {
                double topheightofwall = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(wall)).Centroid.Z;
                double bottomofwall = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(wall)).Centroid.Z;
                BoundingBox Wallbox = wall.GetBoundingBox(true);
                Point3d pl1 = Wallbox.GetCorners()[0];
                Point3d pl2 = Wallbox.GetCorners()[3];
                Point3d pfl1 = new Point3d(pl1.X + sh2, pl1.Y, pl1.Z - 0.048);
                Point3d pfl2 = new Point3d(pl2.X + sh2, pl2.Y, pl2.Z - 0.048);
                Curve l1 = new Line(pfl1, pfl2).ToNurbsCurve();
                allbeams.Add(SpecialM.LoftBeams(l1));
                foreach (Brep beam in maxx_vindu_tp)
                {
                    BoundingBox beambx = beam.GetBoundingBox(true);
                    for (int n = 0; n < maxxlen.Count; n++)
                    {
                        Brep column = maxxlen[n];
                        Point3d pt = AreaMassProperties.Compute(column).Centroid;
                        double bbZ = AreaMassProperties.Compute(beam).Centroid.Z;
                        Point3d truept = new Point3d();
                        if (pt.Z > bbZ && pt.Z < topheightofwall && bbZ > bottomofwall)
                        {
                            truept = new Point3d(pt.X, pt.Y, bbZ);
                        }
                        bool intersect = beambx.Contains(truept);
                        if (intersect == true)
                        {
                            int index = maxxlen.IndexOf(column);
                            Point3d stpoint = new Point3d(pt.X - sh2, pt.Y - 0.11, bbZ - 0.024);
                            Point3d endpoint = new Point3d(pt.X - sh2, pt.Y - 0.11, bbZ + 0.024);
                            Line ll1 = new Line(stpoint, endpoint);
                            Brep cutter = SpecialM.CreateCutter(ll1);
                            Brep tpart = column.Split(cutter, 0.01)[1];
                            maxxlen.RemoveAt(index);
                            maxxlen.Insert(index, tpart);
                        }

                    }
                }

                foreach (Brep beam in maxx_vindu_bp)
                {
                    BoundingBox beambx = beam.GetBoundingBox(true);
                    BoundingBox thewall = wall.GetBoundingBox(true);
                    Point3d testpoint = beambx.Center;
                    if (thewall.Contains(testpoint))
                    {
                        for (int n = 0; n < maxxlen.Count; n++)
                        {
                            Brep column = maxxlen[n];
                            Point3d pt = AreaMassProperties.Compute(column).Centroid;
                            double bbZ = AreaMassProperties.Compute(beam).Centroid.Z;
                            Point3d truept = new Point3d();
                            if (pt.Z < bbZ && pt.Z > bottomofwall)
                            {
                                truept = new Point3d(pt.X, pt.Y, bbZ);
                            }
                            bool intersect = beambx.Contains(truept);
                            if (intersect == true)
                            {
                                int index = maxxlen.IndexOf(column);
                                Point3d stpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ - 0.024);
                                Point3d endpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ + 0.024);
                                Line ll1 = new Line(stpoint, endpoint);
                                Brep cutter = SpecialM.CreateCutter(ll1);
                                Brep bpart = column.Split(cutter, 0.01)[0];
                                maxxlen.RemoveAt(index);
                                maxxlen.Insert(index, bpart);
                            }

                        }
                    }
                }
                Point3d pu1 = Wallbox.GetCorners()[4];
                Point3d pu2 = Wallbox.GetCorners()[7];
                Point3d pfu1 = new Point3d(pu1.X + sh2, pu1.Y, pu1.Z);
                Point3d pfu2 = new Point3d(pu2.X + sh2, pu2.Y, pu2.Z);
                Curve u1 = new Line(pfu1, pfu2).ToNurbsCurve();
                allbeams.Add(SpecialM.LoftBeams(u1));
            }
            output.Add(maxxlen);
            output.Add(allbeams);


            return output;
        }

    }
}
