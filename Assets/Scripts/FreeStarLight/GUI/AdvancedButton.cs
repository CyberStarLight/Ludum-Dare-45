using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class AdvancedButton : Button {

    //public new bool interactable
    //{
    //    get { return base.interactable; }
    //    set { SetInteractable(value); base.interactable = value; }
    //}

    public void SetInteractable(bool isInteractable)
    {
        if (isInteractable != interactable)
            interactable = isInteractable;
    }

    private bool currentlyInterabtable;

    private void _setInteractable(bool isInteractable)
    {
        if (currentlyInterabtable == isInteractable)
            return;
        else
            currentlyInterabtable = isInteractable;

        var images = GetComponentsInChildren<Image>();
        foreach (var image in images)
        {
            if(image.gameObject == gameObject)
                image.color = isInteractable ? this.colors.normalColor : this.colors.disabledColor;
            else
                image.color = isInteractable ? new Color(image.color.r, image.color.g, image.color.b, this.colors.normalColor.a) : new Color(image.color.r, image.color.g, image.color.b, this.colors.disabledColor.a);
        }

        var tmpTexts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in tmpTexts)
        {
            text.color = isInteractable ? new Color(text.color.r, text.color.g, text.color.b, this.colors.normalColor.a) : new Color(text.color.r, text.color.g, text.color.b, this.colors.disabledColor.a);
        }
    }

    protected virtual IEnumerator TweenColorFromCurrent(Color ToColor, float duration)
    {
        var images = GetComponentsInChildren<Image>();
        for (float f = 0; f <= duration; f = f + Time.deltaTime)
        {
            foreach (var image in images)
            {
                var targetColor = Color.Lerp(image.color, ToColor, f);

                if (image.gameObject == gameObject)
                    image.color = targetColor;
                else
                    image.color = new Color(image.color.r, image.color.g, image.color.b, targetColor.a);
            }
            yield return null;
        }

        foreach (var image in images)
        {
            if (image.gameObject == gameObject)
                image.color = ToColor;
        }
    }
    
    private void InstantColorFromCurrent(Color ToColor)
    {
        var images = GetComponentsInChildren<Image>();
        foreach (var image in images)
        {
            if (image.gameObject == gameObject)
                image.color = ToColor;
            else
                image.color = new Color(image.color.r, image.color.g, image.color.b, ToColor.a);
        }
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        //if (forceNoInteraction)
        //    return;

        if (state == SelectionState.Pressed)
        {
            StopAllCoroutines();
            if (gameObject.activeInHierarchy)
                StartCoroutine(TweenColorFromCurrent(this.colors.pressedColor, this.colors.fadeDuration));
            else
                InstantColorFromCurrent(this.colors.pressedColor);
        }
        else if (state == SelectionState.Highlighted)
        {
            //StopAllCoroutines();

            if (gameObject.activeInHierarchy)
                StartCoroutine(TweenColorFromCurrent(this.colors.highlightedColor, this.colors.fadeDuration));
            else
                InstantColorFromCurrent(this.colors.highlightedColor);
        }
        else if (state == SelectionState.Normal)
        {
            _setInteractable(true);
            StopAllCoroutines();

            if (gameObject.activeInHierarchy)
                StartCoroutine(TweenColorFromCurrent(this.colors.normalColor, this.colors.fadeDuration));
            else
                InstantColorFromCurrent(this.colors.normalColor);
        }
        else if (state == SelectionState.Disabled)
        {
            StopAllCoroutines();
            _setInteractable(false);
        }
    }
}
