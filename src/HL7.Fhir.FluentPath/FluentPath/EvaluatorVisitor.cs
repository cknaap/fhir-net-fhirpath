﻿using Hl7.Fhir.Support;
using FP = Hl7.Fhir.FluentPath.Expressions;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.FluentPath;
using Hl7.Fhir.ElementModel;

namespace Hl7.Fhir.FluentPath
{
    internal class EvaluatorVisitor : FP.ExpressionVisitor<Evaluator>
    {
        public override Evaluator VisitConstant(FP.ConstantExpression expression)
        {
            return Return(new ConstantValue(expression.Value));
        }

        public override Evaluator VisitFunctionCall(FP.FunctionCallExpression expression)
        {
            Binding binding = null;
            var isKnown = Binding.Functions.TryGetValue(expression.FunctionName, out binding);

            if (isKnown) binding.Validate(expression);

            var focusEval = expression.Focus.ToEvaluator();
            var argsEval = expression.Arguments.Select(arg => arg.ToEvaluator());

            if(isKnown)
                return Call(focusEval, argsEval, binding);
            else
                return ExternalCall(expression.FunctionName, focusEval, argsEval);


        }

        public override Evaluator VisitLambda(FP.LambdaExpression expression)
        {
            return expression.Accept(this);
        }

        public override Evaluator VisitNewNodeListInit(FP.NewNodeListInitExpression expression)
        {
            return Return(FhirValueList.Empty());
        }

        public override Evaluator VisitVariableRef(FP.VariableRefExpression expression)
        {
            // Special case: $this
            if (expression is FP.AxisExpression)
            {
                var axis = (FP.AxisExpression)expression;
                if (axis.AxisName == "this")
                    return Focus();
                else
                    throw new NotSupportedException("Cannot resolve axis reference " + axis.AxisName);
            }
            else
                return ResolveValue(expression.Name);
        }

        public override Evaluator VisitTypeBinaryExpression(FP.TypeBinaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public static Evaluator Return(IValueProvider value)
        {
            return _ => (new[] { (IValueProvider)value });
        }

        public static Evaluator Return(IEnumerable<IValueProvider> value)
        {
            return _ => value;
        }

        public static Evaluator ResolveValue(string name)
        {
            return ctx => ctx.ResolveValue(name);
        }


        public static Evaluator Focus()
        {
            return ctx => ctx.FocusStack.Peek();
        }

        public static Evaluator Call(Evaluator focus, IEnumerable<Evaluator> arguments, Binding binding)
        {
            return ctx =>
            {
                try
                {
                    var focusNodes = focus(ctx);
                    var argumentNodes = arguments.Select(arg => arg(ctx)).ToList();

                    ctx.FocusStack.Push(focusNodes);

                    return binding.Function(focusNodes, argumentNodes);
                }
                finally
                {
                    ctx.FocusStack.Pop();
                }
            };
        }

        public static Evaluator ExternalCall(string name, Evaluator focus, IEnumerable<Evaluator> arguments)
        {
            return ctx =>
            {
                try
                {
                    var focusNodes = focus(ctx);
                    var argumentNodes = arguments.Select(arg => arg(ctx)).ToList();

                    ctx.FocusStack.Push(focusNodes);

                    return ctx.InvokeExternalFunction(name, focusNodes, argumentNodes);
                }
                finally
                {
                    ctx.FocusStack.Pop();
                }
            };
        }

    }

    public static class EvaluatorExpressionExtensions
    {
        public static Evaluator ToEvaluator(this FP.Expression expr)
        {
            var compiler = new EvaluatorVisitor();
            return expr.Accept<Evaluator>(compiler);
        }
    }

}
