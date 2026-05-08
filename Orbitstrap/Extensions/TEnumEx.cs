using System.ComponentModel;
using System.Reflection;

namespace Orbitstrap.Extensions;

internal static class TEnumEx
{
	public static string? GetDescription<TEnum>(this TEnum e)
	{
		ref TEnum reference = ref e;
		TEnum val = default(TEnum);
		object obj;
		if (val == null)
		{
			val = reference;
			reference = ref val;
			if (val == null)
			{
				obj = null;
				goto IL_0031;
			}
		}
		obj = reference.ToString();
		goto IL_0031;
		IL_0031:
		string text = (string)obj;
		if (text == null)
		{
			return null;
		}
		ref TEnum reference2 = ref e;
		val = default(TEnum);
		object obj2;
		if (val == null)
		{
			val = reference2;
			reference2 = ref val;
			if (val == null)
			{
				obj2 = null;
				goto IL_006e;
			}
		}
		obj2 = reference2.GetType().GetField(text);
		goto IL_006e;
		IL_006e:
		FieldInfo fieldInfo = (FieldInfo)obj2;
		if (fieldInfo == null)
		{
			return null;
		}
		return fieldInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
	}
}
