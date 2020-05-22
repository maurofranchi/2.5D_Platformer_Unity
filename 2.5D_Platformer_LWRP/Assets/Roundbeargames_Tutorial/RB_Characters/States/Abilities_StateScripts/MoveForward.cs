﻿using Roundbeargames.Datasets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roundbeargames
{
    [CreateAssetMenu(fileName = "New State", menuName = "Roundbeargames/AbilityData/MoveForward")]
    public class MoveForward : StateData
    {
        public bool debug;

        public bool AllowEarlyTurn;
        public bool LockDirection;
        public bool LockDirectionNextState;
        public bool Constant;
        public AnimationCurve SpeedGraph;
        public float Speed;
        public float BlockDistance;

        [Header("IgnoreCharacterBox")]
        public bool IgnoreCharacterBox;
        public float IgnoreStartTime;
        public float IgnoreEndTime;

        [Header("Momentum")]
        public bool UseMomentum;
        public float StartingMomentum;
        public float MaxMomentum;
        public bool ClearMomentumOnExit;

        [Header("MoveOnHit")]
        public bool MoveOnHit;
        
        public override void OnEnter(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            characterState.characterControl.animationProgress.LatestMoveForward = this;

            if (AllowEarlyTurn && !characterState.characterControl.animationProgress.disallowEarlyTurn)
            {
                if (!characterState.characterControl.animationProgress.LockDirectionNextState)
                {
                    if (characterState.characterControl.MoveLeft)
                    {
                        characterState.characterControl.FaceForward(false);
                    }
                    if (characterState.characterControl.MoveRight)
                    {
                        characterState.characterControl.FaceForward(true);
                    }
                }
            }

            if (StartingMomentum > 0.001f)
            {
                if (characterState.characterControl.IsFacingForward())
                {
                    characterState.MOMENTUM_DATA.Momentum = StartingMomentum;
                }
                else
                {
                    characterState.MOMENTUM_DATA.Momentum = -StartingMomentum;
                }
            }

            characterState.characterControl.animationProgress.disallowEarlyTurn = false;
            characterState.characterControl.animationProgress.LockDirectionNextState = false;
        }

        public override void UpdateAbility(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (debug)
            {
                Debug.Log(stateInfo.normalizedTime);
            }

            characterState.characterControl.animationProgress.LockDirectionNextState = LockDirectionNextState;

            if (characterState.characterControl.animationProgress.
                LatestMoveForward != this)
            {
                return;
            }

            if (characterState.characterControl.animationProgress.
                IsRunning(typeof(WallSlide)))
            {
                return;
            }

            UpdateCharacterIgnoreTime(characterState.characterControl, stateInfo);

            if (characterState.characterControl.Jump)
            {
                if (characterState.characterControl.animationProgress.Ground != null)
                {
                    animator.SetBool(HashManager.Instance.DicMainParams[TransitionParameter.Jump], true);
                }
            }

            if (characterState.characterControl.Turbo)
            {
                animator.SetBool(HashManager.Instance.DicMainParams[TransitionParameter.Turbo], true);
            }
            else
            {
                animator.SetBool(HashManager.Instance.DicMainParams[TransitionParameter.Turbo], false);
            }

            if (UseMomentum)
            {
                MoveOnMomentum(characterState.characterControl, stateInfo);
            }
            else
            {
                if (Constant)
                {
                    ConstantMove(characterState.characterControl, animator, stateInfo);
                }
                else
                {
                    ControlledMove(characterState.characterControl, animator, stateInfo);
                }
            }
        }

        public override void OnExit(CharacterState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (ClearMomentumOnExit)
            {
                characterState.MOMENTUM_DATA.Momentum = 0f;
            }
        }

        private void MoveOnMomentum(CharacterControl control, AnimatorStateInfo stateInfo)
        {
            float speed = SpeedGraph.Evaluate(stateInfo.normalizedTime) * Speed * Time.deltaTime;
            control.MOMENTUM_DATA.CalculateMomentum(speed, MaxMomentum);

            if (control.MOMENTUM_DATA.Momentum > 0f)
            {
                control.FaceForward(true);
            }
            else if (control.MOMENTUM_DATA.Momentum < 0f)
            {
                control.FaceForward(false);
            }

            if (!IsBlocked(control))
            {
                control.MoveForward(Speed, Mathf.Abs(control.MOMENTUM_DATA.Momentum));
            }
        }

        private void ConstantMove(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (!IsBlocked(control))
            {
                if (MoveOnHit)
                {
                    if (!control.animationProgress.IsFacingAttacker())
                    {
                        control.MoveForward(Speed, SpeedGraph.Evaluate(stateInfo.normalizedTime));
                    }
                    else
                    {
                        control.MoveForward(-Speed, SpeedGraph.Evaluate(stateInfo.normalizedTime));
                    }
                }
                else
                {
                    control.MoveForward(Speed, SpeedGraph.Evaluate(stateInfo.normalizedTime));
                }
            }

            if (!control.MoveRight && !control.MoveLeft)
            {
                animator.SetBool(HashManager.Instance.DicMainParams[TransitionParameter.Move], false);
            }
            else
            {
                animator.SetBool(HashManager.Instance.DicMainParams[TransitionParameter.Move], true);
            }
        }

        private void ControlledMove(CharacterControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (control.MoveRight && control.MoveLeft)
            {
                animator.SetBool(HashManager.Instance.DicMainParams[TransitionParameter.Move], false);
                return;
            }

            if (!control.MoveRight && !control.MoveLeft)
            {
                animator.SetBool(HashManager.Instance.DicMainParams[TransitionParameter.Move], false);
                return;
            }

            if (control.MoveRight)
            {
                if (!IsBlocked(control))
                {
                    control.MoveForward(Speed, SpeedGraph.Evaluate(stateInfo.normalizedTime));
                }
            }

            if (control.MoveLeft)
            {
                if (!IsBlocked(control))
                {
                    control.MoveForward(Speed, SpeedGraph.Evaluate(stateInfo.normalizedTime));
                }
            }

            CheckTurn(control);
        }

        private void CheckTurn(CharacterControl control)
        {
            if (!LockDirection)
            {
                if (control.MoveRight)
                {
                    control.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }

                if (control.MoveLeft)
                {
                    control.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
            }
        }

        void UpdateCharacterIgnoreTime(CharacterControl control, AnimatorStateInfo stateInfo)
        {
            if (!IgnoreCharacterBox)
            {
                control.animationProgress.IsIgnoreCharacterTime = false;
            }

            if (stateInfo.normalizedTime > IgnoreStartTime &&
                stateInfo.normalizedTime < IgnoreEndTime)
            {
                control.animationProgress.IsIgnoreCharacterTime = true;
            }
            else
            {
                control.animationProgress.IsIgnoreCharacterTime = false;
            }
        }
           
        bool IsBlocked(CharacterControl control)
        {
            if (control.BLOCKING_DATA.FrontBlockingDicCount != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}