// --------------------------------------------------------- 
//Sokoban.cs 
// 
//倉庫番のプロジェクト全て担っている
//
// 作成日:2023/10/29 
// 作成者:石黒尊琉 
// ---------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sokoban : MonoBehaviour
{
    private enum TileType
    {
        NONE,               // 何も無い
        GROUND,             // 地面
        TARGET,             // 目的地
        PLAYER,             // プレイヤー
        BLOCK,              // ブロック

        PLAYER_ON_TARGET,   // プレイヤー（目的地の上）
        BLOCK_ON_TARGET,    // ブロック（目的地の上）
    }
    private enum DirectionType
    {
        UP,                 // 上
        RIGHT,              // 右
        DOWN,               // 下
        LEFT,               // 左
    }
    // ステージ構造が記述されたテキストファイル
    [SerializeField] 
    private TextAsset _stageFile=default;
    // 行数
    private int _rows = default;
    // 列数
    private int _columns = default;
    // タイル情報を管理する二次元配列
    private TileType[,] _tileList = default;
    // タイルのサイズ
    [SerializeField]
    private float _tileSize = default;
    //プレイヤーのオブジェクト
    private GameObject _player = default;
    //中心位置
    private Vector2 _middleOffset = default;
    //プレイヤーのプレハブ
    [SerializeField]
    private GameObject _playerPrefab = default;
    //タイルのプレハブ
    [SerializeField]
    private GameObject _tilePrefab = default;
    //ブロックのプレハブ
    [SerializeField]
    private GameObject _blockPrefab = default;
    //目的地のプレハブ
    [SerializeField]
    private GameObject _destinationPrefab = default;
    // ブロックの数
    private int _blockCount = default;
    // ゲームをクリアした場合 true
    private bool _isClear = default;
    //BGM
    private AudioSource _audioSource = default;
    //SE
    [SerializeField] 
    private AudioClip _sound = default;
    // 各位置に存在するゲームオブジェクトを管理する連想配列
    private Dictionary<GameObject, Vector2Int> _gameObjectPosTable = new Dictionary<GameObject, Vector2Int>();

    private void Start()
    {
        // タイルの情報を読み込む
        LoadTileData();
        // ステージを作成
        CreateStage();
        // AudioSource コンポーネントの取得
        _audioSource = GetComponent<AudioSource>();
        // 再生
        _audioSource.Play();

    }
    /// <summary>
    /// タイルの情報を読み込む
    /// </summary>
    private void LoadTileData()
    {
        // タイルの情報を一行ごとに分割
        string[] _rows_lines = _stageFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        // タイルの列数を計算
        string[] _columns_nums = _rows_lines[0].Split(new[] { ',' });
        // タイルの列数と行数を保持
        _rows = _rows_lines.Length;
        _columns = _columns_nums.Length;
        // タイル情報を int 型の２次元配列で保持
        _tileList = new TileType[_columns, _rows];
        for (int y = 0; y < _rows; y++)
        {
            // 一文字ずつ取得
            string st = _rows_lines[y];
            _columns_nums = st.Split(new[] { ',' });
            for (int x = 0; x < _columns; x++)
            {

                // 読み込んだ文字を数値に変換して保持
                _tileList[x, y] = (TileType)int.Parse(_columns_nums[x]);
            }
        }
    }
    /// <summary>
    /// ステージを作成
    /// </summary>
    private void CreateStage()
    {
        // ステージの中心位置を計算
        _middleOffset.x = (_columns * _tileSize * 0.5f) - (_tileSize * 0.5f);
        _middleOffset.y = (_rows * _tileSize * 0.5f) - (_tileSize * 0.5f);
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                //TileType の値
                TileType tile_val = _tileList[x, y];
                // 何も無い場所は無視
                if (tile_val == TileType.NONE)
                {
                    continue;
                }
                // タイルの名前に行番号と列番号を付与
                string name =string.Format("tile{0}_{1}", y, x);
                // プレハブからインスタンスを作成
                GameObject tile = Instantiate(_tilePrefab, GetDisplayPosition(x, y), Quaternion.identity);
                // インスタンス化したタイルの名前を設定
                tile.name = name;
                // タイルの位置を設定
                tile.transform.position = GetDisplayPosition(x, y);
                // 目的地の場合
                if (tile_val == TileType.TARGET)
                {
                    // 目的地のプレハブからインスタンスを作成
                    GameObject destination = Instantiate(_destinationPrefab, GetDisplayPosition(x, y), Quaternion.identity);
                    // インスタンス化した目的地の名前を設定
                    destination.name = "destination";
                }
                // プレイヤーの場合
                if (tile_val == TileType.PLAYER)
                {
                    // プレイヤーのプレハブからインスタンスを作成
                    GameObject player= Instantiate(_playerPrefab, GetDisplayPosition(x, y), Quaternion.identity);
                    // インスタンス化したプレイヤーの名前を設定
                    player.name = "player";
                    // プレイヤーを連想配列に追加
                    _gameObjectPosTable.Add(player, new Vector2Int(x, y));
                }
                // ブロックの場合
                else if (tile_val == TileType.BLOCK)
                {
                    // ブロックのプレハブからインスタンスを作成
                    GameObject block = Instantiate(_blockPrefab, GetDisplayPosition(x, y), Quaternion.identity);
                    // インスタンス化したブロックの名前を設定
                    block.name = "block";
                    // ブロックを連想配列に追加
                    _gameObjectPosTable.Add(block, new Vector2Int(x, y));
                }

            }
        }
    }
    /// <summary>
    /// 指定された行番号と列番号からスプライトの表示位置を計算して返す
    /// </summary>
    /// <param name="x">行</param>
    /// <param name="y">列</param>
    /// <returns>表示位置</returns>
    private Vector2 GetDisplayPosition(int x, int y)
    {
        return new Vector2(
            x * _tileSize - _middleOffset.x,
            y * -_tileSize + _middleOffset.y
        );
    }
    /// <summary>
    /// 指定された位置に存在するゲームオブジェクトを返します
    /// </summary>
    /// <param name="pos">位置</param>
    /// <returns>存在するゲームオブジェクト</returns>
    private GameObject GetGameObjectAtPosition(Vector2Int pos)
    {
        foreach (KeyValuePair<GameObject, Vector2Int> pair in _gameObjectPosTable)
        {
            // 指定された位置が見つかった場合
            if (pair.Value == pos)
            {
                // その位置に存在するゲームオブジェクトを返す
                return pair.Key;
            }
        }
        return null;
    }
    // 指定された位置のタイルがブロックなら true を返す
    private bool IsBlock(Vector2Int pos)
    {
        TileType cell = _tileList[pos.x, pos.y];
        return cell == TileType.BLOCK || cell == TileType.BLOCK_ON_TARGET;
    }
    /// <summary>
    ///  指定された位置がステージ内なら true を返す
    /// </summary>
    /// <param name="pos"> 位置</param>
    /// <returns>true</returns>
    private bool IsValidPosition(Vector2Int pos)
    {
        if (0 <= pos.x && pos.x < _columns && 0 <= pos.y && pos.y < _rows)
        {
            return _tileList[pos.x, pos.y] != TileType.NONE;
        }
        return false;
    }
    private void Update()
    {
        // ゲームクリアしている場合は操作できないようにする
        if (_isClear)
        {
            return;
        }
        // 上矢印が押された場合
        if (Input.GetButtonDown("Up"))
        {
            // プレイヤーが上に移動
            MovePlayer(DirectionType.UP);
        }
        // 右矢印が押された場合
        else if (Input.GetButtonDown("Right"))
        {
            // プレイヤーが右に移動
            MovePlayer(DirectionType.RIGHT);
        }
        // 下矢印が押された場合
        else if (Input.GetButtonDown("Down"))
        {
            // プレイヤーが下に移動
            MovePlayer(DirectionType.DOWN);
        }
        // 左矢印が押された場合
        else if (Input.GetButtonDown("Left"))
        {
            // プレイヤーが左に移動
            MovePlayer(DirectionType.LEFT);
        }
        //リセット
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Main");
        }
    }
    /// <summary>
    /// 指定された方向にプレイヤーが移動
    /// </summary>
    /// <param name="direction"></param>
    private void MovePlayer(DirectionType direction)
    {
        // プレイヤーの現在地を取得
        Vector2Int currentPlayerPos = _gameObjectPosTable[_player];
        // プレイヤーの移動先の位置を計算
        Vector2Int nextPlayerPos = GetNextPositionAlong(currentPlayerPos, direction);
        // プレイヤーの移動先がステージ内ではない場合は無視
        if (!IsValidPosition(nextPlayerPos))
        {
            return;
        }
        // プレイヤーの移動先にブロックが存在する場合
        if (IsBlock(nextPlayerPos))
        {
            // ブロックの移動先の位置を計算
            Vector2Int nextBlockPos = GetNextPositionAlong(nextPlayerPos, direction);
            // ブロックの移動先がステージ内の場合かつ,ブロックの移動先にブロックが存在しない場合
            if (IsValidPosition(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                // 移動するブロックを取得
                GameObject block = GetGameObjectAtPosition(nextPlayerPos);
                // プレイヤーの移動先のタイルの情報を更新
                UpdateGameObjectPosition(nextPlayerPos);
                // ブロックを移動
                block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y);
                // ブロックの位置を更新
                _gameObjectPosTable[block] = nextBlockPos;
                // ブロックの移動先の番号を更新
                if (_tileList[nextBlockPos.x, nextBlockPos.y] == TileType.GROUND)
                {
                    // 移動先が地面ならブロックの番号に更新
                    _tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK;
                }
                else if (_tileList[nextBlockPos.x, nextBlockPos.y] == TileType.TARGET)
                {
                    // 移動先が目的地ならプレイヤー（目的地の上）の番号に更新
                    _tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK_ON_TARGET;
                }
                // プレイヤーの現在地のタイルの情報を更新
                UpdateGameObjectPosition(currentPlayerPos);
                // プレイヤーを移動
                _player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);
                // プレイヤーの位置を更新
                _gameObjectPosTable[_player] = nextPlayerPos;
                // プレイヤーの移動先の番号を更新
                if (_tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.GROUND)
                {
                    // 移動先が地面ならプレイヤーの番号に更新
                    _tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER;
                }
                else if (_tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.TARGET)
                {
                    // 移動先が目的地ならプレイヤー（目的地の上）の番号に更新
                    _tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER_ON_TARGET;
                }
            }
        }
        // プレイヤーの移動先にブロックが存在しない場合
        else
        {
            // プレイヤーの現在地のタイルの情報を更新
            UpdateGameObjectPosition(currentPlayerPos);
            // プレイヤーを移動
            _player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);
            // プレイヤーの位置を更新
            _gameObjectPosTable[_player] = nextPlayerPos;
            // プレイヤーの移動先の番号を更新
            if (_tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.GROUND)
            {
                // 移動先が地面ならプレイヤーの番号に更新
                _tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER;
            }
        }
        // ゲームをクリアしたかどうか確認
        CheckCompletion();
    }
    // 指定された方向の位置を返す
    private Vector2Int GetNextPositionAlong(Vector2Int pos, DirectionType direction)
    {
        switch (direction)
        {
            // 上
            case DirectionType.UP:
                pos.y--;
                break;
            // 右
            case DirectionType.RIGHT:
                pos.x++;
                break;
            // 下
            case DirectionType.DOWN:
                pos.y++;
                break;
            // 左
            case DirectionType.LEFT:
                pos.x--;
                break;
        }
        return pos;
    }
    /// <summary>
    ///  指定された位置のタイルを更新
    /// </summary>
    /// <param name="pos"></param>
    private void UpdateGameObjectPosition(Vector2Int pos)
    {
        // 指定された位置のタイルの番号を取得
        TileType cell = _tileList[pos.x, pos.y];
        // プレイヤーもしくはブロックの場合
        if (cell == TileType.PLAYER || cell == TileType.BLOCK)
        {
            // 地面に変更
            _tileList[pos.x, pos.y] = TileType.GROUND;
        }
        // 目的地に乗っているプレイヤーもしくはブロックの場合
        else if (cell == TileType.PLAYER_ON_TARGET || cell == TileType.BLOCK_ON_TARGET)
        {
            // 目的地に変更
            _tileList[pos.x, pos.y] = TileType.TARGET;
        }
    }
    /// <summary>
    /// ゲームをクリアしたかどうか確認
    /// </summary>
    private void CheckCompletion()
    {
        // 目的地に乗っているブロックの数を計算
        int blockOnTargetCount = 0;
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                if (_tileList[x, y] == TileType.BLOCK_ON_TARGET)
                {
                    blockOnTargetCount++;
                }
            }
        }
        // すべてのブロックが目的地の上に乗っている場合
        if (blockOnTargetCount == _blockCount)
        {
            // ゲームクリア
            _isClear = true;
            //BGM終了
            _audioSource.Stop();
            //SE発動
            _audioSource.PlayOneShot(_sound);
        }
    }
}
