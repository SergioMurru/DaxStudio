﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DaxStudio.UI.Converters
{
    public class ServerNameConverter : IValueConverter
    {
        public static Regex rex = new Regex(@"(?<=powerbi://)([^/]+/v\d+\.\d+)|(?<=asazure://)([^/]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var name = value as string;
            if (name == null) return Binding.DoNothing;
            var m = rex.Matches(name);
            if (m.Count > 0)
            {
                var result = rex.Replace(name, "...");
                return result;
            }
            // if it does not match the regex then return the name as-is
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        

    }
}