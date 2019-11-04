using System;
using System.Collections.Generic;
using Multicad;
using Multicad.Runtime;
using Multicad.DatabaseServices;
using Multicad.Geometry;
using Multicad.CustomObjectBase;
using System.Windows.Forms;
using Multicad.DatabaseServices.StandardObjects;

namespace Metallwork
{
    [CustomEntity(typeof(BOffset), "6de33a6e-a784-11e9-a2a3-2a2ae2dbcce6", "BOffset", "BOffset")]
    [Serializable]
    internal class BOffset : McCustomBase
    {
        private Polyline3d poly = new Polyline3d();
        public BOffset()
        {
        }
            public override void OnDraw(GeometryBuilder dc)
            {
            dc.DrawPolyline(poly.GetTrimmedOffset(100)[0]);
                
            }
        public override hresult PlaceObject(PlaceFlags lInsertType)
        {
            InputJig jig = new InputJig();
            //List<McObjectId> res = jig.SelectObjects("Select a Polyline");
            InputResult res = jig.SelectObject("Select a Polyline");
            McObjectId id = res.ObjectId;
            DbGeometry selection = id.GetObject();

            if (selection.IsKindOf(DbPolyline.TypeID))
            {
                poly = new Polyline3d(selection.Geometry.Polyline);
            }
            else if (selection.IsKindOf(DbLine.TypeID))
            {
                poly = new Polyline3d(selection.Geometry.LineSeg);
            }
            else
            {
                MessageBox.Show("Objecttype isn't valid");
                DbEntity.Erase();
                return hresult.e_Fail;
            }
            DbEntity.AddToCurrentDocument();
            return hresult.s_Ok;
        }
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify()) return;
        }
        public override List<Point3d> OnGetGripPoints()
        {
            List<Multicad.Geometry.Point3d> arr = new List<Point3d>();
            return arr;
        }
        public override void OnMoveGripPoints(List<int> indexes, Vector3d offset, bool isStretch)
        {
            if (!TryModify()) return;
            
        }
    }
}