using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Action
{
	public Data.TileTypes type ;
	public int count;
	public Matches.Shape shape;

	public Action(Data.TileTypes type, Matches.Shape shape, int count)
	{
		this.type = type;
		this.shape = shape;
		this.count = count;
	}

}



