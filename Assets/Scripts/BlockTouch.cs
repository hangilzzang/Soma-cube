using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockTouch : MonoBehaviour
{
    public ScrollRect scrollRect;
    public void DisableScroll()
    {
        scrollRect.horizontal = false;
    }

    public void AbleScroll()
    {
        scrollRect.horizontal = true;
    }
}

