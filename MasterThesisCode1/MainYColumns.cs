using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Geometry.SpatialTrees;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;
using static System.Net.Mime.MediaTypeNames;

namespace MasterThesisCode1
{
    internal class MainYColumns
    {
        public static List<List<Brep>> minylen(List<Brep> miny, double cc, double height, int numfloors, double startingheight)
        {
            List<List<Brep>> output = new List<List<Brep>>();
            List<Brep> minylen = new List<Brep>();
            List<Brep> allbeams = new List<Brep>();

            List<Point3d> minypts = ClassForOpenings.sortingPointsaccourdingZXY(miny);
            List<List<Point3d>> minypts1 = ClassForOpenings.createlistforopnings(minypts, height, numfloors);
            minypts1 = ClassForOpenings.makesureptsare8ineverylist(minypts1);
            List<Brep> O_miny = ClassForOpenings.CreateCutterFormpositivretning(minypts1);

            double w_len = 0;
            double sh1 = 0;
            double sh2 = 0;

            for (int i = 0; i < miny.Count; i++)
            {
                w_len = Todeconstruct_brep.FindShortestlength(miny[i]);
                sh1 = w_len / 2 + 0.05;
                sh2 = w_len - sh1;
                Curve lenmbx = Todeconstruct_brep.FindlongestlengthCurve(Todeconstruct_brep.Findbottombrep(miny[i].GetBoundingBox(true).ToBrep()));
                Point3d p1b = new Point3d(lenmbx.PointAtStart.X, lenmbx.PointAtStart.Y - sh1, lenmbx.PointAtStart.Z);
                Point3d p2b = new Point3d(lenmbx.PointAtEnd.X, lenmbx.PointAtEnd.Y - sh1, lenmbx.PointAtEnd.Z);
                List<Point3d> bpts = SpecialM.pointsatlength(p1b, p2b, cc);

                Curve lenmx = Todeconstruct_brep.FindlongestlengthCurve(Todeconstruct_brep.FindTopbrep(miny[i].GetBoundingBox(true).ToBrep()));
                Point3d p1t = new Point3d(lenmx.PointAtStart.X, lenmx.PointAtStart.Y + sh2, lenmx.PointAtStart.Z);
                Point3d p2t = new Point3d(lenmx.PointAtEnd.X, lenmx.PointAtEnd.Y + sh2, lenmx.PointAtEnd.Z);
                List<Point3d> tpts = SpecialM.pointsatlength(p1t, p2t, cc);
                for (int j = 0; j < tpts.Count; j++)
                {
                    Line l1 = new Line(bpts[j], tpts[j]);
                    Point3d p1 = new Point3d(bpts[j].X + 0.024, bpts[j].Y + 0.099, bpts[j].Z);
                    Point3d p2 = new Point3d(tpts[j].X + 0.024, tpts[j].Y + 0.099, tpts[j].Z);
                    Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p1);
                    Brep column = SpecialM.LoftcurveLengthwise(l1, height);
                    Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p2);
                    List<Brep> columjoin = new List<Brep>() { bjoint, column, tjoint };
                    Brep bp = Brep.MergeBreps(columjoin, 0.01);
                    minylen.Add(bp);
                }

            }

            List<int> indexes = new List<int>();
            List<Brep> newColumns = new List<Brep>();
            List<Brep> Beams_maxxlen = new List<Brep>();
            List<Brep> maxx_vindu_tp = new List<Brep>();
            List<Brep> maxx_vindu_bp = new List<Brep>();
            List<Brep> Columnsintheway = new List<Brep>();

            foreach (Brep bp in O_miny)
            {
                int Counter = 0;
                BoundingBox bx = bp.GetBoundingBox(true);
                for (int k = 0; k < minylen.Count; k++)
                {
                    Brep column = minylen[k];
                    Point3d pt = AreaMassProperties.Compute(column).Centroid;
                    bool intersec = bx.Contains(pt);
                    foreach (Brep wall in miny)
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
                            int index = minylen.IndexOf(column);
                            Point3d outerpoint = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(bx.ToBrep())).Centroid;
                            double oY = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(bx.ToBrep())).Centroid.X;
                            double iY = AreaMassProperties.Compute(Todeconstruct_brep.FindInnerXbrep(bx.ToBrep())).Centroid.X;
                            Point3d midpt = AreaMassProperties.Compute(bx.ToBrep()).Centroid;
                            double Ztp = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                            double Zbp = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                            Point3d bottompoint = new Point3d(pt.X - 0.1, pt.Y - w_len / 2, Zbp);
                            Point3d toppoint = new Point3d(pt.X - 0.1, pt.Y - w_len / 2, Ztp);
                            Line cutterline = new Line(bottompoint, toppoint);
                            BoundingBox cutter = SpecialM.CreateCutter(cutterline).GetBoundingBox(true);
                            Brep btpart = column.Split(cutter.ToBrep(), 0.01)[1];
                            Brep tpart = column.Split(cutter.ToBrep(), 0.01)[2];
                            minylen.RemoveAt(index);
                            minylen.Insert(index, tpart);
                            minylen.Insert(index, btpart);
                            Point3d x1 = AreaMassProperties.Compute(minylen[k - 1]).Centroid;
                            Point3d x2 = AreaMassProperties.Compute(minylen[k + 1]).Centroid;
                            double tol = 0.048;

                            if (Counter == 0)
                            {
                                double doorheight = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp1 = new Point3d(oY, x1.Y - sh2, doorheight);
                                Point3d bp2 = new Point3d(iY, x2.Y - sh2, doorheight);
                                Curve c1 = new Line(bp1, bp2).ToNurbsCurve();
                                Brep beam = SpecialM.LoftBeams(c1);
                                Beams_maxxlen.Add(beam);
                                allbeams.Add(beam);
                            }

                            if (bx.Contains(x1) != true && bx.Contains(x2) != true)
                            {
                                if (Counter != 2)
                                {
                                    if (Math.Abs(iY - x1.X) < tol)
                                    {
                                        Brep columnintheway = minylen[k - 1];
                                        int index1 = minylen.IndexOf(columnintheway);
                                        minylen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(iY - 0.072, x1.Y - sh2, startingheight);
                                        Point3d p2 = new Point3d(iY - 0.072, x1.Y - sh2, height + startingheight);
                                        Point3d p11 = new Point3d(iY - 0.048, x1.Y, startingheight);
                                        Point3d p21 = new Point3d(iY - 0.048, x1.Y, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(iY - x1.X) > tol)
                                    {
                                        Brep columnintheway = minylen[k - 1];
                                        int index1 = minylen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(iY - 0.048, x1.Y - sh2, startingheight);
                                        Point3d p2 = new Point3d(iY - 0.048, x1.Y - sh2, height + startingheight);
                                        Point3d p11 = new Point3d(iY - 0.024, x1.Y, startingheight);
                                        Point3d p21 = new Point3d(iY - 0.024, x1.Y, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    if (Math.Abs(oY - x2.X) < tol)
                                    {
                                        Brep columnintheway = minylen[k + 1];
                                        int index1 = minylen.IndexOf(columnintheway);
                                        //minylen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(oY - 0.048, x2.Y + sh2, startingheight);
                                        Point3d p2 = new Point3d(oY - 0.048, x2.Y + sh2, height + startingheight);
                                        Point3d p11 = new Point3d(oY + 0.024, x2.Y, startingheight);
                                        Point3d p21 = new Point3d(oY + 0.024, x2.Y, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(oY - x2.X) > tol)
                                    {
                                        Brep columnintheway = minylen[k + 1];
                                        int index1 = minylen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(oY, x2.Y - sh2, startingheight);
                                        Point3d p2 = new Point3d(oY, x2.Y - sh2, startingheight + height);
                                        Point3d p11 = new Point3d(oY + 0.024, x2.Y, startingheight);
                                        Point3d p21 = new Point3d(oY + 0.024, x2.Y, startingheight + height);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
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
                            int index = minylen.IndexOf(column);
                            Point3d outerpoint = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(bx.ToBrep())).Centroid;
                            Point3d outerpoint2 = AreaMassProperties.Compute(Todeconstruct_brep.FindInnerXbrep(bx.ToBrep())).Centroid;
                            double oY = outerpoint.X;
                            double iY = outerpoint2.X;
                            Point3d midpt = AreaMassProperties.Compute(bx.ToBrep()).Centroid;
                            double Ztp = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                            double Zbp = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                            Point3d bottompoint = new Point3d(pt.X - 0.1, pt.Y - w_len / 2, Zbp);
                            Point3d toppoint = new Point3d(pt.X - 0.1, pt.Y - w_len / 2, Ztp);
                            Line cutterline = new Line(bottompoint, toppoint);
                            BoundingBox cutter = SpecialM.CreateCutter(cutterline).GetBoundingBox(true);
                            Brep tpart = column.Split(cutter.ToBrep(), 0.01)[0];
                            Brep btpart = column.Split(cutter.ToBrep(), 0.01)[1];
                            minylen.RemoveAt(index);
                            minylen.Insert(index, btpart);
                            minylen.Insert(index, tpart);
                            Point3d x1 = AreaMassProperties.Compute(minylen[k - 1]).Centroid;
                            Point3d x2 = AreaMassProperties.Compute(minylen[k + 1]).Centroid;
                            double tol = 0.058;

                            if (Counter == 0)
                            {
                                double windowheight1 = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp1 = new Point3d(oY, x1.Y - sh2, windowheight1);
                                Point3d bp2 = new Point3d(iY, x2.Y - sh2, windowheight1);
                                Curve c1 = new Line(bp1, bp2).ToNurbsCurve();
                                Brep beam1 = SpecialM.LoftBeams(c1);
                                maxx_vindu_tp.Add(beam1);
                                allbeams.Add(beam1);
                                double windowheight2 = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp11 = new Point3d(oY, x1.Y - sh2, windowheight2 - 0.048);
                                Point3d bp12 = new Point3d(iY, x2.Y - sh2, windowheight2 - 0.048);
                                Curve c11 = new Line(bp11, bp12).ToNurbsCurve();
                                Brep beam2 = SpecialM.LoftBeams(c11);
                                maxx_vindu_bp.Add(beam2);
                                allbeams.Add(beam2);
                            }


                            if (bx.Contains(x1) != true && bx.Contains(x2) != true)
                            {
                                if (Counter != 2)
                                {
                                    if (Math.Abs(iY - x1.X) < tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = minylen[k - 1];
                                        int index1 = minylen.IndexOf(columnintheway);
                                        minylen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(iY - 0.072, x1.Y - sh2, h1);
                                        Point3d p2 = new Point3d(iY - 0.072, x1.Y - sh2, h2);
                                        Point3d p11 = new Point3d(iY - 0.048, x1.Y, h1);
                                        Point3d p21 = new Point3d(iY - 0.048, x1.Y, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }

                                    else if (Math.Abs(iY - x1.X) > tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = minylen[k - 1];
                                        int index1 = minylen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(iY - 0.048, x1.Y - sh2, h1);
                                        Point3d p2 = new Point3d(iY - 0.048, x1.Y - sh2, h2);
                                        Point3d p11 = new Point3d(iY - 0.024, x1.Y, h1);
                                        Point3d p21 = new Point3d(iY - 0.024, x1.Y, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }

                                    if (Math.Abs(oY - x2.X) < tol)
                                    {
                                        double h1 = x2.Z - height / 2;
                                        double h2 = x2.Z + height / 2;
                                        Brep columnintheway = minylen[k + 1];
                                        int index1 = minylen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(oY - 0.072, x2.Y - sh2, h1);
                                        Point3d p2 = new Point3d(oY - 0.072, x2.Y - sh2, h2);
                                        Point3d p11 = new Point3d(iY + 0.048, x1.Y, h1);
                                        Point3d p21 = new Point3d(iY + 0.048, x1.Y, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(oY - x2.X) > tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = minylen[k + 1];
                                        int index1 = minylen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(oY, x1.Y - sh2, h1);
                                        Point3d p2 = new Point3d(oY, x1.Y - sh2, h2);
                                        Point3d p11 = new Point3d(oY + 0.024, x2.Y, h1);
                                        Point3d p21 = new Point3d(oY + 0.024, x2.Y, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
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

            foreach (Brep wall in miny)
            {
                double wallZ = AreaMassProperties.Compute(wall).Centroid.Z;
                BoundingBox thewall = wall.GetBoundingBox(true);
                foreach (Brep op in O_miny)
                {
                    BoundingBox box = op.GetBoundingBox(true);
                    Point3d thept = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(box.ToBrep())).Centroid;
                    Point3d mainpt = box.Center;
                    if (thewall.Contains(mainpt))
                    {
                        List<int> indexesofremove = new List<int>();
                        List<Brep> colmtoadd = new List<Brep>();
                        foreach (Brep colm in minylen)
                        {
                            Point3d pt = AreaMassProperties.Compute(colm).Centroid;
                            if (Math.Abs(wallZ - pt.Z) < 0.1)
                            {
                                double diff = Math.Abs(thept.X - pt.X);
                                if (diff < 0.048)
                                {
                                    int index = minylen.IndexOf(colm);
                                    Point3d p1 = new Point3d(pt.X + diff, pt.Y - 0.099, pt.Z - (height / 2));
                                    Point3d p2 = new Point3d(pt.X + diff, pt.Y - 0.099, pt.Z + (height / 2));
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
                            minylen.RemoveAt(indexesofremove[i]);
                            minylen.Insert(indexesofremove[i], colmtoadd[i]);
                        }
                    }

                }
            }
            for (int b = 0; b < newColumns.Count; b++)
            {
                minylen.Insert(indexes[b], newColumns[b]);
            }

            //For dører 
            foreach (Brep beam in Beams_maxxlen)
            {
                BoundingBox beambx = beam.GetBoundingBox(true);
                for (int n = 0; n < minylen.Count; n++)
                {
                    Brep column = minylen[n];
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
                        int index = minylen.IndexOf(column);
                        Point3d stpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ - 0.024);
                        Point3d endpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ + 0.024);
                        Line ll1 = new Line(stpoint, endpoint);
                        Brep cutter = SpecialM.CreateCutter(ll1);
                        minylen.RemoveAt(index);
                        Brep tpart = column.Split(cutter, 0.01)[0];
                        minylen.Insert(index, tpart);
                    }
                }
            }
            foreach (Brep wall in miny)
            {
                double topheightofwall = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(wall)).Centroid.Z;
                double bottomofwall = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(wall)).Centroid.Z;
                BoundingBox Wallbox = wall.GetBoundingBox(true);
                Point3d pl1 = Wallbox.GetCorners()[3];
                Point3d pl2 = Wallbox.GetCorners()[2];
                Point3d pfl1 = new Point3d(pl1.X, pl1.Y, pl1.Z - 0.048);
                Point3d pfl2 = new Point3d(pl2.X, pl2.Y, pl2.Z - 0.048);
                Curve l1 = new Line(pfl1, pfl2).ToNurbsCurve();
                allbeams.Add(SpecialM.LoftBeams(l1));
                foreach (Brep beam in maxx_vindu_tp)
                {
                    BoundingBox beambx = beam.GetBoundingBox(true);
                    for (int n = 0; n < minylen.Count; n++)
                    {
                        Brep column = minylen[n];
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
                            int index = minylen.IndexOf(column);
                            Point3d stpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ - 0.024);
                            Point3d endpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ + 0.024);
                            Line ll1 = new Line(stpoint, endpoint);
                            Brep cutter = SpecialM.CreateCutter(ll1);
                            Brep tpart = column.Split(cutter, 0.01)[0];
                            minylen.RemoveAt(index);
                            minylen.Insert(index, tpart);
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
                        for (int n = 0; n < minylen.Count; n++)
                        {
                            Brep column = minylen[n];
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
                                int index = minylen.IndexOf(column);
                                Point3d stpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ - 0.024);
                                Point3d endpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ + 0.024);
                                Line ll1 = new Line(stpoint, endpoint);
                                Brep cutter = SpecialM.CreateCutter(ll1);
                                Brep bpart = column.Split(cutter, 0.01)[0];
                                minylen.RemoveAt(index);
                                minylen.Insert(index, bpart);
                            }

                        }
                    }
                }
                Point3d pu1 = Wallbox.GetCorners()[7];
                Point3d pu2 = Wallbox.GetCorners()[6];
                Point3d pfu1 = new Point3d(pu1.X, pu1.Y, pu1.Z);
                Point3d pfu2 = new Point3d(pu2.X, pu2.Y, pu2.Z);
                Curve u1 = new Line(pfu1, pfu2).ToNurbsCurve();
                allbeams.Add(SpecialM.LoftBeams(u1));
            }
            output.Add(minylen);
            output.Add(allbeams);

            return output;
        }
    
        public static List<List<Brep>> maxylen (List<Brep> maxy, double cc, double height, int numfloors, double startingheight)
        {
            List<List<Brep>> output = new List<List<Brep>>();
            List<Brep> maxylen = new List<Brep>();
            List<Brep> allbeams = new List<Brep>();

            List<Point3d> maxypts = ClassForOpenings.sortingPointsaccourdingZXY(maxy);
            List<List<Point3d>> maxypts1 = ClassForOpenings.createlistforopnings(maxypts, height, numfloors);
            maxypts1 = ClassForOpenings.makesureptsare8ineverylist(maxypts1);
            List<Brep> O_maxy = ClassForOpenings.CreateCutterFormpositivretning(maxypts1);
            double w_len = 0;
            for (int i = 0; i < maxy.Count; i++)
            {
                w_len = Todeconstruct_brep.FindShortestlength(maxy[i]);
                Curve lenmbx = Todeconstruct_brep.FindlongestlengthCurve(Todeconstruct_brep.Findbottombrep(maxy[i].GetBoundingBox(true).ToBrep()));
                Point3d p1b = new Point3d(lenmbx.PointAtStart.X, lenmbx.PointAtStart.Y - 0.3, lenmbx.PointAtStart.Z);
                Point3d p2b = new Point3d(lenmbx.PointAtEnd.X, lenmbx.PointAtEnd.Y - 0.3, lenmbx.PointAtEnd.Z);
                List<Point3d> bpts = SpecialM.pointsatlength(p1b, p2b, cc);

                Curve lenmx = Todeconstruct_brep.FindlongestlengthCurve(Todeconstruct_brep.FindTopbrep(maxy[i].GetBoundingBox(true).ToBrep()));
                Point3d p1t = new Point3d(lenmx.PointAtStart.X, lenmx.PointAtStart.Y, lenmx.PointAtStart.Z);
                Point3d p2t = new Point3d(lenmx.PointAtEnd.X, lenmx.PointAtEnd.Y, lenmx.PointAtEnd.Z);
                List<Point3d> tpts = SpecialM.pointsatlength(p1t, p2t, cc);
                for (int j = 0; j < tpts.Count; j++)
                {
                    Line l1 = new Line(bpts[j], tpts[j]);
                    Point3d p1 = new Point3d(bpts[j].X + 0.024, bpts[j].Y + 0.099, bpts[j].Z);
                    Point3d p2 = new Point3d(tpts[j].X + 0.024, tpts[j].Y + 0.099, tpts[j].Z);
                    Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p1);
                    Brep column = SpecialM.LoftcurveLengthwise(l1, height);
                    Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p2);
                    List<Brep> columjoin = new List<Brep>() { bjoint, column, tjoint };
                    Brep bp = Brep.MergeBreps(columjoin, 0.01);
                    maxylen.Add(bp);
                }

            }

            List<int> indexes = new List<int>();
            List<Brep> newColumns = new List<Brep>();
            List<Brep> Beams_maxxlen = new List<Brep>();
            List<Brep> maxx_vindu_tp = new List<Brep>();
            List<Brep> maxx_vindu_bp = new List<Brep>();

            foreach (Brep bp in O_maxy)
            {
                int Counter = 0;
                BoundingBox bx = bp.GetBoundingBox(true);
                for (int k = 0; k < maxylen.Count; k++)
                {
                    Brep column = maxylen[k];
                    Point3d pt = AreaMassProperties.Compute(column).Centroid;
                    bool intersec = bx.Contains(pt);
                    foreach (Brep wall in maxy)
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
                            int index = maxylen.IndexOf(column);
                            maxylen.RemoveAt(index);
                            Brep tpart = column.Split(bx.ToBrep(), 0.01)[2];
                            Brep btpart = column.Split(bx.ToBrep(), 0.01)[1];
                            maxylen.Insert(index, tpart);
                            maxylen.Insert(index, btpart);

                            double oY = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(bx.ToBrep())).Centroid.X;
                            double iY = AreaMassProperties.Compute(Todeconstruct_brep.FindInnerXbrep(bx.ToBrep())).Centroid.X;

                            Point3d x1 = AreaMassProperties.Compute(maxylen[k - 1]).Centroid;
                            Point3d x2 = AreaMassProperties.Compute(maxylen[k + 1]).Centroid;
                            double tol = 0.048;

                            if (Counter == 0)
                            {
                                double doorheight = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp1 = new Point3d(oY, x1.Y - 0.099, doorheight);
                                Point3d bp2 = new Point3d(iY, x2.Y - 0.099, doorheight);
                                Curve c1 = new Line(bp1, bp2).ToNurbsCurve();
                                Brep beam = SpecialM.LoftBeams(c1);
                                Beams_maxxlen.Add(beam);
                                allbeams.Add(beam);
                            }

                            if (bx.Contains(x1) != true && bx.Contains(x2) != true)
                            {
                                if (Counter != 2)
                                {
                                    if (Math.Abs(iY - x1.X) < tol)
                                    {
                                        Brep columnintheway = maxylen[k - 1];
                                        int index1 = maxylen.IndexOf(columnintheway);
                                        maxylen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(iY - 0.048, x1.Y - 0.099, startingheight);
                                        Point3d p2 = new Point3d(iY - 0.048, x1.Y - 0.099, height + startingheight);
                                        Point3d p11 = new Point3d(iY - 0.024, x1.Y, startingheight);
                                        Point3d p21 = new Point3d(iY - 0.024, x1.Y, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(iY - x1.X) > tol)
                                    {
                                        Brep columnintheway = maxylen[k - 1];
                                        int index1 = maxylen.IndexOf(columnintheway);
                                        maxylen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(iY, x1.Y, startingheight);
                                        Point3d p2 = new Point3d(iY, x1.Y, height + startingheight);
                                        Point3d p11 = new Point3d(iY, x1.Y, startingheight);
                                        Point3d p21 = new Point3d(iY, x1.Y, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    if (Math.Abs(oY - x2.X) < tol)
                                    {
                                        Brep columnintheway = maxylen[k + 1];
                                        int index1 = maxylen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(oY - 0.048, x2.Y + 0.099, startingheight);
                                        Point3d p2 = new Point3d(oY - 0.048, x2.Y + 0.099, height + startingheight);
                                        Point3d p11 = new Point3d(oY + 0.072, x2.Y, startingheight);
                                        Point3d p21 = new Point3d(oY + 0.072, x2.Y, height + startingheight);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(oY - x2.X) > tol)
                                    {
                                        Brep columnintheway = maxylen[k + 1];
                                        int index1 = maxylen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(oY, x2.Y - 0.099, startingheight);
                                        Point3d p2 = new Point3d(oY, x2.Y - 0.099, startingheight + height);
                                        Point3d p11 = new Point3d(oY + 0.024, x2.Y, startingheight);
                                        Point3d p21 = new Point3d(oY + 0.024, x2.Y, startingheight + height);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
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
                            int index = maxylen.IndexOf(column);
                            Point3d outerpoint = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(bx.ToBrep())).Centroid;
                            Point3d outerpoint2 = AreaMassProperties.Compute(Todeconstruct_brep.FindInnerXbrep(bx.ToBrep())).Centroid;
                            double oY = outerpoint.X;
                            double iY = outerpoint2.X;

                            Point3d midpt = AreaMassProperties.Compute(bx.ToBrep()).Centroid;
                            Brep tpart = new Brep();
                            Brep btpart = new Brep();

                            if (pt.Z <= outerpoint.Z)
                            {
                                double Ztp = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                                double Zbp = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                                Point3d bottompoint = new Point3d(pt.X - 0.1, pt.Y - 0.1, Zbp);
                                Point3d toppoint = new Point3d(pt.X - 0.1, pt.Y - 0.1, Ztp);
                                Line cutterline = new Line(bottompoint, toppoint);
                                BoundingBox cutter = SpecialM.CreateCutter(cutterline).GetBoundingBox(true);
                                tpart = column.Split(cutter.ToBrep(), 0.01)[2];
                                btpart = column.Split(cutter.ToBrep(), 0.01)[1];
                                maxylen.RemoveAt(index);
                            }
                            else
                            {
                                maxylen.RemoveAt(index);
                                tpart = column.Split(bx.ToBrep(), 0.01)[2];
                                btpart = column.Split(bx.ToBrep(), 0.01)[1];
                            }
                            maxylen.Insert(index, tpart);
                            maxylen.Insert(index, btpart);
                            Point3d x1 = AreaMassProperties.Compute(maxylen[k - 1]).Centroid;
                            Point3d x2 = AreaMassProperties.Compute(maxylen[k + 1]).Centroid;
                            double tol = 0.048;

                            if (Counter == 0)
                            {
                                double windowheight1 = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp1 = new Point3d(oY, x1.Y - 0.099, windowheight1);
                                Point3d bp2 = new Point3d(iY, x2.Y - 0.099, windowheight1);
                                Curve c1 = new Line(bp1, bp2).ToNurbsCurve();
                                Brep beam1 = SpecialM.LoftBeams(c1);
                                maxx_vindu_tp.Add(beam1);
                                allbeams.Add(beam1);
                                double windowheight2 = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(bx.ToBrep())).Centroid.Z;
                                Point3d bp11 = new Point3d(oY, x1.Y - 0.099, windowheight2 - 0.048);
                                Point3d bp12 = new Point3d(iY, x2.Y - 0.099, windowheight2 - 0.048);
                                Curve c11 = new Line(bp11, bp12).ToNurbsCurve();
                                Brep beam2 = SpecialM.LoftBeams(c11);
                                maxx_vindu_bp.Add(beam2);
                                allbeams.Add(beam2);
                            }


                            if (bx.Contains(x1) != true && bx.Contains(x2) != true)
                            {
                                if (Counter != 2)
                                {
                                    if (Math.Abs(iY - x1.X) < tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = maxylen[k - 1];
                                        int index1 = maxylen.IndexOf(columnintheway);
                                        //maxylen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(iY - 0.048, x1.Y - 0.099, h1);
                                        Point3d p2 = new Point3d(iY - 0.048, x1.Y - 0.099, h2);
                                        Point3d p11 = new Point3d(iY - 0.024, x1.Y, h1);
                                        Point3d p21 = new Point3d(iY - 0.024, x1.Y, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }

                                    else if (Math.Abs(iY - x1.X) > tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = maxylen[k - 1];
                                        int index1 = maxylen.IndexOf(columnintheway);
                                        //maxylen.RemoveAt(index1);
                                        Point3d p1 = new Point3d(iY - 0.048, x1.Y - 0.099, h1);
                                        Point3d p2 = new Point3d(iY - 0.048, x1.Y - 0.099, h2);
                                        Point3d p11 = new Point3d(iY - 0.024, x1.Y, h1);
                                        Point3d p21 = new Point3d(iY - 0.024, x1.Y, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }

                                    if (Math.Abs(oY - x2.X) < tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = maxylen[k + 1];
                                        int index1 = maxylen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(oY - 0.048, x2.Y + 0.099, h1);
                                        Point3d p2 = new Point3d(oY - 0.048, x2.Y + 0.099, h2);
                                        Point3d p11 = new Point3d(iY - 0.024, x1.Y, h1);
                                        Point3d p21 = new Point3d(iY - 0.024, x1.Y, h2);
                                        Line l1 = new Line(p1, p2);

                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
                                        List<Brep> columjoin = new List<Brep>() { bjoint, maincolm, tjoint };
                                        Brep Newcolumn = Brep.MergeBreps(columjoin, 0.01);
                                        indexes.Add(index1);
                                        newColumns.Add(Newcolumn);
                                        Counter++;
                                    }
                                    else if (Math.Abs(oY - x2.X) > tol)
                                    {
                                        double h1 = x1.Z - height / 2;
                                        double h2 = x1.Z + height / 2;
                                        Brep columnintheway = maxylen[k + 1];
                                        int index1 = maxylen.IndexOf(columnintheway);
                                        Point3d p1 = new Point3d(oY, x2.Y - 0.099, h1);
                                        Point3d p2 = new Point3d(oY, x2.Y - 0.099, h2);
                                        Point3d p11 = new Point3d(oY + 0.024, x2.Y, h1);
                                        Point3d p21 = new Point3d(oY + 0.024, x2.Y, h2);
                                        Line l1 = new Line(p1, p2);
                                        Brep bjoint = BoxJoint.CreateJointFromBottompointLenght(p11);
                                        Brep maincolm = SpecialM.LoftcurveLengthwise(l1, height);
                                        Brep tjoint = BoxJoint.CreateJointFromToppointLenght(p21);
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
            foreach (Brep wall in maxy)
            {
                double wallZ = AreaMassProperties.Compute(wall).Centroid.Z;
                BoundingBox thewall = wall.GetBoundingBox(true);
                foreach (Brep op in O_maxy)
                {
                    BoundingBox box = op.GetBoundingBox(true);
                    Point3d thept = AreaMassProperties.Compute(Todeconstruct_brep.FindOuterXbrep(box.ToBrep())).Centroid;
                    Point3d mainpt = box.Center;
                    if (thewall.Contains(mainpt))
                    {
                        List<int> indexesofremove = new List<int>();
                        List<Brep> colmtoadd = new List<Brep>();
                        foreach (Brep colm in maxylen)
                        {
                            Point3d pt = AreaMassProperties.Compute(colm).Centroid;
                            if (Math.Abs(wallZ - pt.Z) < 0.1)
                            {
                                double diff = Math.Abs(thept.X - pt.X);
                                if (diff < 0.023)
                                {
                                    int index = maxylen.IndexOf(colm);
                                    Point3d p1 = new Point3d(pt.X + diff, pt.Y - 0.099, pt.Z - (height / 2));
                                    Point3d p2 = new Point3d(pt.X + diff, pt.Y - 0.099, pt.Z + (height / 2));
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
                            maxylen.RemoveAt(indexesofremove[i]);
                            maxylen.Insert(indexesofremove[i], colmtoadd[i]);
                        }

                    }

                }
            }
            for (int b = 0; b < newColumns.Count; b++)
            {
                maxylen.Insert(indexes[b], newColumns[b]);
            }

            //For dører 
            foreach (Brep beam in Beams_maxxlen)
            {
                BoundingBox beambx = beam.GetBoundingBox(true);
                for (int n = 0; n < maxylen.Count; n++)
                {
                    Brep column = maxylen[n];
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
                        int index = maxylen.IndexOf(column);
                        Point3d stpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ - 0.024);
                        Point3d endpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ + 0.024);
                        Line ll1 = new Line(stpoint, endpoint);
                        Brep cutter = SpecialM.CreateCutter(ll1);
                        maxylen.RemoveAt(index);
                        Brep tpart = column.Split(cutter, 0.01)[0];
                        maxylen.Insert(index, tpart);
                    }
                }
            }
            foreach (Brep wall in maxy)
            {
                double topheightofwall = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(wall)).Centroid.Z;
                double bottomofwall = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(wall)).Centroid.Z;
                BoundingBox Wallbox = wall.GetBoundingBox(true);
                Point3d pl1 = Wallbox.GetCorners()[0];
                Point3d pl2 = Wallbox.GetCorners()[1];
                Point3d pfl1 = new Point3d(pl1.X, pl1.Y + 0.198, pl1.Z - 0.048);
                Point3d pfl2 = new Point3d(pl2.X, pl2.Y + 0.198, pl2.Z - 0.048);
                Curve l1 = new Line(pfl1, pfl2).ToNurbsCurve();
                allbeams.Add(SpecialM.LoftBeams(l1));
                foreach (Brep beam in maxx_vindu_tp)
                {
                    BoundingBox beambx = beam.GetBoundingBox(true);
                    for (int n = 0; n < maxylen.Count; n++)
                    {
                        Brep column = maxylen[n];
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
                            int index = maxylen.IndexOf(column);
                            Point3d stpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ - 0.024);
                            Point3d endpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ + 0.024);
                            Line ll1 = new Line(stpoint, endpoint);
                            Brep cutter = SpecialM.CreateCutter(ll1);
                            Brep tpart = column.Split(cutter, 0.01)[0];
                            maxylen.RemoveAt(index);
                            maxylen.Insert(index, tpart);
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
                        for (int n = 0; n < maxylen.Count; n++)
                        {
                            Brep column = maxylen[n];
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
                                int index = maxylen.IndexOf(column);
                                Point3d stpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ - 0.024);
                                Point3d endpoint = new Point3d(pt.X - 0.1, pt.Y - 0.11, bbZ + 0.024);
                                Line ll1 = new Line(stpoint, endpoint);
                                Brep cutter = SpecialM.CreateCutter(ll1);
                                Brep bpart = column.Split(cutter, 0.01)[0];
                                maxylen.RemoveAt(index);
                                maxylen.Insert(index, bpart);
                            }

                        }
                    }
                }
                Point3d pu1 = Wallbox.GetCorners()[4];
                Point3d pu2 = Wallbox.GetCorners()[5];
                Point3d pfu1 = new Point3d(pu1.X, pu1.Y + 0.198, pu1.Z);
                Point3d pfu2 = new Point3d(pu2.X, pu2.Y + 0.198, pu2.Z);
                Curve u1 = new Line(pfu1, pfu2).ToNurbsCurve();
                allbeams.Add(SpecialM.LoftBeams(u1));
            }
            output.Add(maxylen);
            output.Add(allbeams);
            return output;
        }
    }
}
