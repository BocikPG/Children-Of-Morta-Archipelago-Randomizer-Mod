using System.Collections.Generic;

public class APLogs
{
	public List<string> LogLines = new();
	public static APLogs sSingleton;
	private int maxLines_ = 20;

	public float LastUpdateTime;
	public string ScrollText;

	public static System.Action OnNewLogMessage;

	public APLogs()
	{
		sSingleton = this;
	}

	public void LogMessage(string message)
	{
		if (LogLines.Count >= maxLines_)
		{
			LogLines.RemoveAt(0);
		}
		LogLines.Add(message);

		ScrollText = "";

		foreach (var line in LogLines)
		{
			ScrollText += $"> {line}\n";
		}

		LastUpdateTime = Time.time2;
		OnNewLogMessage?.Invoke();
	}
}