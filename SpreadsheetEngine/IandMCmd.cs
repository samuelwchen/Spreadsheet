using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SpreadsheetEngine
{
    /************************************************************* 
    Note that MultiCmd (multiple commands) inherits from ICmd 
    (individual commands).  Here I what I passed to undo/redo stack
    to ICmd if it was a single cell changed, and MultiCmd if multiple
    cells were changed
    *************************************************************/
    public interface ICmd
    {
        ICmd Exec();
    }

    public class MultiCmd : ICmd
    {
        private List<ICmd> ICmdList;

        public MultiCmd()
        {
            ICmdList = new List<ICmd>();
        }

        public void AddCmd(ICmd newCmd)
        {
            ICmdList.Add(newCmd);
        }

        public ICmd Exec()
        {
            var undoList = new MultiCmd();
            for (int i = ICmdList.Count - 1; i >= 0; i--)
            {
                undoList.ICmdList.Add(ICmdList[i].Exec());
            }
            return undoList;
        }
    }

    /************************************************************* 
    RestoreText shoud use ICmd.  should not have to use MultiCmd
    *************************************************************/
    public class RestoreText : ICmd
    {
        private string _oldString;
        private Cell _cellReference;

        public RestoreText(Cell cell, string oldText)
        {
            _oldString = oldText;
            _cellReference = cell;
        }
        public ICmd Exec()
        {
            ICmd tempICmd = new RestoreText(_cellReference, _cellReference.text);
            _cellReference.text = _oldString;
            return tempICmd;
        }
    }

    /************************************************************* 
    Hihg probability of multiple cells selected when BG color changed.
    use MultiCmd
    *************************************************************/
    public class RestoreBGColor : ICmd
    {
        private uint _oldColor;
        private Cell _cellReference;
        public RestoreBGColor(Cell cell, uint color)
        {
            _oldColor = color;
            _cellReference = cell;
        }

        public ICmd Exec()
        {
            ICmd tempICmd = new RestoreBGColor(_cellReference, _cellReference.BGColor);
            _cellReference.BGColor = _oldColor;
            return tempICmd;
        }
    }



}



