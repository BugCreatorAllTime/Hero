using strange.extensions.context.impl;
using UnityEngine;
using System.Collections;

public class WorldMapBootstrap : ContextView {
    void Awake()
    {
        context = new WorldMapContext(this);
    }
}
