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


namespace Multicad.Samples
{
  [CustomEntity(typeof(TextInBox), "1C925FA1-842B-49CD-924F-4ABF9717DB62", "TextInBox", "TextInBox Sample Entity")]
	[Serializable]
	public class TextInBox : McCustomBase
	{
		private Point3d _pnt = new Point3d(50, 50, 0);
		private Point3d _pnt2 = new Point3d(150, 100, 0);
		private String _Text = "Text field";
		public TextInBox()
		{
		}
		[DisplayName("Text label")]
		[Description("Text label description")]
		[Category("Test entity")]
		public String Text
		{
			get
			{
				return _Text;
			}
			set
			{
				if (!TryModify()) return;//without this call undo will not be saved and the object will not be redrawn
				_Text = value;
			}
		}
		public override void OnDraw(GeometryBuilder dc)
		{
			dc.Clear();
      dc.Color = McDbEntity.ByObject;//color will be taken from object properties and object will be redrawn after color change
			dc.DrawPolyline(new Point3d[] { _pnt, new Point3d(_pnt.X, _pnt2.Y, 0), _pnt2, new Point3d(_pnt2.X, _pnt.Y, 0), _pnt});
			dc.TextHeight = 2.5 * DbEntity.Scale;	//Use annotation scale
			dc.Color = Color.Blue;//Draw text in blue
			dc.DrawMText(new Point3d((_pnt2.X + _pnt.X) / 2.0, (_pnt2.Y + _pnt.Y) / 2.0, 0), Vector3d.XAxis, Text, HorizTextAlign.Center, VertTextAlign.Center);
		}
		public override void OnTransform(Matrix3d tfm)
		{
			if (!TryModify()) return;
			_pnt = _pnt.TransformBy(tfm);
			_pnt2 = _pnt2.TransformBy(tfm);
		}
		public override List<Point3d> OnGetGripPoints()
		{
			List<Point3d> arr = new List<Point3d>();
			arr.Add(_pnt);
			arr.Add(_pnt2);
			return arr;
		}
		public override void OnMoveGripPoints(List<int> indexes, Vector3d offset, bool isStretch)
		{
			if (!TryModify()) return;
			if (indexes.Count == 2)
			{
				_pnt += offset;
				_pnt2 += offset;
			}
			else if (indexes.Count == 1)
			{
				if (indexes[0] == 0)
				{
					_pnt += offset;
				}
				else
				{
					_pnt2 += offset;
				}
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
			jig.MouseMove = (s, a) => { TryModify(); _pnt2 = a.Point; DbEntity.Update(); };

			res = jig.GetPoint("Select second point:");
			if (res.Result != InputResult.ResultCode.Normal)
			{
				DbEntity.Erase();
				return hresult.e_Fail;
			}
			_pnt2 = res.Point;
			return hresult.s_Ok;
		}
		public override hresult OnEdit(Point3d pnt, EditFlags lInsertType)
		{
			TextInBox_Form frm = new TextInBox_Form();
			frm.textBox1.Text = Text;
			frm.ShowDialog();
			Text = frm.textBox1.Text;
			return hresult.s_Ok;
		}
	}
}
