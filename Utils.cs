using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Altar.Data;
using Altar.Localization;
using Archipelago.MultiClient.Net.MessageLog.Messages;
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
		var exist_table = LocalizedTextUtility.sSingleton.pBank.pTable.GetFieldValue<List<List<float>>>("table_"); //I guess this checks if translation is in given language - 4 is ENG
		var sizeTable = LocalizedTextUtility.sSingleton.pBank.pTable.GetFieldValue<List<int>>("rows_key_");
		var table = LocalizedTextUtility.sSingleton.pBank.pTable.GetFieldValue<Dictionary<string, List<string>>>("table_string_dict_");
		Plugin.Logger.LogMessage(keys.Count);
		Plugin.Logger.LogMessage(exist_table.Count);
		Plugin.Logger.LogMessage(table.Count);

		AddTranslationToLocalizationData(exist_table, sizeTable, table, "APItemTalentDisplayName", "Talent", "DisplayName", "APItem");
		AddTranslationToLocalizationData(exist_table, sizeTable, table, "APItemTalentDescription", "Talent", "Description", "APItemDesc");
		AddTranslationToLocalizationData(exist_table, sizeTable, table, "APItemTalentShortDescription", "Talent", "ShortDescription", "APItemDesc");

		AddTranslationToLocalizationData(exist_table, sizeTable, table, "EmptyTalentDisplayName", "Talent", "DisplayName", "Empty talent");
		AddTranslationToLocalizationData(exist_table, sizeTable, table, "EmptyTalentShortDescription", "Talent", "Description", "Unlucky it gives nothing");
		AddTranslationToLocalizationData(exist_table, sizeTable, table, "EmptyTalentDescription", "Talent", "ShortDescription", "Unlucky it gives nothing, but it make game not crash - lose-win?");

		AddTranslationToLocalizationData(exist_table, sizeTable, table, "APItemRelicDisplayName", "DivineRelic", "DisplayName", "APItem");
		AddTranslationToLocalizationData(exist_table, sizeTable, table, "APItemRelicDescription", "DivineRelic", "Description", "APItemDesc");
		AddTranslationToLocalizationData(exist_table, sizeTable, table, "APItemRelicShortDescription", "DivineRelic", "ShortDescription", "APItemDesc");

		AddTranslationToLocalizationData(exist_table, sizeTable, table, "Location-100DisplayName", "DivineRelic", "DisplayName", "Blank Relic");
		AddTranslationToLocalizationData(exist_table, sizeTable, table, "Location-100ShortDescription", "DivineRelic", "Description", "It does nothing - really");
		AddTranslationToLocalizationData(exist_table, sizeTable, table, "Location-100Description", "DivineRelic", "ShortDescription", "It polluted the pool, but made game not crush - fair deal if you ask me");

		for(int i=0;i<keys.Count;i++)
		{
			AddTranslationToLocalizationData(exist_table, sizeTable, table, keys[i], region[i], field[i], name[i]);
		}
		fired = true;
		Plugin.Logger.LogMessage(exist_table.Count);
		Plugin.Logger.LogMessage(table.Count);
	}

	public static void AddTranslationToLocalizationData(List<List<float>> exist_table, List<int> sizeTable, Dictionary<string, List<string>> table, string key, string region, string field, string name)
	{
		exist_table.Add(new List<float>([0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]));
		sizeTable.Add(-1);
		table.Add(key, new List<string>([key, region, field, "DESC", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1", name, "1"]));
	}
}