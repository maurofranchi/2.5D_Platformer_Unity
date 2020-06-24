﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roundbeargames
{
    [CreateAssetMenu(fileName = "New State", menuName = "Roundbeargames/AbilityData/Idle")]
    public class Idle : StateData
    {
        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            animator.SetBool(HashManager.Instance.DicMainParams[TransitionParameter.Attack], false);

            characterState.ROTATION_DATA.LockEarlyTurn = false;
            characterState.ROTATION_DATA.LockDirectionNextState = false;
            characterState.BLOCKING_DATA.ClearFrontBlockingObjDic();
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            characterState.ROTATION_DATA.LockEarlyTurn = false;
            characterState.ROTATION_DATA.LockDirectionNextState = false;

            if (characterState.characterControl.Jump)
            {
                // do nothing
            }
            else
            {
                if (!characterState.ANIMATION_DATA.IsRunning(typeof(Jump)))
                {
                    characterState.JUMP_DATA.Jumped = false;
                }
            }
        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {

        }
    }
}