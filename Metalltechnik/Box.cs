//
// Copyright (C) 2019, Pescoller Tobias.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Multicad;
using Multicad.Runtime;
using Multicad.DatabaseServices;
using Multicad.Geometry;
using Multicad.CustomObjectBase;

namespace Metallwork
{
    [CustomEntity(typeof(Box), "6de33a9e-a784-11e9-a2a3-2a2ae2dbcce5", "Box", "Box")]
    [Serializable]
    internal class Box : McCustomBase
    {
        private Point3d _pnt = new Point3d(50, 50, 0);
        private Point3d _pnt1 = new Point3d(50, 100, 0);
        private Point3d _pnt2 = new Point3d(150, 150, 0);
        private Point3d _pnt3 = new Point3d(150, 50, 0);
        public Box()
        {
        }
        [DisplayName("Thickness")]
        [Category("Parameter")]
        public double Thickness { get; set; } = 5;
        [DisplayName("Width")]
        [Category("Parameter")]
        public double Width { get; set; } = 50;
        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;
            Polyline3d poly = new Polyline3d(new List<Point3d> { _pnt, _pnt1, _pnt2, _pnt3, _pnt });
            Polyline3d poly_offset = poly.GetTrimmedOffset(-Width)[0];

            dc.DrawPolyline(poly);
            dc.DrawPolyline(poly_offset);
            dc.DrawLine(_pnt, poly_offset.Points[0]);
            dc.DrawLine(_pnt1, poly_offset.Points[1]);
            dc.DrawLine(_pnt2, poly_offset.Points[2]);
            dc.DrawLine(_pnt3, poly_offset.Points[3]);
            if (Thickness != 0)
            {
                dc.DrawPolyline(poly.GetTrimmedOffset(-Width/2 + Thickness/2)[0]);
                dc.DrawPolyline(poly.GetTrimmedOffset(-Width / 2 + -Thickness / 2)[0]);
            }
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