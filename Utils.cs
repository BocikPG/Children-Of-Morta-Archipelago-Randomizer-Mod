using System.Collections;
using System.Drawing;
using System.IO;
using System.Reflection;
using Altar.Events;
using ArchipelagoRandomizer;
using UnityEngine;
using Zyklus.Home;
using Zyklus.Managers;


public static class Utils
{
	public static T GetFieldValue<T>(this object obj, string name)
	{
		var field = obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		return (T)field?.GetValue(obj);
	}
	public static void SetFieldValue<T>(this object obj, string name, T value)
	{
		obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
	}
}