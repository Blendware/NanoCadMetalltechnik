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
    [CustomEntity(typeof(Conjunction), "6de33a4e-a784-11e9-a2a3-2a2ae2dbccf6", "Conjunction", "Conjunction")]
    [Serializable]
    internal class Conjunction : McCustomBase
    {
        private Polyline3d poly = new Polyline3d();
        private Point3d _pnt1 = new Point3d(0, 0, 0);
        private Point3d _pnt2 = new Point3d(100, 100, 0);

        public Conjunction()
        {
        }
        [DisplayName("Length")]
        [Category("Parameter")]
        public double Length { get; set; } = 30;
        [DisplayName("Thickness")]
        [Category("Parameter")]
        public double Thickness { get; set; } = 8;
        [DisplayName("Margin")]
        [Category("Parameter")]
        public double Margin { get; set; } = 20;
        [DisplayName("Count")]
        [Category("Parameter")]
        public int Count { get; set; } = 2;

        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;

            dc.DrawPolyline(new ConShape(poly.Points.FirstPoint, poly.Points.LastPoint, Thickness, Length, Margin, Count).Shape);
        }
        public override hresult PlaceObject(PlaceFlags lInsertType)
        {
            InputJig jig = new InputJig();
            InputResult res = jig.SelectObject("Select Line");

            if (res.Result != InputResult.ResultCode.Normal)
                return hresult.e_Fail;

            LineSeg3d line = res.Geometry.LineSeg;
            poly = new Polyline3d(line);

            McObjectId id = res.ObjectId;
            DbGeometry selection = id.GetObject();

            DbEntity.AddToCurrentDocument();
            return hresult.s_Ok;
        }
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify()) return;

            poly.TransformBy(tfm);
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
    [CustomEntity(typeof(Conjunction_female), "6de33a4e-a784-11e9-a2a3-2a2ae2dbccf8", "Conjunction_female", "Conjunction_female")]
    [Serializable]
    internal class Conjunction_female : McCustomBase
    {
        private Point3d _pnt1 = new Point3d(0, 0, 0);
        private Point3d _pnt2 = new Point3d(100, 100, 0);
        private bool _Inside = false;

        public Conjunction_female()
        {
        }
        [DisplayName("Length")]
        [Category("Parameter")]
        public double Length { get; set; } = 30;
        [DisplayName("Thickness")]
        [Category("Parameter")]
        public double Thickness { get; set; } = 8;
        [DisplayName("Margin")]
        [Category("Parameter")]
        public double Margin { get; set; } = 20;
        [DisplayName("Count")]
        [Category("Parameter")]
        public int Count { get; set; } = 2;
        [DisplayName("Gap")]
        [Category("Parameter")]
        public double Gap { get; set; } = 0.1;
        [DisplayName("Offset")]
        [Category("Parameter")]
        public double Offset { get; set; } = 0;
        [DisplayName("Inside")]
        [Category("Parameter")]
        public int Inside {
            get
            {
                if (_Inside) return 1;
                else return 0;
            }
            set
            {
                if (!TryModify()) return;
                if (value > 0) _Inside = true;
                else _Inside = false;
            }
        }

        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;

            Polyline3d poly = new Polyline3d(new List<Point3d> {_pnt1, _pnt2});
            double gap = Thickness < 0 ? Gap * -1 : Gap;
            poly = _Inside ? poly.GetTrimmedOffset(Offset-gap)[0] : poly;

            ConShape shape = new ConShape(poly.Points.FirstPoint, poly.Points.LastPoint, Thickness + gap * (_Inside ? 2 : 1), Length + Gap * 2, Margin - Gap, Count);

            for (int i = 1; i < Count+1; i++)
            {
                dc.DrawPolyline(shape.polyline(i, _Inside));
            }
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

            McObjectId id = res.ObjectId;
            DbGeometry selection = id.GetObject();

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

            _pnt1 = _pnt1.TransformBy(tfm);
            _pnt2 = _pnt2.TransformBy(tfm);
        }
        //public override List<Point3d> OnGetGripPoints()
        //{
        //    List<Multicad.Geometry.Point3d> arr = new List<Point3d>();
        //    return arr;
        //}
        //public override void OnMoveGripPoints(List<int> indexes, Vector3d offset, bool isStretch)
        //{
        //    if (!TryModify()) return;

        //}
    }
    class ConShape
    {
        private List<Point3d> _shape;//8ec5m3
        public Point3d[] Shape { get { return _shape.ToArray(); } }
        public double Angle { get; }
        public ConShape(Point3d firstPoint, Point3d secondPoint, double Thickness, double Length, double Margin, int Count)
        {
            double endPoint = firstPoint.X + firstPoint.DistanceTo(secondPoint);
            double inbetween = Math.Round((firstPoint.DistanceTo(secondPoint)-Margin*2-Length)/(Count-1),4);
            
            _shape = new List<Point3d> {
                firstPoint,
                new Point3d(firstPoint.X+Margin, firstPoint.Y, 0),
                new Point3d(firstPoint.X+Margin, firstPoint.Y + Thickness, 0),
                new Point3d(firstPoint.X+Margin+Length, firstPoint.Y + Thickness, 0),
                new Point3d(firstPoint.X+Margin+Length, firstPoint.Y, 0)
             };

            if (Count > 1)
            {
                for (int i = 1; i < Count-1; i++)
                {
                    double distance = inbetween*i;
                    _shape.Add(new Point3d(Margin + firstPoint.X + distance, firstPoint.Y, 0));
                    _shape.Add(new Point3d(Margin + firstPoint.X + distance, firstPoint.Y + Thickness, 0));
                    _shape.Add(new Point3d(Margin + firstPoint.X + distance + Length, firstPoint.Y + Thickness, 0));
                    _shape.Add(new Point3d(Margin + firstPoint.X + distance + Length, firstPoint.Y, 0));
                    //distance += inbetween;
                }
                _shape.Add(new Point3d(endPoint - Margin - Length, firstPoint.Y, 0));
                _shape.Add(new Point3d(endPoint - Margin - Length, firstPoint.Y + Thickness, 0));
                _shape.Add(new Point3d(endPoint - Margin, firstPoint.Y + Thickness, 0));
                _shape.Add(new Point3d(endPoint - Margin, firstPoint.Y, 0));
            }
            _shape.Add(new Point3d(endPoint, firstPoint.Y, 0));
            Angle = GetAngle(firstPoint, secondPoint);
            Rotate(firstPoint);
        }
        public Polyline3d polyline(int Count, bool Inside)
        {
            List<Point3d> new_shape = new List<Point3d> (_shape);
            Polyline3d poly = new Polyline3d(new_shape);
            if (Inside)
            {
                new_shape.RemoveAt(0);
                new_shape.RemoveAt(new_shape.Count - 1);

                if (Count > 1)
                    new_shape.RemoveRange(0, Count * 4 - 4);

                if (new_shape.Count > 4)
                    new_shape.RemoveRange(4, new_shape.Count - 4);

                poly = new Polyline3d(new_shape);
                poly.SetClosed(true);
            }

            return poly;
        }
        private double GetAngle(Point3d firstPoint, Point3d secondPoint)
        {
            double angle = Math.Atan2(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y);
            angle *= 180 / Math.PI;

            return 90 - angle;
        }
        public int Count { get { return Shape.Length; } }
        private void Rotate(Point3d firstPoint)
        {
            for (int i = 0; i < Shape.Length; i++)
            {
                Point3d point = _shape[i];
                point = point.Rotate(firstPoint, Angle);
                _shape[i] = point;
            }

        }
    }
}