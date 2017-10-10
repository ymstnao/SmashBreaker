﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;//移動速度
    public int hp;//体力
    public GameObject smashCol;//攻撃のあたり判定

    private MainCamera camera;//カメラ
    private Animator anim;//アニメーション
    private Vector3 size;//大きさ
    private Vector3 attackColSize;//攻撃あたり判定の大きさ
    private Vector3 iniAttackPos;//攻撃あたり判定の初期位置
    private float x_axis;//横の入力値
    private float y_axis;//縦の入力値
    private bool isSmash;//攻撃フラグ
    private bool isDamage;//ダメージ

    //↓仮変数（後で使わなくなるかも）
    private int flashCnt;//点滅カウント

    /// <summary>
    /// 状態を表す列挙型
    /// </summary>
    private enum State
    {
        IDEL,//待機
        MOVE,//移動
        ATTACK,//攻撃
        DEAD,//死亡
    }
    private State state;//状態

    // Use this for initialization
    void Start()
    {
        camera = GameObject.Find("Main Camera").GetComponent<MainCamera>();
        size = transform.localScale;//大きさ取得
        anim = GetComponent<Animator>();
        isSmash = false;//攻撃フラグfalse
        state = State.IDEL;//最初は待機状態
        //あたり判定の大きさを体力に合わせて変える
        attackColSize = smashCol.transform.localScale;
        iniAttackPos = smashCol.transform.localPosition;
        ChangeHp(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (state != State.DEAD)
        {
            Smash();//攻撃
            Move();//移動
            Rotate();//向き変更
            Damage();//ダメージ演出
        }
        Animation();//アニメーションの制御
        Clamp();//移動制限
    }

    /// <summary>
    /// 移動制限
    /// </summary>
    private void Clamp()
    {
        Vector3 screenMinPos = camera.ScreenMin();//画面の左下の座標
        Vector3 screenMaxPos = camera.ScreenMax();//画面の右下の座標

        //座標を画面内に制限
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, screenMinPos.x + size.x / 2, screenMaxPos.x - size.x / 2);
        pos.y = Mathf.Clamp(pos.y, screenMinPos.y + size.y / 2, screenMaxPos.y - size.y / 2);
        transform.position = pos;
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        if (state == State.ATTACK) return;

        x_axis = Input.GetAxisRaw("Horizontal");
        y_axis = Input.GetAxisRaw("Vertical");

        float vx = 0.0f;
        float vy = 0.0f;
        state = State.IDEL;

        if (x_axis >= 0.5f || x_axis <= -0.5f)
        {
            vx = x_axis * speed * Time.deltaTime;//横移動速度
            state = State.MOVE;
        }
        if (y_axis >= 0.5f || y_axis <= -0.5f)
        {
            vy = y_axis * speed * Time.deltaTime;//縦移動速度
            state = State.MOVE;
        }

        transform.Translate(new Vector2(vx, vy), Space.World);
    }

    /// <summary>
    /// 向きを変更
    /// </summary>
    private void Rotate()
    {
        if (state != State.MOVE) return;

        Vector3 lookPos = new Vector3(transform.position.x + x_axis * -1, transform.position.y + y_axis * -1, 0);//向く方向の座標
        Vector3 vec = (lookPos - transform.position).normalized;//向く方向を正規化
        float angle = (Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg) - 90.0f;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);//入力された方向に向く
    }

    /// <summary>
    /// アニメーションの制御
    /// </summary>
    private void Animation()
    {
        var animState = anim.GetCurrentAnimatorStateInfo(0);

        //移動柱なら移動アニメーション
        if (state == State.MOVE)
        {
            anim.SetBool("Run", true);
        }
        else
        {
            anim.SetBool("Run", false);
        }

        //攻撃柱なら
        if (state == State.ATTACK)
        {
            //攻撃アニメーションが終わったら
            if (animState.IsName("Player_Smash"))
            {
                isSmash = true;
                if (animState.normalizedTime >= 0.7f)
                {
                    smashCol.SetActive(true);//攻撃のあたり判定をアクティブ化
                }
                if (animState.normalizedTime >= 1.0f)
                {
                    smashCol.SetActive(false);//攻撃のあたり判定を非アクティブ化
                }
            }
            //待機状態に戻ったら
            if (animState.IsName("Player_Idle") && isSmash)
            {
                isSmash = false;//攻撃フラグをfalseに
                state = State.IDEL;//待機状態に
            }
        }
    }

    /// <summary>
    /// 攻撃
    /// </summary>
    private void Smash()
    {
        if (state == State.ATTACK) return;

        //攻撃入力がされたら
        if (Input.GetButtonDown("Smash"))
        {
            anim.SetTrigger("Smash");//攻撃アニメーション開始
            x_axis = 0.0f;
            y_axis = 0.0f;
            state = State.ATTACK;//攻撃状態に
        }
    }

    /// <summary>
    /// ダメージ演出
    /// </summary>
    private void Damage()
    {
        if (!isDamage) return;

        SpriteRenderer texture = GetComponent<SpriteRenderer>();
        Color color = texture.color;
        flashCnt += 1;
        color.a = (flashCnt / 5) % 2;
        if (flashCnt >= 60)
        {
            color.a = 1;
            flashCnt = 0;
            isDamage = false;
        }
        texture.color = color;
    }

    /// <summary>
    /// 体力回復
    /// </summary>
    public void ChangeHp(int h)
    {
        hp += h;
        hp = Mathf.Clamp(hp, 0, 5);
        smashCol.transform.localScale = new Vector3(attackColSize.x * hp, attackColSize.y * hp, 1);
        Vector3 attackPos = smashCol.transform.localPosition;
        attackPos.y = iniAttackPos.y - attackColSize.y * hp;
        smashCol.transform.localPosition = attackPos;
        smashCol.SetActive(false);
    }

    /// <summary>
    /// あたり判定
    /// </summary>
    /// <param name="col"></param>
    void OnCollisionStay2D(Collision2D col)
    {
        //敵に当たったらダメージ
        if (col.transform.tag == "Enemy")
        {
            if (hp > 0 && !isDamage)
            {
                ChangeHp(-1);
                isDamage = true;
            }
            if (hp <= 0 && state != State.DEAD)
            {
                hp = 0;
                anim.SetTrigger("Dead");
                state = State.DEAD;
            }
        }
    }
}