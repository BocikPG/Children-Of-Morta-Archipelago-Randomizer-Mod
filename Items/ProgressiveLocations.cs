

using ArchipelagoRandomizer.Items;

public static class ProgressiveLocations
{
	public static bool pIsRelicLocationsEnabled = false;
	public static bool pIsTalentLocationsEnabled = false;
	public static long pMaxRelicId = 0;
	public static long pMaxTalentId = 0;
    public static long pCurrentRelicId
	{
		get => Connection.pSession.DataStorage["CurrentProgressiveRelicId"] > pMaxRelicId ? -200 : Connection.pSession.DataStorage["CurrentProgressiveRelicId"];
	}
    public static long pCurrentTalentId
	{
		get => Connection.pSession.DataStorage["CurrentProgressiveTalentId"] > pMaxTalentId ? -100 : Connection.pSession.DataStorage["CurrentProgressiveTalentId"];
	}

	public static void Init()
	{
		var session = Connection.pSession;
		session.DataStorage["CurrentProgressiveRelicId"].Initialize(APItemsUtils.pBaseLocationsId + 100);
		session.DataStorage["CurrentProgressiveTalentId"].Initialize(APItemsUtils.pBaseLocationsId + 800);
	}

	public static void IncreaseRelicId()
	{
		Connection.pSession.DataStorage["CurrentProgressiveRelicId"] += (long)1;
	}
	public static void IncreaseTalentId()
	{
        Connection.pSession.DataStorage["CurrentProgressiveTalentId"] += (long)1;
	}
}