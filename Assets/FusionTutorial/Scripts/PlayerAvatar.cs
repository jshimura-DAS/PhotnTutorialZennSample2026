using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAvatar : NetworkBehaviour
{
    // プレイヤー名のネットワークプロパティを定義する
    [Networked]
    public NetworkString<_16> NickName { get; set; }

    private NetworkCharacterController characterController;
    private NetworkMecanimAnimator networkAnimator;
    
    private float defaultMaxSpeed;
    private float sprintMaxSpeed;

    public override void Spawned()
    {
        characterController = GetComponent<NetworkCharacterController>();
        networkAnimator = GetComponentInChildren<NetworkMecanimAnimator>();

        var view = GetComponent<PlayerAvatarView>();
        // プレイヤー名をテキストに反映する
        view.SetNickName(NickName.Value);
        // 自身がアバターの権限を持っているなら、カメラの追従対象にする
        if (HasStateAuthority)
        {
            view.MakeCameraTarget();
        }
        
        // デフォルトと走行時の最大速度を保存
        defaultMaxSpeed = characterController.maxSpeed;
        sprintMaxSpeed = defaultMaxSpeed * 6f;
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
        
        // 左Shift キー押下時に最大速度を増加
        bool isSprinting = keyboard.leftShiftKey.isPressed;
        characterController.maxSpeed = isSprinting ? sprintMaxSpeed : defaultMaxSpeed;
        
        characterController.Move(cameraRotation * inputDirection);
        
        // ジャンプ
        if (keyboard.spaceKey.isPressed)
        {
            characterController.Jump();
        }
        
        // アニメーション（ここでは説明を簡単にするため、かなり大雑把な設定になっています）
        var animator = networkAnimator.Animator;
        var grounded = characterController.Grounded;
        var vy = characterController.Velocity.y;
        animator.SetFloat("Speed", characterController.Velocity.magnitude);
        animator.SetBool("Jump", !grounded && vy > 4f);
        animator.SetBool("Grounded", grounded);
        animator.SetBool("FreeFall", !grounded && vy < -4f);
        animator.SetFloat("MotionSpeed", 1f);
    }
}
