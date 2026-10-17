using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace SquadRush.Editor
{
    [InitializeOnLoad]
    public static class RuntimeVerifier
    {
        const string Pending = "SquadRush.RuntimeVerification.Pending";
        static RuntimeVerifier() { EditorApplication.playModeStateChanged += PlayStateChanged; }
        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Runtime verification runs in batch mode.");
            PrototypeBuilder.PrepareVisuals();
            EditorSceneManager.OpenScene("Assets/Scenes/SquadRush.unity");
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }
        static void PlayStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
            var game = UnityEngine.Object.FindFirstObjectByType<SquadRushGame>();
            if (game == null) { Debug.LogError("Runtime game was not created."); EditorApplication.Exit(1); return; }
            game.enabled = false;
            game.StartCoroutine(Guard(CheckRuntime(game)));
        }
        static IEnumerator Guard(IEnumerator steps)
        {
            while (true)
            {
                bool more = false, failed = false;
                object next = null;
                try { more = steps.MoveNext(); if (more) next = steps.Current; }
                catch (Exception error) { failed = true; Debug.LogException(error); }
                if (failed || !more) { EditorApplication.Exit(failed ? 1 : 0); yield break; }
                yield return next;
            }
        }
        static IEnumerator CheckRuntime(SquadRushGame game)
        {
            for (int i = 0; i < 5; i++) yield return null;
            Touchscreen touch = null;
            string storageKey = "SquadRush.RuntimeVerification." + Guid.NewGuid();
            try
            {
                game.ConfigureVerification(storageKey);
                var checks = new System.Collections.Generic.List<string>();
                VerifyCharacterMeshes(game, checks);
                Require(game.State == RunState.Ready && game.SquadCount == 3, "Scene starts with a three-ally squad and menu", checks);
                var play = game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "Play");
                Capture(game, "menu.png");
                Require(game.Hud.GetComponentsInChildren<SquadRushUiIcon>().All(icon => icon.canvasRenderer != null && icon.GeneratedVertices > 0), "Visible navigation icons have renderers and generated geometry", checks);
                Require(game.Hud.CurrentPage == FrontEndPage.Home && !play.gameObject.activeInHierarchy, "Home replaces the old opening dialog", checks);
                Require(Button(game, "HomeSortie").gameObject.activeInHierarchy && Button(game, "HomeCommand").gameObject.activeInHierarchy && Button(game, "HomeEquipment").gameObject.activeInHierarchy, "Home exposes a direct sortie and persistent navigation", checks);
                Click(game, "HomeGacha");
                Require(game.Hud.CurrentPage == FrontEndPage.Gacha && game.Equipment.OwnedCount == 0, "Gacha navigation displays recruitment without granting items", checks);
                Capture(game, "gacha.png");
                Require(!Button(game,"DrawOne").interactable&&!Button(game,"DrawTen").interactable,"New profile cannot draw without earned tickets or gems",checks);
                Click(game,"GachaRates");Capture(game,"gacha-rates.png");RequireTextFits(game,"Published gacha rates and pity table fit their controls",checks);Click(game,"CloseRates");
                Click(game,"GachaCharacters");Click(game,"Character1");
                Require(!Button(game,"SelectCharacter").interactable&&UiText(game,"CharacterSkillDescription").text.Contains("範囲攻撃"),"Locked character inspection shows the real skill but prevents selection",checks);
                Capture(game,"characters-locked.png");Click(game,"HomeCommand");Click(game,"HomeLogin");
                Require(Button(game,"ClaimLogin").interactable,"Home opens a claimable login bonus",checks);Click(game,"ClaimLogin");
                Require(game.Account.Gems==100&&game.Account.Tickets==1&&!Button(game,"ClaimLogin").interactable,"Login button grants exact currency and disables a second claim",checks);Capture(game,"login.png");
                Click(game,"HomeCommand");Click(game,"HomeMissions");Require(!Button(game,"ClaimMission0").interactable,"Incomplete daily missions cannot be claimed through the UI",checks);Capture(game,"missions.png");Click(game,"HomeGacha");
                Click(game, "GachaEquipment");
                var illustratedHero=UiArt(game,"EquipmentHeroIllustration");
                Require(illustratedHero.sprite!=null && illustratedHero.transform.parent.GetComponentsInChildren<UnityEngine.UI.RawImage>(true).Length==0,"Equipment uses a 2D anime hero rather than the 3D portrait",checks);
                Require(Enumerable.Range(0,6).Select(i=>UiArt(game,"InventoryIllustration"+i).sprite).Distinct().Count()==6 && Enumerable.Range(0,6).All(i=>UiArt(game,"InventoryIllustration"+i).sprite!=null),"All six equipment items have distinct illustration sprites",checks);
                Click(game, "InventoryItem0"); Require(game.Hud.CurrentPage == FrontEndPage.Equipment && !Button(game, "EquipmentAction").interactable, "Equipment screen prevents equipping unearned items", checks);
                Require(UiArt(game,"DetailIllustration").sprite==UiArt(game,"InventoryIllustration0").sprite && UiText(game,"DetailComparison").text.Contains("+1") && UiText(game,"DetailSource").text.Contains("STAGE 01"),"Unowned inspection shows the matching art, positive comparison and source stage",checks);
                RequireTextFits(game,"Illustrated item detail labels fit their controls",checks);
                Capture(game, "equipment-locked.png"); Click(game, "CloseEquipmentDetail");
                Require(game.Hud.CurrentPage == FrontEndPage.Equipment && !Button(game, "EquipmentAction").gameObject.activeInHierarchy, "Closing an item inspection returns to inventory without changing gear", checks);
                Click(game, "HomeCommand"); Click(game, "HomeStages");
                Require(game.Hud.CurrentPage == FrontEndPage.Stages && !Button(game, "NextStage").interactable, "Home opens stage selection with locked progression", checks);
                Capture(game, "stages.png");
                Click(game, "StageStart"); Require(game.State == RunState.Playing && game.Hud.CurrentPage == FrontEndPage.Hidden, "Stage sortie button starts combat and hides Home", checks);
                Capture(game, "ground-before.png");
                game.Tick(0.25f, false);
                Require(Mathf.Abs(game.GroundTravel - 0.875f) < 0.001f && game.transform.Find("World/SoilSurface").GetComponent<Renderer>().sharedMaterial.mainTextureOffset.y > 0, "Play-mode ground textures advance with terrain distance", checks);
                Capture(game, "ground-after.png");
                Keyboard keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.RightArrow)); InputSystem.Update();
                game.Tick(0.05f, true);
                Require(game.SquadX > 0, "Keyboard input moves the squad", checks);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
                game.Begin();
                touch = InputSystem.AddDevice<Touchscreen>();
                InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Began, position = new Vector2(100, 400) });
                InputSystem.Update(); game.Tick(0.02f, true);
                InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Moved, position = new Vector2(240, 400) });
                InputSystem.Update(); game.Tick(0.02f, true);
                Require(game.SquadX > 0, "Touch drag moves the squad", checks);
                InputSystem.RemoveDevice(touch); touch = null;
                var pause = game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == "Pause");
                pause.onClick.Invoke(); Require(game.State == RunState.Paused, "Pause button freezes the run", checks);
                play.onClick.Invoke(); Require(game.State == RunState.Playing, "Continue button resumes the run", checks);
                game.StartVerificationStage(1, 5104); game.ChangeSquad(15); game.SetAim(1.5f);
                for (int i = 0; i < 325; i++) game.Tick(0.02f, false);
                Require(game.State == RunState.Playing && game.ActiveTargets > 0 && game.ActiveBullets > 0, "Enemies, squad and automatic bullets run in Play mode", checks);
                var progress = game.Hud.GetComponentsInChildren<RectTransform>().Single(r => r.name == "ProgressFill");
                Require(Mathf.Abs(progress.anchorMax.x - game.Elapsed / game.StageDuration) < 0.01f, "Visible progress bar follows the current stage duration", checks);
                game.SpawnTarget(TargetKind.Crate, -2.5f, 16, 22);
                game.Hud.Flash("");
                yield return null; // Flush Unity's deferred Destroy queue before inspecting rendered objects.
                Capture(game, "gameplay.png");
                game.SpawnTarget(TargetKind.Boss, 0, 15, 1000);
                game.Tick(0.02f, false);
                yield return null;
                Capture(game, "boss.png");
                game.ChangeSquad(-100); Require(game.State == RunState.Lost, "Defeat menu appears in Play mode", checks);
                play.onClick.Invoke(); Require(game.State == RunState.Playing && game.SquadCount == 3 && game.ActiveTargets == 0, "Retry button resets the run", checks);
                game.ChangeSquad(997);
                for (int row = 0; row < 9; row++) for (int column = 0; column < 8; column++)
                    game.SpawnTarget(TargetKind.Enemy, -3.5f + column, 13 + row * 2.2f, 999);
                game.Tick(0.02f, false);
                Require(game.SquadCount == 1000 && game.VisibleAllies == 96 && game.ActiveTargets == 72 && game.ActiveBullets == 40 && game.LastVolleyDamage == 1000, "1000 allies, 96 displayed troops, 72 enemies and weighted automatic fire run in Play mode", checks);
                yield return null;
                Capture(game, "crowd.png");
                var countLabel = game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "SquadCount");
                Require(countLabel.text == "仲間 1000" && !countLabel.isTextOverflowing, "1000-ally HUD is fully visible without clipping", checks);
                game.StartVerificationStage(1, 5104);
                game.SpawnTarget(TargetKind.Crate, 0, 5, 1, SupplyReward.Power);
                for (int i = 0; i < 30; i++) game.Tick(0.02f, false);
                game.SpawnTarget(TargetKind.Crate, 0, 5, 1, SupplyReward.RapidFire);
                for (int i = 0; i < 30; i++) game.Tick(0.02f, false);
                Require(game.WeaponDamage == 2 && game.RapidFireLevel == 1 && game.GetComponentsInChildren<SquadRushTrooper>().Where(t => t.WeaponModule != null).All(t => t.WeaponModule.gameObject.activeSelf), "Weapon crates upgrade damage, firing rate and visible rifles", checks);
                game.ChangeSquad(27);
                game.SpawnTarget(TargetKind.Crate, -2.8f, 14, 18, SupplyReward.Power);
                game.SpawnTarget(TargetKind.Crate, 2.8f, 17, 22, SupplyReward.RapidFire);
                game.Hud.Flash(""); yield return null; Capture(game, "weapons.png");
                var weaponLabel = game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "WeaponStats");
                Require(weaponLabel.text.Contains("威力 2") && !weaponLabel.isTextOverflowing, "Weapon HUD shows acquired upgrades without clipping", checks);
                game.StartVerificationStage(1, 5104); game.ChangeSquad(57); game.SpawnTarget(TargetKind.Boss, 0, 20, 1);
                for (int i = 0; i < 180 && game.State == RunState.Playing; i++) game.Tick(0.02f, false);
                Require(game.State == RunState.Won && new SquadRushProgress(storageKey).ResumeStage == 2, "A real boss defeat saves stage two", checks);
                Require(game.Equipment.Owns(0) && new SquadRushEquipment(storageKey).Owns(0), "Boss clear permanently saves the rifle reward", checks);
                yield return null;
                Capture(game, "clear.png");
                play.onClick.Invoke();
                Require(game.StageNumber == 2 && game.State == RunState.Playing && game.ActiveTargets == 0, "The clear screen Next button advances to stage two", checks);
                game.ChangeSquad(15); game.SetAim(-1.5f);
                for (int i = 0; i < 300; i++) game.Tick(0.02f, false);
                game.Hud.Flash(""); yield return null; Capture(game, "forest.png");
                game.StartVerificationStage(3, 8128); game.ChangeSquad(15); game.SetAim(1.5f);
                for (int i = 0; i < 300; i++) game.Tick(0.02f, false);
                game.Hud.Flash(""); yield return null; Capture(game, "snow.png");
                game.ConfigureVerification(storageKey);
                Require(game.StageNumber == 2 && game.State == RunState.Ready, "Reloading the saved progress offers stage two in the menu", checks);
                Click(game, "HomeEquipment"); Click(game, "InventoryItem0"); Click(game, "EquipmentAction");
                Require(game.Equipment.IsEquipped(0) && game.WeaponDamage == 2 && game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "ItemState0").text.Contains("装備中"), "Equipment button equips the earned rifle and updates its state", checks);
                Require(UiArt(game,"EquippedIllustration0").sprite==UiArt(game,"InventoryIllustration0").sprite && UiText(game,"StatValue0").text=="2","Equipped slot illustration and power value reflect the actual rifle",checks);
                yield return null; Capture(game, "equipment.png");
                RequireTextFits(game,"Equipment page labels fit their visible controls",checks);
                Click(game, "EquippedSlotWeapon"); Capture(game, "equipment-detail.png");
                Require(Button(game, "EquipmentAction").interactable && Button(game, "EquipmentAction").GetComponentInChildren<TMP_Text>().text == "装備を外す", "The equipped weapon slot opens its own removable item detail", checks);
                Click(game, "CloseEquipmentDetail");
                Click(game, "HomeCommand"); Click(game, "HomeStages"); Click(game, "PreviousStage"); Click(game, "StageStart");
                Require(game.StageNumber == 1 && game.WeaponDamage == 2 && game.Equipment.IsEquipped(0), "An earlier unlocked stage starts with permanent equipped bonuses", checks);
                game.TogglePause(); Click(game, "ReturnHome");
                Require(game.State == RunState.Ready && game.Hud.CurrentPage == FrontEndPage.Home && game.ActiveTargets == 0 && game.ActiveBullets == 0, "Pause screen returns cleanly to Home", checks);
                game.ConfigureVerification(storageKey); Click(game, "HomeEquipment");
                Require(game.Equipment.IsEquipped(0) && game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "ItemState0").text.Contains("装備中"), "Reload restores both the equipped rifle and equipment UI", checks);
                Click(game, "EquippedSlotWeapon"); Click(game, "EquipmentAction");
                Require(game.WeaponDamage == 1 && !game.Equipment.IsEquipped(0), "Equipment button also unequips the rifle", checks);
                Click(game, "HomeCommand");
                Click(game, "HomeStages"); Click(game, "PreviousStage");
                Capture(game, "stages-owned.png");
                Require(game.Hud.GetComponentsInChildren<TMP_Text>().Where(t => t.gameObject.activeInHierarchy).All(t => !t.isTextOverflowing), "Stage page fits already-owned reward and navigation labels", checks);
                Click(game, "HomeCommand");
                Click(game, "HomeSortie");
                Require(game.State == RunState.Playing && game.StageNumber == 2 && game.Hud.CurrentPage == FrontEndPage.Hidden, "Home's central sortie starts the current unlocked stage directly", checks);
                game.ShowHome();
                Require(game.Hud.GetComponentsInChildren<TMP_Text>(true).All(t => t.font == null || t.text.All(c => c < 32 || t.font.HasCharacter(c))), "Menu, stage names and Next button have baked Japanese glyphs", checks);
                game.Equipment.Acquire(3); game.OpenEquipment(); Click(game,"InventoryItem3");
                Require(UiText(game,"DetailComparison").text.Contains("+2") && UiArt(game,"DetailIllustration").sprite==UiArt(game,"InventoryIllustration3").sprite,"Stronger weapon inspection shows its own artwork and true bonus",checks);
                Click(game,"EquipmentAction");
                Require(game.WeaponDamage==3 && UiText(game,"StatValue0").text=="3" && UiArt(game,"EquippedIllustration0").sprite==UiArt(game,"InventoryIllustration3").sprite,"Replacing a weapon updates equipped art and real damage together",checks);
                Click(game,"InventoryItem0");
                Require(UiText(game,"DetailComparison").text.Contains("-1"),"Weaker weapon comparison displays the negative difference",checks);
                Capture(game,"equipment-compare.png"); Click(game,"CloseEquipmentDetail"); Click(game,"InventoryItem5");
                Require(UiText(game,"DetailComparison").text.Contains("+20%") && !Button(game,"EquipmentAction").interactable,"Support comparison uses its own slot and percentage while locked",checks);
                Click(game,"CloseEquipmentDetail");
                game.Equipment.Acquire(0);game.Equipment.Acquire(0);game.ToggleEquipment(0);
                Click(game,"InventoryItem0");
                Require(Button(game,"EquipmentMerge").interactable&&UiText(game,"MergePreview").text.Contains("×3")&&UiText(game,"MergePreview").text.Contains("+2"),"Fusion previews three consumed copies and the resulting stronger effect",checks);
                Capture(game,"equipment-fusion-ready.png");Click(game,"EquipmentMerge");
                Require(game.Equipment.Quantity(0)==0&&game.Equipment.Quantity(0,1)==1&&game.Equipment.IsEquipped(0,1)&&game.WeaponDamage==3,"Fusion button consumes exactly three copies and keeps upgraded gear equipped",checks);
                Require(!Button(game,"EquipmentMerge").interactable&&UiText(game,"DetailTitle").text.Contains("グッド")&&UiText(game,"DetailEffect").text.Contains("+2"),"Fusion detail switches to the new rarity and disables insufficient materials",checks);
                RequireTextFits(game,"Fusion detail and upgraded rarity labels fit",checks);Capture(game,"equipment-fusion-result.png");Click(game,"CloseEquipmentDetail");
                game.Equipment.Acquire(0,1);game.Equipment.Acquire(0,1);game.Begin();int n=game.Equipment.Quantity(0,1);Require(game.Equipment.CanMerge(0,1)&&!game.MergeEquipment(0,1)&&game.Equipment.Quantity(0,1)==n,"Fusion cannot consume equipment during a battle",checks);
                game.Tick(.02f,false);Require(game.LastVolleyDamage==game.SquadCount*3,"Fused weapon bonus is used by real combat projectiles",checks);game.ShowHome();
                game.ConfigureVerification(storageKey);Require(game.Equipment.IsEquipped(0,1)&&game.WeaponDamage==3,"Reload keeps the fusion result equipped",checks);
                for(int r=0;r<SquadRushEquipment.RarityCount;r++)for(int id=0;id<SquadRushEquipment.Count;id++)game.Equipment.Acquire(id,r);
                game.OpenEquipment();yield return null;Capture(game,"equipment-collection.png");Capture(game,"equipment-mobile.png",390,844);
                Require(true,"All five inventory columns and labels fit a 390 by 844 phone viewport",checks);
                var scroll=game.Hud.GetComponentInChildren<UnityEngine.UI.ScrollRect>();
                Require(scroll!=null&&scroll.content.GetComponent<UnityEngine.UI.GridLayoutGroup>().constraintCount==5&&scroll.content.rect.height>scroll.viewport.rect.height,"Compact five-column collection grows beyond the viewport for all 30 gear stacks",checks);
                scroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();yield return null;Capture(game,"equipment-collection-bottom.png");
                Click(game,"InventoryItem24");Require(UiText(game,"DetailTitle").text.Contains("レジェンド")&&!Button(game,"EquipmentMerge").interactable,"Highest rarity remains inspectable after scrolling and has no further fusion",checks);Click(game,"CloseEquipmentDetail");
                var stageSprites=new System.Collections.Generic.HashSet<Sprite>();
                for(int stage=1;stage<=6;stage++)
                {
                    game.StartVerificationStage(stage,123);game.Hud.ShowStages();stageSprites.Add(UiArt(game,"StageSelectionIllustration").sprite);
                    Require(UiArt(game,"HomeStageIllustration").sprite==UiArt(game,"StageSelectionIllustration").sprite,"Home and selection share the stage "+stage+" illustration",checks);
                    if(stage==4||stage==6){yield return null;Capture(game,"stage-art-"+stage+".png");}
                }
                Require(stageSprites.Count==6&&game.Hud.GetComponentsInChildren<UnityEngine.UI.RawImage>(true).Length==0,"Six distinct stage illustrations replace the former 3D menu portrait",checks);
                game.ConfigureVerification(storageKey);game.OpenStages();RequireTextFits(game,"Stage artwork, reward and navigation labels fit",checks);
                string accountUiKey=storageKey+".accountUI";
                PlayerPrefs.SetString(SquadRushAccount.StorageKey(accountUiKey),"{\"version\":1,\"characters\":1,\"gems\":1100,\"tickets\":10,\"pity\":89}");
                game.ConfigureVerification(accountUiKey);game.OpenGacha();Click(game,"UseGems");
                Require(Button(game,"DrawOne").interactable&&Button(game,"DrawTen").interactable,"Earned gem balance enables both draw sizes",checks);
                Click(game,"DrawOne");yield return null;
                Require(game.Account.Gems==1000&&game.Account.Tickets==10&&game.Account.Pity==0&&game.Account.LastResults[0].kind==DrawKind.Character,"Guaranteed character gem draw spends only the selected currency and resets pity",checks);
                int earned=game.Account.LastResults[0].id;
                Capture(game,"gacha-character-result.png");RequireTextFits(game,"Character draw receipt labels fit",checks);Click(game,"CloseDrawResults");
                Click(game,"GachaCharacters");Click(game,"Character"+earned);Click(game,"SelectCharacter");
                Require(game.Account.SelectedCharacter==earned&&UiArt(game,"CharacterPreview").sprite==UiArt(game,"CharacterPortrait"+earned).sprite,"Selecting the newly acquired character updates the actual active character and portrait",checks);
                Capture(game,"characters.png");Click(game,"HomeEquipment");Require(UiArt(game,"EquipmentHeroIllustration").sprite==UiArt(game,"CharacterPortrait"+earned).sprite,"Equipment page uses the selected character illustration",checks);
                game.Begin();game.SpawnTarget(TargetKind.Enemy,-3,25,1);Click(game,"CharacterSkill");
                Require(game.SkillActivations==1&&!Button(game,"CharacterSkill").interactable&&(earned==1?game.Kills==1:game.SkillBuffRemaining>0),"Battle skill button activates the selected character effect and displays cooldown",checks);
                Capture(game,"character-skill.png");game.ShowHome();game.ConfigureVerification(accountUiKey);
                Require(game.Account.SelectedCharacter==earned&&game.Account.Gems==1000&&game.Account.Tickets==10,"Reload preserves acquired character, selection, balances and pity",checks);
                game.OpenGacha();Click(game,"LastDrawReceipt");yield return null;Require(UiText(game,"ResultLabel0").text.Contains(SquadRushCharacters.Name(earned)),"Last draw receipt can be reviewed after reloading",checks);Click(game,"CloseDrawResults");
                PlayerPrefs.SetString(SquadRushAccount.StorageKey(accountUiKey),"{\"version\":1,\"characters\":1,\"gems\":1000,\"tickets\":10,\"pity\":88}");
                game.ConfigureVerification(accountUiKey);game.OpenGacha();Click(game,"UseTickets");Click(game,"DrawTen");yield return null;
                Require(game.Account.LastResults.Length==10&&game.Account.LastResults.Any(r=>r.kind==DrawKind.Character)&&game.Account.Tickets==0&&game.Account.Gems==1000,"Ten-ticket UI draw crosses pity, grants ten results and leaves gems untouched",checks);
                Capture(game,"gacha-ten-results.png");Capture(game,"gacha-mobile.png",390,844);RequireTextFits(game,"Ten-result receipt fits its cards",checks);Click(game,"CloseDrawResults");
                Require(!Button(game,"DrawOne").interactable&&!Button(game,"DrawTen").interactable,"Exhausted ticket mode does not fall back to spending gems",checks);
                Click(game,"UseGems");Require(Button(game,"DrawOne").interactable&&Button(game,"DrawTen").interactable,"Gem mode remains available after tickets run out",checks);
                game.OpenMissions();Require(Button(game,"ClaimMission2").interactable,"Completed draw mission becomes claimable",checks);Click(game,"ClaimMission2");Require(game.Account.Gems==1050&&!Button(game,"ClaimMission2").interactable,"Mission UI grants 50 gems exactly once",checks);
                string output = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Verification");
                Directory.CreateDirectory(output); File.WriteAllLines(Path.Combine(output, "runtime-checks.txt"), checks);
                Debug.Log("[SquadRush] " + checks.Count + " Play mode scenarios passed.");
            }
            finally
            {
                if (touch != null) InputSystem.RemoveDevice(touch);
                PlayerPrefs.DeleteKey(storageKey); PlayerPrefs.DeleteKey(SquadRushEquipment.StorageKey(storageKey));PlayerPrefs.DeleteKey(SquadRushAccount.StorageKey(storageKey));PlayerPrefs.DeleteKey(storageKey+".accountUI");PlayerPrefs.DeleteKey(SquadRushEquipment.StorageKey(storageKey+".accountUI"));PlayerPrefs.DeleteKey(SquadRushAccount.StorageKey(storageKey+".accountUI"));PlayerPrefs.Save();
                SessionState.SetBool(Pending, false);
            }
        }
        static void Require(bool condition, string name, System.Collections.Generic.List<string> checks)
        { if (!condition) throw new InvalidOperationException(name); checks.Add("PASS " + name); }
        static void RequireTextFits(SquadRushGame game,string name,System.Collections.Generic.List<string> checks)
        {
            var overflow=game.Hud.GetComponentsInChildren<TMP_Text>().Where(t=>t.gameObject.activeInHierarchy&&t.isTextOverflowing).Select(t=>t.name+"="+t.text).ToArray();
            Require(overflow.Length==0,name+(overflow.Length>0?": "+string.Join(", ",overflow):""),checks);
        }
        static void VerifyCharacterMeshes(SquadRushGame game, System.Collections.Generic.List<string> checks)
        {
            Require(!ShaderUtil.ShaderHasError(Shader.Find("SquadRush/Toon")),"Commander tint and scenery shader compile without errors",checks);
            var allies=game.GetComponentsInChildren<SquadRushTrooper>(true).Where(t=>t.name=="Ally").ToArray();
            var rig=allies[0]; Mesh body=rig.Body.GetComponent<MeshFilter>().sharedMesh, leg=rig.LeftLeg.GetComponent<MeshFilter>().sharedMesh;
            int triangles=(body.triangles.Length+2*leg.triangles.Length)/3;
            Require(allies.Length==96 && allies.All(t=>t.Body.GetComponent<MeshFilter>().sharedMesh==body && t.LeftLeg.GetComponent<MeshFilter>().sharedMesh==leg && t.RightLeg.GetComponent<MeshFilter>().sharedMesh==leg), "All 96 allies share the new body and leg meshes", checks);
            Require(triangles<=6500 && rig.ColoredRenderers.Length==3 && rig.ColoredRenderers.All(r=>r.sharedMaterial.enableInstancing), "Rounded soldier stays within 6500 triangles and three solid renderers", checks);
            Require(new[]{body,leg}.All(m=>m.vertices.All(v=>Finite(v.x)&&Finite(v.y)&&Finite(v.z)) && m.normals.Length==m.vertexCount && m.normals.All(n=>Finite(n.x)&&Finite(n.y)&&Finite(n.z)&&n.sqrMagnitude>0.1f)), "Generated character surfaces have valid vertices and smooth normals", checks);
            Require(rig.LeftLeg.localPosition.y+leg.bounds.min.y>=-0.01f && body.bounds.max.y<1.45f, "Natural body and boots fit the existing ground and portrait framing", checks);
            rig.Animate(1,0.02f,0); rig.Fire(); rig.ResetPose();
            Require(rig.Body.localPosition==Vector3.zero && Quaternion.Angle(rig.LeftLeg.localRotation,Quaternion.identity)<0.01f && Quaternion.Angle(rig.RightLeg.localRotation,Quaternion.identity)<0.01f && !rig.Muzzle.gameObject.activeSelf, "Reusing a soldier resets stride, body pose and muzzle flash", checks);
            string folder=Path.Combine(Path.GetDirectoryName(Application.dataPath),"Verification"); Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"character-mesh-metrics.txt"),"Body triangles: "+body.triangles.Length/3+"\nLeg triangles each: "+leg.triangles.Length/3+"\nSoldier total triangles: "+triangles+"\nSolid renderers: 3\nShared ally instances: "+allies.Length+"\nTripo generation and processing credits spent: 0\n");
        }
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        static UnityEngine.UI.Button Button(SquadRushGame game, string name) => game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b => b.name == name);
        static UnityEngine.UI.Image UiArt(SquadRushGame game,string name) => game.Hud.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(i=>i.name==name);
        static TMP_Text UiText(SquadRushGame game,string name) => game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t=>t.name==name);
        static void Click(SquadRushGame game, string name)
        {
            var button = Button(game, name);
            if (!button.gameObject.activeInHierarchy || !button.interactable) throw new InvalidOperationException("Button unavailable: " + name);
            button.onClick.Invoke();
        }
        static void Capture(SquadRushGame game, string name,int width=540,int height=960)
        {
            game.Hud.UpdateStats();
            var view = game.View;
            var canvas = game.Hud.GetComponentInChildren<Canvas>();
            var safe = (RectTransform)canvas.transform.Find("SafeArea");
            var texture = new RenderTexture(width, height, 24) { antiAliasing = 2 };
            var previous = RenderTexture.active;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                view.rect = new Rect(0, 0, 1, 1); view.targetTexture = texture;
                view.aspect = (float)width/height;
                view.fieldOfView = 2 * Mathf.Atan(Mathf.Tan(15 * Mathf.Deg2Rad) / view.aspect) * Mathf.Rad2Deg;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = view; canvas.planeDistance = 1;
                safe.anchorMin = Vector2.zero; safe.anchorMax = Vector2.one; safe.offsetMin = safe.offsetMax = Vector2.zero;
                Canvas.ForceUpdateCanvases();
                foreach (TMP_Text text in game.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases(); view.Render();
                if(name.EndsWith("-mobile.png"))
                {
                    var scroll=game.Hud.GetComponentInChildren<UnityEngine.UI.ScrollRect>();
                    if(name=="equipment-mobile.png")foreach(var card in scroll.content.GetComponentsInChildren<UnityEngine.UI.Button>())
                    {
                        Bounds bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport,card.transform);
                        if(bounds.min.x<scroll.viewport.rect.xMin-.5f||bounds.max.x>scroll.viewport.rect.xMax+.5f)throw new InvalidOperationException("Mobile inventory clips column: "+card.name);
                    }
                    foreach(var text in game.Hud.GetComponentsInChildren<TMP_Text>())if(text.isTextOverflowing)throw new InvalidOperationException("Mobile text overflow: "+text.name+"="+text.text);
                }
                RenderTexture.active = texture; image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                string output = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Verification", "Preview");
                Directory.CreateDirectory(output); File.WriteAllBytes(Path.Combine(output, name), image.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; view.targetTexture = null; RenderTexture.active = previous;
                UnityEngine.Object.Destroy(image); texture.Release(); UnityEngine.Object.Destroy(texture);
            }
        }
    }
}
