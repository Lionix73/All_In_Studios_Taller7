using System.Collections;
using UnityEngine;

public interface IPassiveSkill
{
    void CheckCondition(); // Se llama regularmente o por evento
    IEnumerator Execute(); // Lógica al activarse
    bool IsOnCooldown { get; }
}

