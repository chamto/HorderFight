using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor.Sprites;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace UnityEditor
{
	[CustomEditor(typeof(RuleExtraTile), true)]
	[CanEditMultipleObjects]
	internal class RuleExtraTileEditor : Editor
	{
		private const string s_MirrorX = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAG1JREFUOE+lj9ENwCAIRB2IFdyRfRiuDSaXAF4MrR9P5eRhHGb2Gxp2oaEjIovTXSrAnPNx6hlgyCZ7o6omOdYOldGIZhAziEmOTSfigLV0RYAB9y9f/7kO8L3WUaQyhCgz0dmCL9CwCw172HgBeyG6oloC8fAAAAAASUVORK5CYII=";
		private const string s_MirrorY = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAG9JREFUOE+djckNACEMAykoLdAjHbPyw1IOJ0L7mAejjFlm9hspyd77Kk+kBAjPOXcakJIh6QaKyOE0EB5dSPJAiUmOiL8PMVGxugsP/0OOib8vsY8yYwy6gRyC8CB5QIWgCMKBLgRSkikEUr5h6wOPWfMoCYILdgAAAABJRU5ErkJggg==";
		private const string s_Rotated = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC41ZYUyZQAAAHdJREFUOE+djssNwCAMQxmIFdgx+2S4Vj4YxWlQgcOT8nuG5u5C732Sd3lfLlmPMR4QhXgrTQaimUlA3EtD+CJlBuQ7aUAUMjEAv9gWCQNEPhHJUkYfZ1kEpcxDzioRzGIlr0Qwi0r+Q5rTgM+AAVcygHgt7+HtBZs/2QVWP8ahAAAAAElFTkSuQmCC";

		private static Texture2D[] s_AutoTransforms;
		public static Texture2D[] autoTransforms
		{
			get
			{
				if (s_AutoTransforms == null)
				{
					s_AutoTransforms = new Texture2D[3];
                    s_AutoTransforms[0] = RuleExtraTile.Base64ToTexture(s_Rotated);
                    s_AutoTransforms[1] = RuleExtraTile.Base64ToTexture(s_MirrorX);
                    s_AutoTransforms[2] = RuleExtraTile.Base64ToTexture(s_MirrorY);
				}
				return s_AutoTransforms;
			}
		}
		
		private ReorderableList m_ReorderableList;
        public RuleExtraTile tile { get { return (target as RuleExtraTile); } }
		private Rect m_ListRect;

		internal const float k_DefaultElementHeight = 48f;
		internal const float k_PaddingBetweenRules = 26f;
		internal const float k_SingleLineHeight = 16f;
		internal const float k_ObjectFieldLineHeight = 20f;
        internal const float k_LabelWidth = 80f;
			
		public void OnEnable()
		{
			if (tile.m_TilingRules == null)
                tile.m_TilingRules = new List<RuleExtraTile.TilingRule>();

            m_ReorderableList = new ReorderableList(tile.m_TilingRules, typeof(RuleExtraTile.TilingRule), true, true, true, true);
			m_ReorderableList.drawHeaderCallback = OnDrawHeader;
			m_ReorderableList.drawElementCallback = OnDrawElement;
			m_ReorderableList.elementHeightCallback = GetElementHeight;
			m_ReorderableList.onReorderCallback = ListUpdated;
			m_ReorderableList.onAddCallback = OnAddElement;


            //에디터상에서 타일맵 전체 갱신을 수동으로 하기 위하여 이 코드를 넣음
            //롤설정 파일을 선택할시 인스펙터창이 활성되면서 자동으로 아래 코드가 호출된다   
            if(null != tile._tilemap_copy)
            {
                //tile._tilemap_copy.ClearAllTiles();
                //DebugWide.LogBlue(tile._tilemap_copy.name + " : ClearAllTiles"); 
            }
            if (null != tile._tilemap_this)
            {
                tile._tilemap_this.RefreshAllTiles();
                //DebugWide.LogBlue(tile._tilemap_this.name + " : RefreshAllTiles"); 
            }
                

		}

		private void ListUpdated(ReorderableList list)
		{
			SaveTile();
		}

		private float GetElementHeight(int index)
		{
			if (tile.m_TilingRules != null && tile.m_TilingRules.Count > 0)
			{
				switch (tile.m_TilingRules[index].m_Output)
				{
                    case RuleExtraTile.TilingRule.OutputSprite.Random_Multi:
                        return k_DefaultElementHeight + k_SingleLineHeight * (tile.m_TilingRules[index].m_Sprites.Length + 5) + k_PaddingBetweenRules;
                    case RuleExtraTile.TilingRule.OutputSprite.Random:
						return k_DefaultElementHeight + k_SingleLineHeight*(tile.m_TilingRules[index].m_Sprites.Length + 4) + k_PaddingBetweenRules;
                    case RuleExtraTile.TilingRule.OutputSprite.Animation:
						return k_DefaultElementHeight + k_SingleLineHeight*(tile.m_TilingRules[index].m_Sprites.Length + 3) + k_PaddingBetweenRules;

                        //멀티모드일 경우 추가
                    case RuleExtraTile.TilingRule.OutputSprite.Multi:
                        return k_DefaultElementHeight + k_SingleLineHeight * (tile.m_TilingRules[index].m_Sprites.Length + 3) + k_PaddingBetweenRules;
                    case RuleExtraTile.TilingRule.OutputSprite.Single:
                        return k_DefaultElementHeight + k_SingleLineHeight * (tile.m_TilingRules[index].m_Sprites.Length + 3) + k_PaddingBetweenRules;
				}
			}
            return k_DefaultElementHeight + k_PaddingBetweenRules;
			//return k_DefaultElementHeight + k_PaddingBetweenRules + 80f; //chamto test
		}

		private void OnDrawElement(Rect rect, int index, bool isactive, bool isfocused)
		{
            RuleExtraTile.TilingRule rule = tile.m_TilingRules[index];

			float yPos = rect.yMin + 2f;
			float height = rect.height - k_PaddingBetweenRules;
			float matrixWidth = k_DefaultElementHeight;
			
			Rect inspectorRect = new Rect(rect.xMin, yPos, rect.width - matrixWidth * 2f - 20f, height);
			Rect matrixRect = new Rect(rect.xMax - matrixWidth * 2f - 10f, yPos, matrixWidth, k_DefaultElementHeight);
            Rect matrixRect_Length = new Rect(rect.xMax - matrixWidth * 2f - 10f, yPos + k_DefaultElementHeight + 5f, matrixWidth, k_DefaultElementHeight);
            Rect matrixRect_RuleID = new Rect(rect.xMax - matrixWidth * 2f + 40f, yPos + k_DefaultElementHeight + 5f , matrixWidth, k_DefaultElementHeight);
			Rect spriteRect = new Rect(rect.xMax - matrixWidth, yPos, matrixWidth, k_DefaultElementHeight);

			EditorGUI.BeginChangeCheck();
			RuleInspectorOnGUI(inspectorRect, rule);
			RuleMatrixOnGUI(tile, matrixRect, rule);
            RuleMatrixOnGUI_Direction(tile, inspectorRect , rule);

            //멀티모드에서만 보여지게 한다 - 보여지는 것과 상관없이 다른모드에도 영향을 준다
            //if (rule.m_Output == RuleTileExtra.TilingRule.OutputSprite.Multi)
            {
                RuleMatrixOnGUI_RuleLength(tile, matrixRect_Length, rule);
                RuleMatrixOnGUI_RuleID(tile, matrixRect_RuleID, rule);    
            }

			SpriteOnGUI(spriteRect, rule);
			if (EditorGUI.EndChangeCheck())
				SaveTile();
		}
		
		private void OnAddElement(ReorderableList list)
		{
            RuleExtraTile.TilingRule rule = new RuleExtraTile.TilingRule();
            rule.m_Output = RuleExtraTile.TilingRule.OutputSprite.Single;
			rule.m_Sprites[0] = tile.m_DefaultSprite;
			rule.m_GameObject = tile.m_DefaultGameObject;
            rule.m_ColliderType = tile.m_DefaultColliderType;
			tile.m_TilingRules.Add(rule);
		}

		private void SaveTile()
		{
			EditorUtility.SetDirty(target);
			SceneView.RepaintAll();
		}

		private void OnDrawHeader(Rect rect)
		{
			GUI.Label(rect, "Tiling Rules");
		}

		public override void OnInspectorGUI()
		{
			tile.m_DefaultSprite = EditorGUILayout.ObjectField("Default Sprite", tile.m_DefaultSprite, typeof(Sprite), false) as Sprite;
			tile.m_DefaultGameObject = EditorGUILayout.ObjectField("Default Game Object", tile.m_DefaultGameObject, typeof(GameObject), false) as GameObject;
            tile.m_DefaultColliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Default Collider", tile.m_DefaultColliderType);
            tile._tilemap_copy = EditorGUILayout.ObjectField("TileMap Copy", tile._tilemap_copy, typeof(Tilemap), true) as Tilemap;
            tile._class_id = (RuleExtraTile.eClassification)EditorGUILayout.EnumPopup("Classification ID", tile._class_id); //대분류 추가 


            RuleExtraTile.eClassification select = 0;
            RuleExtraTile.eClassification next = 0;
            int MAX_LENGTH = 10; //임의로 최대개수를 10개로 잡는다 
            for (int i = 0; i < MAX_LENGTH; i++)
            {
                next = tile.GetPermitRule(i);
                select |= (RuleExtraTile.eClassification)EditorGUILayout.EnumPopup("Permit Rules " + (i+1), next); //대분류 추가 
                if (RuleExtraTile.eClassification.None == next) break;
            }
            tile._permit_rules = select;


            var baseFields = typeof(RuleExtraTile).GetFields().Select(field => field.Name);
			var fields = target.GetType().GetFields().Select(field => field.Name).Where(field => !baseFields.Contains(field));
			foreach (var field in fields)
				EditorGUILayout.PropertyField(serializedObject.FindProperty(field), true);
			
			EditorGUILayout.Space();

			if (m_ReorderableList != null && tile.m_TilingRules != null)
				m_ReorderableList.DoLayoutList();
		}


        internal static void RuleMatrixOnGUI_Direction(RuleExtraTile tile, Rect rect , RuleExtraTile.TilingRule tilingRule)
        {
            Rect r = rect;
            r.x -= 20f;
            r.y += 40f;
            r.width = 20f;
            r.height = 20f;

            //s_Arrows[0] = Base64ToTexture(s_Arrow0); //leftUp -> 4 -> 0
            //s_Arrows[1] = Base64ToTexture(s_Arrow1); //up -> 3 -> 1
            //s_Arrows[2] = Base64ToTexture(s_Arrow2); //rightUp -> 2 -> 2
            //s_Arrows[3] = Base64ToTexture(s_Arrow3); //left -> 5 -> 3
            //s_Arrows[5] = Base64ToTexture(s_Arrow5); //right -> 1 -> 5
            //s_Arrows[6] = Base64ToTexture(s_Arrow6); //leftDown -> 6 -> 6
            //s_Arrows[7] = Base64ToTexture(s_Arrow7); //down -> 7 -> 7
            //s_Arrows[8] = Base64ToTexture(s_Arrow8); //rightDown -> 8 -> 8
            //s_Arrows[9] = Base64ToTexture(s_XIconString); // 0 -> 9
            int[] tr =  new int[]{9,5,2,1,0,3,6,7,8};

            //DebugWide.LogBlue(tilingRule._push_dir8 + "  " + tr.Length);

            if ((int)tilingRule._push_dir8 < 0) tilingRule._push_dir8 = UtilGS9.eDirection8.none;

            GUI.DrawTexture(r, RuleExtraTile.arrows[tr[(int)tilingRule._push_dir8]]);


            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
            {
                tilingRule._push_dir8 = (UtilGS9.eDirection8)((((int)tilingRule._push_dir8 + 1) % 9));
                GUI.changed = true;
                Event.current.Use();
            }
        
        }

        internal static void RuleMatrixOnGUI_RuleID(RuleExtraTile tile, Rect rect, RuleExtraTile.TilingRule tilingRule)
        {
            Handles.color = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.8f) : new Color(0f, 0f, 0f, 0.8f);
            int index = 0;
            float w = rect.width / 2.3f;
            float h = rect.height / 2.3f;

            Handles.color = Color.white;

            for (int y = 0; y <= 2; y++)
            {
                for (int x = 0; x <= 2; x++)
                {
                    Rect r = new Rect(rect.xMin + x * w, rect.yMin + y * h, w - 1, h - 1);
                    if (x != 1 || y != 1)
                    {
                        //8방향
                        EditorGUI.BeginChangeCheck();
                        string newID = EditorGUI.DelayedTextField(r, tilingRule.m_Neighbors_Specifier[index]);
                        if (EditorGUI.EndChangeCheck())
                        {
                            
                            tilingRule.m_Neighbors_Specifier[index] = newID.Substring(0, Mathf.Min(2,newID.Length)); //두글자로 제한한다 
                        }

                        index++;
                    }
                    else
                    {
                        //중앙 
                        GUI.Label(r, tilingRule.m_specifier);
                    }
                }
            }
        }

        internal static void RuleMatrixOnGUI_RuleLength(RuleExtraTile tile, Rect rect, RuleExtraTile.TilingRule tilingRule)
        {
            Handles.color = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.8f) : new Color(0f, 0f, 0f, 0.8f);
            int index = 0;
            float w = rect.width / 3f;
            float h = rect.height / 3f;

            for (int y = 0; y <= 3; y++)
            {
                float top = rect.yMin + y * h;
                Handles.DrawLine(new Vector3(rect.xMin, top), new Vector3(rect.xMax, top));
            }
            for (int x = 0; x <= 3; x++)
            {
                float left = rect.xMin + x * w;
                Handles.DrawLine(new Vector3(left, rect.yMin), new Vector3(left, rect.yMax));
            }
            Handles.color = Color.white;

            for (int y = 0; y <= 2; y++)
            {
                for (int x = 0; x <= 2; x++)
                {
                    Rect r = new Rect(rect.xMin + x * w, rect.yMin + y * h, w - 1, h - 1);
                    if (x != 1 || y != 1)
                    {
                        //8방향
                        EditorGUI.BeginChangeCheck();
                        int newLength = EditorGUI.DelayedIntField(r, tilingRule.m_Neighbors_Length[index]);
                        if (EditorGUI.EndChangeCheck())
                        {
                            tilingRule.m_Neighbors_Length[index] = newLength;
                        }

                        index++;
                    }
                    else
                    {
                        //중앙 
                    }
                }
            }
        }


        internal static void RuleMatrixOnGUI(RuleExtraTile tile, Rect rect, RuleExtraTile.TilingRule tilingRule)
		{
			Handles.color = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0.2f);
			int index = 0;
			float w = rect.width / 3f;
			float h = rect.height / 3f;

			for (int y = 0; y <= 3; y++)
			{
				float top = rect.yMin + y * h;
				Handles.DrawLine(new Vector3(rect.xMin, top), new Vector3(rect.xMax, top));
			}
			for (int x = 0; x <= 3; x++)
			{
				float left = rect.xMin + x * w;
				Handles.DrawLine(new Vector3(left, rect.yMin), new Vector3(left, rect.yMax));
			}
			Handles.color = Color.white;

			for (int y = 0; y <= 2; y++)
			{
				for (int x = 0; x <= 2; x++)
				{
					Rect r = new Rect(rect.xMin + x * w, rect.yMin + y * h, w - 1, h - 1);
					if (x != 1 || y != 1)
					{
						tile.RuleOnGUI(r, new Vector2Int(x, y), tilingRule.m_Neighbors[index]);
						if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
						{
                            int change = 1;
						    if (Event.current.button == 1)
								change = -1;

							var allConsts = tile.m_NeighborType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
							var neighbors = allConsts.Select(c => (int)c.GetValue(null)).ToList();
							neighbors.Sort();

							int oldIndex = neighbors.IndexOf(tilingRule.m_Neighbors[index]);
							int newIndex = (int)Mathf.Repeat(oldIndex + change, neighbors.Count);
							tilingRule.m_Neighbors[index] = neighbors[newIndex];
							GUI.changed = true;
							Event.current.Use();
						}

						index++;
					}
					else
					{
						switch (tilingRule.m_RuleTransform)
						{
                            case RuleExtraTile.TilingRule.Transform.Rotated:
								GUI.DrawTexture(r, autoTransforms[0]);
								break;
                            case RuleExtraTile.TilingRule.Transform.MirrorX:
								GUI.DrawTexture(r, autoTransforms[1]);
								break;
                            case RuleExtraTile.TilingRule.Transform.MirrorY:
								GUI.DrawTexture(r, autoTransforms[2]);
								break;
						}

						if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
						{
                            tilingRule.m_RuleTransform = (RuleExtraTile.TilingRule.Transform)(((int)tilingRule.m_RuleTransform + 1) % 4);
							GUI.changed = true;
							Event.current.Use();
						}
					}
				}
			}
		}

		private static void OnSelect(object userdata)
		{
			MenuItemData data = (MenuItemData) userdata;
			data.m_Rule.m_RuleTransform = data.m_NewValue;
		}

		private class MenuItemData
		{
            public RuleExtraTile.TilingRule m_Rule;
            public RuleExtraTile.TilingRule.Transform m_NewValue;

            public MenuItemData(RuleExtraTile.TilingRule mRule, RuleExtraTile.TilingRule.Transform mNewValue)
			{
				this.m_Rule = mRule;
				this.m_NewValue = mNewValue;
			}
		}

        internal static void SpriteOnGUI(Rect rect, RuleExtraTile.TilingRule tilingRule)
		{

            float HEIGHT = rect.height;

            //출력할 자리가 없음..
            //if (tilingRule.m_Output == RuleTileExtra.TilingRule.OutputSprite.Random_Multi)
            //{

            //    //(0 1) (2 3) (4 5)
            //    //(1 0) (3 2) (5 4)
            //    //(1 1) (5 5) (9 9) 

            //    int revSeq = 0;
            //    float MULTI_PADDING = 0f;
            //    for (int seq = 0; seq < tilingRule.m_Sprites.Length; seq++)
            //    {
            //        if (tilingRule.m_MultiLength * 2 == seq) break; //최대 3개까지만 출력한다

            //        //완전 뒤에서 부터 보기
            //        //revSeq = tilingRule.m_Sprites.Length - 1 - seq; 

            //        //역수를 만들어 뺀다
            //        int multiIndex_1 = (int)(seq % tilingRule.m_MultiLength); // 0 1 0 1 0 1 .... 
            //        multiIndex_1 = (tilingRule.m_MultiLength-1) - (multiIndex_1); //1 0 1 0 1 0 ....
            //        int multiIndex_2 = (int)(seq / tilingRule.m_MultiLength); // 0 0 1 1 2 2 .... 
            //        revSeq = tilingRule.m_MultiLength * multiIndex_2 + multiIndex_1;
            //        //DebugWide.LogBlue(multiIndex_1 + "  " + multiIndex_2 + "  " + revSeq + "  " + seq);

            //        tilingRule.m_Sprites[revSeq] = EditorGUI.ObjectField(new Rect(rect.xMax - rect.height, rect.yMin + MULTI_PADDING, HEIGHT, HEIGHT), tilingRule.m_Sprites[revSeq], typeof(Sprite), false) as Sprite;

            //        MULTI_PADDING += HEIGHT;
            //        //멀티타일 간의 간격을 띈다
            //        if (1 != tilingRule.m_MultiLength && tilingRule.m_MultiLength - 1 == seq % tilingRule.m_MultiLength) 
            //            MULTI_PADDING += 5;
                    
            //    }
            //}
            //else 
            {
                tilingRule.m_Sprites[0] = EditorGUI.ObjectField(new Rect(rect.xMax - rect.height, rect.yMin, HEIGHT, HEIGHT), tilingRule.m_Sprites[0], typeof(Sprite), false) as Sprite;
            }


		}

        internal static void RuleInspectorOnGUI(Rect rect, RuleExtraTile.TilingRule tilingRule )
		{
			float y = rect.yMin;
			EditorGUI.BeginChangeCheck();
            //GUI.Label(new Rect(rect.xMin - 19, rect.yMin + k_ObjectFieldLineHeight, k_LabelWidth, k_SingleLineHeight), index.ToString("00"));

            //ID입력 
            string newID = EditorGUI.DelayedTextField(new Rect(rect.xMin - 19, rect.yMin + k_ObjectFieldLineHeight, 20f, k_SingleLineHeight), tilingRule.m_specifier);
            tilingRule.m_specifier = newID.Substring(0,Mathf.Min(2, newID.Length)); //입력한 글자를 2글자로 제한한다 

            tilingRule._isTilemapCopy = EditorGUI.ToggleLeft(new Rect(rect.xMin, y, k_LabelWidth + 70f, k_SingleLineHeight), "Copy to another Tilemap", tilingRule._isTilemapCopy);
            y += k_SingleLineHeight;

            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Game Object");
            tilingRule.m_GameObject = (GameObject)EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), "", tilingRule.m_GameObject, typeof(GameObject), true);
            y += k_ObjectFieldLineHeight;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Rule");
            tilingRule.m_RuleTransform = (RuleExtraTile.TilingRule.Transform)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_RuleTransform);
			y += k_SingleLineHeight;
			GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Collider");
			tilingRule.m_ColliderType = (Tile.ColliderType)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_ColliderType);
			y += k_SingleLineHeight;
			GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Output");
            tilingRule.m_Output = (RuleExtraTile.TilingRule.OutputSprite)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Output);
			y += k_SingleLineHeight;


            switch(tilingRule.m_Output)
            {
                case RuleExtraTile.TilingRule.OutputSprite.Animation:
                    {
                        GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Speed");
                        tilingRule.m_AnimationSpeed = EditorGUI.FloatField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_AnimationSpeed);
                        y += k_SingleLineHeight;
                    }
                    break;
                case RuleExtraTile.TilingRule.OutputSprite.Random_Multi:
                case RuleExtraTile.TilingRule.OutputSprite.Random:
                    {
                        GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Noise");
                        tilingRule.m_PerlinScale = EditorGUI.Slider(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_PerlinScale, 0.001f, 0.999f);
                        y += k_SingleLineHeight;

                        GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Shuffle");
                        tilingRule.m_RandomTransform = (RuleExtraTile.TilingRule.Transform)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_RandomTransform);
                        y += k_SingleLineHeight;
                    }
                    break;
                
            }
            EditorGUI.EndChangeCheck();

            //멀티모드에서도 여러개의 타일을 넣을수 있게 변경한다 
            if (tilingRule.m_Output != RuleExtraTile.TilingRule.OutputSprite.Single)
			{
                if(tilingRule.m_Output == RuleExtraTile.TilingRule.OutputSprite.Multi ||
                   tilingRule.m_Output == RuleExtraTile.TilingRule.OutputSprite.Random_Multi )
                {
                    GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Multi Length");
                    //EditorGUI.BeginChangeCheck();
                    tilingRule.m_MultiLength = EditorGUI.DelayedIntField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_MultiLength);
                    tilingRule.m_MultiLength = Math.Max(tilingRule.m_MultiLength, 1); //길이가 0이 못되게 최대값을 1로 설정한다
                    //if (EditorGUI.EndChangeCheck())
                    {
                        //멀티길이에 따라 멀티복사 배열도 크기 조정한다 
                        Array.Resize(ref tilingRule._multi_copy, Math.Max(tilingRule.m_MultiLength, 1));    
                    }

                    y += k_SingleLineHeight;    
                }


				GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Size");
				EditorGUI.BeginChangeCheck();
                int newLength = Math.Max(tilingRule.m_Sprites.Length / tilingRule.m_MultiLength, 1); //길이가 0이 못되게 최대값을 1로 설정한다 
                newLength = EditorGUI.DelayedIntField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), newLength);
				if (EditorGUI.EndChangeCheck())
                {
                    Array.Resize(ref tilingRule.m_Sprites, Math.Max(newLength * tilingRule.m_MultiLength, 1));
                }
					
				y += k_SingleLineHeight;

                int seq = 0;
				for (int i = 0; i < tilingRule.m_Sprites.Length; i++)
				{
                    seq = i % tilingRule.m_MultiLength;

                    //멀티타일 길이가 1이면 일반타일이다 
                    if (1 == tilingRule.m_MultiLength)
                        seq = i;


                    //타일맵 복사 가능 옵션 
                    if(true == tilingRule._isTilemapCopy && i < tilingRule.m_MultiLength)
                    {
                        //DebugWide.LogBlue(tilingRule._multi_copy.Length  + "   i" + i + "  ml" + tilingRule.m_MultiLength);
                        if(i < tilingRule._multi_copy.Length)
                            tilingRule._multi_copy[i] = EditorGUI.Toggle(new Rect(rect.xMin + k_LabelWidth - 50f, y, 20, 20), tilingRule._multi_copy[i]);    
                    }

                    GUI.Label(new Rect(rect.xMin + k_LabelWidth - 25, y, 20, 20), seq.ToString(" 0"));
					tilingRule.m_Sprites[i] = EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Sprites[i], typeof(Sprite), false) as Sprite;
					y += k_SingleLineHeight;

                    //멀티타일 간의 간격을 띈다
                    if (1 != tilingRule.m_MultiLength && tilingRule.m_MultiLength-1 == seq) y += 5;
				}
			}
		}

		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			if (tile.m_DefaultSprite != null)
			{
				Type t = GetType("UnityEditor.SpriteUtility");
				if (t != null)
				{
					MethodInfo method = t.GetMethod("RenderStaticPreview", new Type[] {typeof (Sprite), typeof (Color), typeof (int), typeof (int)});
					if (method != null)
					{
						object ret = method.Invoke("RenderStaticPreview", new object[] {tile.m_DefaultSprite, Color.white, width, height});
						if (ret is Texture2D)
							return ret as Texture2D;
					}
				}
			}
			return base.RenderStaticPreview(assetPath, subAssets, width, height);
		}

		private static Type GetType(string TypeName)
		{
			var type = Type.GetType(TypeName);
			if (type != null)
				return type;

			if (TypeName.Contains("."))
			{
				var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));
				var assembly = Assembly.Load(assemblyName);
				if (assembly == null)
					return null;
				type = assembly.GetType(TypeName);
				if (type != null)
					return type;
			}

			var currentAssembly = Assembly.GetExecutingAssembly();
			var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
			foreach (var assemblyName in referencedAssemblies)
			{
				var assembly = Assembly.Load(assemblyName);
				if (assembly != null)
				{
					type = assembly.GetType(TypeName);
					if (type != null)
						return type;
				}
			}
			return null;
		}
		
		[Serializable]
		class RuleExtraTileWrapper
		{
			[SerializeField]
            public List<RuleExtraTile.TilingRule> rules = new List<RuleExtraTile.TilingRule>();
		}
		
        [MenuItem("CONTEXT/RuleExtraTile/Copy All Rules")]
		private static void CopyAllRules(MenuCommand item)
		{
            RuleExtraTile tile = item.context as RuleExtraTile;
			if (tile == null)
				return;
			
			RuleExtraTileWrapper rulesWrapper = new RuleExtraTileWrapper();
			rulesWrapper.rules = tile.m_TilingRules;
			var rulesJson = EditorJsonUtility.ToJson(rulesWrapper);
			EditorGUIUtility.systemCopyBuffer = rulesJson;
		}
		
        [MenuItem("CONTEXT/RuleExtraTile/Paste Rules")]
		private static void PasteRules(MenuCommand item)
		{
            RuleExtraTile tile = item.context as RuleExtraTile;
			if (tile == null)
				return;

			try
			{
				RuleExtraTileWrapper rulesWrapper = new RuleExtraTileWrapper();
				EditorJsonUtility.FromJsonOverwrite(EditorGUIUtility.systemCopyBuffer, rulesWrapper);
				tile.m_TilingRules.AddRange(rulesWrapper.rules);
			}
			catch (Exception)
			{
				Debug.LogError("Unable to paste rules from system copy buffer");
			}
		}
	}
}
