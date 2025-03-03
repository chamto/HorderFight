﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilGS9;

namespace UtilGS9
{
    public enum eIntersectionType
    {
        EMPTY,
        POINT,
        SEGMENT,
        RAY,
        LINE,
        POLYGON,
        PLANE,
        POLYHEDRON,
        OTHER
    }

    public struct IntrPlane
    {
        public Vector3 _normal;
        public float _offset; //원점으로 부터 평면이 떨어져 있는 거리 
        public bool _isPlane; //평면인지 아닌지 상태를 나타낸다 

        public IntrPlane(Vector3 p0, Vector3 p1, Vector3 p2)
        {

            Vector3 edge1 = p1 - p0;
            Vector3 edge2 = p2 - p0;
            _normal = Vector3.Cross(edge1, edge2);

            //평면인지 검사한다. 선분이나 점을 걸러내기 위함 
            _isPlane = true;
            const float EPSILON = 0.0000001f; //적당히 작은값을 지정한다. float.epsilon 은 값이 너무 작아서 걸러내지 못함 
            if (Misc.IsZero(_normal, EPSILON))
                _isPlane = false;

            _normal = UtilGS9.VOp.Normalize(_normal);

            _offset = Vector3.Dot(_normal, p0);

            //DebugWide.LogBlue("plan  " + _isPlane + "  "+ p0 + " " + p1 + " " + p2 + " " + Vector3.Cross(edge1, edge2).sqrMagnitude);

            //if (_normal.sqrMagnitude < float.Epsilon)
            //{
            //    DebugWide.LogRed("평면이 아님 !!!"); 
            //}
            //이 처리가 있으면 [seg0 vs tri1] 상황에서 교점을 찾지 못하는 문제가 발생한다. 애션셜수학 코드의 평면코드는 안쓰는것으로 함 
            //if (_normal.sqrMagnitude < float.Epsilon)
            //{   //평면의 방향을 계산 할 수 없을때 
            //    _normal = Vector3.up;
            //    _offset = 0.0f;
            //}
        }

        //점이 평면에 직각인 부호있는 거리
        public float Test(Vector3 p)
        {
            return Vector3.Dot(_normal, p) - _offset;
        }

        public override string ToString()
        {
            return " N:" + _normal + "   C:" + _offset;
        }
    }

    public struct Triangle2
    {
        public Vector2[] V; //[3]

        //static public Triangle2 zero = new Triangle2(); //배열할당을 안하고 사용하는 문제 생김

        static public Triangle2 Zero()
        {
            Triangle2 triangle2 = new Triangle2(ConstV.v2_zero, ConstV.v2_zero, ConstV.v2_zero);

            return triangle2;
        }

        public Triangle2(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            V = new Vector2[3];
            V[0] = v0; V[1] = v1; V[2] = v2;
        }

    }

    public struct Triangle3
    {

        public struct V012
        {
            public Vector3 V0, V1, V2;
            public Vector3 this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0: return V0;
                        case 1: return V1;
                        case 2: return V2;
                    }

                    DebugWide.LogError("get 범위를 벗어나는 인덱스 " + index);
                    return ConstV.v3_zero;
                }
                set
                {
                    switch (index)
                    {
                        case 0: V0 = value; return;
                        case 1: V1 = value; return;
                        case 2: V2 = value; return;
                    }
                    DebugWide.LogError("set 범위를 벗어나는 인덱스 " + index);
                }
            }
        }
        //public V012 V;
        public Vector3[] V; //[3] //구조체의 배열 복사시 얇은복사를 하여 주소가 공유되는 문제 생김

        //static public Triangle3 zero = new Triangle3(); //배열할당을 안하고 사용하는 문제 생김

        static public Triangle3 Zero()
        {
            Triangle3 triangle3 = new Triangle3(ConstV.v3_zero, ConstV.v3_zero, ConstV.v3_zero);

            return triangle3;
        }

        public Triangle3(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            V = new Vector3[3];
            V[0] = v0; V[1] = v1; V[2] = v2;

            //V = new V012();
            //V[0] = v0; V[1] = v1; V[2] = v2;
        }

        override public string ToString()
        {
            return V[0] + "   " + V[1] + "   " + V[2];
        }
    }

    public struct Segment2
    {
        public Vector2 P0, P1;

        // Center-direction-extent representation.
        public Vector2 Center;
        public Vector2 Direction;
        public float Extent; //크기 : 선분길이의 반 

        public Segment2(Vector2 p0, Vector2 p1)
        {
            P0 = p0;
            P1 = p1;

            //ComputeCenterDirectionExtent();
            Center = 0.5f * (P0 + P1);
            Direction = P1 - P0;
            Extent = 0.5f * Direction.magnitude;
            Direction = VOp.Normalize(Direction);
        }

        void ComputeCenterDirectionExtent()
        {
            Center = 0.5f * (P0 + P1);
            Direction = P1 - P0;
            Extent = 0.5f * Direction.magnitude;
            Direction = VOp.Normalize(Direction);
        }
    }

    public struct Intersector1
    {
        // The intervals to intersect.
        public float[] mU; //[2]
        public float[] mV; //[2]

        // Information about the intersection set.
        public float mFirstTime, mLastTime;
        public int mNumIntersections;
        public float[] mIntersections; //[2]

        public Intersector1(float u0, float u1, float v0, float v1)
        {
            //assertion(u0 <= u1 && v0 <= v1, "Malformed interval\n");
            mU = new float[2];
            mV = new float[2];

            mU[0] = u0;
            mU[1] = u1;
            mV[0] = v0;
            mV[1] = v1;
            mFirstTime = 0f;
            mLastTime = 0f;
            mNumIntersections = 0;

            mIntersections = new float[2];
        }

        public bool Find()
        {
            if (mU[1] < mV[0] || mU[0] > mV[1])
            {
                mNumIntersections = 0;
            }
            else if (mU[1] > mV[0])
            {
                if (mU[0] < mV[1])
                {
                    mNumIntersections = 2;
                    mIntersections[0] = (mU[0] < mV[0] ? mV[0] : mU[0]);
                    mIntersections[1] = (mU[1] > mV[1] ? mV[1] : mU[1]);
                    if (mIntersections[0] == mIntersections[1])
                    {
                        mNumIntersections = 1;
                    }
                }
                else  // mU[0] == mV[1]
                {
                    mNumIntersections = 1;
                    mIntersections[0] = mU[0];
                }
            }
            else  // mU[1] == mV[0]
            {
                mNumIntersections = 1;
                mIntersections[0] = mU[1];
            }

            return mNumIntersections > 0;
        }

    }

    public struct IntrSegment2Triangle2
    {
        public eIntersectionType mIntersectionType;

        // The objects to intersect.
        public Segment2 mSegment;
        public Triangle2 mTriangle;

        // Information about the intersection set.
        public int mQuantity;
        public Vector2[] mPoint; //[2]

        public IntrSegment2Triangle2(Segment2 segment, Triangle2 tri)
        {
            mSegment = segment;
            mTriangle = tri;

            mQuantity = 0;
            mPoint = new Vector2[2]
            { ConstV.v2_zero, ConstV.v2_zero };

            mIntersectionType = eIntersectionType.EMPTY;
        }

        public bool Find()
        {
            float[] dist = new float[3];
            int[] sign = new int[3];
            int positive, negative, zero;
            TriangleLineRelations(mSegment.Center, mSegment.Direction, mTriangle,
                                  ref dist, ref sign, out positive, out negative, out zero);
            //DebugWide.LogBlue(zero); //chamto test
            if (positive == 3 || negative == 3)
            {
                // No intersections.
                mQuantity = 0;
                mIntersectionType = eIntersectionType.EMPTY;
            }
            else
            {
                float[] param = new float[2];
                GetInterval(mSegment.Center, mSegment.Direction, mTriangle, dist, sign, ref param);

                Intersector1 intr = new Intersector1(
                    param[0], param[1], -mSegment.Extent, +mSegment.Extent);

                intr.Find();

                mQuantity = intr.mNumIntersections;
                if (mQuantity == 2)
                {
                    // Segment intersection.
                    mIntersectionType = eIntersectionType.SEGMENT;
                    mPoint[0] = mSegment.Center +
                        intr.mIntersections[0] * mSegment.Direction;
                    mPoint[1] = mSegment.Center +
                        intr.mIntersections[1] * mSegment.Direction;
                }
                else if (mQuantity == 1)
                {
                    // Point intersection.
                    mIntersectionType = eIntersectionType.POINT;
                    mPoint[0] = mSegment.Center +
                        intr.mIntersections[0] * mSegment.Direction;
                }
                else
                {
                    // No intersections.
                    mIntersectionType = eIntersectionType.EMPTY;
                }
            }

            return mIntersectionType != eIntersectionType.EMPTY;
        }//end func

        //dist[3] , sign[3]
        void TriangleLineRelations(Vector2 origin, Vector2 direction, Triangle2 triangle,
                                   ref float[] dist, ref int[] sign,
                                   out int positive, out int negative, out int zero)
        {
            positive = 0;
            negative = 0;
            zero = 0;
            for (int i = 0; i < 3; ++i)
            {
                //!!! direction 가 0벡터인 경우의 예외처리가 없음. 즉 직선이 아닌 점으로 들어온 경우임 
                //!!! 0벡터로 인해 GetInterval 함수에서 에러가 발생한다 
                Vector2 diff = triangle.V[i] - origin;
                dist[i] = VOp.PerpDot(diff, direction); //수직내적 : |a||b|sin@ , a와b벡터가 방향이 같으면 0 
                                                                    //DebugWide.LogGreen(dist[i] + "   " + diff + "   " + direction); //chamto test
                                                                    //직선의 방향으로 두개의 공간으로 나눈다
                                                                    // 윗공간은 sin@ 양수공간 , 아랫공간은 sin@ 음수공간
                                                                    // 직선이 삼각형의 변을 지나간다면 양수공간과 음수공간을 나누어서 지나가게 된다 
                if (dist[i] > float.Epsilon)
                {   //직선의 윗방향
                    sign[i] = 1;
                    ++positive;
                }
                else if (dist[i] < -float.Epsilon)
                {   //직선의 아랫방향
                    sign[i] = -1;
                    ++negative;
                }
                else
                {   //직선이 삼각형의 변과 겹쳐 지남
                    dist[i] = 0f;
                    sign[i] = 0;
                    ++zero;
                }
            }
        }//end func

        //dist[3] , sign[3] , param[2]
        void GetInterval(Vector2 origin, Vector2 direction, Triangle2 triangle,
            float[] dist, int[] sign, ref float[] param)
        {

            //DebugWide.LogGreen("seg-   ori:"+origin + "   dir:" + direction); //chamto test

            // Project triangle onto line.
            float[] proj = new float[3];
            int i;
            for (i = 0; i < 3; ++i)
            {
                Vector2 diff = triangle.V[i] - origin;
                proj[i] = Vector2.Dot(direction, diff);
            }

            // Compute transverse intersections of triangle edges with line.
            float numer, denom;
            int i0, i1, i2;
            int quantity = 0;
            for (i0 = 2, i1 = 0; i1 < 3; i0 = i1++)
            {
                //DebugWide.LogBlue(i0 + "   " + i1); //chamto test
                //2  0
                //0  1
                //1  2
                //두 부호의 곱이 음수라면 직선이 선분을 지난다 
                if (sign[i0] * sign[i1] < 0)
                {
                    //assertion(quantity< 2, "Too many intersections\n");
                    //분석인 안되는 부분 .. - chamto  
                    numer = dist[i0] * proj[i1] - dist[i1] * proj[i0];
                    denom = dist[i0] - dist[i1];
                    //DebugWide.LogBlue(" !!!   d0:"+ dist[i0] + "   p0:" + proj[i0] + "    d1:" + dist[i1] + "   p1:" + proj[i1]); //chamto test
                    //DebugWide.LogBlue("  n:"+numer + "   d:" + denom + "   n/d:" + numer / denom);
                    param[quantity++] = numer / denom;
                }
            }
            // - 1단계 : 두가지 상황을 가정
            //  1.세개의 sign 값이 같은 부호를 가짐 : 직선이 삼각형을 지나지 않음 q = 0
            //  2.두개의 sign 이 같은 부호이고 다른 한개의 sign 은 다른 부호를 가짐 : 직선이 두개의 선분을 지남 q = 2


            //직선이 0개 또는 한개의 선분을 지나는 경우 
            // - 2단계 : 세가지 상황을 가정 
            //  1.두개의 sign 0값을 가지고, 하나의 부호있는 sign 값을 가짐 : 직선이 삼각형의 선분과 완전 겹침 q = 2
            //  2.한개의 sign 0값을 가지고, 두개의 서로다른 부호의 sign 값을 가짐 : 직선이 한개의 선분과 정점을 통과함 q = 2
            //  3.한개의 sign 0값을 가지고, 두개의 같은 부호의 sign 값을 가짐 : 직선이 한개의 정점만 지남 q = 1
            // Check for grazing contact.
            if (quantity < 2)
            {
                //for (i0 = 1, i1 = 2, i2 = 0; i2< 3; i0 = i1, i1 = i2++)
                for (i2 = 0; i2 < 3; i2++)
                {
                    if (sign[i2] == 0)
                    {   //삼각형의 정점과 직선이 겹치는 경우  
                        if (2 == quantity)
                        {
                            DebugWide.LogWarning("dir : " + direction + "  quantity: " + quantity + "   삼각형 형태가 아님!! ");
                            return;
                        }
                        //DebugWide.LogBlue(quantity);; //chamto test
                        //assertion(quantity< 2, "Too many intersections\n");
                        param[quantity++] = proj[i2];
                    }
                }
            }

            //구한값이 2개인 경우 정렬을 한다
            // Sort.
            //assertion(quantity >= 1, "Need at least one intersection\n");
            if (quantity == 2)
            {
                if (param[0] > param[1])
                {
                    float save = param[0];
                    param[0] = param[1];
                    param[1] = save;
                }
            }
            else
            {   //같은 값으로 채움 
                param[1] = param[0];
            }
        }//end func

    }

    public struct IntrTriangle2Triangle2
    {

        public Triangle2 mTriangle0;
        public Triangle2 mTriangle1;


        public int mQuantity;
        public Vector2[] mPoint; //[6]

        public IntrTriangle2Triangle2(Triangle2 t0, Triangle2 t1)
        {
            mTriangle0 = t0;
            mTriangle1 = t1;

            mQuantity = 0;
            mPoint = new Vector2[6];

            for (int i = 0; i < 6; i++)
            {
                mPoint[i] = UtilGS9.ConstV.v3_zero;
            }
        }

        public bool Find()
        {
            // The potential intersection is initialized to triangle1.  The set of
            // vertices is refined based on clipping against each edge of triangle0.
            mQuantity = 3;
            for (int i = 0; i < 3; ++i)
            {
                mPoint[i] = mTriangle1.V[i];
            }

            for (int i1 = 2, i0 = 0; i0 < 3; i1 = i0++)
            {
                // Clip against edge <V0[i1],V0[i0]>.
                Vector2 N = new Vector2(
                    mTriangle0.V[i1].y - mTriangle0.V[i0].y,
                    mTriangle0.V[i0].x - mTriangle0.V[i1].x);
                float c = Vector2.Dot(N, mTriangle0.V[i1]);
                ClipConvexPolygonAgainstLine(N, c, ref mQuantity, ref mPoint);
                if (mQuantity == 0)
                {
                    // Triangle completely clipped, no intersection occurs.
                    return false;
                }
            }

            return true;
        }//end func

        void ClipConvexPolygonAgainstLine(Vector2 N, float c, ref int quantity, ref Vector2[] V) //[6]
        {
            // The input vertices are assumed to be in counterclockwise order.  The
            // ordering is an invariant of this function.

            // Test on which side of line the vertices are.
            int positive = 0, negative = 0, pIndex = -1;
            float[] test = new float[6];
            int i;
            for (i = 0; i < quantity; ++i)
            {
                test[i] = Vector2.Dot(N, V[i]) - c;
                if (test[i] > 0f)
                {
                    positive++;
                    if (pIndex < 0)
                    {
                        pIndex = i;
                    }
                }
                else if (test[i] < 0f)
                {
                    negative++;
                }
            }

            if (positive > 0)
            {
                if (negative > 0)
                {
                    // Line transversely intersects polygon.
                    Vector2[] CV = new Vector2[6];
                    int cQuantity = 0, cur, prv;
                    float t;

                    if (pIndex > 0)
                    {
                        // First clip vertex on line.
                        cur = pIndex;
                        prv = cur - 1;
                        t = test[cur] / (test[cur] - test[prv]);
                        CV[cQuantity++] = V[cur] + t * (V[prv] - V[cur]);

                        // Vertices on positive side of line.
                        while (cur < quantity && test[cur] > 0f)
                        {
                            CV[cQuantity++] = V[cur++];
                        }

                        // Last clip vertex on line.
                        if (cur < quantity)
                        {
                            prv = cur - 1;
                        }
                        else
                        {
                            cur = 0;
                            prv = quantity - 1;
                        }
                        t = test[cur] / (test[cur] - test[prv]);
                        CV[cQuantity++] = V[cur] + t * (V[prv] - V[cur]);
                    }
                    else  // pIndex is 0
                    {
                        // Vertices on positive side of line.
                        cur = 0;
                        while (cur < quantity && test[cur] > 0f)
                        {
                            CV[cQuantity++] = V[cur++];
                        }

                        // Last clip vertex on line.
                        prv = cur - 1;
                        t = test[cur] / (test[cur] - test[prv]);
                        CV[cQuantity++] = V[cur] + t * (V[prv] - V[cur]);

                        // Skip vertices on negative side.
                        while (cur < quantity && test[cur] <= 0f)
                        {
                            ++cur;
                        }

                        // First clip vertex on line.
                        if (cur < quantity)
                        {
                            prv = cur - 1;
                            t = test[cur] / (test[cur] - test[prv]);
                            CV[cQuantity++] = V[cur] + t * (V[prv] - V[cur]);

                            // Vertices on positive side of line.
                            while (cur < quantity && test[cur] > 0f)
                            {
                                CV[cQuantity++] = V[cur++];
                            }
                        }
                        else
                        {
                            // cur = 0
                            prv = quantity - 1;
                            t = test[0] / (test[0] - test[prv]);
                            CV[cQuantity++] = V[0] + t * (V[prv] - V[0]);
                        }
                    }

                    quantity = cQuantity;
                    //memcpy(V, CV, cQuantity*sizeof(Vector2<Real>));
                    CV.CopyTo(V, 0); //CV를 V에 복사 , V의 0인덱스 위치부터 복사한다 
                }
                // else polygon fully on positive side of line, nothing to do.
            }
            else
            {
                // Polygon does not intersect positive side of line, clip all.
                quantity = 0;
            }
        }//end func
    }//end IntrTri2_Tri2


    //======================================================

    public struct IntrTriangle3Triangle3
    {

        public Triangle3 mTriangle0;
        public Triangle3 mTriangle1;

        // Information about the intersection set.
        public int mQuantity;
        public Vector3[] mPoint; //[6]

        public bool mReportCoplanarIntersections;  // default 'true'

        public eIntersectionType mIntersectionType;

        public override string ToString()
        {
            string temp =  "";
            for(int i=0;i<6;i++)
            {
                temp += mPoint[i] + "  ";
            }
            return  mIntersectionType.ToString() + " q: " + mQuantity + "   pt: " + temp;
        }

        //--------------------------------------------------

        public IntrTriangle3Triangle3(Triangle3 t0, Triangle3 t1)
        {
            mTriangle0 = t0;
            mTriangle1 = t1;

            mQuantity = 0;
            mPoint = new Vector3[6];

            for (int i = 0; i < 6; i++)
            {
                mPoint[i] = UtilGS9.ConstV.v3_zero;
            }

            mReportCoplanarIntersections = true;
            mIntersectionType = eIntersectionType.EMPTY;

        }

        // Input W must be a unit-length vector.  The output vectors {U,V} are
        // unit length and mutually perpendicular, and {U,V,W} is an orthonormal
        // basis.
        void GenerateComplementBasis(out Vector3 u, out Vector3 v, Vector3 w)
        {
            float invLength;

            if (Mathf.Abs(w.x) >= Mathf.Abs(w.y))
            {
                // W.x or W.z is the largest magnitude component, swap them
                invLength = 1f / (float)Math.Sqrt(w.x * w.x + w.y * w.y);
                u.x = -w.z * invLength;
                u.y = 0f;
                u.z = +w.x * invLength;

                v.x = w.y * u.z;
                v.y = w.z * u.x - w.x * u.z;
                v.z = -w.y * u.x;
            }
            else
            {
                // W.y or W.z is the largest magnitude component, swap them
                invLength = 1f / (float)Math.Sqrt(w.y * w.y + w.z * w.z);
                u.x = 0f;
                u.y = +w.z * invLength;
                u.z = -w.y * invLength;

                v.x = w.y * u.z - w.z * u.y;
                v.y = -w.x * u.z;
                v.z = w.x * u.y;
            }
        }



        //=======================================
        public void Draw(Color color)
        {
            switch (mIntersectionType)
            {
                case eIntersectionType.POINT:
                    {
                        DebugWide.DrawCircle(mPoint[0], 0.01f, color);
                    }
                    break;
                case eIntersectionType.SEGMENT:
                    {
                        DebugWide.DrawCircle(mPoint[0], 0.01f, color);
                        DebugWide.DrawCircle(mPoint[1], 0.01f, color);
                        DebugWide.DrawLine(mPoint[0], mPoint[1], color);

                    }
                    break;
                case eIntersectionType.PLANE:
                    {
                        for (int i = 0; i < mQuantity; i++)
                        {
                            int i2 = i + 1;
                            i2 = (i2 == mQuantity ? 0 : i2);
                            DebugWide.DrawLine(mPoint[i], mPoint[i2], color);
                        }
                    }
                    break;
            }
        }

        //교차 검사를 두번하여 정확한 값을 찾는다 
        public bool Find_Twice()
        {
            bool result = Find(mTriangle0, mTriangle1);
            if (false == result)
                return Find(mTriangle1, mTriangle0);

            return result;
        }

        //삼각형 형태가 아닌 것(선분,점)은 정상처리가 안된다
        //삼각형 형태만 처리 할 수 있다 
        public bool Find(Triangle3 tri0, Triangle3 tri1)
        {
            //----------
            //검사전 초기화 
            mQuantity = 0;
            mIntersectionType = eIntersectionType.EMPTY;
            for (int a = 0; a < 6; a++) mPoint[a] = ConstV.v3_zero;
            //----------

            int i, iM, iP;

            // Get the plane of triangle0.
            IntrPlane plane0 = new IntrPlane(tri0.V[0], tri0.V[1], tri0.V[2]);

            if (false == plane0._isPlane)
            {
                //tri0이 삼각형이 아닌 선분이나,점으로 들어오면 정상적인 처리를 하지 못한다 
                //선분이나 점은 처리하지 않는다 
                return false; 
            }
            // Compute the signed distances of triangle1 vertices to plane0.  Use
            // an epsilon-thick plane test.
            int pos1, neg1, zero1;
            int[] _sign1 = new int[3];
            float[] _dist1 = new float[3];

            TrianglePlaneRelations(tri1, plane0, ref _dist1, ref _sign1, out pos1, out neg1, out zero1);

            //Plane3 plane1 = new Plane3(tri1.V[0], tri1.V[1], tri1.V[2]);
            //int pos2, neg2, zero2;
            //int[] sign2 = new int[3];
            //float[] dist2 = new float[3];
            //TrianglePlaneRelations(tri0, plane1, ref dist2, ref sign2, out pos2, out neg2, out zero2);
            //DebugWide.LogGreen("  po1:" + pos1 + "  ne:" + neg1 + "   ze:" + zero1 + "  s0:" + sign1[0] + "  s1:" + sign1[1] + "  s2:" + sign1[2] ); //chamto test
            //DebugWide.LogBlue("  po2:" + pos2 + "  ne:" + neg2 + "   ze:" + zero2 + "  s0:" + sign2[0] + "  s1:" + sign2[1] + "  s2:" + sign2[2] ); //chamto test

            // 삼각형의 정점이 모두 평면위쪽에 있거나 평면아래쪽에 있는 경우 
            if (pos1 == 3 || neg1 == 3)
            {
                // Triangle1 is fully on one side of plane0.
                return false;
            }

            // 삼각형의 정점이 모두 평면에 있는 경우 
            if (zero1 == 3)
            {
                // Triangle1 is contained by plane0.
                if (mReportCoplanarIntersections)
                {
                    //DebugWide.LogBlue("0_0---  "); //chamto test

                    //[seg0 vs point1] 로 검사시 교차하지 않는데도  point1 위치를 교차점으로 결과가 나오는 문제있음 
                    //비삼각형 모양중 점은 아주 특수하므로 잘못된 결과를 무시한다 
                    if (Misc.IsZero(tri1.V[0] - tri1.V[1]) && Misc.IsZero(tri1.V[0] - tri1.V[2]))
                        return ContainsPoint(tri0, tri1.V[0]);

                    return GetCoplanarIntersection(plane0, tri0, tri1);
                }
                return false;
            }


            // 평면이 가르는 한쪽영역에 삼각형이 위치하고 그 정점중 하나나 둘이 평면에 있는 모양 
            // Check for grazing contact between triangle1 and plane0.
            if (pos1 == 0 || neg1 == 0)
            {
                if (zero1 == 2)
                {   //정점 두개가 평면에 있음
                    // An edge of triangle1 is in plane0.
                    for (i = 0; i < 3; ++i)
                    {
                        if (_sign1[i] != 0)
                        {
                            //0
                            //1 2
                            //i(0) => mp(2,1)
                            //i(1) => mp(0,2)
                            //i(2) => mp(1,0)
                            iM = (i + 2) % 3;
                            iP = (i + 1) % 3;

                            //DebugWide.LogBlue("2---"); //chamto test

                            if (Misc.IsZero(tri1.V[iM] - tri1.V[iP]))
                                return ContainsPoint(tri0, tri1.V[iM]);
                            

                            return IntersectsSegment(plane0, tri0, tri1.V[iM], tri1.V[iP]);
                        }
                    }
                }
                else if (zero1 == 1)// zero1 == 1
                {   //정점 한개가 평면에 있음 
                    // A vertex of triangle1 is in plane0.
                    for (i = 0; i < 3; ++i)
                    {
                        if (_sign1[i] == 0)
                        {
                            //DebugWide.LogGreen("1_1---  tr0:" + tri0 +  "  tr1:" + tri1 + "  pl0:" + plane0); //chamto test

                            return ContainsPoint(tri0, tri1.V[i]);
                        }
                    }
                }
            }

            // At this point, triangle1 tranversely intersects plane 0.  Compute the
            // line segment of intersection.  Then test for intersection between this
            // segment and triangle 0.
            float t;
            Vector3 intr0, intr1;
            // 삼각형의 정점이 평면상에 하나도 없는 경우 
            if (zero1 == 0)
            {
                //삼각형이 평면에 수직으로 겹쳐진 모양을 가정
                // 두가지의 모양이 있다고 가정
                // 삼각형의 점정이 평면위쪽에 한개 나온 모양[+1] , 평면아래쪽에 한개 나온 모양[-1]
                int iSign = (pos1 == 1 ? +1 : -1);
                for (i = 0; i < 3; ++i)
                {
                    if (_sign1[i] == iSign)
                    {
                        //i(0) => mp(2,1)
                        //i(1) => mp(0,2)
                        //i(2) => mp(1,0)
                        iM = (i + 2) % 3;
                        iP = (i + 1) % 3;

                        //    di ---- vi
                        //       | -
                        //       |-
                        // ------------- plan
                        //      -|
                        //     - |
                        //viM ---- dim
                        //삼각형의 닮음을 이용하여 t를 구함 
                        t = _dist1[i] / (_dist1[i] - _dist1[iM]); //d[im] 은 평면의 아래쪽이므로 음수다. 이는 d[i]와의 합을 의미 
                        intr0 = tri1.V[i] + t * (tri1.V[iM] - tri1.V[i]);
                        t = _dist1[i] / (_dist1[i] - _dist1[iP]);
                        intr1 = tri1.V[i] + t * (tri1.V[iP] - tri1.V[i]);

                        //DebugWide.DrawCircle(intr0,0.05f,Color.green);
                        //DebugWide.LogBlue("3---  di:" + _dist1[i] + "   dm:" + _dist1[iM] +  "   dp:" + _dist1[iP] + "   intr0:" + intr0 + "    intr1:" + intr1); //chamto test
                        //DebugWide.AddDrawQ_Circle(intr0, 0.01f, Color.red);
                        //DebugWide.AddDrawQ_Circle(intr1, 0.01f, Color.red);
                        //intr0 과 intr1 이 같다면 점과 교차하는 처리를 한다 
                        if (Misc.IsZero(intr0 - intr1))
                        {
                            //DebugWide.LogBlue("3_1---");
                            //return ContainsPoint(tri0, intr0);

                            bool result = ContainsPoint(tri0, intr0);
                            //DebugWide.LogBlue("3---  r:"+ result + "  di:" + _dist1[i] + "   dm:" + _dist1[iM] + "   dp:" + _dist1[iP] + "   intr0:" + intr0 + "    intr1:" + intr1); //chamto test
                            return result;
                        }


                        return IntersectsSegment(plane0, tri0, intr0, intr1);
                    }
                }
                return false;
            }

            // 삼각형의 정점이 평면상에 하나 있는 경우 
            if (zero1 == 1)
            {
                for (i = 0; i < 3; ++i)
                {
                    //삼각형의 정점중 하나가 평면에 접해있고 
                    // 평면의 위쪽에 하나, 평면의 아래쪽에 하나의 정점이 있는 모양 
                    if (_sign1[i] == 0)
                    {   //평면과 접점이라면 

                        iM = (i + 2) % 3;
                        iP = (i + 1) % 3;
                        t = _dist1[iM] / (_dist1[iM] - _dist1[iP]);
                        //if (Misc.IsZero((_dist1[iM] - _dist1[iP])))
                            //t = 0;
                        intr0 = tri1.V[iM] + t * (tri1.V[iP] - tri1.V[iM]);

                        DebugWide.LogBlue("4---"); //chamto test
                        //DebugWide.LogBlue("4---" + tri1.V[i] + "  " + intr0);
                        return IntersectsSegment(plane0, tri0, tri1.V[i], intr0);
                    }
                }
            }

            //삼각형0 의 모양이 선분모양인 경우, 여기까지 올 수 있다 
            return false;

        }//end func

        //distance[3] , sign[3]
        void TrianglePlaneRelations(Triangle3 triangle, IntrPlane plane,
                                    ref float[] distance, ref int[] sign, out int positive, out int negative, out int zero)
        {
            // Compute the signed distances of triangle vertices to the plane.  Use
            // an epsilon-thick plane test.
            positive = 0;
            negative = 0;
            zero = 0;
            for (int i = 0; i < 3; ++i)
            {
                distance[i] = plane.Test(triangle.V[i]); //평면에서 점까지의 최소거리 (평면과 직각)
                                                               //DebugWide.LogBlue("  n:" + plane.Normal + "  tri1:" + triangle.V[i] + "   d:" + distance[i] + "  i:" + i); //chamto test

                if (distance[i] > float.Epsilon)
                {   //평면위쪽
                    sign[i] = 1;
                    positive++;
                }
                else if (distance[i] < -float.Epsilon)
                {   //평면아래쪽
                    sign[i] = -1;
                    negative++;
                }
                else
                {   //평면에 위치 
                    distance[i] = 0f;
                    sign[i] = 0;
                    zero++;
                }
            }
        }//end func


        bool GetCoplanarIntersection(IntrPlane plane, Triangle3 tri0, Triangle3 tri1)
        {
            //*
            // Project triangles onto coordinate plane most aligned with plane
            // normal.
            int maxNormal = 0;
            float fmax = Mathf.Abs(plane._normal.x);
            float absMax = Mathf.Abs(plane._normal.y);
            if (absMax > fmax)
            {
                maxNormal = 1;
                fmax = absMax;
            }
            absMax = Mathf.Abs(plane._normal.z);
            if (absMax > fmax)
            {
                maxNormal = 2;
            }

            Triangle2 projTri0 = Triangle2.Zero(), projTri1 = Triangle2.Zero();

            int i;

            if (maxNormal == 0)
            {
                // Project onto yz-plane. x축에서 최대값
                for (i = 0; i < 3; ++i)
                {
                    projTri0.V[i].x = tri0.V[i].y;
                    projTri0.V[i].y = tri0.V[i].z;
                    projTri1.V[i].x = tri1.V[i].y;
                    projTri1.V[i].y = tri1.V[i].z;
                }
            }
            else if (maxNormal == 1)
            {
                // Project onto xz-plane. y축에서 최대값 
                for (i = 0; i < 3; ++i)
                {
                    projTri0.V[i].x = tri0.V[i].x;
                    projTri0.V[i].y = tri0.V[i].z;
                    projTri1.V[i].x = tri1.V[i].x;
                    projTri1.V[i].y = tri1.V[i].z;
                }
            }
            else 
            {
                // Project onto xy-plane. z축에서 최대값 
                for (i = 0; i < 3; ++i)
                {
                    projTri0.V[i].x = tri0.V[i].x;
                    projTri0.V[i].y = tri0.V[i].y;
                    projTri1.V[i].x = tri1.V[i].x;
                    projTri1.V[i].y = tri1.V[i].y;
                }
            }

            // 2D triangle intersection routines require counterclockwise ordering.
            Vector2 save;
            Vector2 edge0 = projTri0.V[1] - projTri0.V[0];
            Vector2 edge1 = projTri0.V[2] - projTri0.V[0];
            //if (edge0.DotPerp(edge1) < (Real)0)
            if (VOp.PerpDot(edge0, edge1) < 0f)
            {
                // Triangle is clockwise, reorder it.
                save = projTri0.V[1];
                projTri0.V[1] = projTri0.V[2];
                projTri0.V[2] = save;
            }

            edge0 = projTri1.V[1] - projTri1.V[0];
            edge1 = projTri1.V[2] - projTri1.V[0];
            //if (edge0.DotPerp(edge1) < (Real)0)
            if (VOp.PerpDot(edge0, edge1) < 0f)
            {
                // Triangle is clockwise, reorder it.
                save = projTri1.V[1];
                projTri1.V[1] = projTri1.V[2];
                projTri1.V[2] = save;
            }

            IntrTriangle2Triangle2 intr = new IntrTriangle2Triangle2(projTri0, projTri1);
            if (!intr.Find())
            {
                return false;
            }

            // Map 2D intersections back to the 3D triangle space.
            mQuantity = intr.mQuantity;
            if (maxNormal == 0)
            {
                float invNX = 1f / plane._normal.x;
                for (i = 0; i < mQuantity; i++)
                {
                    mPoint[i].y = intr.mPoint[i].x;
                    mPoint[i].z = intr.mPoint[i].y;
                    mPoint[i].x = invNX * (plane._offset -
                                    plane._normal.y * mPoint[i].y -
                                           plane._normal.z * mPoint[i].z);
                }
            }
            else if (maxNormal == 1)
            {
                float invNY = 1f / plane._normal.y;
                for (i = 0; i < mQuantity; i++)
                {
                    mPoint[i].x = intr.mPoint[i].x;
                    mPoint[i].z = intr.mPoint[i].y;
                    mPoint[i].y = invNY * (plane._offset -
                                    plane._normal.x * mPoint[i].x -
                                           plane._normal.z * mPoint[i].z);
                }
            }
            else
            {
                float invNZ = 1f / plane._normal.z;
                for (i = 0; i < mQuantity; i++)
                {
                    mPoint[i].x = intr.mPoint[i].x;
                    mPoint[i].y = intr.mPoint[i].y;
                    mPoint[i].z = invNZ * (plane._offset -
                                    plane._normal.x * mPoint[i].x -
                                           plane._normal.y * mPoint[i].y);
                }
            }

            mIntersectionType = eIntersectionType.PLANE;
            //*/
            return true;
        }//end func


        bool IntersectsSegment(IntrPlane plane, Triangle3 triangle, Vector3 end0, Vector3 end1)
        {
            // Compute the 2D representations of the triangle vertices and the
            // segment endpoints relative to the plane of the triangle.  Then
            // compute the intersection in the 2D space.

            // Project the triangle and segment onto the coordinate plane most
            // aligned with the plane normal.
            int maxNormal = 0; //노멀방향 x축
            float fmax = Mathf.Abs(plane._normal.x);
            float absMax = Mathf.Abs(plane._normal.y);
            if (absMax > fmax)
            {
                maxNormal = 1; //노멀방향 y축 
                fmax = absMax;
            }
            absMax = Mathf.Abs(plane._normal.z);
            if (absMax > fmax)
            {
                maxNormal = 2; //노멀방향 z축
            }

            Triangle2 projTri = Triangle2.Zero();
            Vector2 projEnd0 = ConstV.v2_zero, projEnd1 = ConstV.v2_zero;
            int i;

            if (maxNormal == 0)
            {
                // Project onto yz-plane.
                for (i = 0; i < 3; ++i)
                {
                    projTri.V[i].x = triangle.V[i].y;
                    projTri.V[i].y = triangle.V[i].z;
                    projEnd0.x = end0.y;
                    projEnd0.y = end0.z;
                    projEnd1.x = end1.y;
                    projEnd1.y = end1.z;
                }
            }
            else if (maxNormal == 1)
            {
                // Project onto xz-plane.
                for (i = 0; i < 3; ++i)
                {
                    projTri.V[i].x = triangle.V[i].x;
                    projTri.V[i].y = triangle.V[i].z;
                    projEnd0.x = end0.x;
                    projEnd0.y = end0.z;
                    projEnd1.x = end1.x;
                    projEnd1.y = end1.z;
                }
            }
            else
            {
                // Project onto xy-plane.
                for (i = 0; i < 3; ++i)
                {
                    projTri.V[i].x = triangle.V[i].x;
                    projTri.V[i].y = triangle.V[i].y;
                    projEnd0.x = end0.x;
                    projEnd0.y = end0.y;
                    projEnd1.x = end1.x;
                    projEnd1.y = end1.y;
                }
            }
            //DebugWide.LogBlue(end0 + "  " + end1); //chamto test

            Segment2 projSeg = new Segment2(projEnd0, projEnd1);
            //DebugWide.LogBlue(projEnd0 + "  " + projEnd1); //chamto test
            IntrSegment2Triangle2 calc = new IntrSegment2Triangle2(projSeg, projTri);
            if (!calc.Find())
            {
                return false;
            }

            Vector2[] intr = new Vector2[2];
            if (calc.mIntersectionType == eIntersectionType.SEGMENT)
            {
                mIntersectionType = eIntersectionType.SEGMENT;
                mQuantity = 2;
                intr[0] = calc.mPoint[0];
                intr[1] = calc.mPoint[1];
            }
            else
            {
                //assertion(calc.GetIntersectionType() == IT_POINT, "Intersection must be a point\n");
                mIntersectionType = eIntersectionType.POINT;
                mQuantity = 1;
                intr[0] = calc.mPoint[0];
            }

            // Unproject the segment of intersection.
            if (maxNormal == 0)
            {
                float invNX = 1f / plane._normal.x;
                for (i = 0; i < mQuantity; ++i)
                {
                    mPoint[i].y = intr[i].x;
                    mPoint[i].z = intr[i].y;
                    mPoint[i].x = invNX * (plane._offset -
                        plane._normal.y * mPoint[i].y -
                        plane._normal.z * mPoint[i].z);
                }
            }
            else if (maxNormal == 1)
            {
                float invNY = 1f / plane._normal.y;
                for (i = 0; i < mQuantity; ++i)
                {
                    mPoint[i].x = intr[i].x;
                    mPoint[i].z = intr[i].y;
                    mPoint[i].y = invNY * (plane._offset -
                        plane._normal.x * mPoint[i].x -
                        plane._normal.z * mPoint[i].z);
                }
            }
            else
            {
                float invNZ = 1f / plane._normal.z;
                for (i = 0; i < mQuantity; ++i)
                {
                    mPoint[i].x = intr[i].x;
                    mPoint[i].y = intr[i].y;
                    mPoint[i].z = invNZ * (plane._offset -
                        plane._normal.x * mPoint[i].x -
                        plane._normal.y * mPoint[i].y);
                }
            }

            return true;
        }//end func

        //tri 이 삼각형이 아닌 선분형태로 들어왔을 경우 처리해주지 못한다. 점도 처리 못할것으로 예상 
        public bool ContainsPoint(Triangle3 tri, Vector3 point)
        {
            Vector3 v0 = tri.V[1] - tri.V[0];
            Vector3 v1 = tri.V[2] - tri.V[1];
            Vector3 n = Vector3.Cross(v0, v1);

            //DebugWide.DrawLine(P0, P0 + n, Color.green);
            float F_Epsilon = 0.0000001f; //교점이 선분위에 있을때를 판별하기 위한 적당히 작은값 
                                          //교점이 선분위에 있으면 외적의 값이 0에 가까워 진다 (같은 방향의 벡터 외적 , sin0 = 0) 
                                          //외적값이 0에 가까운지로 교점이 선분위에 있는것을 검사한다 
                                          //교점이 선분위에 있으면 내적테스트와 상관없이 통과시킨다 (wTest.sqrMagnitude <= F_Epsilon)
            //F_Epsilon = float.Epsilon;
            //외적의 180도 기점으로 방향이 바뀌는 점을 이용 , 벡터v0이 나누는 공간에서 어느공간에 점이 있는지 테스트하게 된다  
            Vector3 wTest = Vector3.Cross(v0, (point - tri.V[0]));
            //DebugWide.DrawLine(P0, P0 + wTest, Color.yellow);
            //DebugWide.LogBlue("0 -> "+Vector3.Dot(wTest, n));
            //DebugWide.LogBlue("0 " + wTest.sqrMagnitude);
            if (Vector3.Dot(wTest, n) < 0.0f && wTest.sqrMagnitude > F_Epsilon)
            {
                return false;
            }

            wTest = Vector3.Cross(v1, (point - tri.V[1]));
            //DebugWide.DrawLine(P1, P1 + wTest, Color.white);
            //DebugWide.LogBlue("1 -> " + Vector3.Dot(wTest, n) + "    wT:" + wTest + "  " + wTest.sqrMagnitude);
            //DebugWide.LogBlue("1 "+ wTest.sqrMagnitude);
            if (Vector3.Dot(wTest, n) < 0.0f && wTest.sqrMagnitude > F_Epsilon)
            {
                return false;
            }

            Vector3 v2 = tri.V[0] - tri.V[2];
            wTest = Vector3.Cross(v2, (point - tri.V[2]));
            //DebugWide.DrawLine(P2, P2 + wTest, Color.cyan);
            //DebugWide.LogBlue("2 -> " + Vector3.Dot(wTest, n));
            //DebugWide.LogBlue("2 "+wTest.sqrMagnitude);
            if (Vector3.Dot(wTest, n) < 0.0f && wTest.sqrMagnitude > F_Epsilon)
            {
                return false;
            }

            //DebugWide.LogRed("333!! ");

            //DebugWide.AddDrawQ_Line(tri.V[1], tri.V[0], Color.red);
            //DebugWide.AddDrawQ_Line(tri.V[1], tri.V[2], Color.red);
            //DebugWide.AddDrawQ_Line(tri.V[0], tri.V[2], Color.red);

            //------ 포함 !! ------  
            mIntersectionType = eIntersectionType.POINT;
            mQuantity = 1;
            mPoint[0] = point;
            return true;
        }


    }//end IntrTri3_Tri3




}//end namespace

