namespace Pancake.Pattern
{
    /// <summary> Command without parameters. </summary>
    public class Command : ICommand
    {
        protected ICommandReceiver Receiver { get; set; }

        /// <summary> Constructor. </summary>
        /// <param name="receiver">Command receiver</param>
        public Command(ICommandReceiver receiver) => Receiver = receiver;

        /// <summary> Execute the command. </summary>
        /// <returns>True if the execution was successful.</returns>
        public bool OnExecute() => Receiver.DoAction();

        /// <summary> Undo the changes of OnExecute. </summary>
        public void OnUndo() => Receiver.UndoAction();
    }

    /// <summary> Command with one parameter. </summary>
    public class Command<T> : ICommand
    {
        /// <summary> Command' parameter. </summary>
        /// <value>T</value>
        public T Value { get; set; }

        protected ICommandReceiver<T> Receiver { get; set; }

        /// <summary> Constructor. </summary>
        /// <param name="receiver">Command receiver</param>
        public Command(ICommandReceiver<T> receiver) => Receiver = receiver;

        /// <summary> Constructor. </summary>
        /// <param name="receiver">Command receiver</param>
        /// <param name="value">Command' parameter.</param>
        public Command(ICommandReceiver<T> receiver, T value)
        {
            Receiver = receiver;
            Value = value;
        }

        /// <summary> Execute the command. </summary>
        /// <returns>True if the execution was successful.</returns>
        public bool OnExecute() => Receiver.DoAction(Value);

        /// <summary> Undo the changes of OnExecute. </summary>
        public void OnUndo() => Receiver.UndoAction();
    }

    /// <summary> Command with two parameters. </summary>
    public class Command<T0, T1> : ICommand
    {
        /// <summary> First parameter. </summary>
        /// <value>T</value>
        public T0 Value0 { get; set; }

        /// <summary> Second parameter. </summary>
        /// <value>T</value>
        public T1 Value1 { get; set; }

        protected ICommandReceiver<T0, T1> Receiver { get; set; }

        /// <summary> Constructor. </summary>
        /// <param name="receiver">Command receiver</param>
        public Command(ICommandReceiver<T0, T1> receiver) => Receiver = receiver;

        /// <summary> Constructor. </summary>
        /// <param name="receiver">Command receiver</param>
        /// <param name="value0">First parameter</param>
        /// <param name="value1">Second parameter</param>
        public Command(ICommandReceiver<T0, T1> receiver, T0 value0, T1 value1)
        {
            Receiver = receiver;
            Value0 = value0;
            Value1 = value1;
        }

        /// <summary> Execute the command. </summary>
        /// <returns>True if the execution was successful.</returns>
        public bool OnExecute() => Receiver.DoAction(Value0, Value1);

        /// <summary> Undo the changes of OnExecute. </summary>
        public void OnUndo() => Receiver.UndoAction();
    }

    /// <summary> Command with three parameters. </summary>
    public class Command<T0, T1, T2> : ICommand
    {
        /// <summary> First parameter. </summary>
        /// <value>T</value>
        public T0 Value0 { get; set; }

        /// <summary> Second parameter. </summary>
        /// <value>T</value>
        public T1 Value1 { get; set; }

        /// <summary> Third parameter. </summary>
        /// <value>T</value>
        public T2 Value2 { get; set; }

        protected ICommandReceiver<T0, T1, T2> Receiver { get; set; }

        /// <summary> Constructor. </summary>
        /// <param name="receiver">Command receiver</param>
        public Command(ICommandReceiver<T0, T1, T2> receiver) => Receiver = receiver;

        /// <summary> Constructor. </summary>
        /// <param name="receiver">Command receiver</param>
        /// <param name="value0">First parameter</param>
        /// <param name="value1">Second parameter</param>
        /// <param name="value2">Third parameter</param>
        public Command(ICommandReceiver<T0, T1, T2> receiver, T0 value0, T1 value1, T2 value2)
        {
            Receiver = receiver;
            Value0 = value0;
            Value1 = value1;
            Value2 = value2;
        }

        /// <summary> Execute the command. </summary>
        /// <returns>True if the execution was successful.</returns>
        public bool OnExecute() => Receiver.DoAction(Value0, Value1, Value2);

        /// <summary> Undo the changes of OnExecute. </summary>
        public void OnUndo() => Receiver.UndoAction();
    }
}