﻿using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIChoosePack : UIScreen
    {
        [SerializeField] private Transform content = null;
        [SerializeField] private PuzzlePack[] packs = null;
        [SerializeField] private GameObject packButtonPrefab = null;

        private void OnEnable()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            foreach (var pack in packs)
            {
                var go = Instantiate(packButtonPrefab, content);
                go.GetComponent<UIPuzzlePackButton>().puzzlePack = pack;
            }
        }
    }
}
