using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class MonsterSkillCfg
{
	public Dictionary<string, SkillCfgName> AllSkillCfg = new Dictionary<string, SkillCfgName>();
	public DefaultSkillCfg DefaultSkillCfg;
	public Dictionary<string, BoardSkillInfo> DestroyGemSkillCfg = new Dictionary<string, BoardSkillInfo>();
	public Dictionary<string, BoardSkillInfo> TransformGemSkillCfg = new Dictionary<string, BoardSkillInfo>();
	public Dictionary<string, BoardSkillInfo> ShuffleAllGemSkillCfg = new Dictionary<string, BoardSkillInfo>();
	public Dictionary<string, BoardSkillInfo> PoisonSkillCfg;
	public Dictionary<string, BoardSkillInfo> FireSkillCfg;
	public Dictionary<string, BoardSkillInfo> IceLockGemSkillCfg;
	public Dictionary<string, BoardSkillInfo> RockLockGemSkillCfg;
	public Dictionary<string, BoardSkillInfo> AutoRecoverSkillCfg;
	public Dictionary<string, BoardSkillInfo> ShuffleMatchGemSkillCfg;

	public SkillCfgName getSkillCfg(int id)
	{
		return this.AllSkillCfg[id.ToString()];
	}
}

public class SkillCfgName
{
	public string name { get; set; }
}

public class DefaultSkillCfg
{
}
