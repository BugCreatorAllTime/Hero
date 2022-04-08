using System;

public class ObjectStatus
{
	public static readonly int NORMAL = 1<<1;

	public ObjectStatus ()
	{
	}

	public static bool hasStatus(int source, int target) {
		return (source & target) == target;
	}
}

