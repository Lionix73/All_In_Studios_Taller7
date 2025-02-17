using System;
using System.Reflection;
public class Utilities
{
    public static void CopyValues<T>(T source, T destination) //Base y copia
    {
        Type type = source.GetType();
        foreach(FieldInfo field in type.GetFields())
        {
            field.SetValue(destination, field.GetValue(source));
        }
    }
}