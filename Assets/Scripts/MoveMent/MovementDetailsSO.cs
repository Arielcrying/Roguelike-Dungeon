using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementDetails_", menuName = "Scriptable Objects/Movement/MovementDetails")]

public class MovementDetailsSO : ScriptableObject
{
    #region Header MOVEMENT DETAILS
    [Space(10)]
    [Header("MOVEMENT DETAILS")]
    #endregion Header
    #region Tooltip
    [Tooltip("最小移动速度。GetMoveSpeed 方法将在最小移动速度和最大移动速度之间生成随机值")]
    #endregion Tooltip
    public float minMoveSpeed = 8f;


    #region Tooltip
    [Tooltip("最大移动速度。GetMoveSpeed 方法将在最小移动速度和最大移动速度之间生成随机值")]
    #endregion Tooltip
    public float maxMoveSpeed = 8f;

    #region Tooltip
    [Tooltip("玩家滚动速度")]
    #endregion Tooltip
    public float rollSpeed;

    #region Tooltip
    [Tooltip("玩家滚动距离")]
    #endregion Tooltip
    public float rollDistance;

    #region Tooltip
    [Tooltip("玩家滚动冷却时间")]
    #endregion Tooltip
    public float rollCooldownTime;



    public float GetMoveSpeed()
    {
        if (minMoveSpeed == maxMoveSpeed)
        {
            return minMoveSpeed;
        }
        else
        {
            return Random.Range(minMoveSpeed, maxMoveSpeed);
        }
    }


    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(minMoveSpeed), minMoveSpeed, nameof(maxMoveSpeed), maxMoveSpeed, false);
        if (rollDistance != 0f || rollSpeed != 0 || rollCooldownTime != 0)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollDistance), rollDistance, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollSpeed), rollSpeed, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(rollCooldownTime), rollCooldownTime, false);
        }
    }

#endif
    #endregion Validation
}
