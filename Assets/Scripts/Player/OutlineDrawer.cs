using UnityEngine;

public class OutlineDrawer : MonoBehaviour
{
    public Camera playerCamera;
    public float reachDistance = 5f;
    public LayerMask voxelLayer;

    private Vector3Int targetBlock;
    private bool hasTarget = false;
    private Material lineMaterial;

    void Start()
    {
        // Création d'un matériau spécial pour dessiner en blanc
        lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    void Update()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, reachDistance, voxelLayer))
        {
            Vector3 blockPos = hit.point - (hit.normal * 0.1f);
            targetBlock = new Vector3Int(
                Mathf.FloorToInt(blockPos.x),
                Mathf.FloorToInt(blockPos.y),
                Mathf.FloorToInt(blockPos.z)
            );

            hasTarget = true;
        }
        else
        {
            hasTarget = false;
        }
    }

    void OnRenderObject()
    {
        if (hasTarget && lineMaterial != null)
        {
            // Active le matériau pour dessiner en blanc
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(Color.white);

            Vector3 center = targetBlock + Vector3.one * 0.5f;
            Vector3 halfSize = Vector3.one * 0.5f;

            // 8 Sommets du cube
            Vector3 v0 = center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            Vector3 v1 = center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            Vector3 v2 = center + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            Vector3 v3 = center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            Vector3 v4 = center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            Vector3 v5 = center + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            Vector3 v6 = center + new Vector3(halfSize.x, halfSize.y, halfSize.z);
            Vector3 v7 = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            // Lignes des arêtes
            DrawLine(v0, v1);
            DrawLine(v1, v2);
            DrawLine(v2, v3);
            DrawLine(v3, v0);

            DrawLine(v4, v5);
            DrawLine(v5, v6);
            DrawLine(v6, v7);
            DrawLine(v7, v4);

            DrawLine(v0, v4);
            DrawLine(v1, v5);
            DrawLine(v2, v6);
            DrawLine(v3, v7);

            GL.End();
            GL.PopMatrix();
        }
    }

    void DrawLine(Vector3 start, Vector3 end)
    {
        GL.Vertex(start);
        GL.Vertex(end);
    }
}
