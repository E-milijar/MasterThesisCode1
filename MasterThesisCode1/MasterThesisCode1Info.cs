using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace MasterThesisCode1
{
    public class MasterThesisCode1Info : GH_AssemblyInfo
    {
        public override string Name => "MasterThesisCode1";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        //public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("24df34de-65cc-439d-aad6-4874e0876a2c");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}