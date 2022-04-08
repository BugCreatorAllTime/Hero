using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class GridTutorialCfg {

	public Dictionary<string, GridTutorialData> gridtutorial = new Dictionary<string, GridTutorialData>();
	
	public GridTutorialData getGridTutorial (int id)
	{
		return this.gridtutorial [id.ToString ()];
	}

	public GridTutorialData GetGridTutorialByIdTutorial(int IdTutorial)
	{
		for(int i = 0; i < gridtutorial.Count; i++)
		{
			if(gridtutorial.ElementAt(i).Value.IdTutorial == IdTutorial)
			{
				return gridtutorial.ElementAt(i).Value;
				break;
			}
		}
		return null;
	}
}
