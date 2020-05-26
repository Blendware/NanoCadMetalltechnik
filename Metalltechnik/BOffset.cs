using System;
using System.Collections.Generic;
using Multicad;
using Multicad.Runtime;
using Multicad.DatabaseServices;
using Multicad.Geometry;
using Multicad.CustomObjectBase;
using System.Windows.Forms;
using Multicad.DatabaseServices.StandardObjects;
using System.ComponentModel;

namespace Metallwork
{
    [CustomEntity(typeof(BOffset), "6de33a6e-a784-11e9-a2a3-2a2ae2dbcce6", "BOffset", "BOffset")]
    [Serializable]
    internal class BOffset : McCustomBase
    {
        private Point3d _pnt1 = new Point3d(0, 0, 0);
        private Point3d _pnt2 = new Point3d(100, 100, 0);
        private double _Height = 20;

        public BOffset()
        {
        }

        [DisplayName("Radius")]
        [Category("Parameter")]
        public double Radius { get; set; } = 1;
        [DisplayName("Thickness")]
        [Category("Parameter")]
        public double Thickness { get; set; } = 1.5;
        [DisplayName("Height")]
        [Category("Parameter")]
        public double Height
        {
            get
            {
                return _Height;
            }
            set
            {
                if (!TryModify()) return;
                if (value < 0) {
                    Point3d point = _pnt1;
                    _pnt1 = _pnt2;
                    _pnt2 = point;

                    _Height = value * -1;
                }
                else
                {
                    _Height = value;
                }
            }
        }
        [DisplayName("Gap")]
        [Category("Parameter")]
        public double Gap { get; set; } = 0.2;

        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;

            BendingShape shape1 = new BendingShape(_pnt1, _pnt2, Height, Thickness, Gap);
            //BendingShape shape2 = new BendingShape(_pnt1, _pnt2, Height, Thickness);

            Polyline3d poly = new Polyline3d(new List<Point3d>(shape1.Shape));
            //poly.SetClosed(true);

            Polyline3d.VertexDataOfPolyline polyVertecies = poly.Vertices;
            for (int i = 0; i < polyVertecies.Count; i++)
            {
                polyVertecies.MakeFilletAtVertex(i + 1, Radius);
            }
            dc.DrawPolyline(poly);
        }
        public override hresult PlaceObject(PlaceFlags lInsertType)
        {
            InputJig jig = new InputJig();
            InputResult res = jig.SelectObject("Select Line");

            if (res.Result != InputResult.ResultCode.Normal)
                return hresult.e_Fail;

            LineSeg3d line = res.Geometry.LineSeg;
            _pnt1 = line.StartPoint;
            _pnt2 = line.EndPoint;

            //McObjectId id = res.ObjectId;
            //DbGeometry selection = id.GetObject();

            //if (selection.IsKindOf(DbPolyline.TypeID))
            //{
            //    MessageBox.Show("poly");
            //}
            //else if (selection.IsKindOf(DbLine.TypeID))
            //{
            //    MessageBox.Show("line");
            //}
            //else
            //{
            //    MessageBox.Show("Objecttype isn't valid");
            //    DbEntity.Erase();
            //    return hresult.e_Fail;
            //}
            DbEntity.AddToCurrentDocument();
            return hresult.s_Ok;
        }
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify()) return;
        }
        public override List<Point3d> OnGetGripPoints()
        {
            List<Point3d> arr = new List<Point3d>();
            return arr;
        }
        public override void OnMoveGripPoints(List<int> indexes, Vector3d offset, bool isStretch)
        {
            if (!TryModify()) return;
            
        }
    }
}