using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Eto.Drawing;
using Grasshopper.Kernel;
using Rhino.ApplicationSettings;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;

namespace MasterThesisCode1
{
    public class CreateJoint : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateJoint class.
        /// </summary>
        public CreateJoint()
          : base("CreateJoint", "Nickname",
              "Description",
              "NTNU", "Master")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("MiddleStudwork", "", "Columns in building", GH_ParamAccess.list);
            pManager.AddBrepParameter("Endstudwork", "", "Columns in building", GH_ParamAccess.list);
            pManager.AddBrepParameter("Beams", "", "Beams in building", GH_ParamAccess.list);
            pManager.AddNumberParameter("Ratios", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Studwork", "", "Studwork with joints", GH_ParamAccess.list);
            pManager.AddBrepParameter("Beam", "", "Beam with joints", GH_ParamAccess.list);
            pManager.AddPointParameter("Toppoints", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("test", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> midcolumns = new List<Brep>();
            List<Brep> endcolumns = new List<Brep>();
            List<Brep> beams = new List<Brep>();
            List<double> ratios = new List<double>();
            DA.GetDataList(0, midcolumns);
            DA.GetDataList(1, endcolumns);
            DA.GetDataList(2, beams);
            DA.GetDataList(3, ratios);

            List<Point3d> Tpts = new List<Point3d>();
            List<Point3d> eTpts = new List<Point3d>();
            List<Point3d> Bpts = new List<Point3d>();
            List<Point3d> eBpts = new List<Point3d>();
            List<Point3d> tapp = new List<Point3d>();

            List<Brep> output = new List<Brep>();
            List<Brep> newbeams = new List<Brep>();

            var midcount = midcolumns.Count;
            var L = Convert.ToInt32(ratios[0]);
            var W = ratios[1];
            var n = ratios[2];
            
            double lenghtcolm = endcolumns[0].Edges[0].GetLength();
            double uplc = (lenghtcolm / 2);
            for (int i = 0; i < L*4; i++)
            {
                List<Brep> lenghtw = new List<Brep>();
                Point3d p = AreaMassProperties.Compute(midcolumns[i].Surfaces).Centroid;
                Point3d tp = new Point3d(p.X, p.Y, p.Z + uplc);
                Point3d Bp = new Point3d(p.X, p.Y, p.Z - uplc);
                Brep bbrep = BoxJoint.CreateJointFromBottompointLenght(Bp);
                Brep tbrep = BoxJoint.CreateJointFromToppointLenght(tp);

                lenghtw.Add(bbrep);
                lenghtw.Add(midcolumns[i]);
                lenghtw.Add(tbrep);
                Brep bp = Brep.MergeBreps(lenghtw, 0.01);
                output.Add(bp);

                Bpts.Add(Bp);
                Tpts.Add(tp);
            }

            for (int i = (L*4); i < midcount; i++)
            {
                List<Brep> Widthwisej = new List<Brep>();
                Point3d p = AreaMassProperties.Compute(midcolumns[i].Surfaces).Centroid;
                Point3d tp = new Point3d(p.X, p.Y, p.Z + uplc);
                Point3d Bp = new Point3d(p.X, p.Y, p.Z - uplc);
                Brep bbrep = BoxJoint.CreateJointFromBottompointWidth(Bp);
                Brep tbrep = BoxJoint.CreateJointFromToppointWidth(tp);

                Widthwisej.Add(bbrep);
                Widthwisej.Add(midcolumns[i]);
                Widthwisej.Add(tbrep);
                Brep bp = Brep.MergeBreps(Widthwisej, 0.01);
                output.Add(bp);

                Bpts.Add(Bp);
                Tpts.Add(tp);
            }
            
            foreach(Brep bp in endcolumns)
            {
                List<Brep> endbrep = new List<Brep>();
                Point3d p = AreaMassProperties.Compute(bp.Surfaces).Centroid;
                Point3d tp = new Point3d(p.X, p.Y, p.Z + uplc);
                Point3d Bp = new Point3d(p.X, p.Y, p.Z - uplc);
                Brep bbrep = BoxJoint.CreateJointFromBottompointLenght(Bp);
                Brep tbrep = BoxJoint.CreateJointFromToppointLenght(tp);

                endbrep.Add(bbrep);
                endbrep.Add(bp);
                endbrep.Add(tbrep);
                Brep bpr = Brep.MergeBreps(endbrep, 0.01);
                output.Add(bpr);

                eBpts.Add(Bp);
                eTpts.Add(tp);
            }

            
            //CHECK BREP INPUT AS IT DOES NOT PRODUCE BREP 
            DA.SetDataList(0, output);
            DA.SetDataList(1, newbeams);
            DA.SetDataList(2, tapp);
            DA.SetData(3, lenghtcolm);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        /// 
        /*
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }
        */
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("25AD3A43-302B-4835-9C3A-ED5CC8452796"); }
        }
    }
}