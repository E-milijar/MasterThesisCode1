using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterThesisCode1
{
    internal class BoxJoint
    {
        public static Brep CreateJointFromToppointLenght(Point3d pt) 
        {
            Brep brep = new Brep();
            
            Point3d p = new Point3d(pt.X - 0.024, pt.Y-0.063, pt.Z);
            Point3d pt14 = new Point3d(p.X, p.Y, p.Z + 0.014);
            Vector3d vc = new Vector3d(0, 0, 1);

            Plane plane0 = new Plane(p,vc);
            Plane plane1 = new Plane(pt14,vc);
            Rectangle3d rec0 = new Rectangle3d(plane0, 0.048, 0.126);
            Rectangle3d rec1 = new Rectangle3d(plane1, 0.048, 0.126);

            List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
            brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0].CapPlanarHoles(0.01);
            return brep;
        }

        public static Brep CreateJointFromBottompointLenght(Point3d pt)
        {
            Brep brep = new Brep();

            Point3d p = new Point3d(pt.X + 0.024, pt.Y - 0.063, pt.Z);
            Point3d pt14 = new Point3d(p.X, p.Y, p.Z - 0.014);
            Vector3d vc = new Vector3d(0, 0, -1);

            Plane plane0 = new Plane(p, vc);
            Plane plane1 = new Plane(pt14, vc);
            Rectangle3d rec0 = new Rectangle3d(plane0, 0.048, 0.126);
            Rectangle3d rec1 = new Rectangle3d(plane1, 0.048, 0.126);

            List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
            brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0].CapPlanarHoles(0.01);
            return brep;
        }

        public static Brep CreateJointFromToppointWidth(Point3d pt)
        {
            Brep brep = new Brep();

            Point3d p = new Point3d(pt.X - 0.063, pt.Y - 0.024, pt.Z);
            Point3d pt14 = new Point3d(p.X, p.Y, p.Z + 0.014);
            Vector3d vc = new Vector3d(0, 0, 1);

            Plane plane0 = new Plane(p, vc);
            Plane plane1 = new Plane(pt14, vc);
            Rectangle3d rec0 = new Rectangle3d(plane0, 0.126, 0.048);
            Rectangle3d rec1 = new Rectangle3d(plane1, 0.126, 0.048);

            List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
            brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0].CapPlanarHoles(0.01);
            return brep;
        }

        public static Brep CreateJointFromBottompointWidth(Point3d pt)
        {
            Brep brep = new Brep();

            Point3d p = new Point3d(pt.X + 0.063, pt.Y - 0.024, pt.Z);
            Point3d pt14 = new Point3d(p.X, p.Y, p.Z - 0.014);
            Vector3d vc = new Vector3d(0, 0, -1);

            Plane plane0 = new Plane(p, vc);
            Plane plane1 = new Plane(pt14, vc);
            Rectangle3d rec0 = new Rectangle3d(plane0, 0.126, 0.048);
            Rectangle3d rec1 = new Rectangle3d(plane1, 0.126, 0.048);

            List<Curve> crvs = new List<Curve>() { rec0.ToNurbsCurve(), rec1.ToNurbsCurve() };
            brep = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, false)[0].CapPlanarHoles(0.01);
            return brep;
        }
    }
}
