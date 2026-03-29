using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles curved lerp movement of answer boxes from spawn to endpoint,
/// color feedback on all choices, and spin transition on the toFind display.
/// </summary>
public class AnswerLerpManager : MonoBehaviour
{
    [Serializable]
    public struct LerpPath
    {
        public Transform startPoint;
        public Transform endPoint;
    }

    [Header("Paths (one per answer box)")]
    [SerializeField] private LerpPath[] _paths = new LerpPath[4];

    [Header("Lerp Settings")]
    [SerializeField] private float _lerpDuration = 1f;
    [SerializeField] private float _curveHeight = 1.5f;

    [Header("Feedback Settings")]
    [SerializeField] private float _flashDuration = 0.75f;
    [SerializeField] private Color _correctColor = Color.green;
    [SerializeField] private Color _incorrectColor = Color.red;

    public int PathCount => _paths.Length;
    public float FlashDuration => _flashDuration;

    private readonly List<Coroutine> _lerpCoroutines = new List<Coroutine>();
    private Coroutine _spinCoroutine;
    private Quaternion _toFindRestRotation;
    private bool _toFindRestCaptured;

    /// <summary>
    /// Spawn each answer box at its paired start point and curved-lerp it to its end point.
    /// Only stops previous lerp coroutines — flash/spin coroutines are left untouched.
    /// </summary>
    public void AnimateAnswers(Transform[] boxes)
    {
        foreach (Coroutine c in _lerpCoroutines)
        {
            if (c != null)
                StopCoroutine(c);
        }
        _lerpCoroutines.Clear();

        int count = Mathf.Min(boxes.Length, _paths.Length);
        for (int i = 0; i < count; i++)
        {
            Vector3 start = _paths[i].startPoint.position;
            Vector3 end = _paths[i].endPoint.position;
            boxes[i].position = start;
            _lerpCoroutines.Add(StartCoroutine(CurvedLerpRoutine(boxes[i], start, end)));
        }
    }

    /// <summary>
    /// Flash ALL choices: correct one goes green, others go red.
    /// All are destroyed together after flashDuration.
    /// </summary>
    public void ShowAllFeedback(List<GameObject> allChoices, string correctTag)
    {
        StartCoroutine(FlashAllRoutine(allChoices, correctTag));
    }

    /// <summary>
    /// Spin the toFind transform 360 degrees over lerpDuration,
    /// then set the new text at the end.
    /// </summary>
    public void SpinToFindDisplay(Transform toFindTransform, TextMeshProUGUI toFindText, string newText)
    {
        // Capture the rest rotation once, on the very first call
        if (!_toFindRestCaptured)
        {
            _toFindRestRotation = toFindTransform.localRotation;
            _toFindRestCaptured = true;
        }

        // Stop any in-progress spin and snap to rest before starting a new one
        if (_spinCoroutine != null)
        {
            StopCoroutine(_spinCoroutine);
            toFindTransform.localRotation = _toFindRestRotation;
        }

        _spinCoroutine = StartCoroutine(SpinRoutine(toFindTransform, toFindText, newText));
    }

    private IEnumerator CurvedLerpRoutine(Transform target, Vector3 start, Vector3 end)
    {
        Vector3 midpoint = (start + end) * 0.5f;
        Vector3 controlPoint = midpoint + Vector3.up * _curveHeight;

        float elapsed = 0f;

        while (elapsed < _lerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _lerpDuration);

            float smoothT = t * t * (3f - 2f * t);

            float oneMinusT = 1f - smoothT;
            Vector3 pos = oneMinusT * oneMinusT * start
                        + 2f * oneMinusT * smoothT * controlPoint
                        + smoothT * smoothT * end;

            target.position = pos;
            yield return null;
        }

        target.position = end;
    }

    private IEnumerator FlashAllRoutine(List<GameObject> choices, string correctTag)
    {
        // Color all choices
        foreach (GameObject choice in choices)
        {
            if (choice == null) continue;

            TextMeshProUGUI tmp = choice.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.color = choice.CompareTag(correctTag) ? _correctColor : _incorrectColor;
        }

        yield return new WaitForSeconds(_flashDuration);

        // Destroy all at once
        foreach (GameObject choice in choices)
        {
            if (choice != null)
                Destroy(choice);
        }
    }

    private IEnumerator SpinRoutine(Transform target, TextMeshProUGUI text, string newText)
    {
        if (target == null) yield break;

        float elapsed = 0f;

        while (elapsed < _lerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _lerpDuration);

            // Always spin relative to the rest rotation
            target.localRotation = _toFindRestRotation * Quaternion.Euler(0f, 0f, 360f * t);

            // Swap text at the halfway point when it's edge-on
            if (t >= 0.5f && text != null && text.text != newText)
                text.text = newText;

            yield return null;
        }

        // Snap back to rest
        target.localRotation = _toFindRestRotation;
        if (text != null)
            text.text = newText;

        _spinCoroutine = null;
    }
}
