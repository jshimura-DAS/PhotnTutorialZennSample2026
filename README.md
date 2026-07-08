# Photon Fusion チュートリアル(Zenn版） — シーン起動から入力でプレイヤーが動くまで

このリポジトリは Photon Fusion 2 を用いたチュートリアル実装です。ここでは、Unity で Scene を起動してからローカルプレイヤーがキー入力で動作するまでの一連の流れを簡潔にまとめます。実装ファイルへの参照と、Photon Fusion の公式ドキュメントへのリンクを含みます。

---

## 概要
1. Scene 起動 → GameLauncher が NetworkRunner を生成・起動
2. セッション参加 → OnPlayerJoined で自分のアバターを Spawn
3. Spawn により PlayerAvatar.Spawned() が呼ばれ初期化
4. シミュレーションループ（FixedUpdateNetwork）で入力→NetworkCharacterController.Move/Jump を呼ぶ
5. Networked プロパティや NetworkMecanimAnimator による同期

主要な実装ファイル
- Assets/FusionTutorial/Scripts/GameLauncher.cs
  - NetworkRunner を Instantiate し、StartGame(StartGameArgs { GameMode = GameMode.Shared }) を呼ぶ
  - runner.Spawn(...) でアバターを生成
  - 参照: runner.StartGame / runner.Spawn — https://doc.photonengine.com/ja-jp/fusion/v2/tutorials/shared-mode-basics/1-getting-started

- Assets/FusionTutorial/Scripts/PlayerAvatar.cs
  - [Networked] NetworkString<_16> NickName などのネットワークプロパティ
  - Spawned() でコンポーネント初期化（NetworkCharacterController, NetworkMecanimAnimator, PlayerAvatarView）
  - FixedUpdateNetwork() で移動とジャンプを呼ぶ（現状は Keyboard.current を参照）
  - Fusion 固有ライフサイクル: Spawned / FixedUpdateNetwork
  - 参照（NetworkBehaviour・プレイヤー入力）: https://zenn.dev/o8que/books/photon-fusion/viewer/network-behaviour

- Assets/FusionTutorial/Scripts/PlayerAvatarView.cs
  - Cinemachine の優先度を上げてカメラ割当、名前ラベル更新

---

## 詳細フロー（順序とポイント）
1) GameLauncher.Start()
   - Instantiate(networkRunnerPrefab)
   - networkRunner.AddCallbacks(this)
   - await networkRunner.StartGame(new StartGameArgs { GameMode = GameMode.Shared })
   - 参考: 共有モードチュートリアル — https://doc.photonengine.com/ja-jp/fusion/v2/tutorials/shared-mode-basics/1-getting-started

2) OnPlayerJoined(runner, player)
   - player == runner.LocalPlayer を確認して自分のアバターを runner.Spawn(...) で生成
   - onBeforeSpawned でネットワークプロパティ（NickName 等）を初期化
   - 参照: runner.Spawn — https://zenn.dev/o8que/books/photon-fusion/viewer/network-object

3) PlayerAvatar.Spawned()
   - NetworkCharacterController や NetworkMecanimAnimator を取得
   - PlayerAvatarView に NickName を反映
   - HasStateAuthority によるカメラ割当
   - 参照: NetworkBehaviour のライフサイクル — https://zenn.dev/o8que/books/photon-fusion/viewer/network-behaviour

4) 入力と移動
   - 現在の実装は FixedUpdateNetwork 内で Keyboard.current を直接参照している
   - 推奨: INetworkRunnerCallbacks.OnInput で入力を収集して NetworkInput に詰め、シミュレーション側で扱う（メインスレッド・シミュレーション整合性のため）
   - 参照: プレイヤー入力章 — https://zenn.dev/o8que/books/photon-fusion/viewer/player-input

5) 同期
   - [Networked] 属性でプロパティを自動同期
   - NetworkMecanimAnimator 等ビルトインコンポーネントでアニメーション同期

---

## 注意点 / トラブルシューティング
- Unity の Active Input Handling が "Input System Package (New)" の場合、古い `UnityEngine.Input` 呼び出しは例外になる。対策は Project Settings → Player → Active Input Handling を `Both` にするか、Input System に対応した実装へ移行する。
- FixedUpdateNetwork は Fusion のシミュレーションティックの一部として呼ばれるため、入力の取得は OnInput を使うのが安全で推奨される。

---

## 参考リンク
- リポジトリ: https://github.com/jshimura-DAS/PhotnTutorialZennSample2026
- Zenn チュートリアル（Photon Fusion 実装解説）: https://zenn.dev/o8que/books/photon-fusion/viewer/tutorial
- Photon Fusion 公式ドキュメント（入門）: https://doc.photonengine.com/ja-jp/fusion/v2/fusion-intro
- 共有モード / Movement & Camera: https://doc.photonengine.com/ja-jp/fusion/v2/tutorials/shared-mode-basics/3-movement-and-camera

---
