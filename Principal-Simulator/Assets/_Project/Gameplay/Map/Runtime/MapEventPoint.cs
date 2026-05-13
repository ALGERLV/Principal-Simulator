using TBS.Map.Data;
using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Map.Runtime
{
    public class MapEventPoint : MonoBehaviour
    {
        public MapHexCoord Coord { get; private set; }
        public MapEventPointType PointType { get; private set; }
        public string PointName { get; private set; }
        public int ScoreValue { get; private set; }

        public void Initialize(MapEventPointData data, Vector3 worldPos)
        {
            Coord = new MapHexCoord(data.Q, data.R);
            PointType = data.PointType;
            PointName = data.PointName;
            ScoreValue = data.ScoreValue;

            transform.position = worldPos;
            gameObject.name = $"EventPoint_{PointName}";

            BuildVisuals();
        }

        void BuildVisuals()
        {
            Color baseColor;
            Color borderColor;
            float scale = 0.6f;

            switch (PointType)
            {
                case MapEventPointType.KMTReinforcement:
                    baseColor = new Color(0.1f, 0.2f, 0.7f);
                    borderColor = new Color(0.3f, 0.5f, 1f);
                    break;
                case MapEventPointType.JapanReinforcement:
                    baseColor = new Color(0.7f, 0.1f, 0.1f);
                    borderColor = new Color(1f, 0.3f, 0.3f);
                    break;
                default:
                    baseColor = new Color(0.8f, 0.7f, 0.1f);
                    borderColor = new Color(1f, 0.9f, 0.3f);
                    scale = 0.5f + ScoreValue * 0.005f;
                    break;
            }

            // 底座 (类似 UnitToken 的 TokenBase)
            var baseCyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseCyl.name = "TokenBase";
            baseCyl.transform.SetParent(transform);
            baseCyl.transform.localPosition = new Vector3(0, 0.15f, 0);
            baseCyl.transform.localScale = new Vector3(scale, 0.04f, scale);
            Object.DestroyImmediate(baseCyl.GetComponent<CapsuleCollider>());
            baseCyl.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = baseColor };

            // 边框环 (类似 UnitToken 的 TokenBorder)
            var border = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            border.name = "TokenBorder";
            border.transform.SetParent(transform);
            border.transform.localPosition = new Vector3(0, 0.17f, 0);
            border.transform.localScale = new Vector3(scale + 0.05f, 0.02f, scale + 0.05f);
            Object.DestroyImmediate(border.GetComponent<CapsuleCollider>());
            border.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = borderColor };

            // 符号 (类似 UnitToken 的 TokenSymbol_BG)
            var symbol = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            symbol.name = "TokenSymbol";
            symbol.transform.SetParent(transform);
            symbol.transform.localPosition = new Vector3(0, 0.22f, 0);
            symbol.transform.localScale = new Vector3(0.2f, 0.05f, 0.2f);
            Object.DestroyImmediate(symbol.GetComponent<SphereCollider>());
            symbol.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = borderColor };
        }
    }
}
