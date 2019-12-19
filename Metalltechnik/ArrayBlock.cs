using System;
using System.Collections.Generic;
using Multicad;
using Multicad.Runtime;
using Multicad.DatabaseServices;
using Multicad.Geometry;
using Multicad.CustomObjectBase;
using System.ComponentModel;
using HostMgd.EditorInput;
using Multicad.DatabaseServices.StandardObjects;
using System.Windows.Forms;
using Multicad.AplicationServices;

namespace Metallwork
{
    [CustomEntity(typeof(ArrayBlock), "6de33a6e-a784-11e9-a2a3-2a2ae2dbccf6", "ArrayBlock", "ArrayBlock")]
    [Serializable]
    internal class ArrayBlock : McCustomBase
    {
        private List<McBlockRef> _blockRef = new List<McBlockRef>();
        private String _block_name = "ArrayBlock";
        private Point3d _pnt1 = new Point3d(0, 0, 0);
        private Point3d _pnt2 = new Point3d(100, 100, 0);
        private McObjectId _idRef;
        private int _count = 1;

        public ArrayBlock()
        {
        }

        [DisplayName("Count")]
        [Category("Parameter")]
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                if (!TryModify()) return;

                if (value > _count)
                {
                    for (int i = 0; i < value-_count; i++)
                    {
                        _blockRef.Add(_blockRef[0].DbEntity.Clone());
                    }
                }
                else if (value < _count)
                {
                    for (int i = _count-value+2; i > 2; i--)
                    {
                        _blockRef[i].DbEntity.Erase();
                        _blockRef.RemoveAt(i);
                    }
                }
                _count = value;
            }
        }
        [DisplayName("Block name")]
        [Category("Parameter")]
        public string Block_name
        {
            get
            {
                return _block_name;
            }
            set
            {
                if (!TryModify()) return;
                _block_name = value;
            }
        }

        public override void OnDraw(GeometryBuilder dc)
        {
            dc.Clear();
            dc.Color = McDbEntity.ByObject;
            _blockRef[0].DbEntity.Erase();
            _blockRef[1].DbEntity.Erase();

            _blockRef[0].InsertPoint = _pnt1;
            _blockRef[0].DbEntity.AddToCurrentDocument();


            double inbetween = Math.Round(_pnt1.DistanceTo(_pnt2) / (Count + 1), 4);

            for (int i = 1; i <= Count; i++)
            {
                double distance = inbetween * i;

                _blockRef[i+1].DbEntity.Erase();

                _blockRef[i+1].InsertPoint = new Point3d(_pnt1.X + distance, _pnt1.Y, 0).Rotate(_pnt1, _pnt1.AngleTo(_pnt2));
                _blockRef[i+1].DbEntity.AddToCurrentDocument();
            }

            _blockRef[1].InsertPoint = _pnt2;
            _blockRef[1].DbEntity.AddToCurrentDocument();

            dc.Color = System.Drawing.Color.Cyan;
            dc.DrawLeader(_pnt2, _pnt1,Arrows.Arrow, 5);
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

            _idRef = document.GetBlock(_block_name);

            res = jig.GetPoint("Select first point:");
            if (res.Result != InputResult.ResultCode.Normal)
                return hresult.e_Fail;
            _pnt1 = res.Point;
            foreach (McObjectId obj in SelectObjects)
            {
                McDbObject item = obj.GetObject();
                item.Erase();
            }

            McBlockRef blockref = new McBlockRef();
            blockref.BlockName = _block_name;
            blockref.InsertPoint = res.Point;
            blockref.DbEntity.AddToCurrentDocument();
            _blockRef.Add(blockref);
            _blockRef.Add(_blockRef[0].DbEntity.Clone());
            _blockRef.Add(_blockRef[0].DbEntity.Clone());
            DbEntity.AddToCurrentDocument();
            //Exclude this from osnap to avoid osnap to itself
            jig.ExcludeObject(ID);
            //Monitor mouse move
            jig.MouseMove = (s, a) => {
                TryModify();
                _pnt2 = a.Point;
                DbEntity.Update();
            };

            res = jig.GetPoint("Select second point:", res.Point);
            if (res.Result != InputResult.ResultCode.Normal)
            {
                DbEntity.Erase();
                blockref.DbEntity.Erase();
                return hresult.e_Fail;
            }
            _pnt2 = res.Point;

            Editor ed = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            PromptIntegerOptions opts = new PromptIntegerOptions("Enter Count number: ");

            opts.AllowNegative = false;
            opts.AllowZero = false;
            opts.DefaultValue = 1;
            PromptIntegerResult pr = ed.GetInteger(opts);

            if (PromptStatus.OK == pr.Status)
            {
                ed.WriteMessage("You entered: " + pr.StringResult);
                _count = pr.Value;

                _blockRef.Add(blockref.DbEntity.Clone());
                _blockRef[1].InsertPoint = res.Point;
                _blockRef[1].DbEntity.AddToCurrentDocument();
            }
            else
            {
                _count = 1;

                _blockRef.Add(blockref.DbEntity.Clone());
                _blockRef[1].InsertPoint = res.Point;
                _blockRef[1].DbEntity.AddToCurrentDocument();
            }

            for (int i = 1; i < Count; i++)
            {
                _blockRef.Add(_blockRef[0].DbEntity.Clone());
            }

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
            List<Point3d> arr = new List<Point3d>();
            arr.Add(_pnt1);
            arr.Add(_pnt2);
            return arr;
        }
        public override void OnMoveGripPoints(List<int> indexes, Vector3d offset, bool isStretch)
        {
            if (!TryModify()) return;
            for (int i = 0; i < indexes.Count; i++)
            {
                if (indexes[i] == 0)
                {
                    _pnt1 += offset;
                }
                if (indexes[i] == 1)
                {
                    _pnt2 += offset;
                }
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

    internal class res
    {
    }
}