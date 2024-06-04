using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry.Delaunay;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using static System.Net.Mime.MediaTypeNames;

namespace MasterThesisCode1
{
    public class Openingsinstudwork : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Openingsinstudwork()
          : base("Creating studwork for openings", "Nickname",
              "Description",
              "NTNU", "Master")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Houseform with openings", "HfwO", "", GH_ParamAccess.list);
            pManager.AddBrepParameter("Columns in the building", "", "", GH_ParamAccess.list);
            pManager.AddBrepParameter("Beams in the building", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Ratios", "", "From Create Building", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Studwork in the building", "", "", GH_ParamAccess.list);
            pManager.AddPointParameter("TEST", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        { 
            List<Brep> HF = new List<Brep>();
            List<Brep> Columns = new List<Brep>();
            List<Brep> Beams = new List<Brep>();
            List<double> ratios = new List<double>();

            DA.GetDataList(0, HF);
            DA.GetDataList(1, Columns);
            DA.GetDataList(2, Beams);
            DA.GetDataList(3, ratios);
            double n = ratios[2];
            double starth = ratios[3];

            Brep minxbrep = new Brep();
            Brep maxxbrep = new Brep();
            Brep minybrep = new Brep();
            Brep maxybrep = new Brep();

            List<Brep> minx = new List<Brep>();
            List<Brep> maxx = new List<Brep>();
            List<Brep> miny = new List<Brep>();
            List<Brep> maxy = new List<Brep>();

            //rmax osv. MÅ være utenfor forløka sånn at den finner den ene brepen vi ønsker!
            double rxmax = AreaMassProperties.Compute(HF).Centroid.X;
            double rxmin = AreaMassProperties.Compute(HF).Centroid.X;
            double rymax = AreaMassProperties.Compute(HF).Centroid.Y;
            double rymin = AreaMassProperties.Compute(HF).Centroid.Y;
            double rxmax2 = AreaMassProperties.Compute(HF).Centroid.X;
            double rxmin2 = AreaMassProperties.Compute(HF).Centroid.X;
            double rymax2 = AreaMassProperties.Compute(HF).Centroid.Y;
            double rymin2 = AreaMassProperties.Compute(HF).Centroid.Y;

            int fn = HF.Count / 2;

            for (int i = 0; i < fn; i++)
            {
                Brep bbrep = HF[i];
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

            for (int i = fn; i < HF.Count; i++)
            {
                Brep bbrep = HF[i];
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

            //Punkter hvor det er hull i veggene! 
            List<Point3d> minxpts = ClassForOpenings.sortingPointsaccourdingZ(minx);
            List<Point3d> minypts = ClassForOpenings.sortingPointsaccourdingZ(miny);
            List<Point3d> maxxpts = ClassForOpenings.sortingPointsaccourdingZ(maxx);
            List<Point3d> maxypts = ClassForOpenings.sortingPointsaccourdingZ(maxy);

            List<Point3d> listtest = new List<Point3d>();
            

            List<List<Point3d>> minxpts1 = ClassForOpenings.separatepoints(minxpts);
            //List<Brep> test1 = ClassForOpenings.CreateCutterFormpositivretning(minxpts1);
            List<List<Point3d>> maxypts1 = ClassForOpenings.separatepoints(maxypts);
            //List<Brep> test2 = ClassForOpenings.CreateCutterFormpositivretning(maxypts1);
            


            //DA.SetDataList(0, test2);
            DA.SetDataList(1, maxypts1[0]);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
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
            get { return new Guid("FD9D0ED0-9440-47E7-868A-D8F519C9BB09"); }
        }
    }
}