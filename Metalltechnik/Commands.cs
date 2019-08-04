//
// Copyright (C) 2019, Pescoller Tobias.  All rights reserved.
//

using System.Drawing;
using Multicad.Runtime;
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
        [CommandMethod("BLine", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
        static public void BLine()
        {
            BLine obj = new BLine();
            obj.PlaceObject();
        }
        [CommandMethod("NBox", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
        static public void Box()
        {
            Box obj = new Box();
            obj.PlaceObject();
        }
    }
}
