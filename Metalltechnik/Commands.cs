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

using Metallwork;

namespace Multicad.Samples
{
	public class Commands
	{
		[CommandMethod("TextInBox", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
		static public void TextInBoxCmd()
		{
			TextInBox obj = new TextInBox();
			obj.DbEntity.Color = Color.Red;
			obj.PlaceObject();
		}
        [CommandMethod("BBox", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
        static public void BBox()
        {
            BBox obj = new BBox();
            obj.PlaceObject();
        }
    }
}
