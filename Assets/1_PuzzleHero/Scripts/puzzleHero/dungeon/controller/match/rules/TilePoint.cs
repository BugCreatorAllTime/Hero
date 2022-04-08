/// <summary>
/// To record the location of data.
/// </summary>
public struct TilePoint
{
	public int x, y;

	public TilePoint(int px, int py)
	{
		x = px;
		y = py;
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ")";
	}

	public override bool Equals(object obj) {
		if (!(obj is TilePoint))
			return false;

		TilePoint point = (TilePoint)obj;
		if(x != point.x || y != point.y )
		{
			return false;
		}
		return true;
	}
}