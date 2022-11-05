using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.GameService
{
    public class HandleIconFacebook : MonoBehaviour
    {
        public Image imgFacebook;
        public Color colorUnslect;
        public Color colorSlected;

        public void Select() { imgFacebook.color = colorSlected; }

        public void DeSelect() { imgFacebook.color = colorUnslect; }
    }
}