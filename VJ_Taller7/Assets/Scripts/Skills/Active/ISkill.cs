using System.Collections;
using UnityEngine;

public interface ISkill
{
    void Activate();
    IEnumerator Execute();
    bool IsOnCooldown { get; }
}

public interface IMultiSkill
{
    void Activate();
    IEnumerator Execute();
    bool IsOnCooldown { get; }
}