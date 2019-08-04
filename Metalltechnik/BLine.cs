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

using Platform = HostMgd;
using HostMgd.EditorInput;

namespace Metallwork
{
    [CustomEntity(typeof(BLine), "6de33a9e-a784-11e9-a2a3-2a2ae2dbcce6", "BLine", "BLine")]
    [Serializable]
    internal class BLine : McCustomBase
    {
        private List<Point3d> _points = new List<Point3d>();
        private Polyline3d poly;
        public BLine()
        {
        }
        [DisplayName("Thickness")]
        [Category("Parameter")]
        public double Thickness { get; set; } = 1.5;

        [DisplayName("Manual V")]
        [Category("Parameter")]
        public int V_true { get; set; } = 0;

        private int V = 16;

        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;//color will be taken from object properties and object will be redrawn after color change

            if (V_true == 0)
            {
                if (Thickness < 3) V = 16;
                else if (Thickness < 4) V = 20;
                else if (Thickness > 4) V = 50;
            }
            else V = V_true;

            Polyline3d offset = poly.GetTrimmedOffset(-Thickness)[0];

            dc.DrawPolyline(poly);
            dc.DrawPolyline(offset);
            dc.DrawLine(poly.Points.FirstPoint, offset.Points.FirstPoint);
            dc.DrawLine(poly.Points.LastPoint, offset.Points.LastPoint);
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
            poly = new Polyline3d();
            poly.Vertices.AddVertex(res.Point);
            poly.Vertices.AddVertex(res.Point);
            Polyline3d.VertexDataOfPolyline polyVertecies = poly.Vertices;
            
            //_points.Add(res.Point);
            //_points.Add(res.Point); //Bug double?
            DbEntity.AddToCurrentDocument();
            //Exclude this from osnap to avoid osnap to itself
            jig.ExcludeObject(ID);
            //Monitor mouse move
            jig.MouseMove = (s, a) => {
                TryModify();
                //_points[_points.Count-1] = a.Point;
                //polyVertecies[polyVertecies.Count - 1].Point = a.Point;
                polyVertecies.RemoveVertexAt(uint.Parse((polyVertecies.Count-1).ToString()));
                polyVertecies.AddVertex(a.Point);
              
                DbEntity.Update();
            };
            while (res.Result == InputResult.ResultCode.Normal)
            {
                res = jig.GetPoint("Select next point(" + polyVertecies.Count + "):");
                //_points.Add(res.Point);
                poly.Vertices.AddVertex(res.Point);


            }
            DbEntity.Erase();

            polyVertecies.RemoveVertexAt(uint.Parse((polyVertecies.Count - 1).ToString()));
            for (int i = 2; i < polyVertecies.Count-1; i++)
            {
                double angle1 = Math.Atan2(polyVertecies[i - 2].Point.X - polyVertecies[i - 1].Point.X, polyVertecies[i - 2].Point.Y - polyVertecies[i - 1].Point.Y);
                double angle2 = Math.Atan2(polyVertecies[i - 1].Point.X - polyVertecies[i].Point.X, polyVertecies[i - 1].Point.Y - polyVertecies[i].Point.Y);

                angle1 *= 180 / Math.PI;
                angle2 *= 180 / Math.PI;
                double anglesum = 180 - angle1 + angle2;

                double radius;
                if (anglesum < 180)
                    radius = V * 0.16;
                else
                    radius = V * 0.16 + Thickness;

                poly.Vertices.MakeFilletAtVertex(i-1, radius);
            }
            polyVertecies.RemoveVertexAt(uint.Parse((polyVertecies.Count - 1).ToString()));
            DbEntity.AddToCurrentDocument();

            return hresult.s_Ok;
        }
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify()) return;
            foreach (Point3d item in _points)
            {
                item.TransformBy(tfm);
            }
        }
        public override List<Point3d> OnGetGripPoints()
        {
            return _points;
        }
        public override void OnMoveGripPoints(List<int> indexes, Vector3d offset, bool isStretch)
        {
            if (!TryModify()) return;
            if (indexes.Count >= 2)
            {
                for (int i = 0; i < _points.Count; i++)
                {
                    _points[i] += offset;
                }
            }
            else if (indexes.Count == 1)
            {
                _points[indexes[0]] += offset;
            }
        }
    }
}