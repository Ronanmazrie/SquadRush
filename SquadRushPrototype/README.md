# スクワッド・ラッシュ：Unityスマホ試作

参考画像の仕組みをもとにした、縦画面の3D部隊シューティングです。Android / iPhone両対応、広告・課金なし。見た目はオリジナルのプログラム生成モデルです。

## 現在の状態

Unityプロジェクトは隣の `SquadRush` フォルダーに作成済みです。コンパイル、ゲーム処理120ケース、再生モード89ケースの検証が成功しています。画面のプレビューは `SquadRush/Verification/Preview/` に保存しています。このフォルダーは再生成用のソースキットです。

Unity CLI 1.0.0-beta.13の導入、Unityアカウントへのログイン、Personalライセンスの有効化は完了しています。選択したEditorはUnity 6.3 LTS `6000.3.26f1` です。

## セットアップ

作成済みのプロジェクトを遊ぶ場合は、Unityで `SquadRush` を開き、`Assets/Scenes/SquadRush.unity` を開いてPlayを押してください。以下はプロジェクトを再生成する場合の手順です。シーンを作り直すため、独自のシーン編集は先にGitへ保存してください。

1. Unity 6.3 LTS `6000.3.26f1` と公式Unity CLIを用意します。Editorが開いていればいったん閉じます。このPCでは導入済みです。
2. PowerShellでこのフォルダーの `Initialize-Prototype.ps1` を実行します。Unityの公式3Dテンプレートでプロジェクトを作成し、ソースを配置して、日本語フォント・マテリアル・シーンを生成します。Unityの初回パッケージ取得にはネットワーク接続が必要です。
3. セットアップはコンパイル、ゲーム処理、再生モードの検証が成功した場合だけEditorを開きます。失敗した場合はログを残して停止します。
4. `Assets/Scenes/SquadRush.unity` を開き、Unity上部のPlayボタンを押します。

```powershell
& './Initialize-Prototype.ps1'
```

このセットアップはUnity UI（TextMeshProを含む）とInput Systemを含む公式テンプレートを使います。追加パッケージの手動編集は行いません。既存のUnityプロジェクトが別バージョンの場合や、同名フォルダーに別のデータがある場合は停止します。

## 遊び方

- ホーム上部のステージ名で解放済みステージを選び、「出撃」で開始。ガチャはチケットまたはジェムで引けます。
- スマホでは指を左右にドラッグ。PCではマウスのドラッグ、左右矢印、A / Dキー。
- 前方への射撃は自動。味方の人数に応じて部隊全体の攻撃力が増えます。描画は最大96体、代表弾は1斉射40本までですが、人数は1000まで攻撃力へ反映します。
- 加算・倍率ゲートは仲間を増やし、赤い減算ゲートは仲間を減らします。部隊の中心が左右どちらを通ったかで判定します。
- 数字付きの補給BOXを壊すと、青は仲間が4人以上（約10％）増え、金色は威力、緑色は連射が強化されます。武器強化はそれぞれ5段階で、最大時は仲間補給に変わります。壊せず接触すると3人減ります。
- 赤い敵に接触すると仲間が1人減り、0人になると敗北。
- ステージ終盤のボスを倒すとクリア。「次のステージへ」で進み、進行を端末内に保存します。人数の上限は1000人です。次のステージ・再挑戦ではBOXの武器強化を初期化し、装備の効果は残します。
- 「Ⅱ」で一時停止。アプリがバックグラウンドに入っても一時停止します。
- 敗北時は同じステージを再挑戦できます。砂漠・森林・雪原の配置は挑戦ごとに変わり、流れ去った背景も再抽選します。

## スマホ用ビルド

Editorのメニューに `Squad Rush` が追加されます。

- `Generate Prototype`：シーンとフォントを生成。
- `Verify Gameplay Scenarios`：1000人の人数・攻撃力、BOXの武器強化、射撃・ゲート・敗北・ボス撃破、次のステージ、保存再開、ランダム生成と背景再利用など120ケースを確認。
- `Build Android APK`：`Builds/Android/SquadRush.apk` を生成。Unity Hub / CLIでAndroid Build Support、SDK / NDK、OpenJDKを追加する必要があります。
- `Export iOS Xcode Project`：`Builds/iOS` にXcodeプロジェクトを出力。iOS Build Supportの追加が必要です。

iPhoneの実機用アプリへ仕上げるには、MacのXcodeと署名設定、または対応するクラウドビルド環境が必要です。Android APKとブラウザ版はGitHubで配布し、Android / iPhoneの実機確認とストア公開は未実施です。

## ファイル

- `Sources/SquadRushGame.cs`：自動射撃、部隊、敵、箱、ゲート、ボス、オブジェクト再利用。
- `Sources/SquadRushHud.cs`：日本語メニュー・HUD・セーフエリア対応。
- `Sources/SquadRushStage.cs`：ステージ設定とローカル進行保存。
- `Sources/SquadRushScenery.cs`：ランダムな背景の配置・再抽選・オブジェクト再利用。
- `Editor/PrototypeBuilder.cs`：シーン・静的日本語フォント・ビルド・ゲーム処理の検証。
- `Editor/RuntimeVerifier.cs`：再生モードでボタン操作、キーボード、タッチ入力、進行ゲージ、敗北・再挑戦を検証し、画面を撮影。
- `Fonts/`：Noto Sans CJK JP、SIL Open Font License。日本語に使用する文字だけを静的フォントアトラスへ焼きます。

`SquadRush/Verification/gameplay-checks.txt` と `runtime-checks.txt` は、検証がすべて成功した場合の記録です。`SquadRush/Logs/` に生成・検証のログが残ります。`.cs` と生成された `.meta` を一緒にGitで管理しています。

## 調整しやすい値

`SquadRushStage.cs` の `Duration`、`StartingAllies`、`BossHealth`、`EnemyHealth`、`WaveInterval` でステージごとのバランスを調整できます。`SquadRushGame.cs` では人数上限、射撃間隔、ゲート報酬などを調整します。

公式情報：[Unity 6のサポート](https://unity.com/releases/unity-6/support)、[iOS環境の準備](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/iphone/getting-started/ios-environment-setup)、[フォントの配布元](https://github.com/notofonts/noto-cjk)。

ステージクリアで装備を入手し、ホームの「装備」で武器・防具・サポートを各1つ装備できます。所持品と装備状態を端末内へ保存し、以前のクリア報酬も付与します。地面の模様は白線・背景と同じ速度で流れます。


0.7.0では、下部の常設タブ、中央の兵士と大きな出撃ボタン、装備枠とアイコン一覧・詳細を追加しています。アイコンは Sources/SquadRushUiIcon.cs、兵士のUI表示は Sources/SquadRushPortrait.cs、画面構成は Sources/SquadRushFrontEnd.cs です。

0.8.0では Sources/SquadRushCharacterMeshes.cs で顔・服・手足の曲面を生成します。Sources/SquadRushArt.cs と同じ部分クラスで、再生成時は両方を配置してください。生成に外部サービスやクレジットは不要です。

0.9.0の装備画面では Art/ 内の人物と6種類の装備PNGを使用します。Sources/SquadRushEquipmentIllustrations.cs が画像の読み込みと比較表示、Editor/EquipmentArtImporter.cs がUnityのインポート設定を担当します。生成指示は Art/prompts.json に保存しています。


0.10.0: 同じ種類・同じレアリティ3個で1段階上へ合成（全5段階）。再クリア報酬も個数として保存。5列の装備一覧・縦スクロール・個数表示と、6枚のステージ絵を追加。ステージ7以降は6種の絵を繰り返します。旧装備データを引き継ぎます。ガチャは0.11.0で実装しました。


0.11.0: 初期キャラのレン、ガチャから入手するアカネ・ミオと固有スキルを実装。チケット／ジェム、キャラ率0.5%・80連目から上昇・90連目確定、ログインボーナス、クリア報酬、ミッションを追加。詳細な確率・報酬・操作はルートREADME.mdを参照してください。
