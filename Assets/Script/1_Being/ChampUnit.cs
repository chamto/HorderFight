﻿using System.Collections;
using UnityEngine;
//using UnityEngine.Assertions;
using UtilGS9;

namespace HordeFight
{
    //========================================================

    /// <summary>
    /// 뛰어난 존재 
    /// </summary>
    public class ChampUnit : Being
    {

        //직책 
        //public enum eJobPosition
        //{
        //    None = 0,
        //    Squad_Leader, //분대장 10
        //    Platoon_Leader, //소대장 20
        //    Company_Commander, //중대장 100
        //    Battalion_Commander, //대대장 300
        //    Division_Commander, //사단장 , 독립된 부대단위 전술을 펼칠수 있다 3000
        //}
        //public eJobPosition _jobPosition = eJobPosition.None;

        //==================================================

        public ushort _level = 1;

        //능력치1 
        public ushort _power = 1;
        public float _mt_range_min = 0.2f; //충돌원 바깥부터 시작되는 길이값
        public float _mt_range_max = 0.5f; //충돌원 바깥부터 시작되는 길이값

        //보조정보 
        //private Geo.Sphere _collider;
        //private Vector3 _direction = Vector3.forward;

        //==================================================

        //주시대상
        public Being _looking = null;

        //소유아이템
        public Inventory _inventory = null;

        //전용effect
        //private SpriteRenderer _bar_red = null;
        //private SpriteRenderer _bar_blue = null;


        //전용UI
        //public int _UIID_circle_collider = -1;
        //public int _UIID_hp = -1;
        public LineControl.Info _ui_circle;
        public LineControl.Info _ui_hp;


        //==================================================
        //동작정보
        //==================================================
        public SkillControl _skillControl = new SkillControl();
        public Skill_Idle _skill_idle = new Skill_Idle();
        public Skill_Move _skill_move = new Skill_Move();
        public Skill_Attack _skill_attack = new Skill_Attack();

        //진영정보
        //public Camp _belongCamp = null; //소속 캠프
        //public Camp.eKind _campKind = Camp.eKind.None;

        //public Geo.Sphere _activeRange = Geo.Sphere.Zero;

        //==================================================

        //가지정보
        public Bone _bone = new Bone();
        public Limbs _limbs = null;
        //==================================================

        public Behavior _behavior_cur = null; //임시변수 


        public float attack_range_min
        {
            get { return this._collider_radius + _mt_range_min * GridManager.MeterToWorld; }
        }
        public float attack_range_max
        {
            get { return this._collider_radius + _mt_range_max * GridManager.MeterToWorld; }
        }

        //==================================================



        //private void Start()
        //{
        //          //DebugWide.LogBlue("ChampUnit");
        //          //this.Init();
        //}

        //private void Update()
        //{
        //    this.UpdateAll();
        //}

        //LineControl LINE_CONTROL = null;

        public override void Init()
        {
            base.Init();

            _skill_idle.Init(_skillControl, this, Skill.eName.Idle);
            _skill_move.Init(_skillControl, this, Skill.eName.Move_0);
            _skill_attack.Init(_skillControl, this, Skill.eName.Attack_Strong_1);
            //_skill_attack.Init(_skillControl, this, Skill.eName.Attack_Combo_1);
            _skillControl.Init(this, _skill_idle); //초기 애니 설정 

            //_activeRange.radius = GridManager.ONE_METER * 1f;

            //=====================================================

            //_effect.SetActive(Effect.eKind.Bar_Red, false);

            _limbs.SetActive_Sight(false);
            _limbs.SetActive_Waist(false);
            _limbs.SetActive_LeftHand(false);
            _limbs.SetActive_RightHand(false);
            _limbs.SetActive_EquipArmed(false);

            //=====================================================
            // 전용 ui 설정 

            //todo : 성능이 무척 안좋은 처리 , 스프라이트HP바로 바꾸기 
            //_ui_circle = SingleO.lineControl.Create_Circle_AxisY(this.transform, _activeRange.radius, Color.green);
            //_ui_hp = SingleO.lineControl.Create_LineHP_AxisY(this.transform);
            //_ui_circle.gameObject.SetActive(false);
            //_ui_hp.gameObject.SetActive(false);
            ////SingleO.lineControl.SetScale(_UIID_circle_collider, 2f);
        }

        public class Skill_Idle : Skill.BaseInfo
        {
            public override void On_Start()
            {
                being._ani.PlayNow(being._kind, eAniBaseKind.idle, being._move._eDir8);
            }

            public void Play()
            {
                skillControl.PlayNow(this);
            }
        }


        public class Skill_Move : Skill.BaseInfo
        {
            public Vector3 dir;
            public float second;
            public bool forward;

            //public override void On_Start()
            //{
            //    On_Running();
            //}

            public override void On_Running()
            {
                dir.y = 0;
                being._move.SetNextMoving(false);
                being._move.Move_Forward(dir, 1f, second);
                eDirection8 eDirection = being._move._eDir8;

                //전진이 아니라면 애니를 반대방향으로 바꾼다 (뒷걸음질 효과)
                if (false == forward)
                    eDirection = Misc.GetDir8_Reverse_AxisY(eDirection);

                //Play함수는 기존재생중 애니가 있는 경우 바로 전환이 안된다. 예)공격애니중 이동애니 전환시 
                //PlayNow함수를 사용해야 이동애니로 바로 바뀐다 
                being._ani.PlayNow(being._kind, eAniBaseKind.move, eDirection);

                //DebugWide.LogGreen(skillControl._state_current + "  " + skill._name + "  " + skillControl._timeDelta);
            }

            public void Play(Vector3 dir, float second, bool forward)
            {
                this.dir = dir;
                this.second = second;
                this.forward = forward;
                skillControl.PlayNow(this);
            }
        }

        public class Skill_Attack : Skill.BaseInfo
        {
            public Vector3 dir;
            public Being target;
            public Vector3 start_dir;

            public  override void On_Start()
            {
                ChampUnit champ = being as ChampUnit;
                start_dir = dir;
                champ._behavior_cur = skillControl._behavior_cur;


                //being._ani._animator.speed = 1f/skillControl._behavior_cur.runningTime;
                //_ani._animator.Play(_ani.ANI_STATE_ATTACK, 0, 0f); //애니의 노멀타임을 설정한다
                being._move._eDir8 = Misc.GetDir8_AxisY(start_dir);
                being._move._direction = start_dir;
                being._ani.PlayNow(being._kind, eAniBaseKind.attack, being._move._eDir8);


                //DebugWide.LogBlue(skillControl._state_current + "  " + skill._name + "  " + skillControl._timeDelta);
            }
            public override void On_Running()
            {
                //float inpol = Interpolation.Calc(Interpolation.eKind.easeOutCirc, 0, 1f, skillControl._normalTime);
                float angle = skillControl._behavior_cur.angle;
                int rotateCount = 0;
                dir =  Quaternion.AngleAxis(skillControl._normalTime * (angle + (360f * rotateCount)), ConstV.v3_up) * start_dir;

                DebugWide.LogBlue("running : "+ skillControl._timeDelta + "   " + skillControl._normalTime);

                eDirection8 eDirection = Misc.GetDir8_AxisY(dir);
                //being._move._eDir8 = Misc.GetDir8_AxisY(dir);
                //being._move._direction = dir;
                //being._ani.PlayNow(being._kind, eAniBaseKind.attack, eDirection, skillControl._normalTime);
                being._ani.PlayNow(being._kind, eAniBaseKind.attack, eDirection, 0.6f);

            }
            public override void On_Wait()
            {
                float angle = skillControl._behavior_cur.angle;
                int rotateCount = 0;
                dir = Quaternion.AngleAxis(skillControl._normalTime * (angle + (360f * rotateCount)), ConstV.v3_up) * start_dir;

                DebugWide.LogBlue("wait : "+ skillControl._timeDelta + "   " + skillControl._normalTime);

                eDirection8 eDirection = Misc.GetDir8_AxisY(dir);
                being._ani.PlayNow(being._kind, eAniBaseKind.attack, eDirection, 0.6f);
            }
            public override void On_End()
            {
                DebugWide.LogBlue("end : " + skillControl._timeDelta + "   " + skillControl._normalTime);

                //being._ani._animator.speed = 1f;
                //float angle = skillControl._behavior_cur.angle;
                //int rotateCount = 0;
                //dir = Quaternion.AngleAxis(1f * (angle + (360f * rotateCount)), ConstV.v3_up) * start_dir;
                //being._move._eDir8 = Misc.GetDir8_AxisY(dir);
                //being._move._direction = dir;
                //being._ani.PlayNow(being._kind, eAniBaseKind.attack, being._move._eDir8);


                //being._move._eDir8 = Misc.GetDir8_AxisY(start_dir);
                being._ani.PlayNow(being._kind, eAniBaseKind.idle, being._move._eDir8); //애니를 초기방향 아이들로 바꾼다
                //DebugWide.LogRed(skillControl._state_current + "  " + skill._name + "  " + skillControl._timeDelta);
            }

            public void Play(Vector3 dir, Being target)
            {
                this.dir = dir;
                this.target = target;
                skillControl.PlayNext(this);
            }
        }

		public override void Idle()
		{
            _skill_idle.Play();
		}

		public override bool UpdateAll()
        {
            bool result = base.UpdateAll();
            if (true == result)
            {
                //========================================
                _skillControl.Update(); //행동 진행 갱신 

                //========================================



                //ApplyUI_HPBar();
                //_effect.Apply_BarRed((float)_hp_cur / (float)_hp_max);

                if (null != (object)_limbs)
                {
                    _limbs.Update_Frame(); //뼈대 정보 갱신 

                    //MovingModel 처리가 와야 한다 
                    // - 새로 구한 회전값 적용 
                    // - 새로 구한 회전값에서 손의 위치 다시 계산 

                    _limbs.Update_View(); //보이는 정보 처리 

                    //_limbs.Rotate(_move._direction); //move 에서 오일러각을 따로 구한것을 사용하도록 코드 수정하기     
                }

            }

            return result;
        }

        //public void Attack(Vector3 dir)
        //{
        //    this.Attack(dir, null);
        //}

        public bool Throw(Vector3 targetPos)
        {
            if (null == (object)_shot || false == _shot._on_theWay)
            {
                _shot = SingleO.objectManager.GetNextShot();
                if(null != (object)_shot)
                {
                    float targetToLen = (targetPos - _getPos3D).sqrMagnitude;
                    if (targetToLen < attack_range_min * attack_range_min)
                        return false;
                    if (targetToLen > attack_range_max * attack_range_max)
                        return false;

                    _shot.ThrowThings(this, _getPos3D, targetPos);    
                    return true;
                }

            }

            return false;
        }

        public Shot _shot = null;
        Vector3 _appointmentDir = ConstV.v3_zero;
        public Being _target = null;
        public void Attack(Vector3 dir, Being target)
        {
            
            _move._eDir8 = Misc.GetDir8_AxisY(dir);


            _ani.Play(_kind, eAniBaseKind.attack, _move._eDir8);

            //_animator.Play(ANI_STATE_ATTACK, 0, 0.0f); //애니의 노멀타임을 설정한다  
            //_animator.speed = 0.5f; //속도를 설정한다 
            //_target = target;

            //1회 공격중 방향변경 안되게 하기. 1회 공격시간의 80% 경과시 콜백호출 하기.
            _appointmentDir = dir;
            Update_AnimatorState(_ani.ANI_STATE_ATTACK, 0.8f);

            //임시코드 
            //if (eKind.spearman == _kind || eKind.archer == _kind || eKind.catapult == _kind || eKind.cleric == _kind || eKind.conjurer == _kind)
            //{

            //    if (null == (object)_shot || false == _shot._on_theWay)
            //    {
            //        _shot = SingleO.objectManager.GetNextShot();
            //        if (null != (object)_shot)
            //        {
            //            Vector3 targetPos = ConstV.v3_zero;
            //            if (null != (object)target)
            //            {
            //                //targetPos = target.transform.position;
            //                targetPos = target.GetPos3D();
            //            }
            //            else
            //            {
            //                _appointmentDir.Normalize();
            //                //targetPos = this.transform.position + _appointmentDir * attack_range_max;
            //                targetPos = _getPos3D + _appointmentDir * attack_range_max;
            //            }
            //            //_shot.ThrowThings(this, this.transform.position, targetPos);
            //            _shot.ThrowThings(this, _getPos3D, targetPos);
            //        }
            //    }

            //}
        }

        //임시처리
        public override void OnAniState_Start(int hash_state)
        {
            //DebugWide.LogBlue("OnAniState_Start :" + hash_state); //chamto test

            if (hash_state == _ani.ANI_STATE_ATTACK)
            {
                //예약된 방향값 설정
                _move._eDir8 = Misc.GetDir8_AxisY(_appointmentDir);
                _move._direction = Misc.GetDir8_Normal3D_AxisY(_move._eDir8);

                _ani.Play(_kind, eAniBaseKind.attack, _move._eDir8);
                //Switch_Ani(_kind, eAniBaseKind.attack, _move._eDir8);

                //this.SetActiveEffect(eEffectKind.Hand_Right, true);
            }
        }

        //임시처리
        public override void OnAniState_End(int hash_state)
        {
            //DebugWide.LogYellow("OnAniState_End :" + hash_state + "  " + _hash_attack + "   "+ _target); //chamto test

            if (hash_state == _ani.ANI_STATE_ATTACK)
            {

                //목표에 피해를 준다
                if (null != _target)
                {
                    //DebugWide.LogYellow("OnAniState_End :" + hash_state); //chamto test

                    //this.SetActiveEffect(eEffectKind.Hand_Right, false);

                    _target.AddHP(-1);

                    ChampUnit target_champ = _target as ChampUnit;
                    if (null != target_champ)
                    {
                        StartCoroutine(target_champ.Damage());
                        //target_champ._effect.Apply_BarRed((float)target_champ._hp_cur / (float)target_champ._hp_max);
                    }

                }

            }
        }


        bool __in_corutin_Damage = false;
        public IEnumerator Damage()
        {
            //같은 코루틴을 요청하면 빨강색으로 변경후 종료한다  
            //if (true == __in_corutin_Damage)
            //{
            //    _sprRender.color = Color.red;
            //    yield break;
            //}

            //for (int i = 0; i < 10; i++)
            //{
            //    __in_corutin_Damage = true;

            //    _sprRender.color = Color.Lerp(Color.red, Color.white, i / 10f);
            //    //_sprRender.color = Color.red;
            //    yield return new WaitForSeconds(0.05f);
            //}

            //this.SetActiveEffect(eEffectKind.Emotion, true);
            _ani._sprRender.color = Color.red;

            if (true == __in_corutin_Damage)
                yield break;

            __in_corutin_Damage = true;
            yield return new WaitForSeconds(0.5f);

            //this.SetActiveEffect(eEffectKind.Emotion, false);
            _ani._sprRender.color = Color.white;
            __in_corutin_Damage = false;

            yield break;
        }


        //____________________________________________
        //                  충돌반응
        //____________________________________________

        public override void OnCollision_MovePush(Being dst, Vector3 dir, float meterPerSecond)
        {
            Move_Push(dir, meterPerSecond);
        }


        //____________________________________________
        //                  전용 effect 처리
        //____________________________________________
        //public void SetActiveEffect(eEffectKind kind, bool value)
        //{
        //    if (null == _effect[(int)kind]) return;

        //    _effect[(int)kind].gameObject.SetActive(value);
        //}


        //____________________________________________
        //                  디버그 정보 출력
        //____________________________________________

        public bool _active_BEHAVIOR = false;
        void OnDrawGizmos()
        {
            if (true == _active_BEHAVIOR)
                DrawGizmos_Behavior();
        }

        Vector3 _debug_dir = Vector3.zero;
        Quaternion _debug_q = Quaternion.identity;
        Vector3 _debug_line = Vector3.zero;
        public void DrawGizmos_Behavior()
        {
            //BodyControl.Part HL = _bodyControl._parts[(int)BodyControl.Part.eKind.Hand_Left];
            //BodyControl.Part HR = _bodyControl._parts[(int)BodyControl.Part.eKind.Hand_Right];

            Vector3 posBody = this.GetPos3D();
            Quaternion quater_r = Quaternion.FromToRotation(UtilGS9.ConstV.v3_forward, _move._direction);
            //Vector3 posHL = quater_r * HL._pos_standard + posBody;
            //Vector3 posHR = quater_r * HR._pos_standard + posBody;

            float weaponArc_degree = 45f;
            float weaponArc_radius_far = 3f;
            Vector3 weaponArc_dir = _move._direction;
            //*
            //어깨 기준점 
            //Gizmos.color = Color.gray;
            //Gizmos.DrawWireSphere(posHL, HL._range_max);
            //Gizmos.DrawWireSphere(posHR, HR._range_max);

            ////skill 진행 상태 출력 
            //DebugWide.PrintText(posHR, Color.white, _bodyControl._skill_current._kind.ToString() + "  st: " + _bodyControl._state_current.ToString() + "   t: " + _bodyControl._timeDelta.ToString("00.00"));

            //공격 범위 - 호/수직 : Vector3.forward
            //eTraceShape tr = eTraceShape.None;
            //_data.GetBehavior().attack_shape

            if (0 != weaponArc_degree)
            {
                Gizmos.color = Color.yellow;
                _debug_q = Quaternion.AngleAxis(weaponArc_degree * 0.5f, Vector3.up);
                _debug_dir = _debug_q * weaponArc_dir;
                Gizmos.DrawLine(posBody, posBody + _debug_dir * weaponArc_radius_far);
                _debug_q = Quaternion.AngleAxis(weaponArc_degree * -0.5f, Vector3.up);
                _debug_dir = _debug_q * weaponArc_dir;
                Gizmos.DrawLine(posBody, posBody + _debug_dir * weaponArc_radius_far);
            }

            //공격 범위 - 호/수평 : Vector3.up

            //캐릭터카드 충돌원
            //Gizmos.color = Color.black;
            //Gizmos.DrawWireSphere(posSt, _data.GetCollider_Sphere().radius);

            //캐릭터 방향 
            Gizmos.color = Color.black;
            Gizmos.DrawLine(posBody, posBody + _move._direction * 4);
            //Gizmos.DrawSphere(posBody + _move._direction * 4, 0.2f);

            //공격 무기이동 경로
            //Vector3 weapon_curPos = posBody + _bodyControl.CurrentDistance() * weaponArc_dir;
            //_debug_line.y = -0.5f;
            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(posBody, weapon_curPos);
            //Gizmos.DrawWireSphere(weapon_curPos, 0.1f);
            //*/

            //칼죽이기 가능 범위
            //_debug_line.y = -1f;
            //Gizmos.color = Color.green;
            //Gizmos.DrawLine(_data.GetWeaponPosition(_data.GetBehavior().cloggedTime_0) + _debug_line, _data.GetWeaponPosition(_data.GetBehavior().cloggedTime_1) + _debug_line);

            //공격점 범위 
            //_debug_line.y = -1.5f;
            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(_data.GetWeaponPosition(_data.GetBehavior().eventTime_0) + _debug_line, _data.GetWeaponPosition(_data.GetBehavior().eventTime_1) + _debug_line);
        }
    }

}

