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

namespace Metallwork
{
    internal class Metalltechick
    {
        public Metalltechick()
        {
            Editor ed = Platform.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            // Print message to command line
            ed.WriteMessage("Hello World!");

            PromptStringOptions opts = new PromptStringOptions("Enter something");
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

          /*  PromptKeywordOptions kwopts = new PromptKeywordOptions("Enter keyword");
            kwopts.AllowNone = false;
            kwopts.Keywords.Add("First");
            kwopts.Keywords.Add("Second");
            kwopts.Keywords.Add("One_more");

            kwopts.AppendKeywordsToMessage = true;

            PromptResult kw = ed.GetKeywords(kwopts);
            if (PromptStatus.Keyword == kw.Status)
            {
                ed.WriteMessage("You entered the correct keywork. You entered: " + kw.StringResult);
            }*/
        }
    }
}