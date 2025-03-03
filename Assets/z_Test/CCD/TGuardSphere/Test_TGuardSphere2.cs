﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using System;


public class Test_TGuardSphere2 : MonoBehaviour 
{
    
    public class TGS_Info
    {
        public bool _init = false;

        public Transform _T0 = null;
        public Transform _T0_root = null;
        public Transform _T0_root_start = null;
        public Transform _T0_root_end = null;
        public Transform _T0_sub_start = null;
        public Transform _T0_sub_end = null;

        public Transform _T1 = null;
        public Transform _T1_root = null;
        public Transform _T1_root_start = null;
        public Transform _T1_root_end = null;
        public Transform _T1_sub_start = null;
        public Transform _T1_sub_end = null;

        public Transform _Tctl = null;
        public Transform _Tctl_root = null;
        public Transform _Tctl_root_start = null;
        public Transform _Tctl_root_end = null;
        public Transform _Tctl_sub_start = null;
        public Transform _Tctl_sub_end = null;

        public Vector3 _mt0 = ConstV.v3_zero;
        public Vector3 _mt1 = ConstV.v3_zero;

        public void Draw()
        {
            if (false == _init) return;

            float radius;

            DebugWide.DrawLine(_T0_root_start.position, _T0_sub_start.position, Color.gray);
            radius = Vector3.Magnitude(_T0_root_start.position - _T0_sub_start.position);
            //DebugWide.DrawCircle(_T0_root_start.position, radius, Color.gray);

            //-----------------

            DebugWide.DrawLine(_Tctl_root_start.position, _Tctl_root_end.position, Color.gray);
            DebugWide.DrawLine(_Tctl_sub_start.position, _Tctl_sub_end.position, Color.gray);
            DebugWide.DrawCircle(_Tctl_root_end.position, 0.05f, Color.gray);

            //-----------------

            DebugWide.DrawLine(_T0_root_start.position, _T0_root_end.position, Color.magenta);
            DebugWide.DrawLine(_T0_sub_start.position, _T0_sub_end.position, Color.magenta);

            //-----------------

            DebugWide.DrawLine(_T1_root_start.position, _T1_root_end.position, Color.blue);
            DebugWide.DrawLine(_T1_sub_start.position, _T1_sub_end.position, Color.blue);

            //-----------------

            DebugWide.DrawCircle(_mt0, 0.02f, Color.red);

            //DebugWide.DrawLine(_seg1_start.position, _mt1, Color.red);
            DebugWide.DrawLine(_T0_root_start.position, _mt1, Color.red);
            //DebugWide.DrawLine(_T0_root_start.position, _seg0_start.position, Color.red);

        }

        public void Init(Transform _TGS_root_)
        {
            _init = true;

            Transform temp;
            _T0 = Hierarchy.GetTransform(_TGS_root_, "T_0");
            _T0_root = Hierarchy.GetTransform(_T0, "root");
            temp = Hierarchy.GetTransform(_T0_root, "sub_0");
            _T0_root_start = Hierarchy.GetTransform(_T0_root, "start");
            _T0_root_end = Hierarchy.GetTransform(_T0_root, "end");
            _T0_sub_start = Hierarchy.GetTransform(temp, "start");
            _T0_sub_end = Hierarchy.GetTransform(temp, "end");

            _T1 = Hierarchy.GetTransform(_TGS_root_, "T_1");
            _T1_root = Hierarchy.GetTransform(_T1, "root");
            temp = Hierarchy.GetTransform(_T1_root, "sub_0");
            _T1_root_start = Hierarchy.GetTransform(_T1_root, "start");
            _T1_root_end = Hierarchy.GetTransform(_T1_root, "end");
            _T1_sub_start = Hierarchy.GetTransform(temp, "start");
            _T1_sub_end = Hierarchy.GetTransform(temp, "end");

            _Tctl = Hierarchy.GetTransform(_TGS_root_, "Tctl");
            _Tctl_root = Hierarchy.GetTransform(_Tctl, "root");
            temp = Hierarchy.GetTransform(_Tctl_root, "sub_0");
            _Tctl_root_start = Hierarchy.GetTransform(_Tctl_root, "start");
            _Tctl_root_end = Hierarchy.GetTransform(_Tctl_root, "end");
            _Tctl_sub_start = Hierarchy.GetTransform(temp, "start");
            _Tctl_sub_end = Hierarchy.GetTransform(temp, "end");

            //------------

        }


    }//end class

    TGS_Info _tgs0 = new TGS_Info();
    TGS_Info _tgs1 = new TGS_Info();


	// Use this for initialization
	void Start () 
    {
        Transform tgs_root = null;
        tgs_root = Hierarchy.GetTransform(null, "TGS_0");
        _tgs0.Init(tgs_root);
        tgs_root = Hierarchy.GetTransform(null, "TGS_1");
        _tgs1.Init(tgs_root);
	}


    //비율값에 따라 angle_1 을 변환한다 
    public float __rate = 1f;
    void Update()
    {
        
        //Calc_TGSvsTGS(__rate, _tgs0, _tgs1);
        //Test_0(_tgs0, _tgs1);

    }

    private void OnDrawGizmos()
    {
        
        if (true == _tgs0._init)
        {
            //Test_0(_tgs0, _tgs1);
            Calc_TGSvsTGS(__rate, _tgs0, _tgs1);

            _tgs0.Draw();
            _tgs1.Draw();    
        }

    }

    //테스트를 위한 최소한의 알고리즘 
    void Test_0(TGS_Info tgs0, TGS_Info tgs1)
    {

        tgs0._T1_root.position = tgs0._T0_root.position;
        tgs1._T1_root.position = tgs1._T0_root.position;
        tgs0._T1_root.rotation = tgs0._T0_root.rotation;
        tgs1._T1_root.rotation = tgs1._T0_root.rotation;
        tgs0._mt1 = tgs0._T0_root.position;
        tgs1._mt1 = tgs1._T0_root.position;

        Vector3 pt_min, pt_max;
        LineSegment3 ls_sub = new LineSegment3(tgs0._T0_sub_start.position, tgs0._T0_sub_end.position);

        //T0_sub 선분과 Tctl_sub 선분의 시작점이 "t가드 vs 선분" 알고리즘에서는 서로만나는 점에 착안하여,
        //"t가드 vs t가드" 알고리즘에서 서로만나지 않는 t가드sub의 시작점을 만나게 수정하였음 
        //이때 두가지 방식으로 시작점을 만나게 할 수 있다.
        //Tctl_sub 선분이 온전한 row 방식이 각도계산시 정상적으로 나오므로 row를 사용함 

        //row
        LineSegment3 ls_AB = new LineSegment3(tgs1._Tctl_sub_start.position, tgs0._T0_root_start.position); //row 
        LineSegment3 ls_seg0 = new LineSegment3(tgs1._Tctl_sub_start.position, tgs1._T0_sub_end.position); //row 
        LineSegment3 ls_seg1 = new LineSegment3(tgs1._Tctl_sub_start.position, tgs1._Tctl_sub_end.position); //row

        //high
        //LineSegment3 ls_AB = new LineSegment3(tgs1._T0_sub_start.position, tgs0._T0_root_start.position); //high
        //LineSegment3 ls_seg0 = new LineSegment3(tgs1._T0_sub_start.position, tgs1._T0_sub_end.position); //high
        //LineSegment3 ls_seg1 = new LineSegment3(tgs1._T0_sub_start.position, tgs1._Tctl_sub_end.position); //high

        //------
        DebugWide.DrawLine(ls_seg0.origin, ls_seg0.last, Color.cyan); //row
        DebugWide.DrawLine(ls_seg1.origin, ls_seg1.last, Color.cyan); //high
        DebugWide.DrawLine(ls_AB.origin, ls_AB.last, Color.red);
        //------

        LineSegment3.ClosestPoints(out pt_min, out pt_max, ls_sub, ls_seg0);
        Vector3 dir_rootS_min = pt_min - tgs0._T0_root_start.position;

        float c = ls_AB.Length(); //1# root_start , seg_start 사이의 거리 
        float a = (dir_rootS_min).magnitude; //2# root_start , sub 선분의 접촉점 사이의 거리 

        //3# seg_start , seg 선분의 접촉점 사이의 거리용
        //코사인 제2법칙 이용
        Vector3 up_seg1_AB = Vector3.Cross(ls_AB.direction, ls_seg1.direction);
        float angle_cosA = UtilGS9.Geo.AngleSigned(ls_AB.direction, ls_seg1.direction, up_seg1_AB);
        float cosA = (float)Math.Cos(angle_cosA * Mathf.Deg2Rad);


        float dt1 = -2f * c * cosA;
        float dt2 = c * c - a * a;
        DebugWide.LogBlue(dt1 + "  " + dt2);
        //이차방정식의 근의공식 이용
        float disc = dt1 * dt1 - 4 * dt2; //판별값 : b*b-4ac
        float root = (float)Math.Sqrt(disc);
        float b_plus = (-dt1 + root) / 2f;
        float b_minus = (-dt1 - root) / 2f;


        tgs0._mt0 = pt_min;
        tgs0._mt1 = ls_seg1.origin + ls_seg1.direction.normalized * b_minus; //임시 , 플러스인 경우도 있음 

        //=======================

        Vector3 up_t = Vector3.Cross(tgs0._mt0 - tgs0._T0_root_start.position, tgs0._mt1 - tgs0._T0_root_start.position);
        float angle_t = Geo.AngleSigned(tgs0._mt0 - tgs0._T0_root_start.position, tgs0._mt1 - tgs0._T0_root_start.position, up_t);

        tgs0._T1_root.rotation = Quaternion.AngleAxis(angle_t, up_t) * tgs0._T0_root.rotation;


        DebugWide.LogBlue("c : " + c + "  a : " + a + "   b+ : " + b_plus + "  b- : " + b_minus + "  angle : " + angle_cosA + "  판별값 : " + disc);

    }


    //==========


    Vector3 __prev_ctl_root_pos = ConstV.v3_zero;
    Quaternion __prev_ctl_root_rot = Quaternion.identity;
    //tgs1 의 지나간 궤적에 따라 tgs0 의 움직임 계산 
    public void Calc_TGSvsTGS(float rateAtoB, TGS_Info tgs0 , TGS_Info tgs1)
    {
        
        tgs0._T1_root.position = tgs0._T0_root.position;
        tgs0._T1_root.rotation = tgs0._T0_root.rotation;
        tgs0._Tctl_root.position = tgs0._T0_root.position;
        tgs0._Tctl_root.rotation = tgs0._T0_root.rotation;

        tgs1._T1_root.position = tgs1._T0_root.position;
        tgs1._T1_root.rotation = tgs1._T0_root.rotation;

        tgs0._mt1 = tgs0._T0_root.position;
        tgs1._mt1 = tgs1._T0_root.position;

        rateAtoB = Mathf.Clamp(rateAtoB, 0, 1f);

        //------

        Vector3 pt_min, pt_max;
        LineSegment3 ls_tgs0_t0_sub = new LineSegment3(tgs0._T0_sub_start.position, tgs0._T0_sub_end.position);
        LineSegment3 ls_tgs1_t0_sub = new LineSegment3(tgs1._T0_sub_start.position, tgs1._T0_sub_end.position);
        LineSegment3 ls_AB = new LineSegment3(tgs1._Tctl_sub_start.position, tgs0._T0_root_start.position); //row 
        LineSegment3 ls_seg0 = new LineSegment3(tgs1._Tctl_sub_start.position, tgs1._T0_sub_end.position); //row 
        LineSegment3 ls_seg1 = new LineSegment3(tgs1._Tctl_sub_start.position, tgs1._Tctl_sub_end.position); //row
        float lensqr_tgs1_t0_sub = ls_tgs1_t0_sub.LengthSquared();

        //DebugWide.DrawLine(ls_seg0.origin, ls_seg0.last, Color.cyan); //row
        //DebugWide.DrawLine(ls_AB.origin, ls_AB.last, Color.red);


        LineSegment3.ClosestPoints(out pt_min, out pt_max, ls_tgs0_t0_sub, ls_seg0);
        Vector3 dir_rootS_min = pt_min - tgs0._T0_root_start.position;

        float c = ls_AB.Length(); //1# root_start , seg_start 사이의 거리 
        float a = (dir_rootS_min).magnitude; //2# root_start , sub 선분의 접촉점 사이의 거리 

        //근의공식 부호 결정 
        float value_sign = Vector3.Dot(dir_rootS_min, -ls_AB.direction);
        if (value_sign > 0) value_sign = -1; else value_sign = 1;

        //3# seg_start , seg 선분의 접촉점 사이의 거리용
        Vector3 dir_ptmin_1 = pt_min - tgs1._T0_sub_start.position;
        Vector3 dir_ptmin_2 = pt_min - tgs1._Tctl_sub_start.position;


        //---------------------------------------
        //선분1 이 최소선분 위에 있으면 최소선분으로 고정함   
        Vector3 t1 = Vector3.Cross(ls_tgs1_t0_sub.direction, dir_ptmin_1);
        Vector3 t2 = Vector3.Cross(dir_ptmin_2, ls_seg1.direction);
        if (Vector3.Dot(t1, t2) < 0)
        {
            rateAtoB = 0;
            DebugWide.LogYellow("최소선분으로 고정 !! ");
        }

        //---------------------------------------
        //코사인 제2법칙 이용
        float dt2 = c * c - a * a;

        //---------------------------------------
        //비율값으로 새로운 선분구함 
        //Vector3 len_ptmin_ = pt_min - tgs1._T0_sub_start.position;
        Vector3 pt_seg1 = ls_seg1.origin + ls_seg1.direction.normalized * dir_ptmin_2.magnitude;
        Vector3 pt_rate = pt_min + (pt_seg1 - pt_min) * rateAtoB;

        //-----------------
        Vector3 dir_seg_rate = pt_rate - ls_seg0.origin;
        Vector3 up_seg_rate = Vector3.Cross(ls_AB.direction, dir_seg_rate);
        float angle_seg_rate = UtilGS9.Geo.AngleSigned(ls_AB.direction, dir_seg_rate, up_seg_rate);

        //---------------------------------------
        //float cosA = (float)Math.Cos(angle_cosA * Mathf.Deg2Rad);
        float cosA = (float)Math.Cos(angle_seg_rate * Mathf.Deg2Rad);
        float dt1 = -2f * c * cosA; //dt1 : -2cCosA

        //이차방정식의 근의공식 이용 , disc = 판별값 
        float disc = dt1 * dt1 - 4 * dt2;
        float b_1 = (-dt1 + value_sign * (float)Math.Sqrt(disc)) / 2f; //가까운점 
        //DebugWide.LogBlue(disc + "  " + b_1);
        float angle_apply = angle_seg_rate;

        //---
        //비율적용된 각도값이 아닌 원래 각도값의 판별값을 이용한다.
        //비율적용된 각도값의 판별값을 사용하면 비율값에 따라 최대범위가 늘어진다
        Vector3 up_AB_seg1 = Vector3.Cross(ls_AB.direction, ls_seg1.direction);
        float angle_cosA = UtilGS9.Geo.AngleSigned(ls_AB.direction, ls_seg1.direction, up_AB_seg1);
        cosA = (float)Math.Cos(angle_cosA * Mathf.Deg2Rad);
        dt1 = -2f * c * cosA;
        float disc_ori = dt1 * dt1 - 4 * dt2;
        //판별값이 0 보다 작다면 해가 없는 상태이다  
        //disc 가 양수면서 b_1이 음수인 경우가 있다. 판별값이 양수라도 길이가 음수가 나올 수 있다 
        //tgs1_t0_sub 를 넘는 b 의 길이는 처리 하면 안됨 
        if (disc_ori < 0 || b_1 < 0 || disc < 0 || (b_1*b_1) > lensqr_tgs1_t0_sub)
        {
            //임시처리 - tctl의 값이 계산 할 수 없는 영역에 있는 경우 처리를 못해준다 
            tgs1._Tctl_root.position = __prev_ctl_root_pos;
            tgs1._Tctl_root.rotation = __prev_ctl_root_rot;
            DebugWide.LogYellow("처리 할 수 없는 상태 ==================");
            return; //길이가 음수면 처리 할 수 없는 상태임

            //---------------------------------------
            //# 이동가능 최대값 구함 
            float cosA_max_sqr = dt2 / (c * c);
            float cosA_max = (float)Math.Sqrt(cosA_max_sqr);
            cosA_max = Mathf.Clamp(cosA_max, -1f, 1f);
            float angle_cosA_max = (float)Math.Acos(cosA_max);
            angle_cosA_max = angle_cosA_max * Mathf.Rad2Deg;
            float b_max = c * cosA_max; //최대각도에서의 b길이 

            DebugWide.LogBlue(disc_ori + "  " + angle_cosA_max + "   " + b_max + "   " + cosA_max + "  " + cosA_max_sqr + "  " + b_1);

            //dir_seg_max 를 구하는데 up_AB_seg1 이 값을 사용하여 근사값 사용 
            Vector3 dir_seg_max = Quaternion.AngleAxis(angle_cosA_max, up_AB_seg1) * ls_AB.direction;
            pt_max = ls_seg1.origin + dir_seg_max.normalized * b_max;

            pt_rate = pt_min + (pt_max - pt_min) * rateAtoB;
            dir_seg_rate = pt_rate - ls_seg1.origin;
            up_seg_rate = Vector3.Cross(ls_AB.direction, dir_seg_rate);
            angle_seg_rate = UtilGS9.Geo.AngleSigned(ls_AB.direction, dir_seg_rate, up_seg_rate);
            cosA = (float)Math.Cos(angle_seg_rate * Mathf.Deg2Rad);
            dt1 = -2f * c * cosA; //dt1 : -2cCosA
            //이차방정식의 근의공식 이용 , disc = 판별값 
            disc = dt1 * dt1 - 4 * dt2;
            if (disc < 0) disc = 0;
            b_1 = (-dt1 + value_sign * (float)Math.Sqrt(disc)) / 2f; //가까운점 

            angle_apply = angle_seg_rate;
            //---------------------------------------


        }


        Vector3 new_dir_ls_seg1 = Quaternion.AngleAxis(angle_apply, up_seg_rate) * ls_AB.direction;


        tgs0._mt0 = pt_min;
        tgs0._mt1 = ls_seg1.origin + new_dir_ls_seg1.normalized * b_1;
        DebugWide.DrawCircle(tgs0._mt1, 0.05f, Color.red);
        //=======================

        Vector3 up_t = Vector3.Cross(tgs0._mt0 - tgs0._T0_root_start.position, tgs0._mt1 - tgs0._T0_root_start.position);
        float angle_t = Geo.AngleSigned(tgs0._mt0 - tgs0._T0_root_start.position, tgs0._mt1 - tgs0._T0_root_start.position, up_t);

        tgs0._T1_root.rotation = Quaternion.AngleAxis(angle_t, up_t) * tgs0._T0_root.rotation;

        DebugWide.LogBlue("c : " + c + "  a : " + a + "   b_one : " + b_1 + "  new_angle : " + angle_apply + "  cosA :" + cosA + "  disc :" + disc + "  value_sign :" + value_sign);

        __prev_ctl_root_pos = tgs1._Tctl_root.position;
        __prev_ctl_root_rot = tgs1._Tctl_root.rotation;

    }


    /*
    private Vector3 __mt_0, __mt_1;
    public void Update(float rateAtoB, Vector3 fixedOriginPt_a, Vector3 fixedOriginPt_b)
    {
        _T0_root_start.position = fixedOriginPt_a;
        _seg0_start.position = fixedOriginPt_b;
        _T1.position = _T0.position;
        _seg1.position = _seg0.position;
        rateAtoB = Mathf.Clamp(rateAtoB, 0, 1f);

        //1# root_start , seg_start 사이의 거리 
        LineSegment3 ls_AB = new LineSegment3(_seg0_start.position, _T0_root_start.position);
        float c = (_T0_root_start.position - _seg0_start.position).magnitude;

        //2# root_start , sub 선분의 접촉점 사이의 거리 
        Vector3 pt_min, pt_max;
        LineSegment3 ls_sub = new LineSegment3(_T0_sub_start.position, _T0_sub_end.position);
        LineSegment3 ls_seg0 = new LineSegment3(_seg0_start.position, _seg0_end.position);
        LineSegment3.ClosestPoints(out pt_min, out pt_max, ls_sub, ls_seg0);
        Vector3 dir_rootS_min = pt_min - _T0_root_start.position;
        float a = (dir_rootS_min).magnitude;

        //근의공식 부호 결정 
        float value_sign = Vector3.Dot(dir_rootS_min, -ls_AB.direction);
        if (value_sign > 0) value_sign = -1; else value_sign = 1;

        //3# seg_start , seg 선분의 접촉점 사이의 거리용
        Vector3 dir_ptmin = pt_min - _seg0_start.position;
        LineSegment3 ls_seg1 = new LineSegment3(_seg1_start.position, _seg1_end.position);
        Vector3 up_AB_seg1 = Vector3.Cross(ls_AB.direction, ls_seg1.direction);
        //Vector3 up_AB_ptmin = Vector3.Cross(ls_AB.direction, dir_ptmin);

        //---------------------------------------
        //선분1 이 최소선분 위에 있으면 최소선분으로 고정함   
        Vector3 t1 = Vector3.Cross(ls_seg0.direction, dir_ptmin);
        Vector3 t2 = Vector3.Cross(dir_ptmin, ls_seg1.direction);
        if (Vector3.Dot(t1, t2) < 0)
        {
            rateAtoB = 0;
        }

        //---------------------------------------
        //코사인 제2법칙 이용
        float dt2 = c * c - a * a;

        //---------------------------------------
        //비율값으로 새로운 선분구함 
        Vector3 pt_seg1 = ls_seg1.origin + ls_seg1.direction.normalized * dir_ptmin.magnitude;
        Vector3 pt_rate = pt_min + (pt_seg1 - pt_min) * rateAtoB;

        //-----------------
        Vector3 dir_seg_rate = pt_rate - ls_seg0.origin;
        Vector3 up_seg_rate = Vector3.Cross(ls_AB.direction, dir_seg_rate);
        float angle_seg_rate = UtilGS9.Geo.AngleSigned(ls_AB.direction, dir_seg_rate, up_seg_rate);

        //---------------------------------------
        //float cosA = (float)Math.Cos(angle_cosA * Mathf.Deg2Rad);
        float cosA = (float)Math.Cos(angle_seg_rate * Mathf.Deg2Rad);
        float dt1 = -2f * c * cosA; //dt1 : -2cCosA

        //이차방정식의 근의공식 이용 , disc = 판별값 
        float disc = dt1 * dt1 - 4 * dt2;
        float b_1 = (-dt1 + value_sign * (float)Math.Sqrt(disc)) / 2f; //가까운점 

        float angle_apply = angle_seg_rate;

        //---
        //비율적용된 각도값이 아닌 원래 각도값의 판별값을 이용한다.
        //비율적용된 각도값의 판별값을 사용하면 비율값에 따라 최대범위가 늘어진다
        float angle_cosA = UtilGS9.Geo.AngleSigned(ls_AB.direction, ls_seg1.direction, up_AB_seg1);
        cosA = (float)Math.Cos(angle_cosA * Mathf.Deg2Rad);
        dt1 = -2f * c * cosA;
        float disc2 = dt1 * dt1 - 4 * dt2;
        //판별값이 0 보다 작다면 해가 없는 상태이다 
        //if (disc < 0 || b_1 < 0)
        if (disc2 < 0 || b_1 < 0)
        {

            //---------------------------------------
            //# 이동가능 최대값 구함 
            float cosA_max = (float)Math.Sqrt(dt2 / (c * c));
            cosA_max = Mathf.Clamp(cosA_max, -1f, 1f);
            float angle_cosA_max = (float)Math.Acos(cosA_max);
            angle_cosA_max = angle_cosA_max * Mathf.Rad2Deg;
            float b_max = c * cosA_max; //최대각도에서의 b길이 

            //dir_seg_max 를 구하는데 up_AB_seg1 이 값을 사용하여 근사값 사용 
            Vector3 dir_seg_max = Quaternion.AngleAxis(angle_cosA_max, up_AB_seg1) * ls_AB.direction;
            pt_max = ls_seg1.origin + dir_seg_max.normalized * b_max;

            pt_rate = pt_min + (pt_max - pt_min) * rateAtoB;
            dir_seg_rate = pt_rate - ls_seg0.origin;
            up_seg_rate = Vector3.Cross(ls_AB.direction, dir_seg_rate);
            angle_seg_rate = UtilGS9.Geo.AngleSigned(ls_AB.direction, dir_seg_rate, up_seg_rate);
            cosA = (float)Math.Cos(angle_seg_rate * Mathf.Deg2Rad);
            dt1 = -2f * c * cosA; //dt1 : -2cCosA
            //이차방정식의 근의공식 이용 , disc = 판별값 
            disc = dt1 * dt1 - 4 * dt2;
            if (disc < 0) disc = 0;
            b_1 = (-dt1 + value_sign * (float)Math.Sqrt(disc)) / 2f; //가까운점 

            angle_apply = angle_seg_rate;
            //---------------------------------------

            //최대각도로 다시 지정 
            //angle_apply = angle_cosA_max; 
            //b_1 = b_max;

        }


        //Vector3 new_dir_ls_seg1 = Quaternion.AngleAxis(angle_apply, up_AB_seg1) * ls_AB.direction;
        Vector3 new_dir_ls_seg1 = Quaternion.AngleAxis(angle_apply, up_seg_rate) * ls_AB.direction;


        __mt_0 = pt_min;
        __mt_1 = ls_seg1.origin + new_dir_ls_seg1.normalized * b_1;

        //=======================

        Vector3 up_t = Vector3.Cross(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position);
        float angle_t = Geo.AngleSigned(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position, up_t);

        _T1_root.rotation = Quaternion.AngleAxis(angle_t, up_t) * _T0_root.rotation;

        //DebugWide.LogBlue("c : " + c + "  a : " + a + "   b_one : " + b_1 + "  new_angle : " + 0 + "  cosA :" + cosA + "  disc :" + disc + "  value_sign :" + value_sign);

    }
    //*/

}
