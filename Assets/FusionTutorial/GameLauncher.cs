using Fusion;
using UnityEngine;

public class GameLauncher : MonoBehaviour
{
    [SerializeField]
    private NetworkRunner networkRunnerPrefab;

    private async void Start()
    {
        // NetworkRunnerを生成する
        var networkRunner = Instantiate(networkRunnerPrefab);
        // 共有モードのセッションに参加する
        var result = await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared
        });
        // 結果をコンソールに出力する
        Debug.Log(result);
    }
}