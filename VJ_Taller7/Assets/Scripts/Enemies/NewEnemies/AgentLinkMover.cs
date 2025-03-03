using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using System;

public enum OffMeshLinkMoveMethod
{
    Teleport,
    NormalSpeed,
    Parabola,
    Curve
}

[RequireComponent(typeof(NavMeshAgent))]
public class AgentLinkMover : MonoBehaviour
{
    public OffMeshLinkMoveMethod defaultMoveMethod = OffMeshLinkMoveMethod.Parabola;
    public AnimationCurve m_Curve = new AnimationCurve();
    public float parabolaHeight = 8.0f;
    public delegate void LinkEvent(OffMeshLinkMoveMethod m_Method);
    public LinkEvent OnLinkStart;
    public LinkEvent OnLinkEnd;
    public List<LinkTraversalConfig> linkTraversalConfigs = new List<LinkTraversalConfig>();

    IEnumerator Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;
        while (true)
        {
            if (agent.isOnOffMeshLink)
            {
                OffMeshLinkData data = agent.currentOffMeshLinkData;
                if (agent.navMeshOwner is NavMeshLink link)
                {
                    int areaType = link.area;

                    if(Vector3.Distance(data.endPos, agent.destination) < Vector3.Distance(data.startPos, agent.destination)){
                    
                        LinkTraversalConfig config = linkTraversalConfigs.Find((type) => type.areaType == areaType);

                        if ((config != null && config.m_Method == OffMeshLinkMoveMethod.NormalSpeed) || (config == null && defaultMoveMethod == OffMeshLinkMoveMethod.NormalSpeed)){
                            OnLinkStart?.Invoke(OffMeshLinkMoveMethod.NormalSpeed);
                            yield return StartCoroutine(MoveNormalSpeed(agent));
                            OnLinkEnd?.Invoke(OffMeshLinkMoveMethod.NormalSpeed);
                        }
                        else if ((config != null && config.m_Method == OffMeshLinkMoveMethod.Parabola) || (config == null && defaultMoveMethod == OffMeshLinkMoveMethod.Parabola)){
                            OnLinkStart?.Invoke(OffMeshLinkMoveMethod.Parabola);
                            yield return StartCoroutine(MoveParabola(agent, parabolaHeight, Vector3.Distance(data.startPos, data.endPos) / agent.speed));
                            OnLinkEnd?.Invoke(OffMeshLinkMoveMethod.Parabola);
                        }
                        else if (config != null && config.m_Method == OffMeshLinkMoveMethod.Curve || (config == null && defaultMoveMethod == OffMeshLinkMoveMethod.Curve)){
                            OnLinkStart?.Invoke(OffMeshLinkMoveMethod.Curve);
                            yield return StartCoroutine(MoveCurve(agent, Vector3.Distance(data.startPos, data.endPos) / agent.speed));
                            OnLinkEnd?.Invoke(OffMeshLinkMoveMethod.Curve);
                        }
                        else if((config != null && config.m_Method == OffMeshLinkMoveMethod.Teleport) || (config == null && defaultMoveMethod == OffMeshLinkMoveMethod.Teleport)){
                            OnLinkStart?.Invoke(OffMeshLinkMoveMethod.Teleport);
                            OnLinkEnd?.Invoke(OffMeshLinkMoveMethod.Teleport);
                        }
                    }
                }

                agent.CompleteOffMeshLink();
            }
            yield return null;
        }
    }

    IEnumerator MoveNormalSpeed(NavMeshAgent agent)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        while (agent.transform.position != endPos)
        {
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator MoveParabola(NavMeshAgent agent, float height, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = height * (normalizedTime - normalizedTime * normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }

    IEnumerator MoveCurve(NavMeshAgent agent, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = m_Curve.Evaluate(normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
    }

    [Serializable] 
    public class LinkTraversalConfig{
        public OffMeshLinkMoveMethod m_Method;
        public int areaType;
    }
}