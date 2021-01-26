using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    class UICameraOffsetEditor : UIPropertyEditor
    {
        [SerializeField] private Button _button = null;

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();
        }
    }
}
