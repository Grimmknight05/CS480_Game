using UnityEngine;


public interface ILock
{

    bool LockState { get; set; }
    

    void Lock(bool lockstate);
}
