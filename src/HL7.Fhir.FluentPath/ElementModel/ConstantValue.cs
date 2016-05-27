﻿/* 
 * Copyright (c) 2015, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using System;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Support;

namespace Hl7.Fhir.ElementModel
{
    public class ConstantValue : IValueProvider
    {
        public static object ToFluentPathValue(object value)
        {
            object Value;

            if (value is Boolean)
                Value = value;
            else if (value is String)
                Value = value;
            else if (value is Uri)
                Value = ((Uri)value).OriginalString;
            else if (value is char)
                Value = new String((char)value,1);
            else if (value is Int32 || value is Int16 || value is UInt16 || value is UInt32 || value is Int64 || value is UInt64)
                Value = Convert.ToInt64(value);
            else if (value is float || value is double || value is Decimal)
                Value = Convert.ToDecimal(value);
            else if (value is DateTimeOffset)
                Value = PartialDateTime.FromDateTime((DateTimeOffset)value);
            else if (value is DateTime)
                Value = PartialDateTime.FromDateTime((DateTime)value);
            else if (value is PartialDateTime)
                Value = value;
            else if (value is Time)
                Value = value;
            else
                throw Error.NotSupported("Don't know how to convert an instance of .NET type {0} (with value '{1}') to a FluentPath constant"
                    .FormatWith(value.GetType().Name, value.ToString()));

            return Value;
        }


        private object _original;

        public ConstantValue(object value)
        {
            _original = value;
            Value = ToFluentPathValue(value);
        }

        public object Value
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is IValueProvider)
                return Object.Equals((obj as IValueProvider).Value,Value);
            else
                return false;
        }

        public override int GetHashCode()
        {
            if (Value != null)
                return Value.GetHashCode();
            else
                return 0;
        }
    }
}
