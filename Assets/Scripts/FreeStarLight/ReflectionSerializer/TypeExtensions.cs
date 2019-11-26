using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

public static class TypeExtensions
{
    public static bool IsNullable(this Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }

    public static object Cast(this Type type, object data)
    {
        var DataParam = Expression.Parameter(typeof(object), "data");
        var Body = Expression.Block(Expression.Convert(Expression.Convert(DataParam, data.GetType()), type));

        var Run = Expression.Lambda(Body, DataParam).Compile();
        var ret = Run.DynamicInvoke(data);
        return ret;
    }

    public static bool TryCast(this Type type, object data, out object result)
    {
        try
        {
            result = type.Cast(data);
            return true;
        }
        catch (Exception)
        {
            result = null;
            return false;
        }
    }
    
    public static bool TryConvert(this Type type, object data, out object result)
    {
        try
        {
            result = Convert.ChangeType(data, type);
            return true;
        }
        catch (Exception)
        {
            result = null;
            return false;
        }
    }
}
