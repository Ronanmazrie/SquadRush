# 装備画面のイラスト — 0.9.0

2026-10-17。内蔵の画像生成ツールを使用。CLI/APIキー経由やTripoのクレジットを使う処理は行っていません。

生成指示全文は [prompts.json](prompts.json) に記録しています。生成PNGはそのまま保存し、Unityのインポート設定で人物1024px・装備512pxを上限に縮小して表示します。透過を保持し、クリック判定を持たないImageとして配置しています。

保存先は `SquadRush/Assets/Resources/EquipmentArt/`（このPCでは `C:/Users/wakam/OneDrive/ドキュメント/ChatGPT/ゲーム開発/SquadRush/Assets/Resources/EquipmentArt/`）。再生成用ソースキットの `SquadRushPrototype/Art/` にも同じPNGを保存しています。

| ファイル | 用途 |
| --- | --- |
| hero.png | 装備画面中央の人物イラスト |
| scout-rifle.png | スカウトライフル |
| ranger-vest.png | レンジャーベスト |
| quick-magazine.png | クイックマガジン |
| assault-rifle.png | アサルトライフル |
| guardian-vest.png | ガーディアンベスト |
| rapid-magazine.png | 高速マガジン |

参考：[Survivor.io公式紹介](https://play.google.com/store/apps/details?id=com.dxx.firenow)、前回確認した[装備画面の解説・画面例](https://www.talkandroid.com/24372-survivor-io-ultimate-game-guide-with-tips-2023/)。中央の人物、周囲の装備枠、下部の一覧、アイテム詳細という見せ方を参考にしています。ゲーム固有の人物や装備画像は取り込まず、Squad Rush用の新しい絵を生成しました。

画面の枠色は装備中・所持・未入手を示します。新しいレア度・強化レベル・通貨は導入していません。詳細の差分は選択中の装備と現在の同じ種類の装備の実際の効果を比較し、同種の装備がない場合は0を基準とします。ガチャは準備中のままです。

0.11.0でakane.pngとmio.pngのキャライラストを追加。内蔵image_genで生成し、正確な指示はcharacter-prompts.jsonに保存しています。
