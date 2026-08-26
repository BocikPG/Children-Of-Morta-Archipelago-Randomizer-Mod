using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Altar.Localization;
using ArchipelagoRandomizer;


public static class Utils
{
	private static bool fired = false;

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
		if (source == null)
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

	public static void AddTranslationsToLocalizationData(List<string> keys, List<string> region, List<string> field, List<string> name)
	{
		if (fired)
			return;
		var transVersionTable = LocalizedTextUtility.sSingleton.pBank.pTable.GetFieldValue<List<List<float>>>("table_"); // It can be all 0 afaik (it's probably for internal processing)
		var sizeTable = LocalizedTextUtility.sSingleton.pBank.pTable.GetFieldValue<List<int>>("rows_key_");
		var table = LocalizedTextUtility.sSingleton.pBank.pTable.GetFieldValue<Dictionary<string, List<string>>>("table_string_dict_");
		Plugin.Logger.LogMessage(keys.Count);
		Plugin.Logger.LogMessage(transVersionTable.Count);
		Plugin.Logger.LogMessage(table.Count);

		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "APItemTalentDisplayName", "Talent", "DisplayName", "APItem");
		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "APItemTalentDescription", "Talent", "Description", "APItemDesc");
		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "APItemTalentShortDescription", "Talent", "ShortDescription", "APItemDesc");

		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "EmptyTalentDisplayName", "Talent", "DisplayName", "Empty talent");
		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "EmptyTalentShortDescription", "Talent", "Description", "Unlucky it gives nothing");
		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "EmptyTalentDescription", "Talent", "ShortDescription", "Unlucky it gives nothing, but it make game not crash - lose-win?");

		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "APItemRelicDisplayName", "DivineRelic", "DisplayName", "APItem");
		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "APItemRelicDescription", "DivineRelic", "Description", "APItemDesc");
		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "APItemRelicShortDescription", "DivineRelic", "ShortDescription", "APItemDesc");

		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "Location-100DisplayName", "DivineRelic", "DisplayName", "Blank Relic");
		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "Location-100ShortDescription", "DivineRelic", "Description", "It does nothing - really");
		AddTranslationToLocalizationData(transVersionTable, sizeTable, table, "Location-100Description", "DivineRelic", "ShortDescription", "It polluted the pool, but made game not crush - fair deal if you ask me");

		for(int i=0;i<keys.Count;i++)
		{
			AddTranslationToLocalizationData(transVersionTable, sizeTable, table, keys[i], region[i], field[i], name[i]);
		}
		fired = true;
		Plugin.Logger.LogMessage(transVersionTable.Count);
		Plugin.Logger.LogMessage(table.Count);
	}

	public static void AddTranslationToLocalizationData(List<List<float>> transVersionTable, List<int> sizeTable, Dictionary<string, List<string>> table, string key, string region, string field, string name)
	{
		transVersionTable.Add(new List<float>([0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]));
		sizeTable.Add(-1);
		table.Add(key, new List<string>([key, region, field, "DESC", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1"]));
	}
}