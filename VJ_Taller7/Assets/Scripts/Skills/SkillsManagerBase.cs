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

    public void SetupShaderMaterial(Image img, Sprite skillSprite)
    {
        img.sprite = skillSprite;

        Texture2D skillTxt = skillSprite.texture;
        img.material = new Material(img.material);
        img.material.SetTexture("_Texture", skillTxt);
    }

    public IEnumerator DecreaseActiveSkillMask(float cooldown)
    {
        _actSkillImg.material.SetFloat("_WaveSpeed", 0f);

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
        _actSkillImg.material.SetFloat("_WaveSpeed", 2f);
        yield return null;
    }

    public IEnumerator DecreasePassiveSkillMask(float cooldown)
    {
        _pasSkillImg.material.SetFloat("_WaveSpeed", 0f);
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
        _pasSkillImg.material.SetFloat("_WaveSpeed", 2f);
        yield return null;
    }
}
