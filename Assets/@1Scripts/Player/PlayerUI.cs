using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Player player;
    public Slider hpSlider;
    public Slider staminaSlider;
    void Awake()
    {
        player.hpEvent += HPChanged;
        player.staminaEvent += StaminaChanged;
    }

    public void HPChanged(float hp)
    {
        hpSlider.value = hp;
    }
    public void StaminaChanged(float stamina)
    {
        staminaSlider.value = stamina;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
