using System;
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
    [CustomEntity(typeof(BLine), "6de33a9e-a784-11e9-a2a3-2a2ae2dbcce6", "BLine", "BLine")]
    [Serializable]
    internal class BLine : McCustomBase
    {
        private List<Point3d> _points = new List<Point3d>();
        private Polyline3d poly;
        private double _length_sum = 0;
        public BLine()
        {
        }
        [DisplayName("Thickness")]
        [Category("Parameter")]
        public double Thickness { get; set; } = 1.5;

        [DisplayName("Manual V")]
        [Category("Parameter")]
        public int V_true { get; set; } = 0;
        [DisplayName("K-factor")]
        [Category("Parameter")]
        public double k_factor { get; set; } = 0.5;
        [DisplayName("Length")]
        [Category("Parameter")]
        public double length_sum { get { return _length_sum; } }

        private int V = 16;
        private Point3d _textPoint = new Point3d(0, 0, 0);

        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;//color will be taken from object properties and object will be redrawn after color change

            if (V_true == 0)
            {
                double pos_thickness = Thickness < 0 ? Thickness * -1 : Thickness;
                if (pos_thickness < 3) V = 16;
                else if (pos_thickness == 3) V = 20;
                else if (pos_thickness == 4) V = 35;
                else if (pos_thickness > 4) V = 50;
            }
            else V = V_true;

            Polyline3d polyround = new Polyline3d(poly);

            for (int i = 2; i < polyround.Vertices.Count; i++)
            {
                double angle1 = Math.Atan2(polyround.Vertices[i - 2].Point.X - polyround.Vertices[i - 1].Point.X, polyround.Vertices[i - 2].Point.Y - polyround.Vertices[i - 1].Point.Y);
                double angle2 = Math.Atan2(polyround.Vertices[i - 1].Point.X - polyround.Vertices[i].Point.X, polyround.Vertices[i - 1].Point.Y - polyround.Vertices[i].Point.Y);

                //if (angle1 < 0) angle1 *= -1;
                //if (angle2 < 0) angle2 *= -1;
                double anglesum = Math.Atan((angle2 - angle1) / (1 + angle2 * angle1));//angle1 + angle2

                double radius;
                if (anglesum < 0)
                    radius = V * 0.16;
                else
                    radius = V * 0.16 + Thickness;
                if (Thickness < 0) radius += Thickness * -1;


                polyround.Vertices.MakeFilletAtVertex(i - 1, radius);
            }
            Polyline3d offset = polyround.GetTrimmedOffset(-Thickness)[0];

            dc.DrawPolyline(polyround);
            dc.DrawPolyline(offset);
            dc.DrawLine(polyround.Points.FirstPoint, offset.Points.FirstPoint);
            dc.DrawLine(polyround.Points.LastPoint, offset.Points.LastPoint);
            Polyline3d middlePoly = polyround.GetTrimmedOffset(-Thickness * (Thickness < 0 ? 1-k_factor : k_factor))[0];
            dc.TextHeight = 2.5 * DbEntity.Scale;   //Use annotation scale
            _length_sum = middlePoly.Length;
            dc.DrawMText(_textPoint, Vector3d.XAxis, Math.Round(middlePoly.Length,1).ToString(), HorizTextAlign.Center, VertTextAlign.Center);
            dc.StrLineType = "Center";
            dc.Color = Color.Yellow;
            dc.LinetypeScale = DbEntity.LinetypeScale*0.3;
            dc.DrawPolyline(middlePoly);
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
            _textPoint = new Point3d(res.Point.X, res.Point.Y - 5 - Thickness, 0);
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
                res = jig.GetPoint("Select next point(" + polyVertecies.Count + "):", res.Point);
                //_points.Add(res.Point);
                poly.Vertices.AddVertex(res.Point);


            }
            DbEntity.Erase();

            polyVertecies.RemoveVertexAt(uint.Parse((polyVertecies.Count - 1).ToString()));
            polyVertecies.RemoveVertexAt(uint.Parse((polyVertecies.Count - 1).ToString()));
            DbEntity.AddToCurrentDocument();

            return hresult.s_Ok;
        }
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify()) return;

            _textPoint = _textPoint.TransformBy(tfm);
            poly.TransformBy(tfm);
        }
        public override List<Point3d> OnGetGripPoints()
        {
            List<Point3d> arr = new List<Point3d>();
            arr.Add(_textPoint);
            for (int i = 0; i < poly.Vertices.Count; i++)
            {
                arr.Add(poly.Points[i]);
            }
            return arr;
        }
        public override void OnMoveGripPoints(List<int> indexes, Vector3d offset, bool isStretch)
        {
            if (!TryModify()) return;
            for (int i = 0; i < indexes.Count; i++)
            {
                if (indexes[i] == 0)
                {
                    _textPoint += offset;
                }
                else
                {
                    poly.Points[indexes[i] - 1] += offset;
                }
            }
        }
    }
}