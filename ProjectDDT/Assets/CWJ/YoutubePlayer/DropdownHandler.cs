using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.MUIP;
using System.Linq;
using UnityEngine.Events;

public class DropdownHandler : MonoBehaviour
{
    [SerializeField] private Michsky.MUIP.CustomDropdown dropdown;
    public string initName = "전체";

    [TextArea]
    public string itemNameParamsStr;
    public string[] curItemNamesCache;

    private void OnValidate()
    {
        if (dropdown && !string.IsNullOrEmpty( initName))
        {
            if (dropdown.selectedText.text != initName)
                dropdown.selectedText.SetText(initName);
        }
    }

    private void Reset()
    {
        if(dropdown == null)
            dropdown = GetComponent<Michsky.MUIP.CustomDropdown>();
    }

    private void Start()
    {
        if (!string.IsNullOrWhiteSpace((itemNameParamsStr)))
        {
            UpdateItemNamesByParamStr(itemNameParamsStr);
            itemNameParamsStr = null;
        }
        else if (curItemNamesCache.Length > 0)
            UpdateItemNames(curItemNamesCache);
        else if (initName != null)
            UpdateItemNamesByParamStr(initName);
    }

    void UpdateItemNamesByParamStr(string param)
    {
        if (string.IsNullOrWhiteSpace(param))
        {
            return;
        }
        UpdateItemNames(param.Split(','));
    }

    void UpdateItemNames(string[] names)
    {
        if (names == null)
        {
            return;
        }
        dropdown.Interactable(false);
        dropdown.items.Clear();
        int startIndex = 0;
        if (names.Length>0 && names[0] != initName)
        {
            dropdown.items.Add(new CustomDropdown.Item
                               {
                                   itemIndex = 0,
                                   itemName = initName,
                                   itemIcon = null
                               });
            startIndex = 1;
        }

        dropdown.items.AddRange(names.Select((s, i) => new CustomDropdown.Item
                                                       {
                                                           itemIndex = startIndex + i,
                                                           itemName = s,
                                                           itemIcon = null
                                                       }));
        dropdown.SetupDropdown();
        dropdown.SetDropdownIndex(0, true);
        dropdown.Interactable(true);
        curItemNamesCache = dropdown.items.Select(o => o.itemName).ToArray();
    }
}
