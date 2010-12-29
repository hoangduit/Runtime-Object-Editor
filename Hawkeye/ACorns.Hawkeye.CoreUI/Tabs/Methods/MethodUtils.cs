/* ****************************************************************************
 *  Hawkeye - The .Net Runtime Object Editor
 * 
 * Copyright (c) 2005 Corneliu I. Tusnea
 * 
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author be held liable for any damages arising from 
 * the use of this software.
 * Permission to use, copy, modify, distribute and sell this software for any 
 * purpose is hereby granted without fee, provided that the above copyright 
 * notice appear in all copies and that both that copyright notice and this 
 * permission notice appear in supporting documentation.
 * 
 * Corneliu I. Tusnea (corneliutusnea@yahoo.com.au)
 * http://www.acorns.com.au/hawkeye/
 * ****************************************************************************/


using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace ACorns.Hawkeye.Tabs.Methods
{
	/// <summary>
	/// Summary description for MethodUtils.
	/// </summary>
	internal sealed class MethodUtils
	{
		private MethodUtils()
		{
		}

		public static PropertyDescriptorCollection GetMethodProperties(object obj)
		{
			Type type = obj.GetType();

			if (obj is MethodPropertyDescriptor.MethodPropertyValueHolder)
			{
				MethodPropertyDescriptor.MethodPropertyValueHolder mobj = obj as MethodPropertyDescriptor.MethodPropertyValueHolder;
				return mobj.Method.GetChildProperties(null, null);
			}

			ArrayList methodDesc = new ArrayList();

			int depth = 1;
			do
			{
				MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

				for (int i = 0; i < methods.Length; i++)
				{
					try
					{
						MethodInfo method = methods[i];
						if (!method.IsSpecialName || method.Name.EndsWith("get_Item") || method.Name.EndsWith("set_Item"))
						{
							methodDesc.Add(new MethodPropertyDescriptor(obj, method, type, depth));
						}
					}
					catch (Exception)
					{
					}
				}

				if ( type == typeof(object) )
					break;

				type = type.BaseType;
				depth ++;
			}
			while(true);

			methodDesc.Sort(new MethodNameComparer());

			MethodPropertyDescriptor[] methodsDesc = (MethodPropertyDescriptor[]) methodDesc.ToArray(typeof (MethodPropertyDescriptor));
			return new PropertyDescriptorCollection(methodsDesc);
		}

		private class MethodNameComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				if (x == null && y == null)
					return 0;
				if (x == null)
					return 1;
				if (y == null)
					return -1;
				MethodPropertyDescriptor mx = (MethodPropertyDescriptor) x;
				MethodPropertyDescriptor my = (MethodPropertyDescriptor) y;
				return String.Compare(mx.MethodInfo.Name, my.MethodInfo.Name);
			}
		}

		public static ArrayList GetMethodParams(MethodPropertyDescriptor methodDesc)
		{
			ArrayList list = new ArrayList();
			ParameterInfo[] paramInfo = methodDesc.MethodInfo.GetParameters();
			for (int i = 0; i < paramInfo.Length; i++)
			{
				ParameterInfo param = paramInfo[i];
				list.Add(new ParameterPropertyDescriptor(methodDesc, param));
			}
			return list;
		}


		public static string GetMethodSignature(MethodInfo method)
		{
			// Build Method Signature
			StringBuilder builder = new StringBuilder();

			builder.Append(GetMethodAccess(method));

			builder.Append(method.ReturnType.Name);
			builder.Append(" ");

			builder.Append(method.Name);
			builder.Append(" ( ");

			ParameterInfo[] param = method.GetParameters();
			foreach (ParameterInfo parameterInfo in param)
			{
				builder.Append(parameterInfo.ParameterType.Name);
				builder.Append(" ");
				builder.Append(parameterInfo.Name);
				builder.Append(", ");
			}
			if (param.Length > 0)
				builder.Remove(builder.Length - 2, 2);

			builder.Append(" )");

			return builder.ToString();
		}

		public static string GetMethodAccess(MethodInfo method)
		{
			StringBuilder builder = new StringBuilder();
			if (method.IsPrivate)
				builder.Append("private ");
			if (method.IsPublic)
				builder.Append("public ");
			if (method.IsAssembly)
				builder.Append("internal ");
			if (method.IsFamily)
				builder.Append("protected ");
			if (method.IsFamilyOrAssembly)
				builder.Append("internal protected ");
			if (method.IsFamilyAndAssembly)
				builder.Append("internal+protected ");

			if (method.IsStatic)
				builder.Append("static ");
			if (method.IsVirtual)
				builder.Append("virtual ");
			if (method.IsAbstract)
				builder.Append("abstract ");

			return builder.ToString();
		}

		public static string GetMethodAccessShort(MethodInfo method)
		{
			StringBuilder builder = new StringBuilder();
			if (method.IsPublic)
				builder.Append("public ");

			if (method.IsFamilyAndAssembly||method.IsFamilyOrAssembly||method.IsFamily||method.IsPrivate||method.IsAssembly)
				builder.Append("non-public ");

			if (method.IsStatic)
				builder.Append("static ");
			if (method.IsVirtual)
				builder.Append("virtual ");
			if (method.IsAbstract)
				builder.Append("abstract ");

			return builder.ToString();
		}
	}
}