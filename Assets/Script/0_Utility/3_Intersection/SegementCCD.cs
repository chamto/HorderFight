﻿using UnityEngine;
using UtilGS9;
using System;


namespace UtilGS9
{
    public class SegementCCD
    {
        public LineSegment3 _prev_seg_A;
        public LineSegment3 _prev_seg_B;
        public LineSegment3 _cur_seg_A;
        public LineSegment3 _cur_seg_B;

        //삼각형 0,1 합친모양의 사각형
        public Tetragon3 _tetr01;
        public Tetragon3 _tetr23;

        private IntrTriangle3Triangle3 _intr_0_2;
        private IntrTriangle3Triangle3 _intr_0_3;
        private IntrTriangle3Triangle3 _intr_1_2;
        private IntrTriangle3Triangle3 _intr_1_3;
        public Vector3 _close_B, _close_A;
        public Vector3 _meetPt;
        public Vector3 _meetPt_A, _meetPt_B;
        public float _radius_A = 0f;
        public float _radius_B = 0f;


        public Vector3 _dir_move_A = ConstV.v3_zero;
        public Vector3 _dir_move_B = ConstV.v3_zero;
        public Quaternion _localRota_A = Quaternion.identity;
        public Quaternion _localRota_B = Quaternion.identity;


        public SegementCCD()
        {
            _tetr01 = new Tetragon3();
            _tetr23 = new Tetragon3();
            _tetr01.tri0 = Triangle3.Zero();
            _tetr01.tri1 = Triangle3.Zero();
            _tetr23.tri0 = Triangle3.Zero();
            _tetr23.tri1 = Triangle3.Zero();
            _intr_0_2 = new IntrTriangle3Triangle3(_tetr01.tri0, _tetr23.tri0);
            _intr_0_3 = new IntrTriangle3Triangle3(_tetr01.tri0, _tetr23.tri1);
            _intr_1_2 = new IntrTriangle3Triangle3(_tetr01.tri1, _tetr23.tri0);
            _intr_1_3 = new IntrTriangle3Triangle3(_tetr01.tri1, _tetr23.tri1);
        }

        //CalcSegment_PushPoint 함수 호출전 초기값 prev 와 cur 을 같게 만들어 줘야 한다  
        //public void InitSegAB(LineSegment3 segA, LineSegment3 segB)
        //{
        //    _cur_seg_A = segA;
        //    _prev_seg_A = segA;

        //    _cur_seg_B = segB;
        //    _prev_seg_B = segB;
        //}


        public void Draw()
        {
            //if (__result_meet)
            //{
            //    //min, max
            //    DebugWide.DrawCircle(_minV, 0.02f, Color.white);
            //    DebugWide.DrawCircle(_maxV, 0.04f, Color.white);

            //    //meetPt
            //    //DebugWide.DrawCircle(_meetPt, 0.5f, Color.red); //chamto test
            //}

            if (__isSeg_A && __isSeg_B)
            {   //선분과 선분

                _prev_seg_A.Draw(Color.blue);
                _prev_seg_B.Draw(Color.magenta);
                //if (true == __intr_A_B_inside)
                //{
                //    DebugWide.DrawCircle(__cpPt0, 0.05f, Color.red);
                //    //DebugWide.DrawCircle(__cpPt1, 0.05f, Color.red);
                //    //DebugWide.DrawLine(__cpPt0, __cpPt1, Color.red);    
                //}


            }
            else
            {   //삼각형과 삼각형 

                _tetr01.Draw(Color.blue);
                _tetr23.Draw(Color.magenta);
                _intr_0_2.Draw(Color.red);
                _intr_0_3.Draw(Color.red);
                _intr_1_2.Draw(Color.red);
                _intr_1_3.Draw(Color.red);

            }

            Vector3 dirN = VOp.PerpN(_cur_seg_A.direction, Vector3.up);
            DebugWide.DrawLine(_cur_seg_A.origin + dirN * _radius_A, _cur_seg_A.last + dirN * _radius_A, Color.gray);
            DebugWide.DrawLine(_cur_seg_A.origin - dirN * _radius_A, _cur_seg_A.last - dirN * _radius_A, Color.gray);

            dirN = VOp.PerpN(_cur_seg_B.direction, Vector3.up);
            DebugWide.DrawLine(_cur_seg_B.origin + dirN * _radius_B, _cur_seg_B.last + dirN * _radius_B, Color.gray);
            DebugWide.DrawLine(_cur_seg_B.origin - dirN * _radius_B, _cur_seg_B.last - dirN * _radius_B, Color.gray);
        }

        //ctp : 접촉점 
        //stand : 정렬하기 위한 기준점 
        public void SortMinMax(Vector3 ctP, Vector3 stand,
                                         ref Vector3 minV, ref Vector3 maxV, ref float minD, ref float maxD)
        {
            float a = (stand - ctP).sqrMagnitude;
            if (a <= minD)
            {
                minV = ctP;
                minD = a;
            }
            if (a > maxD)
            {
                maxV = ctP;
                maxD = a;
            }
        }

        //!!! count 2 일때, 즉 선분검사시 제대로 정렬을 못해줌. 대체함수 사용하기 
        //count : 삼각형 vs 삼각형의 찾은 접촉점(최대6개)에 대하여 검색할 최대 개수 지정 , 삼각형 2개가 합쳐진 사각형이 대상이 아님  
        // 예) 찾은 접촉점이 점과 선분에 해당한다면 개수를 2로 지정 , 3이상은 평면
        public bool GetMinMax_ContactPt(Vector3 comparisonSt, out Vector3 minV, out Vector3 maxV, int count)
        {

            //_intr_0_2.Draw(Color.black);
            //_intr_0_3.Draw(Color.black);
            //_intr_1_2.Draw(Color.black);
            //_intr_1_3.Draw(Color.black);

            bool result = false;
            minV = ConstV.v3_zero;
            maxV = ConstV.v3_zero;
            float minD = 1000000, maxD = 0;
            //int count = 2;
            for (int i = 0; i < count; i++)
            {
                //DebugWide.DrawLine(comparisonSt, _intr_0_2.mPoint[i], Color.green);
                //DebugWide.DrawLine(comparisonSt, _intr_0_3.mPoint[i], Color.green);
                //DebugWide.DrawLine(comparisonSt, _intr_1_2.mPoint[i], Color.green);
                //DebugWide.DrawLine(comparisonSt, _intr_1_3.mPoint[i], Color.green);
                //DebugWide.PrintText(_intr_0_2.mPoint[i], Color.green , (comparisonSt- _intr_0_2.mPoint[i]).magnitude + "");
                //DebugWide.PrintText(_intr_0_3.mPoint[i], Color.green , (comparisonSt - _intr_0_3.mPoint[i]).magnitude + "");
                //DebugWide.PrintText(_intr_1_2.mPoint[i], Color.green , (comparisonSt - _intr_1_2.mPoint[i]).magnitude + "");
                //DebugWide.PrintText(_intr_1_3.mPoint[i], Color.green , (comparisonSt - _intr_1_3.mPoint[i]).magnitude + "");


                //교점이 있을때 
                if (i < _intr_0_2.mQuantity)
                {
                    SortMinMax(_intr_0_2.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i02:"+_intr_0_2.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
                if (i < _intr_0_3.mQuantity)
                {
                    SortMinMax(_intr_0_3.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i03:" + _intr_0_3.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
                if (i < _intr_1_2.mQuantity)
                {
                    SortMinMax(_intr_1_2.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i12:" + _intr_1_2.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
                if (i < _intr_1_3.mQuantity)
                {
                    SortMinMax(_intr_1_3.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i13:" + _intr_1_3.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
            }

            //DebugWide.DrawCircle(minV, 0.03f, Color.red);
            //DebugWide.DrawCircle(maxV, 0.07f, Color.red);

            //_intr_0_2.Draw(Color.black);
            //_intr_0_3.Draw(Color.black);
            //_intr_1_2.Draw(Color.black);
            //_intr_1_3.Draw(Color.black);

            return result;
        }

        //선분과 접촉점의 최소값 첫번쨰와 두번째값을 찾는다 
        public void SortMin0Min1(Vector3 ctp, LineSegment3 line, 
                                         ref Vector3 min0, ref Vector3 min1, ref float len0, ref float len1)
        {

            Vector3 pt = Line3.ClosestPoint(line.origin, line.direction, ctp);
            float sqrDis = (pt - ctp).sqrMagnitude;
            if (len0 > sqrDis)
            {
                //min1로 값을 옮긴다 
                if (len1 > len0)
                {
                    len1 = len0;
                    min1 = min0;
                }

                len0 = sqrDis;
                min0 = ctp;
            }
            else
            {
                if (len1 > sqrDis)
                {
                    len1 = sqrDis;
                    min1 = ctp;
                }
            }
        }

        //의도한데로 작동하지 않음 - 사용하지 말것 , 내부 개별알고리즘음은 맞음 
        //충돌점중 선분과 가까운 점을 찾는다 
        public bool GetMinMax_Tetragon(LineSegment3 line, out Vector3 min, out Vector3 max)
        {
            bool contact = false;
            Vector3[] arr = new Vector3[8];
            float[] lens = new float[8];
            for (int i = 0; i < 8; i++) lens[i] = 2000000; //SortMin0Min1 의 정렬을 하기 위해 최대값으로 초기화가 되어야함 

            min = ConstV.v3_zero;
            max = ConstV.v3_zero;


            const int COUNT = 6; //최대 교점 검사
            for (int i = 0; i < COUNT; i++)
            {
                //교점이 있을때 
                if (i < _intr_0_2.mQuantity)
                {
                    SortMin0Min1(_intr_0_2.mPoint[i], line, ref arr[0], ref arr[1], ref lens[0], ref lens[1]);
                    contact = true;
                }
                if (i < _intr_0_3.mQuantity)
                {
                    SortMin0Min1(_intr_0_3.mPoint[i], line, ref arr[2], ref arr[3], ref lens[2], ref lens[3]);
                    contact = true;
                }
                if (i < _intr_1_2.mQuantity)
                {
                    SortMin0Min1(_intr_1_2.mPoint[i], line, ref arr[4], ref arr[5], ref lens[4], ref lens[5]);
                    contact = true;
                }
                if (i < _intr_1_3.mQuantity)
                {
                    SortMin0Min1(_intr_1_3.mPoint[i], line, ref arr[6], ref arr[7], ref lens[6], ref lens[7]);
                    contact = true;
                }
            }

            if (false == contact) return false;

            //선택정렬을 한다 
            Vector3 vTemp;
            float fTemp;
            for (int a = 0; a < 8; a++)
            {
                for (int i = a+1; i < 8; i++)
                {
                    if (lens[a] > lens[i])
                    {
                        vTemp = arr[a];
                        arr[a] = arr[i];
                        arr[i] = vTemp;

                        fTemp = lens[a];
                        lens[a] = lens[i];
                        lens[i] = fTemp;
                    }
                }
            }


            //선분위에서 최소값과 최대값을 찾는다 
            float sqrDis, minDis = 1000000, maxDis = 0;
            for (int i = 0; i < 8; i++)
            {
                DebugWide.DrawCircle(arr[i], 0.008f + 0.005f * i, Color.green);
                DebugWide.LogBlue(i + "  " + lens[i]);
                if (lens[i] > 1000000) continue; //교점없는 점은 거른다 

                sqrDis = (line.origin - arr[i]).sqrMagnitude;
                if(minDis > sqrDis)
                {
                    minDis = sqrDis;
                    min = arr[i]; 
                }

                if (maxDis <= sqrDis)
                {
                    maxDis = sqrDis;
                    max = arr[i];
                }
            }

            return true; 
        }

        //선분과 점 처리 전용함수 
        public bool GetMinMax_Segement(Vector3 dir, out Vector3 min, out Vector3 max)
        {

            bool contact = false;
            Vector3[] arr = new Vector3[8];
            arr[0] = _intr_0_2.mPoint[0];
            arr[1] = _intr_0_2.mPoint[1];
            arr[2] = _intr_0_3.mPoint[0];
            arr[3] = _intr_0_3.mPoint[1];
            arr[4] = _intr_1_2.mPoint[0];
            arr[5] = _intr_1_2.mPoint[1];
            arr[6] = _intr_1_3.mPoint[0];
            arr[7] = _intr_1_3.mPoint[1];

            min = ConstV.v3_zero;
            max = ConstV.v3_zero;

            if (1 <= _intr_0_2.mQuantity || 1 <= _intr_0_3.mQuantity || 1 <= _intr_1_2.mQuantity || 1 <= _intr_1_3.mQuantity)
            {
                //DebugWide.LogBlue(_intr_0_2.mIntersectionType);
                //DebugWide.LogBlue(_intr_0_3.mIntersectionType);
                //DebugWide.LogBlue(_intr_1_2.mIntersectionType);
                //DebugWide.LogBlue(_intr_1_3.mIntersectionType);
                //for(int i=0;i<8;i++)
                //{
                //    DebugWide.LogBlue(i+" : " + arr[i]); 
                //}

                contact = true;

                //선택정렬을 한다 
                Vector3 temp;
                for (int a = 1; a < 8; a++)
                {
                    for (int i = a; i < 8; i++)
                    {
                        if (0 > Vector3.Dot(dir, arr[i] - arr[a - 1]))
                        {
                            temp = arr[a - 1];
                            arr[a - 1] = arr[i];
                            arr[i] = temp;
                        }
                    }
                }

                //0벡터를 제외하고 정렬된배열에서 최소값과 최대값을 찾는다  
                for (int i = 0; i < 8; i++)
                {
                    //DebugWide.LogBlue(i + "  " + arr[i]);
                    if (!Misc.IsZero(arr[i]))
                    {
                        min = arr[i];
                        break;
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    if (!Misc.IsZero(arr[7 - i]))
                    {
                        max = arr[7 - i];
                        break;
                    }
                }
            }//end if

            //_tetr01.Draw(Color.blue);
            //_tetr23.Draw(Color.magenta);
            //if(true == contact)
            //{
            //    for (int i = 0; i < 8; i++)
            //    {
            //        Color color = Color.green;
            //        if (Misc.IsZero(arr[i]))
            //            color = Color.black;
            //        if(Misc.IsZero(arr[i]-min))
            //            color = Color.black;
            //        if (Misc.IsZero(arr[i] - max))
            //            color = Color.black;
            //        DebugWide.DrawCircle(arr[i], 0.008f + 0.005f * i, color);
            //    }
                //DebugWide.DrawCircle(min, 0.03f, Color.red);
                //DebugWide.DrawCircle(max, 0.07f, Color.red);

            //    _intr_0_2.Draw(Color.black);
            //    _intr_0_3.Draw(Color.black);
            //    _intr_1_2.Draw(Color.black);
            //    _intr_1_3.Draw(Color.black);
            //}

            return contact;

        }

        // ** meetPt 를 지나는 적당한(근사값) 선분 구하기 ** 
        // ** start 와 end 의 길이가 같다는 전제로 작성된 알고리즘 **
        //선분이 start => end 로 이동시 사다리꼴 모양의 평면이 만들어진다 
        //사다리꼴의 위쪽을 v_up , 아래쪽을 v_down 하자
        //v_up 과 v_down 의 길이가 작은쪽을 기준으로 새로운 선분의 시작점을(origin) 구한다
        //새로운 선분은 origin 에서 meetPt 를 지난다 
        private void CalcSegment(Vector3 meetPt, LineSegment3 start, LineSegment3 end, out LineSegment3 newSeg)
        {

            Vector3 v_up = end.last - start.last;
            Vector3 v_down = end.origin - start.origin;
            float len_up = v_up.sqrMagnitude;
            float len_down = v_down.sqrMagnitude;

            Vector3 n_left = VOp.Normalize(start.direction);
            Vector3 n_right = VOp.Normalize(end.direction);
            float len_proj_left = Vector3.Dot(n_left, (meetPt - start.origin));
            float len_proj_right = Vector3.Dot(n_right, (meetPt - end.origin));
            float len_perp_left = ((n_left * len_proj_left + start.origin) - meetPt).magnitude;
            float len_perp_right = ((n_right * len_proj_right + end.origin) - meetPt).magnitude;
            float rate = len_perp_left / (len_perp_left + len_perp_right);

            Vector3 origin, last;
            float len_start = start.direction.magnitude;
            //작은쪽을 선택 
            if (len_up > len_down)
            {
                origin = v_down * rate + start.origin;

                last = VOp.Normalize(meetPt - origin) * len_start + origin;

            }
            else
            {
                last = v_up * rate + start.last;

                origin = VOp.Normalize(meetPt - last) * len_start + last;
            }
            newSeg = new LineSegment3(origin, last);

        }


        //fixedOriginPt : 고정된 새로운 선분의 출발점 
        private void CalcSegment(bool allowFixed, Vector3 fixedOriginPt, Vector3 meetPt, LineSegment3 start, LineSegment3 end, out LineSegment3 newSeg)
        {
            Vector3 origin, last;
            float len_start = start.direction.magnitude;

            if (allowFixed)
            {
                origin = fixedOriginPt;
                last = VOp.Normalize(meetPt - origin) * len_start + origin;
            }
            else
            {
                Vector3 v_up = end.last - start.last;
                Vector3 v_down = end.origin - start.origin;
                float len_up = v_up.sqrMagnitude;
                float len_down = v_down.sqrMagnitude;

                Vector3 n_left = VOp.Normalize(start.direction);
                Vector3 n_right = VOp.Normalize(end.direction);
                float len_proj_left = Vector3.Dot(n_left, (meetPt - start.origin));
                float len_proj_right = Vector3.Dot(n_right, (meetPt - end.origin));
                float len_perp_left = ((n_left * len_proj_left + start.origin) - meetPt).magnitude;
                float len_perp_right = ((n_right * len_proj_right + end.origin) - meetPt).magnitude;
                float rate = len_perp_left / (len_perp_left + len_perp_right);


                //NaN 예외처리 추가 
                if (Misc.IsZero(len_perp_left + len_perp_right))
                {
                    //DebugWide.LogYellow("prev: " + start + " cur: " + end + "  left: " + len_perp_left +"   right: "+ len_perp_right);
                    rate = 0;
                }


                //작은쪽을 선택 
                if (len_up > len_down)
                {
                    origin = v_down * rate + start.origin;

                    last = VOp.Normalize(meetPt - origin) * len_start + origin;

                }
                else
                {
                    last = v_up * rate + start.last;

                    origin = VOp.Normalize(meetPt - last) * len_start + last;
                }
            }


            newSeg = new LineSegment3(origin, last);
        }



        LineSegment3[] __list_line_A = new LineSegment3[3];
        LineSegment3[] __list_line_B = new LineSegment3[3];
        public void ClosestLine(out Vector3 min_A, out Vector3 min_B)
        {
            int count_A = 1;
            int count_B = 1;

            __list_line_A[0] = _cur_seg_A;
            __list_line_B[0] = _cur_seg_B;
            if (false == __isSeg_A)
            {
                count_A = 3;

                __list_line_A[1] = _tetr01.GetLine_Last();
                __list_line_A[2] = _tetr01.GetLine_Origin();
            }

            if (false == __isSeg_B)
            {
                count_B = 3;

                __list_line_B[1] = _tetr23.GetLine_Last();
                __list_line_B[2] = _tetr23.GetLine_Origin();
            }

            Vector3 pt_close_A, pt_close_B, dir_A_B;
            min_A = ConstV.v3_zero; min_B = ConstV.v3_zero;
            float min = 10000000f;
            float cur = 0f;
            for (int a = 0; a < count_A; a++)
            {
                for (int b = 0; b < count_B; b++)
                {
                    LineSegment3.ClosestPoints(out pt_close_A, out pt_close_B, __list_line_A[a], __list_line_B[b]);
                    dir_A_B = pt_close_B - pt_close_A;
                    cur = dir_A_B.sqrMagnitude;
                    if (min > cur)
                    {
                        min = cur;
                        min_A = pt_close_A;
                        min_B = pt_close_B;
                    }
                }
            }
        }

        Vector3 _pt_cur_A, _pt_cur_B;
        Vector3 _pt_prev_A, _pt_prev_B;
        public bool _intr_A_B_inside = false;
        public bool Find_TGuard_vs_TGuard(float rateAtoB, bool allowFixed_a, bool allowFixed_b, Transform root_0, Transform root_1)
        {
            int result_contact = 0;

            Vector3 minV = ConstV.v3_zero, maxV = ConstV.v3_zero;
            Vector3 meetPt = ConstV.v3_zero;
            //Vector3 pt_close_A, pt_close_B;
            float rad_AB = _radius_A + _radius_B;
            LineSegment3 newSegA = _cur_seg_A, newSegB = _cur_seg_B;

            _localRota_A = Quaternion.identity;
            _localRota_B = Quaternion.identity;
            _dir_move_A = ConstV.v3_zero;
            _dir_move_B = ConstV.v3_zero;

            LineSegment3.ClosestPoints(out _pt_prev_A, out _pt_prev_B, _prev_seg_A, _prev_seg_B);
            _prev_A_B_order = _pt_prev_B - _pt_prev_A;
            LineSegment3.ClosestPoints(out _pt_cur_A, out _pt_cur_B, _cur_seg_A, _cur_seg_B);
            _cur_A_B_order = _pt_cur_B - _pt_cur_A;

            //첫번째 통과선분을 찾아내기 위한 최소제곱거리 반환 , 정지상태를 가정하고 검사한다
            _sqrLen_TestCCD_Prev_A_B_Order = TestCCD_Prev_A_B_Order(_pt_prev_A, _pt_prev_B);

            _intr_A_B_inside = false;
            {
                result_contact = Collision_CCD_2(rateAtoB, allowFixed_a, allowFixed_b, root_0, root_1);
                //DebugWide.LogBlue("ccd : " + result_contact);

                if (0 == result_contact)
                {
                    if (rad_AB * rad_AB > _cur_A_B_order.sqrMagnitude)
                    {
                        _intr_A_B_inside = true;
                        result_contact = Collision_Normal_2(rateAtoB, allowFixed_a, allowFixed_b, root_0, root_1);
                    }
                    //prev 선분에 대하여 검사를 진행한다 
                    //else if (rad_AB * rad_AB > _prev_A_B_order.sqrMagnitude)
                    //{
                    //    if (0 <= Vector3.Dot(__dir_A, __dir_B))
                    //    {
                    //        _intr_A_B_inside = true;
                    //        result_contact = Collision_Normal_2(rateAtoB, allowFixed_a, allowFixed_b, root_0, root_1,
                    //            _prev_seg_A, _prev_seg_B, _pt_prev_A, _pt_prev_B);
                    //    }
                    //}
                    //DebugWide.LogBlue("inside : " + _intr_A_B_inside);
                }
            }



            if(1 != result_contact)
            {
                _prev_seg_A = _cur_seg_A;
                _prev_seg_B = _cur_seg_B;
            }

            //0 접촉없음 , 1 접촉있음 , 2 계산불가 
            if (1 == result_contact) return true;
            return false;
        }

        public void Collision_Normal_1(float rateAtoB, bool allowFixed_a, bool allowFixed_b, Transform root_0, Transform root_1)
        {
            //DebugWide.LogGreen("!! 현재선분검사  " + __isSeg_A + "  " + __isSeg_B + "  " + __dir_A + "  " + __dir_B);

            LineSegment3 newSegA = _cur_seg_A, newSegB = _cur_seg_B;

            //minV = maxV = meetPt = pt_close_A;
            //result_contact = true;


            float penetration = (_radius_A + _radius_B) - _cur_A_B_order.magnitude;
            Vector3 n_close_BA = -VOp.Normalize(_cur_A_B_order);
            Vector3 pt_first_A, pt_center_A;
            Vector3 pt_first_B, pt_center_B;

            //선분A , B가 서로 교차하였을 경우 방향에 맞게 침투길이를 다시 계산한다 
            //선분의 끝에서 끝없이 미는 버그가 있어 사용안함 
            //if(false == __isSeg_A)
            //{
            //    if( 0 > Vector3.Dot(dir_A_B, __dir_A))
            //    {
            //        n_close_BA *= -1f;
            //        penetration = (_radius_A * 2 + _radius_B * 2) - penetration;
            //        DebugWide.LogRed("침투 !! aaaa");
            //    }
            //}
            //else if(false == __isSeg_B)
            //{
            //    if (0 < Vector3.Dot(dir_A_B, __dir_B))
            //    {
            //        n_close_BA *= -1f;
            //        penetration = (_radius_A * 2 + _radius_B * 2) - penetration;
            //        DebugWide.LogRed("침투 !! bbbb");
            //    }
            //}

            //DebugWide.DrawCircle(pt_close_A, _radius_A, Color.blue);
            //DebugWide.DrawCircle(pt_close_B, _radius_B, Color.magenta);

            if (true == allowFixed_a)
            {
                CalcTGuard_First_ClosePt(false, penetration * (1f - rateAtoB), _pt_cur_A, n_close_BA, root_0, out pt_center_A, out pt_first_A);
                RotateTGuard_FirstToLast(_pt_cur_A, pt_first_A, root_0.position, _cur_seg_A, out newSegA, out _localRota_A);
                _meetPt_A = pt_first_A;
            }
            else
            {
                pt_first_A = _pt_cur_A + n_close_BA * (penetration * (1f - rateAtoB));
                _dir_move_A = pt_first_A - _pt_cur_A;
                //DebugWide.LogBlue(VOp.ToString(__dir_move_A));
                _meetPt_A = pt_first_A;
                //newSegA = LineSegment3.Move(_prev_seg_A, __dir_move_A);
                newSegA = LineSegment3.Move(_cur_seg_A, _dir_move_A);
            }

            if (true == allowFixed_b)
            {
                CalcTGuard_First_ClosePt(false, penetration * (rateAtoB), _pt_cur_B, -n_close_BA, root_1, out pt_center_B, out pt_first_B);
                RotateTGuard_FirstToLast(_pt_cur_B, pt_first_B, root_1.position, _cur_seg_B, out newSegB, out _localRota_B);
                _meetPt_B = pt_first_B;
            }
            else
            {

                pt_first_B = _pt_cur_B + -n_close_BA * (penetration * (rateAtoB));
                _dir_move_B = pt_first_B - _pt_cur_B;
                _meetPt_B = pt_first_B;
                //newSegB = LineSegment3.Move(_prev_seg_B, __dir_move_B);
                newSegB = LineSegment3.Move(_cur_seg_B, _dir_move_B);
            }


            //DebugWide.DrawCircle(pt_first_A, _radius_A, Color.blue);
            //DebugWide.DrawCircle(pt_first_B, _radius_B, Color.magenta);
            //DebugWide.DrawLine(pt_close_A, pt_close_B, Color.red);
            //===============


            _cur_seg_A = newSegA;
            _cur_seg_B = newSegB;
            _prev_seg_A = newSegA;
            _prev_seg_B = newSegB;

            //_minV = minV;
            //_maxV = maxV;
            //_meetPt = meetPt;
        }

        //public void CollisionDetector_CCD()
        //{ }
        //public void GetContact()
        //{ }
        //public void ResolveInterpenetration()
        //{ }

        //유사 CCD검사를 하여 close_A 와 close_B 사이의 제곱길이를 반환한다 
        //사각꼴과 선분이 서로 닿지 않고 반지름내 충돌상황인 경우 , 평면과 선분 검사를 하여 교점을 찾아낸다  
        //public float TestCCD_Cur_A_B_Order(Vector3 close_A, Vector3 close_B)
        //{
        //    Vector3 none;

        //    if (false == __isSeg_A)
        //    {
        //        Plane plane = new Plane(_cur_seg_A.origin, _cur_seg_A.last, _prev_seg_A.last);

        //        float inT;
        //        int result = Plane.Intersect(out inT, new Line3(_cur_seg_B.origin, _cur_seg_B.direction), plane);
        //        //DebugWide.LogBlue("plane 1 " + result);
        //        if (1 == result)
        //        {
        //            close_B = _cur_seg_B.origin + inT * _cur_seg_B.direction;

        //            if (true == __isSeg_B)
        //            {
        //                LineSegment3.ClosestPoints(out none, out close_A, new LineSegment3(close_B, close_B - __dir_A * 10000), _cur_seg_A);
        //            }
        //        }

        //    }

        //    if (false == __isSeg_B)
        //    {

        //        Plane plane = new Plane(_cur_seg_B.origin, _cur_seg_B.last, _prev_seg_B.last);
        //        //plane.Draw(5, Color.white);
        //        //Vector3 inPt;
        //        float inT;
        //        //int result = Plane.Intersect(out inPt, _cur_seg_A, plane);
        //        int result = Plane.Intersect(out inT, new Line3(_cur_seg_A.origin, _cur_seg_A.direction), plane);
        //        //DebugWide.LogBlue("plane 1 " + result);
        //        if (1 == result)
        //        {
        //            close_A = _cur_seg_A.origin + inT * _cur_seg_A.direction;
        //            //close_A = inPt;

        //            if (true == __isSeg_A)
        //            {
        //                LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A - __dir_B * 10000), _cur_seg_B);
        //            }
        //        }

        //    }

        //    return (close_A - close_B).sqrMagnitude;
        //}

        //정지상태를 가정하고 검사한다
        //유사 CCD검사를 하여 close_A 와 close_B 사이의 제곱길이를 반환한다 
        //사각꼴과 선분이 서로 닿지 않고 반지름내 충돌상황인 경우 , 평면과 선분 검사를 하여 교점을 찾아낸다  
        public float TestCCD_Prev_A_B_Order(Vector3 close_A, Vector3 close_B)
        {
            Vector3 none;

            if (false == __isSeg_A)
            {
                Plane plane = new Plane(_cur_seg_A.origin, _cur_seg_A.last, _prev_seg_A.last);

                float inT;
                int result = Plane.Intersect(out inT, new Line3(_prev_seg_B.origin, _prev_seg_B.direction), plane);
                //DebugWide.LogBlue("plane 1 " + result);
                if (1 == result)
                {
                    close_B = _prev_seg_B.origin + inT * _prev_seg_B.direction;

                    if (true == __isSeg_B)
                    {
                        LineSegment3.ClosestPoints(out none, out close_A, new LineSegment3(close_B, close_B - __dir_A * 10000), _prev_seg_A);
                    }
                }

            }

            if (false == __isSeg_B)
            {

                Plane plane = new Plane(_cur_seg_B.origin, _cur_seg_B.last, _prev_seg_B.last);
                //plane.Draw(5, Color.white);
                //Vector3 inPt;
                float inT;
                //int result = Plane.Intersect(out inPt, _cur_seg_A, plane);
                int result = Plane.Intersect(out inT, new Line3(_prev_seg_A.origin, _prev_seg_A.direction), plane);
                //DebugWide.LogBlue("plane 1 " + result);
                if (1 == result)
                {
                    close_A = _prev_seg_A.origin + inT * _prev_seg_A.direction;
                    //close_A = inPt;

                    if (true == __isSeg_A)
                    {
                        LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A - __dir_B * 10000), _prev_seg_B);
                    }
                }

            }

            return (close_A - close_B).sqrMagnitude;
        }

        //close_A 와 close_B 가 같을때의 예외처리가 안되어 있음 , 같을때 엉뚱한 위치로 계산한다  - fixme 
        public bool Collision_Normal_3(float rateAtoB, bool allowFixed_a, bool allowFixed_b, Transform root_0, Transform root_1)
        {

            LineSegment3 newSegA = _cur_seg_A, newSegB = _cur_seg_B;

            //float penetration = (_radius_A + _radius_B) - _cur_A_B_order.magnitude;
            //Vector3 dir_drop = VOp.Normalize(_cur_A_B_order);

            //------
            Vector3 close_A = _pt_cur_A;
            Vector3 close_B = _pt_cur_B;


            float rad_AB = _radius_A + _radius_B;
            Vector3 none;
            //------

            if (false == __isSeg_A)
            {
                Plane plane = new Plane(_cur_seg_A.origin, _cur_seg_A.last, _prev_seg_A.last);

                float inT;
                int result = Plane.Intersect(out inT, new Line3(_cur_seg_B.origin, _cur_seg_B.direction), plane);
                //DebugWide.LogBlue("plane 1 " + result);
                if (1 == result)
                {
                    close_B = _cur_seg_B.origin + inT * _cur_seg_B.direction;

                    if (true == __isSeg_B)
                    {
                        LineSegment3.ClosestPoints(out none, out close_A, new LineSegment3(close_B, close_B - __dir_A * 10000), _cur_seg_A);
                    }
                }

            }

            if (false == __isSeg_B)
            {

                Plane plane = new Plane(_cur_seg_B.origin, _cur_seg_B.last, _prev_seg_B.last);
                //plane.Draw(5, Color.white);
                //Vector3 inPt;
                float inT;
                //int result = Plane.Intersect(out inPt, _cur_seg_A, plane);
                int result = Plane.Intersect(out inT, new Line3(_cur_seg_A.origin, _cur_seg_A.direction), plane);
                //DebugWide.LogBlue("plane 1 " + result);
                if (1 == result)
                {
                    close_A = _cur_seg_A.origin + inT * _cur_seg_A.direction;
                    //close_A = inPt;

                    if (true == __isSeg_A)
                    {
                        LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A - __dir_B * 10000), _cur_seg_B);
                    }
                }

            }
            Vector3 meetPt = close_B + (close_A - close_B) * rateAtoB;
            Vector3 lastPt1 = meetPt;
            Vector3 lastPt2 = meetPt;
            Vector3 dir_drop = VOp.Normalize(close_B - close_A);

            //if(Misc.IsZero(_pt_cur_A - _pt_cur_B))
            //{
            //    DebugWide.LogBlue("zero llllll"); 
            //}
            //if (Misc.IsZero(dir_drop))
            //{
            //    DebugWide.LogBlue("zero 22222");
            //}

            //모서리에서 이상한 계산 막음 
            if (0 > Vector3.Dot(_prev_A_B_order, _cur_A_B_order))
            {
                dir_drop *= -1; 
            }
            //------
            Vector3 dir2 = _cur_seg_B.direction;
            if (0 < Vector3.Dot(_cur_seg_B.direction, __dir_A))
            {
                dir2 *= -1;
            }
            float theta = Geo.AngleSigned(-dir_drop, dir2, Vector3.Cross(-dir_drop, dir2));
            float dropping = rad_AB / (float)Math.Sin(theta * Mathf.Deg2Rad);
            dropping *= (1f - rateAtoB);
            //------

            //float dropping = rad_AB * (1f - rateAtoB);
            if (true == allowFixed_a)
            {
                //이상회전을 막는다 
                if ((_cur_seg_A.origin - _pt_cur_B).sqrMagnitude <= rad_AB * rad_AB)
                {
                    return false;
                }

                lastPt1 += -dir_drop * dropping; //dropping 처리 
                _meetPt_A = lastPt1;

                //현재 선분에 회전적용
                Vector3 firstPt = CalcTGuard_FirstPt2(lastPt1, root_0, _cur_seg_A);
                RotateTGuard_FirstToLast(firstPt, lastPt1, root_0.position, _cur_seg_A, out newSegA, out _localRota_A);

            }
            else
            {

                lastPt1 += -dir_drop * dropping;
                _dir_move_A = lastPt1 - close_A;
                _meetPt_A = lastPt1;

                newSegA = LineSegment3.Move(_cur_seg_A, _dir_move_A);

            }


            //------
            dir2 = _cur_seg_A.direction;
            if (0 < Vector3.Dot(_cur_seg_A.direction, __dir_B))
            {
                dir2 *= -1;
            }
            theta = Geo.AngleSigned(-dir_drop, dir2, Vector3.Cross(-dir_drop, dir2));
            dropping = rad_AB / (float)Math.Sin(theta * Mathf.Deg2Rad);
            dropping *= (rateAtoB);
            //------

            //dropping = rad_AB * (rateAtoB);
            if (true == allowFixed_b)
            {
                //이상회전을 막는다 
                if ((_cur_seg_B.origin - _pt_cur_A).sqrMagnitude <= rad_AB * rad_AB)
                {
                    return false;
                }

                lastPt2 += dir_drop * dropping; //dropping 처리
                _meetPt_B = lastPt2;

                //현재 선분에 회전적용
                Vector3 firstPt = CalcTGuard_FirstPt2(lastPt2, root_1, _cur_seg_B);
                RotateTGuard_FirstToLast(firstPt, lastPt2, root_1.position, _cur_seg_B, out newSegB, out _localRota_B);

            }
            else
            {

                //pt_close_A
                lastPt2 += dir_drop * dropping;
                _dir_move_B = lastPt2 - close_B;
                _meetPt_B = lastPt2;

                newSegB = LineSegment3.Move(_cur_seg_B, _dir_move_B);

            }

            //------
            _cur_seg_A = newSegA;
            _cur_seg_B = newSegB;
            _prev_seg_A = newSegA;
            _prev_seg_B = newSegB;

            _close_B = close_B;
            _close_A = close_A;
            _meetPt = meetPt;

            return true;
        }

        public float _sqrLen_TestCCD_Prev_A_B_Order = 0;
        public int Collision_Normal_2(float rateAtoB, bool allowFixed_a, bool allowFixed_b, Transform root_0, Transform root_1)
        {
            //평면상에서의 선분vs선분의 겹침처리를 안하려고 추가했으나 , 엇갈려 만날을 때를 처리하지 않는문제 때문에 주석한다 
            //if (Misc.IsZero(_cur_A_B_order))
            //{
            //    _prev_seg_A = _cur_seg_A;
            //    _prev_seg_B = _cur_seg_B;
            //    DebugWide.LogBlue("zero!!");
            //    return;
            //}

            //DebugWide.LogGreen("!! c_n2 현재선분검사  " + __isSeg_A + "  " + __isSeg_B + "  " + __dir_A + "  " + __dir_B);

            LineSegment3 newSegA = _cur_seg_A, newSegB = _cur_seg_B;

            //float penetration = (_radius_A + _radius_B) - _cur_A_B_order.magnitude;

            //_cur_A_B_order 가 0이 될때 방향을 못구하는 문제가 발생한다. 이때 _prev_A_B_order 를 쓴다  
            Vector3 dir_drop = _cur_A_B_order;
            if (Misc.IsZero(_cur_A_B_order))
            {
                dir_drop = _prev_A_B_order;
            }
            dir_drop = VOp.Normalize(dir_drop);

            //------
            Vector3 close_A = _pt_cur_A;
            Vector3 close_B = _pt_cur_B;
            Vector3 meetPt = close_B + (close_A - close_B) * rateAtoB;
            Vector3 lastPt1 = meetPt;
            Vector3 lastPt2 = meetPt;
            float rad_AB = _radius_A + _radius_B;

            //------
            //_sqrLen_TestCCD_Cur_A_B_Order = TestCCD_Cur_A_B_Order(close_A, close_B);
            //------

            float dropping = rad_AB * (1f - rateAtoB);
            if (true == allowFixed_a)
            {
                //이상회전을 막는다 
                if ((_cur_seg_A.origin - _pt_cur_B).sqrMagnitude <= rad_AB*rad_AB)
                {
                    return 2;
                }

                lastPt1 += -dir_drop * dropping; //dropping 처리 
                _meetPt_A = lastPt1;

                //현재 선분에 회전적용
                Vector3 firstPt = CalcTGuard_FirstPt2(lastPt1, root_0, _cur_seg_A);
                RotateTGuard_FirstToLast(firstPt, lastPt1, root_0.position, _cur_seg_A, out newSegA, out _localRota_A);

            }
            else
            {

                lastPt1 += -dir_drop * dropping;
                _dir_move_A = lastPt1 - close_A;
                _meetPt_A = lastPt1;

                newSegA = LineSegment3.Move(_cur_seg_A, _dir_move_A);

            }


            dropping = rad_AB * (rateAtoB);
            if (true == allowFixed_b)
            {
                //이상회전을 막는다 
                if ((_cur_seg_B.origin - _pt_cur_A).sqrMagnitude <= rad_AB * rad_AB)
                {
                    return 2;
                }

                lastPt2 += dir_drop * dropping; //dropping 처리
                _meetPt_B = lastPt2;

                //현재 선분에 회전적용
                Vector3 firstPt = CalcTGuard_FirstPt2(lastPt2, root_1, _cur_seg_B);
                RotateTGuard_FirstToLast(firstPt, lastPt2, root_1.position, _cur_seg_B, out newSegB, out _localRota_B);

            }
            else
            {

                //pt_close_A
                lastPt2 += dir_drop * dropping;
                _dir_move_B = lastPt2 - close_B;
                _meetPt_B = lastPt2;

                newSegB = LineSegment3.Move(_cur_seg_B, _dir_move_B);

            }

            //------
            _cur_seg_A = newSegA;
            _cur_seg_B = newSegB;
            _prev_seg_A = newSegA;
            _prev_seg_B = newSegB;

            _close_B = close_B;
            _close_A = close_A;
            _meetPt = meetPt;

            return 1;
        }

        public int Collision_Normal_2(float rateAtoB, bool allowFixed_a, bool allowFixed_b, Transform root_0, Transform root_1,
            LineSegment3 segA, LineSegment3 segB, Vector3 close_A , Vector3 close_B)
        {

            DebugWide.LogGreen("!! c_n2 현재선분검사  " + __isSeg_A + "  " + __isSeg_B + "  " + __dir_A + "  " + __dir_B);

            LineSegment3 newSegA, newSegB;


            //dir_drop 가 0이 될때 방향을 못구하는 문제가 발생한다. 이때 _prev_A_B_order 를 쓴다  
            Vector3 dir_drop = close_B - close_A;
            if (Misc.IsZero(dir_drop))
            {
                dir_drop = _prev_A_B_order;
            }
            dir_drop = VOp.Normalize(dir_drop);

            //------

            Vector3 meetPt = close_B + (close_A - close_B) * rateAtoB;
            Vector3 lastPt1 = meetPt;
            Vector3 lastPt2 = meetPt;
            float rad_AB = _radius_A + _radius_B;


            float dropping = rad_AB * (1f - rateAtoB);
            if (true == allowFixed_a)
            {
                //이상회전을 막는다 
                if ((segA.origin - close_B).sqrMagnitude <= rad_AB * rad_AB)
                {
                    return 2;
                }

                lastPt1 += -dir_drop * dropping; //dropping 처리 
                _meetPt_A = lastPt1;

                //현재 선분에 회전적용
                Vector3 firstPt = CalcTGuard_FirstPt2(lastPt1, root_0, segA);
                RotateTGuard_FirstToLast(firstPt, lastPt1, root_0.position, segA, out newSegA, out _localRota_A);

            }
            else
            {

                lastPt1 += -dir_drop * dropping;
                _dir_move_A = lastPt1 - close_A;
                _meetPt_A = lastPt1;

                newSegA = LineSegment3.Move(segA, _dir_move_A);

            }


            dropping = rad_AB * (rateAtoB);
            if (true == allowFixed_b)
            {
                //이상회전을 막는다 
                if ((segB.origin - close_A).sqrMagnitude <= rad_AB * rad_AB)
                {
                    return 2;
                }

                lastPt2 += dir_drop * dropping; //dropping 처리
                _meetPt_B = lastPt2;

                //현재 선분에 회전적용
                Vector3 firstPt = CalcTGuard_FirstPt2(lastPt2, root_1, segB);
                RotateTGuard_FirstToLast(firstPt, lastPt2, root_1.position, segB, out newSegB, out _localRota_B);

            }
            else
            {

                //pt_close_A
                lastPt2 += dir_drop * dropping;
                _dir_move_B = lastPt2 - close_B;
                _meetPt_B = lastPt2;

                newSegB = LineSegment3.Move(segB, _dir_move_B);

            }

            //------
            _cur_seg_A = newSegA;
            _cur_seg_B = newSegB;
            _prev_seg_A = newSegA;
            _prev_seg_B = newSegB;

            _close_B = close_B;
            _close_A = close_A;
            _meetPt = meetPt;

            return 1;
        }


        public bool Collision_CCD_3(float rateAtoB, bool allowFixed_a, bool allowFixed_b, Transform root_0, Transform root_1)
        {
            bool result_contact = false;
            //bool isContact_onPlan = false;
            Vector3 close_B = ConstV.v3_zero, close_A = ConstV.v3_zero;
            Vector3 meetPt = ConstV.v3_zero;
            float rad_AB = _radius_A + _radius_B;
            LineSegment3 newSegA = _cur_seg_A, newSegB = _cur_seg_B;
            //DebugWide.LogGreen("!! 사각꼴(선분) vs 사각꼴(선분) 검사 ");


            _intr_0_2.Find_Twice();
            _intr_0_3.Find_Twice();
            _intr_1_2.Find_Twice();
            _intr_1_3.Find_Twice();

            //사각꼴이 서로 같은 평면에서 만난경우
            if (eIntersectionType.PLANE == _intr_0_2.mIntersectionType ||
                eIntersectionType.PLANE == _intr_0_3.mIntersectionType ||
                eIntersectionType.PLANE == _intr_1_2.mIntersectionType ||
                eIntersectionType.PLANE == _intr_1_3.mIntersectionType)
            {

                //평면에서 만난 경우 처리하지 않는다. 계산실패 
                //_prev_seg_A = _cur_seg_A;
                //_prev_seg_B = _cur_seg_B;
                return false;

                Vector3 none;
                Vector3 ori, last;
                float radAB = _radius_A + _radius_B;
                //선분과 사각꼴이 같은 평면에서 만난경우
                if (true == __isSeg_A && false == __isSeg_B)
                {
                    //DebugWide.LogBlue("!! 선분 vs 사각꼴 ");

                    GetMinMax_ContactPt(_cur_seg_A.origin, out ori, out last, 4);

                    //순서를 맞춰준다 
                    if(0 > Vector3.Dot(_prev_seg_A.direction, _prev_seg_B.direction))
                    {
                        //swap
                        none = ori;
                        ori = last;
                        last = none;   
                    }

                    //짧은 쪽을 선택한다 
                    float a_sqr, b_sqr;
                    a_sqr = (ori - _prev_seg_B.origin).sqrMagnitude;
                    b_sqr = (last - _prev_seg_B.last).sqrMagnitude;
                    if(a_sqr >= b_sqr)
                    {
                        close_A = last;
                    }
                    else
                    {
                        close_A = ori;
                    }

                    //삼각형인 경우 
                    if (Misc.IsZero(_cur_seg_B.origin - _prev_seg_B.origin))
                    {
                        close_A = last;
                    }

                    LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A + __dir_B * 10000), _cur_seg_B);

                    result_contact = true;

                }
                else if (false == __isSeg_A && true == __isSeg_B)
                {
                    //DebugWide.LogBlue("!! 사각꼴 vs 선분 ");

                    GetMinMax_ContactPt(_cur_seg_B.origin, out ori, out last, 4);

                    //순서를 맞춰준다 
                    if (0 > Vector3.Dot(_prev_seg_A .direction, _prev_seg_B.direction))
                    {
                        //swap
                        none = ori;
                        ori = last;
                        last = none;
                    }

                    //짧은 쪽을 선택한다 
                    float a_sqr, b_sqr;
                    a_sqr = (ori - _prev_seg_A.origin).sqrMagnitude;
                    b_sqr = (last - _prev_seg_A.last).sqrMagnitude;
                    if (a_sqr >= b_sqr)
                    {
                        close_A = last;
                    }
                    else
                    {
                        close_A = ori;
                    }

                    //삼각형인 경우 
                    if (Misc.IsZero(_cur_seg_A.origin - _prev_seg_A.origin))
                    {
                        close_A = last;
                    }

                    LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A + __dir_A * 10000), _cur_seg_A);

                    //swap
                    none = close_B;
                    close_B = close_A;
                    close_A = none;

                    result_contact = true;
                }
                //사각꼴과 사각꼴이 같은 평면에서 만난경우 
                else if (false == __isSeg_A && false == __isSeg_B)
                {
                    //!!!! 초기에 prev 와 cur 을 같게 안만들어주면 여기로 처리가 들어오게 된다
                    //  잘못된 정보에 의해 접촉한것으로 계산하게 되며 초기값에 의해 방향성을 가지게 되어 
                    //  dropping 에서 잘못된 방향으로 무한히 떨어뜨리게 된다.
                    //DebugWide.LogRed("!! 사각꼴과 사각꼴이 같은 평면에서 만난경우 ");


                    //GetMinMax_Tetragon(_cur_seg_A, out ori, out last);
                    //DebugWide.DrawCircle(ori, 0.02f, Color.white);
                    //DebugWide.DrawCircle(last, 0.04f, Color.white);

                    //_prev_seg_A = _cur_seg_A;
                    //_prev_seg_B = _cur_seg_B;
                    //return false; //평면에서 만난 경우 처리하지 않는다 

                    //평면에서 만난 경우 처리하지 않는다 
                    result_contact = false;

                }
                meetPt = close_B + (close_A - close_B) * rateAtoB;

                //DebugWide.LogBlue("!! c_ccd 0 현재선분검사  " + __isSeg_A + "  " + __isSeg_B + "  " + __dir_A + "  " + __dir_B);

            }
            //사각꼴(선분)이 서로 엇갈려 만난경우
            else if (1 <= _intr_0_2.mQuantity || 1 <= _intr_0_3.mQuantity || 1 <= _intr_1_2.mQuantity || 1 <= _intr_1_3.mQuantity)
            {

                //GetMinMax_ContactPt 함수는 선분값 상태에서 최소/최대값을 제대로 못찾는다. 선분값에서는 GetMinMax_Segement 함수 사용하기
                //result_contact = GetMinMax_ContactPt(_cur_seg_A.origin, out _minV, out _maxV, 2);

                //DebugWide.LogBlue("엇갈려 " + result_contact + "  " );

                //선분vs사각꼴 , 사각꼴vs선분의 경우 minV 와 maxV가 동일함. 한점에서 만나기 때문. 그렇기 때문에 __dir_A 값과 상관없음 
                //사각꼴vs사각꼴의 경우에 minV 와 maxV가 다름. __dir_A값 기준으로 minV 와 maxV를 구하게  됨
                GetMinMax_Segement(__dir_A, out close_B, out close_A);

                result_contact = true;

                //사각꼴과 선분이 만난 경우 : 교점이 하나만 나오므로 max를 따로 구해야 한다 
                if (true == __isSeg_A && false == __isSeg_B)
                {

                    //min 구하기 
                    Vector3 none;
                    Line3.ClosestPoints(out close_B, out none, new Line3(close_A, __dir_B), new Line3(_cur_seg_B.origin, _cur_seg_B.direction));
                    //DebugWide.LogBlue("aaa ");

                }
                else if (false == __isSeg_A && true == __isSeg_B)
                {

                    //max 구하기
                    Vector3 none;
                    Line3.ClosestPoints(out close_A, out none, new Line3(close_B, __dir_A), new Line3(_cur_seg_A.origin, _cur_seg_A.direction));
                    //DebugWide.LogBlue("bbb ");

                }
                //사각꼴과 사각꼴이 만난 경우
                else if (false == __isSeg_A && false == __isSeg_B)
                {
                    Vector3 dir_BtoA = close_A - close_B;
                    Vector3 none;

                    //사각꼴이 서로 방향이 같은 경우 
                    if (0 < Vector3.Dot(__dir_A, __dir_B))
                    {

                        //20210705 실험노트에 알고리즘 있음 
                        //밀기처리 할 수 있음
                        if (0 > Vector3.Dot(_cur_A_B_order, _prev_A_B_order))
                        {
                            //B가 A를 민다
                            if (0 < Vector3.Dot(_cur_A_B_order, __dir_A))
                            {
                                LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A + dir_BtoA * 10000), _cur_seg_B);
                                //Line3.ClosestPoints(out minV, out none, new Line3(maxV, dir), new Line3(_cur_seg_B.origin, _cur_seg_B.direction));
                            }
                            //A가 B를 민다
                            else
                            {
                                LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A + dir_BtoA * 10000), _cur_seg_A);
                                //Line3.ClosestPoints(out minV, out none, new Line3(maxV, dir), new Line3(_cur_seg_A.origin, _cur_seg_A.direction));

                                //swap
                                none = close_B;
                                close_B = close_A;
                                close_A = none;
                            }
                        }
                        //밀기처리 할 수 없음
                        else
                        {
                            result_contact = false;
                            //_prev_seg_A = _cur_seg_A;
                            //_prev_seg_B = _cur_seg_B;
                            //return false;

                            //--밀기처리 시험 , 논리적으로 선택 할 수 없는 문제가 있음  
                            //B가 A를 민다
                            //if (0 < Vector3.Dot(_cur_A_B_order, __dir_A))
                            //{
                            //    Line3.ClosestPoints(out minV, out none, new Line3(maxV, dir), new Line3(_cur_seg_B.origin, _cur_seg_B.direction));
                            //}
                            ////A가 B를 민다
                            //else
                            //{
                            //    Line3.ClosestPoints(out minV, out none, new Line3(maxV, dir), new Line3(_cur_seg_A.origin, _cur_seg_A.direction));

                            //    //swap
                            //    none = minV;
                            //    minV = maxV;
                            //    maxV = none;
                            //}

                        }

                    }
                    //사각꼴이 서로 방향이 다른 경우 
                    else
                    {
                        LineSegment3.ClosestPoints(out none, out close_A, new LineSegment3(close_A, close_A + dir_BtoA * 10000), _cur_seg_A);
                        LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A - dir_BtoA * 10000), _cur_seg_B);
                        //Line3.ClosestPoints(out maxV, out none, new Line3(maxV, dir), new Line3(_cur_seg_A.origin, _cur_seg_A.direction));
                        //Line3.ClosestPoints(out minV, out none, new Line3(maxV, -dir), new Line3(_cur_seg_B.origin, _cur_seg_B.direction));
                    }
                }

                meetPt = close_B + (close_A - close_B) * rateAtoB;

            }else
            {
                //if (true == __isSeg_A && true == __isSeg_B)
                //{

                //    LineSegment3 mer_a = LineSegment3.Merge(_prev_seg_A, _cur_seg_A);
                //    LineSegment3 mer_b = LineSegment3.Merge(_prev_seg_B, _cur_seg_B);
                //    //mer_a.Draw(Color.black);
                //    //mer_b.Draw(Color.white);

                //    LineSegment3.ClosestPoints(out minV, out maxV, mer_a, mer_b);

                //    //적당히 작은값을 설정해준다. 너무 작은값은 검사를 통과하지 못한다 
                //    if (0.0000001f > (minV - maxV).sqrMagnitude)
                //    {
                //        //DebugWide.LogBlue("교점이 있음 2");
                //        //교점이 있음 
                //        //isContact_onPlan = true;
                //        result_contact = true;
                //    }

                //    DebugWide.LogBlue(result_contact + "  " + 4);
                //} 

                result_contact = false;
            }

            //==================================================

            //동일한 점일떄는 CCD를 처리하지 못한다 
            if (Misc.IsZero(close_A - close_B))
            {
                result_contact = false;
            }

            if (result_contact)
            {

                //DebugWide.AddDrawQ_Line(close_A, close_B, Color.red);
                //DebugWide.AddDrawQ_Circle(meetPt, 0.02f, Color.red);
                //DebugWide.LogBlue("!! 사각꼴(선분)이 서로 엇갈려 만난 경우  r:" + result_contact + "  sA:" + __isSeg_A + "  sB:" + __isSeg_B);

                Vector3 dir_BtoA = close_A - close_B;
                dir_BtoA = VOp.Normalize(dir_BtoA);

                Vector3 lastPt1 = meetPt, lastPt2 = meetPt;


                //----------
                //sin@ = b/a , b = rad_AB 로 알고있음 , a를 구해야함 , a * sin@ = b , a = b / sin@
                Vector3 dir2 = _cur_seg_B.direction;
                if (0 < Vector3.Dot(_cur_seg_B.direction, __dir_A))
                {
                    dir2 *= -1;
                }
                float theta = Geo.AngleSigned(dir_BtoA, dir2, Vector3.Cross(dir_BtoA, dir2));
                float dropping = rad_AB / (float)Math.Sin(theta * Mathf.Deg2Rad);
                dropping *= (1f - rateAtoB);
                //----------

                //float dropping = rad_AB * (1f - rateAtoB);
                if (true == allowFixed_a)
                {

                    lastPt1 += -dir_BtoA  * dropping ; //dropping 처리 
                    _meetPt_A = lastPt1;
                    //이전 선분에 회전적용
                    //Vector3 firstPt = CalcTGuard_FirstPt2(lastPt1 ,root_0, _prev_seg_A);
                    //RotateTGuard_FirstToLast(firstPt, lastPt1, root_0.position, _prev_seg_A, out newSegA, out __localRota_A);    

                    //현재 선분에 회전적용
                    Vector3 firstPt = CalcTGuard_FirstPt2(lastPt1, root_0, _cur_seg_A);
                    RotateTGuard_FirstToLast(firstPt, lastPt1, root_0.position, _cur_seg_A, out newSegA, out _localRota_A);

                    //DebugWide.DrawCircle(lastPt1, _radius_A, Color.blue);

                }
                else
                {

                    lastPt1 += -dir_BtoA  * dropping;
                    _dir_move_A = lastPt1 - close_A;
                    _meetPt_A = lastPt1;
                    //newSegA = LineSegment3.Move(_prev_seg_A, __dir_move_A);
                    newSegA = LineSegment3.Move(_cur_seg_A, _dir_move_A);

                    //DebugWide.AddDrawQ_Line(newSegA.origin, newSegA.last, Color.red);
                    //DebugWide.AddDrawQ_Circle(_meetPt_A, _radius_A, Color.red);
                }

                //----------
                //sin@ = b/a , b = rad_AB 로 알고있음 , a를 구해야함 , a * sin@ = b , a = b / sin@
                dir2 = _cur_seg_A.direction;
                if (0 < Vector3.Dot(_cur_seg_A.direction, __dir_B))
                {
                    dir2 *= -1;
                }
                theta = Geo.AngleSigned(dir_BtoA, dir2, Vector3.Cross(dir_BtoA, dir2));
                dropping = rad_AB / (float)Math.Sin(theta * Mathf.Deg2Rad);
                dropping *= (rateAtoB);
                //----------

                //dropping = rad_AB * (rateAtoB);
                if (true == allowFixed_b)
                {

                    lastPt2 += dir_BtoA  * dropping; //dropping 처리
                    _meetPt_B = lastPt2;
                    //이전 선분에 회전적용
                    //Vector3 firstPt = CalcTGuard_FirstPt2(lastPt2, root_1, _prev_seg_B);
                    //RotateTGuard_FirstToLast(firstPt, lastPt2, root_1.position, _prev_seg_B, out newSegB, out __localRota_B);    

                    //현재 선분에 회전적용
                    Vector3 firstPt = CalcTGuard_FirstPt2(lastPt2, root_1, _cur_seg_B);
                    RotateTGuard_FirstToLast(firstPt, lastPt2, root_1.position, _cur_seg_B, out newSegB, out _localRota_B);

                    //DebugWide.DrawCircle(lastPt2, _radius_B, Color.magenta);

                }
                else
                {

                    //pt_close_A
                    lastPt2 += dir_BtoA  * dropping;
                    _dir_move_B = lastPt2 - close_B;
                    _meetPt_B = lastPt2;
                    //newSegB = LineSegment3.Move(_prev_seg_B, __dir_move_B);
                    newSegB = LineSegment3.Move(_cur_seg_B, _dir_move_B);

                    //DebugWide.AddDrawQ_Line(newSegB.origin, newSegB.last, Color.black);
                    //DebugWide.AddDrawQ_Circle(_meetPt_B, _radius_B, Color.black);
                }

                _cur_seg_A = newSegA;
                _cur_seg_B = newSegB;
                _prev_seg_A = newSegA;
                _prev_seg_B = newSegB;

                _close_B = close_B;
                _close_A = close_A;
                _meetPt = meetPt;
            }
            //else
            //{
            //    _prev_seg_A = _cur_seg_A;
            //    _prev_seg_B = _cur_seg_B;
            //}

            return result_contact;
        }

        public int Collision_CCD_2(float rateAtoB, bool allowFixed_a, bool allowFixed_b, Transform root_0, Transform root_1)
        {
            int result_contact = 0; //0 접점없음 , 1 접점있음 , 2 계산불가
            //bool isContact_onPlan = false;
            Vector3 close_B = ConstV.v3_zero, close_A = ConstV.v3_zero;
            Vector3 meetPt = ConstV.v3_zero;
            float rad_AB = _radius_A + _radius_B;
            LineSegment3 newSegA = _cur_seg_A, newSegB = _cur_seg_B;
            //DebugWide.LogGreen("!! 사각꼴(선분) vs 사각꼴(선분) 검사 ");


            _intr_0_2.Find_Twice();
            _intr_0_3.Find_Twice();
            _intr_1_2.Find_Twice();
            _intr_1_3.Find_Twice();

            //사각꼴이 서로 같은 평면에서 만난경우
            if (eIntersectionType.PLANE == _intr_0_2.mIntersectionType ||
                eIntersectionType.PLANE == _intr_0_3.mIntersectionType ||
                eIntersectionType.PLANE == _intr_1_2.mIntersectionType ||
                eIntersectionType.PLANE == _intr_1_3.mIntersectionType)
            {

                //평면에서 만난 경우 처리하지 않는다. 계산실패 

                return 2;

            }
            //사각꼴(선분)이 서로 엇갈려 만난경우
            else if (1 <= _intr_0_2.mQuantity || 1 <= _intr_0_3.mQuantity || 1 <= _intr_1_2.mQuantity || 1 <= _intr_1_3.mQuantity)
            {

                //선분vs사각꼴 , 사각꼴vs선분의 경우 minV 와 maxV가 동일함. 한점에서 만나기 때문. 그렇기 때문에 __dir_A 값과 상관없음 
                //사각꼴vs사각꼴의 경우에 minV 와 maxV가 다름. __dir_A값 기준으로 minV 와 maxV를 구하게  됨
                //GetMinMax_Segement(__dir_A, out close_B, out close_A);

                Vector3 horiDirA, horiDirB; //수평방향 
                Vector3 outPrev, outCur;
                Ray3.ClosestPoints(out outPrev, out outCur, new Ray3(_prev_seg_A.origin, _prev_seg_A.direction), new Ray3(_cur_seg_A.origin, _cur_seg_A.direction));
                horiDirA = outCur - outPrev;
                Ray3.ClosestPoints(out outPrev, out outCur, new Ray3(_prev_seg_B.origin, _prev_seg_B.direction), new Ray3(_cur_seg_B.origin, _cur_seg_B.direction));
                horiDirB = outCur - outPrev;

                if(Misc.IsZero(_prev_seg_A.origin - _cur_seg_A.origin))
                {
                    horiDirA = __dir_A;
                }
                if (Misc.IsZero(_prev_seg_B.origin - _cur_seg_B.origin))
                {
                    horiDirB = __dir_B;
                }

                GetMinMax_Segement(horiDirA, out close_B, out close_A);
                //DebugWide.DrawCircle(close_A, 0.07f, Color.red);
                //DebugWide.DrawCircle(close_B, 0.1f, Color.red);


                result_contact = 1;


                //DebugWide.LogBlue("엇갈려 " + result_contact + "  " + __isSeg_A + "  " + __isSeg_B);

                //사각꼴과 선분이 만난 경우 : 교점이 하나만 나오므로 max를 따로 구해야 한다 
                if (true == __isSeg_A && false == __isSeg_B)
                {

                    //min 구하기 
                    Vector3 none;
                    Line3.ClosestPoints(out close_B, out none, new Line3(close_A, __dir_B), new Line3(_cur_seg_B.origin, _cur_seg_B.direction));
                    //DebugWide.LogBlue("aaa ");

                }
                else if (false == __isSeg_A && true == __isSeg_B)
                {

                    //max 구하기
                    Vector3 none;
                    Line3.ClosestPoints(out close_A, out none, new Line3(close_B, __dir_A), new Line3(_cur_seg_A.origin, _cur_seg_A.direction));
                    //DebugWide.LogBlue("bbb ");

                }
                //사각꼴과 사각꼴이 만난 경우
                else if (false == __isSeg_A && false == __isSeg_B)
                {
                    Vector3 dir_BtoA = close_A - close_B;
                    Vector3 none;

                    //사각꼴이 서로 방향이 같은 경우 
                    //if (0 < Vector3.Dot(__dir_A, __dir_B))
                    if (0 < Vector3.Dot(horiDirA, horiDirB))
                    {

                        //20210705 실험노트에 알고리즘 있음 
                        //밀기처리 할 수 있음
                        if (0 > Vector3.Dot(_cur_A_B_order, _prev_A_B_order))
                        {

                            //B가 A를 민다
                            //if (0 < Vector3.Dot(_cur_A_B_order, __dir_A))
                            if (0 < Vector3.Dot(_cur_A_B_order, horiDirA))
                            {
                                LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A + dir_BtoA * 10000), _cur_seg_B);
                                //DebugWide.LogBlue("B -> A");

                                //B 또는 A 가 밀때 이상처리 될떄가 있다. 처리안하도록 걸러낸다 
                                Vector3 closePt = Line3.ClosestPoint(new Line3(_cur_seg_A.origin, _cur_seg_A.direction), close_A);
                                if (false == Misc.IsZero(closePt - close_A))
                                {
                                    result_contact = 2;
                                }
                            }
                            //A가 B를 민다
                            //else
                            else if (0 > Vector3.Dot(_cur_A_B_order, horiDirB))
                            {
                                LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A + dir_BtoA * 10000), _cur_seg_A);
                                //DebugWide.LogGreen("A -> B");

                                Vector3 closePt = Line3.ClosestPoint(new Line3(_cur_seg_B.origin, _cur_seg_B.direction), close_A);
                                if(false == Misc.IsZero(closePt-close_A))
                                {
                                    result_contact = 2;
                                }

                                //swap
                                none = close_B;
                                close_B = close_A;
                                close_A = none;
                            }
                            else
                            {
                                //DebugWide.LogRed(" xxxx ");
                                result_contact = 2;
                            }
                        }
                        //밀기처리 할 수 없음
                        else
                        {
                            result_contact = 2;

                        }

                    }
                    //사각꼴이 서로 방향이 다른 경우 
                    else
                    {
                        LineSegment3.ClosestPoints(out none, out close_A, new LineSegment3(close_A, close_A + dir_BtoA * 10000), _cur_seg_A);
                        LineSegment3.ClosestPoints(out none, out close_B, new LineSegment3(close_A, close_A - dir_BtoA * 10000), _cur_seg_B);

                        //DebugWide.DrawCircle(close_A, 0.07f, Color.red);
                        //DebugWide.DrawCircle(close_B, 0.1f, Color.red);
                    }
                }

                meetPt = close_B + (close_A - close_B) * rateAtoB;

            }
            else
            {

                result_contact = 0;
            }

            //==================================================

            //동일한 점일떄는 CCD를 처리하지 못한다 
            if (Misc.IsZero(close_A - close_B))
            {
                result_contact = 0;
            }

            if (1 == result_contact)
            {

                //DebugWide.AddDrawQ_Line(close_A, close_B, Color.red);
                //DebugWide.AddDrawQ_Circle(meetPt, 0.02f, Color.red);
                //DebugWide.LogBlue("!! 사각꼴(선분)이 서로 엇갈려 만난 경우  r:" + result_contact + "  sA:" + __isSeg_A + "  sB:" + __isSeg_B);

                //Vector3 dir_BtoA = close_A - close_B; //만나는 각이 커질수록 파고드는 현상이 있음 
                //_cur_A_B_order를 사용하면 collision_nomal_2 과 연계해 표현이 자연스러워 진다 
                Vector3 dir_BtoA = -_cur_A_B_order;
                dir_BtoA = VOp.Normalize(dir_BtoA);

                Vector3 lastPt1 = meetPt, lastPt2 = meetPt;


                float dropping = rad_AB * (1f - rateAtoB);
                if (true == allowFixed_a)
                {

                    lastPt1 += -dir_BtoA * dropping; //dropping 처리 
                    _meetPt_A = lastPt1;

                    //현재 선분에 회전적용
                    Vector3 firstPt = CalcTGuard_FirstPt2(lastPt1, root_0, _cur_seg_A);
                    RotateTGuard_FirstToLast(firstPt, lastPt1, root_0.position, _cur_seg_A, out newSegA, out _localRota_A);

                }
                else
                {

                    lastPt1 += -dir_BtoA * dropping;
                    _dir_move_A = lastPt1 - close_A;
                    _meetPt_A = lastPt1;

                    newSegA = LineSegment3.Move(_cur_seg_A, _dir_move_A);

                }

                dropping = rad_AB * (rateAtoB);
                if (true == allowFixed_b)
                {

                    lastPt2 += dir_BtoA * dropping; //dropping 처리
                    _meetPt_B = lastPt2;

                    //현재 선분에 회전적용
                    Vector3 firstPt = CalcTGuard_FirstPt2(lastPt2, root_1, _cur_seg_B);
                    RotateTGuard_FirstToLast(firstPt, lastPt2, root_1.position, _cur_seg_B, out newSegB, out _localRota_B);

                }
                else
                {

                    lastPt2 += dir_BtoA * dropping;
                    _dir_move_B = lastPt2 - close_B;
                    _meetPt_B = lastPt2;

                    newSegB = LineSegment3.Move(_cur_seg_B, _dir_move_B);

                }

                _cur_seg_A = newSegA;
                _cur_seg_B = newSegB;
                _prev_seg_A = newSegA;
                _prev_seg_B = newSegB;

                _close_B = close_B;
                _close_A = close_A;
                _meetPt = meetPt;
            }


            return result_contact;
        }

        //n_dir 은 노멀값이어야 함 
        public void CalcTGuard_First_ClosePt(bool draw, float penetration, Vector3 pt_close, Vector3 n_dir, Transform root_0,
                                                out Vector3 pt_center, out Vector3 pt_first)
        {
            const float ERROR_RATE = 1f; //20프로 정도 침투길이를 늘려 계산할려는 목적 

            Vector3 meetPt_A = pt_close + (n_dir * penetration);

            //===============
            //회전방향과 수직인 직선을 구한다. 루트위치를 지나가게 한다. 직선에서 pt_close 와 가장 가까운 위치를 중심으로 삼는다 
            //(T가드일때 의미있는 코드이다)
            Vector3 up_center = Vector3.Cross(root_0.rotation * ConstV.v3_forward, n_dir);
            pt_center = Line3.ClosestPoint(new Line3(root_0.position, up_center), pt_close);

            pt_center = root_0.position; //한계위치에서의 계산을 위해 임시로 처리 
            //===============
            //코사인 제2법칙 이용, a 와 c 는 길이가 같은점을 이용하여 식을 정리 
            //    0
            //  a   b
            // 0  c   0 --- cosa
            //
            // a2 = b2+c2-2bc*cosa
            // a2 - b2 - c2 = -2bc * cosa
            // (a2 - b2 - c2)/-2bc = cosa
            // a == c
            // b/2c = cosa
            float c = (pt_close - pt_center).magnitude;
            float b = penetration * ERROR_RATE; //오차율로 인해 ab선분의 반지름 합을 못 벗어나는 문제가 있음. 이를 제거함  
            float cos_a = b / (2 * c);
            float angle_a = Mathf.Acos(cos_a) * Mathf.Rad2Deg;

            Vector3 up = Vector3.Cross((pt_center - pt_close), (meetPt_A - pt_close));
            Vector3 dir_rot = Quaternion.AngleAxis(angle_a, up) * ((pt_center - pt_close).normalized * b);

            pt_first = pt_close + dir_rot;


            if (draw)
            {
                DebugWide.DrawLine(pt_center, pt_close, Color.yellow);
                DebugWide.DrawLine(meetPt_A, pt_close, Color.green);
                DebugWide.DrawLine(pt_first, pt_close, Color.black);
                DebugWide.LogBlue("  len:" + _cur_A_B_order.magnitude + "  p:" + penetration + "  ang:" + angle_a);
                DebugWide.LogBlue((pt_close - meetPt_A).magnitude + "  => " + (dir_rot).magnitude);
            }


        }




        int __count = 0;

        public Vector3 CalcTGuard_FirstPt2(Vector3 meetPt, Transform root_0,
                                           LineSegment3 t0_sub_prev)
        {

            float a = (meetPt - root_0.position).magnitude;

            //선분의 시작점이 원의 안쪽으로 들어가 접촉점 계산이 잘못되는 문제해결을 위해 선분연장 
            //한계검사를 통해 meetPt값이 조절되지 않으면 연장된 선분에 의해 교점검사 실패시 범위를 넘어가는 값이 나올 수 있다 
            LineSegment3 seg = t0_sub_prev;
            seg.last = seg.last + seg.direction;
            seg.origin = seg.origin + -seg.direction;

            //원과 접하는 두개의 교점을 찾느다 이중 meetPT와 가까운 교점이 찾으려는 점이다 , CalcTGuard_FirstPt 함수의 문제를 해결 
            Vector3 pt_first1, pt_first2;
            bool r1 = Geo.IntersectLineSegment(root_0.position, a, seg, out pt_first1);
            Vector3 temp = seg.origin;
            seg.origin = seg.last;
            seg.last = temp;
            bool r2 = Geo.IntersectLineSegment(root_0.position, a, seg, out pt_first2);

            float sqrlen1 = (pt_first1 - meetPt).sqrMagnitude;
            float sqrlen2 = (pt_first2 - meetPt).sqrMagnitude;
            bool r = r1;
            Vector3 closePt = pt_first1;
            if (sqrlen1 > sqrlen2)
            {
                closePt = pt_first2;
                r = r2;
            }

            //DebugWide.LogBlue(Mathf.Sqrt(sqrlen1) + " vs " + Mathf.Sqrt(sqrlen2) + "  ");
            //seg.Draw(Color.cyan);
            //DebugWide.DrawCircle(root_0.position, a, Color.cyan); //chamto test        
            //if(true == r)
            //    DebugWide.DrawCircle(closePt, 0.06f, Color.cyan); //chamto test        
            //else
            //DebugWide.DrawCircle(closePt, 0.06f, Color.yellow); //chamto test        

            return closePt;
        }

        public void RotateTGuard_FirstToLast(Vector3 meetPt_first, Vector3 meetPt_last,
                               Vector3 pos_t0_root,
                                LineSegment3 t0_sub_start,
                               out LineSegment3 new_t0_sub, out Quaternion localRot)
        {
            //---------------------------------------

            float angle_t = 0;
            localRot = Quaternion.identity;
            Vector3 up_t = Vector3.Cross(meetPt_first - pos_t0_root, meetPt_last - pos_t0_root);
            if (false == Misc.IsZero(up_t))
            {
                angle_t = Geo.AngleSigned(meetPt_first - pos_t0_root, meetPt_last - pos_t0_root, up_t);
                localRot = Quaternion.AngleAxis(angle_t, up_t);
                //DebugWide.LogBlue(meetPt_first + "   " + meetPt_last + " ---  "+ up_t + "  " + angle_t);
            }

            //DebugWide.LogBlue(" --  "+ up_t + "  " + angle_t);
            //--------

            new_t0_sub = t0_sub_start.Rotate(pos_t0_root, localRot);

            //---------------------------------------

        }



        //==================================================




        public bool __isSeg_A, __isSeg_B;
        //float __cpS, __cpT;
        //Vector3 __cpPt0;
        public Vector3 __dir_A = ConstV.v3_zero;
        public Vector3 __dir_B = ConstV.v3_zero;
        public Vector3 _prev_A_B_order = ConstV.v3_zero;
        public Vector3 _cur_A_B_order = ConstV.v3_zero;
        //public Vector3 __cur_A_B_order2 = ConstV.v3_zero;


        public void Input_TGuard(LineSegment3 prev_segA, LineSegment3 prev_segB, LineSegment3 cur_segA, LineSegment3 cur_segB)
        {

            _cur_seg_A = cur_segA;
            _cur_seg_B = cur_segB;
            _prev_seg_A = prev_segA;
            _prev_seg_B = prev_segB;
            //--------------------

            //선분의 이동방향
            __dir_A = (_cur_seg_A.origin - _prev_seg_A.origin) + (_cur_seg_A.last - _prev_seg_A.last);
            __dir_B = (_cur_seg_B.origin - _prev_seg_B.origin) + (_cur_seg_B.last - _prev_seg_B.last);
            //__dir_A = VOp.Normalize(__dir_A);
            //__dir_B = VOp.Normalize(__dir_B); //방향값을 노멀값으로 만들면 test값이 커져 선분이 아닌데 선분으로 판별되는 문제가 발생함


            //방향이 같으면 0벡터가 되는 외적의 성질을 이용해 선분상태를 구함 
            Vector3 test;
            test = Vector3.Cross(__dir_A, _cur_seg_A.direction);
            __isSeg_A = Misc.IsZero(test);
            test = Vector3.Cross(__dir_B, _cur_seg_B.direction);
            __isSeg_B = Misc.IsZero(test);

            //DebugWide.LogRed(__isSeg_A + "  " + __isSeg_B);

            //선분상태를 구한뒤 방향값을 노멀로 만든다 
            __dir_A = VOp.Normalize(__dir_A);
            __dir_B = VOp.Normalize(__dir_B);

            //선분상태를 유지한 채 이동한 경우 선분으로 안나옴
            //__isSeg_B = Misc.IsZero(__dir_B);
            //__isSeg_A = Misc.IsZero(__dir_A);


            _tetr01.Set(_prev_seg_A, _cur_seg_A);
            _tetr23.Set(_prev_seg_B, _cur_seg_B);

        }
    }//end class
}


