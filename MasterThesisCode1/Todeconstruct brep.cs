using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;

namespace MasterThesisCode1
{
    internal class Todeconstruct_brep
    {

        public static Brep FindTopbrep(Brep brep)
        {
            Brep topbrep = brep.Surfaces[0].ToBrep();
            double rmax = AreaMassProperties.Compute(brep.Surfaces).Centroid.Z;
            foreach (Surface srf in brep.Surfaces)
            {
                Point3d cpt = AreaMassProperties.Compute(srf).Centroid;
                if (cpt.Z > rmax)
                {
                    rmax = cpt.Z;
                    topbrep = srf.ToBrep();
                }
            }
            return topbrep;
        }


        public static Brep Findbottombrep (Brep brep)
        {
            Brep btbrep = brep.Surfaces[0].ToBrep();
            double rmin = AreaMassProperties.Compute(brep.Surfaces).Centroid.Z;
            foreach (Surface srf in brep.Surfaces)
            {
                Point3d cpt = AreaMassProperties.Compute(srf).Centroid;
                if (cpt.Z < rmin)
                {
                    rmin = cpt.Z;
                    btbrep = srf.ToBrep();
                }
            }
            return btbrep;
        }

        /*
       public static double Findlongestlength (Brep brep)
        {
            double length = brep.Edges[0].GetLength();
            foreach(Curve edge in brep.Edges)
            {
                double len1 = edge.GetLength();
                if(len1 > length)
                {
                    length = len1;
                }
            }
            return length;
        }
        */
        public static Curve FindlongestlengthCurve(Brep brep)
        {
            double length = brep.Edges[0].GetLength();
            Curve len = brep.Edges[0];
            foreach (Curve edge in brep.Edges)
            {
                double len1 = edge.GetLength();
                if (len1 > length)
                {
                    length = len1;
                    len = edge;
                }
            }
            return len;
        }
        public static double FindShortestlength(Brep brep)
        {
            double length = brep.Edges[0].GetLength();
            foreach (Curve edge in brep.Edges)
            {
                double len1 = edge.GetLength();
                if (len1 < length)
                {
                    length = len1;
                }
            }
            return length;
        }

        public static Brep FindOuterYbrep(Brep brep)
        {
            Brep topbrep = brep.Surfaces[0].ToBrep();
            double rmax = AreaMassProperties.Compute(brep.Surfaces).Centroid.Y;
            foreach (Surface srf in brep.Surfaces)
            {
                Point3d cpt = AreaMassProperties.Compute(srf).Centroid;
                if (cpt.Y > rmax)
                {
                    rmax = cpt.Y;
                    topbrep = srf.ToBrep();
                }
            }
            return topbrep;
        }

        public static Brep FindInnerYbrep(Brep brep)
        {
            Brep topbrep = brep.Surfaces[0].ToBrep();
            double rmin = AreaMassProperties.Compute(brep.Surfaces).Centroid.Y;
            foreach (Surface srf in brep.Surfaces)
            {
                Point3d cpt = AreaMassProperties.Compute(srf).Centroid;
                if (cpt.Y < rmin)
                {
                    rmin = cpt.Y;
                    topbrep = srf.ToBrep();
                }
            }
            return topbrep;
        }
        public static Brep FindOuterXbrep(Brep brep)
        {
            Brep topbrep = brep.Surfaces[0].ToBrep();
            double rmax = AreaMassProperties.Compute(brep.Surfaces).Centroid.X;
            foreach (Surface srf in brep.Surfaces)
            {
                Point3d cpt = AreaMassProperties.Compute(srf).Centroid;
                if (cpt.X > rmax)
                {
                    rmax = cpt.X;
                    topbrep = srf.ToBrep();
                }
            }
            return topbrep;
        }

        public static Brep FindInnerXbrep(Brep brep)
        {
            Brep topbrep = brep.Surfaces[0].ToBrep();
            double rmin = AreaMassProperties.Compute(brep.Surfaces).Centroid.X;
            foreach (Surface srf in brep.Surfaces)
            {
                Point3d cpt = AreaMassProperties.Compute(srf).Centroid;
                if (cpt.X < rmin)
                {
                    rmin = cpt.X;
                    topbrep = srf.ToBrep();
                }
            }
            return topbrep;
        }
    }
}
