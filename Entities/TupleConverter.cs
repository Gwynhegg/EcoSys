﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace EcoSys.Entities
{
    public class TupleConverter<T1, T2> : System.ComponentModel.TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type source_type)
        {
            return source_type == typeof(string) || base.CanConvertFrom(context, source_type);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var key = Convert.ToString(value).Trim('(').Trim(')');
            var parts = Regex.Split(key, (", "));
            var item1 = (T1)TypeDescriptor.GetConverter(typeof(T1)).ConvertFromInvariantString(parts[0]);
            var item2 = (T2)TypeDescriptor.GetConverter(typeof(T2)).ConvertFromInvariantString(parts[1]);
            return new ValueTuple<T1, T2>(item1, item2);
        }
    }

    public class TripletConverter<T1,T2,T3> : System.ComponentModel.TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type source_type)
        {
            return source_type == typeof(string) || base.CanConvertFrom(context, source_type);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var key = Convert.ToString(value).Trim('(').Trim(')');
            var parts = Regex.Split(key, (", "));
            var item1 = (T1)TypeDescriptor.GetConverter(typeof(T1)).ConvertFromInvariantString(parts[0]);
            var item2 = (T2)TypeDescriptor.GetConverter(typeof(T2)).ConvertFromInvariantString(parts[1]);
            var item3 = (T3)TypeDescriptor.GetConverter(typeof(T3)).ConvertFromInvariantString(parts[2]);
            return new ValueTuple<T1, T2, T3>(item1, item2, item3);
        }
    }
}
