using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HomeTownTutorialData {

	public int Id{ get;set;}
	public string Location { get; set;}
	public int Type { get; set;}
	public string Target { get; set;}
	public string Description {get;set;}
	public List<int> TalkSize { get; set;}
	public List<int> TalkPosition{ get; set;}
	public List<int> TextSize { get; set;}
	public List<int> TextPosition { get; set;}
	public int Rotation { get; set;}
	public List<string> ImgName { get; set;} 
	public int EventName {get;set;}
}
