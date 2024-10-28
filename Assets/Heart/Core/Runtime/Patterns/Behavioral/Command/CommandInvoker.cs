using System.Collections.Generic;

namespace Pancake.Pattern
{
    /// <summary> Asks the command to carry out the request. </summary>
    public class CommandInvoker
    {
        private readonly Stack<ICommand> _undo = new();
        private readonly Stack<ICommand> _redo = new();

        /// <summary> Execute a command and add it to the undo redo stack if success. </summary>
        /// <param name="command"></param>
        /// <returns>True if the execution was successful.</returns>
        public bool Execute(ICommand command)
        {
            if (command.OnExecute())
            {
                _undo.Push(command);
                _redo.Clear();

                return true;
            }

            return false;
        }

        /// <summary> Reverse the last command executed. </summary>
        public void Undo()
        {
            if (_undo.Count > 0)
            {
                ICommand executed = _undo.Pop();
                executed.OnUndo();
                _redo.Push(executed);
            }
            else UnityEngine.Debug.LogWarning("Undo stack empty");
        }

        /// <summary> Replay the last undone command. </summary>
        public void Redo()
        {
            if (_redo.Count > 0)
            {
                ICommand undone = _redo.Pop();
                undone.OnExecute();
                _undo.Push(undone);
            }
            else UnityEngine.Debug.LogWarning("Redo stack empty");
        }
    }
}