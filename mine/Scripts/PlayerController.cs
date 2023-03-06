using UnityEngine;
using UnityEngine.UI;

//プレイヤーの制御クラス
public class PlayerController : MonoBehaviour
{
    //プレイヤーの歩くスピード
    const float _WALK_SPEED = 3.0f;
    //プレイヤーの走るスピード
    const float _RUN_SPEED = 5.0f;
    //走りに代わるスティックの傾き
    const float _RUN_STICK_SLOP = 0.7f;
    //プレイヤーのライフ
    public int _life = 1000;
    //左ステックの縦の傾き
    private float _verticalValue;
    //左スティックの横の傾き
    private float _horizontalValue;
    //プレイヤーの攻撃の範囲
    const float _ATTACK_RANGE = 1.0f;
    //プレイヤーの攻撃の当たる距離
    const float _ATTACK_DISTANCE = 2.0f;

    //プレイヤーのアニメーター
    private Animator _animator;
    //プレイヤーのキャラクターコントローラー
    private CharacterController _controller;

    //カメラ
    private GameObject _camera;

    //敵のオブジェクトプールのスクリプト
    ObjectPool _enmyPool;
    //敵のレイヤーの番号
    const int _ENEMY_LAYER = 1 << 8;

    //ゲーム開始から経過した時間
    private float _gameTime;
    //ゲームの制限時間
    const float _LIMIT_TIME = 60.0f;
    //残り時間のテキスト
    [SerializeField]
    private Text _remainingTimeText;
    //終了キャンバス
    [SerializeField]
    private GameObject _endCanvas;
    //倒した敵の数のテキスト
    [SerializeField]
    private Text _enemyDefeatedCountText;
    //敵を倒した数
    private int _enemyDefeatedCount;

    //カメラの名前
    const string _CAMERA_NAME = "MainCamera";
    //敵のオブジェクトプールの名前
    const string _ENEMY_POOL_NAME = "EnemyPool";
    //攻撃ボタンの名前
    const string _ATTACK_BUTTON_NAME = "Fire1";

    //プレイヤーのステートの種類
    private enum State
    {
        Idel,
        Walk,
        Run,
        Attack,
        Die
    }
    //プレイヤーのステート
    State _playerState;

    void Start()
    {
        //プレイヤーのアニメーター取得
        try
        {
            _animator = GetComponent<Animator>();
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogError("アニメーターが存在しません。");
        }
        //プレイヤーのキャラクターコントローラー取得
        try
        {
            _controller = GetComponent<CharacterController>();
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogError("キャラクターコントローラーが存在しません。");
        }
        //プレイヤーのステートをIdleでスタートさせる
        _playerState = State.Idel;

        //メインカメラの取得
        try
        {
            _camera = GameObject.Find(_CAMERA_NAME);
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogError("カメラが存在しません。");
        }

        //オブジェクトプールのスクリプト取得
        try
        {
            _enmyPool = GameObject.Find(_ENEMY_POOL_NAME).GetComponent<ObjectPool>();
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogError("敵のオブジェクトプールが存在しません。");
        }

        //経過時間を0に
        _gameTime = 0.0f;
        //倒した敵の数を0に
        _enemyDefeatedCount = 0;
    }

    private void Update()
    {
        //ゲームの経過時間を計測
        _gameTime += Time.deltaTime;
        //経過時間から残り時間を計算しテキストに代入
        _remainingTimeText.text = string.Format("{0:00}", _LIMIT_TIME - _gameTime);

        //コントローラーのスティックの傾きを取得
        _verticalValue = Input.GetAxis("Vertical");
        _horizontalValue = Input.GetAxis("Horizontal");

        //プレイヤーのライフが０以下になったか
        if (_life <= 0)
        {
            //プレイヤーのステートをDieにする
            _playerState = State.Die;
            if (_animator.GetInteger("PlayerState") != 4)
                _animator.SetInteger("PlayerState", 4);
            _enemyDefeatedCountText.text = string.Format("{0:0}", _enemyDefeatedCount);
            _endCanvas.SetActive(true);
        }
        //経過時間が制限時間を超えたら
        else if (_gameTime >= _LIMIT_TIME)
        {
            _enemyDefeatedCountText.text = string.Format("{0:0}", _enemyDefeatedCount);
            _endCanvas.SetActive(true);

        }

        //コントローラーのAボタン、左クリックがされたか
        if (Input.GetButtonDown(_ATTACK_BUTTON_NAME))
        {
            //プレイヤーのステートをAttackにする
            if (_playerState != State.Attack)
                _playerState = State.Attack;
            if (_animator.GetInteger("PlayerState") != 3)
                _animator.SetInteger("PlayerState", 3);
        }
        //プレイヤーのアニメーターが攻撃のアニメーターじゃないか
        else if (_playerState != State.Attack)
        {
            //コントローラーの左スティックが傾いているか
            if (_verticalValue != 0 || _horizontalValue != 0)
            {
                //コントローラーの傾きが_runStateで決められた以上に傾いているか
                if (_verticalValue > _RUN_STICK_SLOP || _verticalValue < _RUN_STICK_SLOP * -1.0f ||
                    _horizontalValue > _RUN_STICK_SLOP || _horizontalValue < _RUN_STICK_SLOP * -1.0f)
                {
                    //プレイヤーのステートをRunにする
                    if (_playerState != State.Run)
                        _playerState = State.Run;
                    if (_animator.GetInteger("PlayerState") != 2)
                        _animator.SetInteger("PlayerState", 2);
                }
                else
                {
                    //傾きが小さかった場合はプレイヤーのステートをWalkにする
                    if (_playerState != State.Walk)
                    _playerState = State.Walk;
                    if (_animator.GetInteger("PlayerState") != 1)
                        _animator.SetInteger("PlayerState", 1);

                }
            }
            else
            {
                //何も操作されていないか
                if (_playerState != State.Idel)
                    _playerState = State.Idel;
                if (_animator.GetInteger("PlayerState") != 0)
                    _animator.SetInteger("PlayerState", 0);
            }
        }
        //なにも操作がされていないか
        else
        {
            if (_playerState != State.Idel)
                _playerState = State.Idel;
            if (_animator.GetInteger("PlayerState") != 0)
                _animator.SetInteger("PlayerState", 0);
        }
    }
    private void FixedUpdate()
    {
        //プレイヤーのステートに合わせてSwitch文を走らせる
        switch (_playerState)
        {
            //Idleの場合はidleFlag以外をfalseにしてidleFlagをtrueにする
            case State.Idel:
                break;
            //Walkの場合はwalkFlag以外をfalseにしてwalkFlagをtrueにする
            case State.Walk:
                
                //プレイヤー移動のメソッド呼び出し
                PlayerMove(_WALK_SPEED);
                break;
            //Runの場合はrunFlag以外をfalseにしてrunFlagをtrueにする
            case State.Run:
          
                //プレイヤーの移動のメソッド呼び出し
                PlayerMove(_RUN_SPEED);
                break;
            //Attackの場合はattackFlag以外をfalseにしてattackFlagをtrueにする
            case State.Attack:
                PlayerAttack();
                break;
            //Dieの場合はdieFlag以外をfalseにしてdieFlagをtrueにする
            case State.Die: 
                break;
        }
    }

    //プレイヤーの移動のメソッド
    private void PlayerMove(float speed)
    {
        //カメラの向きからプレイヤーの移動の向きを取得
        Quaternion horizontalRotation = Quaternion.AngleAxis(_camera.transform.eulerAngles.y, Vector3.up);
        //カメラとの差だとY軸も回ってしまうが、Y軸は変えないので0に指定
        Vector3 direction = horizontalRotation * new Vector3(_horizontalValue, 0, _verticalValue).normalized;
        //コントローラーの傾きとカメラの向きからプレイヤーの横に回転させる
        transform.rotation = Quaternion.LookRotation(direction);
        //プレイヤーの傾きは上記で決まったのでその向きに進める
        _controller.Move(transform.forward * speed * Time.deltaTime);

    }

    //プレイヤーの攻撃メソッド
    private void PlayerAttack()
    {
        //プレイヤーの座標を取得
        Vector3 attackPosition = transform.position;
        //プレイヤーのY座標では低いので上げる
        attackPosition.y = 1.0f;
        //SphereCastに当たった敵の情報を配列に全て入れる
        RaycastHit[] hits = Physics.SphereCastAll(attackPosition, _ATTACK_RANGE, this.transform.forward, _ATTACK_DISTANCE, _ENEMY_LAYER);
        //SphereCastに当たった敵の分for文を回す
        for (int i = 0; i < hits.Length; i++)
        {
            //当たったオブジェクトのSetActiveをfalseにする
            hits[i].collider.gameObject.SetActive(false);
            _enemyDefeatedCount++;
        }
        //攻撃が敵に一体でも当たっていたら
        if (hits.Length > 0)
        {
            //オブジェクトプールに返す
            _enmyPool.EnemyStateSearch();
        }
    }
}