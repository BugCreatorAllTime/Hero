using System.Collections.Generic;

public class BoardCfg
{
	public Dictionary<string, BoardBgInfo> BoardBg = new Dictionary<string, BoardBgInfo>();
}

public class BoardBgInfo
{
	public int Id { get; set; }
	public string FirstCell { get; set; }
	public string Background { get; set; }
	public string SecondCell { get; set; }
}