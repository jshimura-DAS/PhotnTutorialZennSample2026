using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAvatar : NetworkBehaviour
{
    // プレイヤー名のネットワークプロパティを定義する
    [Networked]
    public NetworkString<_16> NickName { get; set; }

    private NetworkCharacterController characterController;

    public override void Spawned()
    {
        characterController = GetComponent<NetworkCharacterController>();

        var view = GetComponent<PlayerAvatarView>();
        // プレイヤー名をテキストに反映する
        view.SetNickName(NickName.Value);
        // 自身がアバターの権限を持っているなら、カメラの追従対象にする
        if (HasStateAuthority)
        {
            view.MakeCameraTarget();
        }
    }

    public override void FixedUpdateNetwork()
    {
        // 移動
        var cameraRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);

        var keyboard = Keyboard.current;
        var inputDirection = new Vector3(
            (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
            0f,
            (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0)
        );
        // characterController.Move(inputDirection);
        characterController.Move(cameraRotation * inputDirection);
        // ジャンプ
        if (keyboard.spaceKey.isPressed)
        {
            characterController.Jump();
        }
    }
}
