# スクワッド・ラッシュ 0.14.0

Unity 6.3 LTS `6000.3.26f1` の縦画面ゲーム。広告・課金なし。

Unity Hubでこのフォルダーを追加し、`Squad Rush > Play Prototype`、または `Assets/Scenes/SquadRush.unity` を開いてPlay。
Windowsでは親フォルダーの `Start-SquadRush.cmd` からも開けます。

下部の6タブ：ステージ・装備・キャラ・ガチャ・ミッション・メール。ステージは同じ画面の矢印で選択して出撃。
恒常とイベント「紅蓮作戦」は実際に抽選でき、天井を別々に保存。確率の数値表示はありません。
ログイン報酬はミッション内、無料10連チケットはメールで管理。以前の受取状態も引き継ぎます。
ステージ7～12を個別設定し、クリアで難易度に応じたレアリティの装備1個をランダム獲得します。

詳細・ドロップ表・配布先・操作は親のREADME.mdを参照してください。
ゲーム処理148件・Playモード100件、計248件成功。記録はVerification。スマホ実機は未確認。

Android・Webのビルドは `Squad Rush > Distribution`。各プラットフォームのモジュールが必要です。
iOS用の最終ビルドはMacのXcodeと署名が必要で、現在はブラウザ版のみ配布しています。
データは端末内保存。フォントはNoto Sans CJK JP（SIL OFL、Assets/Fonts/LICENSE.txt）。
