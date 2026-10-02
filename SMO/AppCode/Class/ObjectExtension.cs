using SMO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace SMO
{
    public static class ObjectExtension
    {
        public static object CloneObject(this object objSource)
        {
            //Get the type of source object and create a new instance of that type
            Type typeSource = objSource.GetType();
            object objTarget = Activator.CreateInstance(typeSource);
            //Get all the properties of source object type
            PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //Assign all source property to taget object 's properties
            foreach (PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite)
                {
                    //check whether property type is value type, enum or string type
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    }
                    //else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
                    //else
                    //{
                    //    object objPropertyValue = property.GetValue(objSource, null);
                    //    if (objPropertyValue == null)
                    //    {
                    //        property.SetValue(objTarget, null, null);
                    //    }
                    //    else
                    //    {
                    //        property.SetValue(objTarget, objPropertyValue.CloneObject(), null);
                    //    }
                    //}
                }
            }
            return objTarget;
        }
    }

    public static class DecimalExtension
    {
        public static string ToStringVN(this decimal value)
        {
            try
            {
                CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");   // try with "en-US"
                var result = "";
                result = value.ToString(Global.NumberFormat, cul);
                return result;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }

    public static class StringExtension
    {
        public static string RemoveZeroWidthSpaces(this string input)
        {
            if (String.IsNullOrEmpty(input)) return input;
            char ZeroWidthSpace = (char)8203;
            if (input.Contains(ZeroWidthSpace.ToString()))
                return input.Replace(ZeroWidthSpace.ToString(), "");
            return input;
        }
    }
}