using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles curved lerp movement of answer boxes from spawn to endpoint,
/// and color feedback (green/red) on the selected answer.
/// Attach to any GameObject in the scene and reference from DictionaryManager.
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

    private readonly List<Coroutine> _lerpCoroutines = new List<Coroutine>();

    /// <summary>
    /// Spawn each answer box at its paired start point and curved-lerp it to its end point.
    /// Only stops previous lerp coroutines — flash coroutines are left untouched.
    /// </summary>
    public void AnimateAnswers(Transform[] boxes)
    {
        // Stop only lerp coroutines, not flash coroutines
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
    /// Flash the hit answer green or red for 0.75s, then destroy the choice.
    /// </summary>
    public void ShowFeedback(TextMeshProUGUI hitText, bool isCorrect, GameObject choiceRoot = null)
    {
        StartCoroutine(FlashColorRoutine(hitText, isCorrect, choiceRoot));
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

            // Smoothstep ease in-out
            float smoothT = t * t * (3f - 2f * t);

            // Quadratic bezier: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
            float oneMinusT = 1f - smoothT;
            Vector3 pos = oneMinusT * oneMinusT * start
                        + 2f * oneMinusT * smoothT * controlPoint
                        + smoothT * smoothT * end;

            target.position = pos;
            yield return null;
        }

        target.position = end;
    }

    private IEnumerator FlashColorRoutine(TextMeshProUGUI text, bool isCorrect, GameObject choiceRoot)
    {
        Color originalColor = text.color;
        text.color = isCorrect ? _correctColor : _incorrectColor;

        yield return new WaitForSeconds(_flashDuration);

        if (choiceRoot != null)
            Destroy(choiceRoot);
        else
            text.color = originalColor;
    }
}
