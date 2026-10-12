# スクワッド・ラッシュ

Unity 6.3 LTS `6000.3.26f1` で作成した縦画面のスマホゲーム試作です。Android / iPhoneを対象とし、広告・課金はありません。

## 開く場所

プロジェクトフォルダー：`C:\Users\wakam\OneDrive\ドキュメント\ChatGPT\ゲーム開発\SquadRush`

Unity Hubで「Add / ディスクから追加」からこのフォルダーを選ぶか、Unity Editorでこのプロジェクトを開きます。`Assets/Scenes/SquadRush.unity` をダブルクリックし、上部の ▶ Play、その後ゲーム画面の「スタート」を押してください。

簡単に起動するには、Unityが閉じている状態で、親の「ゲーム開発」フォルダーにある `Start-SquadRush.cmd` をダブルクリックします。インストール済みのUnity本体を直接起動し、試作シーンを再生します。ゲーム画面の「スタート」を押してください。

すでにUnityが開いている場合は、上部メニューの `Squad Rush > Play Prototype` を選びます。シーンを探して開く操作は、このメニューが自動で行います。Unityの起動ログは `Logs/editor-session.log` に保存されます。

## 遊び方

- マウスを左右にドラッグ、左右矢印、A / Dで部隊を移動。スマホでは指のドラッグに対応。
- 射撃は自動。青緑の加算・倍率ゲートを選ぶと味方が増えます。
- 数字付きの箱を撃破すると味方が4人増えます。敵や箱との接触、赤い減算ゲートで味方が減ります。
- 仲間が0人になると敗北。60秒後に出るボスを倒すとクリア。何度でも再挑戦できます。
- 「Ⅱ」で一時停止。バックグラウンド移行時にも一時停止します。

## 検証とビルド

コンパイル、ゲーム処理11ケース、再生モード10ケースを確認済み。`Verification/` に結果と画面プレビューがあります。スマホのビルドモジュールと実機での確認は未完了です。

Android Build Support（SDK / NDK / OpenJDKを含む）を追加後、`Squad Rush > Build Android APK` でビルドできます。iOS Build Supportを追加後、`Squad Rush > Export iOS Xcode Project` で出力できます。iPhone実機用の最終ビルドにはMacのXcodeと署名設定、または対応するクラウドビルド環境が必要です。

編集するゲーム処理は `Assets/Scripts/SquadRushGame.cs`、画面表示は `Assets/Scripts/SquadRushHud.cs` にあります。参考画像の仕組みをもとに、オリジナルの簡易モデルで実装しています。フォントはNoto Sans CJK JP（SIL OFL）、ライセンスは `Assets/Fonts/LICENSE.txt` です。

プロジェクトと再生成用ソースキットは親フォルダーのGitで管理します。Assetsの変更時は `.meta` ファイルも一緒に保存してください。
