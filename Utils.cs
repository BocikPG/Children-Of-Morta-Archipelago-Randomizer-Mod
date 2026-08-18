using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


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

	public static T DeepCopy<T>(this T source) where T : class
	{
		if(source == null)
		{
			return null;
		}

		using (MemoryStream stream = new MemoryStream())
		{
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, source);
			stream.Seek(0, SeekOrigin.Begin);
			return (T)formatter.Deserialize(stream);
		}
	}
}