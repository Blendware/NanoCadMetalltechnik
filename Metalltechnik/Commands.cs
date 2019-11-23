using Multicad.Runtime;
using Metallwork;

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
        [CommandMethod("ArrayBlock", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
        static public void ArrayBlock()
        {
            ArrayBlock obj = new ArrayBlock();
            obj.PlaceObject();
        }
        [CommandMethod("Conjunction", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
        static public void Conjunction()
        {
            Conjunction obj = new Conjunction();
            obj.PlaceObject();
        }
        [CommandMethod("Conjunction_female", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
        static public void Conjunction_female()
        {
            Conjunction_female obj = new Conjunction_female();
            obj.PlaceObject();
        }
    }
}
