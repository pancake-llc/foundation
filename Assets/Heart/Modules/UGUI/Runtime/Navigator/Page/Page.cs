﻿using UnityEngine;

namespace Pancake.UI
{
    public abstract class Page : UIContext
    {
        [field: SerializeField] public bool IsRecycle { get; private set; } = true;
    }
}