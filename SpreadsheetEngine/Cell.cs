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

namespace SpreadsheetEngine
{
    public class Class1
    {
    }

    public abstract class Cell
    {
        private uint _BGColor;
        private readonly int _indexCol;
        private readonly int _indexRow;
        protected string _text;
        protected string _value;
        public event PropertyChangedEventHandler PropertyChanged;
        public HashSet<Cell> CellUses;

        /************************************************************* 
        Cell Constructor
        *************************************************************/
        public Cell(int indexCol, int indexRow)
        {
            _BGColor = 4294967295;
            _indexRow = indexRow;
            _indexCol = indexCol;
            _text = "";         // set to null because we're goign to be testing against this value
            _value = "";
            CellUses = new HashSet<Cell>();
        }

        public uint BGColor
        {
            get
            {
                return _BGColor;
            }
            set
            {
                if (value != _BGColor)
                {
                    _BGColor = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("BGColor"));
                }
            }
        }


        // no rowIndex or colIndex setters
        public int indexCol
        {
            get
            {
                return _indexCol;
            }
        }
        public int indexRow
        {
            get
            {
                return _indexRow;
            }
        }
        // text is what one types in
        public string text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != "")
                    _text = _text.Replace(" ", ""); // removing spaces

                if (value != _text)
                {
                    _text = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                }
            }
        }
        // what i sdisplayed.  only really matters is there is a ex "=A2"..which displays the VALUE of A2
        public string value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }
}