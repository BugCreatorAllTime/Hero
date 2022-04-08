using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ItemSetSkillCfg
{
	public Dictionary<string, SetSkill> SetSkill;

	public string GetSkillNameById(int id)
	{
		return (string) GetFieldDataById("SkillName", id);
	}

	public StatsBonus GetSkillBonusById(int id)
	{
		return (StatsBonus) GetFieldDataById("statsBonus", id);
	}

	public string GetSkillExtrasById(int id) {
		return (string) GetFieldDataById("extras", id);
	}

	private object GetFieldDataById(string fieldName, int id)
	{
		for (int i = 0; i < SetSkill.Count; i++) {
			KeyValuePair<string, SetSkill> pair = SetSkill.ElementAt(i);
			if (pair.Value.Id == id)
			{
				foreach (FieldInfo fieldInfo in pair.Value.GetType().GetFields()) {
					//Logger.Trace("field to find", fieldName, "filtered field", fieldInfo.Name);
					if (fieldInfo.Name.Equals(fieldName))
					{
						return fieldInfo.GetValue(pair.Value);
					}
				}
			}
		}
		return null;
	}
}

public class SetSkill
{
	public int Id;
	public string SkillName;
	public StatsBonus statsBonus;
	public string extras;
}

public class StatsBonus
{
	public double hp = 0;
	public double armor = 0;
	public double damage = 0;
}