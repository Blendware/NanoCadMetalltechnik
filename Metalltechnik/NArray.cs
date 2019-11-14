using System;
using System.Collections.Generic;
using Multicad;
using Multicad.Runtime;
using Multicad.DatabaseServices;
using Multicad.Geometry;
using Multicad.CustomObjectBase;
using System.Windows.Forms;
using Multicad.DatabaseServices.StandardObjects;

namespace Metallwork
{
    [CustomEntity(typeof(NArray), "6de33a6e-a784-11e9-a2a3-2a2ae2dbccf6", "NArray", "NArray")]
    [Serializable]
    internal class NArray : McCustomBase
    {
        private Point3d _pnt1 = new Point3d(0, 0, 0);
        private Point3d _pnt2 = new Point3d(100, 100, 0);

        public NArray()
        {
        }
        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;

            McBlockRef test = new McBlockRef();
            test.BlockName = "Bending Offset";
            test.InsertPoint = _pnt1;

            McDocument doc = McDocumentsManager.GetActiveDoc();
            if (doc == null) return;
            McObjectId idRef = doc.GetBlock("Bending Offset");

            ObjGeomRef blockRef = new ObjGeomRef();
            blockRef.ObjectId = idRef;

            //blockRef.Tfm = Matrix3d.Identity;
            blockRef.TranslateBy(_pnt1.GetAsVector());
            EntityGeometry geom = new EntityGeometry(blockRef);
            dc.DrawGeometry(geom, 1);

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
            McDocument block = document.CreateBlock("Bending Offset", true);

            foreach (McObjectId obj in SelectObjects)
            {
                McDbObject item = obj.GetObject();
                block.AddObject(item.Clone());
            }
            InputResult res = jig.GetPoint("Select first point:");
            if (res.Result != InputResult.ResultCode.Normal)
                return hresult.e_Fail;
            _pnt1 = res.Point;
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

            //if (selection.IsKindOf(DbPolyline.TypeID))
            //{
            //    poly = new Polyline3d(selection.Geometry.Polyline);
            //}
            //else if (selection.IsKindOf(DbLine.TypeID))
            //{
            //    poly = new Polyline3d(selection.Geometry.LineSeg);
            //}
            //else
            //{
            //    MessageBox.Show("Objecttype isn't valid");
            //    DbEntity.Erase();
            //    return hresult.e_Fail;
            //}
            //DbEntity.AddToCurrentDocument();
        }
        public override void OnTransform(Matrix3d tfm)
        {
            if (!TryModify()) return;
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
}