using DG.Tweening;
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

    public void ActivateMask(bool isActiveSkill, float cooldown)
    {
        if(isActiveSkill)
        {
            StartCoroutine(DecreaseSkillMask(cooldown, _actSkillImg, _actSkillMask));
            StartCoroutine(SkillReloadEffects(cooldown, _actSkillImg));
        }
        else
        {
            StartCoroutine(DecreaseSkillMask(cooldown, _pasSkillImg, _pasSkillMask));
            StartCoroutine(SkillReloadEffects(cooldown, _pasSkillImg));
        }
    }

    public IEnumerator DecreaseSkillMask(float cooldown, Image img, Image mask)
    {
        mask.fillAmount = 1;

        float epsilon = 0f;

        while (epsilon < cooldown)
        {
            epsilon += Time.deltaTime;
            float t = Mathf.Clamp01(epsilon / cooldown);

            mask.fillAmount = Mathf.Lerp(1, 0, t);
            yield return null;
        }

        mask.fillAmount = 0;
    }

    private IEnumerator SkillReloadEffects(float cooldown, Image img)
    {
        img.material.SetFloat("_WaveSpeed", 0f); // The sinny effect do not activate

        yield return new WaitForSeconds(cooldown);

        img.material.SetFloat("_WaveSpeed", 2f); // Activate the sinny effect

        img.transform.parent.DOScale(1.15f, 0.5f); // Scale the skill frame and icon
        yield return new WaitForSeconds(0.5f);
        img.transform.parent.DOScale(1, 0.5f); // Return to original scale

        yield return new WaitForSeconds(3.8f);
        img.material.SetFloat("_WaveSpeed", 0f);
    }
}
