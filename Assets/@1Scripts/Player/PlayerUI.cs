using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Player player;
    public Slider hpSlider;
    public Slider staminaSlider;
    public Image knifeImage;
    void Awake()
    {
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
        knifeImage.rectTransform.localScale = new Vector3(1,size,1);
    }
}
