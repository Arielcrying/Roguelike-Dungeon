using UnityEngine;

//����������,Tֻ����MonoBehaviour��������
public abstract class SingletonMonobehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public  static T Instance
    {
        get { return instance; }
    }

    protected virtual void Awake()
    {
        if(Instance == null)
        {
            instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
