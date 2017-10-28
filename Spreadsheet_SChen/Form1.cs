/**********************************
Samuel Chen
10728476
CS 360 
HW 4
**********************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpreadsheetEngine;
using System.IO;

namespace Spreadsheet_SChen
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static int numCol = 26;
        static int numRow = 50;
        Spreadsheet daSpreadsheet = new Spreadsheet(numCol, numRow);

        private void Form1_Load(object sender, EventArgs e)
        {

            char c1 = 'A';

            daSpreadsheet.CellPropertyChanged += Spreadsheet_CellChanged;

            // assuming thatwe're only going from A-Z, so i just incremented that char and attached it column header.
            for (int i = 0; i < numCol; i++)
            {
                dataGridView1.Columns.Add(c1.ToString(), c1.ToString());
                c1++;
            }

            // adding rows.
            for (int i = 0; i < numRow; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
            dataGridView1.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            redoToolStripMenuItem.Enabled = false;
            undoToolStripMenuItem.Enabled = false;
        }

        //signalluing a value changed.
        public void Spreadsheet_CellChanged(object sender, PropertyChangedEventArgs e)
        {
            Cell tempCell = sender as Cell;
            switch (e.PropertyName)
            {
                case "Values":
                    dataGridView1[tempCell.indexCol, tempCell.indexRow].Value = tempCell.value;   // WTF MATE!  THE REST OF THE WORLD USES ROW COLUMN....not FREAKING COLUMN ROW
                    break;
                case "BGColor":
                    Cell c = daSpreadsheet.GetCell(tempCell.indexCol, tempCell.indexRow);
                    DataGridViewCell d = dataGridView1[tempCell.indexCol, tempCell.indexRow];
                    d.Style.BackColor = Color.FromArgb((int)c.BGColor);
                    break;
                default:
                    break;
            }
            if (daSpreadsheet.UndoStackIsEmpty())
                undoToolStripMenuItem.Enabled = false;
            else
                undoToolStripMenuItem.Enabled = true;

            if (daSpreadsheet.RedoStackIsEmpty())
                redoToolStripMenuItem.Enabled = false;
            else
                redoToolStripMenuItem.Enabled = true;

        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            dataGridView1[e.ColumnIndex, e.RowIndex].Value = daSpreadsheet.GetCell(e.ColumnIndex, e.RowIndex).text;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var tempMultiCmd = new MultiCmd();

            if (dataGridView1[e.ColumnIndex, e.RowIndex].Value != null)
            {
                daSpreadsheet.UndoStackPush_spread(new RestoreText(daSpreadsheet.GetCell(e.ColumnIndex, e.RowIndex), daSpreadsheet.GetCell(e.ColumnIndex, e.RowIndex).text));
                daSpreadsheet.GetCell(e.ColumnIndex, e.RowIndex).text = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
                dataGridView1[e.ColumnIndex, e.RowIndex].Value = daSpreadsheet.GetCell(e.ColumnIndex, e.RowIndex).value;
            }
        }

        /************************************************************* 
        BG color change.  Note that the we update Undo and Redo stacks in
        function as it may never reach Spreadsheet_CellChanged
        *************************************************************/
        private void changeCellBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                List<Cell> cellsChanged = new List<Cell>();
                var tempMultiCmd = new MultiCmd();

                foreach (DataGridViewTextBoxCell selectedCells in dataGridView1.SelectedCells)
                {
                    tempMultiCmd.AddCmd(new RestoreBGColor(daSpreadsheet.GetCell(selectedCells.ColumnIndex, selectedCells.RowIndex), daSpreadsheet.GetCell(selectedCells.ColumnIndex, selectedCells.RowIndex).BGColor));
                    cellsChanged.Add(daSpreadsheet.GetCell(selectedCells.RowIndex, selectedCells.ColumnIndex));
                    daSpreadsheet.GetCell(selectedCells.ColumnIndex, selectedCells.RowIndex).BGColor = (uint)colorDialog1.Color.ToArgb();
                }
                daSpreadsheet.UndoStackPush_spread(tempMultiCmd);               // because we dont necessarily go throuhg the SpreadSheetCellChanged after, we need to do UI update here
                if (daSpreadsheet.RedoStackIsEmpty())
                    redoToolStripMenuItem.Enabled = false;
            }
        }

        /************************************************************* 
        // In charge of UI undo display.
        *************************************************************/
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            daSpreadsheet.UndoStackPop_spread();
            redoToolStripMenuItem.Enabled = true;
        }

        /************************************************************* 
        // In charge of UI redo display.
        *************************************************************/
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            daSpreadsheet.RedoStackPop_spread();
            undoToolStripMenuItem.Enabled = true;
        }

        /************************************************************* 
        // In charge of UI save display.
        *************************************************************/
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            daSpreadsheet.SaveFile_spread(saveFileDialog1.OpenFile());
        }
        /************************************************************* 
        // In charge of UI load display.
        *************************************************************/
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                daSpreadsheet.LoadFile_spread(openFileDialog1.OpenFile());
            }
        }
    }
}