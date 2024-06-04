using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types.Transforms;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace MasterThesisCode1
{
    public class CreateBuilding : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public CreateBuilding()
          : base("CreateBuildingframework", "CreateBuilding",
            "Description",
            "NTNU", "Master")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Houseform", "HF", "Houseform in brep", GH_ParamAccess.item);
            pManager.AddNumberParameter("StartingHeigth", "sh", "Starting heigth of building", GH_ParamAccess.item);
            pManager.AddNumberParameter("FloorHeight", "H", "Height of the Floors, must be between 2.4m and 3.8 m", GH_ParamAccess.item);
            pManager.AddNumberParameter("CenterDistance", "cc", "Center distance of the studs", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Number of floors", "nf", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("MiddelColumns", "", "Columns in building", GH_ParamAccess.list);
            pManager.AddBrepParameter("EndColumns", "", "Columns in building", GH_ParamAccess.list);
            pManager.AddBrepParameter("Beams", "", "Beams in studwork", GH_ParamAccess.list);
            pManager.AddNumberParameter("Ratios", "", "", GH_ParamAccess.list);
            pManager.AddBrepParameter("Beams inbetween floors", "", "Beams in Floor", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep HF = new Brep();
            double sh = new double();
            double H = new double();
            double cc = new double();
            //int n = 0;

            DA.GetData(0, ref HF);
            DA.GetData(1, ref sh);
            DA.GetData(2, ref H);
            DA.GetData(3, ref cc);
            //DA.GetData(4, ref n);

            Brep topbrep = Todeconstruct_brep.FindTopbrep(HF);
            Brep bottombrep = Todeconstruct_brep.Findbottombrep(HF);

            sh = sh + 0.048;
            double tpoint = AreaMassProperties.Compute(topbrep).Centroid.Z - sh;
            int n = Convert.ToInt32(tpoint / H);
            BrepEdgeList c1 = bottombrep.Edges;
            double L = c1[0].GetLength()-0.198;
            double W = c1[1].GetLength()-0.198;

            List<NurbsSurface> Floors = new List<NurbsSurface>();
            List<Point3d> endpoints = new List<Point3d>();
            List<Point3d> points = new List<Point3d>();

            List<Curve> Lbeams = new List<Curve>();
            List<Curve> Wbeams = new List<Curve>();

            List<List<Point3d>> outpts1 = new List<List<Point3d>>();
            List<List<Point3d>> outpts2 = new List<List<Point3d>>();
            List<List<Point3d>> outpts3 = new List<List<Point3d>>();
            List<List<Point3d>> outpts4 = new List<List<Point3d>>();

            List<List<Point3d>> Lmidpoints = new List<List<Point3d>>();

            //Creating points for the different height as well as floors. 
            for (int i = 0; i < n + 1; i++)
            {

                Point3d p1 = new Point3d(0, 0, sh + H * i);
                Point3d p2 = new Point3d(L, 0, sh + H * i);
                Point3d p3 = new Point3d(L, W, sh + H * i);
                Point3d p4 = new Point3d(0, W, sh + H * i);

                List<Point3d> pts = new List<Point3d>() { p1, p4, p2, p3};
                List<Point3d> pt1 = new List<Point3d> {p1};
                List<Point3d> pt2 = new List<Point3d> {p2};
                List<Point3d> pt3 = new List<Point3d> {p4};
                List<Point3d> pt4 = new List<Point3d> {p4};


                var num1 = (L * 10) % (cc * 10);
                var num2 = (W * 10) % (cc * 10);


                List<Point3d>  pts1 = SpecialM.pointsatlength(p1, p2, cc);
                List<Point3d> ptsm1 = SpecialM.FindMidPoints(num1, pts1);
                outpts1.Add(ptsm1);
                for (int j = 0; j < ptsm1.Count; j++)
                {
                    points.Add(ptsm1[j]);
                    pt1.Add(ptsm1[j]);
                }
                pt1.Add(p2);

                List<Point3d> pts2 = SpecialM.pointsatlength(p2, p3, cc);
                List<Point3d> ptsm2 = SpecialM.FindMidPoints(num2, pts2);
                outpts2.Add(ptsm2);
                for (int j = 0; j < ptsm2.Count; j++)
                {
                    points.Add(ptsm2[j]);
                    pt2.Add(ptsm2[j]);
                }
                pt2.Add(p3);

                List<Point3d> pts3 = SpecialM.pointsatlength(p4, p3, cc);
                List<Point3d> ptsm3 = SpecialM.FindMidPoints(num1, pts3);
                outpts3.Add(ptsm3);
                for (int j = 0; j < ptsm3.Count; j++)
                {
                    points.Add(ptsm3[j]);
                    pt3.Add(ptsm3[j]);
                }
                pt3.Add(p3);


                if (i != 0 && i != n)
                {
                    List<Point3d> bb = new List<Point3d>();
                    List<Point3d> aa = new List<Point3d>();
                    for (int j = 0; j < pt1.Count; j++)
                    {
                        Point3d b = new Point3d(pt1[j].X, pt1[j].Y  + 0.099, pt1[j].Z + 0.048);
                        Point3d a = new Point3d(pt3[j].X, pt3[j].Y + 0.099, pt3[j].Z + 0.048);
                        bb.Add(b);
                        aa.Add(a);
                    }
                    Lmidpoints.Add(bb);
                    Lmidpoints.Add(aa);
                }

                List<Point3d> pts4 = SpecialM.pointsatlength(p1, p4, cc);
                List<Point3d> ptsm4 = SpecialM.FindMidPoints(num2 , pts4);
                outpts4.Add(ptsm4);
                for (int j = 0; j < ptsm4.Count; j++)
                {
                    points.Add(ptsm4[j]);
                    pt4.Add(ptsm4[j]);
                }
                pt4.Add(p1);


                if (i == n)
                {
                    List<Point3d> pt1moved = new List<Point3d>();
                    List<Point3d> pt2moved = new List<Point3d>();
                    List<Point3d> pt3moved = new List<Point3d>();
                    List<Point3d> pt4moved = new List<Point3d>();
                    for (int j  = 0; j < pt1.Count; j++)
                    {
                        Point3d d = new Point3d(pt1[j].X, pt1[j].Y+0.198, pt1[j].Z + 0.341);
                        Point3d dd = new Point3d(pt3[j].X + 0.198, pt3[j].Y + 0.198, pt3[j].Z + 0.341);
                        pt1moved.Add(d);
                        pt3moved.Add(dd);
                    }
                    for (int j = 0; j < pt2.Count; j++)
                    {
                        Point3d d = new Point3d(pt2[j].X, pt2[j].Y, pt2[j].Z + 0.341);
                        Point3d dd = new Point3d(pt4[j].X + 0.198, pt4[j].Y + 0.198, pt4[j].Z + 0.341);
                        pt2moved.Add(d);
                        pt4moved.Add(dd);
                    }
                    Curve LLL1 = Curve.CreateControlPointCurve(pt1moved, 1);
                    Curve LLL2 = Curve.CreateControlPointCurve(pt2moved, 1);
                    Curve LLL3 = Curve.CreateControlPointCurve(pt3moved, 1);
                    Curve LLL4 = Curve.CreateControlPointCurve(pt4moved, 1);
                    Lbeams.Add(LLL1);
                    Lbeams.Add(LLL3);
                    Wbeams.Add(LLL2);
                    Wbeams.Add(LLL4);
                }
                else if (i != 0 && i != n) 
                {
                    List<Point3d> pt11moved = new List<Point3d>();
                    List<Point3d> pt21moved = new List<Point3d>();
                    List<Point3d> pt31moved = new List<Point3d>();
                    List<Point3d> pt41moved = new List<Point3d>();
                    List<Point3d> pt1moved = new List<Point3d>();
                    List<Point3d> pt2moved = new List<Point3d>();
                    List<Point3d> pt3moved = new List<Point3d>();
                    List<Point3d> pt4moved = new List<Point3d>();
                    for (int j = 0; j < pt1.Count; j++)
                    {
                        Point3d d = new Point3d(pt1[j].X, pt1[j].Y+0.198, pt1[j].Z );
                        Point3d dd = new Point3d(pt3[j].X + 0.198, pt3[j].Y + 0.198, pt3[j].Z);
                        pt1moved.Add(d);
                        pt3moved.Add(dd);
                    }
                    for (int j = 0; j < pt2.Count; j++)
                    {
                        Point3d d = new Point3d(pt2[j].X, pt2[j].Y, pt2[j].Z);
                        Point3d dd = new Point3d(pt4[j].X + 0.198, pt4[j].Y + 0.198, pt4[j].Z);
                        pt2moved.Add(d);
                        pt4moved.Add(dd);
                    }
                    Curve LLL1 = Curve.CreateControlPointCurve(pt1moved, 1);
                    Curve LLL2 = Curve.CreateControlPointCurve(pt2moved, 1);
                    Curve LLL3 = Curve.CreateControlPointCurve(pt3moved, 1);
                    Curve LLL4 = Curve.CreateControlPointCurve(pt4moved, 1);
                    Lbeams.Add(LLL1);
                    Lbeams.Add(LLL3);
                    Wbeams.Add(LLL2);
                    Wbeams.Add(LLL4);
                    for (int j = 0; j < pt1.Count; j++)
                    {
                        Point3d d = new Point3d(pt1[j].X, pt1[j].Y + 0.198, pt1[j].Z + 0.293);
                        Point3d dd = new Point3d(pt3[j].X + 0.198, pt3[j].Y + 0.198, pt3[j].Z + 0.293);
                        pt11moved.Add(d);
                        pt31moved.Add(dd);
                    }
                    for (int j = 0; j < pt2.Count; j++)
                    {
                        Point3d d = new Point3d(pt2[j].X, pt2[j].Y, pt2[j].Z + 0.293);
                        Point3d dd = new Point3d(pt4[j].X + 0.198, pt4[j].Y + 0.198, pt4[j].Z + 0.293);
                        pt21moved.Add(d);
                        pt41moved.Add(dd);
                    }
                    Curve LLL11 = Curve.CreateControlPointCurve(pt11moved, 1);
                    Curve LLL21 = Curve.CreateControlPointCurve(pt21moved, 1);
                    Curve LLL31 = Curve.CreateControlPointCurve(pt31moved, 1);
                    Curve LLL41 = Curve.CreateControlPointCurve(pt41moved, 1);
                    Lbeams.Add(LLL11);
                    Lbeams.Add(LLL31);
                    Wbeams.Add(LLL21);
                    Wbeams.Add(LLL41);
                }
                else 
                {
                    List<Point3d> pt1moved = new List<Point3d>();
                    List<Point3d> pt2moved = new List<Point3d>();
                    List<Point3d> pt3moved = new List<Point3d>();
                    List<Point3d> pt4moved = new List<Point3d>();
                    for (int j = 0; j < pt1.Count; j++)
                    {
                        Point3d d = new Point3d(pt1[j].X, pt1[j].Y + 0.198, pt1[j].Z-0.048);
                        Point3d dd = new Point3d(pt3[j].X + 0.198, pt3[j].Y + 0.198, pt3[j].Z - 0.048);
                        pt1moved.Add(d);
                        pt3moved.Add(dd);
                    }
                    for (int j = 0; j < pt2.Count; j++)
                    {
                        Point3d d = new Point3d(pt2[j].X, pt2[j].Y, pt2[j].Z - 0.048);
                        Point3d dd = new Point3d(pt4[j].X + 0.198, pt4[j].Y + 0.198 , pt4[j].Z - 0.048);
                        pt2moved.Add(d);
                        pt4moved.Add(dd);
                    }
                    Curve LLL1 = Curve.CreateControlPointCurve(pt1moved, 1);
                    Curve LLL2 = Curve.CreateControlPointCurve(pt2moved, 1);
                    Curve LLL3 = Curve.CreateControlPointCurve(pt3moved, 1);
                    Curve LLL4 = Curve.CreateControlPointCurve(pt4moved, 1);
                    Lbeams.Add(LLL1);
                    Lbeams.Add(LLL3);
                    Wbeams.Add(LLL2);
                    Wbeams.Add(LLL4);
                }
              

                NurbsSurface srf = NurbsSurface.CreateFromPoints(pts, 2, 2, 1, 1);
                Floors.Add(srf);
                if ( i != 0 && i != n)
                {
                    endpoints.Add(new Point3d(p1.X, p1.Y, p1.Z));
                    endpoints.Add(new Point3d(p2.X , p2.Y, p2.Z));
                    endpoints.Add(new Point3d(p3.X , p3.Y, p3.Z));
                    endpoints.Add(new Point3d(p4.X, p4.Y , p4.Z));
                    endpoints.Add(new Point3d(p1.X, p1.Y,p1.Z + 0.341));
                    endpoints.Add(new Point3d(p2.X , p2.Y, p2.Z + 0.341));
                    endpoints.Add(new Point3d(p3.X , p3.Y, p3.Z + 0.341));
                    endpoints.Add(new Point3d(p4.X, p4.Y, p4.Z + 0.341));
                }
                else if (i == n)
                {
                    endpoints.Add(new Point3d(p1.X, p1.Y, p1.Z + 0.341));
                    endpoints.Add(new Point3d(p2.X, p2.Y, p2.Z + 0.341));
                    endpoints.Add(new Point3d(p3.X, p3.Y, p3.Z + 0.341));
                    endpoints.Add(new Point3d(p4.X, p4.Y, p4.Z + 0.341));
                }
                else 
                {
                    endpoints.Add(new Point3d(p1.X, p1.Y, p1.Z));
                    endpoints.Add(new Point3d(p2.X, p2.Y, p2.Z));
                    endpoints.Add(new Point3d(p3.X, p3.Y, p3.Z));
                    endpoints.Add(new Point3d(p4.X, p4.Y, p4.Z));
                }
            }
            
            //Creating brep of the columns.
            List<Brep> br_columns = new List<Brep>();
            //Splitting Curves accourding to Which side they're on. So that the studwork is accourding 
            List<Line> l1 = SpecialM.moveptscreatecolumns(outpts1);
            List<Line> l2 = SpecialM.moveptscreatecolumns(outpts2);
            List<Line> l3 = SpecialM.moveptscreatecolumns(outpts3);
            List<Line> l4 = SpecialM.moveptscreatecolumns(outpts4);
            
            for (int b = 0; b < l1.Count; b++)
            {
                br_columns.Add(SpecialM.LoftcurveLengthwise(l1[b], H).CapPlanarHoles(0.01));
                br_columns.Add(SpecialM.LoftcurveLengthwise(l3[b], H).CapPlanarHoles(0.01));
            }
            
            for (int b = 0; b < l2.Count; b++)
            {
                br_columns.Add(SpecialM.LoftcurveWidthwise(l2[b], H).CapPlanarHoles(0.01));
                br_columns.Add(SpecialM.LoftcurveWidthwise(l4[b], H).CapPlanarHoles(0.01));
            }

            List<Line> outercolumns = SpecialM.createcolumnsfromendpts(endpoints, H, sh);
            List<Brep> br_outcolumns = new List<Brep>();
            for (int b = 0; (b < outercolumns.Count); b++)
            {
                br_outcolumns.Add(SpecialM.Loftoutercolumns(outercolumns[b]).CapPlanarHoles(0.01));
            }
            

            //Creating breps for the beams
            List<Brep> br_beams = new List<Brep>();
            for (int j = 0; j < Lbeams.Count; j++)
            {
                br_beams.Add(SpecialM.LoftBeams(Lbeams[j]));
            }
            
            for (int j = 0; j < Wbeams.Count; j++)
            {
                br_beams.Add(SpecialM.LoftBeams(Wbeams[j]));
            }
            List<Brep> flbr_beams = new List<Brep>();
            for(int j = 0; j < Lmidpoints.Count-1; j++)
            {
                for (int k = 0;k < Lmidpoints[j].Count; k++)
                {
                    Point3d p1 = Lmidpoints[j][k];
                    Point3d p2 = Lmidpoints[j+1][k];
                    Line l11 = new Line(p1,p2);
                    flbr_beams.Add(SpecialM.Creatingbeamsacross(l11).CapPlanarHoles(0.01));
                }
            }

            List<double> ftall = new List<double>();
            int f_L = Convert.ToInt32(L / cc) -1;
            int f_W = Convert.ToInt32(W / cc) -1;
            ftall.Add(f_L);
            ftall.Add(f_W);
            ftall.Add(n);
            ftall.Add(sh);

            DA.SetDataList(0, br_columns);
            DA.SetDataList(1, br_outcolumns);
            DA.SetDataList(2, br_beams);
            DA.SetDataList(3, ftall);
            DA.SetDataList(4, flbr_beams);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
       /// protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("22eca582-f140-4fdd-8b92-e33936801113");
    }
}