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
    public static class IFluentPathElementExtensions
    {
        public static IEnumerable<IElement> Children(this IElement element)
        {
            return element.GetChildNames().SelectMany(name => element.GetChildrenByName(name));
        }


        public static IEnumerable<IElement> Children(this IElement element, string name)
        {

            // REFACTOR
            //var result = element.Children().Where(c => c.IsMatch(name)).Select(c => c.Child);
            var result = element.GetChildrenByName(name);

            // If we are at a resource, we should match a path that is possibly not rooted in the resource
            // (e.g. doing "name.family" on a Patient is equivalent to "Patient.name.family")        
            if (!result.Any()) 
            {
                var rootname = element.GetChildNames().SingleOrDefault(n => Char.IsUpper(n[0]));
                
                if (rootname != null)
                {
                    var root = element.GetChildrenByName(rootname);
                    result = root.Children(name);
                }
                    
            }

            return result;
        }

        public static IEnumerable<IElement> Descendants(this IElement element)
        {
            //TODO: Don't think this is performant with these nested yields
            foreach (var child in element.Children())
            {
                yield return child;
                foreach (var grandchild in child.Descendants()) yield return grandchild;
            }
        }

    }
}
