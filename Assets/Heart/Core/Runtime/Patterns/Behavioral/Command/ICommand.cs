namespace Pancake.Pattern
{
    /// <summary> Interface for executing an operation. </summary>
    public interface ICommand
    {
        /// <summary> Execute the command. </summary>
        /// <returns>True if the execution was successful.</returns>
        bool OnExecute();

        /// <summary> Undo the changes of OnExecute. </summary>
        void OnUndo();
    }
}