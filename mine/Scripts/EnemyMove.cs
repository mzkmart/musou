using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    //敵の移動のスピード
    const float _ENEMY_SPEED = 2.0f;
    //敵の攻撃ステートになる距離
    const float _ATTACK_RANGE = 2.0f;
    //敵のステートの種類
    private enum State
    {
        Idel,
        Move,
        Attack,
    }
    //敵のステート
    private State _enemyState;
    //敵のアニメーター
    private Animator _animator;
    //プレイヤー
    private GameObject _player;
    //プレイヤーのスクリプト
    private PlayerController _playerController;
    //プレイヤーと敵の座標の差
    private Vector3 _direction;

    private CharacterController _controller;

    void OnEnable()
    {
        //敵のアニメーターを取得
        _animator = GetComponent<Animator>();
        //敵のキャラクターコントローラーを取得
        _controller = GetComponent<CharacterController>();
        //敵のステートをIdelでスタートさせる
        _enemyState = State.Idel;

        //プレイヤーを取得
        _player = GameObject.Find("Player");
        //プレイヤーのスクリプトを取得
        _playerController = _player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        //プレイヤーと敵の座標の差を求める
        _direction = _player.transform.position - this.transform.position;
        //Y座標は固定にしたいので0にする
        _direction.y = 0.0f;
        // 座標の差からQuaternion(回転値)を取得
        Quaternion quaternion = Quaternion.LookRotation(_direction);
        //敵をプレイヤーの方向に向ける
        this.transform.rotation = quaternion;
    }

    private void FixedUpdate()
    {
        //敵のステートに合わせてSwitch文を走らせる
        switch (_enemyState)
        {
            //Idleの場合
            case State.Idel:
                //スポーンのアニメーションが終わっていたらステートをMoveにする
                if (_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Dash Forward In Place")
                {
                    _enemyState = State.Move;
                }
                break;
            case State.Move:
                //アニメーションのwalkFlagをtrue
                _animator.SetBool("walkFlag", true);
                //アニメーションのattackFlagをfalse
                _animator.SetBool("attackFlag", false);
                //敵を向かっている方向に進める
                _controller.Move(transform.forward * _ENEMY_SPEED * Time.deltaTime);
                //プレイヤーとの差が無くなったらステートをAttackにする
                if ((_direction.x < _ATTACK_RANGE && _direction.x > _ATTACK_RANGE * -1) &&
                    (_direction.z < _ATTACK_RANGE && _direction.z > _ATTACK_RANGE * -1))
                {
                    _enemyState = State.Attack;
                }
                break;
            case State.Attack:
                //アニメーションのwalkFlagをfalse
                _animator.SetBool("walkFlag", false);
                //アニメーションのattackFlagをtrue
                _animator.SetBool("attackFlag", true);
                //敵の座標を取得
                Vector3 vector3 = transform.position;
                //プレイヤーに当たるように下げる
                vector3.y = 0f;
                //Raycaastに当たったプレイヤーの情報を入れる変数を宣言
                RaycastHit hit;
                //Raycastをプレイヤーにしか当たらないようにレイヤーを宣言
                int layerMask = 1 << 9;
                //後で消す　　Raycastの視覚化
                Debug.DrawRay(vector3, this.transform.forward, Color.blue, 10.0f);
                //敵が向いてるほうにRayを飛ばしプレイヤーにぶつかったか
                if (Physics.Raycast(vector3, this.transform.forward, out hit, 10.0f, layerMask))
                {
                    //ぶつかった場合はプレイヤーのライフを1減らす
                    _playerController._life--;
                }
                //プレイヤーとの距離が離れた場合はステートをMoveにする
                if (!(_direction.x < _ATTACK_RANGE && _direction.x > _ATTACK_RANGE * -1) &&
                    !(_direction.z < _ATTACK_RANGE && _direction.z > _ATTACK_RANGE * -1))
                {
                    _enemyState = State.Move;
                }
                break;
        }
    }
}
