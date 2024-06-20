using System;
using UnityEngine;
using UnityEngine.UI;

namespace Utils {
public class ButtonGroupExclusiveSelected : MonoBehaviour
{
    /*
     * Assumptions/requirements:
     * - the name of the button, with 'btn' removed, and in lowercase, should match the enum name, or at least,
     *   the name passed into ShowButtonSelected(string buttonName) , which will typically be the enum name
     *   used in the inspector
     */
    [SerializeField] Button defaultButton;

    public void ShowButtonSelected(string buttonName) {
        var foundButton = false;
        foreach(var child in GetComponentsInChildren<Button>(true)) {
            var isTarget = child.name.ToLower().Replace("btn", "") == buttonName.ToLower();
            ShowButtonSelected(child, isTarget);
            if (isTarget) foundButton = true;
        }
        if (!foundButton) {
            throw new Exception($"Could not find button {buttonName}. You might need to check the enum or similar.");
        }
    }

    void OnEnable()
    {
        Button toClick = null;
        foreach(var child in GetComponentsInChildren<Button>()) {
            ShowButtonSelected(child, child == toClick);
            if(child == defaultButton) {
                toClick = child;
            }
            child.onClick.AddListener(() =>
                OnClickButton(child));
        }
        if(toClick != null) {
            toClick.onClick.Invoke();
        }
    }

    void ShowButtonSelected(Button button, bool selected) {
        var colors = button.colors;
        if(selected) {
            colors.normalColor = new Color(1f, 1f, 1f);
            colors.pressedColor = new Color(1f, 1f, 1f);
            colors.selectedColor = new Color(1f, 1f, 1f);
            colors.highlightedColor = new Color(1f, 1f, 1f);
        } else {
            colors.normalColor = new Color(0.5f, 0.5f, 0.5f);
            colors.pressedColor = new Color(0.5f, 0.5f, 0.5f);
            colors.selectedColor = new Color(0.5f, 0.5f, 0.5f);
            colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f);
        }
        button.colors = colors;
    }

    void OnClickButton(Button btn) {
        foreach(var child in GetComponentsInChildren<Button>()) {
            ShowButtonSelected(child, child == btn);
        }
    }

}
}
