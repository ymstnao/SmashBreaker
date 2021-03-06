﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmashGage : MonoBehaviour
{
    public float max;//最大値

    private float sp;//スマッシュポイント
    private Slider slider;
    private Player player;

    // Use this for initialization
    void Start()
    {
        slider = GetComponent<Slider>();
        player = GameObject.Find("Chara").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        SmashPoint();
    }

    /// <summary>
    /// プレイヤーのスマッシュポイントを表示
    /// </summary>
    private void SmashPoint()
    {
        sp = player.GetSP;
        slider.value = sp / max;

        if (sp >= max)
        {
            player.ChangeHp(1);
        }
    }
}
