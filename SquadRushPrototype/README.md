# スクワッド・ラッシュ：再生成用ソースキット 0.14.0

作成済みのUnityプロジェクトは隣のSquadRushフォルダーです。Unity 6.3 LTS `6000.3.26f1` を使用します。
通常は作成済みプロジェクトをUnity Hubに追加し、`Squad Rush > Play Prototype` で起動してください。

## 再生成

PowerShellで `Initialize-Prototype.ps1` を実行します。シーンを作り直すため、独自の編集は先にGitへ保存してください。
Unity Editor・公式Unity CLI・必要なプラットフォームモジュールが必要です。初回パッケージ取得にはネットワーク接続を使います。
Sourcesの全C#、Editorの検証・生成コード、Art・StageArt・Shaders・Fontsを配置し、コンパイル・ゲーム処理・Playモード検証が成功した場合のみEditorを開きます。

## 現在の機能

常設6タブからステージ・装備・キャラ・ガチャ・ミッション・メールへ直接移動。
実際のイベントガチャ、独立した天井、メールでの無料10連チケット、ミッション内のログイン報酬。
ステージ7～12の個別設定と、難易度別のランダム装備ドロップ。
仲間1000人、固有スキル、装備3個の合成、ランダム背景、端末内保存を含みます。
キャラ確率の数値・提供割合画面は表示しません。
仕様・ドロップ表・遊び方は親フォルダーのREADME.mdを参照してください。

ゲーム処理148件・Playモード100件が成功。プレビューはSquadRush/Verification/Preview。
Android APK・ブラウザ版をGitHubで配布。iPhoneはブラウザ版を利用でき、iOSネイティブアプリは未配布。
実機のAndroid・iPhone・Safariの確認は未実施です。Fonts内にSIL OFLライセンスを同梱しています。
