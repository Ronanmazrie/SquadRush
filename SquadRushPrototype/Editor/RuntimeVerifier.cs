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
                Capture(game,"menu.png");Capture(game,"home-mobile.png",390,844);VerifyIconSet(game,checks);
                string[] navigation={"HomeCommand","HomeEquipment","HomeCharacters","HomeGacha","HomeMissions","HomeMail"};
                FrontEndPage[] destinations={FrontEndPage.Home,FrontEndPage.Equipment,FrontEndPage.Characters,FrontEndPage.Gacha,FrontEndPage.Missions,FrontEndPage.Mail};
                for(int i=0;i<navigation.Length;i++){Click(game,navigation[i]);Require(game.Hud.CurrentPage==destinations[i]&&navigation.All(n=>Button(game,n).gameObject.activeInHierarchy),"Bottom tab opens "+destinations[i]+" in one tap and remains visible",checks);}
                Require(!game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Any(b=>new[]{"HomeEvents","HomeStages","HomeLogin","GachaCharacters","GachaEquipment","GachaRates","CampaignToPermanent"}.Contains(b.name)),"Duplicate shortcuts and probability controls are removed",checks);
                Require(Button(game,"MailClaimRelease").interactable&&game.Account.Tickets==0,"Mail stores the unclaimed gift without automatically granting it",checks);Capture(game,"mail.png");Capture(game,"mail-mobile.png",390,844);
                Click(game,"HomeGacha");Capture(game,"gacha.png");Capture(game,"recruitment-mobile.png",390,844);
                Require(!Button(game,"DrawOne").interactable&&!Button(game,"DrawTen").interactable,"New profile cannot draw without earned tickets or gems",checks);
                Require(game.Hud.GetComponentsInChildren<TMP_Text>().All(t=>!t.text.Contains("%")&&!t.text.Contains("キャラ率")&&!t.text.Contains("提供割合")),"Permanent gacha displays no character probability",checks);
                Click(game,"EventGachaTab");Require(game.Hud.ShowingGachaEvents&&Button(game,"EventDrawTen").gameObject.activeInHierarchy&&!Button(game,"DrawTen").gameObject.activeInHierarchy,"Event tab contains actual event draw controls rather than gifts",checks);
                Require(game.Hud.GetComponentsInChildren<TMP_Text>().All(t=>!t.text.Contains("%")),"Event gacha displays no character probability",checks);Capture(game,"events.png");Capture(game,"events-mobile.png",390,844);
                Click(game,"HomeCharacters");Click(game,"Character1");Require(!Button(game,"SelectCharacter").interactable&&UiText(game,"CharacterSkillDescription").text.Contains("範囲攻撃"),"Locked character inspection shows the skill but prevents selection",checks);Capture(game,"characters-locked.png");Capture(game,"characters-mobile.png",390,844);
                Click(game,"HomeMissions");Click(game,"LoginRewardsTab");Click(game,"ClaimLogin");Require(game.Account.Gems==100&&game.Account.Tickets==1&&!Button(game,"ClaimLogin").interactable,"Login subtab under Missions grants its exact reward once",checks);Capture(game,"login.png");Capture(game,"login-mobile.png",390,844);
                Click(game,"HomeMissions");Require(!Button(game,"ClaimMission0").interactable,"Incomplete daily missions cannot be claimed through the UI",checks);Capture(game,"missions.png");Capture(game,"missions-mobile.png",390,844);
                Click(game,"HomeEquipment");var illustratedHero=UiArt(game,"EquipmentHeroIllustration");Require(illustratedHero.sprite!=null&&illustratedHero.transform.parent.GetComponentsInChildren<UnityEngine.UI.RawImage>(true).Length==0,"Equipment uses a 2D anime hero",checks);
                Require(Enumerable.Range(0,6).Select(i=>UiArt(game,"InventoryIllustration"+i).sprite).Distinct().Count()==6,"Six equipment illustrations remain distinct",checks);
                Click(game,"InventoryItem0");Require(!Button(game,"EquipmentAction").interactable&&UiText(game,"DetailSource").text.Contains("STAGE 01"),"Locked equipment shows the earliest random-drop stage",checks);RequireTextFits(game,"Item inspection fits",checks);Capture(game,"equipment-locked.png");Click(game,"CloseEquipmentDetail");
                Click(game,"HomeCommand");Require(!Button(game,"HomeNextStage").interactable,"Stage screen prevents selecting locked stages",checks);Capture(game,"stages.png");Click(game,"HomeSortie");Require(game.State==RunState.Playing&&game.Hud.CurrentPage==FrontEndPage.Hidden&&navigation.All(n=>Button(game,n).gameObject.activeInHierarchy&&!Button(game,n).interactable),"Battle keeps all six bottom tabs visible and prevents navigation during a run",checks);
                Capture(game,"ground-before.png");game.Tick(.25f,false);Require(Mathf.Abs(game.GroundTravel-.875f)<.001f,"Play-mode ground moves with terrain distance",checks);Capture(game,"ground-after.png");
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
                Require(game.Equipment.Quantity(game.LastStageDrop.Id,game.LastStageDrop.Rarity)>0 && new SquadRushEquipment(storageKey).Quantity(game.LastStageDrop.Id,game.LastStageDrop.Rarity)>0, "Boss clear permanently saves the actual random drop", checks);
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
                if(!game.Equipment.Owns(0))game.Equipment.Acquire(0);
                Click(game, "HomeEquipment"); Click(game, "InventoryItem0"); Click(game, "EquipmentAction");
                Require(game.Equipment.IsEquipped(0) && game.WeaponDamage == 2 && game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "ItemState0").text.Contains("装備中"), "Equipment button equips the earned rifle and updates its state", checks);
                Require(UiArt(game,"EquippedIllustration0").sprite==UiArt(game,"InventoryIllustration0").sprite && UiText(game,"StatValue0").text=="2","Equipped slot illustration and power value reflect the actual rifle",checks);
                yield return null; Capture(game, "equipment.png");
                RequireTextFits(game,"Equipment page labels fit their visible controls",checks);
                Click(game, "EquippedSlotWeapon"); Capture(game, "equipment-detail.png");
                Require(Button(game, "EquipmentAction").interactable && Button(game, "EquipmentAction").GetComponentInChildren<TMP_Text>().text == "装備を外す", "The equipped weapon slot opens its own removable item detail", checks);
                Click(game, "CloseEquipmentDetail");
                Click(game, "HomeCommand"); Click(game, "HomePreviousStage"); Click(game, "HomeSortie");
                Require(game.StageNumber == 1 && game.WeaponDamage == 2 && game.Equipment.IsEquipped(0), "An earlier unlocked stage starts with permanent equipped bonuses", checks);
                game.TogglePause(); Click(game, "HomeCommand");
                Require(game.State == RunState.Ready && game.Hud.CurrentPage == FrontEndPage.Home && game.ActiveTargets == 0 && game.ActiveBullets == 0, "Pause screen returns cleanly to Home", checks);
                game.ConfigureVerification(storageKey); Click(game, "HomeEquipment");
                Require(game.Equipment.IsEquipped(0) && game.Hud.GetComponentsInChildren<TMP_Text>(true).Single(t => t.name == "ItemState0").text.Contains("装備中"), "Reload restores both the equipped rifle and equipment UI", checks);
                Click(game, "EquippedSlotWeapon"); Click(game, "EquipmentAction");
                Require(game.WeaponDamage == 1 && !game.Equipment.IsEquipped(0), "Equipment button also unequips the rifle", checks);
                Click(game, "HomeCommand");
                Click(game, "HomePreviousStage");
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
                    game.StartVerificationStage(stage,123);game.Hud.ShowHome();stageSprites.Add(UiArt(game,"StageSelectionIllustration").sprite);
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
                Click(game,"HomeCharacters");Click(game,"Character"+earned);Click(game,"SelectCharacter");
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
                Click(game,"HomeMail");int campaignGems=game.Account.Gems,campaignPity=game.Account.Pity,campaignPulls=game.Account.TotalPulls;Click(game,"MailClaimRelease");
                Require(game.Account.Tickets==10&&game.Account.ReleaseCampaignClaimed&&!Button(game,"MailClaimRelease").interactable&&game.Account.Gems==campaignGems&&game.Account.Pity==campaignPity&&game.Account.TotalPulls==campaignPulls,"Mail claim grants ten tickets once without drawing or spending gems",checks);Capture(game,"mail-received.png");Capture(game,"mail-received-mobile.png",390,844);
                string eventFixture=PlayerPrefs.GetString(SquadRushAccount.StorageKey(accountUiKey)).Replace("\"eventPity\":0","\"eventPity\":89");PlayerPrefs.SetString(SquadRushAccount.StorageKey(accountUiKey),eventFixture);game.ConfigureVerification(accountUiKey);Click(game,"HomeGacha");Click(game,"EventGachaTab");Click(game,"EventUseTickets");Click(game,"EventDrawTen");yield return null;
                Require(game.Account.LastResults.Length==10&&game.Account.LastResults[0].kind==DrawKind.Character&&game.Account.LastResults[0].id==1&&game.Account.Tickets==0&&game.Account.Gems==campaignGems&&game.Account.Pity==campaignPity&&game.Account.LastBanner==DrawBanner.AkaneEvent,"Event ten-pull awards featured Akane at its own pity and preserves permanent pity and gems",checks);
                Require(navigation.All(n=>Button(game,n).gameObject.activeInHierarchy&&Button(game,n).interactable),"Bottom tabs stay usable over draw results",checks);Capture(game,"event-ten-results.png");Capture(game,"event-results-mobile.png",390,844);
                Click(game,"HomeCharacters");Require(game.Hud.CurrentPage==FrontEndPage.Characters&&!Button(game,"CloseDrawResults").gameObject.activeInHierarchy,"Bottom navigation leaves draw results directly without an extra close step",checks);
                int eventPityAfter=game.Account.EventPity;game.ConfigureVerification(accountUiKey);Click(game,"HomeMail");Require(!Button(game,"MailClaimRelease").interactable&&!game.ClaimReleaseCampaign()&&game.Account.Tickets==0&&game.Account.EventPity==eventPityAfter&&game.Account.Pity==campaignPity,"Reload preserves both banner counters and the already-received legacy gift",checks);
                PlayerPrefs.SetInt(accountUiKey,12);game.ConfigureVerification(accountUiKey);Capture(game,"stage-12.png");Capture(game,"stage-12-mobile.png",390,844);int countBefore=game.Equipment.OwnedCount;game.Begin();game.ChangeSquad(997);game.SpawnTarget(TargetKind.Boss,0,20,1);for(int i=0;i<180&&game.State==RunState.Playing;i++)game.Tick(.02f,false);
                Require(game.State==RunState.Won&&game.Equipment.OwnedCount==countBefore+1&&game.LastStageDrop.Rarity>=2&&game.LastStageDrop.Rarity<=3,"Stage twelve boss grants one actual rare or epic drop",checks);Capture(game,"stage-12-clear.png");Capture(game,"stage-12-clear-mobile.png",390,844);RequireTextFits(game,"Higher-rarity clear reward fits beside persistent navigation",checks);
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
        static void VerifyIconSet(SquadRushGame game,System.Collections.Generic.List<string> checks)
        {
            var parent=game.Hud.GetComponentsInChildren<RectTransform>(true).Single(r=>r.name=="SafeArea");
            var sheet=new GameObject("IconVerification",typeof(RectTransform),typeof(UnityEngine.UI.Image));sheet.transform.SetParent(parent,false);
            var root=sheet.GetComponent<RectTransform>();root.anchorMin=new Vector2(.035f,.115f);root.anchorMax=new Vector2(.965f,.89f);root.offsetMin=root.offsetMax=Vector2.zero;sheet.GetComponent<UnityEngine.UI.Image>().color=new Color(.045f,.08f,.14f);
            var icons=new System.Collections.Generic.List<SquadRushUiIcon>();int n=0;
            foreach(SquadIcon kind in Enum.GetValues(typeof(SquadIcon)))
            {
                float x=(n%3)/3f,y=1-(n/3+1)/8f;
                var cell=new GameObject("Sample"+kind,typeof(RectTransform));cell.transform.SetParent(root,false);var rect=cell.GetComponent<RectTransform>();rect.anchorMin=new Vector2(x,y);rect.anchorMax=new Vector2(x+1/3f,y+1/8f);rect.offsetMin=rect.offsetMax=Vector2.zero;
                var picture=new GameObject("Icon"+kind,typeof(RectTransform),typeof(CanvasRenderer),typeof(SquadRushUiIcon));picture.transform.SetParent(rect,false);var imageRect=picture.GetComponent<RectTransform>();imageRect.anchorMin=new Vector2(.17f,.24f);imageRect.anchorMax=new Vector2(.83f,.98f);imageRect.offsetMin=imageRect.offsetMax=Vector2.zero;var icon=picture.GetComponent<SquadRushUiIcon>();icon.Kind=kind;icon.raycastTarget=false;icons.Add(icon);
                var caption=new GameObject("Caption"+kind,typeof(RectTransform),typeof(TextMeshProUGUI));caption.transform.SetParent(rect,false);var label=caption.GetComponent<TextMeshProUGUI>();label.font=Resources.Load<TMP_FontAsset>("SquadFont");label.text=kind.ToString();label.fontSize=15;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=new Vector2(1,.24f);label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;n++;
            }
            Capture(game,"icons.png");
            Canvas.ForceUpdateCanvases();
            var fingerprints=new System.Collections.Generic.HashSet<string>();
            foreach(var icon in icons)
            {
                var mesh=icon.canvasRenderer.GetMesh();Rect rect=icon.rectTransform.rect;
                bool bounded=mesh!=null&&mesh.vertexCount>0&&mesh.vertices.All(p=>!float.IsNaN(p.x)&&!float.IsInfinity(p.x)&&!float.IsNaN(p.y)&&!float.IsInfinity(p.y)&&p.x>=rect.xMin-.01f&&p.x<=rect.xMax+.01f&&p.y>=rect.yMin-.01f&&p.y<=rect.yMax+.01f);
                if(!bounded||icon.raycastTarget)throw new InvalidOperationException("Invalid decorative icon geometry: "+icon.Kind+" rect="+rect+" mesh="+(mesh==null?"null":mesh.bounds.ToString()));
                fingerprints.Add(string.Join(",",mesh.vertices.Select(p=>p.ToString("F3")))+string.Join(",",mesh.colors32.Select(c=>c.ToString())));
            }
            Require(fingerprints.Count==icons.Count,"All 23 icons are distinct, bounded, finite and do not intercept touches",checks);
            sheet.SetActive(false);UnityEngine.Object.Destroy(sheet);
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
