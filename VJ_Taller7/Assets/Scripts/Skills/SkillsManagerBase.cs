using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillsManagerBase : MonoBehaviour
{
    protected Image _actSkillImg;
    protected Image _pasSkillImg;
    protected Image _actSkillMask;
    protected Image _pasSkillMask;

    private void Start()
    {
        SearchSkillsUI();
    }
    public void SearchSkillsUI()
    {
        _actSkillImg = GameObject.Find("Active_Skill_Img").GetComponent<Image>();
        _actSkillMask = GameObject.Find("Active_Skill_Mask").GetComponent<Image>();
        _pasSkillImg = GameObject.Find("Passive_Skill_Img").GetComponent<Image>();
        _pasSkillMask = GameObject.Find("Passive_Skill_Mask").GetComponent<Image>();
    }

    public IEnumerator DecreaseActiveSkillMask(float cooldown)
    {
        _actSkillMask.fillAmount = 1;

        float epsilon = 0f;

        while (epsilon < cooldown)
        {
            epsilon += Time.deltaTime;
            float t = Mathf.Clamp01(epsilon / cooldown);

            _actSkillMask.fillAmount = Mathf.Lerp(1, 0, t);
            yield return null;
        }

        _actSkillMask.fillAmount = 0;
        yield return null;
    }

    public IEnumerator DecreasePassiveSkillMask(float cooldown)
    {
        if (cooldown > 0)
        {
            _pasSkillMask.fillAmount = 1;

            float epsilon = 0f;

            while (epsilon < cooldown)
            {
                epsilon += Time.deltaTime;
                float t = Mathf.Clamp01(epsilon / cooldown);

                _pasSkillMask.fillAmount = Mathf.Lerp(1, 0, t);
                yield return null;
            }
        }

        _pasSkillMask.fillAmount = 0;
        yield return null;
    }
}
