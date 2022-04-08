using UnityEngine;
using System.Collections;
using System;

[AttributeUsage(AttributeTargets.Property, 
                AllowMultiple = false,
                Inherited = true)]
public sealed class NGUITag : Attribute 
{
	public string name;
	public NGUITag (string name) {
		this.name = name;
	}

	public NGUITag() {

	}
}
