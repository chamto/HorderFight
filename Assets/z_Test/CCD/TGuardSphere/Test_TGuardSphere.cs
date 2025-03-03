﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;
using System;

public class Test_TGuardSphere : MonoBehaviour 
{

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

    public Transform _seg0 = null;
    public Transform _seg0_start = null;
    public Transform _seg0_end = null;

    public Transform _seg1 = null;
    public Transform _seg1_start = null;
    public Transform _seg1_end = null;

    private bool _init = false;

    private void OnDrawGizmos()
    {
        if (false == _init) return;

        float radius;
        radius = (_T0_root_start.position - _seg0_end.position).magnitude;
        DebugWide.DrawCircle(_T0_root_start.position, radius, Color.gray);

        DebugWide.DrawLine(_T0_root_start.position, _T0_sub_start.position, Color.gray);
        radius = Vector3.Magnitude(_T0_root_start.position - _T0_sub_start.position);
        DebugWide.DrawCircle(_T0_root_start.position, radius, Color.gray);

        radius = (_seg0_start.position - _seg0_end.position).magnitude;
        DebugWide.DrawCircle(_seg0_start.position, radius, Color.gray);
        DebugWide.DrawCircle(_seg0_start.position, 0.1f, Color.gray);
        DebugWide.DrawLine(_seg0_start.position, _seg0_end.position, Color.yellow); 
        DebugWide.DrawLine(_seg1_start.position, _seg1_end.position, Color.green); 


        //-----------------

        DebugWide.DrawLine(_T0_root_start.position, _T0_root_end.position, Color.magenta);
        DebugWide.DrawLine(_T0_sub_start.position, _T0_sub_end.position, Color.magenta);

        //-----------------

        DebugWide.DrawLine(_T1_root_start.position, _T1_root_end.position, Color.blue);
        DebugWide.DrawLine(_T1_sub_start.position, _T1_sub_end.position, Color.blue);

        //-----------------

        Vector3 n_seg1 = (_seg1_end.position - _seg1_start.position).normalized;
        Vector3 b_pos = __mt_1;
        //Vector3 b_pos = _seg1_start.position + n_seg1 * __b_plus;
        DebugWide.DrawLine(_seg1_start.position, b_pos, Color.red);
        DebugWide.DrawLine(_T0_root_start.position, b_pos, Color.red);
        DebugWide.DrawLine(_T0_root_start.position, _seg0_start.position, Color.red);

        //----------------



    }

	// Use this for initialization
	void Start () 
    {
        _init = true;
        Transform t0sub, t1sub;
        _T0 = Hierarchy.GetTransform(null, "T_0");	
        _T0_root = Hierarchy.GetTransform(_T0, "root");
        t0sub = Hierarchy.GetTransform(_T0_root, "sub_0");
        _T0_root_start = Hierarchy.GetTransform(_T0_root, "start");
        _T0_root_end = Hierarchy.GetTransform(_T0_root, "end");
        _T0_sub_start = Hierarchy.GetTransform(t0sub, "start");
        _T0_sub_end = Hierarchy.GetTransform(t0sub, "end");

        _T1 = Hierarchy.GetTransform(null, "T_1");
        _T1_root = Hierarchy.GetTransform(_T1, "root");
        t1sub = Hierarchy.GetTransform(_T1_root, "sub_0");
        _T1_root_start = Hierarchy.GetTransform(_T1_root, "start");
        _T1_root_end = Hierarchy.GetTransform(_T1_root, "end");
        _T1_sub_start = Hierarchy.GetTransform(t1sub, "start");
        _T1_sub_end = Hierarchy.GetTransform(t1sub, "end");

        //------------

        _seg0 = Hierarchy.GetTransform(null, "seg_0");
        _seg1 = Hierarchy.GetTransform(null, "seg_1");

        _seg0_start = Hierarchy.GetTransform(_seg0, "start");
        _seg0_end = Hierarchy.GetTransform(_seg0, "end");
        _seg1_start = Hierarchy.GetTransform(_seg1, "start");
        _seg1_end = Hierarchy.GetTransform(_seg1, "end");
	}


    //비율값에 따라 angle_1 을 변환한다 
    public float __rate = 1f;
    void Update()
    {
        Update_4(__rate, _T0_root_start.position, _seg0_start.position);
    }

    public void Update_4(float rateAtoB, Vector3 fixedOriginPt_a, Vector3 fixedOriginPt_b)
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
        LineSegment3.ClosestPoints(out pt_min, out pt_max, ls_sub, ls_seg0); //최소값이 t가드 위에 있음 
        //LineSegment3.ClosestPoints(out pt_min, out pt_max, ls_seg0, ls_sub); //잘못된방식 : 이렇게 하면 최소값이 선분0위에 있게 됨
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

        DebugWide.LogBlue("c : " + c + "  a : " + a + "   b_one : " + b_1 + "  new_angle : " + 0 + "  cosA :" + cosA + "  disc :" + disc + "  value_sign :" + value_sign);

    }

    public void Update_3(float rateAtoB, Vector3 fixedOriginPt_a, Vector3 fixedOriginPt_b)
    {
        _T0_root_start.position = fixedOriginPt_a;
        _seg0_start.position = fixedOriginPt_b;
        _T1.position = _T0.position;
        _seg1.position = _seg0.position;

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
        Vector3 up_AB_ptmin = Vector3.Cross(ls_AB.direction, dir_ptmin);

        float angle_cosA = UtilGS9.Geo.AngleSigned(ls_AB.direction, ls_seg1.direction, up_AB_seg1);
        //float angle_firstPt = UtilGS9.Geo.AngleSigned(ls_AB.direction, dir_ptmin , up_AB_ptmin);
        //float angle_all = UtilGS9.Geo.AngleSigned(dir_ptmin, ls_seg1.direction, up_AB_ptmin);


        //코사인 제2법칙 이용
        float dt2 = c * c - a * a; 

        //---------------------------------------
        //# 이동가능 최대각도 구함 
        float cosA_max = (float)Math.Sqrt(dt2 / (c * c));
        cosA_max = Mathf.Clamp(cosA_max, -1f, 1f);
        float angle_cosA_max = (float)Math.Acos(cosA_max);
        angle_cosA_max = angle_cosA_max * Mathf.Rad2Deg;

        //---------------------------------------
        //** 비율값이 적용된 각도를 구한다 **
        //if (Vector3.Dot(up_AB_ptmin, up_AB_seg1) < 0) angle_cosA_max *= -1f; //음수각도인지 찾는다 

        //float angle_diff = 0;
        //if(angle_cosA > angle_cosA_max && angle_cosA > 0)
        //{
        //    angle_diff = angle_cosA - angle_cosA_max;
        //} 
        //if(angle_cosA < angle_cosA_max && angle_cosA < 0)
        //{
        //    angle_diff = angle_cosA - angle_cosA_max;
        //}

        float angle_rate = 0f;
        //float angle_rate = angle_firstPt + (angle_all - angle_diff) * rateAtoB;
        //DebugWide.LogBlue("  fpt : " + angle_firstPt + " +( all: " + angle_all + "  " +  "  diff: " + angle_diff + " ) * " + __rate + "  = rate: "+ angle_rate +  "  cosA: " + angle_cosA + "  cosAMax: " + angle_cosA_max);
        //---------------------------------------

        //float cosA = (float)Math.Cos(angle_rate * Mathf.Deg2Rad);
        float cosA = (float)Math.Cos(angle_cosA * Mathf.Deg2Rad);
        float dt1 = -2f * c * cosA; //dt1 : -2cCosA


        //이차방정식의 근의공식 이용 , disc = 판별값 
        float disc = dt1 * dt1 - 4 * dt2;
        float b_1 = (-dt1 + value_sign * (float)Math.Sqrt(disc)) / 2f; //가까운점 


        //판별값이 0 보다 작다면 해가 없는 상태이다 
        //disc 를 0으로 설정해 이동가능 최대치를 구한다
        if (disc < 0 || b_1 < 0)
        {
            angle_cosA = angle_cosA_max; //최대각도로 다시 지정 
            //if (angle_cosA < 0) angle_cosA *= -1f;

            b_1 = c * cosA_max;
        }



        //if (angle_rate < 0) angle_rate *= -1f;
        //Vector3 new_dir_ls_seg1 = Quaternion.AngleAxis(angle_rate, up_AB_seg1) * ls_AB.direction; 
        Vector3 new_dir_ls_seg1 = Quaternion.AngleAxis(angle_cosA, up_AB_seg1) * ls_AB.direction; 


        __mt_0 = pt_min;
        __mt_1 = ls_seg1.origin + new_dir_ls_seg1.normalized * b_1;

        //=======================

        Vector3 up_t = Vector3.Cross(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position);
        float angle_t = Geo.AngleSigned(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position, up_t);

        _T1_root.rotation = Quaternion.AngleAxis(angle_t, up_t) * _T0_root.rotation;
        //_T0_root.rotation = Quaternion.AngleAxis(angle_t , up_t) * _T0_root.rotation;

        DebugWide.LogBlue("c : " + c + "  a : " + a + "   b_one : " + b_1 + "  new_angle : " + angle_rate + "  cosA :" + cosA + "  disc :" + disc + "  value_sign :" + value_sign );

    }


    //T가드의 최대이동 제약이 있는 알고리즘 
    void Update_1()
    {
        //1# root_start , seg_start 사이의 거리 
        LineSegment3 ls_AB = new LineSegment3(_seg0_start.position, _T0_root_start.position);
        float c = (_T0_root_start.position - _seg0_start.position).magnitude;

        //2# root_start , sub 선분의 접촉점 사이의 거리 
        Vector3 pt_min, pt_max;
        LineSegment3 ls_sub = new LineSegment3(_T0_sub_start.position, _T0_sub_end.position);
        LineSegment3 ls_seg0 = new LineSegment3(_seg0_start.position, _seg0_end.position);
        LineSegment3.ClosestPoints(out pt_min, out pt_max, ls_sub, ls_seg0);

        float a = (_T0_root_start.position - pt_min).magnitude;

        //3# seg_start , seg 선분의 접촉점 사이의 거리용
        //코사인 제2법칙 이용
        LineSegment3 ls_seg1 = new LineSegment3(_seg1_start.position, _seg1_end.position);
        Vector3 up_seg1_AB = Vector3.Cross(ls_AB.direction, ls_seg1.direction);
        float cosA = UtilGS9.Geo.AngleSigned(ls_AB.direction, ls_seg1.direction, up_seg1_AB);
        cosA = (float)Math.Cos(cosA * Mathf.Deg2Rad);

        float dt1 = -2f * c * cosA;
        //float dt2 = c * c - a * a;

        //이차방정식의 근의공식 이용
        float root = 0;
        float b_1 = (-dt1 + root) / 2f;

        //코사인 제2법칙으로 cosA를 다시 구함 
        cosA = (-(a * a) + (b_1 * b_1) + (c * c)) / (2 * b_1 * c);
        float new_angle = (float)Math.Acos(cosA);
        new_angle = new_angle * Mathf.Rad2Deg;
        Vector3 new_dir_ls_seg1 = Quaternion.AngleAxis(new_angle, up_seg1_AB) * ls_AB.direction;


        __mt_0 = pt_min;
        //__mt_1 = new_dir_ls_seg1; //길이가 b_1 이 아니기 때문에 다른 결과가 나온다 
        __mt_1 = ls_seg1.origin + new_dir_ls_seg1.normalized * b_1; 


        //=======================

        Vector3 dir_root = _T0_root_end.position - _T0_root_start.position;
        Vector3 up_t = Vector3.Cross(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position);
        float angle_t = Geo.AngleSigned(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position, up_t);

        _T1_root.rotation = Quaternion.AngleAxis(angle_t, up_t) * _T0_root.rotation;

        DebugWide.LogBlue("c : " + c + "  a : " + a + "   b_one : " + b_1 + "  new_angle : " + new_angle + "  cosA :" + cosA);

    }

    //T가드의 최대이동 제약이 없는 알고리즘 
	void Update_0 () 
    {
        //1# root_start , seg_start 사이의 거리 
        LineSegment3 ls_AB = new LineSegment3(_seg0_start.position, _T0_root_start.position);
        float c = (_T0_root_start.position - _seg0_start.position).magnitude;

        //2# root_start , sub 선분의 접촉점 사이의 거리 
        Vector3 pt_min, pt_max;
        LineSegment3 ls_sub = new LineSegment3(_T0_sub_start.position, _T0_sub_end.position);
        LineSegment3 ls_seg0 = new LineSegment3(_seg0_start.position, _seg0_end.position);
        LineSegment3.ClosestPoints(out pt_min, out pt_max,ls_sub , ls_seg0);

        float a = (_T0_root_start.position - pt_min).magnitude;

        //3# seg_start , seg 선분의 접촉점 사이의 거리용
        //코사인 제2법칙 이용
        LineSegment3 ls_seg1 = new LineSegment3(_seg1_start.position, _seg1_end.position);
        Vector3 up_seg1_AB = Vector3.Cross(ls_AB.direction, ls_seg1.direction);
        float cosA = UtilGS9.Geo.AngleSigned(ls_AB.direction, ls_seg1.direction, up_seg1_AB);
        cosA = (float)Math.Cos(cosA * Mathf.Deg2Rad);

        float dt1 = -2f * c * cosA;
        float dt2 = c * c - a * a;

        //이차방정식의 근의공식 이용
        float root = (float)Math.Sqrt(dt1 * dt1 - 4 * dt2);
        float b_plus = (-dt1 + root) / 2f;
        float b_minus = (-dt1 - root) / 2f;

        //__b_plus = b_plus;
        //__b_minus = b_minus;

        __mt_0 = pt_min;
        __mt_1 = ls_seg1.origin + ls_seg1.direction.normalized * b_minus; //임시 , 플러스인 경우도 있음 

        //=======================

        float disc = dt1 * dt1 - 4 * dt2; //판별값 : b*b-4ac

        Vector3 dir_root = _T0_root_end.position - _T0_root_start.position;
        Vector3 up_t = Vector3.Cross(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position);
        float angle_t = Geo.AngleSigned(__mt_0 - _T0_root_start.position, __mt_1 - _T0_root_start.position, up_t);
        //Vector3 new_dir_root = Quaternion.AngleAxis(angle_t,up_t) * dir_root;
        //_T1_root.rotation = _T0_root.rotation * Quaternion.AngleAxis(angle_t, up_t); //부정확한 값이 나옴  
        _T1_root.rotation = Quaternion.AngleAxis(angle_t, up_t) * _T0_root.rotation;  //정확한 값이 나옴 
        //부모회전행렬을 오른쪽에 놓는것이 고정된 식인지 "행/열우선 행렬"에 따라 곱하는 순서가 변화되어야 하는지 모르겠음. 다음에 분석해보기 - chamto 20200720
        //ref : https://m.blog.naver.com/PostView.nhn?blogId=sipack7297&logNo=220428447625&proxyReferer=https:%2F%2Fwww.google.com%2F


        DebugWide.LogBlue("c : " + c + "  a : " + a + "   b+ : " + b_plus + "  b- : " + b_minus + "  angle_t : " + angle_t + "  판별값 : " + disc);

	}
    //float __b_plus = 0, __b_minus = 0;
    Vector3 __mt_0, __mt_1;
}
