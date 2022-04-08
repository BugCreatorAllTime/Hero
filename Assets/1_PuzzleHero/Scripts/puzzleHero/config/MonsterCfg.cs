using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterCfg
{
	public const int TYPE_MOB = 0;
	public const int TYPE_MINI_BOSS = 1;
	public const int TYPE_BOSS = 2;
	public const int TYPE_CHEST = -1;

    public Dictionary<string, MonsterCfgImpl> monster = new Dictionary<string, MonsterCfgImpl>();

    public MonsterCfgImpl GetMonsterCfgData(int id)
    {
        return this.monster[id.ToString()];
    }
}
