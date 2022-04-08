using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MiniMapData
{
	public int ID { get; set; }
	public int NumberNode { get; set; }
	public NoteMiniMap NodePosition { get; set;}
	public NoteMiniMap RockPosition { get; set;}
	public NoteMiniMap TreePosition { get; set;}
	public NoteMiniMap SwampPosition { get; set;}
}