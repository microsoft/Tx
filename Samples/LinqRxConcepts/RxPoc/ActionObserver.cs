using System;

namespace System.Reactive
{
    class ActionObserver<T> : IObserver<T>
    {
        Action<T> _action;
        public ActionObserver(Action<T> action)
        {
            _action = action;
        }
        public void OnNext(T value)
        {
            _action(value);
        }

        public void OnCompleted()
        {
            ;
        }

        public void OnError(Exception error)
        {
            ConsoleColor f = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red; ;
            Console.WriteLine(error.Message);
            Console.ForegroundColor = f;

            throw (error);
        }
    }
}
