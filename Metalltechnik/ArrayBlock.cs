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
using HostMgd.EditorInput;

namespace Metallwork
{
    [CustomEntity(typeof(ArrayBlock), "6de33a6e-a784-11e9-a2a3-2a2ae2dbccf6", "ArrayBlock", "ArrayBlock")]
    [Serializable]
    internal class ArrayBlock : McCustomBase
    {
        private String _block_name = "ArrayBlock";
        private Point3d _pnt1 = new Point3d(0, 0, 0);
        private Point3d _pnt2 = new Point3d(100, 100, 0);

        public ArrayBlock()
        {
        }

        [DisplayName("Count")]
        [Category("Parameter")]
        public int Count { get; set; } = 1;

        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;

            McDocument doc = McDocumentsManager.GetActiveDoc();
            if (doc == null) return;
            McObjectId idRef = doc.GetBlock(_block_name);

            ObjGeomRef blockRef = new ObjGeomRef();
            blockRef.ObjectId = idRef;

            blockRef.TranslateBy(_pnt1.GetAsVector());
            EntityGeometry geom = new EntityGeometry(blockRef);
            dc.DrawGeometry(geom, 1);

            double inbetween = Math.Round(_pnt1.DistanceTo(_pnt2) / (Count + 1), 4);

            for (int i = 1; i <= Count; i++)
            {
                double distance = inbetween * i;

                ObjGeomRef blockRefInb = new ObjGeomRef();
                blockRefInb.ObjectId = idRef;
                blockRefInb.TranslateBy(new Point3d(_pnt1.X + distance, _pnt1.Y, 0).Rotate(_pnt1, _pnt1.AngleTo(_pnt2)).GetAsVector());
                EntityGeometry geomInb = new EntityGeometry(blockRefInb);
                dc.DrawGeometry(geomInb, 1);
            }

            ObjGeomRef blockRef2 = new ObjGeomRef();
            blockRef2.ObjectId = idRef;
            blockRef2.TranslateBy(_pnt2.GetAsVector());
            EntityGeometry geom2 = new EntityGeometry(blockRef2);
            dc.DrawGeometry(geom2, 1);
        }
        public override hresult PlaceObject(PlaceFlags lInsertType)
        {
            InputJig jig = new InputJig();
            List<McObjectId> SelectObjects = jig.SelectObjects("Select Object");
            //InputResult res = jig.SelectObject("Select a Polyline");
            //McObjectId id = SelectObjects[0];//.ObjectId;
            //DbGeometry selection = id.GetObject();
            if (SelectObjects.Count == 0)
            {
                DbEntity.Erase();
                return hresult.e_Fail;
            }

            McDocument document = McDocumentsManager.GetActiveDoc();
            McDocument block = document.CreateBlock("ArrayBlock", true);
            _block_name = block.Name;

            InputResult res = jig.GetPoint("Select Base Point:");

            foreach (McObjectId obj in SelectObjects)
            {
                McDbObject item = obj.GetObject();
                item.Entity.DbEntity.Transform(Matrix3d.Displacement(res.Point.GetAsVector().MultiplyBy(-1)));
                block.AddObject(item.Clone());
            }
            res = jig.GetPoint("Select first point:");
            if (res.Result != InputResult.ResultCode.Normal)
                return hresult.e_Fail;
            _pnt1 = res.Point;

            foreach (McObjectId obj in SelectObjects)
            {
                McDbObject item = obj.GetObject();
                item.Erase();
            }

            DbEntity.AddToCurrentDocument();
            //Exclude this from osnap to avoid osnap to itself
            jig.ExcludeObject(ID);
            //Monitor mouse move
            jig.MouseMove = (s, a) => { TryModify(); _pnt2 = a.Point; DbEntity.Update(); };

            res = jig.GetPoint("Select second point:", res.Point);
            if (res.Result != InputResult.ResultCode.Normal)
            {
                DbEntity.Erase();
                return hresult.e_Fail;
            }
            _pnt2 = res.Point;
            return hresult.s_Ok;
        }
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify()) return;
            _pnt1 = _pnt1.TransformBy(tfm);
            _pnt2 = _pnt2.TransformBy(tfm);
        }
        public override List<Point3d> OnGetGripPoints()
        {
            List<Multicad.Geometry.Point3d> arr = new List<Point3d>();
            arr.Add(_pnt1);
            arr.Add(_pnt2);
            return arr;
        }
        public override void OnMoveGripPoints(List<int> indexes, Vector3d offset, bool isStretch)
        {
            if (!TryModify()) return;
            if (indexes[0] == 0)
            {
                _pnt1 += offset;
            }
            if (indexes[0] == 1)
            {
                _pnt2 += offset;
            }
        }
        public override hresult OnEdit(Point3d pnt, EditFlags lFlag)
        {
            Editor ed = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            PromptIntegerOptions opts = new PromptIntegerOptions("Enter Count number: ");
            opts.AllowNegative = false;
            opts.AllowZero = false;
            PromptIntegerResult pr = ed.GetInteger(opts);
            if (PromptStatus.OK == pr.Status)
            {
                ed.WriteMessage("You entered: " + pr.StringResult);
                Count = pr.Value;
                DbEntity.Erase();
                DbEntity.AddToCurrentDocument();
            }
            else
            {
                ed.WriteMessage("Cancel");
            }

            return hresult.s_Ok;
        }
    }
}