using System.Data.Common;
using UnityEngine;

public class EnemyBodyType : MonoBehaviour
{
    public EnemyBodyEnum bodyType;

    private float damageMultiplier;

    public float DamageMultiplyer()
    {
            switch (bodyType)
            {
                case EnemyBodyEnum.head:
                    damageMultiplier = 2.5f;
                break;
                case EnemyBodyEnum.torso:
                    damageMultiplier = 1.0f;
                break;
                case EnemyBodyEnum.upperarm:
                    damageMultiplier = 0.75f;
                break;
                case EnemyBodyEnum.forearm:
                   damageMultiplier = 0.6f;
                break;
                case EnemyBodyEnum.thigh:
                    damageMultiplier = 0.75f;
                break;
                case EnemyBodyEnum.loweleg:
                    damageMultiplier = 0.6f;
                break;
                case EnemyBodyEnum.other:
                    damageMultiplier = 0.5f;
                break;
                default:
                    damageMultiplier = 1.0f;
                break;
        }

        return damageMultiplier;
    }
}
