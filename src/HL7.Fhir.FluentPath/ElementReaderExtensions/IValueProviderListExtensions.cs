﻿/* 
 * Copyright (c) 2015, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */


using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.ElementModel;

namespace Hl7.Fhir.FluentPath
{
    public static class ValueProviderExtensions
    {

        public static IEnumerable<IElementReader> JustElements(this IEnumerable<IValueProvider> focus)
        {
            return focus.OfType<IElementReader>();
        }

        public static IEnumerable<IValueProvider> JustValues(this IEnumerable<IValueProvider> focus)
        {
            return focus.Where(f => f.Value != null);
        }
   
        public static object SingleValue(this IEnumerable<IValueProvider> focus)
        {
            return focus.JustValues().Single().Value;
        }

        public static long AsInteger(this IEnumerable<IValueProvider> focus)
        {
            return focus.Single().AsInteger();
        }

        public static decimal AsDecimal(this IEnumerable<IValueProvider> focus)
        {
            return focus.Single().AsDecimal();
        }

        public static bool AsBoolean(this IEnumerable<IValueProvider> focus)
        {
            return focus.Single().AsBoolean();
        }

        public static string AsString(this IEnumerable<IValueProvider> focus)
        {
            return focus.Single().AsString();
        }

        public static PartialDateTime AsDateTime(this IEnumerable<IValueProvider> focus)
        {
            return focus.Single().AsDateTime();
        }

        private static bool booleanEval(this IEnumerable<IValueProvider> focus)
        {
            var result = false;

            // An empty result is considered "false"
            if (!focus.Any())
                result = false;

            // A single result that's a boolean should be interpreted as a boolean
            else if (focus.JustValues().Count() == 1 && focus.JustValues().Single().Value is Boolean)
            {
                return focus.Single().AsBoolean();
            }

            // Otherwise, we have "some" content, which we'll consider "true"
            else
                result = true;

            return result;
        }

        public static IEnumerable<IValueProvider> BooleanEval(this IEnumerable<IValueProvider> focus)
        {
            if (focus.Any())
                return FhirValueList.Create(focus.booleanEval());
            else
                return FhirValueList.Empty();               
        }
        public static IEnumerable<IValueProvider> IsEmpty(this IEnumerable<IValueProvider> focus)
        {
            return FhirValueList.Create(!focus.Any());
        }

        public static IEnumerable<IValueProvider> Not(this IEnumerable<IValueProvider> focus)
        {
            if (focus.Any())
                return FhirValueList.Create(!focus.booleanEval());
            else
                return FhirValueList.Empty();
        }

        public static IEnumerable<IValueProvider> Exists(this IEnumerable<IValueProvider> focus)
        {
            return FhirValueList.Create(focus.Any());
        }


        public static IEnumerable<IValueProvider> Or(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            return FhirValueList.Create(left.booleanEval() || right.booleanEval());
        }

        public static IEnumerable<IValueProvider> And(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            return FhirValueList.Create(left.booleanEval() && right.booleanEval());
        }

        public static IEnumerable<IValueProvider> Xor(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            return FhirValueList.Create(left.booleanEval() ^ right.booleanEval());
        }

        public static IEnumerable<IValueProvider> Implies(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            return FhirValueList.Create(!left.booleanEval() || right.booleanEval());
        }

        public static IEnumerable<IValueProvider> Item(this IEnumerable<IValueProvider> focus, int index)
        {
            return focus.Skip(index).Take(1);
        }

        public static IEnumerable<IValueProvider> Where(this IEnumerable<IValueProvider> focus, 
                        Func<IEnumerable<IValueProvider>, IEnumerable<IValueProvider>> condition)
        {
            return focus.Where(v => condition(FhirValueList.Create(v)).booleanEval());
        }

        public static IEnumerable<IValueProvider> Any(this IEnumerable<IValueProvider> focus,
        Func<IEnumerable<IValueProvider>, IEnumerable<IValueProvider>> condition)
        {
            return FhirValueList.Create(focus.Any(v => condition(FhirValueList.Create(v)).booleanEval()));
        }

        public static IEnumerable<IValueProvider> All(this IEnumerable<IValueProvider> focus,
                Func<IEnumerable<IValueProvider>, IEnumerable<IValueProvider>> condition)
        {
            return FhirValueList.Create(focus.All(v => condition(FhirValueList.Create(v)).booleanEval()));
        }


        public static IEnumerable<IValueProvider> Select(this IEnumerable<IValueProvider> focus,
                Func<IEnumerable<IValueProvider>, IEnumerable<IValueProvider>> mapper)
        {
            return focus.SelectMany(v => mapper(FhirValueList.Create(v)));
        }


        public static IEnumerable<IValueProvider> Distinct(this IEnumerable<IValueProvider> focus)
        {
            return focus.Distinct(new FhirPathValueEqualityComparer());
        }

        public static IEnumerable<IValueProvider> CountItems(this IEnumerable<IValueProvider> focus)
        {
            return FhirValueList.Create(focus.Count());
        }

        public static IEnumerable<IValueProvider> IntegerEval(this IEnumerable<IValueProvider> focus)
        {
            if (focus.JustValues().Count() == 1)
            {
                var val = focus.Single().Value;
                if(val != null)
                {
                    if (val is long) return FhirValueList.Create((long)val);
                    //if (val is decimal) return (Int64)Math.Round((decimal)val);
                    if (val is string)
                    {
                        long result;
                        if (Int64.TryParse((string)val, out result))
                            return FhirValueList.Create(result);
                    }

                }
            }

            return FhirValueList.Empty();
        }

        public static IEnumerable<IValueProvider> Substring(this IEnumerable<IValueProvider> focus, long start, long? length)
        {
            if(focus.Count() == 1)
            {
                if (focus.First().Value != null)
                {
                    var str = focus.First().AsStringRepresentation();

                    if (length.HasValue)
                        return FhirValueList.Create(str.Substring((int)start, (int)length.Value));
                    else
                        return FhirValueList.Create(str.Substring((int)start));
                }
            }

            return FhirValueList.Empty();
        }

        public static IEnumerable<IValueProvider> StartingWith(this IEnumerable<IValueProvider> focus, string prefix)
        {
            return focus.JustValues().Where(value => value.AsStringRepresentation().StartsWith(prefix));
        }

        //public static IEnumerable<IFluentPathElement> Resolve(this IEnumerable<IFluentPathValue> focus, FhirClient client)
        //{
        //    return focus.Resolve(new BaseEvaluationContext(client));
        //}

        //public static IEnumerable<IFluentPathElement> Resolve(this IEnumerable<IFluentPathValue> focus, IEvaluationContext context)
        //{
        //    foreach (var item in focus)
        //    {
        //        string url = null;

        //        // Something that looks like a Reference
        //        if (item is IFluentPathElement)
        //        {
        //            var maybeReference = ((IFluentPathElement)item).Children("reference").SingleOrDefault();

        //            if (maybeReference != null && maybeReference.Value != null && maybeReference.Value is string)
        //                url = maybeReference.AsString();
        //        }

        //        // A string as a direct url
        //        if (item.Value != null && item.Value is string)
        //            url = item.AsString();

        //        if(url != null)
        //            yield return context.ResolveResource(url);
        //    }
        //}

        public static IEnumerable<IValueProvider> MaxLength(this IEnumerable<IValueProvider> focus)
        {
            return FhirValueList.Create( focus.JustValues()
                .Aggregate(0, (val, item) => Math.Max(item.AsStringRepresentation().Length, val)) );
        }

        public static IEnumerable<IValueProvider> SubsetOf(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            return left.All(l => right.Any(r => l.IsEqualTo(r)));
        }


        public static IEnumerable<IValueProvider> Extension(this IEnumerable<IValueProvider> focus, string url)
        {            
            return focus.Children("extension").Where(es => es.Children("url").IsEqualTo(url));
        }

        public static IEnumerable<IElementReader> Children(this IEnumerable<IValueProvider> focus, string name)
        {
            return focus.JustElements().SelectMany(node => node.Children(name));
        }

        //public static IEnumerable<IFhirPathElement> Child(this IEnumerable<IFhirPathValue> focus, string name)
        //{
        //    return focus.JustFhirPathElements().Child(name);
        //}

        //public static IEnumerable<IFhirPathElement> Child(this IEnumerable<IFhirPathElement> focus, string name)
        //{
        //    return focus.SelectMany(node => node.Children().Where(child => child.IsMatch(name)));
        //}


        public static IEnumerable<IElementReader> Children(this IValueProvider focus)
        {
            if (focus is IElementReader)
            {
                return ((IElementReader)focus).Children();
            }

            return Enumerable.Empty<IElementReader>();
        }

        public static IEnumerable<IElementReader> Children(this IEnumerable<IValueProvider> focus)
        {
            return focus.JustElements().SelectMany(node => node.Children());
        }

        public static IEnumerable<IElementReader> Descendants(this IEnumerable<IElementReader> focus)
        {
            return focus.SelectMany(node => node.Descendants());
        }

        public static IEnumerable<IValueProvider> IsEqualTo(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            if (!left.Any() || !right.Any()) return FhirValueList.Empty();

            if (left.Count() != right.Count()) return FhirValueList.Create(false);

            return FhirValueList.Create(left.Zip(right, (l, r) => l.IsEqualTo(r)).All(x => x));
        }

        public static IEnumerable<IValueProvider> IsEqualTo(this IEnumerable<IValueProvider> left, object value)
        {
            var result = left.SingleOrDefault(v => Object.Equals(v.Value,value)) != null;
            return FhirValueList.Create(result);
        }

        public static IEnumerable<IValueProvider> IsEquivalentTo(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            if (!left.Any() && !right.Any()) return FhirValueList.Create(true);
            if (left.Count() != right.Count()) return FhirValueList.Create(false);

            return FhirValueList.Create(left.All((IValueProvider l) => right.Any(r => l.IsEquivalentTo(r))));
        }


        public static IEnumerable<IValueProvider> GreaterThan(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            if (left.Count() == 1 && right.Count() == 1)
                yield return left.Single().GreaterThan(right.Single());
        }

        public static IEnumerable<IValueProvider> GreaterOrEqual(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            if (left.Count() == 1 && right.Count() == 1)
                yield return left.Single().GreaterOrEqual(right.Single());
        }

        public static IEnumerable<IValueProvider> LessThan(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            if (left.Count() == 1 && right.Count() == 1)
                yield return left.Single().LessThan(right.Single());
        }

        public static IEnumerable<IValueProvider> LessOrEqual(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            if (left.Count() == 1 && right.Count() == 1)
                yield return left.Single().LessOrEqual(right.Single());
        }

        public static IEnumerable<IValueProvider> Add(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            if (!left.Any() || !right.Any()) yield break;
                    
            if (left.Count() == 1 && right.Count() == 1)
                yield return left.Single().Add(right.Single());
        }

        public static IEnumerable<IValueProvider> Sub(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            if (left.Count() == 1 && right.Count() == 1)
                yield return left.Single().Sub(right.Single());
        }

        public static IEnumerable<IValueProvider> Mul(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            if (left.Count() == 1 && right.Count() == 1)
                yield return left.Single().Mul(right.Single());
        }

        public static IEnumerable<IValueProvider> Div(this IEnumerable<IValueProvider> left, IEnumerable<IValueProvider> right)
        {
            if (left.Count() == 1 && right.Count() == 1)
                yield return left.Single().Div(right.Single());
        }

        //public static IEnumerable<IFhirPathValue> Union(this IEnumerable<IFhirPathValue> left, IEnumerable<IFhirPathValue> right)
        //{
        //    return left.Union(right);
        //}

        ///// <summary>Enumerate the descendants of the specified nodes.</summary>
        ///// <typeparam name="T">The type of a tree node.</typeparam>
        ///// <param name="nodeSet">A set of tree nodes.</param>
        ///// <returns>An enumerator for nodes of type <typeparamref name="T"/>.</returns>
        //public static IEnumerable<T> Descendants<T>(this IEnumerable<T> nodeSet) where T : FhirInstanceTree
        //{
        //    return nodeSet.SelectMany(node => node.Descendants());
        //}
        ///// <summary>Enumerate the specified nodes and their descendants.</summary>
        ///// <typeparam name="T">The type of a tree node.</typeparam>
        ///// <param name="nodeSet">A set of tree nodes.</param>
        ///// <returns>An enumerator for nodes of type <typeparamref name="T"/>.</returns>
        //public static IEnumerable<T> DescendantsAndSelf<T>(this IEnumerable<T> nodeSet) where T : FhirInstanceTree
        //{
        //    return nodeSet.SelectMany(node => node.DescendantsAndSelf());
        //}

        ///// <summary>Enumerate the ancestors of the specified nodes.</summary>
        ///// <typeparam name="T">The type of a tree node.</typeparam>
        ///// <param name="nodeSet">A set of tree nodes.</param>
        ///// <returns>An enumerator for nodes of type <typeparamref name="T"/>.</returns>
        //public static IEnumerable<T> Ancestors<T>(this IEnumerable<T> nodeSet) where T : FhirInstanceTree
        //{
        //    return nodeSet.SelectMany(node => node.Ancestors());
        //}

        ///// <summary>Enumerate the specified nodes and their ancestors.</summary>
        ///// <typeparam name="T">The type of a tree node.</typeparam>
        ///// <param name="nodeSet">A set of tree nodes.</param>
        ///// <returns>An enumerator for nodes of type <typeparamref name="T"/>.</returns>
        //public static IEnumerable<T> AncestorsAndSelf<T>(this IEnumerable<T> nodeSet) where T : FhirInstanceTree
        //{
        //    return nodeSet.SelectMany(node => node.AncestorsAndSelf());
        //}

        ///// <summary>Enumerate the siblings preceding the specified nodes.</summary>
        ///// <typeparam name="T">The type of a tree node.</typeparam>
        ///// <param name="nodeSet">A set of tree nodes.</param>
        ///// <returns>An enumerator for nodes of type <typeparamref name="T"/>.</returns>
        //public static IEnumerable<T> PrecedingSiblings<T>(this IEnumerable<T> nodeSet) where T : FhirInstanceTree
        //{
        //    return nodeSet.SelectMany(node => node.PrecedingSiblings());
        //}

        ///// <summary>Enumerate the siblings following the specified nodes.</summary>
        ///// <typeparam name="T">The type of a tree node.</typeparam>
        ///// <param name="nodeSet">A set of tree nodes.</param>
        ///// <returns>An enumerator for nodes of type <typeparamref name="T"/>.</returns>
        //public static IEnumerable<T> FollowingSiblings<T>(this IEnumerable<T> nodeSet) where T : FhirInstanceTree
        //{
        //    return nodeSet.SelectMany(node => node.PrecedingSiblings());
        //}

        class FhirPathValueEqualityComparer : IEqualityComparer<IValueProvider>
        {
            public bool Equals(IValueProvider x, IValueProvider y)
            {
                return x.IsEqualTo(y);
            }

            public int GetHashCode(IValueProvider value)
            {
                var result = value.Value != null ? value.Value.GetHashCode() : 0;

                if (value is IElementReader)
                {
                    result ^= (((IElementReader)value).GetChildNames().SingleOrDefault() ?? "key").GetHashCode();
                }

                return result;
            }
        }
    }
}


