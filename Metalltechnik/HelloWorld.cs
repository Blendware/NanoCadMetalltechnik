using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Teigha.DatabaseServices;
using Teigha.Runtime;
using Teigha.Geometry;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;

using Platform = HostMgd;
using PlatformDb = Teigha;

namespace Metalltechnik
{
/// <summary>
/// Main class
/// </summary>
    public class HelloHost
    {
        /// <summary>
        /// Command line examples
        /// </summary>
        [CommandMethod("Example1")]
        public void Template1()
        {
            Editor ed = Platform.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            // Print message to command line
            ed.WriteMessage("Hello nanoCAD .NET API!");

            PromptStringOptions opts = new PromptStringOptions("Enter string");
            opts.AllowSpaces = true;
            PromptResult pr = ed.GetString(opts);
            if (PromptStatus.OK == pr.Status)
            {
                ed.WriteMessage("You entered: " + pr.StringResult);
            }
            else
            {
                ed.WriteMessage("Cancel");
            }

            PromptKeywordOptions kwopts = new PromptKeywordOptions("Enter keyword");
            kwopts.AllowNone = false;
            kwopts.Keywords.Add("First");
            kwopts.Keywords.Add("Second");
            kwopts.Keywords.Add("One_more");

            kwopts.AppendKeywordsToMessage = true;

            PromptResult kw = ed.GetKeywords(kwopts);
            if (PromptStatus.Keyword == kw.Status)
            {
                ed.WriteMessage("You entered the correct keywork. You entered: " + kw.StringResult);
            }
        }

        /// <summary>
        /// Layer examples
        /// </summary>
        [CommandMethod("Example2")]
        public void Template2()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = Platform.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PlatformDb.DatabaseServices.TransactionManager tm = db.TransactionManager;

            // Print first 10 layers to command line
            ed.WriteMessage("First 10 layers:");
            using (Transaction myT = tm.StartTransaction())
            {
                LayerTable lt = (LayerTable)tm.GetObject(db.LayerTableId, OpenMode.ForRead, false);
                LayerTableRecord ltrec;

                SymbolTableEnumerator lte = lt.GetEnumerator();
                for (int i = 0; i < 10; ++i)
                {
                    if (!lte.MoveNext())
                    {
                        break;
                    }

                    ObjectId id = (ObjectId)lte.Current;
                    ltrec = (LayerTableRecord)tm.GetObject(id, OpenMode.ForRead);
                    ed.WriteMessage(string.Format("Layer name:{0}; Layer color: {1}; Layer code:{2}", ltrec.Name, ltrec.Color.ToString(), ltrec.Description));

                    myT.Dispose();
                }
            }

            PromptStringOptions opts = new PromptStringOptions("Please enter new layer name");
            opts.AllowSpaces = true;
            PromptResult pr = ed.GetString(opts);
            if (PromptStatus.OK == pr.Status)
            {
                string newLayerName = pr.StringResult;

                // Create new layer
                using (Transaction myT = tm.StartTransaction())
                {
                    try
                    {
                        LayerTable lt = (LayerTable)tm.GetObject(db.LayerTableId, OpenMode.ForWrite, false);

                        // Check if the layer exists
                        if (!lt.Has(newLayerName))
                        {
                            LayerTableRecord ltrec = new LayerTableRecord();
                            ltrec.Name = newLayerName;
                            lt.Add(ltrec);
                            tm.AddNewlyCreatedDBObject(ltrec, true);
                            myT.Commit();
                        }
                    }
                    finally
                    {
                        myT.Dispose();
                    }
                }
            }
            else
            {
                ed.WriteMessage("Cancel");
            }
        }
    }
}
