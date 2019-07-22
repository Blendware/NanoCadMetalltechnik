//
// Copyright (C) 2012, ZAO Nanosoft.  All rights reserved.
//
// This software, all its documentation and related materials (the Software) 
// is owned by ZAO Nanosoft. The Software can be used to develop any software in accordance 
// with the terms of ZAO Nanosoft's "nanoCAD Developer Community Program Agreement".
//
// The Software is protected by copyright laws and relevant international treaty provisions.
//
// By use of the Software you acknowledge and accept the above terms.
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
    [CustomEntity(typeof(BBox), "6de33a9e-a784-11e9-a2a3-2a2ae2dbcce4", "BBox", "BBox Entity")]
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

            Point3d[] pointList = {
                PerpendicularPoint(_pnt, _pnt3),              //0
                PerpendicularPoint(_pnt, _pnt1, true),        //1
                PerpendicularPoint(_pnt1, _pnt2, true),       //2
                PerpendicularPoint(_pnt1, _pnt, false, true), //3
                PerpendicularPoint(_pnt2, _pnt3, true),       //4
                PerpendicularPoint(_pnt2, _pnt1, false, true),//5
                PerpendicularPoint(_pnt3, _pnt, true),        //6
                PerpendicularPoint(_pnt3, _pnt2, false, true) //7
        };
            Polyline3d poly = new Polyline3d(new List<Point3d> {
                _pnt,
                PerpendicularPoint(_pnt, _pnt1, -Thickness-0.5, true, true),
                pointList[1],
                pointList[3],
                PerpendicularPoint(_pnt1, _pnt, -Thickness-0.5, false, true),
                _pnt1,
                pointList[2],
                pointList[5],
                _pnt2,
                pointList[4],
                pointList[7],
                _pnt3,
                pointList[6],
                pointList[0],
                _pnt
        });
            Polyline3d.VertexDataOfPolyline polyVertecies = poly.Vertices;
            for (int i = 0; i < polyVertecies.Count; i+= 2)
            {
                polyVertecies.MakeFilletAtVertex(i, Radius);
            }

            dc.DrawPolyline(poly);
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
                point1.Y - (point1.X > point2.X ? (Height * Math.Cos(angle)) : (Height * Math.Cos(angle)*-1)),
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
            double angle1 = Math.Asin((point2.Y - point1.Y) / (point2.X-point1.X));
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
}