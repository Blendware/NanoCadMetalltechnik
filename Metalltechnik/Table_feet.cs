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
        public double Angle1 { get; set; } = 25;
        [DisplayName("Angle2")]
        [Category("Parameter")]
        public double Angle2 { get; set; } = 75;
        [DisplayName("FL Length")]
        [Category("Parameter")]
        public double Fl_width { get; set; } = 100;
        [DisplayName("FL Thickness")]
        [Category("Parameter")]
        public double Fl_tickness { get; set; } = 15;

        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;//color will be taken from object properties and object will be redrawn after color change

            //HostMgd.EditorInput.Editor ed = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            //Outside rechtangle
            Polyline3d poly = new Polyline3d(new List<Point3d> {_pnt1, _pnt3, new Point3d(_pnt2.X,_pnt3.Y,0), _pnt2});
            poly.SetClosed(true);
            for (int i = 0; i < 8; i+=2)
            {
                poly.Vertices.MakeFilletAtVertex(i, 8);
            }
            dc.DrawPolyline(poly);

            //Fl 100x15
            Polyline3d flat_bar1 = RotatedRechtangle(Fl_width, Fl_tickness, new Point3d(_pnt1.X+95.7, _pnt1.Y + (_pnt3.Y - _pnt1.Y)/2, 0), Angle1);
            Polyline3d flat_bar2 = RotatedRechtangle(Fl_width, Fl_tickness, new Point3d(_pnt2.X - 95.7, _pnt1.Y + (_pnt3.Y - _pnt1.Y) / 2, 0), -Angle1);
            dc.DrawPolyline(flat_bar1);
            dc.DrawPolyline(flat_bar2);

            //Side view
            Point3d pnt4_base = new Point3d(_pnt3.X, _pnt3.Y + 200, 0);
            dc.DrawPolyline(new Point3d[] { pnt4_base, new Point3d(pnt4_base.X, pnt4_base.Y + Fl_tickness, 0), new Point3d(pnt4_base.X+Fl_width, pnt4_base.Y + Fl_tickness, 0), new Point3d(pnt4_base.X+Fl_width, pnt4_base.Y + Fl_tickness, 0), new Point3d(pnt4_base.X+Fl_width, pnt4_base.Y, 0), pnt4_base });
            Point3d top_position = TopPosition(Angle2);
            dc.DrawLine(new Point3d(pnt4_base.X, pnt4_base.Y + Fl_tickness, 0), top_position);
            dc.DrawLine(new Point3d(pnt4_base.X+Fl_width, pnt4_base.Y + Fl_tickness, 0), new Point3d(top_position.X+Fl_width, top_position.Y,0));
            //dc.DrawLine(top_position, new Point3d(top_position.X + 100, top_position.Y, 0));
            double rech_offset = (_pnt1.DistanceTo(_pnt3)-Fl_width)/2;
            dc.DrawPolyline(new Point3d[] { new Point3d(top_position.X - rech_offset, top_position.Y, 0), new Point3d(top_position.X - rech_offset, top_position.Y + 6, 0), new Point3d(top_position.X + Fl_width + rech_offset, top_position.Y + 6, 0), new Point3d(top_position.X + Fl_width + rech_offset, top_position.Y + 6, 0), new Point3d(top_position.X + Fl_width + rech_offset, top_position.Y, 0), new Point3d(top_position.X - rech_offset, top_position.Y, 0) });

            //Fl 100x15 bottom view
            double bottom_view_point_offset = BottomViewPointOffset1();
            Point3d rech_pnt1 = new Point3d(flat_bar1.Points[0].X - BottomViewPointOffset2(flat_bar1), flat_bar1.Points[0].Y, 0);
            Point3d rech_pnt2 = new Point3d(flat_bar2.Points[1].X + BottomViewPointOffset2(flat_bar1), flat_bar1.Points[0].Y, 0);
            Point3d bottom_view_point1 = new Point3d(rech_pnt1.X - bottom_view_point_offset, rech_pnt1.Y - (top_position.X - _pnt1.X), 0);
            Point3d bottom_view_point2 = new Point3d(rech_pnt2.X + bottom_view_point_offset, bottom_view_point1.Y, 0);
            dc.DrawLine(bottom_view_point1, bottom_view_point2);
            dc.DrawLine(bottom_view_point1, flat_bar1.Points[1]);
            dc.DrawLine(bottom_view_point2, flat_bar2.Points[0]);

            //Fl 100x15 bottom
            Polyline3d bottom_flat_bar1 = new Polyline3d(flat_bar1);
            Polyline3d bottom_flat_bar2 = new Polyline3d(flat_bar2);
            bottom_flat_bar1.TranslateBy(new Vector3d(-bottom_view_point_offset, _pnt1.X-top_position.X, 0));
            bottom_flat_bar2.TranslateBy(new Vector3d(bottom_view_point_offset, _pnt1.X - top_position.X, 0));
            dc.DrawPolyline(bottom_flat_bar1);
            dc.DrawPolyline(bottom_flat_bar2);

            //Adding remaining Lines
            dc.DrawLine(bottom_flat_bar1.Points[2], bottom_flat_bar2.Points[3]);
            dc.DrawLine(bottom_flat_bar1.Points[3], flat_bar1.Points[0]);
            dc.DrawLine(flat_bar2.Points[1], bottom_flat_bar2.Points[2]);
            double side_view_line_offset = BottomViewPointOffset2(flat_bar1) - (flat_bar1.Points[0].X - flat_bar1.Points[1].X);
            dc.DrawLine(new Point3d(pnt4_base.X + side_view_line_offset, pnt4_base.Y + Fl_tickness, 0), new Point3d(top_position.X + side_view_line_offset, top_position.Y, 0));

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
            double rad = angle * Math.PI / 180;
            double height = _pnt4.DistanceTo(new Point3d(_pnt3.X, _pnt3.Y + 200 + Fl_tickness, 0))-6;
            double width = height / Math.Tan(rad);

            return new Point3d(_pnt4.X+width, _pnt4.Y-6, 0);
        }
        private double BottomViewPointOffset1()
        {
            double rad = Angle1 * Math.PI / 180;
            double distance = TopPosition(Angle2).X - _pnt1.X;
            double offset = distance * Math.Tan(rad);

            return offset;
        }
        private double BottomViewPointOffset2(Polyline3d poly)
        {
            double rad = Angle1 * Math.PI / 180;
            double distance = poly.Points[1].DistanceTo(poly.Points[0]);
            double offset = distance / Math.Cos(rad);

            return offset;
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
                _pnt4 = new Point3d(_pnt1.X, _pnt1.Y + 1100, 0);
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
            _pnt4 = new Point3d(_pnt1.X, _pnt1.Y + 1100, 0);
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