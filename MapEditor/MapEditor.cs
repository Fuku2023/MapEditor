/*--------------------------------------------------*
 * ファイル名 MapEditorForm.source
 * 
 * 作成日 2023年 11月 1日
 * 
 * 名前 福原 龍弥
 *--------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// 倉庫番のマップをエクセルにデータを保存するプログラム
/// </summary>
class MapEditorForm : Form
{
    private const int L = 48;
    // 表示するコンポーネントの管理するコンテナ
    private System.ComponentModel.IContainer _components;
    // 生成するマップの大きさ(横)
    private int _width = 15;
    // 生成するマップの大きさ(縦)
    private int _height = 15;
    // 黒いマスの配列データ
    private int _blackTrout = 2;
    // マスの配列データ
    private int _trout = 3;
    // ブロックの配列データ
    private int _block = 4;
    // キャラクターの配列データ
    private int _character = 5;
    // マップの外
    private int _mapOut = -1;

    // 画像表示
    PictureBox _basePictureBox;
    PictureBox _middlePictureBox;
    PictureBox _topPictureBox;

    // 素材の管理 -------------------------
    // マップ
    Bitmap _mapBitmap;
    // マスの色
    Bitmap _cursorBitmap;
    // ブロック
    Bitmap _blockBitmap;
    // キャラクター
    Bitmap _characterBitmap;
    // ------------------------------------

    // トリミング
    Rectangle[] _srcRect;
    // データ破壊
    Rectangle[,] _destRect;
    // マップの配列
    int[,] _mapData;
    // キャラクターのポイント
    Point _characterPoint;
    // マウスのポイント
    Point _mousePoint;
    // ブロックリスト
    List<Point> _blockList;
    // clipIndex=マップに配置するマス、キャラクター、ブロックの配列(1青色マス,2赤色マス,3黒色マス,4ブロック,5キャラクター)
    private int _chipIndex;
    // マウスのボタンが離されたかのフラグ
    private bool _mouseEntered;
    // マウスクリック
    MouseButtons _clickedMouseButton;

    /// <summary>
    /// メソッド呼び込み
    /// </summary>
    public MapEditorForm()
    {
        // 初期化したマップ生成
        Init();
        // 情報更新
        Draw();
    }
    /// <summary>
    /// 画像を分割して表示する処理
    /// </summary>
    /// <param name="disposing">処分するかどうか</param>
    protected override void Dispose(bool disposing)
    {
        // 今使用する画像かどうか
        if(disposing && _components != null)
        {
            // 画像を分割して表示
            _components.Dispose();
        }
        // 使用しない画像は表示しない
        base.Dispose(disposing);
    }
    
    /// <summary>
    /// マップの初期化
    /// </summary>
    private void Init()
    {
        // 初期化
        _components = new System.ComponentModel.Container();
        // 初期化
        _basePictureBox = new PictureBox();
        // 初期化
        _middlePictureBox = new PictureBox();
        // 初期化
        _topPictureBox = new PictureBox();
        // 配列対象のコントロールの制約を初期化
        SuspendLayout();

        _basePictureBox.Image = new Bitmap(_basePictureBox.Width, _basePictureBox.Height);
        _basePictureBox.Location = new Point(10, 10);

        // ブロック--------------
        _middlePictureBox.Image = new Bitmap(_middlePictureBox.Width, _middlePictureBox.Height);
        _middlePictureBox.BackColor = Color.Transparent;
        _middlePictureBox.Parent = _basePictureBox;
        //----------------------

        // マス------------------
        _topPictureBox.Image = new Bitmap(_topPictureBox.Width, _topPictureBox.Height);
        _topPictureBox.BackColor = Color.Transparent;
        _topPictureBox.Parent = _middlePictureBox;
        _topPictureBox.MouseMove += PictureBox_MouseMove;
        _topPictureBox.MouseDown += PictureBox_MouseDown;
        //----------------------
        // フォームに追加する
        Controls.Add(_basePictureBox);
        // 保留中のレイアウトを実行しない
        ResumeLayout(false);
        // マップ(画像ファイルを読み込み)
        _mapBitmap = new Bitmap("mapchip.png");
        // マスの色(画像ファイルを読み込み)
        _cursorBitmap = new Bitmap("cursor.png");
        // 操作キャラクター(画像ファイルを読み込み)
        _characterBitmap = new Bitmap("betty.png");
        // ブロック(画像ファイルを読み込み)
        _blockBitmap = new Bitmap("block.png");
        // 制作するマップの大きさ
        _mapData = new int[_width, _height];
        // 右クリックしたらそのマスにあるデータ破壊
        _destRect = new Rectangle[_mapData.GetLength(0), _mapData.GetLength(1)];
        // マップの横
        for (int x = 0; x < _mapData.GetLength(0); x++)
        {
            // マップの縦
            for (int y = 0; y < _mapData.GetLength(1); y++)
            {
                _mapData[x, y] = x > 0 && x < 4 && y > 0 && y < 4 ? 0 : 2;

                _destRect[x, y] = new Rectangle(x * L, y * L, L, L);
            }
        }
        // 使用する配列データをsrcRectに格納
        _srcRect = new Rectangle[5];
        // 使用する配列データの確認
        for (int i = 0; i < _srcRect.Length; i++)
        {
            // 使用する配列データの位置にサイズを表す4つの整数を格納
            _srcRect[i] = new Rectangle(i * L, 0, L, L);
        }
        // 赤いマスの初期配置
        _mapData[1, 1] = 1;
        // 赤いマスの初期配置
        _mapData[3, 3] = 1;
        // キャラクターの初期配置
        _characterPoint = new Point(2, 2);
        // マウスポインターの初期位置(マップ外)
        _mousePoint = new Point(_mapOut, _mapOut);
        // 起動したときは青色のマスを格納(初期化)
        _chipIndex = default;
        // マウスボタンを離していない
        _mouseEntered = false;
        // 押されたマウスボタンを格納
        _clickedMouseButton = MouseButtons.None;
        //ブロックリスト
        _blockList = new List<Point>();
        //ブロックの初期配置
        _blockList.Add(new Point(2, 1));
        //ブロックの初期配置
        _blockList.Add(new Point(2, 3));

        _basePictureBox.Size = new Size(_mapData.GetLength(0) * L, _mapData.GetLength(1) * L);
        _middlePictureBox.Size = new Size(_mapData.GetLength(0) * L, _mapData.GetLength(1) * L);
        _topPictureBox.Size = new Size(_mapData.GetLength(0) * L, _mapData.GetLength(1) * L);
        ClientSize = new Size(_mapData.GetLength(0) * L + 20, _mapData.GetLength(1) * L + 20);
    }

    /// <summary>
    /// マップの範囲内の処理
    /// </summary>
    /// <returns></returns>
    private Rectangle GetMapArea()
    {
        // マップの範囲設定
        int left = 0, right = _mapData.GetLength(0) - 1, top = 0, bottom = _mapData.GetLength(1) - 1;
        // 横(左から右)
        for(int x = left; x <= right; x++)
        {
            // フラグ
            bool trimFlag = true;
            // 縦
            for(int y = top; y <= bottom; y++)
            {
                // 黒色のマスかどうか
                if(_mapData[x,y] != _blackTrout)
                {
                    trimFlag = false;
                    break;
                }
            }
            // true
            if(trimFlag)
            {
                left++;
            }
            // false
            else
            {
                break;
            }
        }
        // 横
        for(int x = right; x >= 0; x--)
        {
            // フラグ
            bool trimFlag = true;
            // 縦(上から下)
            for(int y = top; y <= bottom; y++)
            {
                // 黒色のマスかどうか
                if (_mapData[x,y] != _blackTrout)
                {
                    trimFlag = false;
                    break;
                }
            }
            // true
            if (trimFlag)
            {
                right--;
            }
            // false
            else
            {
                break;
            }
        }
        // 縦(上から下)
        for (int y = top; y <= bottom; y++)
        {
            // フラグ
            bool trimFlag = true;
            // 横(左から右)
            for (int x = left; x <= right; x++)
            {
                // 黒色のマスかどうか
                if (_mapData[x,y] != _blackTrout)
                {
                    trimFlag = false;
                    break;
                }
            }
            // true
            if (trimFlag)
            {
                top++;
            }
            // false
            else
            {
                break;
            }
        }
        // 縦
        for (int y = bottom; y >= 0; y--)
        {
            // フラグ
            bool trimFlag = true;
            // 横(左から右)
            for (int x = left; x <= right; x++)
            {
                // 黒色のマスかどうか
                if (_mapData[x,y] != _blackTrout)
                {
                    trimFlag = false;
                    break;
                }
            }
            // true
            if (trimFlag)
            {
                bottom--;
            }
            // false
            else
            {
                break;
            }
        }
        // 
        return new Rectangle(left - 1, top - 1, right - left + 3, bottom - top + 3);
    }

    /// <summary>
    ///データをキーに割り当てる
    /// </summary>
    /// <param name="key">キーボード</param>
    protected override void OnKeyDown(KeyEventArgs key)
    {
        switch(key.KeyCode)
        {
            // 数字キー１
            case Keys.D1:
                // 青色のマス
                _chipIndex = 0;
                break;
            // 数字キー2
            case Keys.D2:
                // 赤色のマス
                _chipIndex = 1;
                break;
            // 数字キー3
            case Keys.D3:
                // 黒色のマス
                _chipIndex = 2;
                break;
            // 数字キー4
            case Keys.D4:
                // ブロック
                _chipIndex = 3;
                break;
            // 数字キー5
            case Keys.D5:
                // 操作するキャラクター
                _chipIndex = 4;
                break;
            // Sキー
            case Keys.S:
                // マップ保存
                mapSave();
                break;
        }
    }
    /// <summary>
    /// マウス入力の場所の更新処理
    /// </summary>
    /// <param name="paint">場所</param>
    protected override void OnPaint(PaintEventArgs paint)
    {
        // 更新処理
        Draw();
    }

    /// <summary>
    /// 制作したマップの保存
    /// </summary>
    private void mapSave()
    {
        // 名前を付けて保存するダイアログボックス表示
        SaveFileDialog dialog = new SaveFileDialog();
        // 保存ファイルの種類
        dialog.Filter = "マップファイル(*.csv)|*.csv";
        // 保存するかどうか(戻り値でOKが来たら保存)
        if(dialog.ShowDialog() == DialogResult.OK)
        {
            // テキストファイルにデータ(名前等)書き込む
            using(System.IO.StreamWriter writer = new StreamWriter(dialog.FileName, false) )
            {
                // マップ内の座標
                Rectangle rectangle  = GetMapArea();
                // ファイルデータにマップのサイズ書き込み
                writer.WriteLine($"{rectangle.Width},{rectangle.Height}");
                // 縦
                for(int y = 0; y < rectangle.Height; y++)
                {
                    // 横
                    for(int x = 0; x < rectangle.Width; x++)
                    {
                        // ファイルデータにマップ内のデータの書き込み
                        writer.Write($"{_mapData[rectangle.Left + x, rectangle.Top + y]}");
                        //if(x < rectangle.Width - 1)
                        //{
                        //    writer.Write(',');
                        //}
                    }
                    // マップデータ書き込み終わったら改行
                    writer.WriteLine();
                }
                // ファイルデータにキャラクターの座標の書き込み
                writer.WriteLine($"{_characterPoint.X},{_characterPoint.Y}");
                // ファイルデータにブロックの個数カウント
                writer.WriteLine(_blockList.Count);
                // リストからブロックを取り出す
                foreach (Point point in _blockList)
                {
                    // ファイルデータにブロック一つ一つの座標の書き込み
                    writer.WriteLine($"{point.X},{point.Y}");
                }
                // ファイルを閉じる
                writer.Close();
            }
        }
    }
    /// <summary>
    /// 配列データの更新処理
    /// </summary>
    /// <param name="x">マップの横</param>
    /// <param name="y">マップの縦</param>
    /// <param name="index">データ</param>
    private void SetChip(int x, int y, int index)
    {
        // マスの配置
        if(index < _trout)
        {
            // データの配置
            _mapData[x, y] = index;
        }
        // ブロックの配置
        else if(index == 4)
        {
            // ポインターの位置を記憶
            Point point = new Point(x, y);
            // 指定のデータ格納
            int i = _blockList.IndexOf(point);
            // マウスポインターの場所にブロックがあるかどうか
            if (i != -1)
            {
                // ポインターにブロックがあったらブロック削除
                _blockList.RemoveAt(i);
            }
            else
            {
                // マウスポインターの場所にブロック配置
                _blockList.Add(point);
            }
        }
        // キャラクターの配置
        else if(index == _character)
        {
            //マウスのポインターにキャラクター配置(前の場所に配置したキャラクターは消える)
            _characterPoint = new Point(x, y);
        }
    }
    /// <summary>
    /// マウスがクリックされた時にキーボードに割り当てられたデータとマウスの座標のデータが同じかどうか
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="botton">キーボード</param>
    private void PictureBox_MouseDown(object sender, MouseEventArgs botton)
    {
        // マウスがクリックされたかどうか
        if(_clickedMouseButton == MouseButtons.None)
        {
            // マウスの左クリック押されたら
            if(botton.Button == MouseButtons.Left)
            {
                // 左クリックに保存された配列のデータに更新
                _clickedMouseButton = botton.Button;
                // データの更新
                SetChip(_mousePoint.X, _mousePoint.Y, _chipIndex);
            }
            // マウスの右クリック押されたら
            else if(botton.Button == MouseButtons.Right)
            {
                // 黒色のマスに戻す
                _clickedMouseButton = botton.Button;
                // 黒色のマスに更新
                SetChip(_mousePoint.X, _mousePoint.Y, 2);
            }
        }
    }
    /// <summary>
    /// マウスがクリックされたかどうか
    /// </summary>
    /// <param name="mouse">マウスボタン</param>
    private void PictureBox_MouseMove(MouseEventArgs mouse)
    {
        // マウスポインターの位置の計算
        int x = mouse.X / L, y = mouse.Y / L;
        // 座標が異なっていたら
        if (_mousePoint.X != x || _mousePoint.Y != y)
        {
            // x座標代入
            _mousePoint.X = x;
            // y座標代入
            _mousePoint.Y = y;

            //マウスの左クリック押されたら
            if (_clickedMouseButton == MouseButtons.Left)
            {
                //左クリックに保存された配列のデータに更新
                SetChip(x, y, _chipIndex);
            }
            //マウスの右クリック押されたら
            else if (_clickedMouseButton == MouseButtons.Right)
            {
                //黒色のマスに戻す
                SetChip(x, y, _blackTrout);
            }
        }
    }
    /// <summary>
    /// 配置する際に更新するメソッド呼び込み
    /// </summary>
    private void Draw()
    {
        // 
        BaseDraw();
        // ブロック,キャラクターの配置
        MiddleDraw();
        // マスの配置
        TopDraw();
    }
    /// <summary>
    /// 
    /// </summary>
    private void BaseDraw()
    {
        // 配列の中の画像オブジェクト作成
        Bitmap bitmap = new Bitmap(_basePictureBox.Width, _basePictureBox.Height);
        // 現在のデータと異なっていたらデータを書き換える(使い終わったらリソースを開放) 
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            // 指定したデータを指定した場所に描画
            graphics.DrawImage(_mapBitmap, _destRect[x, y], _srcRect[_mapData[x, y]], GraphicsUnit.Pixel);
        }
        // Imageによって使用されているすべてのリソースを解放
        _basePictureBox.Image.Dispose();
        // 更新
        _basePictureBox.Image = bitmap;
    }
    /// <summary>
    /// ブロック,キャラクターの配置
    /// </summary>
    private void MiddleDraw()
    {
        // 配列の中の画像オブジェクト作成
        Bitmap bitmap = new Bitmap(_middlePictureBox.Width, _middlePictureBox.Height);
        // 現在のデータと異なっていたらデータを書き換える(使い終わったらリソースを開放)
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            // リストからブロックを取り出す
            foreach(Point point in _blockList)
            {
                // ブロックを指定した場所に描画
                graphics.DrawImage(_blockBitmap, point.X * L, point.Y * L);
            }
            // キャラクターを指定した場所に描画
            graphics.DrawImage(_characterBitmap, _destRect[_characterPoint.X,_characterPoint.Y], new Rectangle(0,0,L,L), GraphicsUnit.Pixel);
        }
        // Imageによって使用されているすべてのリソースを解放
        _middlePictureBox.Image.Dispose();
        // 更新
        _middlePictureBox.Image = bitmap;
    }

    /// <summary>
    /// マスの配置
    /// </summary>
    private void TopDraw()
    {
        // 配列の中の画像オブジェクト作成
        Bitmap bitmap = new Bitmap(_topPictureBox.Width, _topPictureBox.Height);
        // マウスが離されていない
        if(_mouseEntered)
        {
            // 現在のデータと異なっていたらデータを書き換える(使い終わったらリソースを開放)
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // 指定されたマスを指定した場所に描画
                graphics.DrawImage(_cursorBitmap, new Rectangle(_mousePoint.X * L, _mousePoint.Y * L, L, L), _srcRect[_chipIndex], GraphicsUnit.Pixel);
            }
        }
        // Imageによって使用されているすべてのリソースを解放
        _topPictureBox.Image.Dispose();
        // 更新
        _topPictureBox.Image = bitmap;
    }

    /// <summary>
    /// アプリを起動したら処理を行う
    /// </summary>
    [STAThread]
    public static void Main()
    {
        Application.Run(new MapEditorForm() );
    }
}