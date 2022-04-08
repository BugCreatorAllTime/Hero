using UnityEngine;
using System.Collections;

public class ItemCfgImpl
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string SetName { get; set; }
	public int SetId { get; set; }
	public int Color {get; set;}
	public Stat Stat { get; set; }
	public string Icon { get; set; }
	public int SetSkillId;
	public string Description { get; set; }
	public string NameOfSet { get; set;}

	public ItemCfgImpl Clone() 
	{
		ItemCfgImpl item = new ItemCfgImpl();
		item.Id = this.Id;
		item.Name = this.Name;
		item.SetName = this.SetName;
		item.SetId = this.SetId;
		item.Color = this.Color;
		item.Stat = this.Stat;
		item.Icon = this.Icon;
		item.Description = this.Description;
		item.NameOfSet = this.NameOfSet;
		return item;
	}


}
