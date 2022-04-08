using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterCfgImpl
{
    public string Name { get; set; }
	public string Ani {get;set;}
    public int MonsterId { get; set; }
	public int Type {get; set;}
	public int HpFormularA {get;set;}
	public int HpFormularC {get;set;}
	public int DamageFormularC {get;set;}
    public List<int> skill_id = new List<int>();
    public List<int> skill_lv = new List<int>();
    public Stat Stat = new Stat();
}