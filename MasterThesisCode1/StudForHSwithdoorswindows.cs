using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Diagnostics.Metrics;
using System.Linq;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.SettingsControls;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Resources;

namespace MasterThesisCode1
{
    public class StudForHSwithdoorswindows : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the StudForHSwithdoorswindows class.
        /// </summary>
        public StudForHSwithdoorswindows()
          : base("Studwork with openings for brep walls", "Studworkcreator",
              "Creates Studwork for walls and floor breps with openings",
              "NTNU", "MasterMain")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Walls", "W", "", GH_ParamAccess.list);
            pManager.AddBrepParameter("Floors", "F", "Doesn't include the roof. ONLY Floors, not roofs ", GH_ParamAccess.list);
            pManager.AddNumberParameter("CenterDistance", "CC", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Studwork", "", "The first number is either  studwork for the walls(0) or floors(1). The second says which wall it is." +
                " Where min X (0), max X (1), min Y (2) and max Y (3), while for the floor it's which level it's on. " +
                "Third says whenever it's columns(0) and Beams(1) in the wall, while for the floor it's the same number for each floor", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> Walls = new List<Brep>();
            List<Brep> Floors = new List<Brep>();
            double cc = 0.6;


            DA.GetDataList(0, Walls);
            DA.GetDataList(1, Floors);
            DA.GetData(2, ref cc);

            Brep minxbrep = new Brep();
            Brep maxxbrep = new Brep();
            Brep minybrep = new Brep();
            Brep maxybrep = new Brep();

            List<Brep> minx = new List<Brep>();
            List<Brep> maxx = new List<Brep>();
            List<Brep> miny = new List<Brep>();
            List<Brep> maxy = new List<Brep>();

            //rmax and such must be outside the for-loop to find the brep.
            double rxmax = AreaMassProperties.Compute(Walls).Centroid.X;
            double rxmin = AreaMassProperties.Compute(Walls).Centroid.X;
            double rymax = AreaMassProperties.Compute(Walls).Centroid.Y;
            double rymin = AreaMassProperties.Compute(Walls).Centroid.Y;

            double rxmax2 = AreaMassProperties.Compute(Walls).Centroid.X;
            double rxmin2 = AreaMassProperties.Compute(Walls).Centroid.X;
            double rymax2 = AreaMassProperties.Compute(Walls).Centroid.Y;
            double rymin2 = AreaMassProperties.Compute(Walls).Centroid.Y;

            Point3d btbreptp = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(Floors[0])).Centroid;
            Point3d ttbrepbp = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(Floors[1])).Centroid;
            Point3d ttbreptp = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(Floors[1])).Centroid;
            double height = btbreptp.DistanceTo(ttbrepbp);
            double heightoffloor = ttbrepbp.DistanceTo(ttbreptp);

            double startingheight = btbreptp.Z;
            int numfloors = Floors.Count;
            int fn = Walls.Count / numfloors;

            for (int i = 0; i < fn; i++)
            {
                Brep bbrep = Walls[i];
                Point3d cpt = AreaMassProperties.Compute(bbrep).Centroid;
                if (cpt.X > rxmax)
                {
                    rxmax = cpt.X;
                    maxxbrep = bbrep;
                }

                if (cpt.X < rxmin)
                {
                    rxmin = cpt.X;
                    minxbrep = bbrep;
                }
                if (cpt.Y > rymax)
                {
                    rymax = cpt.Y;
                    maxybrep = bbrep;
                }

                if (cpt.Y < rymin)
                {
                    rymin = cpt.Y;
                    minybrep = bbrep;
                }
            }
            minx.Add(minxbrep);
            maxx.Add(maxxbrep);
            miny.Add(minybrep);
            maxy.Add(maxybrep);

            for (int i = fn; i < Walls.Count; i++)
            {
                Brep bbrep = Walls[i];
                Point3d cpt = AreaMassProperties.Compute(bbrep).Centroid;
                if (cpt.X > rxmax2)
                {
                    rxmax2 = cpt.X;
                    maxxbrep = bbrep;
                }

                if (cpt.X < rxmin2)
                {
                    rxmin2 = cpt.X;
                    minxbrep = bbrep;
                }
                if (cpt.Y > rymax2)
                {
                    rymax2 = cpt.Y;
                    maxybrep = bbrep;
                }

                if (cpt.Y < rymin2)
                {
                    rymin2 = cpt.Y;
                    minybrep = bbrep;
                }
            }
            minx.Add(minxbrep);
            maxx.Add(maxxbrep);
            miny.Add(minybrep);
            maxy.Add(maxybrep);

            
            List<List<Brep>> minxlen = MainXColumns.minxlen(minx, cc, height, numfloors, startingheight);
            List<List<Brep>> maxxlen = MainXColumns.maxxlen(maxx, cc, height, numfloors, startingheight);
            List<List<Brep>> minylen = MainYColumns.minylen(miny, cc, height, numfloors, startingheight);
            List<List<Brep>> maxylen = MainYColumns.maxylen(maxy, cc, height, numfloors, startingheight);

            List<List<Brep>> Longbeams = new List<List<Brep>>();
            for (int i = 1; i < numfloors; i++)
            {
                Point3d b1 = AreaMassProperties.Compute(Todeconstruct_brep.Findbottombrep(Floors[i])).Centroid;
                Point3d b2 = AreaMassProperties.Compute(Todeconstruct_brep.FindTopbrep(Floors[i])).Centroid;
                double stheightbeams = b1.Z + 0.048;
                double endheightbeams = b2.Z - 0.048;
                double Heightofbeams = endheightbeams - stheightbeams;
                Curve lenght = Todeconstruct_brep.FindlongestlengthCurve(Todeconstruct_brep.Findbottombrep(miny[i]));
                double width = Todeconstruct_brep.FindlongestlengthCurve(Todeconstruct_brep.Findbottombrep(maxx[i])).GetLength();
                Point3d p1 = lenght.PointAtStart;
                Point3d p2 = lenght.PointAtEnd;
                List<Point3d> pts = SpecialM.pointsatlength(p1, p2, cc);
                if (Math.Abs(p2.X - pts[pts.Count-1].X) < 0.1 && Math.Abs(p2.Y - pts[pts.Count - 1].Y) < 0.1)
                {
                    pts.RemoveAt(pts.Count - 1);
                }
                List<Brep> pts1 = new List<Brep>();
                Point3d lastpt = pts[0];
                foreach(Point3d pt in pts)
                {
                    Point3d bpmoved1 = new Point3d(pt.X - 0.048, pt.Y + 0.1, pt.Z - 0.048 - Heightofbeams);
                    Point3d bpmoved2 = new Point3d(pt.X - 0.048, pt.Y + width + 0.1, pt.Z - 0.048 - Heightofbeams);
                    Line beamline = new Line(bpmoved1, bpmoved2);
                    Interval inter = beamline.ToNurbsCurve().Domain;
                    Rhino.Geometry.Plane plane0 = beamline.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { inter.Min, inter.Max })[0];
                    Rhino.Geometry.Plane plane1 = beamline.ToNurbsCurve().GetPerpendicularFrames(new List<double>() { inter.Min, inter.Max })[1];

                    Rectangle3d rec0 = new Rectangle3d(plane0, Heightofbeams, 0.048);
                    Rectangle3d rec1 = new Rectangle3d(plane1, Heightofbeams, 0.048);
                    List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
                    Brep bp = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0];
                    BoundingBox bx = bp.GetBoundingBox(true);
                    Brep beam = bx.ToBrep();
                    pts1.Add(beam);
                    
                }
               Longbeams.Add(pts1);
            }

            List<List<List<Brep>>> AllWalls = new List<List<List<Brep>>>();
            AllWalls.Add(minxlen);
            AllWalls.Add(maxxlen);
            AllWalls.Add(minylen);
            AllWalls.Add(maxylen);
            
            DataTree<Brep> Thetree = new DataTree<Brep>();
            for(int i = 0; i < AllWalls.Count; i++)
            {
                for(int j = 0; j < AllWalls[i].Count; j++)
                {
                    Thetree.AddRange(AllWalls[i][j], new GH_Path(0,i,j));
                }
            }
            
            for(int i = 0; i< Longbeams.Count; i++)
            {
                Thetree.AddRange(Longbeams[i], new GH_Path(1, 0, i));
            }
           
            DA.SetDataTree(0, Thetree);
        }

        /// <summary>
        /// Provides an Icon for the component
        /// </summary>


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1E1B9331-9FE9-4C89-ABDC-A0E91BD53B4E"); }
        }
    }
}