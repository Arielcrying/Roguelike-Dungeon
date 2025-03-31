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
    [Tooltip("��С�ƶ��ٶȡ�GetMoveSpeed ����������С�ƶ��ٶȺ�����ƶ��ٶ�֮���������ֵ")]
    #endregion Tooltip
    public float minMoveSpeed = 8f;


    #region Tooltip
    [Tooltip("����ƶ��ٶȡ�GetMoveSpeed ����������С�ƶ��ٶȺ�����ƶ��ٶ�֮���������ֵ")]
    #endregion Tooltip
    public float maxMoveSpeed = 8f;

    #region Tooltip
    [Tooltip("��ҹ����ٶ�")]
    #endregion Tooltip
    public float rollSpeed;

    #region Tooltip
    [Tooltip("��ҹ�������")]
    #endregion Tooltip
    public float rollDistance;

    #region Tooltip
    [Tooltip("��ҹ�����ȴʱ��")]
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
