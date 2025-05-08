using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prismify.Toolkit{
    public abstract class UIEffect_Base : MonoBehaviour
    {
        public Action OnShow, OnHide;
        public abstract void Show();

        public abstract void Hide();
    }
}
