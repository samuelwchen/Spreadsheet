using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetEngine
{
    /************************************************************* 
    Will be constructed in Spreadsheet.cs.  Commands should be self
    explanatory.
    *************************************************************/
    public class UndoRedo
    {
        private Stack<ICmd> _undoStack;
        private Stack<ICmd> _redoStack;

        public UndoRedo()
        {
            _undoStack = new Stack<ICmd>();
            _redoStack = new Stack<ICmd>();
        }

        public bool UndoStackIsEmpty()
        {
            if (_undoStack.Count() == 0)
                return true;
            return false;
        }

        public bool RedoStackIsEmpty()
        {
            if (_redoStack.Count() == 0)
                return true;
            return false;
        }

        public void UndoStackPop()
        {
            ICmd tempCmd = _undoStack.Pop();
            tempCmd = tempCmd.Exec();
            _redoStack.Push(tempCmd);
        }

        public void UndoStackPush(ICmd cmd)
        {
            _undoStack.Push(cmd);
            _redoStack.Clear();
        }

        public void RedoStackPop()
        {
            ICmd tempCmd = _redoStack.Pop();
            tempCmd = tempCmd.Exec();
            _undoStack.Push(tempCmd);
        }

        public void RedoStackClear()
        {
            _redoStack.Clear();
        }


        public void UndoStackClear()
        {
            _undoStack.Clear();
        }
    }
}
