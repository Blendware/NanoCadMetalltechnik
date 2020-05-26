﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using Multicad;
using Multicad.Runtime;
using Multicad.DatabaseServices;
using Multicad.Geometry;
using Multicad.CustomObjectBase;

namespace Metallwork
{
    [CustomEntity(typeof(Table_feet), "6de33a9e-a784-11e9-a2a3-2a2ae2dbccf6", "Table_feet", "Table_feet")]
    [Serializable]
    internal class Table_feet : McCustomBase
    {
        private Point3d _pnt1 = new Point3d(0,0,0);
        private Point3d _pnt2 = new Point3d(600, 0, 0);
        private Point3d _pnt3 = new Point3d(0, 200, 0);
        private Point3d _pnt4 = new Point3d(0, 800, 0);

        public Table_feet()
        {
        }

        [DisplayName("Count")]
        [Category("Parameter")]
        public int Count { get; set; } = 5;
        [DisplayName("Angle1")]
        [Category("Parameter")]
        public int Angle1 { get; set; } = 25;
        [DisplayName("Angle2")]
        [Category("Parameter")]
        public int Angle2 { get; set; } = 75;

        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;//color will be taken from object properties and object will be redrawn after color change
            
            //Outside rechtangle
            Polyline3d poly = new Polyline3d(new List<Point3d> {_pnt1, _pnt3, new Point3d(_pnt2.X,_pnt3.Y,0), _pnt2});
            poly.SetClosed(true);
            for (int i = 0; i < 8; i+=2)
            {
                poly.Vertices.MakeFilletAtVertex(i, 8);
            }
            dc.DrawPolyline(poly);

            //Fl 100x15
            Polyline3d flat_bar = RotatedRechtangle(100, 15, new Point3d(_pnt1.X+95.7, _pnt1.Y + (_pnt3.Y - _pnt1.Y)/2, 0), Angle1);
            Polyline3d flat_bar2 = RotatedRechtangle(100, 15, new Point3d(_pnt2.X - 95.7, _pnt1.Y + (_pnt3.Y - _pnt1.Y) / 2, 0), -Angle1);
            dc.DrawPolyline(flat_bar);
            dc.DrawPolyline(flat_bar2);


            //Side view
            Point3d pnt4_base = new Point3d(_pnt3.X, _pnt3.Y + 200, 0);
            dc.DrawPolyline(new Point3d[] { pnt4_base, new Point3d(pnt4_base.X, pnt4_base.Y + 15, 0), new Point3d(pnt4_base.X+100, pnt4_base.Y + 15, 0), new Point3d(pnt4_base.X+100, pnt4_base.Y + 15, 0), new Point3d(pnt4_base.X+100, pnt4_base.Y, 0), pnt4_base });
            Point3d top_position = TopPosition(Angle2);
            dc.DrawLine(new Point3d(pnt4_base.X, pnt4_base.Y + 15, 0), top_position);
            dc.DrawLine(new Point3d(pnt4_base.X+100, pnt4_base.Y + 15, 0), new Point3d(top_position.X+100, top_position.Y,0));
            //dc.DrawLine(top_position, new Point3d(top_position.X + 100, top_position.Y, 0));
            double rech_offset = (_pnt1.DistanceTo(_pnt3)-100)/2;
            dc.DrawPolyline(new Point3d[] { new Point3d(top_position.X - rech_offset, top_position.Y, 0), new Point3d(top_position.X - rech_offset, top_position.Y + 6, 0), new Point3d(top_position.X + 100 + rech_offset, top_position.Y + 6, 0), new Point3d(top_position.X + 100 + rech_offset, top_position.Y + 6, 0), new Point3d(top_position.X + 100 + rech_offset, top_position.Y, 0), new Point3d(top_position.X - rech_offset, top_position.Y, 0) });

            //Holes
            dc.DrawCircle(new Point3d(_pnt1.X + 35, _pnt1.Y + 25, 0), 2.75);
            dc.DrawCircle(new Point3d(_pnt1.X + 35, _pnt3.Y - 25, 0), 2.75);
            dc.DrawCircle(new Point3d(_pnt2.X - 35, _pnt1.Y + 25, 0), 2.75);
            dc.DrawCircle(new Point3d(_pnt2.X - 35, _pnt3.Y - 25, 0), 2.75);
            double inbetween = Math.Round((_pnt1.DistanceTo(_pnt2) - 70) / (Count - 1), 4);
            for (int i = 1; i < Count - 1; i++)
            {
                double distance = inbetween * i;
                dc.DrawCircle(new Point3d(_pnt1.X + 35 + distance, _pnt1.Y + 25, 0), 2.75);
                dc.DrawCircle(new Point3d(_pnt1.X + 35 + distance, _pnt3.Y - 25, 0), 2.75);
            }

        }
        private Polyline3d RotatedRechtangle(double length, double width, Point3d center_position, double angle)
        {
            angle = 90 - angle;
            List<Point3d> points = new List<Point3d>();
            points.Add(new Point3d(center_position.X - length / 2, center_position.Y - width / 2, 0).Rotate(center_position, angle));
            points.Add(new Point3d(center_position.X - length / 2, center_position.Y + width / 2, 0).Rotate(center_position, angle));
            points.Add(new Point3d(center_position.X + length / 2, center_position.Y + width / 2, 0).Rotate(center_position, angle));
            points.Add(new Point3d(center_position.X + length / 2, center_position.Y - width / 2, 0).Rotate(center_position, angle));

            Polyline3d poly = new Polyline3d(points);
            poly.SetClosed(true);
            return poly;
        }
        private Point3d TopPosition(double angle)
        {
            angle = angle * Math.PI / 180;
            double height = _pnt4.DistanceTo(new Point3d(_pnt3.X, _pnt3.Y + 200+15, 0))-6;
            double width = height / Math.Tan(angle);

            return new Point3d(_pnt4.X+width, _pnt4.Y-6, 0);
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
            _pnt1 = res.Point;
            DbEntity.AddToCurrentDocument();
            //Exclude this from osnap to avoid osnap to itself
            jig.ExcludeObject(ID);
            //Monitor mouse move
            jig.MouseMove = (s, a) => {
                TryModify();
                _pnt2 = _pnt2 = new Point3d(a.Point.X, _pnt1.Y, 0);
                _pnt3 = new Point3d(_pnt1.X, _pnt1.Y + 200, 0);
                _pnt4 = new Point3d(_pnt1.X, _pnt1.Y + 1000, 0);
                DbEntity.Update();
            };

            res = jig.GetPoint("Select second point:", res.Point);
            if (res.Result != InputResult.ResultCode.Normal)
            {
                DbEntity.Erase();
                return hresult.e_Fail;
            }
            _pnt2 = new Point3d(res.Point.X,_pnt1.Y,0);
            _pnt3 = new Point3d(_pnt1.X, _pnt1.Y + 200, 0);
            _pnt4 = new Point3d(_pnt1.X, _pnt1.Y + 1000, 0);
            return hresult.s_Ok;
        }
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify()) return;

            _pnt1 = _pnt1.TransformBy(tfm);
            _pnt2 = _pnt2.TransformBy(tfm);
            _pnt3 = _pnt3.TransformBy(tfm);
            _pnt4 = _pnt4.TransformBy(tfm);
        }
        public override List<Point3d> OnGetGripPoints()
        {
            List<Point3d> arr = new List<Point3d>();
            arr.Add(_pnt1);
            arr.Add(_pnt2);
            arr.Add(_pnt3);
            arr.Add(_pnt4);
            return arr;
        }
        public override void OnMoveGripPoints(List<int> indexes, Vector3d offset, bool isStretch)
        {
            if (!TryModify()) return;
            for (int i = 0; i < indexes.Count; i++)
            {
                if (indexes[i] == 0)
                {
                    _pnt1 = new Point3d(_pnt1.X + offset.X, _pnt1.Y, 0);
                    _pnt3 = new Point3d(_pnt1.X, _pnt3.Y, 0);
                    _pnt4 = new Point3d(_pnt4.X + offset.X, _pnt4.Y, 0);
                }
                else if (indexes[i] == 1) _pnt2 = new Point3d(_pnt2.X + offset.X, _pnt2.Y, 0);
                else if (indexes[i] == 2)
                {
                    _pnt3 = new Point3d(_pnt3.X, _pnt3.Y + offset.Y, 0);
                    _pnt4 = new Point3d(_pnt4.X, _pnt4.Y + offset.Y, 0);
                }
                else if (indexes[i] == 3) _pnt4 = new Point3d(_pnt1.X, _pnt4.Y + offset.Y, 0);
            }
        }
    }
}