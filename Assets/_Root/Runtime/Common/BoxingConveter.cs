using System;
using System.Linq.Expressions;

namespace Pancake
{
    public sealed class BoxingConveter<TIn, TOut>
    {
        public static readonly BoxingConveter<TIn, TOut> Instance = new BoxingConveter<TIn, TOut>();

        public Func<TIn, TOut> Convert { get; }

        private BoxingConveter()
        {
            if (typeof(TIn) != typeof(TOut))
            {
                throw new InvalidOperationException("Both generic type parameters must represent the same type.");
            }

            var paramExpr = Expression.Parameter(typeof(TIn));
            Convert = Expression.Lambda<Func<TIn, TOut>>(paramExpr, // this conversion is legal as typeof(TIn) = typeof(TOut)
                    paramExpr)
                .Compile();
        }
    }
}