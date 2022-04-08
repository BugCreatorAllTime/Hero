using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoteMiniMap
{
	public NoteMiniMap()
	{
	}

	public NoteMiniMap(List<int> x, List<int> y)
	{
		this.x = x;
		this.y = y;
	}
	
	public List<int> x { get; set;}
	public List<int> y { get; set;}

}
