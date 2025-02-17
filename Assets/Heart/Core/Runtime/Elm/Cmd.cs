using System;

namespace Pancake.Elm
{
    public readonly struct Cmd<T> where T : struct
    {
        private readonly Action<Dispatcher<T>> _task;
        public Cmd(Action<Dispatcher<T>> task) { _task = task; }

        public static Cmd<T> none = new(_ => { });

        public static Cmd<T> Batch(Cmd<T>[] cmds)
        {
            return new Cmd<T>(d =>
            {
                foreach (var cmd in cmds)
                {
                    cmd.Execute(d);
                }
            });
        }

        public void Execute(Dispatcher<T> d) => _task(d);
    }
}