// --------------------------------------------------------- 
//Sokoban.cs 
// 
//�q�ɔԂ̃v���W�F�N�g�S�ĒS���Ă���
//
// �쐬��:2023/10/29 
// �쐬��:�΍����� 
// ---------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sokoban : MonoBehaviour
{
    private enum TileType
    {
        NONE,               // ��������
        GROUND,             // �n��
        TARGET,             // �ړI�n
        PLAYER,             // �v���C���[
        BLOCK,              // �u���b�N

        PLAYER_ON_TARGET,   // �v���C���[�i�ړI�n�̏�j
        BLOCK_ON_TARGET,    // �u���b�N�i�ړI�n�̏�j
    }
    private enum DirectionType
    {
        UP,                 // ��
        RIGHT,              // �E
        DOWN,               // ��
        LEFT,               // ��
    }
    // �X�e�[�W�\�����L�q���ꂽ�e�L�X�g�t�@�C��
    [SerializeField] 
    private TextAsset _stageFile=default;
    // �s��
    private int _rows = default;
    // ��
    private int _columns = default;
    // �^�C�������Ǘ�����񎟌��z��
    private TileType[,] _tileList = default;
    // �^�C���̃T�C�Y
    [SerializeField]
    private float _tileSize = default;
    //�v���C���[�̃I�u�W�F�N�g
    private GameObject _player = default;
    //���S�ʒu
    private Vector2 _middleOffset = default;
    //�v���C���[�̃v���n�u
    [SerializeField]
    private GameObject _playerPrefab = default;
    //�^�C���̃v���n�u
    [SerializeField]
    private GameObject _tilePrefab = default;
    //�u���b�N�̃v���n�u
    [SerializeField]
    private GameObject _blockPrefab = default;
    //�ړI�n�̃v���n�u
    [SerializeField]
    private GameObject _destinationPrefab = default;
    // �u���b�N�̐�
    private int _blockCount = default;
    // �Q�[�����N���A�����ꍇ true
    private bool _isClear = default;
    //BGM
    private AudioSource _audioSource = default;
    //SE
    [SerializeField] 
    private AudioClip _sound = default;
    // �e�ʒu�ɑ��݂���Q�[���I�u�W�F�N�g���Ǘ�����A�z�z��
    private Dictionary<GameObject, Vector2Int> _gameObjectPosTable = new Dictionary<GameObject, Vector2Int>();

    private void Start()
    {
        // �^�C���̏���ǂݍ���
        LoadTileData();
        // �X�e�[�W���쐬
        CreateStage();
        // AudioSource �R���|�[�l���g�̎擾
        _audioSource = GetComponent<AudioSource>();
        // �Đ�
        _audioSource.Play();

    }
    /// <summary>
    /// �^�C���̏���ǂݍ���
    /// </summary>
    private void LoadTileData()
    {
        // �^�C���̏�����s���Ƃɕ���
        string[] _rows_lines = _stageFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        // �^�C���̗񐔂��v�Z
        string[] _columns_nums = _rows_lines[0].Split(new[] { ',' });
        // �^�C���̗񐔂ƍs����ێ�
        _rows = _rows_lines.Length;
        _columns = _columns_nums.Length;
        // �^�C������ int �^�̂Q�����z��ŕێ�
        _tileList = new TileType[_columns, _rows];
        for (int y = 0; y < _rows; y++)
        {
            // �ꕶ�����擾
            string st = _rows_lines[y];
            _columns_nums = st.Split(new[] { ',' });
            for (int x = 0; x < _columns; x++)
            {

                // �ǂݍ��񂾕����𐔒l�ɕϊ����ĕێ�
                _tileList[x, y] = (TileType)int.Parse(_columns_nums[x]);
            }
        }
    }
    /// <summary>
    /// �X�e�[�W���쐬
    /// </summary>
    private void CreateStage()
    {
        // �X�e�[�W�̒��S�ʒu���v�Z
        _middleOffset.x = (_columns * _tileSize * 0.5f) - (_tileSize * 0.5f);
        _middleOffset.y = (_rows * _tileSize * 0.5f) - (_tileSize * 0.5f);
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                //TileType �̒l
                TileType tile_val = _tileList[x, y];
                // ���������ꏊ�͖���
                if (tile_val == TileType.NONE)
                {
                    continue;
                }
                // �^�C���̖��O�ɍs�ԍ��Ɨ�ԍ���t�^
                string name =string.Format("tile{0}_{1}", y, x);
                // �v���n�u����C���X�^���X���쐬
                GameObject tile = Instantiate(_tilePrefab, GetDisplayPosition(x, y), Quaternion.identity);
                // �C���X�^���X�������^�C���̖��O��ݒ�
                tile.name = name;
                // �^�C���̈ʒu��ݒ�
                tile.transform.position = GetDisplayPosition(x, y);
                // �ړI�n�̏ꍇ
                if (tile_val == TileType.TARGET)
                {
                    // �ړI�n�̃v���n�u����C���X�^���X���쐬
                    GameObject destination = Instantiate(_destinationPrefab, GetDisplayPosition(x, y), Quaternion.identity);
                    // �C���X�^���X�������ړI�n�̖��O��ݒ�
                    destination.name = "destination";
                }
                // �v���C���[�̏ꍇ
                if (tile_val == TileType.PLAYER)
                {
                    // �v���C���[�̃v���n�u����C���X�^���X���쐬
                    GameObject player= Instantiate(_playerPrefab, GetDisplayPosition(x, y), Quaternion.identity);
                    // �C���X�^���X�������v���C���[�̖��O��ݒ�
                    player.name = "player";
                    // �v���C���[��A�z�z��ɒǉ�
                    _gameObjectPosTable.Add(player, new Vector2Int(x, y));
                }
                // �u���b�N�̏ꍇ
                else if (tile_val == TileType.BLOCK)
                {
                    // �u���b�N�̃v���n�u����C���X�^���X���쐬
                    GameObject block = Instantiate(_blockPrefab, GetDisplayPosition(x, y), Quaternion.identity);
                    // �C���X�^���X�������u���b�N�̖��O��ݒ�
                    block.name = "block";
                    // �u���b�N��A�z�z��ɒǉ�
                    _gameObjectPosTable.Add(block, new Vector2Int(x, y));
                }

            }
        }
    }
    /// <summary>
    /// �w�肳�ꂽ�s�ԍ��Ɨ�ԍ�����X�v���C�g�̕\���ʒu���v�Z���ĕԂ�
    /// </summary>
    /// <param name="x">�s</param>
    /// <param name="y">��</param>
    /// <returns>�\���ʒu</returns>
    private Vector2 GetDisplayPosition(int x, int y)
    {
        return new Vector2(
            x * _tileSize - _middleOffset.x,
            y * -_tileSize + _middleOffset.y
        );
    }
    /// <summary>
    /// �w�肳�ꂽ�ʒu�ɑ��݂���Q�[���I�u�W�F�N�g��Ԃ��܂�
    /// </summary>
    /// <param name="pos">�ʒu</param>
    /// <returns>���݂���Q�[���I�u�W�F�N�g</returns>
    private GameObject GetGameObjectAtPosition(Vector2Int pos)
    {
        foreach (KeyValuePair<GameObject, Vector2Int> pair in _gameObjectPosTable)
        {
            // �w�肳�ꂽ�ʒu�����������ꍇ
            if (pair.Value == pos)
            {
                // ���̈ʒu�ɑ��݂���Q�[���I�u�W�F�N�g��Ԃ�
                return pair.Key;
            }
        }
        return null;
    }
    // �w�肳�ꂽ�ʒu�̃^�C�����u���b�N�Ȃ� true ��Ԃ�
    private bool IsBlock(Vector2Int pos)
    {
        TileType cell = _tileList[pos.x, pos.y];
        return cell == TileType.BLOCK || cell == TileType.BLOCK_ON_TARGET;
    }
    /// <summary>
    ///  �w�肳�ꂽ�ʒu���X�e�[�W���Ȃ� true ��Ԃ�
    /// </summary>
    /// <param name="pos"> �ʒu</param>
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
        // �Q�[���N���A���Ă���ꍇ�͑���ł��Ȃ��悤�ɂ���
        if (_isClear)
        {
            return;
        }
        // ���󂪉����ꂽ�ꍇ
        if (Input.GetButtonDown("Up"))
        {
            // �v���C���[����Ɉړ�
            MovePlayer(DirectionType.UP);
        }
        // �E��󂪉����ꂽ�ꍇ
        else if (Input.GetButtonDown("Right"))
        {
            // �v���C���[���E�Ɉړ�
            MovePlayer(DirectionType.RIGHT);
        }
        // ����󂪉����ꂽ�ꍇ
        else if (Input.GetButtonDown("Down"))
        {
            // �v���C���[�����Ɉړ�
            MovePlayer(DirectionType.DOWN);
        }
        // ����󂪉����ꂽ�ꍇ
        else if (Input.GetButtonDown("Left"))
        {
            // �v���C���[�����Ɉړ�
            MovePlayer(DirectionType.LEFT);
        }
        //���Z�b�g
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Main");
        }
    }
    /// <summary>
    /// �w�肳�ꂽ�����Ƀv���C���[���ړ�
    /// </summary>
    /// <param name="direction"></param>
    private void MovePlayer(DirectionType direction)
    {
        // �v���C���[�̌��ݒn���擾
        Vector2Int currentPlayerPos = _gameObjectPosTable[_player];
        // �v���C���[�̈ړ���̈ʒu���v�Z
        Vector2Int nextPlayerPos = GetNextPositionAlong(currentPlayerPos, direction);
        // �v���C���[�̈ړ��悪�X�e�[�W���ł͂Ȃ��ꍇ�͖���
        if (!IsValidPosition(nextPlayerPos))
        {
            return;
        }
        // �v���C���[�̈ړ���Ƀu���b�N�����݂���ꍇ
        if (IsBlock(nextPlayerPos))
        {
            // �u���b�N�̈ړ���̈ʒu���v�Z
            Vector2Int nextBlockPos = GetNextPositionAlong(nextPlayerPos, direction);
            // �u���b�N�̈ړ��悪�X�e�[�W���̏ꍇ����,�u���b�N�̈ړ���Ƀu���b�N�����݂��Ȃ��ꍇ
            if (IsValidPosition(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                // �ړ�����u���b�N���擾
                GameObject block = GetGameObjectAtPosition(nextPlayerPos);
                // �v���C���[�̈ړ���̃^�C���̏����X�V
                UpdateGameObjectPosition(nextPlayerPos);
                // �u���b�N���ړ�
                block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y);
                // �u���b�N�̈ʒu���X�V
                _gameObjectPosTable[block] = nextBlockPos;
                // �u���b�N�̈ړ���̔ԍ����X�V
                if (_tileList[nextBlockPos.x, nextBlockPos.y] == TileType.GROUND)
                {
                    // �ړ��悪�n�ʂȂ�u���b�N�̔ԍ��ɍX�V
                    _tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK;
                }
                else if (_tileList[nextBlockPos.x, nextBlockPos.y] == TileType.TARGET)
                {
                    // �ړ��悪�ړI�n�Ȃ�v���C���[�i�ړI�n�̏�j�̔ԍ��ɍX�V
                    _tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK_ON_TARGET;
                }
                // �v���C���[�̌��ݒn�̃^�C���̏����X�V
                UpdateGameObjectPosition(currentPlayerPos);
                // �v���C���[���ړ�
                _player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);
                // �v���C���[�̈ʒu���X�V
                _gameObjectPosTable[_player] = nextPlayerPos;
                // �v���C���[�̈ړ���̔ԍ����X�V
                if (_tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.GROUND)
                {
                    // �ړ��悪�n�ʂȂ�v���C���[�̔ԍ��ɍX�V
                    _tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER;
                }
                else if (_tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.TARGET)
                {
                    // �ړ��悪�ړI�n�Ȃ�v���C���[�i�ړI�n�̏�j�̔ԍ��ɍX�V
                    _tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER_ON_TARGET;
                }
            }
        }
        // �v���C���[�̈ړ���Ƀu���b�N�����݂��Ȃ��ꍇ
        else
        {
            // �v���C���[�̌��ݒn�̃^�C���̏����X�V
            UpdateGameObjectPosition(currentPlayerPos);
            // �v���C���[���ړ�
            _player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);
            // �v���C���[�̈ʒu���X�V
            _gameObjectPosTable[_player] = nextPlayerPos;
            // �v���C���[�̈ړ���̔ԍ����X�V
            if (_tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.GROUND)
            {
                // �ړ��悪�n�ʂȂ�v���C���[�̔ԍ��ɍX�V
                _tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER;
            }
        }
        // �Q�[�����N���A�������ǂ����m�F
        CheckCompletion();
    }
    // �w�肳�ꂽ�����̈ʒu��Ԃ�
    private Vector2Int GetNextPositionAlong(Vector2Int pos, DirectionType direction)
    {
        switch (direction)
        {
            // ��
            case DirectionType.UP:
                pos.y--;
                break;
            // �E
            case DirectionType.RIGHT:
                pos.x++;
                break;
            // ��
            case DirectionType.DOWN:
                pos.y++;
                break;
            // ��
            case DirectionType.LEFT:
                pos.x--;
                break;
        }
        return pos;
    }
    /// <summary>
    ///  �w�肳�ꂽ�ʒu�̃^�C�����X�V
    /// </summary>
    /// <param name="pos"></param>
    private void UpdateGameObjectPosition(Vector2Int pos)
    {
        // �w�肳�ꂽ�ʒu�̃^�C���̔ԍ����擾
        TileType cell = _tileList[pos.x, pos.y];
        // �v���C���[�������̓u���b�N�̏ꍇ
        if (cell == TileType.PLAYER || cell == TileType.BLOCK)
        {
            // �n�ʂɕύX
            _tileList[pos.x, pos.y] = TileType.GROUND;
        }
        // �ړI�n�ɏ���Ă���v���C���[�������̓u���b�N�̏ꍇ
        else if (cell == TileType.PLAYER_ON_TARGET || cell == TileType.BLOCK_ON_TARGET)
        {
            // �ړI�n�ɕύX
            _tileList[pos.x, pos.y] = TileType.TARGET;
        }
    }
    /// <summary>
    /// �Q�[�����N���A�������ǂ����m�F
    /// </summary>
    private void CheckCompletion()
    {
        // �ړI�n�ɏ���Ă���u���b�N�̐����v�Z
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
        // ���ׂẴu���b�N���ړI�n�̏�ɏ���Ă���ꍇ
        if (blockOnTargetCount == _blockCount)
        {
            // �Q�[���N���A
            _isClear = true;
            //BGM�I��
            _audioSource.Stop();
            //SE����
            _audioSource.PlayOneShot(_sound);
        }
    }
}
