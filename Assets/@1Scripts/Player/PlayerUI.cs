using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Event 등록을 통한 UI 갱신
public class PlayerUI : MonoBehaviour
{
    [Header("Internal Object")]
    public Slider hpSlider;
    public Slider staminaSlider;
    [Header("Extern Object")]
    public Player player;
    public Image knifeImage;

    void Awake()
    {
        //Event 등록
        player.hpEvent += HPChanged;
        player.staminaEvent += StaminaChanged;
        player.knifeCoolEvent += KnifeCoolChanged;
    }

    public void HPChanged(float hp)
    {
        hpSlider.value = hp;
    }
    public void StaminaChanged(float stamina)
    {
        staminaSlider.value = stamina;
    }
    
    public void KnifeCoolChanged(float size)
    {
        //size: 0~1
        if (size < 0)
            return;
        knifeImage.rectTransform.localScale = new Vector3(1,size,1);
    }
}
