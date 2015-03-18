using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFramework.Caching
{
    /// <summary>
    /// Enables cache key support for local collection values.
    /// </summary>
    internal class LocalCollectionExpander : ExpressionVisitor
    {
        public static Expression Rewrite(Expression expression)
        {
            return new LocalCollectionExpander().Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // pair the method's parameter types with its arguments
            var map = node.Method.GetParameters()
                .Zip(node.Arguments, (p, a) => new { Param = p.ParameterType, Arg = a })
                .ToLinkedList();

            // deal with instance methods
            var instanceType = node.Object == null ? null : node.Object.Type;
            map.AddFirst(new { Param = instanceType, Arg = node.Object });

            // for any local collection parameters in the method, make a
            // replacement argument which will print its elements
            var replacements = (from x in map
                                where x.Param != null && x.Param.IsGenericType
                                let g = x.Param.GetGenericTypeDefinition()
                                where g == typeof(IEnumerable<>) || g == typeof(List<>)
                                where x.Arg.NodeType == ExpressionType.Constant
                                let elementType = x.Param.GetGenericArguments().First()
                                let printer = MakePrinter((ConstantExpression)x.Arg, elementType)
                                select new { x.Arg, Replacement = printer }).ToList();

            if (replacements.Any())
            {
                var args = map.Select(x => (from r in replacements
                                            where r.Arg == x.Arg
                                            select r.Replacement).FirstOrDefault() ?? x.Arg).ToList();

                node = node.Update(args.First(), args.Skip(1));
            }

            return base.VisitMethodCall(node);
        }

        ConstantExpression MakePrinter(ConstantExpression enumerable, Type elementType)
        {
            var value = (IEnumerable)enumerable.Value;
            var printerType = typeof(Printer<>).MakeGenericType(elementType);
            var printer = Activator.CreateInstance(printerType, value);

            return Expression.Constant(printer);
        }

        /// <summary>
        /// Overrides ToString to print each element of a collection.
        /// </summary>
        /// <remarks>
        /// Inherits List in order to support List.Contains instance method as well
        /// as standard Enumerable.Contains/Any extension methods.
        /// </remarks>
        class Printer<T> : List<T>
        {
            public Printer(IEnumerable collection)
            {
                this.AddRange(collection.Cast<T>());
            }

            public override string ToString()
            {
                return "{" + this.ToConcatenatedString(t => t.ToString(), "|") + "}";
            }
        }
    }
}