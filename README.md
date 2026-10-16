# スクワッド・ラッシュ / Squad Rush

敵を自動射撃しながら、部隊を左右に動かして仲間を増やす縦画面のUnityゲーム試作です。加算・倍率ゲート、数字付きの箱、敵の大群、終盤のボス戦を実装しています。広告・課金はありません。

## スマホで遊ぶ

AndroidではAPKをダウンロードしてインストールします。iPhone・Android・PCではブラウザ版も利用できます。

- [遊ぶ・ダウンロードのページ](https://ronanmazrie.github.io/SquadRush/)
- [Android版APKとリリース情報](https://github.com/Ronanmazrie/SquadRush/releases/tag/v0.4.0)

AndroidアプリはAndroid 8.0以降のARM64端末向けです。iPhoneではSafariからブラウザ版を開きます。Android用APKはiPhoneにインストールできません。iPhone用ネイティブアプリは未配布です。

## 遊び方

0.4.0ではランダム生成とステージ進行を追加しました。ステージ1は砂漠、2は森林、3は雪原。その後もテーマを切り替えながら進みます。景色の種類・場所・向き・大きさは挑戦ごとに変わり、流れ去った景色も再抽選します。敵やゲートの並びも固定していません。背景は48個の枠と共有メッシュを使い回します。

「スタート」を押し、指を左右にドラッグして部隊を移動します。射撃は自動です。加算・倍率ゲートを通り、箱を撃破して仲間を増やしてください。仲間が0人になると同じステージを再挑戦できます。ステージ終盤のボスを倒すとクリアとなり、「次のステージへ」で先へ進みます。ステージの長さは40～58秒＋ボス戦で、難易度は徐々に上がります。PCではマウス、左右矢印、A / Dでも遊べます。

進行はクリア時に端末内へ自動保存し、次回は続きから再開できます。ブラウザ版は同じブラウザ・同じ配信元のIndexedDBへ保存します（[Unity PlayerPrefsの説明](https://docs.unity3d.com/ja/current/ScriptReference/PlayerPrefs.html)）。Androidアプリの保存とは別です。

## Unityで開く

Unity 6.3 LTS `6000.3.26f1` を使用します。このリポジトリの `SquadRush` フォルダーをUnity Hubへ追加して開き、上部の `Squad Rush > Play Prototype` を選びます。WindowsでUnityを標準インストール場所へ導入している場合は、ルートの `Start-SquadRush.cmd` からも開けます。

Android用のビルドにはAndroid Build Support、SDK / NDK、OpenJDKが必要です。ブラウザ版にはWeb Build Supportが必要です。対応モジュールを導入後、`Squad Rush > Distribution` のメニューから配布用ビルドを作成できます。

## 開発と検証

- `SquadRush/Assets/Scripts/`：ゲーム処理、日本語HUD、共有モデル生成と兵士アニメーション。
- `SquadRushStage.cs` / `SquadRushScenery.cs`：ステージ設定・進行保存と景色の自動生成。
- `SquadRush/Assets/Resources/*.shader`：兵士の色・陰影と半透明エフェクト。
- `SquadRush/Assets/Editor/SquadRush/`：シーン生成、検証、起動、配布用ビルド。
- `SquadRush/Verification/`：ゲーム処理21ケースと再生モード15ケースの検証記録、画面プレビュー。
- `SquadRush/Builds/Android/`：このPCで作成したAPKとSHA-256（Git管理対象外。GitHub Releasesで配布）。
- `SquadRushPrototype/`：プロジェクト再生成用のソースキット。
- `docs/`：GitHub Pagesの配布ページとブラウザ版。

Editor内の検証に合格しています。ブラウザ版はスマホサイズのPCブラウザで起動・移動・射撃・一時停止・再挑戦を確認済みです。Android APKは署名・対応OS・CPU・アライメントの確認に合格しています。スマホ実機での性能・操作は未検証です。

## 素材

キャラクターと背景はオリジナルのプログラム生成モデルです。参考広告の画像・動画・3Dモデルはゲームへ取り込んでいません。日本語フォントはNoto Sans CJK JP（SIL Open Font License）を使い、ライセンスを `SquadRush/Assets/Fonts/LICENSE.txt` に含めています。Unity TextMeshPro付属フォントのライセンスも対応する素材フォルダーに含めています。
