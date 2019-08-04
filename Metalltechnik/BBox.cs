//
// Copyright (C) 2019, Pescoller Tobias.  All rights reserved.
//

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using Multicad;
using Multicad.AplicationServices;
using Multicad.Runtime;
using Multicad.DatabaseServices;
using Multicad.DataServices;
using Multicad.Geometry;
using Multicad.CustomObjectBase;

namespace Metallwork
{
    [CustomEntity(typeof(BBox), "6de33a9e-a784-11e9-a2a3-2a2ae2dbcce4", "BBox", "BBox")]
    [Serializable]
    internal class BBox : McCustomBase
    {
        private Point3d _pnt = new Point3d(50, 50, 0);
        private Point3d _pnt1 = new Point3d(50, 100, 0);
        private Point3d _pnt2 = new Point3d(150, 150, 0);
        private Point3d _pnt3 = new Point3d(150, 50, 0);
        private double _Height = 20;
        public BBox()
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
                if (!TryModify()) return;//without this call undo will not be saved and the object will not be redrawn
                _Height = value;
            }
        }
        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;//color will be taken from object properties and object will be redrawn after color change
            //dc.DrawPolyline(new Point3d[] { _pnt, _pnt1, _pnt2, _pnt3, _pnt });
            //dc.DrawPolyline(poly.GetTrimmedOffset(Height)[0]);

            BendingShape shape1 = new BendingShape(_pnt, _pnt1, Height, Thickness);
            BendingShape shape2 = new BendingShape(_pnt1, _pnt2, Height);
            BendingShape shape3 = new BendingShape(_pnt2, _pnt3, Height, Thickness);
            BendingShape shape4 = new BendingShape(_pnt3, _pnt, Height);

            Polyline3d poly1 = new Polyline3d(new List<Point3d> (shape1.Shape));
            Polyline3d poly2 = new Polyline3d(new List<Point3d> (shape2.Shape));
            Polyline3d poly3 = new Polyline3d(new List<Point3d> (shape3.Shape));
            Polyline3d poly4 = new Polyline3d(new List<Point3d> (shape4.Shape));

            //CircArc3d test = new CircArc3d(_pnt1, new Vector3d(), new Vector3d(), 0.25, ((180 + _pnt.AngleTo(_pnt1)) * Math.PI / 180), (_pnt.AngleTo(_pnt1) * Math.PI / 180));
            //dc.DrawArc(test);

            //dc.DrawArc(_pnt1, 0.25, ((180+_pnt1.AngleTo(_pnt2)) * Math.PI/180), (_pnt1.AngleTo(_pnt2) * Math.PI/180));

            Polyline3d.VertexDataOfPolyline polyVertecies1 = poly1.Vertices;
            Polyline3d.VertexDataOfPolyline polyVertecies2 = poly2.Vertices;
            Polyline3d.VertexDataOfPolyline polyVertecies3 = poly3.Vertices;
            Polyline3d.VertexDataOfPolyline polyVertecies4 = poly4.Vertices;
            for (int i = 0; i < polyVertecies1.Count; i+=2)
            {
                polyVertecies1.MakeFilletAtVertex(i+1, Radius);
                polyVertecies2.MakeFilletAtVertex(i+1, Radius);
                polyVertecies3.MakeFilletAtVertex(i+1, Radius);
                polyVertecies4.MakeFilletAtVertex(i+1, Radius);
            }
            dc.DrawPolyline(poly1);
            dc.DrawPolyline(poly2);
            dc.DrawPolyline(poly3);
            dc.DrawPolyline(poly4);
        }
        public Point3d PerpendicularPoint(Point3d point1, Point3d point2, bool invert1 = false, bool invert2 = false)
        {
            double x = point2.X - point1.X;
            double y = point2.Y - point1.Y;
            x = point1.X > point2.X ? x * -1 : x;
            //y = y < 0 ? y * -1 : y;
            double angle = Math.Atan((y) / (x));
            //double angle2 = Math.Atan((point2.Y - point1.Y) / (point2.X - point1.X));
            if (invert1)
            {
                return new Point3d(
                point1.X - (Height * Math.Sin(angle)),
                point1.Y - (point1.X > point2.X ? (Height * Math.Cos(angle)) : (Height * Math.Cos(angle) * -1)),
                point1.Z);
            }
            else if (invert2)
            {
                return new Point3d(
                point1.X + (Height * Math.Sin(angle)),
                point1.Y + (point1.X > point2.X ? (Height * Math.Cos(angle)) : (Height * Math.Cos(angle) * -1)),
                point1.Z);
            }
            return new Point3d(
            point1.X + (Height * Math.Sin(angle)),
            point1.Y - (Height * Math.Cos(angle)),
            point1.Z);
        }
        public Point3d PerpendicularPoint(Point3d point1, Point3d point2, double Height, bool invert1 = false, bool invert2 = false)
        {
            double x = point2.X - point1.X;
            double y = point2.Y - point1.Y;
            x = point1.X > point2.X ? x * -1 : x;
            //y = y < 0 ? y * -1 : y;
            double angle = Math.Atan((y) / (x));
            //double angle2 = Math.Atan((point2.Y - point1.Y) / (point2.X - point1.X));
            if (invert1)
            {
                return new Point3d(
                point1.X - (Height * Math.Sin(angle)),
                point1.Y - (point1.X > point2.X ? (Height * Math.Cos(angle)) : (Height * Math.Cos(angle) * -1)),
                point1.Z);
            }
            else if (invert2)
            {
                return new Point3d(
                point1.X + (Height * Math.Sin(angle)),
                point1.Y + (point1.X > point2.X ? (Height * Math.Cos(angle)) : (Height * Math.Cos(angle) * -1)),
                point1.Z);
            }
            return new Point3d(
            point1.X + (Height * Math.Sin(angle)),
            point1.Y - (Height * Math.Cos(angle)),
            point1.Z);
        }
        public static Point3d PolarPoint(Point3d point1, Point3d point2, Point3d point3, double radius)
        {
            double angle1 = Math.Asin((point2.Y - point1.Y) / (point2.X - point1.X));
            double angle2 = Math.Asin((point3.Y - point2.Y) / (point3.X - point2.X));
            double angle = angle1 + angle2;
            // credits to Tony Tanzillo
            return new Point3d(
            point2.X + (radius * Math.Cos(angle)),
            point2.Y + (radius * Math.Sin(angle)),
            point2.Z);
        }
        public override hresult PlaceObject(PlaceFlags lInsertType)
        {
            InputJig jig = new InputJig();
            InputResult res = jig.GetPoint("Select first point:");
            if (res.Result != InputResult.ResultCode.Normal)
                return hresult.e_Fail;
            _pnt = res.Point;
            DbEntity.AddToCurrentDocument();
            //Exclude this from osnap to avoid osnap to itself
            jig.ExcludeObject(ID);
            //Monitor mouse move
            jig.MouseMove = (s, a) => {
                TryModify();
                _pnt2 = a.Point;
                _pnt3 = new Point3d(_pnt2.X, _pnt.Y, 0);
                _pnt1 = new Point3d(_pnt.X, _pnt2.Y, 0);
                DbEntity.Update();
            };

            res = jig.GetPoint("Select second point:");
            if (res.Result != InputResult.ResultCode.Normal)
            {
                DbEntity.Erase();
                return hresult.e_Fail;
            }
            _pnt2 = res.Point;
            _pnt1 = new Point3d(_pnt.X, _pnt2.Y, 0);
            _pnt3 = new Point3d(_pnt2.X, _pnt.Y, 0);
            return hresult.s_Ok;
        }
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify()) return;
            _pnt = _pnt.TransformBy(tfm);
            _pnt1 = _pnt1.TransformBy(tfm);
            _pnt2 = _pnt2.TransformBy(tfm);
            _pnt3 = _pnt3.TransformBy(tfm);
        }
        public override List<Point3d> OnGetGripPoints()
        {
            List<Point3d> arr = new List<Point3d>();
            arr.Add(_pnt);
            arr.Add(_pnt1);
            arr.Add(_pnt2);
            arr.Add(_pnt3);
            return arr;
        }
        public override void OnMoveGripPoints(List<int> indexes, Vector3d offset, bool isStretch)
        {
            if (!TryModify()) return;
            if (indexes.Count >= 2)
            {
                _pnt += offset;
                _pnt1 += offset;
                _pnt2 += offset;
                _pnt3 += offset;
            }
            else if (indexes.Count == 1)
            {
                switch (indexes[0])
                {
                    case 0:
                        _pnt += offset;
                        break;
                    case 1:
                        _pnt1 += offset;
                        break;
                    case 2:
                        _pnt2 += offset;
                        break;
                    case 3:
                        _pnt3 += offset;
                        break;
                }
            }
        }
    }
    public static class Shape
    {
        public static double AngleTo(this Point3d firstPoint, Point3d secondPoint)
        {
            double angle = Math.Atan2(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y);
            angle *= 180 / Math.PI;

            return 90 - angle;
        }
        public static Point3d Rotate(this Point3d point, Point3d center, double angle)
        {
            double radians = (angle) * (Math.PI / 180);

            double X = point.X - center.X;
            double Y = point.Y - center.Y;

            double rotatedX = X * Math.Cos(radians) - Y * Math.Sin(radians);
            double rotatedY = X * Math.Sin(radians) + Y * Math.Cos(radians);

            Point3d rotetedPoint = new Point3d(center.X + rotatedX, center.Y + rotatedY, 0);

            return rotetedPoint;
        }
        public static Point3d[] Rotate(this Point3d[] shape, double angle)
        {
            Point3d[] newShape = shape;
            for (int i = 0; i < shape.Length; i++)
            {
                Point3d point = newShape[i];
                point = point.Rotate(newShape[0], angle);
                newShape[i] = point;
            }
            return newShape;
        }
    }
    class BendingShape
    {
        private Point3d[] _shape;
        public Point3d[] Shape { get { return _shape; } }
        public double Angle { get; }
        public BendingShape(Point3d firstPoint, Point3d secondPoint, double Height)
        {
            _shape = new Point3d[] {
                 new Point3d(firstPoint.X, firstPoint.Y, 0),
                 new Point3d(firstPoint.X, firstPoint.Y + Height, 0),
                 new Point3d(firstPoint.X + firstPoint.DistanceTo(secondPoint), firstPoint.Y + Height, 0),
                 new Point3d(firstPoint.X + firstPoint.DistanceTo(secondPoint), firstPoint.Y, 0)
             };
            Angle = GetAngle(firstPoint, secondPoint);
            Rotate(firstPoint);
        }
        public BendingShape(Point3d firstPoint, Point3d secondPoint, double Height, double overlap)
        {
            _shape = new Point3d[] {
                 new Point3d(firstPoint.X, firstPoint.Y - overlap, 0),
                 //new Point3d(firstPoint.X, firstPoint.Y, 0),
                 new Point3d(firstPoint.X, firstPoint.Y + Height - overlap, 0),
                 new Point3d(firstPoint.X + firstPoint.DistanceTo(secondPoint), firstPoint.Y + Height - overlap, 0),
                 //new Point3d(firstPoint.X + firstPoint.DistanceTo(secondPoint), firstPoint.Y, 0),
                 new Point3d(firstPoint.X + firstPoint.DistanceTo(secondPoint), firstPoint.Y - overlap, 0)
             };
            Angle = GetAngle(firstPoint, secondPoint);
            Rotate(firstPoint);
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
            Point3d[] newShape = Shape;
            for (int i = 0; i < Shape.Length; i++)
            {
                Point3d point = newShape[i];
                point = point.Rotate(firstPoint, Angle);
                newShape[i] = point;
            }
            _shape = newShape;

        }
    }
}