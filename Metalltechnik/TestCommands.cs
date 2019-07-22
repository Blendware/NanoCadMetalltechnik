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

namespace Multicad.Samples.Symbols
{
    using System;
    using Multicad.DatabaseServices;
    using Multicad.Geometry;
    using Multicad.Runtime;
    using Multicad.Symbols;
    using Multicad.Symbols.Tables;

    public class TestCommands : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }

        [CommandMethod("testTable", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
        public void testTable()
        {
            McTable table = new McTable();

            table.Columns.AddRange(0, 10);
            foreach (McTable.Column col in table.Columns)
            {
                col.Width = 50;
            }

            table.Rows.AddRange(0, 10);
            foreach (McTable.Row row in table.Rows)
            {
                row.Height = 40;
                foreach (Cell cell in row.Cells)
                {
                    cell.Value = "test";
                }
            }

            int cnt = table.Columns.Count;
            table.Rows.InsertSection(SectionTypeEnum.Footer, 1);
            table.Rows[1].Cells[1].Value = "test";
            table.Columns[2].Width = 32;
            table.Columns[3].Hidden = false;
            table[0, 0].TextXScale = 12;
            table[0, 0].Value = "test";
            table[1, 1].TextColor = System.Drawing.Color.AliceBlue;
            table.Pages.Height = 50;
            table.Rows[1].Cells[5].FillColor = System.Drawing.Color.Aqua;
            table.DefaultCell.TextColor = System.Drawing.Color.Coral;
            table[1, 0].Value = "test";
            table.DefaultCell.VerticalTextAlign = VertTextAlign.Center;

            System.Drawing.Rectangle rct;
            rct = table[0, 0].Rect;
            System.Drawing.Rectangle rct2;
            rct2 = table[5, 5].Rect;
            rct.Width = rct2.Right - rct.Left;
            rct.Height = rct2.Bottom - rct.Top;
            table.Merge(rct);

            table[0, 0].TextAngle = 90;
            table[0, 0].ValueComment = "comment";
            table.Columns.Move(1, 4);
            table.Rows.Delete(5);

            Cell currCell = table.GetCellFromPointECS(new Point3d(600, 400, 0));
            if (currCell != null)
            {
                int rowCurrCell = currCell.RowIndex;
                int colCurrCell = currCell.ColumnIndex;
                currCell.Value = currCell.Value + Convert.ToString(rowCurrCell) + Convert.ToString(colCurrCell);
            }

            table.PlaceObject();
            currCell = table.GetCellFromPoint(new Point3d(600, 400, 0));
        }

        [CommandMethod("testSymbols", CommandFlags.NoCheck | CommandFlags.NoPrefix)]
        public void testsymbols()
        {
            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d(100, 50, 0);
            Point3d p3 = new Point3d(200, 200, 0);

            // Note
            McNote note = new McNote();
            note.Origin = p3;
            McBLSegment segment = note.Leader.AddSegment(p2, p3);
            McBLSegment segment2 = segment.AddSegment(p1, p2);
            segment2.Arrow = Arrows.Arrow;
            note.Items.Add("above_text", 0);
            note.Items.Add("below_text", 0);
            note.PlaceObject(McEntity.PlaceFlags.Silent);

            // Note
            McNote note2 = new McNote();
            note2.Origin = p3;
            McBLSegment note2segment = note2.Leader.AddSegment(p2, p3);
            McBLSegment note2segment2 = note2segment.AddSegment(p1, p2);
            note2segment2.Arrow = Arrows.Arrow;
            note2.Items.Add("above_text", 0);
            note2.Items.Add("below_text", 0);
            note2.PlaceObject(McEntity.PlaceFlags.Silent);

            // NoteComb
            McNoteComb noteComb = new McNoteComb();
            noteComb.Angle = 45;
            noteComb.LineAngleStep = 10;
            noteComb.TextSize = 12;
            noteComb.Align = HorizTextAlign.Center;
            noteComb.PlaceObject(McEntity.PlaceFlags.Wout_Position | McEntity.PlaceFlags.Silent);

            // NoteChain
            McNoteChain noteChain = new McNoteChain();
            noteChain.Angle = 30;
            noteChain.Arrow = Arrows.Dotsmall;
            noteChain.FirstLine = "test";
            noteChain.SecondLine = "test2";
            noteChain.PlaceObject(McEntity.PlaceFlags.Wout_Position | McEntity.PlaceFlags.Silent);

            // NoteSecant
            McNoteSecant noteSecant = new McNoteSecant();
            noteSecant.Note = "test";
            noteSecant.Sheet = "1";
            noteSecant.SingleStroke = true;
            noteSecant.Knot = "test3";
            noteSecant.PlaceObject(McEntity.PlaceFlags.Wout_Position | McEntity.PlaceFlags.Silent);

            // NoteKnot
            McNoteKnot noteKnot = new McNoteKnot();
            noteKnot.Center = new Point3d(50, 50, 0);
            noteKnot.DAngl = 30;
            noteKnot.TextSize = 12;
            noteKnot.XRadius = 30;
            noteKnot.YRadius = 80;
            noteKnot.OvaloRotate = 20;
            noteKnot.Sheet = "test";
            noteKnot.Knot = "test2";
            noteKnot.Note = "test3";
            noteKnot.PlaceObject(McEntity.PlaceFlags.Wout_Position | McEntity.PlaceFlags.Silent);

            // NotePosition
            McNotePosition notePosition = new McNotePosition();
            notePosition.FirstLine = "test";
            notePosition.SecondLine = "test2";
            notePosition.LeaderAngleStep = 0;
            notePosition.Align = HorizTextAlign.Center;
            notePosition.Origin = new Point3d(50, 50, 0);
            notePosition.PlaceObject(McEntity.PlaceFlags.Wout_Position | McEntity.PlaceFlags.Silent);

            // NoteMultilayer
            McNoteMultilayer noteMultilayer = new McNoteMultilayer();
            noteMultilayer.Units.Add("test1", HorizTextAlign.Center);
            noteMultilayer.Units.Add("test2", HorizTextAlign.Center);
            noteMultilayer.Units.Add("test3", HorizTextAlign.Center);
            noteMultilayer.Units.Add("test4", HorizTextAlign.Center);
            noteMultilayer.Units.Add("test5", HorizTextAlign.Center);
            noteMultilayer.IsVerticalLine = true;
            noteMultilayer.IsOnUpCorner = true;
            noteMultilayer.PlaceObject(McEntity.PlaceFlags.Wout_Position | McEntity.PlaceFlags.Silent);
        }
    }
}
