using System.Drawing;
using Multicad.Runtime;
using Metallwork;
using Multicad.DatabaseServices;
using System.Windows.Forms;
using Multicad.DatabaseServices.StandardObjects;

namespace Multicad.Samples
{
	public class Commands
	{
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
        [CommandMethod("BOffset", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
        static public void BOffset()
        {
            BOffset obj = new BOffset();
            obj.PlaceObject();
        }
        [CommandMethod("Conjunction", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
        static public void Conjunction()
        {
            Conjunction obj = new Conjunction();
            obj.PlaceObject();
        }
    }
}
