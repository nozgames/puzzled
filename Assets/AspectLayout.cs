using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class AspectLayout : LayoutGroup
    {
        private float height = 0.0f;
        public override void CalculateLayoutInputVertical()
        {
            SetLayoutInputForAxis(0.0f, rectTransform.rect.height, -1, 1);

            if(height != rectTransform.rect.height)
            {
                height = rectTransform.rect.height;
                SetDirty();                
            }
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            SetLayoutInputForAxis(0.0f, 16.0f * rectTransform.rect.height / 9.0f, -1, 0);
        }

        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
        }
    }
}
