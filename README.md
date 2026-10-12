# スクワッド・ラッシュ / Squad Rush

敵を自動射撃しながら、部隊を左右に動かして仲間を増やす縦画面のUnityゲーム試作です。加算・倍率ゲート、数字付きの箱、敵の大群、終盤のボス戦を実装しています。広告・課金はありません。

## スマホで遊ぶ

AndroidではAPKをダウンロードしてインストールします。iPhone・Android・PCではブラウザ版も利用できます。

- [遊ぶ・ダウンロードのページ](https://ronanmazrie.github.io/SquadRush/)
- [Android版APKとリリース情報](https://github.com/Ronanmazrie/SquadRush/releases/tag/v0.1.0)

AndroidアプリはAndroid 8.0以降のARM64端末向けです。iPhoneではSafariからブラウザ版を開きます。Android用APKはiPhoneにインストールできません。iPhone用ネイティブアプリは未配布です。

## 遊び方

「スタート」を押し、指を左右にドラッグして部隊を移動します。射撃は自動です。加算・倍率ゲートを通り、箱を撃破して仲間を増やしてください。赤い敵との接触や減算ゲートで人数が減り、0人になると敗北します。60秒後に出現するボスを倒すとクリアです。PCではマウス、左右矢印、A / Dでも遊べます。

## Unityで開く

Unity 6.3 LTS `6000.3.26f1` を使用します。このリポジトリの `SquadRush` フォルダーをUnity Hubへ追加して開き、上部の `Squad Rush > Play Prototype` を選びます。WindowsでUnityを標準インストール場所へ導入している場合は、ルートの `Start-SquadRush.cmd` からも開けます。

Android用のビルドにはAndroid Build Support、SDK / NDK、OpenJDKが必要です。ブラウザ版にはWeb Build Supportが必要です。対応モジュールを導入後、`Squad Rush > Distribution` のメニューから配布用ビルドを作成できます。

## 開発と検証

- `SquadRush/Assets/Scripts/`：ゲーム処理と日本語HUD。
- `SquadRush/Assets/Editor/SquadRush/`：シーン生成、検証、起動、配布用ビルド。
- `SquadRush/Verification/`：ゲーム処理11ケースと再生モード10ケースの検証記録、画面プレビュー。
- `SquadRush/Builds/Android/`：このPCで作成したAPKとSHA-256（Git管理対象外。GitHub Releasesで配布）。
- `SquadRushPrototype/`：プロジェクト再生成用のソースキット。
- `docs/`：GitHub Pagesの配布ページとブラウザ版。

Editor内の検証に合格しています。ブラウザ版はスマホサイズのPCブラウザで起動・移動・射撃・一時停止・再挑戦を確認済みです。Android APKは署名・対応OS・CPU・アライメントの確認に合格しています。スマホ実機での性能・操作は未検証です。

## 素材

キャラクターと背景は試作用のオリジナル簡易モデルです。日本語フォントはNoto Sans CJK JP（SIL Open Font License）を使い、ライセンスを `SquadRush/Assets/Fonts/LICENSE.txt` に含めています。Unity TextMeshPro付属フォントのライセンスも対応する素材フォルダーに含めています。
