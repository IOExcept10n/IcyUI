// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Linq.Expressions;
using System.Reflection;
using CommunityToolkit.Diagnostics;
using Icy.Data.Markup;

namespace Icy.Data.Bindings
{
    /// <summary>
    /// Represents the property path whose value is compiled for the faster execution.
    /// </summary>
    public sealed class CompiledPropertyPath : PropertyPath
    {
        private static readonly MethodInfo GetRawValueMethod = typeof(IPropertyReference).GetMethod(nameof(IPropertyReference.GetRawValue))!;
        private static readonly MethodInfo SetRawValueMethod = typeof(IPropertyReference).GetMethod(nameof(IPropertyReference.SetRawValue))!;

        private readonly Func<object, object?> getter;
        private readonly Action<object, object?>? setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompiledPropertyPath"/> class.
        /// </summary>
        /// <param name="path">The path to access an object.</param>
        /// <param name="typeContext">The type to get info about properties.</param>
        /// <param name="registry">
        /// The registry to resolve registered properties through, or <see langword="null"/> to capture
        /// <see cref="PropertyRegistry.Current"/> at construction time. See the remarks on
        /// <see cref="PropertyPath(string, Type, PropertyRegistry?)"/>.
        /// </param>
        public CompiledPropertyPath(string path, Type typeContext, PropertyRegistry? registry = null)
            : base(path, typeContext, registry)
        {
            // Each reference-typed value along the path is held in a temporary and checked for null before the next
            // member is read on it, so a null intermediate reads null and writes nothing (as PropertyPath and
            // DynamicPropertyPath do) instead of throwing. Value-typed ones can't be null, and stay inline so that a
            // write still reaches the original storage (an unboxed source is written in place).
            ParameterExpression parameter = Expression.Parameter(typeof(object));
            Expression root = typeContext.IsValueType ? Expression.Unbox(parameter, typeContext) : Expression.TypeAs(parameter, typeContext);
            PathSegment lastSegment = PathSegments[^1];

            List<ParameterExpression> getVariables = [];
            List<Expression> getBody = [];
            LabelTarget getExit = Expression.Label(typeof(object));
            Expression value = BuildChain(PathSegments, root, PathSegments.Count, getVariables, getBody, Expression.Return(getExit, Expression.Constant(null, typeof(object))));
            getBody.Add(Expression.Label(getExit, Expression.Convert(value, typeof(object))));
            getter = Expression.Lambda<Func<object, object?>>(Expression.Block(typeof(object), getVariables, getBody), parameter).Compile();

            if (!lastSegment.IsReadOnly)
            {
                ParameterExpression setParameter = Expression.Parameter(typeof(object));
                List<ParameterExpression> setVariables = [];
                List<Expression> setBody = [];
                LabelTarget setExit = Expression.Label();
                Expression exit = Expression.Return(setExit);
                Expression owner = Guard(BuildChain(PathSegments, root, PathSegments.Count - 1, setVariables, setBody, exit), setVariables, setBody, exit);
                if (lastSegment.Reference != null)
                {
                    // A reference-backed segment (e.g. a property registered through PropertyRegistry, or an
                    // attached/synthetic property) has no CLR member to assign to directly — route the value
                    // through IPropertyReference.SetRawValue so it still participates in the value-precedence
                    // system, the same way PathSegment.SetValue does for the non-compiled path.
                    setBody.Add(Expression.Call(
                        Expression.Constant(lastSegment.Reference),
                        SetRawValueMethod,
                        Expression.Convert(owner, typeof(object)),
                        setParameter));
                }
                else
                {
                    Expression uncast = lastSegment.ResultType.IsValueType ? Expression.Unbox(setParameter, lastSegment.ResultType) : Expression.TypeAs(setParameter, lastSegment.ResultType);
                    setBody.Add(Expression.Assign(GetAccessor(lastSegment, owner), uncast));
                }

                setBody.Add(Expression.Label(setExit));
                setter = Expression.Lambda<Action<object, object?>>(Expression.Block(setVariables, setBody), parameter, setParameter).Compile();
            }
        }

        /// <inheritdoc/>
        public override object? GetValue(object source)
        {
            return getter(source);
        }

        /// <inheritdoc/>
        public override void SetValue(object target, object? value)
        {
            if (setter == null)
                ThrowHelper.ThrowInvalidOperationException("Can't assign value to the readonly path segment.");
            setter(target, value);
        }

        // Reads the first `count` of `segments` starting at `root`, appending to `body` a null check (running `exit`) before
        // each member read on a reference-typed value. Returns the value the last of them reads, unchecked.
        private static Expression BuildChain(List<PathSegment> segments, Expression root, int count, List<ParameterExpression> variables, List<Expression> body, Expression exit)
        {
            Expression current = root;
            for (int i = 0; i < count; i++)
                current = GetAccessor(segments[i], Guard(current, variables, body, exit));
            return current;
        }

        // A reference-typed value goes into a temporary, and `exit` runs when it's null. A value-typed one can't be
        // null and is returned as is.
        private static Expression Guard(Expression value, List<ParameterExpression> variables, List<Expression> body, Expression exit)
        {
            if (value.Type.IsValueType)
                return value;

            ParameterExpression temporary = Expression.Variable(value.Type);
            variables.Add(temporary);
            body.Add(Expression.Assign(temporary, value));
            body.Add(Expression.IfThen(Expression.ReferenceEqual(temporary, Expression.Constant(null, value.Type)), exit));
            return temporary;
        }

        private static Expression GetAccessor(PathSegment segment, Expression argument)
        {
            if (segment.IsArray)
            {
                return Expression.ArrayAccess(argument, segment.Params!.Select(Expression.Constant));
            }
            else if (segment.Reference != null)
            {
                Expression call = Expression.Call(Expression.Constant(segment.Reference), GetRawValueMethod, Expression.Convert(argument, typeof(object)));
                return Expression.Convert(call, segment.ResultType);
            }
            else if (segment.Params != null)
            {
                return Expression.MakeIndex(argument, segment.Property, segment.Params.Select(Expression.Constant));
            }

            // PathSegment's factories set Property for every segment that isn't an array access or a registered
            // reference (a missing property throws when the path is parsed), so it's set here.
            return Expression.Property(argument, segment.Property!);
        }
    }
}
