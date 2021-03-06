﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected float shootSpeed;//吹き飛ぶ速度
    protected GameObject player;//プレイヤー
    protected Vector3 playerVec;//プレイヤーの方向
    protected Vector3 lookPos;//見る方向
    protected bool isStan;//気絶フラグ

    private MainCamera camera;//カメラ
    private Vector3 size;//サイズ

    // Use this for initialization
    public virtual void Initialize()
    {
        camera = GameObject.Find("Main Camera").GetComponent<MainCamera>();
        player = GameObject.Find("Chara");//プレイヤーを探す
        isStan = false;
        size = transform.localScale;
    }

    // Update is called once per frame
    public virtual void EnemyUpdate()
    {
        lookPos = player.transform.position;//向く方向の座標
        playerVec = (lookPos - transform.position).normalized;//向く方向を正規化
        Dead();//消滅
    }

    /// <summary>
    /// 消滅
    /// </summary>
    private void Dead()
    {
        if (!isStan) return;

        Vector3 pos = transform.position;
        Vector3 screenMinPos = camera.ScreenMin;//画面の左下の座標
        Vector3 screenMaxPos = camera.ScreenMax;//画面の右下の座標

        //画面外に出たら消滅
        if (pos.x <= screenMinPos.x - size.x / 2 || pos.x >= screenMaxPos.x + size.x / 2
            || pos.y <= screenMinPos.y - size.y / 2 || pos.y >= screenMaxPos.y + size.y / 2)
        {
            camera.SetShake();//画面振動
            Destroy(gameObject);//消滅
        }
    }

    /// <summary>
    /// 吹き飛ぶ
    /// </summary>
    public virtual void Shoot(GameObject col)
    {
        if (isStan) return;

        Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        GetComponent<BoxCollider2D>().isTrigger = true;//あたり判定のトリガーオン
        rigid.AddForce(-playerVec * shootSpeed, ForceMode2D.Impulse);//後ろに吹き飛ぶ
        isStan = true;//気絶フラグtrue
        player.GetComponent<Player>().AddSP(1);//プレイヤーのスマッシュポイント加算
        //player.GetComponent<Player>().SetBack();//プレイヤー後退開始
        Time.timeScale = 0.0f;//ゲーム停止
    }

    /// <summary>
    /// 気絶フラグ
    /// </summary>
    public bool IsStan
    {
        get { return isStan; }
    }


    public virtual void TriggerEnter(Collider2D col) { }

    /// <summary>
    /// あたり判定
    /// </summary>
    void OnTriggerEnter2D(Collider2D col)
    {
        TriggerEnter(col);
    }

}
