/**********************************************
Samuel Chen
10728476
**********************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using CptS321;
using System.IO;
using System.Xml.Linq;

namespace SpreadsheetEngine
{
    public class SpreadCell : Cell
    {
        public SpreadCell(int col, int row) : base(col, row) { }

    }

    public class Spreadsheet
    {
        private UndoRedo _UndoRedo;

        private Cell[,] array;
        private Dictionary<Cell, HashSet<Cell>> dependencyHash;

        /*************************************************************
        Spreadsheet Constructor
        *************************************************************/
        public Spreadsheet(int colLength, int rowLength)
        {
            _UndoRedo = new UndoRedo();
            array = new SpreadCell[colLength, rowLength];
            dependencyHash = new Dictionary<Cell, HashSet<Cell>>();



            for (int i = 0; i < colLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    array[i, j] = new SpreadCell(i, j);
                    array[i, j].PropertyChanged += CellChanged;     // we are subscribing all cells to spreadsheet PropertyChanged
                    dependencyHash.Add(array[i, j], new HashSet<Cell>());
                }
            }
        }

        /*************************************************************
        Undo helper functions cause UI can't access the undo and redo stacks directly
        *************************************************************/
        public void UndoStackPop_spread()
        {
            if (_UndoRedo.UndoStackIsEmpty())
                return;
            _UndoRedo.UndoStackPop();
        }

        public void UndoStackPush_spread(ICmd cmd)
        {
            _UndoRedo.UndoStackPush(cmd);
        }

        public bool UndoStackIsEmpty()
        {
            if (_UndoRedo.UndoStackIsEmpty())
                return true;
            return false;
        }

        /*************************************************************
        Redo helper functions cause UI can't access the undo and redo stacks directly
        *************************************************************/
        public void RedoStackPop_spread()
        {
            if (_UndoRedo.RedoStackIsEmpty())
                return;
            _UndoRedo.RedoStackPop();
        }

        public bool RedoStackIsEmpty()
        {
            if (_UndoRedo.RedoStackIsEmpty())
                return true;
            return false;
        }



        public PropertyChangedEventHandler CellPropertyChanged;     // notifies the UI if anything gets changed

        /*************************************************************
        letting spreadsheet know that text has changed.
        *************************************************************/
        public void CellChanged(object sender, PropertyChangedEventArgs e)
        {
            Cell tempCell = sender as Cell;
            if (e.PropertyName == "Text")
            {
                this.evaluate(tempCell.indexCol, tempCell.indexRow);
                CellPropertyChanged(sender, new PropertyChangedEventArgs("Values"));
            }
            else if (e.PropertyName == "BGColor")
            {
                CellPropertyChanged(sender, new PropertyChangedEventArgs("BGColor"));
            }
        }

        /*************************************************************
        this is the heart of trying to determine if the value shoul be copied from the text of teh same cell, OR, ir the 
        cell needs to find the value of a target cell.
        *************************************************************/
        public void evaluate(int colIndex, int rowIndex)
        {
            if (array[colIndex, rowIndex].text == "")       // this is do guard if the UndoRedo option reverts cell back to empty text
            {
                array[colIndex, rowIndex].value = "";
                array[colIndex, rowIndex].CellUses.Clear();
                UpdateTableDelete(array[colIndex, rowIndex]);
                CellPropertyChanged(array[colIndex, rowIndex], new PropertyChangedEventArgs("Values")); // letting the spreadsheet know that a cell changed...in values.
            }
            else if (array[colIndex, rowIndex].text[0] == '=')
            {
                string tempText = array[colIndex, rowIndex].text;
                tempText.Replace(" ", "");

                // creating dependency tree and expression tree
                string[] arrString = tempText.Split(new Char[] { '=', ' ', '(', ')', '+', '-', '*', '/' }, StringSplitOptions.RemoveEmptyEntries);
                HashSet<Cell> cellsInText = FindCellsInText(arrString);

                Cell targetCell = GetCell(colIndex, rowIndex);
                ExpTree tree = new ExpTree(tempText);

                UpdateCell(targetCell, cellsInText);
            }
            else
            {
                array[colIndex, rowIndex].value = array[colIndex, rowIndex].text;
                // UpdateCell(array[colIndex, rowIndex], array[colIndex, rowIndex].CellUses);
                var cellsInText = new HashSet<Cell>();
                UpdateCell(array[colIndex, rowIndex], cellsInText);
            }
            CellPropertyChanged(array[colIndex, rowIndex], new PropertyChangedEventArgs("Values")); // letting the spreadsheet know that a cell changed...in values.
        }


        HashSet<Cell> FindCellsInText(string[] arrString)
        {
            double dummyDouble = 0;

            HashSet<Cell> cellsInText = new HashSet<Cell>();

            foreach (var element in arrString)
            {
                if (!double.TryParse(element, out dummyDouble))
                {
                    if (double.TryParse(element.Substring(1), out dummyDouble))
                    {
                        cellsInText.Add(FindCell(element));     // create a list of cells found in given text
                    }
                }
            }
            return cellsInText;

        }

        /*************************************************************
        // this is a public function to find the target cell in spreadsheet.  called from UI.
        *************************************************************/
        public Cell GetCell(int colIndex, int rowIndex)
        {
            return array[colIndex, rowIndex];
        }

        /*************************************************************
        given text (we assume a string contains either a cell reference, number, or string.
        This finds the cell of given cell reference (which is a string)
        *************************************************************/
        public Cell FindCell(string strCell)
        {
            // defending against  alower case cell entry
            string tempString = strCell;
            if (tempString.Any(char.IsLower))
            {
                tempString = tempString.ToUpper();
            }

            // get row and col
            int targetCol = tempString[0] - 'A';          // we made the asssumption that the number of columns is 1 character
            int targetRow = 0;
            Int32.TryParse(strCell.Substring(1), out targetRow);

            return GetCell(targetCol, targetRow - 1);     // -1 here because we lowest row is 1, BUT, our array lowest row is at 0!!!
        }

        /*************************************************************
        does all updates for cell and recursively call dependencies
        *************************************************************/
        public void UpdateCell(Cell updateThisCell, HashSet<Cell> cellsInText)
        {
            UpdateTableDelete(updateThisCell);
            UpdateTableAdd(updateThisCell, cellsInText);
            UpdateCellsFoundInTextProperty(updateThisCell, cellsInText);
            if (isBadRef(updateThisCell))
            {
                CellPropertyChanged(updateThisCell, new PropertyChangedEventArgs("Values")); // letting the spreadsheet know that a cell changed...in values.
            }
            else if (isSelfReference(updateThisCell))
            {
                CellPropertyChanged(updateThisCell, new PropertyChangedEventArgs("Values")); // letting the spreadsheet know that a cell changed...in values.
            }
            else if (isCircularReference(updateThisCell))
            {
                CellPropertyChanged(updateThisCell, new PropertyChangedEventArgs("Values")); // letting the spreadsheet know that a cell changed...in values.
            }
            else
            {
                UpdateCellValue(updateThisCell, cellsInText);
                CellPropertyChanged(updateThisCell, new PropertyChangedEventArgs("Values")); // letting the spreadsheet know that a cell changed...in values.

                var prevVars = new HashSet<string>();
                foreach (Cell dependency in dependencyHash[updateThisCell])
                {
                    prevVars.Add(getCellName(dependency));
                }

                foreach (string stringDependency in prevVars) // update dependencies
                {
                    Cell dependency = FindCell(stringDependency);
                    UpdateCell(dependency, dependency.CellUses);
                }
            }
        }



        private bool isCircularReference(Cell updateThisCell)
        {
            bool test = false;
            foreach (var element in dependencyHash[updateThisCell])
            {
                //if (element == updateThisCell)
                //{
                //    test = true;
                //    break;
                //}
                test = isCircularReferenceHelper(updateThisCell, element);
                if (test == true)
                {
                    updateThisCell.value = "!Circular Reference!";
                    break;
                }
            }
            return test;
        }

        private bool isCircularReferenceHelper(Cell initialCell, Cell cellInDependencyTable)
        {
            bool test = false;

            foreach (var element in dependencyHash[cellInDependencyTable])
            {
                if (element == initialCell)
                {
                    test = true;
                    break;
                }
                test = isCircularReferenceHelper(initialCell, element);
                if (test == true)
                    break;
            }
            return test;
        }


        //private bool isCircularReferenceHelper(Cell updateThisCell, HashSet<Cell> parent)
        //{
        //    bool test = false;
        //    if (parent.Contains(updateThisCell))
        //    {
        //        updateThisCell.value = "!Has circular reference.";
        //        return true;
        //    }

        //    parent.Add(updateThisCell);

        //    foreach (var element in dependencyHash[updateThisCell])
        //    {
        //        test = isCircularReferenceHelper(element, parent);
        //        if (test == true)
        //            return true;
        //    }
        //    return test;
        //}

        private bool isSelfReference(Cell curCell)
        {
            foreach (Cell element in curCell.CellUses)
            {
                if (element == curCell)
                {
                    curCell.value = "!Self Reference!";
                    return true;
                }
            }
            return false;
        }
        private bool isBadRef(Cell updateThisCell)
        {
            string[] arrString = updateThisCell.text.Split(new Char[] { '=', ' ', '(', ')', '+', '-', '*', '/' }, StringSplitOptions.RemoveEmptyEntries);
            char dummyChar = '\0';
            double dummyDouble = 0;
            int dummyInt = 0;

            foreach (var element in arrString)
            {
                if (!double.TryParse(element, out dummyDouble))
                {
                    if (char.TryParse(element.Substring(0, 1), out dummyChar))
                        if (int.TryParse(element.Substring(1), out dummyInt))
                        {
                            if (dummyInt < 0 || dummyInt > 50)
                            {
                                updateThisCell.value = "!BadReference";
                                return true;
                            }
                        }
                        else
                        {
                            updateThisCell.value = "!BadReference";
                            return true;
                        }
                }
            }
            return false;
        }

        private void UpdateCellsFoundInTextProperty(Cell updateThisCell, HashSet<Cell> cellsInText)
        {             // rplacing the list (CellUse) with new list (cellsInText)
            //if (updateThisCell.CellUses == null)
            //    updateThisCell.CellUses = new HashSet<Cell>();
            updateThisCell.CellUses.Clear();

            if (cellsInText != null)
            {
                foreach (var moo in cellsInText)
                    updateThisCell.CellUses.Add(moo);
            }
        }
        /************************************************************* 
        creating expression tree and updating value of cell and dependencies
        *************************************************************/
        private void UpdateCellValue(Cell updateThisCell, HashSet<Cell> cellsInText)
        {

            // constructing eval tree and evaluating
            string[] arrString = updateThisCell.text.Split(new Char[] { '=', ' ', '(', ')', '+', '-', '*', '/' }, StringSplitOptions.RemoveEmptyEntries);
            ExpTree tree = new ExpTree(updateThisCell.text);        // expression tree created

            Cell targetCell = null;
            double dummyDouble = 0;

            foreach (var element in arrString)
            {
                if (!double.TryParse(element, out dummyDouble))
                {
                    targetCell = FindCell(element);
                    double.TryParse(targetCell.value, out dummyDouble);
                    tree.SetVar(element, dummyDouble);         // creating lookup dictionary for expression tree
                }
                updateThisCell.value = tree.Eval().ToString();
            }
        }

        /************************************************************* 
       updating dependency table by adding new dependencies
       *************************************************************/
        private void UpdateTableAdd(Cell updateThisCell, HashSet<Cell> cellsInText)
        {
            if (cellsInText != null)
            {
                foreach (var element in cellsInText)
                {
                    if (!dependencyHash[element].Contains(updateThisCell))
                    {
                        dependencyHash[element].Add(updateThisCell);
                    }
                }
            }
        }
        /************************************************************* 
        updating dependency table by deleting the old dependencies
        *************************************************************/
        private void UpdateTableDelete(Cell curCell)
        {
            foreach (Cell element in curCell.CellUses)
            {
                if (dependencyHash[element].Contains(curCell))
                    dependencyHash[element].Remove(curCell);
            }
            //foreach (Cell element in curCell.CellUses)
            //{
            //    if (dependencyHash[curCell].Contains(element))
            //        dependencyHash[curCell].Remove(element);
            //}
        }

        private bool IsDefaultCell(Cell saveCell)
        {
            if ((saveCell.BGColor != 4294967295) || (saveCell.text != ""))
                return false;

            return true;
        }


        /************************************************************* 
        given a cell, return the name.  i.e. array[1][2] = B3        
        *************************************************************/
        public string getCellName(Cell curCell)
        {
            string col = ((char)('A' + curCell.indexCol)).ToString();
            string row = (curCell.indexRow + 1).ToString();
            return col + row;
        }

        public void ClearSpreadsheet()
        {
            _UndoRedo.RedoStackClear();
            _UndoRedo.UndoStackClear();

            foreach (var cell in array)
            {
                cell.BGColor = 4294967295;
                cell.text = "";
                cell.value = "";
            }
        }



        /************************************************************* 
        saving spreadsheet.  using xdocuments because has predefined
        use of attrivutes
        *************************************************************/
        public void SaveFile_spread(Stream streamToSave)
        {
            XDocument writer = new XDocument();
            writer.Add(new XElement("Spreadsheet"));
            writer.Element("Spreadsheet").Add(new XElement("Cells"));

            foreach (Cell saveCell in array)
            {
                if (!IsDefaultCell(saveCell))           // checks if value changed
                    writer.Root.Element("Cells").Add(new XElement("Cell", new XAttribute("Col", saveCell.indexCol), new XAttribute("Row", saveCell.indexRow),
                                                new XElement("Text", saveCell.text),
                                                new XElement("BGColor", saveCell.BGColor),
                                                new XElement("Value", saveCell.value)));
            }

            writer.Save(streamToSave);
        }

        /************************************************************* 
        clears the current spreadsheet and loads the file (given as a 
        stream) and loads and updates the cells
        *************************************************************/
        public void LoadFile_spread(Stream streamFromFile)
        {
            ClearSpreadsheet();

            TextReader sr = new StringReader("<Spreadsheet> </Spreadsheet> <Cells> </Cells>");
            XDocument doc = XDocument.Load(streamFromFile);

            var curCell = from cell in doc.Element("Spreadsheet").Elements("Cells").Descendants("Cell")  // curCell is a list of all cells found in doc
                          select new                                                                    // constructor
                          {
                              Col = cell.Attribute("Col").Value,
                              Row = cell.Attribute("Row").Value,
                              Text = cell.Element("Text").Value,
                              BGColor = cell.Element("BGColor").Value,
                              Value = cell.Element("Value").Value
                          };
            foreach (var cell in curCell)
            {
                array[int.Parse(cell.Col), int.Parse(cell.Row)].BGColor = uint.Parse(cell.BGColor);
                array[int.Parse(cell.Col), int.Parse(cell.Row)].text = cell.Text;
                array[int.Parse(cell.Col), int.Parse(cell.Row)].value = cell.Value;
            }
        }
    }
}




/************************************************************* 

*************************************************************/
